using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SoundPanel
{
    public partial class VolumeServerForm : Form
    {
        private VolumeServerManager serverManager;
        private Form1 mainForm;
        private System.ComponentModel.IContainer components = null;
        private int clientCount = 0;
        private int commandCount = 0;
        private Stopwatch serverUptime = new Stopwatch();

        public VolumeServerForm(Form1 form)
        {
            InitializeComponent();
            mainForm = form;
            
            mainForm.FormClosing += (s, e) =>
            {
                if (!this.IsDisposed)
                {
                    this.Close();
                }
            };
        }

        private async void VolumeServerForm_Load(object sender, EventArgs e)
        {
            this.BackColor = ColorTranslator.FromHtml("#1e2333");
            this.ForeColor = Color.White;

            serverManager = new VolumeServerManager(
                volumeValue => UpdateVolumeOnServer(volumeValue),
                () => GetCurrentVolume()
            );

            serverManager.ServerStateChanged += ServerManager_ServerStateChanged;

            Log("🚀 Sunucu başlatılıyor...");
            bool started = await serverManager.StartServerAsync();
            
            if (started)
            {
                serverUptime.Start();
                lblServerInfo.Text = $"📍 IP: {serverManager.ServerIP}\n🔌 Port: {serverManager.ServerPort}\n🔐 Şifre: {serverManager.ServerPassword}";
                lblServerInfo.ForeColor = Color.LimeGreen;
                lblStatus.Text = "✅ Sunucu ÇALIŞIYOR";
                lblStatus.ForeColor = Color.LimeGreen;
                Log($"✅ Sunucu başarıyla başlatıldı: {serverManager.ServerIP}:{serverManager.ServerPort}");
            }
            else
            {
                lblStatus.Text = "❌ Sunucu başlatılamadı";
                lblStatus.ForeColor = Color.Red;
                Log("❌ Sunucu başlatma başarısız!");
            }

            // Uptime göstergesi
            System.Windows.Forms.Timer uptimeTimer = new System.Windows.Forms.Timer();
            uptimeTimer.Interval = 1000;
            uptimeTimer.Tick += (s, args) =>
            {
                if (serverUptime.IsRunning)
                {
                    string uptime = FormatUptime(serverUptime.Elapsed);
                    lblServerStats.Text = $"⏱️ Çalışma Süresi: {uptime} | 👥 İstemciler: {clientCount} | 📊 Komutlar: {commandCount}";
                }
            };
            uptimeTimer.Start();
        }

        private void ServerManager_ServerStateChanged(object sender, string e)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke(new Action(() =>
                {
                    if (!this.IsDisposed)
                    {
                        Log(e);
                    }
                }));
            }
        }

        private void UpdateVolumeOnServer(float volume)
        {
            if (mainForm != null && mainForm.IsHandleCreated && !mainForm.IsDisposed)
            {
                mainForm.Invoke(new Action(() =>
                {
                    try
                    {
                        mainForm.SetVolumeFromServer((int)(volume * 100));
                    }
                    catch { }
                }));
            }
        }

        private float GetCurrentVolume()
        {
            float result = 0.5f;
            if (mainForm != null && mainForm.IsHandleCreated && !mainForm.IsDisposed)
            {
                mainForm.Invoke(new Action(() =>
                {
                    try
                    {
                        result = mainForm.GetCurrentVolume();
                    }
                    catch { }
                }));
            }
            return result;
        }

        private void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}";

            if (message.Contains("bağlandı"))
                clientCount++;
            else if (message.Contains("kesildi") || message.Contains("koptu"))
                clientCount = Math.Max(0, clientCount - 1);

            if (message.Contains("SET_VOLUME") || message.Contains("GET_VOLUME") || message.Contains("PING"))
                commandCount++;

            lblLog.Text = logEntry + "\n" + lblLog.Text;
            
            string[] lines = lblLog.Text.Split(new[] { "\n" }, StringSplitOptions.None);
            if (lines.Length > 20)
            {
                lblLog.Text = string.Join("\n", lines, 0, 20);
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

        private void VolumeServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                serverManager?.StopServer();
                if (components != null)
                {
                    components.Dispose();
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VolumeServerForm));
            lblServerInfo = new Label();
            lblStatus = new Label();
            lblServerStats = new Label();
            lblLog = new Label();
            SuspendLayout();
            // 
            // lblServerInfo
            // 
            lblServerInfo.BackColor = Color.FromArgb(39, 45, 66);
            lblServerInfo.BorderStyle = BorderStyle.FixedSingle;
            lblServerInfo.Font = new Font("Poppins", 11F, FontStyle.Bold);
            lblServerInfo.ForeColor = Color.White;
            lblServerInfo.Location = new Point(12, 12);
            lblServerInfo.Name = "lblServerInfo";
            lblServerInfo.Size = new Size(500, 100);
            lblServerInfo.TabIndex = 0;
            lblServerInfo.Text = "Sunucu bilgileri yükleniyor...";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Poppins", 12F, FontStyle.Bold);
            lblStatus.ForeColor = Color.White;
            lblStatus.Location = new Point(12, 120);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(215, 28);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "⏳ Durum: Başlatılıyor...";
            // 
            // lblServerStats
            // 
            lblServerStats.Font = new Font("Consolas", 9F);
            lblServerStats.ForeColor = Color.LimeGreen;
            lblServerStats.Location = new Point(12, 145);
            lblServerStats.Name = "lblServerStats";
            lblServerStats.Size = new Size(500, 50);
            lblServerStats.TabIndex = 2;
            lblServerStats.Text = "⏱️ Çalışma Süresi: Hesaplanıyor...";
            // 
            // lblLog
            // 
            lblLog.BackColor = Color.FromArgb(26, 31, 46);
            lblLog.BorderStyle = BorderStyle.FixedSingle;
            lblLog.Font = new Font("Consolas", 8F);
            lblLog.ForeColor = Color.LimeGreen;
            lblLog.Location = new Point(12, 205);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(500, 350);
            lblLog.TabIndex = 3;
            lblLog.Text = "🔍 İşlem Günlüğü:\n";
            // 
            // VolumeServerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 35, 51);
            ClientSize = new Size(524, 565);
            Controls.Add(lblServerInfo);
            Controls.Add(lblStatus);
            Controls.Add(lblServerStats);
            Controls.Add(lblLog);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimumSize = new Size(524, 500);
            Name = "VolumeServerForm";
            Text = "🔊 Uzaktan Ses Sunucusu";
            FormClosing += VolumeServerForm_FormClosing;
            Load += VolumeServerForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Label lblServerInfo;
        private Label lblStatus;
        private Label lblServerStats;
        private Label lblLog;
    }
}
