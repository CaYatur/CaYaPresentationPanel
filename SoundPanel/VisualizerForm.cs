using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Collections.Generic;
using NAudio.Dsp;
using SoundPanel.Properties;

public enum VisualizationStyle
{
    ClassicBars = 0,        // Orijinal tasarımınız
    CircularSpectrum = 1,   // Dairesel spektrum
    WaveformRipple = 2,     // Dalga formu dalgalanması
    ParticleField = 3,      // Parçacık alanı
    Matrix3D = 4,           // 3D Matrix tarzı
    FireWave = 5,           // Ateş dalgası
    NeonTunnel = 6,         // Neon tünel efekti
    LightningStorm = 7,     // Şimşek fırtınası
    QuantumField = 8,       // Kuantum alan efekti
    HologramCube = 9        // Hologram küp
}

public class VisualizerForm : Form
{
    private BufferedGraphicsContext _context;
    private BufferedGraphics _buffer;
    private System.Windows.Forms.Timer _timer;
    private WasapiLoopbackCapture _loopbackCapture;
    private WaveInEvent _micCapture;
    private float[] _audioData = new float[256];
    private bool useMicrophone = false;
    private float[] _smoothedAudioData = new float[256];
    private float smoothingFactor = 0.5f;
    private float _backgroundPhase = 0f;
    private const int FFT_SIZE = 16384;
    private Complex[] _fftBuffer = new Complex[FFT_SIZE];
    private float[] _spectrum = new float[FFT_SIZE / 2];
    private float[] _spectrumSmooth = new float[FFT_SIZE / 2];
    private float spectrumSmoothing = 0.5f;
    private const int BAR_COUNT = 256;
    private bool _isFullscreen = false;
    private FormBorderStyle _oldBorderStyle;
    private Rectangle _oldBounds;
    private FormWindowState _oldWindowState;

    // Yeni özellikler
    private VisualizationStyle _currentStyle = VisualizationStyle.ClassicBars;
    private List<Particle> _particles = new List<Particle>();
    private List<Lightning> _lightnings = new List<Lightning>();
    private List<QuantumPoint> _quantumPoints = new List<QuantumPoint>();
    private float _rotationAngle = 0f;
    private float _waveOffset = 0f;
    private float _tunnelDepth = 0f;
    private float _hologramRotation = 0f;
    private Random _random = new Random();

    // Parçacık sınıfı
    private class Particle
    {
        public float X, Y, VX, VY, Life, MaxLife, Size;
        public Color Color;

        public void Update()
        {
            X += VX;
            Y += VY;
            Life -= 0.02f;
            VY += 0.1f; // Yerçekimi
        }

        public bool IsAlive => Life > 0;
    }

    // Şimşek sınıfı
    private class Lightning
    {
        public List<PointF> Points = new List<PointF>();
        public float Life = 1f;
        public Color Color;
        public float Thickness;

        public void Update()
        {
            Life -= 0.1f;
        }

        public bool IsAlive => Life > 0;
    }

    // Kuantum noktası sınıfı
    private class QuantumPoint
    {
        public float X, Y, Z, VX, VY, VZ;
        public float Phase, Frequency;
        public Color Color;
        public float Size;

        public void Update()
        {
            X += VX;
            Y += VY;
            Z += VZ;
            Phase += Frequency;
        }
    }

    public VisualizerForm()
    {
        this.DoubleBuffered = true;
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.Black;

        // Başlangıçta görselleştirme tarzını sor
        ShowStyleSelection();

        _context = BufferedGraphicsManager.Current;
        _buffer = _context.Allocate(this.CreateGraphics(), this.DisplayRectangle);

        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += (s, e) =>
        {
            UpdateAnimations();
            this.Invalidate();
        };
        _timer.Start();

        StartAudioCapture();
        InitializeEffects();

        // Kontroller
        this.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Space)
            {
                useMicrophone = !useMicrophone;
                StartAudioCapture();
            }
            else if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
            }
            else if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                int styleIndex = e.KeyCode == Keys.D0 ? 9 : (e.KeyCode - Keys.D1);
                if (styleIndex < 10)
                    _currentStyle = (VisualizationStyle)styleIndex;
            }
            else if (e.KeyCode == Keys.Tab)
            {
                _currentStyle = (VisualizationStyle)(((int)_currentStyle + 1) % 10);
                this.Text = $"Ses Görselleştiricisi - {GetStyleName(_currentStyle)} (Tab: Değiştir, 1-0: Seç, Space: Kaynak)";
            }
        };

        this.Text = $"Ses Görselleştiricisi - {GetStyleName(_currentStyle)} (Tab: Değiştir, 1-0: Seç, Space: Kaynak)";
        using (var ms = new System.IO.MemoryStream(Resources.SoundPanelLogo3_2048))
        {
            this.Icon = new Icon(ms);
        }
    }

    private void InitializeEffects()
    {
        // Kuantum noktalarını başlat
        for (int i = 0; i < 50; i++)
        {
            _quantumPoints.Add(new QuantumPoint
            {
                X = _random.Next(this.ClientSize.Width),
                Y = _random.Next(this.ClientSize.Height),
                Z = _random.NextSingle() * 100,
                VX = (_random.NextSingle() - 0.5f) * 2f,
                VY = (_random.NextSingle() - 0.5f) * 2f,
                VZ = (_random.NextSingle() - 0.5f) * 2f,
                Phase = _random.NextSingle() * (float)Math.PI * 2,
                Frequency = 0.1f + _random.NextSingle() * 0.2f,
                Color = Color.FromArgb(100, 0, 255, 255),
                Size = 3f + _random.NextSingle() * 5f
            });
        }
    }

    private void ShowStyleSelection()
    {
        string message = "Hangi görselleştirme tarzını kullanmak istiyorsunuz?\n\n" +
                        "1 - Klasik Çubuklar (Varsayılan)\n" +
                        "2 - Dairesel Spektrum\n" +
                        "3 - Dalga Dalgalanması\n" +
                        "4 - Parçacık Alanı\n" +
                        "5 - 3D Matrix\n" +
                        "6 - Ateş Dalgası\n" +
                        "7 - Neon Tünel\n" +
                        "8 - Şimşek Fırtınası\n" +
                        "9 - Kuantum Alan\n" +
                        "0 - Hologram Küp\n\n" +
                        "Çalışırken Tab tuşu ile değiştirebilirsiniz.";

        string input = Microsoft.VisualBasic.Interaction.InputBox(
            message, "Görselleştirme Tarzı Seçin", "1");

        if (int.TryParse(input, out int choice))
        {
            if (choice == 0) choice = 10; // 0 -> HologramCube (9)
            if (choice >= 1 && choice <= 10)
            {
                _currentStyle = (VisualizationStyle)(choice - 1);
            }
        }
    }

    private string GetStyleName(VisualizationStyle style)
    {
        switch (style)
        {
            case VisualizationStyle.ClassicBars: return "Klasik Çubuklar";
            case VisualizationStyle.CircularSpectrum: return "Dairesel Spektrum";
            case VisualizationStyle.WaveformRipple: return "Dalga Dalgalanması";
            case VisualizationStyle.ParticleField: return "Parçacık Alanı";
            case VisualizationStyle.Matrix3D: return "3D Matrix";
            case VisualizationStyle.FireWave: return "Ateş Dalgası";
            case VisualizationStyle.NeonTunnel: return "Neon Tünel";
            case VisualizationStyle.LightningStorm: return "Şimşek Fırtınası";
            case VisualizationStyle.QuantumField: return "Kuantum Alan";
            case VisualizationStyle.HologramCube: return "Hologram Küp";
            default: return "Bilinmeyen";
        }
    }

    private void UpdateAnimations()
    {
        _backgroundPhase += 0.01f;
        _rotationAngle += 1f;
        _waveOffset += 0.1f;
        _tunnelDepth += 0.05f;
        _hologramRotation += 2f;

        // Parçacıkları güncelle
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            _particles[i].Update();
            if (!_particles[i].IsAlive)
            {
                _particles.RemoveAt(i);
            }
        }

        // Şimşekleri güncelle
        for (int i = _lightnings.Count - 1; i >= 0; i--)
        {
            _lightnings[i].Update();
            if (!_lightnings[i].IsAlive)
            {
                _lightnings.RemoveAt(i);
            }
        }

        // Kuantum noktalarını güncelle
        foreach (var point in _quantumPoints)
        {
            point.Update();
            // Sınırları kontrol et
            if (point.X < 0 || point.X > this.ClientSize.Width) point.VX *= -1;
            if (point.Y < 0 || point.Y > this.ClientSize.Height) point.VY *= -1;
        }

        // Yeni efektler için özel güncellemeler
        UpdateSpecialEffects();
    }

    private void UpdateSpecialEffects()
    {
        float avgSpectrum = GetAverageSpectrum();

        // ParticleField için parçacık ekleme
        if (_currentStyle == VisualizationStyle.ParticleField && _particles.Count < 200)
        {
            for (int i = 0; i < Math.Min(_spectrumSmooth.Length, 50); i += 5)
            {
                float intensity = _spectrumSmooth[i];
                if (intensity > 0.1f && _random.NextDouble() < intensity)
                {
                    var particle = new Particle
                    {
                        X = _random.Next(this.ClientSize.Width),
                        Y = this.ClientSize.Height,
                        VX = (_random.NextSingle() - 0.5f) * 10f,
                        VY = -_random.NextSingle() * 15f * intensity,
                        Life = 1f,
                        MaxLife = 1f,
                        Size = 2f + intensity * 8f,
                        Color = ColorFromValue(intensity, i, (float)DateTime.Now.TimeOfDay.TotalSeconds)
                    };
                    _particles.Add(particle);
                }
            }
        }

        // LightningStorm için şimşek ekleme
        if (_currentStyle == VisualizationStyle.LightningStorm && avgSpectrum > 0.3f && _random.NextDouble() < 0.1)
        {
            var lightning = new Lightning
            {
                Life = 1f,
                Color = Color.FromArgb(200, 100 + _random.Next(155), 100 + _random.Next(155), 255),
                Thickness = 2f + avgSpectrum * 5f
            };

            // Şimşek çizgisi oluştur
            var startX = _random.Next(this.ClientSize.Width);
            var startY = 0;
            var endX = _random.Next(this.ClientSize.Width);
            var endY = this.ClientSize.Height;

            lightning.Points.Add(new PointF(startX, startY));

            // Zigzaklı çizgi oluştur
            for (int i = 1; i < 10; i++)
            {
                float progress = (float)i / 10f;
                float x = startX + (endX - startX) * progress + (_random.NextSingle() - 0.5f) * 100;
                float y = startY + (endY - startY) * progress;
                lightning.Points.Add(new PointF(x, y));
            }

            lightning.Points.Add(new PointF(endX, endY));
            _lightnings.Add(lightning);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
        {
            _buffer?.Dispose();
            _buffer = _context.Allocate(this.CreateGraphics(), this.DisplayRectangle);
        }
    }

    private void ToggleFullscreen()
    {
        if (!_isFullscreen)
        {
            _isFullscreen = true;
            _oldBorderStyle = this.FormBorderStyle;
            _oldBounds = this.Bounds;
            _oldWindowState = this.WindowState;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Normal;
            var screen = Screen.FromControl(this);
            this.Bounds = screen.Bounds;
            this.TopMost = true;
        }
        else
        {
            _isFullscreen = false;
            this.TopMost = false;
            this.FormBorderStyle = _oldBorderStyle;
            this.WindowState = _oldWindowState;
            this.Bounds = _oldBounds;
        }
        _buffer?.Dispose();
        _buffer = _context.Allocate(this.CreateGraphics(), this.DisplayRectangle);
    }

    private void StartAudioCapture()
    {
        StopCurrentCapture();

        if (useMicrophone)
        {
            _micCapture = new WaveInEvent();
            _micCapture.WaveFormat = new WaveFormat(44100, 1);
            _micCapture.DataAvailable += (s, e) =>
            {
                var buffer = new WaveBuffer(e.Buffer);
                int sampleCount = e.BytesRecorded / 2;
                float[] samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                    samples[i] = buffer.ShortBuffer[i] / 32768f;

                ProcessAudio(samples);
            };
            _micCapture.StartRecording();
        }
        else
        {
            _loopbackCapture = new WasapiLoopbackCapture();
            _loopbackCapture.DataAvailable += (s, e) =>
            {
                var buffer = new WaveBuffer(e.Buffer);
                int sampleCount = e.BytesRecorded / 4;
                float[] samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                    samples[i] = BitConverter.ToSingle(buffer.ByteBuffer, i * 4);

                ProcessAudio(samples);
            };
            _loopbackCapture.StartRecording();
        }
    }

    private void ProcessAudio(float[] samples)
    {
        int len = Math.Min(samples.Length, FFT_SIZE);
        for (int i = 0; i < FFT_SIZE; i++)
        {
            if (i < len)
                _fftBuffer[i].X = (float)(samples[i] * FastFourierTransform.HammingWindow(i, FFT_SIZE));
            else
                _fftBuffer[i].X = 0f;
            _fftBuffer[i].Y = 0f;
        }
        FastFourierTransform.FFT(true, (int)Math.Log(FFT_SIZE, 2), _fftBuffer);
        for (int i = 0; i < _spectrum.Length; i++)
        {
            float mag = (float)Math.Sqrt(_fftBuffer[i].X * _fftBuffer[i].X + _fftBuffer[i].Y * _fftBuffer[i].Y);
            float db = 20 * (float)Math.Log10(mag + 1e-8f);
            db = Math.Max(-80, db);
            float norm = (db + 80) / 80f;
            float weight = 1.0f - (float)i / _spectrum.Length * 0.2f;
            norm *= weight;
            _spectrumSmooth[i] = _spectrumSmooth[i] * spectrumSmoothing + norm * (1 - spectrumSmoothing);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        _buffer.Graphics.Clear(Color.Black);
        DrawBackground(_buffer.Graphics);

        switch (_currentStyle)
        {
            case VisualizationStyle.ClassicBars:
                DrawClassicBars(_buffer.Graphics);
                break;
            case VisualizationStyle.CircularSpectrum:
                DrawCircularSpectrum(_buffer.Graphics);
                break;
            case VisualizationStyle.WaveformRipple:
                DrawWaveformRipple(_buffer.Graphics);
                break;
            case VisualizationStyle.ParticleField:
                DrawParticleField(_buffer.Graphics);
                break;
            case VisualizationStyle.Matrix3D:
                DrawMatrix3D(_buffer.Graphics);
                break;
            case VisualizationStyle.FireWave:
                DrawFireWave(_buffer.Graphics);
                break;
            case VisualizationStyle.NeonTunnel:
                DrawNeonTunnel(_buffer.Graphics);
                break;
            case VisualizationStyle.LightningStorm:
                DrawLightningStorm(_buffer.Graphics);
                break;
            case VisualizationStyle.QuantumField:
                DrawQuantumField(_buffer.Graphics);
                break;
            case VisualizationStyle.HologramCube:
                DrawHologramCube(_buffer.Graphics);
                break;
        }

        _buffer.Render(e.Graphics);
    }

    private void DrawBackground(Graphics g)
    {
        Rectangle rect = this.ClientRectangle;
        float avgSpectrum = GetAverageSpectrum();

        // Dinamik arkaplan - sese göre değişen renk yoğunluğu
        int intensity = (int)(80 + 60 * avgSpectrum);

        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            rect,
            Color.FromArgb(40, intensity, 0, intensity),
            Color.FromArgb(40, 0, intensity, intensity),
            90f + _backgroundPhase * 10f))
        {
            g.FillRectangle(brush, rect);
        }

        // Geniş ekranlar için ek efekt katmanları
        if (this.ClientSize.Width > 1600)
        {
            // Büyük ekranlar için parlak köşe efektleri
            for (int i = 0; i < 4; i++)
            {
                float cornerX = i % 2 == 0 ? 0 : this.ClientSize.Width;
                float cornerY = i < 2 ? 0 : this.ClientSize.Height;

                using (var radialBrush = new System.Drawing.Drawing2D.PathGradientBrush(
                    new PointF[] {
                        new PointF(cornerX, cornerY),
                        new PointF(cornerX + (i % 2 == 0 ? 200 : -200), cornerY),
                        new PointF(cornerX, cornerY + (i < 2 ? 200 : -200))
                    }))
                {
                    radialBrush.CenterColor = Color.FromArgb((int)(50 * avgSpectrum), 100, 0, 200);
                    radialBrush.SurroundColors = new Color[] { Color.Transparent, Color.Transparent, Color.Transparent };
                    g.FillRectangle(radialBrush, rect);
                }
            }
        }
    }

    private void DrawClassicBars(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        int barCount = Math.Min(BAR_COUNT, width / 4); // Geniş ekranlar için daha fazla çubuk
        int centerY = height / 2;
        float barWidth = width / (float)barCount;
        float scale = height * 0.95f;
        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;
        int fftLen = _spectrumSmooth.Length;

        int[] bandIndices = new int[barCount + 1];
        double minFreq = 20;
        double maxFreq = 22050;
        double minLog = Math.Log10(minFreq);
        double maxLog = Math.Log10(maxFreq);
        for (int i = 0; i <= barCount; i++)
        {
            double logIndex = minLog + (maxLog - minLog) * i / barCount;
            double freq = Math.Pow(10, logIndex);
            int idx = (int)Math.Round(freq / maxFreq * (fftLen - 1));
            if (idx < 0) idx = 0;
            if (idx >= fftLen) idx = fftLen - 1;
            bandIndices[i] = idx;
        }

        for (int i = 0; i < barCount; i++)
        {
            float max = 0f;
            for (int j = bandIndices[i]; j < bandIndices[i + 1]; j++)
            {
                if (_spectrumSmooth[j] > max) max = _spectrumSmooth[j];
            }
            float value = max;
            float heightFactor = (float)Math.Pow(value, 0.85) * scale;
            float x = i * barWidth;
            float y1 = centerY - heightFactor;
            float y2 = centerY + heightFactor;

            Color color = ColorFromValue(value, i, time);

            // Geniş ekranlar için gelişmiş glow efekti
            float glowSize = width > 1600 ? barWidth + 20 : barWidth + 10;
            using (var glowPen = new Pen(Color.FromArgb(90, color), glowSize))
            {
                glowPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                g.DrawLine(glowPen, x + barWidth / 2, y1, x + barWidth / 2, y2);
            }

            using (Brush brush = new SolidBrush(color))
            {
                var rect = new RectangleF(x + 1, y1, barWidth - 2, y2 - y1);
                g.FillRectangle(brush, rect);
                g.FillEllipse(brush, x + 1, y1 - (barWidth - 2) / 2, barWidth - 2, barWidth - 2);
                g.FillEllipse(brush, x + 1, y2 - (barWidth - 2) / 2, barWidth - 2, barWidth - 2);
            }
        }
    }

    private void DrawCircularSpectrum(Graphics g)
    {
        int centerX = this.ClientSize.Width / 2;
        int centerY = this.ClientSize.Height / 2;
        float maxRadius = Math.Min(centerX, centerY) * 0.8f;
        int ringCount = this.ClientSize.Width > 1600 ? 5 : 3; // Geniş ekranlarda daha fazla halka
        int barCount = 360; // Tam daire için daha fazla çubuk
        float angleStep = 360f / barCount;

        for (int ring = 0; ring < ringCount; ring++)
        {
            float radius = maxRadius * (0.3f + 0.7f * ring / ringCount);

            for (int i = 0; i < barCount; i++)
            {
                float angle = i * angleStep + _rotationAngle + ring * 30f;
                float radians = angle * (float)Math.PI / 180f;

                int specIndex = (int)(i * _spectrumSmooth.Length / barCount);
                float value = _spectrumSmooth[specIndex];
                float barHeight = value * radius * 0.6f;

                float x1 = centerX + (float)Math.Cos(radians) * radius;
                float y1 = centerY + (float)Math.Sin(radians) * radius;
                float x2 = centerX + (float)Math.Cos(radians) * (radius + barHeight);
                float y2 = centerY + (float)Math.Sin(radians) * (radius + barHeight);

                Color color = ColorFromValue(value, i + ring * 100, (float)DateTime.Now.TimeOfDay.TotalSeconds);
                color = Color.FromArgb((int)(255 * (1f - ring * 0.15f)), color);

                using (var pen = new Pen(color, 2 + ring))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        // Dinamik merkez
        float centerSize = 30 + GetAverageSpectrum() * 150;
        using (var centerBrush = new System.Drawing.Drawing2D.PathGradientBrush(
            new PointF[] {
                new PointF(centerX - centerSize/2, centerY - centerSize/2),
                new PointF(centerX + centerSize/2, centerY - centerSize/2),
                new PointF(centerX + centerSize/2, centerY + centerSize/2),
                new PointF(centerX - centerSize/2, centerY + centerSize/2)
            }))
        {
            centerBrush.CenterColor = Color.FromArgb(200, 255, 255, 255);
            centerBrush.SurroundColors = new Color[] {
                Color.FromArgb(50, 100, 100, 255),
                Color.FromArgb(50, 100, 100, 255),
                Color.FromArgb(50, 100, 100, 255),
                Color.FromArgb(50, 100, 100, 255)
            };
            g.FillEllipse(centerBrush, centerX - centerSize / 2, centerY - centerSize / 2, centerSize, centerSize);
        }
    }

    private void DrawWaveformRipple(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        int centerY = height / 2;
        int rippleCount = width > 1600 ? 8 : 5; // Geniş ekranlarda daha fazla dalga

        for (int ripple = 0; ripple < rippleCount; ripple++)
        {
            float rippleOffset = _waveOffset + ripple * 1.5f;
            var points = new List<PointF>();

            for (int x = 0; x < width; x += 2) // Daha yoğun noktalar
            {
                float progress = (float)x / width;
                int specIndex = (int)(progress * _spectrumSmooth.Length * 0.4f);
                float intensity = _spectrumSmooth[specIndex];

                // Karmaşık dalga kombinasyonu
                float wave1 = (float)(Math.Sin((x * 0.008f + rippleOffset) * Math.PI) * intensity * 80);
                float wave2 = (float)(Math.Sin((x * 0.015f + rippleOffset * 1.5f) * Math.PI) * intensity * 40);
                float wave3 = (float)(Math.Sin((x * 0.025f + rippleOffset * 2f) * Math.PI) * intensity * 20);

                float combinedWave = wave1 + wave2 + wave3;
                float y = centerY + combinedWave + (ripple - rippleCount / 2) * 60;
                points.Add(new PointF(x, y));
            }

            if (points.Count > 1)
            {
                int alpha = Math.Max(50, 150 - ripple * 15);
                Color color = Color.FromArgb(alpha,
                    (int)(100 + ripple * 20),
                    (int)(150 + ripple * 15),
                    255);

                using (var pen = new Pen(color, 3 + ripple))
                {
                    g.DrawCurve(pen, points.ToArray());
                }

                // Glow efekti
                if (width > 1600)
                {
                    Color glowColor = Color.FromArgb(30, color);
                    using (var glowPen = new Pen(glowColor, 8 + ripple * 2))
                    {
                        g.DrawCurve(glowPen, points.ToArray());
                    }
                }
            }
        }
    }

    private void DrawParticleField(Graphics g)
    {
        // Parçacıkları çiz
        foreach (var particle in _particles)
        {
            float alpha = particle.Life * 255f;
            Color color = Color.FromArgb((int)alpha, particle.Color);

            // Büyük ekranlar için parçacık glow efekti
            if (this.ClientSize.Width > 1600)
            {
                using (var glowBrush = new System.Drawing.Drawing2D.PathGradientBrush(
                    new PointF[] {
                        new PointF(particle.X - particle.Size, particle.Y - particle.Size),
                        new PointF(particle.X + particle.Size, particle.Y - particle.Size),
                        new PointF(particle.X + particle.Size, particle.Y + particle.Size),
                        new PointF(particle.X - particle.Size, particle.Y + particle.Size)
                    }))
                {
                    glowBrush.CenterColor = Color.FromArgb((int)(alpha * 0.3f), particle.Color);
                    glowBrush.SurroundColors = new Color[] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent };
                    g.FillEllipse(glowBrush, particle.X - particle.Size, particle.Y - particle.Size,
                                 particle.Size * 2, particle.Size * 2);
                }
            }

            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, particle.X - particle.Size / 2, particle.Y - particle.Size / 2,
                             particle.Size, particle.Size);
            }
        }

        // Alt spektrum çizgisi
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        float y = height - 80;

        for (int i = 0; i < width; i += 2)
        {
            float progress = (float)i / width;
            int specIndex = (int)(progress * _spectrumSmooth.Length * 0.6f);
            float intensity = _spectrumSmooth[specIndex];
            float lineHeight = intensity * 60;

            Color color = ColorFromValue(intensity, i, (float)DateTime.Now.TimeOfDay.TotalSeconds);
            using (var pen = new Pen(color, 3))
            {
                g.DrawLine(pen, i, y, i, y - lineHeight);
            }
        }
    }

    private void DrawMatrix3D(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        int gridSize = width > 1600 ? 15 : 20; // Geniş ekranlarda daha yoğun grid
        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        for (int x = 0; x < width; x += gridSize)
        {
            for (int y = height / 2; y < height; y += gridSize)
            {
                float progress = (float)x / width;
                int specIndex = (int)(progress * _spectrumSmooth.Length * 0.4f);
                float intensity = _spectrumSmooth[specIndex];

                float depth = (y - height / 2) / (float)(height / 2);
                float perspectiveScale = 1f / (1f + depth * 3f);

                float cubeSize = gridSize * perspectiveScale * (0.4f + intensity * 1.2f);
                float cubeX = x * perspectiveScale + width / 2 * (1 - perspectiveScale);
                float cubeY = y;
                float cubeHeight = intensity * 120 * perspectiveScale;

                if (intensity > 0.08f)
                {
                    Color color = ColorFromValue(intensity, x / gridSize, time);
                    color = Color.FromArgb((int)(255 * perspectiveScale), color);

                    // 3D küp çizimi - gelişmiş
                    using (var brush = new SolidBrush(color))
                    {
                        // Üst yüz
                        var topRect = new RectangleF(cubeX - cubeSize / 2, cubeY - cubeHeight - cubeSize / 2,
                                                   cubeSize, cubeSize);
                        g.FillRectangle(brush, topRect);

                        // Sol yan yüz
                        using (var sideBrush = new SolidBrush(Color.FromArgb(color.A / 2, color.R / 2, color.G / 2, color.B / 2)))
                        {
                            var leftSide = new PointF[]
                            {
                                new PointF(cubeX - cubeSize/2, cubeY - cubeHeight - cubeSize/2),
                                new PointF(cubeX - cubeSize/2, cubeY - cubeHeight + cubeSize/2),
                                new PointF(cubeX - cubeSize, cubeY + cubeSize/2),
                                new PointF(cubeX - cubeSize, cubeY - cubeHeight)
                            };
                            g.FillPolygon(sideBrush, leftSide);
                        }

                        // Sağ yan yüz
                        using (var sideBrush = new SolidBrush(Color.FromArgb(color.A / 3, color.R / 3, color.G / 3, color.B / 3)))
                        {
                            var rightSide = new PointF[]
                            {
                                new PointF(cubeX + cubeSize/2, cubeY - cubeHeight - cubeSize/2),
                                new PointF(cubeX + cubeSize/2, cubeY - cubeHeight + cubeSize/2),
                                new PointF(cubeX + cubeSize, cubeY + cubeSize/2),
                                new PointF(cubeX + cubeSize, cubeY - cubeHeight)
                            };
                            g.FillPolygon(sideBrush, rightSide);
                        }
                    }
                }
            }
        }
    }

    private void DrawFireWave(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        int flameCount = width > 1600 ? 200 : 120; // Geniş ekranlarda daha yoğun alevler
        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        for (int i = 0; i < flameCount; i++)
        {
            float x = (float)i / flameCount * width;
            int specIndex = (int)((float)i / flameCount * _spectrumSmooth.Length * 0.6f);
            float intensity = _spectrumSmooth[specIndex];

            if (intensity > 0.03f)
            {
                float baseY = height - 60;
                int flameHeight = (int)(intensity * 250);
                int flameWidth = 6 + (int)(intensity * 12);

                // Çoklu alev katmanları
                for (int layer = 0; layer < 7; layer++)
                {
                    float layerIntensity = intensity * (7 - layer) / 7f;
                    float layerY = baseY - flameHeight * layer / 7f;
                    float layerWidth = flameWidth * (1f - layer * 0.12f);

                    // Alev dalgalanması
                    float shake1 = (float)(Math.Sin(time * 15 + x * 0.08f + layer) * layerIntensity * 6);
                    float shake2 = (float)(Math.Cos(time * 12 + x * 0.12f + layer * 1.5f) * layerIntensity * 4);

                    // Dinamik alev renkleri
                    int red = 255;
                    int green = Math.Min(255, (int)(255 * layer / 5f + intensity * 100));
                    int blue = layer > 5 ? Math.Min(255, (int)(150 + intensity * 105)) : 0;
                    int alpha = (int)(255 * layerIntensity);

                    Color flameColor = Color.FromArgb(alpha, red, green, blue);

                    using (var brush = new SolidBrush(flameColor))
                    {
                        var flameRect = new RectangleF(x - layerWidth / 2 + shake1 + shake2, layerY - 25,
                                                     layerWidth, 50);
                        g.FillEllipse(brush, flameRect);
                    }
                }
            }
        }

        // Gelişmiş zemin efekti
        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Rectangle(0, height - 60, width, 60),
            Color.FromArgb(180, 255, 120, 0),
            Color.FromArgb(60, 200, 0, 0),
            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
        {
            g.FillRectangle(brush, 0, height - 60, width, 60);
        }

        // Büyük ekranlar için ek ısı dalgası efekti
        if (width > 1600)
        {
            float avgSpectrum = GetAverageSpectrum();
            for (int i = 0; i < 3; i++)
            {
                using (var heatBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, height - 200 - i * 30, width, 30),
                    Color.FromArgb((int)(40 * avgSpectrum), 255, 100, 0),
                    Color.Transparent,
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(heatBrush, 0, height - 200 - i * 30, width, 30);
                }
            }
        }
    }

    // YENİ EFEKTLER:

    private void DrawNeonTunnel(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        int centerX = width / 2;
        int centerY = height / 2;

        int ringCount = width > 1600 ? 15 : 10;
        float maxRadius = Math.Min(centerX, centerY) * 1.2f;

        for (int ring = 0; ring < ringCount; ring++)
        {
            float progress = (float)ring / ringCount;
            float radius = maxRadius * (1f - progress) + 20;

            // Tünel derinlik efekti
            float depthOffset = (_tunnelDepth + ring * 0.5f) % (float)(Math.PI * 2);
            int specIndex = (int)(progress * _spectrumSmooth.Length * 0.4f);
            float intensity = _spectrumSmooth[specIndex];

            // Dinamik renk ve boyut
            float ringThickness = 3f + intensity * 8f;
            int alpha = Math.Min(255, Math.Max(0, (int)(255 * (1f - progress) * (0.5f + intensity))));

            Color neonColor = ColorFromValue(intensity, ring, (float)DateTime.Now.TimeOfDay.TotalSeconds + depthOffset);
            neonColor = Color.FromArgb(alpha, neonColor);

            // Ana halka
            using (var pen = new Pen(neonColor, ringThickness))
            {
                g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // Glow efekti
            using (var glowPen = new Pen(Color.FromArgb(Math.Max(0, alpha / 3), neonColor), ringThickness + 6))
            {
                g.DrawEllipse(glowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // Parıldama efektleri
            if (intensity > 0.6f)
            {
                for (int spark = 0; spark < 8; spark++)
                {
                    float sparkAngle = spark * 45f + _rotationAngle + ring * 10f;
                    float sparkRad = sparkAngle * (float)Math.PI / 180f;
                    float sparkX = centerX + (float)Math.Cos(sparkRad) * radius;
                    float sparkY = centerY + (float)Math.Sin(sparkRad) * radius;

                    using (var sparkBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                    {
                        g.FillEllipse(sparkBrush, sparkX - 3, sparkY - 3, 6, 6);
                    }
                }
            }
        }

        // Merkez parlama
        float centerGlow = 30 + GetAverageSpectrum() * 80;
        using (var centerBrush = new System.Drawing.Drawing2D.PathGradientBrush(
            new PointF[] {
                new PointF(centerX - centerGlow, centerY - centerGlow),
                new PointF(centerX + centerGlow, centerY - centerGlow),
                new PointF(centerX + centerGlow, centerY + centerGlow),
                new PointF(centerX - centerGlow, centerY + centerGlow)
            }))
        {
            centerBrush.CenterColor = Color.FromArgb(200, 255, 255, 255);
            centerBrush.SurroundColors = new Color[] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent };
            g.FillEllipse(centerBrush, centerX - centerGlow, centerY - centerGlow, centerGlow * 2, centerGlow * 2);
        }
    }

    private void DrawLightningStorm(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;

        // Mevcut şimşekleri çiz
        foreach (var lightning in _lightnings)
        {
            if (lightning.Points.Count > 1)
            {
                int lightningAlpha = Math.Min(255, Math.Max(0, (int)(lightning.Life * 255)));
                Color lightningColor = Color.FromArgb(lightningAlpha, lightning.Color);

                // Ana şimşek çizgisi
                using (var pen = new Pen(lightningColor, lightning.Thickness))
                {
                    g.DrawLines(pen, lightning.Points.ToArray());
                }

                // Glow efekti
                int glowAlpha = Math.Min(255, Math.Max(0, (int)(lightning.Life * 100)));
                using (var glowPen = new Pen(Color.FromArgb(glowAlpha, Color.White), lightning.Thickness + 6))
                {
                    g.DrawLines(glowPen, lightning.Points.ToArray());
                }

                // Büyük ekranlar için elektrik kıvılcımları
                if (width > 1600)
                {
                    foreach (var point in lightning.Points)
                    {
                        if (_random.NextDouble() < 0.3)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                float sparkX = point.X + (_random.NextSingle() - 0.5f) * 20;
                                float sparkY = point.Y + (_random.NextSingle() - 0.5f) * 20;
                                int sparkAlpha = Math.Min(255, Math.Max(0, (int)(lightning.Life * 150)));
                                using (var sparkBrush = new SolidBrush(Color.FromArgb(sparkAlpha, 150, 200, 255)))
                                {
                                    g.FillEllipse(sparkBrush, sparkX - 2, sparkY - 2, 4, 4);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Frekans çubukları (yıldırım tetikleyicileri)
        for (int i = 0; i < width; i += 8)
        {
            float progress = (float)i / width;
            int specIndex = (int)(progress * _spectrumSmooth.Length * 0.3f);
            float intensity = _spectrumSmooth[specIndex];

            if (intensity > 0.4f)
            {
                float barHeight = intensity * 100;
                Color barColor = ColorFromValue(intensity, i, (float)DateTime.Now.TimeOfDay.TotalSeconds);
                int barAlpha = Math.Min(255, Math.Max(0, (int)(255 * intensity)));
                barColor = Color.FromArgb(barAlpha, barColor);

                using (var pen = new Pen(barColor, 2))
                {
                    g.DrawLine(pen, i, height, i, height - barHeight);
                }

                // Elektrik çıkıntıları
                if (intensity > 0.7f && _random.NextDouble() < 0.1)
                {
                    float sparkX = i + (_random.NextSingle() - 0.5f) * 30;
                    float sparkY = height - barHeight + (_random.NextSingle() - 0.5f) * 20;

                    using (var sparkPen = new Pen(Color.FromArgb(200, 255, 255, 255), 3))
                    {
                        g.DrawLine(sparkPen, i, height - barHeight, sparkX, sparkY);
                    }
                }
            }
        }

        // Atmosferik elektrik efekti
        float avgSpectrum = GetAverageSpectrum();
        if (avgSpectrum > 0.5f)
        {
            int atmosphereAlpha = Math.Min(255, Math.Max(0, (int)(60 * avgSpectrum)));
            using (var atmosphereBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, width, height / 3),
                Color.FromArgb(atmosphereAlpha, 100, 100, 200),
                Color.Transparent,
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(atmosphereBrush, 0, 0, width, height / 3);
            }
        }
    }

    private void DrawQuantumField(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        // Kuantum noktalarını çiz
        foreach (var point in _quantumPoints)
        {
            float waveEffect = (float)Math.Sin(point.Phase) * 0.5f + 0.5f;
            float size = point.Size * (0.5f + waveEffect);

            // Z konumuna göre perspektif
            float perspective = 1f / (1f + point.Z * 0.01f);
            float screenX = point.X * perspective + width / 2 * (1 - perspective);
            float screenY = point.Y * perspective + height / 2 * (1 - perspective);

            // Alpha değerini 0-255 arasında sınırla
            int alpha = Math.Min(255, Math.Max(0, (int)(255 * perspective * waveEffect)));
            Color pointColor = Color.FromArgb(alpha, point.Color);

            // Ana nokta
            using (var brush = new SolidBrush(pointColor))
            {
                g.FillEllipse(brush, screenX - size / 2, screenY - size / 2, size, size);
            }

            // Kuantum dalgaları
            if (waveEffect > 0.7f)
            {
                for (int ring = 1; ring <= 3; ring++)
                {
                    float ringRadius = size * ring * 2;
                    int ringAlpha = Math.Min(255, Math.Max(0, alpha / (ring * 2)));
                    Color ringColor = Color.FromArgb(ringAlpha, point.Color);

                    using (var pen = new Pen(ringColor, 1))
                    {
                        g.DrawEllipse(pen, screenX - ringRadius / 2, screenY - ringRadius / 2, ringRadius, ringRadius);
                    }
                }
            }
        }

        // Kuantum bağlantıları
        for (int i = 0; i < _quantumPoints.Count; i++)
        {
            for (int j = i + 1; j < _quantumPoints.Count; j++)
            {
                float distance = (float)Math.Sqrt(
                    Math.Pow(_quantumPoints[i].X - _quantumPoints[j].X, 2) +
                    Math.Pow(_quantumPoints[i].Y - _quantumPoints[j].Y, 2));

                if (distance < 150)
                {
                    float connectionStrength = 1f - distance / 150f;
                    int alpha = Math.Min(255, Math.Max(0, (int)(100 * connectionStrength)));

                    using (var pen = new Pen(Color.FromArgb(alpha, 0, 255, 255), 1))
                    {
                        g.DrawLine(pen, _quantumPoints[i].X, _quantumPoints[i].Y,
                                 _quantumPoints[j].X, _quantumPoints[j].Y);
                    }
                }
            }
        }

        // Frekans spektrumu - kuantum çubukları
        for (int i = 0; i < width; i += 6)
        {
            float progress = (float)i / width;
            int specIndex = (int)(progress * _spectrumSmooth.Length * 0.4f);
            float intensity = _spectrumSmooth[specIndex];

            if (intensity > 0.1f)
            {
                float barHeight = intensity * 120;
                float quantumFlicker = (float)(Math.Sin(time * 20 + i * 0.1f) * 0.3f + 0.7f);

                Color barColor = ColorFromValue(intensity * quantumFlicker, i, time);
                int alpha = Math.Min(255, Math.Max(0, (int)(255 * intensity * quantumFlicker)));
                barColor = Color.FromArgb(alpha, barColor);

                using (var pen = new Pen(barColor, 3))
                {
                    g.DrawLine(pen, i, height - 30, i, height - 30 - barHeight);
                }

                // Kuantum parçacık patlaması
                if (intensity > 0.8f && _random.NextDouble() < 0.05)
                {
                    for (int p = 0; p < 5; p++)
                    {
                        float particleX = i + (_random.NextSingle() - 0.5f) * 40;
                        float particleY = height - 30 - barHeight + (_random.NextSingle() - 0.5f) * 40;

                        using (var particleBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
                        {
                            g.FillEllipse(particleBrush, particleX - 2, particleY - 2, 4, 4);
                        }
                    }
                }
            }
        }
    }

    private void DrawHologramCube(Graphics g)
    {
        int width = this.ClientSize.Width;
        int height = this.ClientSize.Height;
        int centerX = width / 2;
        int centerY = height / 2;
        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        // Birden fazla hologram küpü
        int cubeCount = width > 1600 ? 5 : 3;

        for (int cubeIndex = 0; cubeIndex < cubeCount; cubeIndex++)
        {
            float cubeSize = 80 + cubeIndex * 40;
            float rotationX = _hologramRotation + cubeIndex * 30f;
            float rotationY = _hologramRotation * 1.5f + cubeIndex * 20f;
            float rotationZ = _hologramRotation * 0.8f + cubeIndex * 15f;

            // Ses yoğunluğuna göre küp boyutu
            float avgSpectrum = GetAverageSpectrum();
            cubeSize *= (0.8f + avgSpectrum * 0.4f);

            // Küp köşe noktaları
            PointF[] cubePoints = new PointF[8];
            float[,] vertices = new float[8, 3]
            {
                {-1, -1, -1}, {1, -1, -1}, {1, 1, -1}, {-1, 1, -1},
                {-1, -1, 1}, {1, -1, 1}, {1, 1, 1}, {-1, 1, 1}
            };

            // 3D rotasyon ve projeksiyon
            for (int i = 0; i < 8; i++)
            {
                float x = vertices[i, 0] * cubeSize;
                float y = vertices[i, 1] * cubeSize;
                float z = vertices[i, 2] * cubeSize;

                // X ekseni rotasyonu
                float tempY = y * (float)Math.Cos(rotationX * Math.PI / 180) - z * (float)Math.Sin(rotationX * Math.PI / 180);
                float tempZ = y * (float)Math.Sin(rotationX * Math.PI / 180) + z * (float)Math.Cos(rotationX * Math.PI / 180);
                y = tempY;
                z = tempZ;

                // Y ekseni rotasyonu
                float tempX = x * (float)Math.Cos(rotationY * Math.PI / 180) + z * (float)Math.Sin(rotationY * Math.PI / 180);
                tempZ = -x * (float)Math.Sin(rotationY * Math.PI / 180) + z * (float)Math.Cos(rotationY * Math.PI / 180);
                x = tempX;
                z = tempZ;

                // Z ekseni rotasyonu
                tempX = x * (float)Math.Cos(rotationZ * Math.PI / 180) - y * (float)Math.Sin(rotationZ * Math.PI / 180);
                tempY = x * (float)Math.Sin(rotationZ * Math.PI / 180) + y * (float)Math.Cos(rotationZ * Math.PI / 180);
                x = tempX;
                y = tempY;

                // Perspektif projeksiyon
                float perspective = 400f / (400f + z);
                cubePoints[i] = new PointF(centerX + x * perspective, centerY + y * perspective);
            }

            // Küp kenarlarını çiz
            int[,] edges = new int[12, 2]
            {
                {0,1}, {1,2}, {2,3}, {3,0}, // Arka yüz
                {4,5}, {5,6}, {6,7}, {7,4}, // Ön yüz
                {0,4}, {1,5}, {2,6}, {3,7}  // Bağlantılar
            };

            for (int edge = 0; edge < 12; edge++)
            {
                int startIdx = edges[edge, 0];
                int endIdx = edges[edge, 1];

                // Frekans verilerine göre renk
                float intensity = _spectrumSmooth[Math.Min(edge * 10 + cubeIndex * 20, _spectrumSmooth.Length - 1)];
                Color edgeColor = ColorFromValue(intensity, edge + cubeIndex * 20, time);

                // Hologram efekti - titreşim
                float flicker = (float)(Math.Sin(time * 15 + edge + cubeIndex) * 0.3f + 0.7f);
                int alpha = Math.Min(255, Math.Max(0, (int)(255 * intensity * flicker * (1f - cubeIndex * 0.15f))));
                edgeColor = Color.FromArgb(alpha, edgeColor);

                using (var pen = new Pen(edgeColor, 2 + intensity * 3))
                {
                    g.DrawLine(pen, cubePoints[startIdx], cubePoints[endIdx]);
                }

                // Glow efekti
                if (intensity > 0.5f)
                {
                    int glowAlpha = Math.Max(0, alpha / 3);
                    using (var glowPen = new Pen(Color.FromArgb(glowAlpha, edgeColor), 6))
                    {
                        g.DrawLine(glowPen, cubePoints[startIdx], cubePoints[endIdx]);
                    }
                }
            }

            // Küp köşelerinde parçacık efektleri
            for (int i = 0; i < 8; i++)
            {
                float intensity = _spectrumSmooth[Math.Min(i * 5 + cubeIndex * 40, _spectrumSmooth.Length - 1)];
                if (intensity > 0.4f)
                {
                    Color vertexColor = ColorFromValue(intensity, i + cubeIndex * 10, time);
                    int alpha = Math.Min(255, Math.Max(0, (int)(255 * intensity)));

                    using (var brush = new SolidBrush(Color.FromArgb(alpha, vertexColor)))
                    {
                        float dotSize = 4 + intensity * 6;
                        g.FillEllipse(brush, cubePoints[i].X - dotSize / 2, cubePoints[i].Y - dotSize / 2, dotSize, dotSize);
                    }
                }
            }
        }

        // Hologram zemin ızgarası
        if (width > 1600)
        {
            int gridSpacing = 30;
            for (int x = 0; x < width; x += gridSpacing)
            {
                for (int y = height * 2 / 3; y < height; y += gridSpacing)
                {
                    float distance = (float)Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
                    float intensity = _spectrumSmooth[Math.Min((int)(distance * 0.01f), _spectrumSmooth.Length - 1)];

                    if (intensity > 0.2f)
                    {
                        int gridAlpha = Math.Min(255, Math.Max(0, (int)(100 * intensity)));
                        Color gridColor = Color.FromArgb(gridAlpha, 0, 255, 255);
                        using (var pen = new Pen(gridColor, 1))
                        {
                            g.DrawLine(pen, x, y, x + gridSpacing, y);
                            g.DrawLine(pen, x, y, x, y + gridSpacing);
                        }
                    }
                }
            }
        }
    }

    private float GetAverageSpectrum()
    {
        float sum = 0f;
        int count = Math.Min(50, _spectrumSmooth.Length);
        for (int i = 0; i < count; i++)
        {
            sum += _spectrumSmooth[i];
        }
        return sum / count;
    }

    private Color ColorFromValue(float value, int index, float time)
    {
        float hue = (index * 2f + time * 50f) % 360f;
        float sat = 0.8f + 0.2f * value;
        float val = 0.6f + 0.4f * value;
        return ColorFromHSV(hue, sat, val);
    }

    private Color ColorFromHSV(float hue, float saturation, float value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);
        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
        switch (hi)
        {
            case 0: return Color.FromArgb(v, t, p);
            case 1: return Color.FromArgb(q, v, p);
            case 2: return Color.FromArgb(p, v, t);
            case 3: return Color.FromArgb(p, q, v);
            case 4: return Color.FromArgb(t, p, v);
            default: return Color.FromArgb(v, p, q);
        }
    }

    private void StopCurrentCapture()
    {
        _loopbackCapture?.StopRecording();
        _loopbackCapture?.Dispose();
        _micCapture?.StopRecording();
        _micCapture?.Dispose();
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisualizerForm));
        SuspendLayout();
        // 
        // VisualizerForm
        // 
        ClientSize = new Size(284, 261);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "VisualizerForm";
        ResumeLayout(false);

    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopCurrentCapture();
        base.OnFormClosing(e);
    }
}