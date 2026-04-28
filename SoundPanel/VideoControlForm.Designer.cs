namespace SoundPanel
{
    partial class VideoControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoControlForm));
            mainPanel = new Panel();
            volumePanel = new Panel();
            lblVolumeValue = new Label();
            lblVolumeTitle = new Label();
            volumeTrackBar = new ModernTrackBar();
            controlButtonsPanel = new Panel();
            btnClose = new CustomButton();
            btnStop = new CustomButton();
            btnPlayPause = new CustomButton();
            timePanel = new Panel();
            timeTrackBar = new ModernTrackBar();
            lblCurrentTime = new Label();
            lblTotalTime = new Label();
            lblTimeDisplay = new Label();
            titlePanel = new Panel();
            lblTitle = new Label();
            mainPanel.SuspendLayout();
            volumePanel.SuspendLayout();
            controlButtonsPanel.SuspendLayout();
            timePanel.SuspendLayout();
            titlePanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.FromArgb(45, 45, 48);
            mainPanel.Controls.Add(volumePanel);
            mainPanel.Controls.Add(controlButtonsPanel);
            mainPanel.Controls.Add(timePanel);
            mainPanel.Controls.Add(titlePanel);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(15);
            mainPanel.Size = new Size(520, 374);
            mainPanel.TabIndex = 0;
            // 
            // volumePanel
            // 
            volumePanel.BackColor = Color.FromArgb(37, 37, 38);
            volumePanel.Controls.Add(lblVolumeValue);
            volumePanel.Controls.Add(lblVolumeTitle);
            volumePanel.Controls.Add(volumeTrackBar);
            volumePanel.Dock = DockStyle.Bottom;
            volumePanel.Location = new Point(15, 219);
            volumePanel.Name = "volumePanel";
            volumePanel.Padding = new Padding(15);
            volumePanel.Size = new Size(490, 80);
            volumePanel.TabIndex = 3;
            // 
            // lblVolumeValue
            // 
            lblVolumeValue.BackColor = Color.FromArgb(0, 122, 204);
            lblVolumeValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblVolumeValue.ForeColor = Color.White;
            lblVolumeValue.Location = new Point(410, 15);
            lblVolumeValue.Name = "lblVolumeValue";
            lblVolumeValue.Size = new Size(65, 28);
            lblVolumeValue.TabIndex = 2;
            lblVolumeValue.Text = "50%";
            lblVolumeValue.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblVolumeTitle
            // 
            lblVolumeTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblVolumeTitle.ForeColor = Color.White;
            lblVolumeTitle.Location = new Point(15, 15);
            lblVolumeTitle.Name = "lblVolumeTitle";
            lblVolumeTitle.Size = new Size(140, 28);
            lblVolumeTitle.TabIndex = 1;
            lblVolumeTitle.Text = "🔊 Ses Seviyesi";
            lblVolumeTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // volumeTrackBar
            // 
            volumeTrackBar.BackColor = Color.Transparent;
            volumeTrackBar.BackgroundColor = Color.FromArgb(37, 37, 38);
            volumeTrackBar.BorderColor = Color.FromArgb(0, 122, 204);
            volumeTrackBar.BorderThickness = 0;
            volumeTrackBar.CornerRadius = 8;
            volumeTrackBar.Location = new Point(15, 50);
            volumeTrackBar.Maximum = 100;
            volumeTrackBar.Minimum = 0;
            volumeTrackBar.Name = "volumeTrackBar";
            volumeTrackBar.Size = new Size(460, 20);
            volumeTrackBar.TabIndex = 0;
            volumeTrackBar.ThumbBorderColor = Color.FromArgb(0, 122, 204);
            volumeTrackBar.ThumbColor = Color.White;
            volumeTrackBar.ThumbSizeRatio = 1.8F;
            volumeTrackBar.TrackBackColor = Color.FromArgb(60, 60, 60);
            volumeTrackBar.TrackProgressColor = Color.FromArgb(0, 174, 255);
            volumeTrackBar.TrackThicknessRatio = 0.4F;
            volumeTrackBar.Value = 50;
            volumeTrackBar.ValueChanged += volumeTrackBar_ValueChanged;
            volumeTrackBar.ScrollStart += volumeTrackBar_ScrollStart;
            volumeTrackBar.ScrollEnd += volumeTrackBar_ScrollEnd;
            // 
            // controlButtonsPanel
            // 
            controlButtonsPanel.BackColor = Color.FromArgb(37, 37, 38);
            controlButtonsPanel.Controls.Add(btnClose);
            controlButtonsPanel.Controls.Add(btnStop);
            controlButtonsPanel.Controls.Add(btnPlayPause);
            controlButtonsPanel.Dock = DockStyle.Bottom;
            controlButtonsPanel.Location = new Point(15, 299);
            controlButtonsPanel.Name = "controlButtonsPanel";
            controlButtonsPanel.Padding = new Padding(15);
            controlButtonsPanel.Size = new Size(490, 60);
            controlButtonsPanel.TabIndex = 2;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.Transparent;
            btnClose.BorderColor = Color.FromArgb(232, 17, 35);
            btnClose.BorderThickness = 2;
            btnClose.ButtonBackColor = Color.FromArgb(232, 17, 35);
            btnClose.CornerRadius = 8;
            btnClose.DisabledBackColor = Color.FromArgb(229, 229, 229);
            btnClose.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnClose.HoverColor = Color.FromArgb(250, 50, 65);
            btnClose.ImagePadding = new Padding(0);
            btnClose.Location = new Point(340, 15);
            btnClose.Name = "btnClose";
            btnClose.PressedColor = Color.FromArgb(200, 15, 30);
            btnClose.Size = new Size(135, 30);
            btnClose.TabIndex = 2;
            btnClose.Text = "✕ Videoyu Kapat";
            btnClose.Click += btnClose_Click;
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.Transparent;
            btnStop.BorderColor = Color.FromArgb(255, 140, 0);
            btnStop.BorderThickness = 2;
            btnStop.ButtonBackColor = Color.FromArgb(255, 140, 0);
            btnStop.CornerRadius = 8;
            btnStop.DisabledBackColor = Color.FromArgb(229, 229, 229);
            btnStop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStop.HoverColor = Color.FromArgb(255, 160, 30);
            btnStop.ImagePadding = new Padding(0);
            btnStop.Location = new Point(180, 15);
            btnStop.Name = "btnStop";
            btnStop.PressedColor = Color.FromArgb(220, 120, 0);
            btnStop.Size = new Size(120, 30);
            btnStop.TabIndex = 1;
            btnStop.Text = "⏹ Durdur";
            btnStop.Click += btnStop_Click;
            // 
            // btnPlayPause
            // 
            btnPlayPause.BackColor = Color.Transparent;
            btnPlayPause.BorderColor = Color.FromArgb(16, 137, 62);
            btnPlayPause.BorderThickness = 2;
            btnPlayPause.ButtonBackColor = Color.FromArgb(16, 137, 62);
            btnPlayPause.CornerRadius = 8;
            btnPlayPause.DisabledBackColor = Color.FromArgb(229, 229, 229);
            btnPlayPause.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnPlayPause.HoverColor = Color.FromArgb(30, 150, 80);
            btnPlayPause.ImagePadding = new Padding(0);
            btnPlayPause.Location = new Point(15, 15);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.PressedColor = Color.FromArgb(14, 120, 55);
            btnPlayPause.Size = new Size(120, 30);
            btnPlayPause.TabIndex = 0;
            btnPlayPause.Text = "⏸ Duraklat";
            btnPlayPause.Click += btnPlayPause_Click;
            // 
            // timePanel
            // 
            timePanel.BackColor = Color.FromArgb(37, 37, 38);
            timePanel.Controls.Add(timeTrackBar);
            timePanel.Controls.Add(lblCurrentTime);
            timePanel.Controls.Add(lblTotalTime);
            timePanel.Controls.Add(lblTimeDisplay);
            timePanel.Dock = DockStyle.Fill;
            timePanel.Location = new Point(15, 86);
            timePanel.Name = "timePanel";
            timePanel.Padding = new Padding(15);
            timePanel.Size = new Size(490, 273);
            timePanel.TabIndex = 1;
            // 
            // timeTrackBar
            // 
            timeTrackBar.BackColor = Color.Transparent;
            timeTrackBar.BackgroundColor = Color.FromArgb(37, 37, 38);
            timeTrackBar.BorderColor = Color.FromArgb(0, 122, 204);
            timeTrackBar.BorderThickness = 0;
            timeTrackBar.CornerRadius = 8;
            timeTrackBar.Location = new Point(15, 75);
            timeTrackBar.Maximum = 1000;
            timeTrackBar.Minimum = 0;
            timeTrackBar.Name = "timeTrackBar";
            timeTrackBar.Size = new Size(460, 25);
            timeTrackBar.TabIndex = 3;
            timeTrackBar.ThumbBorderColor = Color.FromArgb(0, 174, 255);
            timeTrackBar.ThumbColor = Color.White;
            timeTrackBar.ThumbSizeRatio = 1.6F;
            timeTrackBar.TrackBackColor = Color.FromArgb(60, 60, 60);
            timeTrackBar.TrackProgressColor = Color.FromArgb(0, 174, 255);
            timeTrackBar.TrackThicknessRatio = 0.5F;
            timeTrackBar.Value = 0;
            timeTrackBar.ScrollStart += timeTrackBar_ScrollStart;
            timeTrackBar.ScrollEnd += timeTrackBar_ScrollEnd;
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCurrentTime.ForeColor = Color.FromArgb(0, 174, 255);
            lblCurrentTime.Location = new Point(15, 45);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Size = new Size(80, 25);
            lblCurrentTime.TabIndex = 2;
            lblCurrentTime.Text = "00:00";
            lblCurrentTime.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotalTime
            // 
            lblTotalTime.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalTime.ForeColor = Color.Silver;
            lblTotalTime.Location = new Point(395, 45);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Size = new Size(80, 25);
            lblTotalTime.TabIndex = 1;
            lblTotalTime.Text = "00:00";
            lblTotalTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTimeDisplay
            // 
            lblTimeDisplay.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTimeDisplay.ForeColor = Color.White;
            lblTimeDisplay.Location = new Point(15, 10);
            lblTimeDisplay.Name = "lblTimeDisplay";
            lblTimeDisplay.Size = new Size(460, 30);
            lblTimeDisplay.TabIndex = 0;
            lblTimeDisplay.Text = "⏰ 00:00 / 00:00";
            lblTimeDisplay.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // titlePanel
            // 
            titlePanel.BackColor = Color.FromArgb(30, 30, 32);
            titlePanel.Controls.Add(lblTitle);
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Location = new Point(15, 15);
            titlePanel.Name = "titlePanel";
            titlePanel.Padding = new Padding(15);
            titlePanel.Size = new Size(490, 71);
            titlePanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 174, 255);
            lblTitle.Location = new Point(15, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(460, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🎬 Video Oynatıcı Kontrol Paneli";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // VideoControlForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 48);
            ClientSize = new Size(520, 374);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "VideoControlForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🎬 Video Oynatıcı Kontrolleri";
            TopMost = true;
            Load += VideoControlForm_Load;
            mainPanel.ResumeLayout(false);
            volumePanel.ResumeLayout(false);
            controlButtonsPanel.ResumeLayout(false);
            timePanel.ResumeLayout(false);
            titlePanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel mainPanel;
        private Panel titlePanel;
        private Label lblTitle;
        private Panel timePanel;
        private Label lblTimeDisplay;
        private Label lblCurrentTime;
        private Label lblTotalTime;
        private ModernTrackBar timeTrackBar;
        private Panel controlButtonsPanel;
        private CustomButton btnPlayPause;
        private CustomButton btnStop;
        private CustomButton btnClose;
        private Panel volumePanel;
        private ModernTrackBar volumeTrackBar;
        private Label lblVolumeTitle;
        private Label lblVolumeValue;
    }
}