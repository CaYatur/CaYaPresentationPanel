using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class BackgroundImageForm : Form
    {
        public static BackgroundImageForm Instance { get; private set; }
        public static Screen SelectedScreen { get; set; }

        private PictureBox pictureBox;

        public BackgroundImageForm(string imagePath)
        {
            InitializeComponent();
            InitializeCloseBtnHover();
            Instance = this; // Tekil örneği sakla
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;

            // PictureBox'ı Zoom modunda oluşturuyoruz.
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(pictureBox);
            UpdateImage(imagePath);
        }

        public void UpdateImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                pictureBox.ImageLocation = imagePath;
            }
            else
            {
                MessageBox.Show("Resim dosyası bulunamadı!");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Instance = null;
            SelectedScreen = null;
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

                // Lokal değişkenlerle fare hareketi / durağanlık takibi (alan içine girip 10s hareketsiz => gizle)
                var lastCursorPos = Cursor.Position;
                var lastMoveTime = DateTime.UtcNow;
                var inactivityThreshold = TimeSpan.FromSeconds(10);
                var hiddenByInactivity = false;

                closeBtnHoverTimer = new System.Windows.Forms.Timer
                {
                    Interval = 100 // daha duyarlı tepki için 100ms
                };

                closeBtnHoverTimer.Tick += (s, e) =>
                {
                    try
                    {

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

        private void CloseBtnW_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}
