using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace SoundPanel
{
    public partial class VideoPlayerCY : Form
    {
        private VideoView vlcVideoView;
        private LibVLC libVLC;
        private MediaPlayer mediaPlayer;
        private Media media;
        private string videoPath;
        private VideoControlForm controlForm;
        private volatile bool isClosing = false;
        private volatile bool isDisposed = false;
        private readonly object lockObject = new object();
        private CancellationTokenSource cancellationTokenSource;
        private readonly string libVlcDirectory;

        public VideoPlayerCY(string videoPath)
        {
            Opacity = 0;
            InitializeComponent();
            InitializeCloseBtnHover();
            Hide();
            this.videoPath = videoPath;
            this.cancellationTokenSource = new CancellationTokenSource();

            // Determine embedded libvlc folder (expected in output under "libvlc\win-x64" or "libvlc\win-x86")
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var archFolder = Environment.Is64BitProcess ? "win-x64" : "win-x86";
            libVlcDirectory = Path.Combine(baseDir, "libvlc", archFolder);

            // If the folder exists, prepend to PATH so native loader finds libvlc.dll and libvlccore.dll
            try
            {
                if (Directory.Exists(libVlcDirectory))
                {
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                    if (!currentPath.Contains(libVlcDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.SetEnvironmentVariable("PATH", libVlcDirectory + ";" + currentPath);
                    }
                }
            }
            catch { }

            // Initialize LibVLCSharp core AFTER we adjusted PATH
            Core.Initialize();
        }

        private void VideoPlayerCY_Load(object sender, EventArgs e)
        {
            Hide();
            try
            {
                // Video yolu kontrolü
                if (string.IsNullOrEmpty(videoPath) || !System.IO.File.Exists(videoPath))
                {
                    MessageBox.Show("Geçersiz video dosyası yolu.");
                    return;
                }

                // VLC Media Player'ı başlat
                AddMediaPlayer(videoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata meydana geldi: " + ex.Message);
            }
        }

        private async void AddMediaPlayer(string url)
        {
            try
            {
                // Try to create LibVLC using embedded plugin path if available
                try
                {
                    if (!string.IsNullOrEmpty(libVlcDirectory) && Directory.Exists(libVlcDirectory))
                    {
                        var pluginsPath = Path.Combine(libVlcDirectory, "plugins");
                        if (Directory.Exists(pluginsPath))
                        {
                            libVLC = new LibVLC(new[] { $"--plugin-path={pluginsPath}" });
                        }
                        else
                        {
                            // folder exists but no plugins subfolder -> create LibVLC without explicit plugin path
                            libVLC = new LibVLC();
                        }
                    }
                    else
                    {
                        libVLC = new LibVLC();
                    }
                }
                catch (Exception exLibInit)
                {
                    // Fallback to default initialization
                    try
                    {
                        libVLC = new LibVLC();
                    }
                    catch (Exception ex2)
                    {
                        throw new Exception("LibVLC başlatılamadı: " + exLibInit.Message + " / " + ex2.Message);
                    }
                }

                // MediaPlayer oluştur
                mediaPlayer = new MediaPlayer(libVLC);

                // VideoView kontrolünü oluştur
                vlcVideoView = new VideoView();
                vlcVideoView.Dock = DockStyle.Fill;
                vlcVideoView.MediaPlayer = mediaPlayer;

                this.Controls.Add(vlcVideoView);

                // Kontrol formunu başlat
                controlForm = new VideoControlForm(mediaPlayer, this);
                controlForm.Show();

                // Media oluştur
                media = new Media(libVLC, url, FromType.FromPath);

                // Video tamamlandığında formu kapat
                mediaPlayer.EndReached += MediaPlayer_EndReached;

                // Formu tam ekran yap
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.BackColor = System.Drawing.Color.Black;

                // Başlangıç ayarları - SES SEVİYESİNİ VARSAYILAN %50 YAP
                mediaPlayer.Volume = 50;
                this.Opacity = 0;
                vlcVideoView.Visible = false;

                await Task.Delay(200, cancellationTokenSource.Token);

                if (!isClosing && !isDisposed && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    this.Opacity = 1;
                    vlcVideoView.Visible = true;
                    label1.Visible = false;

                    // Videoyu oynat
                    mediaPlayer.Play(media);

                    await Task.Delay(200, cancellationTokenSource.Token);
                    if (!isClosing && !isDisposed && !cancellationTokenSource.Token.IsCancellationRequested)
                        mediaPlayer.Volume = 50; // Ses seviyesini tekrar %50'ye ayarla
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation normal durum
            }
            catch (Exception ex)
            {
                MessageBox.Show("VLC Media Player başlatma hatası: " + ex.Message);
            }
            await Task.Delay(150);
            Show();
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            // Video normal şekilde bittiğinde
            Task.Run(async () =>
            {
                try
                {
                    if (!isClosing && !isDisposed)
                    {
                        await Task.Delay(100); // Kısa bekleme

                        if (this.InvokeRequired)
                        {
                            this.BeginInvoke(new Action(() => CloseApplicationSafely(false)));
                        }
                        else
                        {
                            CloseApplicationSafely(false);
                        }
                    }
                }
                catch { }
            });
        }

        public void CloseApplication()
        {
            CloseApplicationSafely(true); // Manuel kapatma
        }

        private void CloseApplicationSafely(bool isManualClose)
        {
            lock (lockObject)
            {
                if (isClosing || isDisposed) return;
                isClosing = true;
            }

            try
            {
                // Cancellation token'ı iptal et
                cancellationTokenSource?.Cancel();

                // Manuel kapatmada kontrol panelini zorla kapat
                if (isManualClose)
                {
                    CloseControlFormImmediately();
                }
                else
                {
                    // Normal kapanmada kontrol formunu güvenli şekilde kapat
                    if (controlForm != null && !controlForm.IsDisposed)
                    {
                        controlForm.RequestClose();
                    }
                }

                // VLC kaynaklarını hemen temizle
                ForceCleanupVLC();

                // Ana formu kapat
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                this.Close();
                            }
                            catch { }
                        }));
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
            catch (Exception)
            {
                // Hata durumunda zorla kapat
                try
                {
                    CloseControlFormImmediately();
                    KillAllVLCProcesses();
                    ForceCleanup();
                    Application.Exit();
                }
                catch { }
            }
        }

        private void CloseControlFormImmediately()
        {
            try
            {
                if (controlForm != null && !controlForm.IsDisposed)
                {
                    // Kontrol formunu hemen kapat
                    if (controlForm.InvokeRequired)
                    {
                        controlForm.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                controlForm.ForceClose();
                            }
                            catch { }
                        }));
                    }
                    else
                    {
                        controlForm.ForceClose();
                    }
                }
            }
            catch { }
        }

        private void ForceCleanupVLC()
        {
            try
            {
                // Event handler'ları hemen ayır
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.EndReached -= MediaPlayer_EndReached;
                    }
                    catch { }
                }

                // VideoView'i hemen ayır
                if (vlcVideoView != null)
                {
                    try
                    {
                        vlcVideoView.MediaPlayer = null;
                    }
                    catch { }
                }

                // MediaPlayer'ı hemen durdur
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.Volume = 0;
                        mediaPlayer.Stop();
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void KillAllVLCProcesses()
        {
            try
            {
                // VLC ile ilgili tüm süreçleri sonlandır
                Process[] processes = Process.GetProcessesByName("vlc");
                foreach (Process process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch { }
                }

                // libvlc süreçlerini de kontrol et
                processes = Process.GetProcessesByName("libvlc");
                foreach (Process process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ForceCleanup()
        {
            lock (lockObject)
            {
                if (isDisposed) return;
                isDisposed = true;
            }

            try
            {
                ForceCleanupVLC();

                // MediaPlayer'ı dispose et
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.Dispose();
                    }
                    catch { }
                    mediaPlayer = null;
                }

                // Media'yı temizle
                if (media != null)
                {
                    try
                    {
                        media.Dispose();
                    }
                    catch { }
                    media = null;
                }

                // LibVLC'yi temizle
                if (libVLC != null)
                {
                    try
                    {
                        libVLC.Dispose();
                    }
                    catch { }
                    libVLC = null;
                }

                // VideoView'i temizle
                if (vlcVideoView != null)
                {
                    try
                    {
                        vlcVideoView.Dispose();
                    }
                    catch { }
                    vlcVideoView = null;
                }

                // CancellationTokenSource'u temizle
                if (cancellationTokenSource != null)
                {
                    try
                    {
                        cancellationTokenSource.Dispose();
                    }
                    catch { }
                    cancellationTokenSource = null;
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            lock (lockObject)
            {
                if (isClosing || isDisposed)
                {
                    ForceCleanup();
                    base.OnFormClosing(e);
                    return;
                }
                isClosing = true;
            }

            try
            {
                // Manuel kapatma
                CloseControlFormImmediately();
                KillAllVLCProcesses();
                ForceCleanup();
            }
            catch (Exception)
            {
                try
                {
                    KillAllVLCProcesses();
                    ForceCleanup();
                }
                catch { }
            }

            base.OnFormClosing(e);
        }

        private void CloseBtnW_Click(object sender, EventArgs e)
        {
            Close();
        }

 

        private System.Windows.Forms.Timer closeBtnHoverTimer;

        private void InitializeCloseBtnHover()
        {
            try
            {
                // İlk durumda gizle (designer ile eklenmiş CloseBtnW varsa)
                if (this.Controls.ContainsKey("CloseBtnW"))
                {
                    try { CloseBtnW.Visible = false; } catch { }
                }

                // Lokal değişkenlerle fare hareketi / durağanlık takibi (alan içine girip 1s hareketsiz => gizle)
                var lastCursorPos = Cursor.Position;
                var lastMoveTime = DateTime.UtcNow;
                var inactivityThreshold = TimeSpan.FromSeconds(1);
                var hiddenByInactivity = false;

                closeBtnHoverTimer = new System.Windows.Forms.Timer
                {
                    Interval = 100 // daha duyarlı tepki için 100ms
                };

                closeBtnHoverTimer.Tick += (s, e) =>
                {
                    try
                    {
                        if (isClosing || isDisposed) return;

                        // Eğer CloseBtnW yoksa atla
                        if (!this.Controls.ContainsKey("CloseBtnW")) return;

                        var cursorPos = Cursor.Position;
                        // Formun ekran koordinatlarındaki dikdörtgeni
                        var formRect = this.RectangleToScreen(this.ClientRectangle);
                        bool isOverForm = formRect.Contains(cursorPos);

                        // Fare form alanına girdiyse: hareket var mı kontrol et
                        if (isOverForm)
                        {
                            bool moved = cursorPos != lastCursorPos;

                            if (moved)
                            {
                                // Hareket varsa butonu göster ve zamanlayıcıyı resetle
                                lastMoveTime = DateTime.UtcNow;
                                if (!CloseBtnW.Visible)
                                {
                                    CloseBtnW.Visible = true;
                                    try { CloseBtnW.BringToFront(); } catch { }
                                }
                                hiddenByInactivity = false;
                            }
                            else
                            {
                                // Hareket yoksa, durağanlık süresini kontrol et
                                if (!hiddenByInactivity && DateTime.UtcNow - lastMoveTime >= inactivityThreshold)
                                {
                                    // 3 saniyelik hareketsizlik => gizle
                                    try { CloseBtnW.Visible = false; } catch { }
                                    hiddenByInactivity = true;
                                }
                            }
                        }
                        else
                        {
                            // Fare alanın dışındaysa kesin gizle ve resetle
                            if (CloseBtnW.Visible)
                            {
                                try { CloseBtnW.Visible = false; } catch { }
                            }
                            hiddenByInactivity = false;
                            lastMoveTime = DateTime.UtcNow;
                        }

                        lastCursorPos = cursorPos;
                    }
                    catch
                    {
                        // Sessiz yoksay
                    }
                };

                closeBtnHoverTimer.Start();

                // Form kapanırken timer'ı temizle
                this.FormClosing += (s, e) =>
                {
                    try
                    {
                        closeBtnHoverTimer?.Stop();
                        closeBtnHoverTimer?.Dispose();
                        closeBtnHoverTimer = null;
                    }
                    catch { }
                };
            }
            catch
            {
                // Sessiz yoksay
            }
        }




    }
}