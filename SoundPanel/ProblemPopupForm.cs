using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class ProblemPopupForm : Form
    {
        private Action onAutoFix;

        public ProblemPopupForm(SystemCheckItem problem, string solutionSuggestion, Action onAutoFix = null)
        {
            InitializeComponent();
            this.onAutoFix = onAutoFix;
            this.Text = "Kritik Sistem Sorunu";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(470, 330);
            this.BackColor = Color.FromArgb(36, 40, 56);
            this.StartPosition = FormStartPosition.CenterScreen;

            var icon = new PictureBox
            {
                Left = 18,
                Top = 18,
                Width = 42,
                Height = 42,
                Image = problem.State == CheckState.Error
                    ? SystemCheck.Properties.Resources.close_circle
                    : (problem.State == CheckState.Warning
                        ? SystemCheck.Properties.Resources.warning
                        : SystemCheck.Properties.Resources.info_circle),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };
            this.Controls.Add(icon);

            var lblTitle = new Label
            {
                Left = 72,
                Top = 18,
                Width = 370,
                Height = 32,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Text = problem.Title + " sorunu"
            };
            this.Controls.Add(lblTitle);

            var txtDescription = new Label
            {
                Left = 18,
                Top = 68,
                Width = this.ClientSize.Width - 36,
                Height = 60,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 220),
                Text = problem.Description,
                AutoSize = false
            };
            this.Controls.Add(txtDescription);

            var txtSolution = new Label
            {
                Left = 18,
                Top = 138,
                Width = this.ClientSize.Width - 36,
                Height = 80,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(130, 180, 255),
                Text = string.IsNullOrWhiteSpace(solutionSuggestion)
                    ? "Manuel işlem yapmanız gerekebilir."
                    : $"Otomatik çözüm önerisi:\n{solutionSuggestion}"
            };
            this.Controls.Add(txtSolution);

            if (!string.IsNullOrWhiteSpace(solutionSuggestion))
            {
                var btnFix = new Button
                {
                    Text = "Otomatik Çöz",
                    Width = 140,
                    Height = 38,
                    Top = this.ClientSize.Height - 56,
                    Left = this.ClientSize.Width / 2 - 150,
                    BackColor = Color.FromArgb(60, 180, 90),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                btnFix.FlatAppearance.BorderSize = 0;
                btnFix.Click += (s, e) =>
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    onAutoFix?.Invoke();
                };
                this.Controls.Add(btnFix);

                var btnClose = new Button
                {
                    Text = "Kapat",
                    Width = 120,
                    Height = 38,
                    Top = this.ClientSize.Height - 56,
                    Left = this.ClientSize.Width / 2 + 20,
                    BackColor = Color.FromArgb(70, 80, 110),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => this.Close();
                this.Controls.Add(btnClose);
            }
            else
            {
                var btnClose = new Button
                {
                    Text = "Kapat",
                    Width = 160,
                    Height = 38,
                    Top = this.ClientSize.Height - 56,
                    Left = this.ClientSize.Width / 2 - 80,
                    BackColor = Color.FromArgb(70, 80, 110),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => this.Close();
                this.Controls.Add(btnClose);
            }
        }
    }
}