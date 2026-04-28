using AxWMPLib;
using System;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class VideoPCYBGS : Form
    {
        private AxWindowsMediaPlayer wmPlayer;
        private readonly string videoPath;
        public static VideoPCYBGS Instance { get; private set; }

        public VideoPCYBGS(string videoPath)
        {
            // Sadece gerekli property atamaları (InitializeComponent en sona bırakılmalı)
            this.videoPath = videoPath;
            Instance = this;
            this.Opacity = 0; // Form açılırken hemen görünmesin
            InitializeComponent();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Instance = null;
            if (wmPlayer != null)
            {
                wmPlayer.Dispose();
                wmPlayer = null;
            }
        }

        private void PreloadAndShowContent()
        {
            if (string.IsNullOrEmpty(videoPath) || !System.IO.File.Exists(videoPath))
            {
                MessageBox.Show("Geçersiz video dosyası yolu.");
                Close();
                return;
            }

            // Player'ı doğrudan ekle ve yükle
            wmPlayer = new AxWindowsMediaPlayer
            {
                Name = "wmPlayer",
                Dock = DockStyle.Fill,
                Enabled = true
            };

            ((System.ComponentModel.ISupportInitialize)(wmPlayer)).BeginInit();
            Controls.Add(wmPlayer);
            ((System.ComponentModel.ISupportInitialize)(wmPlayer)).EndInit();

            // En hızlı başlatma için property'ler
            wmPlayer.uiMode = "none";
            wmPlayer.stretchToFit = true;
            wmPlayer.settings.volume = 0;
            wmPlayer.settings.setMode("loop", true);

            // Olayı başta bağla
            wmPlayer.PlayStateChange += WmPlayer_PlayStateChange;

            // Formu tam ekran ve siyah yap
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = System.Drawing.Color.Black;

            // Video'yu yükle ve oynat
            wmPlayer.URL = videoPath;
            wmPlayer.Ctlcontrols.play();

            // Player gerçekten oynatmaya başladığında göster
            wmPlayer.PlayStateChange += (s, e) =>
            {
                if (e.newState == (int)WMPLib.WMPPlayState.wmppsPlaying)
                {
                    // Sadece ilk oynatma anında görünür yap
                    if (Opacity == 0) Opacity = 1;
                }
            };
        }

        private void WmPlayer_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == (int)WMPLib.WMPPlayState.wmppsStopped)
            {
                Close();
            }
        }

        private void VideoPCYBGS_Load(object sender, EventArgs e)
        {
            // Video yükleme işlemini ctor'dan hemen başlat
            BeginInvoke(new Action(PreloadAndShowContent));
        }
    }
}