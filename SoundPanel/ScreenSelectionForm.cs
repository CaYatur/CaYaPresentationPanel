using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class ScreenSelectionForm : Form
    {
        public Screen SelectedScreen { get; private set; }

        public ScreenSelectionForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Ekran Seçimi";
            Width = 480;
            Height = Math.Min(140 + Screen.AllScreens.Length * 70, 420);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(28, 30, 38);
            Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 16, 16));

            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(35, 37, 45)
            };

            var title = new Label
            {
                Text = "Ekran Seçimi",
                Font = new Font("Segoe UI Variable Display", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var closeBtn = new Label
            {
                Text = "✕",
                AutoSize = false,
                Width = 40,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            closeBtn.Click += (s, e) => Close();
            closeBtn.MouseEnter += (s, e) => closeBtn.ForeColor = Color.Red;
            closeBtn.MouseLeave += (s, e) => closeBtn.ForeColor = Color.LightGray;

            titlePanel.Controls.Add(closeBtn);
            titlePanel.Controls.Add(title);

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Padding = new Padding(15),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            foreach (var screen in Screen.AllScreens)
            {
                bool isPrimary = screen.Equals(Screen.PrimaryScreen);

                var card = new Panel
                {
                    Width = 420,
                    Height = 50,
                    Margin = new Padding(0, 5, 0, 5),
                    BackColor = Color.FromArgb(45, 47, 55),
                    Cursor = Cursors.Hand
                };

                card.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    var font = new Font("Segoe UI Variable Text", 10, FontStyle.Bold);
                    var brush = new SolidBrush(Color.White);

                    e.Graphics.DrawString(GetDisplayInfo(screen), font, brush, 15, 16);
                };

                card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(65, 105, 225);
                card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(45, 47, 55);

                card.Click += (s, e) =>
                {
                    SelectedScreen = screen;
                    DialogResult = DialogResult.OK;
                    Close();
                };

                card.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, card.Width, card.Height, 10, 10));
                panel.Controls.Add(card);
            }

            Controls.Add(panel);
            Controls.Add(titlePanel);
        }

        private string GetDisplayInfo(Screen screen)
        {
            string displayInfo = screen.DeviceName;
            if (screen.Equals(Screen.PrimaryScreen))
                displayInfo += "  (Varsayılan)";
            return displayInfo + $"  [{screen.Bounds.Width}x{screen.Bounds.Height}]";
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }
    }
}
