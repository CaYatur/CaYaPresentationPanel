using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundPanel
{
    public class ExtractionForm : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnCancel;
        private CancellationTokenSource cts;

        public ExtractionForm()
        {
            InitializeComponents();
            Shown += ExtractionForm_Shown;
        }

        private void InitializeComponents()
        {
            Width = 480;
            Height = 140;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dosyalar Ayýklanýyor...";
            MaximizeBox = false;
            MinimizeBox = false;

            lblStatus = new Label()
            {
                Text = "Hazýrlanýyor...",
                AutoSize = false,
                Width = ClientSize.Width - 20,
                Height = 30,
                Top = 10,
                Left = 10
            };

            progressBar = new ProgressBar()
            {
                Style = ProgressBarStyle.Continuous,
                Width = ClientSize.Width - 20,
                Height = 20,
                Top = lblStatus.Bottom + 6,
                Left = 10
            };

            btnCancel = new Button()
            {
                Text = "Ýptal",
                Width = 80,
                Height = 26,
                Top = progressBar.Bottom + 8,
                Left = ClientSize.Width - 90
            };
            btnCancel.Click += (s, e) => { btnCancel.Enabled = false; cts?.Cancel(); };

            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnCancel);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractionForm));
            SuspendLayout();
            // 
            // ExtractionForm
            // 
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ExtractionForm";
            ResumeLayout(false);

        }

        private async void ExtractionForm_Shown(object? sender, EventArgs e)
        {
            await Task.Yield();

            cts = new CancellationTokenSource();
            var progress = new Progress<int>(p =>
            {
                if (p <= 0) progressBar.Style = ProgressBarStyle.Marquee; else progressBar.Style = ProgressBarStyle.Continuous;
                try { progressBar.Value = Math.Max(0, Math.Min(100, p)); } catch { }
            });

            string error = null;
            bool ok = await Task.Run(() => Program.ExtractEmbeddedLibVlcWithProgress(progress, s => { if (!string.IsNullOrEmpty(s)) lblStatus.Text = s; }, cts.Token, out error));

            if (!ok)
            {
                lblStatus.Text = string.IsNullOrEmpty(error) ? "Ayýklama baþarýsýz." : error;
                MessageBox.Show("LibVLC ayýklama baþarýsýz: " + lblStatus.Text, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Let user decide - continue to app so they can try again or cancel
                await Task.Delay(800);
                this.Close();
                return;
            }

            lblStatus.Text = "Ayýklama tamamlandý.";
            await Task.Delay(400);
            this.Close();
        }
    }
}
