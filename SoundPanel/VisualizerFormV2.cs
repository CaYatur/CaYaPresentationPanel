using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;

public enum VisualizerStyle
{
    ClassicBars = 0,
    CircularSpectrum = 1,
    WaveformRipple = 2,
    ParticleField = 3,
    Matrix3D = 4,
    FireWave = 5,
    NeonTunnel = 6,
    LightningStorm = 7,
    QuantumField = 8,
    HologramCube = 9
}

public sealed class VisualizerFormV2 : Form
{
    private const int FftExponent = 12;                    // 4096 - daha detaylı analiz
    private const int FftSize = 1 << FftExponent;
    private const int RingBufferSize = FftSize * 12;
    private const int MaxBars = 256;                       // 384'ten -> 256 (performance)
    private const float MinVisibleFrequency = 20f;
    private const float MaxVisibleFrequency = 20000f;
    private const float MinDb = -92f;
    private const float MaxDb = -18f;

    private readonly object _audioLock = new();

    private readonly float[] _ringBuffer = new float[RingBufferSize];
    private readonly float[] _window = new float[FftSize];
    private readonly float[] _timeDomain = new float[FftSize];
    private readonly Complex[] _fftBuffer = new Complex[FftSize];

    private readonly float[] _targetBars = new float[MaxBars];
    private readonly float[] _displayBars = new float[MaxBars];
    private readonly float[] _peakBars = new float[MaxBars];
    private readonly float[] _spectrumSmooth = new float[MaxBars];
    private readonly int[] _bandStartBins = new int[MaxBars];
    private readonly int[] _bandEndBins = new int[MaxBars];
    private readonly Color[] _barColors = new Color[MaxBars];

    private readonly System.Windows.Forms.Timer _renderTimer;
    
    // FFT Async computation (no thread - safer than background thread)
    private Task? _fftComputeTask;
    private volatile bool _fftComputePending = false;
    
    // Particle ve effect sistemleri
    private readonly List<Particle> _particles = new();
    private readonly List<Lightning> _lightnings = new();
    private readonly Random _random = new();

    private WasapiCapture? _capture;
    private MMDevice? _device;
    private MMDeviceEnumerator? _deviceEnumerator;  // CRITICAL: Keep as class field to prevent GC (NAudio Issue #849)

    private int _ringWriteIndex;
    private int _ringCount;
    private int _sampleRate = 48000;
    private int _barCount = 256;
    private float _backgroundPhase;
    private volatile bool _stopping;
    private volatile bool _deviceChanged;
    
    // Audio health monitoring ve recovery system
    private volatile bool _audioHealthy = false;
    private volatile int _audioFailureCount = 0;
    private DateTime _lastSuccessfulAudioUpdate = DateTime.Now;
    private System.Windows.Forms.Timer? _healthCheckTimer;
    private const int HealthCheckInterval = 2000; // 2 seconds
    private const int AudioTimeoutMs = 3000; // 3 seconds without update = problem
    
    private VisualizerStyle _currentStyle = VisualizerStyle.ClassicBars;
    private float _rotationAngle = 0f;
    private float _waveOffset = 0f;
    private float _tunnelDepth = 0f;
    private float _hologramRotation = 0f;
    
    // Spektrum smoothing parametreleri
    private const float SpectrumSmoothingFactor = 0.6f;
    private float _averageSpectrum = 0f;
    
    // Fullscreen state
    private bool _isFullscreen = false;
    private FormBorderStyle _oldBorderStyle;
    private Rectangle _oldBounds;
    private FormWindowState _oldWindowState;
    private AudioCaptureMode _audioMode = AudioCaptureMode.Speaker;
    private int _captureTransitionInProgress;

    private enum AudioCaptureMode
    {
        Speaker = 0,
        Microphone = 1
    }

    public VisualizerFormV2()
    {
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1280, 720);
        MinimumSize = new Size(960, 540);
        BackColor = Color.Black;
        DoubleBuffered = true;
        KeyPreview = true;

        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        BuildWindow();
        BuildPalette();
        RecalculateBarLayout();

        _renderTimer = new System.Windows.Forms.Timer
        {
            Interval = 16  // 60 FPS - daha optimize
        };
        _renderTimer.Tick += RenderTimer_Tick;

        // Health check timer - monitör audio sistem durumu
        _healthCheckTimer = new System.Windows.Forms.Timer
        {
            Interval = HealthCheckInterval
        };
        _healthCheckTimer.Tick += HealthCheck_Tick;

        Shown += (_, _) => 
        {
            RestartCaptureInternal(showErrorDialogs: true);
            _healthCheckTimer?.Start();
        };
        Resize += (_, _) => RecalculateBarLayout();

        UpdateWindowTitle();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (HandleShortcutKey(keyData & Keys.KeyCode))
            return true;

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool HandleShortcutKey(Keys keyCode)
    {
        if (keyCode == Keys.Tab)
        {
            _currentStyle = (VisualizerStyle)(((int)_currentStyle + 1) % 10);
            UpdateWindowTitle();
            return true;
        }

        if (keyCode == Keys.Space)
        {
            ToggleAudioMode();
            return true;
        }

        if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
        {
            int styleIndex = keyCode == Keys.D0 ? 9 : (keyCode - Keys.D1);
            if (styleIndex >= 0 && styleIndex < 10)
            {
                _currentStyle = (VisualizerStyle)styleIndex;
                UpdateWindowTitle();
            }

            return true;
        }

        if (keyCode == Keys.F11)
        {
            ToggleFullscreen();
            return true;
        }

        return false;
    }

    private bool IsCaptureTransitionInProgress =>
        Interlocked.CompareExchange(ref _captureTransitionInProgress, 0, 0) != 0;

    private bool TryBeginCaptureTransition() =>
        Interlocked.CompareExchange(ref _captureTransitionInProgress, 1, 0) == 0;

    private void EndCaptureTransition() =>
        Interlocked.Exchange(ref _captureTransitionInProgress, 0);

    private void RestartCaptureInternal(bool showErrorDialogs)
    {
        if (!TryBeginCaptureTransition())
            return;

        try
        {
            StopCapture();
            Thread.Sleep(150);
            StartCapture(showErrorDialogs);
        }
        catch (Exception ex)
        {
            if (showErrorDialogs && !IsDisposed && IsHandleCreated)
            {
                try
                {
                    MessageBox.Show(
                        this,
                        $"Ses yeniden başlatılamadı: {ex.Message}",
                        "Ses Görselleştirici",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                catch { }
            }
        }
        finally
        {
            EndCaptureTransition();
            UpdateWindowTitle();
        }
    }

    private void ToggleAudioMode()
    {
        if (!TryBeginCaptureTransition())
            return;

        AudioCaptureMode previousMode = _audioMode;
        AudioCaptureMode nextMode = previousMode == AudioCaptureMode.Speaker
            ? AudioCaptureMode.Microphone
            : AudioCaptureMode.Speaker;

        try
        {
            _audioMode = nextMode;
            StopCapture();
            Thread.Sleep(150);

            if (StartCapture(showErrorDialogs: false))
                return;

            _audioMode = previousMode;
            StopCapture();
            Thread.Sleep(150);

            bool restored = StartCapture(showErrorDialogs: false);

            if (!IsDisposed && IsHandleCreated)
            {
                string message = restored
                    ? $"{GetAudioModeName(nextMode)} moduna geçilemedi. {GetAudioModeName(previousMode)} modu geri yüklendi."
                    : $"{GetAudioModeName(nextMode)} moduna geçilemedi. Ses yakalama şu anda kullanılamıyor.";

                try
                {
                    MessageBox.Show(
                        this,
                        message,
                        "Ses Görselleştirici",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                catch { }
            }
        }
        finally
        {
            EndCaptureTransition();
            UpdateWindowTitle();
        }
    }

    private string GetAudioModeName(AudioCaptureMode mode) => mode switch
    {
        AudioCaptureMode.Speaker => "Hoparlör",
        AudioCaptureMode.Microphone => "Mikrofon",
        _ => "Bilinmeyen"
    };

    private DataFlow GetCurrentDataFlow() =>
        _audioMode == AudioCaptureMode.Microphone ? DataFlow.Capture : DataFlow.Render;

    private WasapiCapture CreateCaptureForCurrentMode(MMDevice device) =>
        _audioMode == AudioCaptureMode.Microphone
            ? new WasapiCapture(device)
            : new WasapiLoopbackCapture(device);

    private void UpdateWindowTitle()
    {
        Text = $"Ses Görselleştirici - {GetStyleName(_currentStyle)} | Kaynak: {GetAudioModeName(_audioMode)} | Tab: Stil değiştir | Boşluk: Mikrofon/Hoparlör geçiş";
    }

    private string GetStyleName(VisualizerStyle style) => style switch
    {
        VisualizerStyle.ClassicBars => "Classic Bars",
        VisualizerStyle.CircularSpectrum => "Circular Spectrum",
        VisualizerStyle.WaveformRipple => "Waveform Ripple",
        VisualizerStyle.ParticleField => "Particle Field",
        VisualizerStyle.Matrix3D => "Matrix 3D",
        VisualizerStyle.FireWave => "Fire Wave",
        VisualizerStyle.NeonTunnel => "Neon Tunnel",
        VisualizerStyle.LightningStorm => "Lightning Storm",
        VisualizerStyle.QuantumField => "Quantum Field",
        VisualizerStyle.HologramCube => "Hologram Cube",
        _ => "Unknown"
    };

    private void ToggleFullscreen()
    {
        if (!_isFullscreen)
        {
            _isFullscreen = true;
            _oldBorderStyle = FormBorderStyle;
            _oldBounds = Bounds;
            _oldWindowState = WindowState;
            
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            var screen = Screen.FromControl(this);
            Bounds = screen.Bounds;
            TopMost = true;
        }
        else
        {
            _isFullscreen = false;
            TopMost = false;
            FormBorderStyle = _oldBorderStyle;
            WindowState = _oldWindowState;
            Bounds = _oldBounds;
        }
    }

    /// <summary>
    /// Call this method when songs change or audio device changes to safely restart audio capture.
    /// This prevents ExecutionEngineException during device transitions.
    /// Must be called from UI thread.
    /// </summary>
    public void RestartAudioCapture()
    {
        RestartCaptureInternal(showErrorDialogs: true);
    }

    private bool StartCapture(bool showErrorDialogs = true)
    {
        if (_capture != null)
        {
            // If capture already exists, stop it cleanly first
            StopCapture();
            Thread.Sleep(100); // Wait for complete cleanup
        }

        try
        {
            _stopping = false;
            _deviceChanged = false;  // Clear flag
            _audioHealthy = false;
            _audioFailureCount = 0;
            _lastSuccessfulAudioUpdate = DateTime.Now; // Initialize
            
            ResetState();

            // CRITICAL FIX (NAudio Issue #849): Keep enumerator as class field to prevent garbage collection
            // Do NOT create local MMDeviceEnumerator that gets disposed
            try
            {
                _deviceEnumerator ??= new MMDeviceEnumerator();
            }
            catch (COMException comEx)
            {
                throw new InvalidOperationException("Failed to initialize audio device enumerator: " + comEx.Message, comEx);
            }

            // Get default device with retry (Windows audio service might be initializing)
            _device = null;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    _device = _deviceEnumerator.GetDefaultAudioEndpoint(GetCurrentDataFlow(), Role.Multimedia);
                    if (_device != null)
                        break;
                }
                catch (COMException comEx) when (attempt < 2)
                {
                    Thread.Sleep(500); // Wait before retry - Windows audio service might be updating
                }
            }

            if (_device == null)
                throw new InvalidOperationException("No audio device found after 3 attempts");

            _capture = CreateCaptureForCurrentMode(_device);
            
            // Access WaveFormat with COMException protection (device might be removed)
            try
            {
                _sampleRate = _capture.WaveFormat.SampleRate;
                if (_sampleRate <= 0)
                    throw new InvalidOperationException("Invalid sample rate");
            }
            catch (COMException)
            {
                throw new InvalidOperationException("Failed to access audio format properties - device removed or audio service error");
            }

            RecalculateBarLayout();

            _capture.DataAvailable += Capture_DataAvailable;
            _capture.RecordingStopped += Capture_RecordingStopped;
            _capture.StartRecording();

            _renderTimer.Start();
            _audioHealthy = true;
            return true;
        }
        catch (COMException comEx)
        {
            // Windows audio COM error - often device removed or audio service issue
            _audioHealthy = false;
            try
            {
                StopCapture();
            }
            catch { }

            if (showErrorDialogs)
            {
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                        MessageBox.Show(
                            this,
                            $"Audio device error (COM): {comEx.Message}\r\nVisualizer will work without audio.",
                            "Sound Visualizer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                }
                catch { }
            }

            return false;
        }
        catch (ExecutionEngineException eex)
        {
            // CLR crash from NAudio - catastrophic failure (NAudio Issue #849)
            _audioHealthy = false;
            try
            {
                StopCapture();
            }
            catch { }

            if (showErrorDialogs)
            {
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                        MessageBox.Show(
                            this,
                            "Critical audio error (CLR crash). Visualizer will work without audio.",
                            "Sound Visualizer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                }
                catch { }
            }

            return false;
        }
        catch (Exception ex)
        {
            _audioHealthy = false;
            try
            {
                StopCapture();
            }
            catch { }

            if (showErrorDialogs)
            {
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                        MessageBox.Show(
                            this,
                            "Could not start audio capture.\r\n\r\n" + ex.Message,
                            "Sound Visualizer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                }
                catch { }
            }

            return false;
        }
    }

    private void StopCapture()
    {
        try { _renderTimer.Stop(); } catch { }
        
        // Signal all threads to stop IMMEDIATELY
        _stopping = true;
        _deviceChanged = true;
        
        // Wait a bit to ensure no callbacks are pending
        Thread.Sleep(200);
        
        try
        {
            if (_capture != null)
            {
                // Remove event handlers first (this is critical)
                try
                {
                    _capture.DataAvailable -= Capture_DataAvailable;
                }
                catch { }
                
                try
                {
                    _capture.RecordingStopped -= Capture_RecordingStopped;
                }
                catch { }

                // Stop recording with multiple attempts
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        _capture.StopRecording();
                        break;
                    }
                    catch (COMException)
                    {
                        // Device may be removed - try again
                        Thread.Sleep(50);
                    }
                    catch
                    {
                        Thread.Sleep(50);
                    }
                }

                // Final wait before disposal
                Thread.Sleep(150);
                
                try
                {
                    _capture.Dispose();
                }
                catch { }
                finally
                {
                    _capture = null;
                }
            }
        }
        catch { }

        // Release device
        try
        {
            _device?.Dispose();
            _device = null;
        }
        catch (COMException)
        {
            // Device already removed by Windows - ignore
            _device = null;
        }
        catch { }
        
        // Final cleanup
        try
        {
            lock (_audioLock)
            {
                _ringCount = 0;
                _ringWriteIndex = 0;
            }
        }
        catch { }
    }

    private void Capture_RecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (_stopping || IsDisposed)
            return;

        if (e.Exception == null)
            return;

        if (!IsHandleCreated)
            return;

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(
                        this,
                        "Audio capture stopped.\r\n\r\n" + e.Exception.Message,
                        "Sound Visualizer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }));
        }
        catch { }
    }

    private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        // Triple-check to prevent any access during device change
        if (_capture == null || _stopping || _deviceChanged)
            return;

        try
        {
            // Safety: re-check capture state
            if (_capture == null)
                return;
            
            WaveFormat? format = null;
            try
            {
                // Aggressive COMException handling for WaveFormat access (Issue #849)
                format = _capture.WaveFormat;
            }
            catch (COMException)
            {
                // Device removed or audio service crashed
                _deviceChanged = true;
                return;
            }
            catch (ExecutionEngineException)
            {
                // NAudio crashed - device state invalid
                _deviceChanged = true;
                _audioHealthy = false;
                return;
            }
            
            if (format == null)
                return;

            int bytesPerSample = format.BitsPerSample / 8;
            int channels = format.Channels;

            if (bytesPerSample <= 0 || channels <= 0 || format.BlockAlign <= 0)
                return;

            int frameCount = e.BytesRecorded / format.BlockAlign;
            if (frameCount <= 0)
                return;

            // Use timeout on lock to prevent deadlock
            if (!Monitor.TryEnter(_audioLock, 10))
                return;

            try
            {
                // Re-check flags after lock acquired
                if (_stopping || _deviceChanged || _capture == null)
                    return;

                for (int frame = 0; frame < frameCount; frame++)
                {
                    int frameOffset = frame * format.BlockAlign;
                    float mixed = 0f;

                    for (int ch = 0; ch < channels; ch++)
                    {
                        int sampleOffset = frameOffset + (ch * bytesPerSample);
                        if (sampleOffset + bytesPerSample <= e.BytesRecorded)
                        {
                            mixed += ReadSample(e.Buffer, sampleOffset, format, bytesPerSample);
                        }
                    }

                    mixed /= channels;

                    _ringBuffer[_ringWriteIndex] = mixed;
                    _ringWriteIndex = (_ringWriteIndex + 1) % _ringBuffer.Length;

                    if (_ringCount < _ringBuffer.Length)
                        _ringCount++;
                }

                // Mark successful audio update for health monitoring
                _lastSuccessfulAudioUpdate = DateTime.Now;
            }
            finally
            {
                Monitor.Exit(_audioLock);
            }
        }
        catch (ExecutionEngineException)
        {
            // NAudio crash during audio callback - mark device as changed for recovery
            _deviceChanged = true;
            _audioHealthy = false;
        }
        catch (COMException)
        {
            // Windows audio error
            _deviceChanged = true;
            _audioHealthy = false;
        }
        catch
        {
            // Silently handle all other audio buffer errors
        }
    }

    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            // Safety check
            if (_stopping || IsDisposed)
                return;

            // Start async FFT computation if not already running
            if (!_fftComputePending)
            {
                _fftComputePending = true;
                _fftComputeTask = Task.Run(() =>
                {
                    try
                    {
                        UpdateSpectrum();
                    }
                    catch
                    {
                        // Silently ignore FFT errors
                    }
                    finally
                    {
                        _fftComputePending = false;
                    }
                });
            }
            
            // Render on UI thread
            UpdateVisualSmoothing();
            UpdateEffects();
            
            try
            {
                Invalidate();
            }
            catch
            {
                // Ignore invalidate errors during shutdown
            }
        }
        catch
        {
            // Silently ignore all render timer errors
        }
    }

    private void HealthCheck_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (IsDisposed)
                return;

            if (IsCaptureTransitionInProgress)
                return;

            // Check if audio is responding
            TimeSpan timeSinceLastUpdate = DateTime.Now - _lastSuccessfulAudioUpdate;

            if (timeSinceLastUpdate.TotalMilliseconds > AudioTimeoutMs)
            {
                // Audio hasn't updated in too long - it's probably crashed
                _audioFailureCount++;

                if (_audioFailureCount >= 2) // 2 consecutive failures
                {
                    _audioHealthy = false;

                    try
                    {
                        // Try to recover - restart audio capture
                        if (_capture != null)
                        {
                            RestartCaptureInternal(showErrorDialogs: false);
                            _audioFailureCount = 0;
                        }
                    }
                    catch (COMException comEx)
                    {
                        // Windows audio COM error - may recover on next attempt
                        _audioHealthy = false;
                    }
                    catch
                    {
                        // Audio recovery failed - continue with visualization but no sound
                        _audioHealthy = false;
                    }
                }
            }
            else
            {
                // Audio is responding - reset failure count
                _audioFailureCount = 0;
                _audioHealthy = true;
            }
        }
        catch
        {
            // Ignore health check errors
        }
    }

    private void UpdateSpectrum()
    {
        if (_stopping || _deviceChanged)
            return;

        bool enoughData;

        // Use timeout to prevent deadlock when device changes
        if (!Monitor.TryEnter(_audioLock, 20))
            return;

        try
        {
            enoughData = _ringCount >= FftSize;

            if (enoughData)
            {
                int start = (_ringWriteIndex - FftSize + _ringBuffer.Length) % _ringBuffer.Length;

                for (int i = 0; i < FftSize; i++)
                {
                    _timeDomain[i] = _ringBuffer[(start + i) % _ringBuffer.Length];
                }
            }
        }
        finally
        {
            Monitor.Exit(_audioLock);
        }

        if (!enoughData)
        {
            for (int i = 0; i < _barCount; i++)
                _targetBars[i] *= 0.92f;

            return;
        }

        try
        {
            // Hann penceresi uygula
            for (int i = 0; i < FftSize; i++)
            {
                _fftBuffer[i].X = _timeDomain[i] * _window[i];
                _fftBuffer[i].Y = 0f;
            }

            FastFourierTransform.FFT(true, FftExponent, _fftBuffer);

            for (int band = 0; band < _barCount; band++)
            {
                int startBin = _bandStartBins[band];
                int endBin = _bandEndBins[band];

                float sumPower = 0f;
                int count = 0;

                for (int bin = startBin; bin <= endBin; bin++)
                {
                    float re = _fftBuffer[bin].X;
                    float im = _fftBuffer[bin].Y;
                    float power = (re * re) + (im * im);

                    sumPower += power;
                    count++;
                }

                if (count <= 0)
                {
                    _targetBars[band] = 0f;
                    continue;
                }

                float rms = MathF.Sqrt(sumPower / count);
                float db = 20f * MathF.Log10(rms + 1e-12f);

                float normalized = (db - MinDb) / (MaxDb - MinDb);
                normalized = Math.Clamp(normalized, 0f, 1f);
                normalized = MathF.Pow(normalized, 1.05f);

                _targetBars[band] = normalized;
            }

            // 3-band smoothing
            for (int i = 1; i < _barCount - 1; i++)
            {
                _targetBars[i] =
                    (_targetBars[i - 1] * 0.2f) +
                    (_targetBars[i] * 0.6f) +
                    (_targetBars[i + 1] * 0.2f);
            }
        }
        catch
        {
            // Silently handle FFT errors
        }
    }

    private void UpdateVisualSmoothing()
    {
        _backgroundPhase += 0.012f;

        for (int i = 0; i < _barCount; i++)
        {
            float target = _targetBars[i];

            if (target > _displayBars[i])
                _displayBars[i] += (target - _displayBars[i]) * 0.75f;
            else
                _displayBars[i] += (target - _displayBars[i]) * 0.15f;

            if (_displayBars[i] > _peakBars[i])
                _peakBars[i] = _displayBars[i];
            else
                _peakBars[i] = Math.Max(_displayBars[i], _peakBars[i] - 0.018f);
                
            // Spektrum smoothing - daha detaylı analiz
            _spectrumSmooth[i] += (_displayBars[i] - _spectrumSmooth[i]) * SpectrumSmoothingFactor;
        }
        
        // Ortalama spektrum hesapla - effect'ler için
        _averageSpectrum = 0f;
        int count = Math.Min(_barCount, 64);
        for (int i = 0; i < count; i++)
            _averageSpectrum += _spectrumSmooth[i];
        _averageSpectrum /= count;
    }

    private void UpdateEffects()
    {
        _rotationAngle += 1f;
        _waveOffset += 0.15f;
        _tunnelDepth += 0.08f;
        _hologramRotation += 2.5f;

        // Parçacıkları güncelle
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            _particles[i].Update();
            if (!_particles[i].IsAlive)
                _particles.RemoveAt(i);
        }

        // Şimşekleri güncelle
        for (int i = _lightnings.Count - 1; i >= 0; i--)
        {
            _lightnings[i].Update();
            if (!_lightnings[i].IsAlive)
                _lightnings.RemoveAt(i);
        }

        // ParticleField için yeni parçacıklar
        if (_currentStyle == VisualizerStyle.ParticleField && _averageSpectrum > 0.1f && _particles.Count < 180)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_random.NextDouble() < _averageSpectrum * 0.8f)
                {
                    var p = new Particle
                    {
                        X = _random.Next(ClientSize.Width),
                        Y = ClientSize.Height,
                        VX = (_random.NextSingle() - 0.5f) * 12f,
                        VY = -_random.NextSingle() * 12f,
                        Life = 1f,
                        MaxLife = 1f,
                        Size = 2f + _averageSpectrum * 6f,
                        Color = _barColors[_random.Next(_barCount)]
                    };
                    _particles.Add(p);
                }
            }
        }

        // LightningStorm için şimşekler
        if (_currentStyle == VisualizerStyle.LightningStorm && _averageSpectrum > 0.4f)
        {
            if (_random.NextDouble() < 0.12f)
            {
                GenerateLightning();
            }
        }
    }

    private void GenerateLightning()
    {
        var lightning = new Lightning
        {
            Life = 0.8f,
            Color = Color.FromArgb(200, 150 + _random.Next(105), 100 + _random.Next(155), 255),
            Thickness = 1.5f + _averageSpectrum * 3f
        };

        float startX = _random.Next(ClientSize.Width);
        float startY = 0;
        float endX = _random.Next(ClientSize.Width);
        float endY = ClientSize.Height;

        lightning.Points.Add(new PointF(startX, startY));

        for (int i = 1; i < 12; i++)
        {
            float progress = (float)i / 12f;
            float x = startX + (endX - startX) * progress + (_random.NextSingle() - 0.5f) * 80f;
            float y = startY + (endY - startY) * progress;
            lightning.Points.Add(new PointF(x, y));
        }

        lightning.Points.Add(new PointF(endX, endY));
        _lightnings.Add(lightning);
    }

    private void RecalculateBarLayout()
    {
        _barCount = Math.Clamp(ClientSize.Width / 4, 140, MaxBars);

        float maxFreq = Math.Min(_sampleRate / 2f, MaxVisibleFrequency);
        double ratio = maxFreq / MinVisibleFrequency;

        for (int i = 0; i < _barCount; i++)
        {
            double startFreq = MinVisibleFrequency * Math.Pow(ratio, i / (double)_barCount);
            double endFreq = MinVisibleFrequency * Math.Pow(ratio, (i + 1d) / _barCount);

            int startBin = Math.Max(1, (int)Math.Floor(startFreq * FftSize / _sampleRate));
            int endBin = Math.Max(startBin, (int)Math.Ceiling(endFreq * FftSize / _sampleRate));

            startBin = Math.Min(startBin, (FftSize / 2) - 1);
            endBin = Math.Min(endBin, (FftSize / 2) - 1);

            _bandStartBins[i] = startBin;
            _bandEndBins[i] = endBin;
        }
    }

    private void BuildWindow()
    {
        for (int i = 0; i < FftSize; i++)
        {
            _window[i] = 0.5f * (1f - MathF.Cos((2f * MathF.PI * i) / (FftSize - 1)));
        }
    }

    private void BuildPalette()
    {
        for (int i = 0; i < MaxBars; i++)
        {
            float t = i / (float)(MaxBars - 1);
            float hue = 18f + (292f * t);
            _barColors[i] = HsvToColor(hue, 0.86f, 1f);
        }
    }

    private void ResetState()
    {
        lock (_audioLock)
        {
            try
            {
                Array.Clear(_ringBuffer, 0, _ringBuffer.Length);
            }
            catch { }
            
            _ringWriteIndex = 0;
            _ringCount = 0;
        }

        try
        {
            Array.Clear(_targetBars, 0, _targetBars.Length);
            Array.Clear(_displayBars, 0, _displayBars.Length);
            Array.Clear(_peakBars, 0, _peakBars.Length);
        }
        catch { }

        _backgroundPhase = 0f;
        _stopping = false;
        _deviceChanged = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (ClientSize.Width <= 1 || ClientSize.Height <= 1)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighSpeed;  // Changed from HighQuality for speed
        g.PixelOffsetMode = PixelOffsetMode.HighSpeed;         // Changed from HighQuality for speed
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        DrawBackground(g);

        switch (_currentStyle)
        {
            case VisualizerStyle.ClassicBars:
                DrawClassicBars(g);
                break;
            case VisualizerStyle.CircularSpectrum:
                DrawCircularSpectrum(g);
                break;
            case VisualizerStyle.WaveformRipple:
                DrawWaveformRipple(g);
                break;
            case VisualizerStyle.ParticleField:
                DrawParticleField(g);
                break;
            case VisualizerStyle.Matrix3D:
                DrawMatrix3D(g);
                break;
            case VisualizerStyle.FireWave:
                DrawFireWave(g);
                break;
            case VisualizerStyle.NeonTunnel:
                DrawNeonTunnel(g);
                break;
            case VisualizerStyle.LightningStorm:
                DrawLightningStorm(g);
                break;
            case VisualizerStyle.QuantumField:
                DrawQuantumField(g);
                break;
            case VisualizerStyle.HologramCube:
                DrawHologramCube(g);
                break;
        }

        DrawBorder(g);
    }

    private void DrawBackground(Graphics g)
    {
        // Simple solid background - faster than gradient
        g.Clear(Color.FromArgb(8, 10, 20));

        // Minimal glow effect - only if average spectrum is high
        if (_averageSpectrum > 0.3f)
        {
            Rectangle rect = ClientRectangle;
            float glowAlpha = 15 + (_averageSpectrum * 25f);

            using GraphicsPath centerGlowPath = new();
            centerGlowPath.AddEllipse(
                -rect.Width * 0.1f,
                rect.Height * 0.15f,
                rect.Width * 1.2f,
                rect.Height * 0.7f);

            using var centerGlow = new PathGradientBrush(centerGlowPath)
            {
                CenterColor = Color.FromArgb((int)glowAlpha, 30, 60, 120),
                SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) }
            };

            g.FillPath(centerGlow, centerGlowPath);
        }
    }

    private void DrawClassicBars(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        float centerY = height / 2f;
        float maxHalfHeight = height * 0.42f;
        
        // Adaptive bar count based on screen size
        int displayBarCount = Math.Min(200, Math.Max(80, width / 6));
        displayBarCount = (displayBarCount / 8) * 8; // Align to 8 for better batching
        float barWidth = (float)width / displayBarCount;
        
        // Create line array for batch drawing
        PointF[] topPoints = new PointF[displayBarCount * 2];
        PointF[] bottomPoints = new PointF[displayBarCount * 2];
        Color[] lineColors = new Color[displayBarCount * 2];

        for (int i = 0; i < displayBarCount; i++)
        {
            int dataIndex = (int)((float)i / displayBarCount * _barCount);
            dataIndex = Math.Clamp(dataIndex, 0, _barCount - 1);
            
            float value = _displayBars[dataIndex];
            if (value < 0.001f)
                value = 0.001f;

            float x = i * barWidth + barWidth * 0.5f;
            float amplitude = value * maxHalfHeight;

            // RGB spectrum color
            Color barColor = GetRGBColor(i, displayBarCount, _backgroundPhase);

            // Top bar line
            topPoints[i * 2] = new PointF(x, centerY);
            topPoints[i * 2 + 1] = new PointF(x, centerY - amplitude);
            lineColors[i * 2] = barColor;
            lineColors[i * 2 + 1] = barColor;

            // Bottom bar line (mirror)
            bottomPoints[i * 2] = new PointF(x, centerY);
            bottomPoints[i * 2 + 1] = new PointF(x, centerY + amplitude);
            lineColors[i * 2] = barColor;
            lineColors[i * 2 + 1] = barColor;
        }

        // Use direct line drawing (no glow, no caps - pure performance)
        using var pen = new Pen(Color.White, Math.Max(1f, barWidth * 0.85f))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        // Draw all top bars
        for (int i = 0; i < displayBarCount; i++)
        {
            pen.Color = lineColors[i * 2];
            g.DrawLine(pen, topPoints[i * 2], topPoints[i * 2 + 1]);
        }

        // Draw all bottom bars
        for (int i = 0; i < displayBarCount; i++)
        {
            pen.Color = lineColors[i * 2];
            g.DrawLine(pen, bottomPoints[i * 2], bottomPoints[i * 2 + 1]);
        }

        // Draw center line
        DrawCenterLine(g);
    }

    private Color GetRGBColor(int index, int total, float phase)
    {
        // RGB spektrum: Kırmızı -> Yeşil -> Mavi -> Kırmızı
        float t = (float)index / total;
        
        // Hue to RGB
        float hue = (t * 360f + phase * 20f) % 360f;
        return HsvToColor(hue, 0.95f, 1f);
    }

    private void DrawCenterLine(Graphics g)
    {
        float y = ClientSize.Height / 2f;

        using var pen = new Pen(Color.FromArgb(85, 255, 200, 120), 1f)
        {
            DashStyle = DashStyle.Dot
        };

        g.DrawLine(pen, 0, y, ClientSize.Width, y);
    }

    private void DrawCircularSpectrum(Graphics g)
    {
        int centerX = ClientSize.Width / 2;
        int centerY = ClientSize.Height / 2;
        float maxRadius = Math.Min(centerX, centerY) * 0.75f;
        int barCount = Math.Min(_barCount, 256);
        float angleStep = 360f / barCount;

        for (int i = 0; i < barCount; i++)
        {
            float angle = i * angleStep * (MathF.PI / 180f);
            float value = _spectrumSmooth[i * _barCount / barCount];
            
            float innerRadius = maxRadius * 0.4f;
            float barHeight = value * maxRadius * 0.5f;
            float outerRadius = innerRadius + barHeight;

            float x1 = centerX + innerRadius * MathF.Cos(angle);
            float y1 = centerY + innerRadius * MathF.Sin(angle);
            float x2 = centerX + outerRadius * MathF.Cos(angle);
            float y2 = centerY + outerRadius * MathF.Sin(angle);

            Color barColor = _barColors[i % _barCount];

            using var pen = new Pen(Color.FromArgb(200, barColor), 2.5f);
            g.DrawLine(pen, x1, y1, x2, y2);
            
            using var capBrush = new SolidBrush(Color.FromArgb(220, barColor));
            g.FillEllipse(capBrush, x2 - 2.5f, y2 - 2.5f, 5f, 5f);
        }

        float centerSize = 25 + _averageSpectrum * 60f;
        using GraphicsPath centerPath = new();
        centerPath.AddEllipse(centerX - centerSize / 2, centerY - centerSize / 2, centerSize, centerSize);
        using var centerBrush = new PathGradientBrush(centerPath)
        {
            CenterColor = Color.FromArgb(180, 255, 255, 255),
            SurroundColors = new[] { Color.FromArgb(40, 100, 100, 200) }
        };
        g.FillPath(centerBrush, centerPath);
    }

    private void DrawWaveformRipple(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        int centerY = height / 2;
        int rippleCount = 5;

        for (int ripple = 0; ripple < rippleCount; ripple++)
        {
            var points = new List<PointF>();
            float phase = _waveOffset + ripple * 0.3f;

            for (int x = 0; x <= width; x += 8)
            {
                float indexNorm = (float)x / width;
                int specIndex = (int)(indexNorm * _barCount);
                specIndex = Math.Clamp(specIndex, 0, _barCount - 1);
                
                float value = _spectrumSmooth[specIndex];
                float offset = MathF.Sin(indexNorm * MathF.PI * 2 + phase) * value * (height * 0.35f);
                points.Add(new PointF(x, centerY + offset));
            }

            if (points.Count > 1)
            {
                float alpha = 200 * (1f - (ripple / (float)rippleCount));
                Color color = Color.FromArgb((int)alpha, _barColors[ripple % _barCount]);

                using var pen = new Pen(color, 2.5f + ripple * 0.5f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                g.DrawCurve(pen, points.ToArray(), 0.3f);
            }
        }

        DrawCenterLine(g);
    }

    private void DrawParticleField(Graphics g)
    {
        foreach (var particle in _particles)
        {
            if (particle.Life <= 0) continue;

            float alpha = particle.Life * 235f;
            Color color = Color.FromArgb((int)alpha, particle.Color);

            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, particle.X - particle.Size / 2f, particle.Y - particle.Size / 2f,
                particle.Size, particle.Size);

            if (particle.Life > 0.5f)
            {
                using var glowBrush = new SolidBrush(Color.FromArgb((int)(alpha * 0.4f), 255, 255, 255));
                g.FillEllipse(glowBrush, particle.X - particle.Size, particle.Y - particle.Size,
                    particle.Size * 2, particle.Size * 2);
            }
        }

        int width = ClientSize.Width;
        for (int i = 0; i < Math.Min(_barCount, width / 2); i++)
        {
            float x = (float)i / _barCount * width;
            float value = _spectrumSmooth[i];
            float y = ClientSize.Height - 40 - value * 80f;

            Color color = _barColors[i];
            using var pen = new Pen(Color.FromArgb(180, color), 2f);
            g.DrawLine(pen, x, ClientSize.Height - 40, x, y);
        }
    }

    private void DrawMatrix3D(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        int gridSize = 25;
        float time = _waveOffset * 0.5f;

        for (int x = 0; x < width; x += gridSize)
        {
            for (int y = 0; y < height; y += gridSize)
            {
                float specIndex = (float)x / width * _barCount;
                int idx = (int)Math.Clamp(specIndex, 0, _barCount - 1);
                float value = _spectrumSmooth[idx];

                float dist = MathF.Sqrt((x - width / 2f) * (x - width / 2f) + (y - height / 2f) * (y - height / 2f));
                float depth = MathF.Sin(dist * 0.01f + time) * 0.5f + 0.5f;
                
                float alpha = (depth + value) * 100f;
                alpha = Math.Clamp(alpha, 0, 200);

                Color gridColor = Color.FromArgb((int)alpha, 0, 200, 150);
                using var pen = new Pen(gridColor, 1f);
                
                g.DrawLine(pen, x, y, x + gridSize, y);
                g.DrawLine(pen, x, y, x, y + gridSize);
            }
        }
    }

    private void DrawFireWave(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        float time = _backgroundPhase * 10f;

        for (int i = 0; i < width; i += 3)
        {
            int specIndex = (int)((float)i / width * _barCount);
            specIndex = Math.Clamp(specIndex, 0, _barCount - 1);
            float intensity = _spectrumSmooth[specIndex];

            if (intensity > 0.05f)
            {
                float flameHeight = intensity * height * 0.6f;
                float flameX = i + MathF.Sin(time + i * 0.02f) * 8f;
                
                using var flameBrush = new LinearGradientBrush(
                    new PointF(flameX, height),
                    new PointF(flameX, height - flameHeight),
                    Color.FromArgb((int)(intensity * 220), 255, 100, 0),
                    Color.FromArgb(30, 255, 200, 0));

                g.FillRectangle(flameBrush, flameX - 4, height - flameHeight, 8, flameHeight);
            }
        }

        using var baseGradient = new LinearGradientBrush(
            new Rectangle(0, height - 60, width, 60),
            Color.FromArgb(180, 255, 120, 0),
            Color.FromArgb(60, 200, 0, 0),
            LinearGradientMode.Vertical);
        g.FillRectangle(baseGradient, 0, height - 60, width, 60);
    }

    private void DrawNeonTunnel(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        int centerX = width / 2;
        int centerY = height / 2;
        int ringCount = 12;
        float maxRadius = Math.Min(centerX, centerY) * 1.1f;

        for (int ring = 0; ring < ringCount; ring++)
        {
            float ringProgress = (float)ring / ringCount;
            int specIndex = (int)(ringProgress * (_barCount - 1));
            float intensity = _spectrumSmooth[specIndex];

            float radius = maxRadius * (1f - ringProgress) + _tunnelDepth * 2f;
            if (radius <= 0) continue;

            float alpha = 150 - ring * 12f;
            alpha = Math.Max(0, alpha + intensity * 60f);

            int hue = (int)(ring * 36 + _backgroundPhase * 100) % 360;
            Color neonColor = HsvToColor(hue, 0.9f, 1f);
            Color lineColor = Color.FromArgb((int)alpha, neonColor);

            using var pen = new Pen(lineColor, 1.5f + intensity * 2f);
            g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
        }

        float centerGlow = 30 + _averageSpectrum * 80f;
        using GraphicsPath glowPath = new();
        glowPath.AddEllipse(centerX - centerGlow, centerY - centerGlow, centerGlow * 2, centerGlow * 2);
        using var centerBrush = new PathGradientBrush(glowPath)
        {
            CenterColor = Color.FromArgb(200, 255, 255, 255),
            SurroundColors = new[] { Color.Transparent }
        };
        g.FillPath(centerBrush, glowPath);
    }

    private void DrawLightningStorm(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;

        foreach (var lightning in _lightnings)
        {
            if (lightning.Points.Count > 1)
            {
                int alpha = (int)(lightning.Life * 200);
                Color lightningColor = Color.FromArgb(alpha, lightning.Color);

                using var pen = new Pen(lightningColor, lightning.Thickness)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                g.DrawCurve(pen, lightning.Points.ToArray(), 0.2f);

                if (lightning.Life > 0.7f)
                {
                    Color glowColor = Color.FromArgb(alpha / 3, Color.White);
                    using var glowPen = new Pen(glowColor, lightning.Thickness + 4f);
                    g.DrawCurve(glowPen, lightning.Points.ToArray(), 0.2f);
                }
            }
        }

        for (int i = 0; i < width; i += 6)
        {
            int specIndex = (int)((float)i / width * _barCount);
            specIndex = Math.Clamp(specIndex, 0, _barCount - 1);
            float intensity = _spectrumSmooth[specIndex];

            if (intensity > 0.3f)
            {
                float barHeight = intensity * height * 0.3f;
                Color barColor = _barColors[i % _barCount];

                using var pen = new Pen(Color.FromArgb((int)(intensity * 180), barColor), 2f);
                g.DrawLine(pen, i, height - 30, i, height - 30 - barHeight);
            }
        }
    }

    private void DrawQuantumField(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;

        for (int i = 0; i < Math.Min(_barCount, 80); i++)
        {
            float angle = (i / (float)Math.Min(_barCount, 80)) * MathF.PI * 2;
            float distance = 80 + _spectrumSmooth[i] * 120f;
            
            float x = width / 2f + MathF.Cos(angle) * distance;
            float y = height / 2f + MathF.Sin(angle) * distance;

            Color qColor = _barColors[i];
            using var brush = new SolidBrush(Color.FromArgb(200, qColor));
            g.FillEllipse(brush, x - 4, y - 4, 8, 8);

            if (i < Math.Min(_barCount, 40))
            {
                float nextAngle = ((i + 1) / (float)Math.Min(_barCount, 80)) * MathF.PI * 2;
                float nextDistance = 80 + _spectrumSmooth[i + 1] * 120f;
                
                float x2 = width / 2f + MathF.Cos(nextAngle) * nextDistance;
                float y2 = height / 2f + MathF.Sin(nextAngle) * nextDistance;

                using var line = new Pen(Color.FromArgb(100, qColor), 1f);
                g.DrawLine(line, x, y, x2, y2);
            }
        }

        for (int i = 0; i < width; i += 5)
        {
            int specIndex = (int)((float)i / width * _barCount);
            specIndex = Math.Clamp(specIndex, 0, _barCount - 1);
            float intensity = _spectrumSmooth[specIndex];

            if (intensity > 0.15f)
            {
                float barHeight = intensity * 100f;
                Color barColor = _barColors[specIndex];

                using var pen = new Pen(Color.FromArgb((int)(intensity * 150), barColor), 2.5f);
                g.DrawLine(pen, i, height - 20, i, height - 20 - barHeight);
            }
        }
    }

    private void DrawHologramCube(Graphics g)
    {
        int width = ClientSize.Width;
        int height = ClientSize.Height;
        int centerX = width / 2;
        int centerY = height / 2;

        float cubeSize = 120 + _averageSpectrum * 80f;
        float angle = _hologramRotation * MathF.PI / 180f;

        // Cube köşeleri
        float[][] corners = new float[][]
        {
            new[] { -cubeSize, -cubeSize, -cubeSize },
            new[] { cubeSize, -cubeSize, -cubeSize },
            new[] { cubeSize, cubeSize, -cubeSize },
            new[] { -cubeSize, cubeSize, -cubeSize },
            new[] { -cubeSize, -cubeSize, cubeSize },
            new[] { cubeSize, -cubeSize, cubeSize },
            new[] { cubeSize, cubeSize, cubeSize },
            new[] { -cubeSize, cubeSize, cubeSize }
        };

        // Köşeleri dön
        float[][] rotated = new float[8][];
        for (int i = 0; i < 8; i++)
        {
            float x = corners[i][0];
            float y = corners[i][1];
            float z = corners[i][2];

            float newX = x * MathF.Cos(angle) - z * MathF.Sin(angle);
            float newZ = x * MathF.Sin(angle) + z * MathF.Cos(angle);

            float newY = y * MathF.Cos(angle) - newZ * MathF.Sin(angle);
            newZ = y * MathF.Sin(angle) + newZ * MathF.Cos(angle);

            rotated[i] = new[] { newX, newY, newZ };
        }

        // Kenarları çiz
        int[][] edges = new[]
        {
            new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 0 },
            new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 4 },
            new[] { 0, 4 }, new[] { 1, 5 }, new[] { 2, 6 }, new[] { 3, 7 }
        };

        for (int i = 0; i < edges.Length; i++)
        {
            int p1 = edges[i][0];
            int p2 = edges[i][1];

            float x1 = centerX + rotated[p1][0];
            float y1 = centerY + rotated[p1][1];
            float x2 = centerX + rotated[p2][0];
            float y2 = centerY + rotated[p2][1];

            Color edgeColor = _barColors[i % _barCount];
            using var pen = new Pen(Color.FromArgb(180, edgeColor), 2f);
            g.DrawLine(pen, x1, y1, x2, y2);
        }

        // Merkez noktası
        using var centerBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
        g.FillEllipse(centerBrush, centerX - 6, centerY - 6, 12, 12);
    }

    private void DrawBorder(Graphics g)
    {
        Rectangle rect = Rectangle.Inflate(ClientRectangle, -4, -4);

        using GraphicsPath path = CreateRoundedRectangle(rect, 12f);
        using var pen = new Pen(Color.FromArgb(26, 255, 255, 255), 1f);

        g.DrawPath(pen, path);
    }

    private float GetAverageBarLevel() => _averageSpectrum;

    private static float ReadSample(byte[] buffer, int offset, WaveFormat format, int bytesPerSample)
    {
        bool isFloat =
            format.Encoding == WaveFormatEncoding.IeeeFloat ||
            (format.Encoding == WaveFormatEncoding.Extensible && bytesPerSample == 4);

        if (isFloat)
        {
            if (bytesPerSample == 4)
                return BitConverter.ToSingle(buffer, offset);

            if (bytesPerSample == 8)
                return (float)BitConverter.ToDouble(buffer, offset);
        }

        return bytesPerSample switch
        {
            2 => BitConverter.ToInt16(buffer, offset) / 32768f,
            3 => Read24Bit(buffer, offset) / 8388608f,
            4 => BitConverter.ToInt32(buffer, offset) / 2147483648f,
            _ => 0f
        };
    }

    private static int Read24Bit(byte[] buffer, int offset)
    {
        int value =
            buffer[offset] |
            (buffer[offset + 1] << 8) |
            (buffer[offset + 2] << 16);

        if ((value & 0x800000) != 0)
            value |= unchecked((int)0xFF000000);

        return value;
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, float radius)
    {
        float d = radius * 2f;
        GraphicsPath path = new();

        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static Color ShiftHue(Color color, float shift)
    {
        ColorToHsv(color, out float h, out float s, out float v);
        h = (h + shift) % 360f;
        if (h < 0f) h += 360f;
        return HsvToColor(h, s, v);
    }

    private static Color HsvToColor(float hue, float saturation, float value)
    {
        hue = hue % 360f;
        if (hue < 0f) hue += 360f;

        saturation = Math.Clamp(saturation, 0f, 1f);
        value = Math.Clamp(value, 0f, 1f);

        float c = value * saturation;
        float x = c * (1f - MathF.Abs(((hue / 60f) % 2f) - 1f));
        float m = value - c;

        float r1, g1, b1;

        if (hue < 60f) { r1 = c; g1 = x; b1 = 0f; }
        else if (hue < 120f) { r1 = x; g1 = c; b1 = 0f; }
        else if (hue < 180f) { r1 = 0f; g1 = c; b1 = x; }
        else if (hue < 240f) { r1 = 0f; g1 = x; b1 = c; }
        else if (hue < 300f) { r1 = x; g1 = 0f; b1 = c; }
        else { r1 = c; g1 = 0f; b1 = x; }

        int r = (int)MathF.Round((r1 + m) * 255f);
        int g = (int)MathF.Round((g1 + m) * 255f);
        int b = (int)MathF.Round((b1 + m) * 255f);

        return Color.FromArgb(
            255,
            Math.Clamp(r, 0, 255),
            Math.Clamp(g, 0, 255),
            Math.Clamp(b, 0, 255));
    }

    private static void ColorToHsv(Color color, out float hue, out float saturation, out float value)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;

        hue = 0f;

        if (delta > 0f)
        {
            if (max == r)
                hue = 60f * (((g - b) / delta) % 6f);
            else if (max == g)
                hue = 60f * (((b - r) / delta) + 2f);
            else
                hue = 60f * (((r - g) / delta) + 4f);
        }

        if (hue < 0f)
            hue += 360f;

        saturation = max == 0f ? 0f : delta / max;
        value = max;
    }

    private class Particle
    {
        public float X, Y, VX, VY, Life, MaxLife, Size;
        public Color Color;

        public void Update()
        {
            X += VX;
            Y += VY;
            Life -= 0.015f;
            VY += 0.08f;
        }

        public bool IsAlive => Life > 0;
    }

    private class Lightning
    {
        public List<PointF> Points = new();
        public float Life = 1f;
        public Color Color;
        public float Thickness;

        public void Update()
        {
            Life -= 0.08f;
        }

        public bool IsAlive => Life > 0;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        try
        {
            _renderTimer?.Stop();
            StopCapture();
        }
        catch { }

        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                // Stop health check timer first
                try { _healthCheckTimer?.Stop(); } catch { }
                
                // Stop render timer
                try { _renderTimer?.Stop(); } catch { }
                
                // Stop audio capture
                StopCapture();

                // CRITICAL FIX (NAudio Issue #849):
                // DO NOT dispose _deviceEnumerator here!
                // Windows COM callbacks may still be active, causing ExecutionEngineException
                // Keep it alive for the form lifetime - it will be cleaned up by GC
                // Attempting to dispose causes: "System.ExecutionEngineException threw from NAudio.Wasapi.dll"
                
                // Cancel any pending FFT task
                try { _fftComputeTask?.Wait(500); } catch { }

                // Dispose resources
                try { _healthCheckTimer?.Dispose(); } catch { }
                try { _renderTimer?.Dispose(); } catch { }

                // Clear collections
                _particles.Clear();
                _lightnings.Clear();
            }
            catch
            {
                // Silently ignore cleanup errors
            }
        }

        try
        {
            base.Dispose(disposing);
        }
        catch
        {
            // NAudio may throw during disposal - ignore
        }
    }
}
