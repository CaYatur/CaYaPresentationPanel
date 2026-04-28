using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Threading.Tasks;

namespace SoundPanel
{
    public partial class VirtualCableTransferForm : Form
    {
        // Static ctor: DllImport çözümleyiciyi kur (DllImport çağrılmadan önce çalışır)
        static VirtualCableTransferForm()
        {
            NativeLoader.SetupResolver();
        }

        // Native loader: Resource'tan çıkarır, yükler ve cleanup yapar.
        private static class NativeLoader
        {
            private const string LibBaseName = "ApplicationLoopback";
            private static readonly object sync = new object();
            private static string extractedPath;
            private static IntPtr loadedHandle = IntPtr.Zero;
            private static bool resolverInstalled = false;

            public static void SetupResolver()
            {
                lock (sync)
                {
                    if (resolverInstalled) return;
                    NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), Resolver);
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();
                    resolverInstalled = true;
                }
            }

            private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
            {
                // Sadece bizim kütüphanemiz için müdahale et
                if (!string.Equals(libraryName, LibBaseName, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(libraryName, LibBaseName + ".dll", StringComparison.OrdinalIgnoreCase))
                {
                    return IntPtr.Zero;
                }

                lock (sync)
                {
                    if (loadedHandle != IntPtr.Zero)
                    {
                        return loadedHandle;
                    }

                    // Resource'tan bytes al
                    var asm = assembly ?? Assembly.GetExecutingAssembly();
                    var rm = Properties.Resources.ResourceManager;

                    string arch = IntPtr.Size == 8 ? "x64" : "x86";
                    string[] candidates = {
                        $"{LibBaseName}_{arch}",
                        $"{LibBaseName}_{arch}.dll",
                        $"{LibBaseName}",
                        $"{LibBaseName}.dll"
                    };

                    object resObj = null;
                    byte[] bytes = null;
                    foreach (var name in candidates)
                    {
                        try { resObj = rm.GetObject(name); } catch { resObj = null; }
                        if (resObj == null) continue;

                        if (resObj is byte[] b) { bytes = b; break; }
                        if (resObj is System.IO.UnmanagedMemoryStream ums)
                        {
                            using var ms = new MemoryStream();
                            ums.CopyTo(ms);
                            bytes = ms.ToArray();
                            break;
                        }
                        if (resObj is MemoryStream ms2) { bytes = ms2.ToArray(); break; }
                    }

                    if (bytes == null)
                    {
                        // Resource yoksa resolver başarısız olur, DllImport normal yüklemeye devam eder
                        return IntPtr.Zero;
                    }

                    // Geçici dosyaya yaz (benzersiz isim)
                    string tempDir = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name, "native");
                    Directory.CreateDirectory(tempDir);
                    string fileName = $"{LibBaseName}_{arch}_{Guid.NewGuid():N}.dll";
                    extractedPath = Path.Combine(tempDir, fileName);
                    File.WriteAllBytes(extractedPath, bytes);

                    // Load
                    if (NativeLibrary.TryLoad(extractedPath, out var handle))
                    {
                        loadedHandle = handle;
                        return loadedHandle;
                    }

                    // Load başarısızsa temizle
                    try { File.Delete(extractedPath); } catch { }
                    extractedPath = null;
                    return IntPtr.Zero;
                }
            }

            // Cleanup: önce loadedHandle'ı free et, sonra dosyayı sil
            public static void Cleanup()
            {
                lock (sync)
                {
                    try
                    {
                        if (loadedHandle != IntPtr.Zero)
                        {
                            try
                            {
                                NativeLibrary.Free(loadedHandle);
                            }
                            catch
                            {
                                // Free başarısız olabilir (kullanımdaysa). Yine de deniyoruz.
                            }
                            loadedHandle = IntPtr.Zero;
                        }

                        if (!string.IsNullOrEmpty(extractedPath))
                        {
                            try
                            {
                                if (File.Exists(extractedPath))
                                    File.Delete(extractedPath);
                                var parent = Path.GetDirectoryName(extractedPath);
                                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent) && Directory.GetFiles(parent).Length == 0 && Directory.GetDirectories(parent).Length == 0)
                                {
                                    try { Directory.Delete(parent); } catch { }
                                }
                            }
                            catch
                            {
                                // Silme başarısız olursa yok say
                            }
                            extractedPath = null;
                        }
                    }
                    catch { /* hiçbir exception dışarı fırlatılmaz */ }
                }
            }
        }

        // DLL Importları (Cdecl ile eşleştirildi)
        [DllImport("ApplicationLoopback.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetAudioCallback(AudioCallback callback);

        [DllImport("ApplicationLoopback.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartCaptureAsync(uint processId, bool includeProcessTree, ushort channel, uint sampleRate, ushort bitsPerSample);

        [DllImport("ApplicationLoopback.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopCaptureAsync();

        // Callback Delegate
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioCallback(IntPtr data, uint size);

        // NAudio Bileşenleri
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveOutEvent waveOutEvent; // WaveOut yerine WaveOutEvent
        private WaveFormat waveFormat;
        private VolumeWaveProvider16 volumeProvider; // Ses seviyesi için

        // Ses Parametreleri
        private float outputVolume = 6.5f; // Varsayılan ses seviyesi (%100)
        private const uint SampleRate = 96000; // Daha uyumlu örnekleme
        private const ushort Channels = 2; // Stereo
        private const ushort BitsPerSample = 16; // DLL ve NAudio 16 bit bekliyor
        private int bufferMilliseconds = 300; // Daha yüksek gecikme, kalite için

        // Cihaz numaralarını saklamak için liste
        private List<int> deviceNumbers = new List<int>();

        private AudioCallback audioCallbackRef; // GC koruması için
        private bool isTransferring = false; // Aktarım durumu
        private uint externalPid = 0; // Sadece başlatılırken girilen PID
        private CancellationTokenSource processMonitorTokenSource;
        private Task processMonitorTask; // izleme görevini referansla

        private volatile bool shouldExitImmediately = false;

        // Shutdown control
        private volatile bool isClosingInProgress = false;
        private volatile bool allowClose = false;

        public VirtualCableTransferForm(uint pid)
        {
            InitializeComponent();
            waveFormat = new WaveFormat((int)SampleRate, BitsPerSample, Channels);
            externalPid = pid;

            // Kendi PID'sini tespit et
            uint myPid = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;

            // Eğer dışarıdan verilen PID kendi PID ise, uyarı ver ve formu kapat
            if (externalPid == myPid)
            {
                MessageBox.Show("Hata: Kendi uygulamanızın PID'si ile ses transferi başlatılamaz! Bu yankı/feedback sorununa yol açar. Lütfen başka bir uygulamanın PID'sini girin.", "Yankı Koruması");
                Environment.Exit(0);
                return;
            }

            if (externalPid == 0)
            {
                MessageBox.Show("Hata: Program 1'im PID'si verilmedi!");
                Environment.Exit(0); // PID yoksa formu kapat
            }

            // FormClosing handler ekle (kapatılma akışını kontrol etmek için)
            this.FormClosing += VirtualCableTransferForm_FormClosing;
        }

        private void VirtualCableTransferForm_Load(object sender, EventArgs e)
        {
            TopMost = true; // Formu en üste yap
            TopMost = false; // Formu en üste yap
            Focus(); // Formu odakla

            // Tüm oynatma cihazlarını listele (artık Cable / Virtual filtrelemesi yok)
            cmbDevices.Items.Clear();
            deviceNumbers.Clear();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                try
                {
                    var capabilities = WaveOut.GetCapabilities(i);
                    string productName = string.IsNullOrWhiteSpace(capabilities.ProductName)
                        ? $"Ses Cihazı {i}"
                        : capabilities.ProductName;

                    cmbDevices.Items.Add(productName);
                    deviceNumbers.Add(i);
                }
                catch
                {
                    // Herhangi bir cihaz okunamazsa diğer cihazları listelemeye devam et
                }
            }

            if (cmbDevices.Items.Count > 0)
                cmbDevices.SelectedIndex = 0;
            else
                MessageBox.Show("Kullanılabilir ses çıkış cihazı bulunamadı!");

            // PID izleme her zaman başlasın (aktarımı başlatmadan da)
            StartProcessMonitorIfNeeded(externalPid);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isTransferring)
            {
                SafeUpdateStatus("Zaten aktarma başlatıldı!");
                return;
            }

            if (cmbDevices.SelectedIndex < 0)
            {
                SafeUpdateStatus("Ses cihazı seçin!");
                return;
            }

            uint processId = externalPid;

            // Reset previous
            InvokeIfRequired(() =>
            {
                try { waveOutEvent?.Stop(); } catch { }
                try { waveOutEvent?.Dispose(); } catch { }
                waveOutEvent = null;
                bufferedWaveProvider = null;
                volumeProvider = null;
            });

            bufferMilliseconds = 300; // Daha yüksek gecikme, kalite için
            bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
            {
                DiscardOnBufferOverflow = true, // Bozulmayı önle
                BufferLength = waveFormat.AverageBytesPerSecond * bufferMilliseconds / 1000,
                ReadFully = true
            };

            volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider)
            {
                Volume = outputVolume
            };

            waveOutEvent = new WaveOutEvent()
            {
                DeviceNumber = deviceNumbers[cmbDevices.SelectedIndex],
                DesiredLatency = bufferMilliseconds,
                NumberOfBuffers = 4
            };
            waveOutEvent.Init(volumeProvider);
            waveOutEvent.Play();

            audioCallbackRef = new AudioCallback(OnAudioReceived);
            SetAudioCallback(audioCallbackRef);

            isTransferring = true;

            SafeUpdateStatus($"Durum: Aktarma başladı (PID: {processId})...");

            // Önceden çalışan izleyici varsa yenisini başlatmadan önce iptal et
            RestartProcessMonitor(processId);

            Task.Run(() =>
            {
                StartCaptureAsync(processId, true, Channels, SampleRate, BitsPerSample);
            });
        }

        private void StartProcessMonitorIfNeeded(uint pid)
        {
            if (processMonitorTask == null || processMonitorTask.IsCompleted || processMonitorTokenSource == null || processMonitorTokenSource.IsCancellationRequested)
            {
                processMonitorTokenSource?.Dispose();
                processMonitorTokenSource = new CancellationTokenSource();
                processMonitorTask = StartProcessMonitor(pid, processMonitorTokenSource.Token);
            }
        }

        private void RestartProcessMonitor(uint pid)
        {
            try { processMonitorTokenSource?.Cancel(); } catch { }
            try { processMonitorTask?.Wait(500); } catch { }
            try { processMonitorTokenSource?.Dispose(); } catch { }

            processMonitorTokenSource = new CancellationTokenSource();
            processMonitorTask = StartProcessMonitor(pid, processMonitorTokenSource.Token);
        }

        private Task StartProcessMonitor(uint pid, CancellationToken token)
        {
            return Task.Run(() =>
            {
                bool processExited = false;
                try
                {
                    while (!token.IsCancellationRequested && !shouldExitImmediately)
                    {
                        try
                        {
                            var proc = Process.GetProcessById((int)pid);
                            if (proc.HasExited)
                            {
                                processExited = true;
                                break;
                            }
                        }
                        catch
                        {
                            // Process yoksa veya hata varsa çık
                            processExited = true;
                            break;
                        }
                        Thread.Sleep(500); // Daha sık kontrol (yarım saniye)
                    }
                }
                catch { processExited = true; }

                // PID kapandıysa uygulamayı tamamen kapat
                if ((processExited || shouldExitImmediately) && !token.IsCancellationRequested)
                {
                    // Tüm threadlerden ve UI'dan bağımsız olarak uygulamayı sonlandır
                    try { InvokeIfRequired(() => Application.Exit()); } catch { }
                }
            }, token);
        }

        // UI thread-safe güncelleme fonksiyonu
        private void SafeUpdateStatus(string text)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = text));
            }
            else
            {
                lblStatus.Text = text;
            }
        }

        private void OnAudioReceived(IntPtr data, uint size)
        {
            if (!isTransferring || bufferedWaveProvider == null) return;
            if (size == 0) return;

            // 16 bit stereo PCM kontrolü
            int frameSize = Channels * (BitsPerSample / 8);
            if (size % frameSize != 0) return; // Tam frame değilse ekleme

            byte[] buffer = new byte[(int)size];
            Marshal.Copy(data, buffer, 0, (int)size);
            bufferedWaveProvider.AddSamples(buffer, 0, (int)size);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!isTransferring) return;

            StopTransferInternal();
            try { processMonitorTokenSource?.Cancel(); } catch { }
            SafeUpdateStatus("Durum: Aktarma durduruldu.");
        }

        // Internal method to stop transfer safely on UI thread
        private void StopTransferInternal()
        {
            InvokeIfRequired(() =>
            {
                try { StopCaptureAsync(); } catch { }
                try { SetAudioCallback(null); } catch { }
                try { waveOutEvent?.Stop(); } catch { }
                try { waveOutEvent?.Dispose(); } catch { }
                waveOutEvent = null;
                bufferedWaveProvider = null;
                volumeProvider = null;
                audioCallbackRef = null;
                isTransferring = false;
            });
        }

        // Helper to invoke on UI thread if necessary
        private void InvokeIfRequired(Action a)
        {
            if (this.IsHandleCreated && this.InvokeRequired)
                this.Invoke(a);
            else
                a();
        }

        // FormClosing handler: sade ve güvenli kapanış
        private void VirtualCableTransferForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isClosingInProgress) return;
            isClosingInProgress = true;
            try
            {
                SafeUpdateStatus("Kapatılıyor...");
                try { processMonitorTokenSource?.Cancel(); } catch { }
                try { processMonitorTask?.Wait(500); } catch { }
                StopTransferInternal();
                try { SetAudioCallback(null); } catch { }
                shouldExitImmediately = true;
                try { NativeLoader.Cleanup(); } catch { }

                // Form normal şekilde kapanacak; Application.Run dönecek ve process sonlanacak
            }
            catch { }
            finally
            {
                // Kapanışı engelleme, doğal akışta kapansın
                isClosingInProgress = false;
            }
        }

        // Helper that waits until key resources appear released or timeout
        // We'll check: isTransferring==false && waveOut==null
        private async Task<bool> WaitForCleanupAsync(int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!isTransferring && waveOutEvent == null)
                {
                    try { NativeLoader.Cleanup(); } catch { }
                    return true;
                }
                await Task.Delay(100);
            }
            return false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { StopCaptureAsync(); } catch { }
            try { SetAudioCallback(null); } catch { }
            try { waveOutEvent?.Stop(); } catch { }
            try { waveOutEvent?.Dispose(); } catch { }
            waveOutEvent = null;
            bufferedWaveProvider = null;
            volumeProvider = null;
            isTransferring = false;
            try { processMonitorTokenSource?.Cancel(); } catch { }
            try { processMonitorTokenSource?.Dispose(); } catch { }
            shouldExitImmediately = true; // Kapatma isteği
            NativeLoader.Cleanup();
            base.OnFormClosed(e);
        }
    }
}