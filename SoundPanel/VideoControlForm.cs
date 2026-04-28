using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using System.Diagnostics;

namespace SoundPanel
{
    public partial class VideoControlForm : Form
    {
        private MediaPlayer mediaPlayer;
        private System.Windows.Forms.Timer timer;
        private bool wasPlayingBeforeDrag = false;
        private bool isDragging = false;
        private VideoPlayerCY parentForm;
        private volatile bool isClosing = false;
        private volatile bool isDisposed = false;
        private readonly object lockObject = new object();

        public VideoControlForm(MediaPlayer mediaPlayer, VideoPlayerCY parentForm)
        {
            InitializeComponent();

            if (mediaPlayer == null)
            {
                MessageBox.Show("Media Player başlatılamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.mediaPlayer = mediaPlayer;
            this.parentForm = parentForm;
            InitializeTimer();
        }

        private async void InitializeTimer()
        {
            try
            {
                // Başlangıç ayarları
                volumeTrackBar.Value = 50;
                lblVolumeValue.Text = "50%";

                // Timer
                timer = new System.Windows.Forms.Timer { Interval = 500 };
                timer.Tick += Timer_Tick;

                this.Enabled = false;
                await Task.Delay(500);

                // SES SEVİYESİNİ VARSAYILAN %50 OLARAK AYARLA
                if (!isClosing && !isDisposed)
                {
                    lock (lockObject)
                    {
                        if (mediaPlayer != null)
                        {
                            try
                            {
                                mediaPlayer.Volume = 50; // Her zaman %50 başla
                                volumeTrackBar.Value = 50; // TrackBar'ı da %50'ye ayarla
                                lblVolumeValue.Text = "50%";
                            }
                            catch { }
                        }
                    }
                }

                this.Enabled = true;
                if (!isClosing && !isDisposed)
                    timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Control form başlatma hatası: " + ex.Message);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isClosing || isDisposed)
            {
                timer?.Stop();
                return;
            }

            lock (lockObject)
            {
                try
                {
                    if (mediaPlayer?.Media != null)
                    {
                        // Güvenli property erişimi
                        long currentTime = 0;
                        long totalTime = 0;
                        float position = 0;

                        try
                        {
                            currentTime = mediaPlayer.Time;
                            totalTime = mediaPlayer.Length;
                            position = mediaPlayer.Position;
                        }
                        catch
                        {
                            // Property erişiminde hata varsa timer'ı durdur
                            timer?.Stop();
                            return;
                        }

                        if (totalTime > 0)
                        {
                            string currentTimeStr = FormatTime(currentTime);
                            string totalTimeStr = FormatTime(totalTime);

                            lblTimeDisplay.Text = $"⏰ {currentTimeStr} / {totalTimeStr}";
                            lblCurrentTime.Text = currentTimeStr;
                            lblTotalTime.Text = totalTimeStr;

                            // Sadece kullanıcı sürüklemiyorsa güncelle
                            if (!isDragging && position >= 0 && position <= 1)
                            {
                                try
                                {
                                    timeTrackBar.Value = (int)(position * 1000);
                                }
                                catch { }
                            }
                        }

                        // Play/Pause buton metnini ve rengini güncelle
                        if (mediaPlayer.IsPlaying)
                        {
                            btnPlayPause.Text = "⏸ Duraklat";
                            btnPlayPause.ButtonBackColor = Color.FromArgb(255, 193, 7); // Sarı
                            btnPlayPause.BorderColor = Color.FromArgb(255, 193, 7);
                            btnPlayPause.HoverColor = Color.FromArgb(255, 210, 40);
                            btnPlayPause.PressedColor = Color.FromArgb(220, 170, 0);
                        }
                        else
                        {
                            btnPlayPause.Text = "▶ Oynat";
                            btnPlayPause.ButtonBackColor = Color.FromArgb(16, 137, 62); // Yeşil
                            btnPlayPause.BorderColor = Color.FromArgb(16, 137, 62);
                            btnPlayPause.HoverColor = Color.FromArgb(30, 150, 80);
                            btnPlayPause.PressedColor = Color.FromArgb(14, 120, 55);
                        }
                    }
                }
                catch (Exception)
                {
                    // Hata durumunda timer'ı durdur
                    timer?.Stop();
                }
            }
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (isClosing || isDisposed) return;

            lock (lockObject)
            {
                if (mediaPlayer == null) return;

                try
                {
                    if (mediaPlayer.IsPlaying)
                        mediaPlayer.Pause();
                    else
                        mediaPlayer.Play();
                }
                catch { }
            }
        }


       
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isClosing || isDisposed) return;

            lock (lockObject)
            {
                if (mediaPlayer == null) return;

                try
                {
                    mediaPlayer.Stop();
                    timeTrackBar.Value = 0;
                    lblTimeDisplay.Text = "⏰ 00:00 / 00:00";
                    lblCurrentTime.Text = "00:00";
                    lblTotalTime.Text = "00:00";
                }
                catch { }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseApplication();
        }

        private void volumeTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (isClosing || isDisposed) return;

            lock (lockObject)
            {
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.Volume = volumeTrackBar.Value;
                        lblVolumeValue.Text = $"{volumeTrackBar.Value}%";
                    }
                    catch { }
                }
            }
        }

        private void volumeTrackBar_ScrollStart(object sender, EventArgs e)
        {
            // Ses trackbar sürüklenmeye başladığında
        }

        private void volumeTrackBar_ScrollEnd(object sender, EventArgs e)
        {
            // Ses trackbar sürükleme bittiğinde
        }

        private void timeTrackBar_ScrollStart(object sender, EventArgs e)
        {
            if (isClosing || isDisposed) return;

            lock (lockObject)
            {
                if (mediaPlayer == null) return;

                isDragging = true;
                try
                {
                    if (mediaPlayer.Media != null && mediaPlayer.IsPlaying)
                    {
                        wasPlayingBeforeDrag = true;
                        mediaPlayer.Pause();
                    }
                    else
                    {
                        wasPlayingBeforeDrag = false;
                    }
                }
                catch { }
            }
        }

        private void timeTrackBar_ScrollEnd(object sender, EventArgs e)
        {
            if (isClosing || isDisposed) return;

            lock (lockObject)
            {
                if (mediaPlayer == null) return;

                try
                {
                    if (mediaPlayer.Media != null)
                    {
                        float position = (float)timeTrackBar.Value / 1000.0f;
                        mediaPlayer.Position = position;
                        isDragging = false;
                        if (wasPlayingBeforeDrag)
                        {
                            mediaPlayer.Play();
                        }
                    }
                }
                catch { }
            }
        }

        private void CloseApplication()
        {
            lock (lockObject)
            {
                if (isClosing || isDisposed) return;
                isClosing = true;
            }

            try
            {
                // Timer'ı hemen durdur
                StopTimer();

                // SESİ TAMAMEN KAPAT VE DURDUR
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.Volume = 0;
                        mediaPlayer.Stop();
                    }
                    catch { }
                }

                // VLC süreçlerini sonlandır
                KillAllVLCProcesses();

                if (parentForm != null && !parentForm.IsDisposed)
                {
                    parentForm.CloseApplication();
                }
                else
                {
                    ForceClose();
                }
            }
            catch (Exception)
            {
                try
                {
                    KillAllVLCProcesses();
                    ForceClose();
                }
                catch { }
            }
        }

        private void StopTimer()
        {
            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
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

        public void RequestClose()
        {
            lock (lockObject)
            {
                if (isClosing || isDisposed) return;
                isClosing = true;
            }

            try
            {
                StopTimer();

                // SESİ TAMAMEN KAPAT VE DURDUR
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.Volume = 0;
                        mediaPlayer.Stop();
                    }
                    catch { }
                }

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
            catch (Exception)
            {
                ForceClose();
            }
        }

        public void ForceClose()
        {
            lock (lockObject)
            {
                if (isDisposed) return;
                isDisposed = true;
            }

            try
            {
                StopTimer();

                // SESİ TAMAMEN KAPAT VE DURDUR
                if (mediaPlayer != null)
                {
                    try
                    {
                        mediaPlayer.Volume = 0;
                        mediaPlayer.Stop();
                    }
                    catch { }
                }

                mediaPlayer = null;
                parentForm = null;

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
            catch
            {
                try
                {
                    Application.Exit();
                }
                catch { }
            }
        }

        private string FormatTime(long timeInMs)
        {
            try
            {
                int totalSeconds = (int)(timeInMs / 1000);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
            catch
            {
                return "00:00";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            lock (lockObject)
            {
                if (isClosing || isDisposed)
                {
                    StopTimer();
                    base.OnFormClosing(e);
                    return;
                }
            }

            // Eğer kullanıcı X tuşına basarak kapatıyorsa
            CloseApplication();
            e.Cancel = true; // Bu formu doğrudan kapatma, önce ana uygulamayı kapat
        }

        private void VideoControlForm_Load(object sender, EventArgs e)
        {
            TopMost = true;
            TopMost = false;
            TopMost = true;
        }

        
    }
}