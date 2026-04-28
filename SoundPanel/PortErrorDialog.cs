using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundPanel
{
    public class PortErrorDialog : Form
    {
        public enum PortDialogResult { RetryAuto, EnterManually, Cancel }
        public PortDialogResult Result { get; private set; } = PortDialogResult.Cancel;
        public int EnteredPort { get; private set; } = 0;

        private TextBox txtPort;
        private Button btnRetryAuto, btnEnterManually, btnCancel, btnOk;
        private Label lblMessage;

        public PortErrorDialog(string message, int lastTriedPort)
        {
            this.Text = "Web Sunucu Port Hatasý";
            this.Width = 420;
            this.Height = 220;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(32, 34, 44);
            this.ForeColor = Color.White;

            lblMessage = new Label
            {
                Text = message,
                AutoSize = false,
                Width = 370,
                Height = 60,
                Top = 18,
                Left = 20,
                ForeColor = Color.White
            };
            this.Controls.Add(lblMessage);

            btnRetryAuto = new Button
            {
                Text = "Otomatik Port Dene",
                Width = 140,
                Height = 36,
                Top = 90,
                Left = 20
            };
            btnRetryAuto.Click += (s, e) => { Result = PortDialogResult.RetryAuto; this.DialogResult = DialogResult.OK; };
            this.Controls.Add(btnRetryAuto);

            btnEnterManually = new Button
            {
                Text = "Port Gir",
                Width = 90,
                Height = 36,
                Top = 90,
                Left = 180
            };
            this.Controls.Add(btnEnterManually);

            txtPort = new TextBox
            {
                Text = lastTriedPort.ToString(),
                Width = 80,
                Height = 28,
                Top = 90,
                Left = 280,
                Enabled = false
            };
            txtPort.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && txtPort.Enabled)
                {
                    TryConfirmPort();
                }
            };
            this.Controls.Add(txtPort);

            btnOk = new Button
            {
                Text = "Onayla",
                Width = txtPort.Width,
                Height = 28,
                Top = txtPort.Top + txtPort.Height + 8,
                Left = txtPort.Left,
                Enabled = false
            };
            btnOk.Click += (s, e) =>
            {
                TryConfirmPort();
            };
            this.Controls.Add(btnOk);

            btnEnterManually.Click += (s, e) =>
            {
                txtPort.Enabled = true;
                txtPort.Focus();
                btnOk.Enabled = true;
            };

            btnCancel = new Button
            {
                Text = "Çýkýþ",
                Width = 80,
                Height = 36,
                Top = 140,
                Left = 20
            };
            btnCancel.Click += (s, e) => { Result = PortDialogResult.Cancel; this.DialogResult = DialogResult.Cancel; };
            this.Controls.Add(btnCancel);
        }

        private void TryConfirmPort()
        {
            if (int.TryParse(txtPort.Text, out int port) && port > 0 && port < 65536)
            {
                EnteredPort = port;
                Result = PortDialogResult.EnterManually;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Geçerli bir port girin (1-65535)", "Hatalý Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponent()
        {

        }
    }
}
