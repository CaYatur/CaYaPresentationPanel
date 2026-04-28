using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SoundPanel
{
    public partial class VolumeClientForm : Form
    {
        private VolumeClientManager clientManager;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.Timer statsTimer;
        private bool isUpdatingSlider = false;
        private float minVolume = 0f;
        private float maxVolume = 1f;
        private int packetLosses = 0;
        private int totalRequests = 0;
        private Stopwatch connectionTime = new Stopwatch();
        private bool isConnected = false;

        // UI Mode: 0 = Horizontal (Default), 1 = Vertical
        private int volumeMode = 0;

        public VolumeClientForm()
        {
            InitializeComponent();
        }

        private async void VolumeClientForm_Load(object sender, EventArgs e)
        {
            this.BackColor = ColorTranslator.FromHtml("#1e2333");
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(500, 600);

            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 500;
            refreshTimer.Tick += RefreshTimer_Tick;

            statsTimer = new System.Windows.Forms.Timer();
            statsTimer.Interval = 1000;
            statsTimer.Tick += StatsTimer_Tick;

            this.Resize += (s, e) =>
            {
                if (!this.IsDisposed)
                    RepositionControls();
            };
        }

        private void RepositionControls()
        {
            // Dinamik kontrol yerleştirme
            int panelWidth = this.ClientSize.Width - 40;
            pnlConnection.Width = panelWidth;
            panelControls.Width = panelWidth;
            lblStatus.Width = panelWidth;
            lblStats.Width = panelWidth;

        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            string portStr = txtPort.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("IP, Port ve Şifre alanlarını doldurunuz", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(portStr, out int port))
            {
                MessageBox.Show("Port geçerli bir sayı olmalıdır", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnConnect.Enabled = false;
            lblStatus.Text = "⏳ Bağlanıyor...";
            lblStatus.ForeColor = Color.Yellow;

            clientManager = new VolumeClientManager(ip, port, password);
            clientManager.ConnectionStateChanged += ClientManager_ConnectionStateChanged;

            bool connected = await clientManager.ConnectAsync();

            if (connected)
            {
                isConnected = true;
                lblStatus.Text = "✅ Bağlı";
                lblStatus.ForeColor = Color.LimeGreen;
                connectionTime.Restart();
                
                // Sunucudan ses bilgilerini al
                var volumeInfo = await clientManager.GetVolumeInfoAsync();
                if (volumeInfo != null)
                {
                    minVolume = volumeInfo.Minimum;
                    maxVolume = volumeInfo.Maximum;
                    
                    // TrackBar'ı güncelle
                    trackVolume.Minimum = (int)(minVolume * 100);
                    trackVolume.Maximum = (int)(maxVolume * 100);
                    trackVolume.Value = (int)(volumeInfo.Current * 100);
                    
                    UpdateVolumeLabel();
                }
                
                // Kontrol panelini aktive et
                panelControls.Visible = true;
                pnlConnection.Visible = false;

                await RefreshVolume();
                refreshTimer.Start();
                statsTimer.Start();
            }
            else
            {
                isConnected = false;
                lblStatus.Text = "❌ Bağlantı Başarısız";
                lblStatus.ForeColor = Color.Red;
                btnConnect.Enabled = true;
            }
        }

        private void ClientManager_ConnectionStateChanged(object sender, string e)
        {
            this.Invoke(new Action(() =>
            {
                lblLog.Text = e + "\n" + lblLog.Text;
                if (lblLog.Text.Length > 500)
                    lblLog.Text = lblLog.Text.Substring(0, 500);
            }));
        }

        private async void trackVolume_ValueChanged(object sender, EventArgs e)
        {
            if (clientManager?.IsConnected == true && !isUpdatingSlider)
            {
                float normalizedValue = (trackVolume.Value - trackVolume.Minimum) / 
                                       (float)(trackVolume.Maximum - trackVolume.Minimum);
                normalizedValue = Math.Max(minVolume, Math.Min(maxVolume, normalizedValue));
                
                bool success = await clientManager.SetVolumeAsync(normalizedValue);
                totalRequests++;
                if (!success) packetLosses++;
                
                UpdateVolumeLabel();
            }
        }

        private void UpdateVolumeLabel()
        {
            int percentage = (int)((trackVolume.Value - trackVolume.Minimum) / 
                                  (float)(trackVolume.Maximum - trackVolume.Minimum) * 100);
            lblVolumeValue.Text = $"🔊 Ses Düzeyi: %{percentage}";
        }

        private async Task RefreshVolume()
        {
            if (clientManager?.IsConnected == true)
            {
                var volume = await clientManager.GetVolumeAsync();
                if (volume.HasValue)
                {
                    isUpdatingSlider = true;
                    
                    int normalizedValue = (int)((volume.Value - minVolume) / (maxVolume - minVolume) * 100);
                    int trackBarValue = trackVolume.Minimum + (int)((normalizedValue / 100f) * (trackVolume.Maximum - trackVolume.Minimum));
                    
                    trackBarValue = Math.Max(trackVolume.Minimum, Math.Min(trackVolume.Maximum, trackBarValue));
                    trackVolume.Value = trackBarValue;
                    UpdateVolumeLabel();
                    isUpdatingSlider = false;
                }
            }
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshVolume();
        }

        private void StatsTimer_Tick(object sender, EventArgs e)
        {
            if (isConnected && connectionTime.IsRunning)
            {
                string uptime = FormatUptime(connectionTime.Elapsed);
                string lossRate = totalRequests > 0 ? 
                    $"{(packetLosses * 100.0 / totalRequests):F1}%" : "0%";

                lblStats.Text = $"⏱️ Bağlantı Süresi: {uptime} | 📊 Paket Kaybı: {lossRate} | 📈 İstek: {totalRequests}";
            }
        }

        private string FormatUptime(TimeSpan ts)
        {
            if (ts.Hours > 0)
                return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            else if (ts.Minutes > 0)
                return $"{ts.Minutes}m {ts.Seconds}s";
            else
                return $"{ts.Seconds}s";
        }

        private void btnModeSwitch_Click(object sender, EventArgs e)
        {
            volumeMode = (volumeMode + 1) % 2;
            if (volumeMode == 0)
            {
                btnModeSwitch.Text = "📐 Dikey Moda Geç";
                trackVolume.Orientation = Orientation.Horizontal;
                trackVolume.Height = 30;
                trackVolume.Width = 300;
            }
            else
            {
                btnModeSwitch.Text = "⟷ Yatay Moda Geç";
                trackVolume.Orientation = Orientation.Vertical;
                trackVolume.Height = 150;
                trackVolume.Width = 30;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            refreshTimer?.Stop();
            statsTimer?.Stop();
            clientManager?.Disconnect();
            
            isConnected = false;
            lblStatus.Text = "❌ Bağlantısız";
            lblStatus.ForeColor = Color.Red;
            panelControls.Visible = false;
            pnlConnection.Visible = true;

            btnConnect.Enabled = true;
            packetLosses = 0;
            totalRequests = 0;
        }

        private void VolumeClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer?.Dispose();
            statsTimer?.Dispose();
            clientManager?.Disconnect();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VolumeClientForm));
            pnlConnection = new Panel();
            lblIPLabel = new Label();
            txtIP = new TextBox();
            lblPortLabel = new Label();
            txtPort = new TextBox();
            lblPasswordLabel = new Label();
            txtPassword = new TextBox();
            btnConnect = new CustomButton();
            panelControls = new Panel();
            lblVolumeValue = new Label();
            trackVolume = new ModernTrackBar();
            btnModeSwitch = new CustomButton();
            btnDisconnect = new CustomButton();
            lblStatus = new Label();
            lblStats = new Label();
            lblLog = new Label();
            pnlConnection.SuspendLayout();
            panelControls.SuspendLayout();
            SuspendLayout();
            // 
            // pnlConnection
            // 
            pnlConnection.BackColor = Color.FromArgb(39, 45, 66);
            pnlConnection.Controls.Add(lblIPLabel);
            pnlConnection.Controls.Add(txtIP);
            pnlConnection.Controls.Add(lblPortLabel);
            pnlConnection.Controls.Add(txtPort);
            pnlConnection.Controls.Add(lblPasswordLabel);
            pnlConnection.Controls.Add(txtPassword);
            pnlConnection.Controls.Add(btnConnect);
            pnlConnection.Location = new Point(12, 12);
            pnlConnection.Name = "pnlConnection";
            pnlConnection.Size = new Size(460, 250);
            pnlConnection.TabIndex = 0;
            // 
            // lblIPLabel
            // 
            lblIPLabel.AutoSize = true;
            lblIPLabel.Font = new Font("Poppins", 9F, FontStyle.Bold);
            lblIPLabel.ForeColor = Color.White;
            lblIPLabel.Location = new Point(10, 10);
            lblIPLabel.Name = "lblIPLabel";
            lblIPLabel.Size = new Size(73, 22);
            lblIPLabel.TabIndex = 0;
            lblIPLabel.Text = "Sunucu IP:";
            // 
            // txtIP
            // 
            txtIP.Font = new Font("Poppins", 9F);
            txtIP.Location = new Point(10, 35);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(440, 25);
            txtIP.TabIndex = 1;
            // 
            // lblPortLabel
            // 
            lblPortLabel.AutoSize = true;
            lblPortLabel.Font = new Font("Poppins", 9F, FontStyle.Bold);
            lblPortLabel.ForeColor = Color.White;
            lblPortLabel.Location = new Point(10, 67);
            lblPortLabel.Name = "lblPortLabel";
            lblPortLabel.Size = new Size(38, 22);
            lblPortLabel.TabIndex = 2;
            lblPortLabel.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Font = new Font("Poppins", 9F);
            txtPort.Location = new Point(10, 92);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(440, 25);
            txtPort.TabIndex = 3;
            // 
            // lblPasswordLabel
            // 
            lblPasswordLabel.AutoSize = true;
            lblPasswordLabel.Font = new Font("Poppins", 9F, FontStyle.Bold);
            lblPasswordLabel.ForeColor = Color.White;
            lblPasswordLabel.Location = new Point(10, 120);
            lblPasswordLabel.Name = "lblPasswordLabel";
            lblPasswordLabel.Size = new Size(40, 22);
            lblPasswordLabel.TabIndex = 4;
            lblPasswordLabel.Text = "Şifre:";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Poppins", 9F);
            txtPassword.Location = new Point(10, 145);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(440, 25);
            txtPassword.TabIndex = 5;
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.Transparent;
            btnConnect.BorderColor = Color.FromArgb(79, 200, 229);
            btnConnect.BorderThickness = 2;
            btnConnect.ButtonBackColor = Color.FromArgb(79, 200, 229);
            btnConnect.CornerRadius = 4;
            btnConnect.Font = new Font("Poppins", 10F, FontStyle.Bold);
            btnConnect.HoverColor = Color.FromArgb(61, 155, 178);
            btnConnect.ImagePadding = new Padding(0);
            btnConnect.Location = new Point(10, 175);
            btnConnect.Name = "btnConnect";
            btnConnect.PressedColor = Color.FromArgb(43, 110, 126);
            btnConnect.Size = new Size(440, 35);
            btnConnect.TabIndex = 6;
            btnConnect.Text = "Bağlan";
            btnConnect.Click += btnConnect_Click;
            // 
            // panelControls
            // 
            panelControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelControls.BackColor = Color.FromArgb(39, 45, 66);
            panelControls.Controls.Add(lblVolumeValue);
            panelControls.Controls.Add(trackVolume);
            panelControls.Controls.Add(btnModeSwitch);
            panelControls.Controls.Add(btnDisconnect);
            panelControls.Location = new Point(12, 270);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(460, 349);
            panelControls.TabIndex = 1;
            panelControls.Visible = false;
            // 
            // lblVolumeValue
            // 
            lblVolumeValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblVolumeValue.Font = new Font("Poppins", 14F, FontStyle.Bold);
            lblVolumeValue.ForeColor = Color.LimeGreen;
            lblVolumeValue.Location = new Point(10, 10);
            lblVolumeValue.Name = "lblVolumeValue";
            lblVolumeValue.Size = new Size(440, 40);
            lblVolumeValue.TabIndex = 0;
            lblVolumeValue.Text = "🔊 Ses Düzeyi: %50";
            lblVolumeValue.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // trackVolume
            // 
            trackVolume.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackVolume.BackColor = Color.Transparent;
            trackVolume.BackgroundColor = Color.FromArgb(35, 41, 59);
            trackVolume.BorderColor = Color.FromArgb(78, 87, 114);
            trackVolume.BorderThickness = 2;
            trackVolume.CornerRadius = 2;
            trackVolume.Location = new Point(80, 60);
            trackVolume.Maximum = 100;
            trackVolume.Minimum = 0;
            trackVolume.Name = "trackVolume";
            trackVolume.Size = new Size(300, 30);
            trackVolume.TabIndex = 1;
            trackVolume.ThumbBorderColor = Color.White;
            trackVolume.ThumbColor = Color.White;
            trackVolume.TrackBackColor = Color.FromArgb(50, 56, 75);
            trackVolume.TrackProgressColor = Color.FromArgb(255, 204, 123);
            trackVolume.Value = 50;
            trackVolume.ValueChanged += trackVolume_ValueChanged;
            // 
            // btnModeSwitch
            // 
            btnModeSwitch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnModeSwitch.BackColor = Color.Transparent;
            btnModeSwitch.BorderColor = Color.FromArgb(156, 39, 176);
            btnModeSwitch.ButtonBackColor = Color.FromArgb(156, 39, 176);
            btnModeSwitch.CornerRadius = 4;
            btnModeSwitch.Font = new Font("Poppins", 9F, FontStyle.Bold);
            btnModeSwitch.HoverColor = Color.FromArgb(123, 31, 162);
            btnModeSwitch.ImagePadding = new Padding(0);
            btnModeSwitch.Location = new Point(10, 260);
            btnModeSwitch.Name = "btnModeSwitch";
            btnModeSwitch.PressedColor = Color.FromArgb(88, 24, 112);
            btnModeSwitch.Size = new Size(440, 35);
            btnModeSwitch.TabIndex = 2;
            btnModeSwitch.Text = "Dikey Moda Geç";
            btnModeSwitch.Click += btnModeSwitch_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnDisconnect.BackColor = Color.Transparent;
            btnDisconnect.BorderColor = Color.FromArgb(255, 79, 108);
            btnDisconnect.BorderThickness = 2;
            btnDisconnect.ButtonBackColor = Color.FromArgb(255, 79, 108);
            btnDisconnect.CornerRadius = 4;
            btnDisconnect.Font = new Font("Poppins", 10F, FontStyle.Bold);
            btnDisconnect.HoverColor = Color.FromArgb(255, 99, 128);
            btnDisconnect.ImagePadding = new Padding(0);
            btnDisconnect.Location = new Point(10, 300);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.PressedColor = Color.FromArgb(200, 60, 90);
            btnDisconnect.Size = new Size(440, 35);
            btnDisconnect.TabIndex = 3;
            btnDisconnect.Text = "Bağlantıyı Kes";
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Poppins", 11F, FontStyle.Bold);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(12, 622);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(123, 26);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "❌ Bağlantısız";
            // 
            // lblStats
            // 
            lblStats.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStats.Font = new Font("Consolas", 8F);
            lblStats.ForeColor = Color.LimeGreen;
            lblStats.Location = new Point(12, 647);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(460, 50);
            lblStats.TabIndex = 4;
            lblStats.Text = "⏱️ Bağlantı: Yok | 📊 Paket Kaybı: 0% | 📈 İstekler: 0";
            // 
            // lblLog
            // 
            lblLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblLog.BackColor = Color.FromArgb(20, 25, 35);
            lblLog.BorderStyle = BorderStyle.FixedSingle;
            lblLog.Font = new Font("Consolas", 7F);
            lblLog.ForeColor = Color.LimeGreen;
            lblLog.Location = new Point(12, 702);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(460, 100);
            lblLog.TabIndex = 5;
            lblLog.Text = "Durum Günlüğü:\n";
            // 
            // VolumeClientForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(30, 35, 51);
            ClientSize = new Size(500, 812);
            Controls.Add(pnlConnection);
            Controls.Add(panelControls);
            Controls.Add(lblStatus);
            Controls.Add(lblStats);
            Controls.Add(lblLog);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "VolumeClientForm";
            Text = "🔊 Uzaktan Ses Kontrolü";
            FormClosing += VolumeClientForm_FormClosing;
            Load += VolumeClientForm_Load;
            pnlConnection.ResumeLayout(false);
            pnlConnection.PerformLayout();
            panelControls.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private Panel pnlConnection;
        private Label lblIPLabel;
        private TextBox txtIP;
        private Label lblPortLabel;
        private TextBox txtPort;
        private Label lblPasswordLabel;
        private TextBox txtPassword;
        private CustomButton btnConnect;
        private Panel panelControls;
        private Label lblVolumeValue;
        private ModernTrackBar trackVolume;
        private CustomButton btnModeSwitch;
        private CustomButton btnDisconnect;
        private Label lblStatus;
        private Label lblStats;
        private Label lblLog;
    }
}
