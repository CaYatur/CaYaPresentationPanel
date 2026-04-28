using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoundPanel
{
    public class BackgroundImageControlForm : Form
    {
        public static BackgroundImageControlForm Instance { get; private set; }

        private Label lblTitle;
        private Label lblStatus;
        private Button btnCloseImage;
        private Button btnMinimize;

        // Abonelik yönetimi: tekrar abone olmamak için son takip edilen instance
        private BackgroundImageForm lastSubscribedImageForm = null;

        // Periyodik UI güncelleme
        private System.Windows.Forms.Timer updateTimer;

        public BackgroundImageControlForm()
        {
            Instance = this;



            // Tema: koyu lacivert / siyah
            this.Text = "Arka Plan Resim Kontrol";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(420, 260);
            this.TopMost = false;
            this.BackColor = Color.FromArgb(12, 18, 30); // koyu lacivertimsi siyah
            this.ForeColor = Color.FromArgb(230, 235, 240);
            this.Font = new Font("Poppins", 9f, FontStyle.Regular);

            // Başlık
            lblTitle = new Label
            {
                Text = "Resim Kontrolü",
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 6, 12, 0),
                Font = new Font("Poppins", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 230, 240),
                BackColor = Color.Transparent
            };

            // Durum metni
            lblStatus = new Label
            {
                Text = "Açık resim yok.",
                AutoSize = false,
                Location = new Point(12, 44),
                Size = new Size(this.ClientSize.Width - 24, 56),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.FromArgb(200, 210, 220),
                BackColor = Color.Transparent,
                Padding = new Padding(6),
                Font = new Font("Poppins", 9f, FontStyle.Regular)
            };

            // Stil yardımcı
            Color btnAccent = Color.FromArgb(26, 115, 196); // mavi aksan
            Color btnAccentHover = Color.FromArgb(36, 135, 216);
            Color btnDanger = Color.FromArgb(220, 60, 70);
            Color btnDangerHover = Color.FromArgb(240, 80, 90);
            Size btnSize = new Size(120, 34);

            // "Resmi Kapat" butonu -- önce resmi kapatır, sonra kontrol penceresini kapatır
            btnCloseImage = new Button
            {
                Text = "Resmi Kapat",
                Size = btnSize,
                BackColor = btnDanger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Poppins", 9f, FontStyle.Bold)
            };
            btnCloseImage.FlatAppearance.BorderSize = 0;
            btnCloseImage.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCloseImage.Location = new Point(this.ClientSize.Width - btnSize.Width - 14, this.ClientSize.Height - btnSize.Height - 12);
            btnCloseImage.MouseEnter += (s, e) => btnCloseImage.BackColor = btnDangerHover;
            btnCloseImage.MouseLeave += (s, e) => btnCloseImage.BackColor = btnDanger;
            btnCloseImage.Click += (s, e) =>
            {
                try
                {
                    if (BackgroundImageForm.Instance != null && !BackgroundImageForm.Instance.IsDisposed)
                    {
                        BackgroundImageForm.Instance.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim kapatılırken hata: " + ex.Message);
                }

                // Kontrol penceresini de kapat
                try
                {
                    if (!this.IsDisposed)
                        this.Close();
                }
                catch { }
            };

            // "Küçült" butonu
            btnMinimize = new Button
            {
                Text = "Küçült",
                Size = new Size(92, 34),
                BackColor = btnAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Poppins", 9f, FontStyle.Bold)
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMinimize.Location = new Point(btnCloseImage.Left - btnMinimize.Width - 10, btnCloseImage.Top);
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = btnAccentHover;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = btnAccent;
            btnMinimize.Click += (s, e) =>
            {
                try { this.WindowState = FormWindowState.Minimized; } catch { }
            };

            // Add controls
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnCloseImage);
            this.Controls.Add(btnMinimize);

            // Yeniden boyutlandırma sırasında buton pozisyonlarını koru
            this.Resize += (s, e) =>
            {
                lblStatus.Size = new Size(this.ClientSize.Width - 24, 56);
                btnCloseImage.Location = new Point(this.ClientSize.Width - btnCloseImage.Width - 14, this.ClientSize.Height - btnCloseImage.Height - 12);
                btnMinimize.Location = new Point(btnCloseImage.Left - btnMinimize.Width - 10, btnCloseImage.Top);
            };

            // Kontrol penceresi kapandığında arkaplan formunu kapat
            this.FormClosed += (s, e) =>
            {
                if (Instance == this) Instance = null;
                UnsubscribeFromImageForm();

                try
                {
                    if (BackgroundImageForm.Instance != null && !BackgroundImageForm.Instance.IsDisposed)
                    {
                        BackgroundImageForm.Instance.Close();
                    }
                }
                catch { }
            };

            // Update timer
            updateTimer = new System.Windows.Forms.Timer { Interval = 500 };
            updateTimer.Tick += (s, e) => UpdateUI();
            updateTimer.Start();

            // İlk güncelleme
            UpdateUI();
        }

        public static void ShowOrFocus(IWin32Window? owner = null)
        {
            if (Instance == null)
            {
                var f = new BackgroundImageControlForm();
                if (owner != null) f.Show(owner);
                else f.Show();
                f.UpdateUI();
            }
            else
            {
                try
                {
                    Instance.WindowState = FormWindowState.Normal;
                    Instance.Focus();
                    Instance.BringToFront();
                    Instance.UpdateUI();
                }
                catch { }
            }
        }

        private void SubscribeToImageFormIfNeeded()
        {
            var img = BackgroundImageForm.Instance;
            if (img == null)
            {
                UnsubscribeFromImageForm();
                return;
            }

            if (!ReferenceEquals(lastSubscribedImageForm, img))
            {
                UnsubscribeFromImageForm();
                try
                {
                    img.FormClosed += ImageForm_FormClosed;
                    lastSubscribedImageForm = img;
                }
                catch { }
            }
        }

        private void UnsubscribeFromImageForm()
        {
            try
            {
                if (lastSubscribedImageForm != null)
                {
                    lastSubscribedImageForm.FormClosed -= ImageForm_FormClosed;
                    lastSubscribedImageForm = null;
                }
            }
            catch { }
        }

        private void ImageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Resim formu kapandığında kontrol penceresini doğrudan kapat
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ImageForm_FormClosed(sender, e)));
                return;
            }

            try
            {
                if (!this.IsDisposed)
                    this.Close();
            }
            catch { }
        }

        public void UpdateUI()
        {
            if (this.IsDisposed) return;

            SubscribeToImageFormIfNeeded();

            string info = "Açık resim bulunmuyor.";
            if (BackgroundImageForm.Instance != null)
            {
                if (BackgroundImageForm.Instance.Tag is string path && !string.IsNullOrEmpty(path))
                {
                    info = "Açık resim:\n" + Path.GetFileName(path) + "\n\nTam yol:\n" + path;
                }
                else
                {
                    info = "Bir resim penceresi açık (yol bilinmiyor).";
                }
            }

            lblStatus.Text = info;
            btnCloseImage.Enabled = BackgroundImageForm.Instance != null;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackgroundImageControlForm));
            SuspendLayout();
            // 
            // BackgroundImageControlForm
            // 
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "BackgroundImageControlForm";
            ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    updateTimer?.Stop();
                    updateTimer?.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}