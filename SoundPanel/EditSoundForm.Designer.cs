using System.Windows.Forms;

namespace SoundPanel
{
    partial class EditSoundForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel topPanel;
        private Panel waveformPanel;
        private Panel controlPanel;
        private Panel timelinePanel;

        private CustomButton openButton;
        private CustomButton playButton;
        private CustomButton stopButton;
        private CustomButton saveButton;
        private CustomButton trimButton;
        private CustomButton mergeButton;

        private ModernTrackBar positionTrackBar;
        private ModernTrackBar trimStartTrackBar;
        private ModernTrackBar trimEndTrackBar;

        private Label currentTimeLabel;
        private Label totalTimeLabel;
        private Label trimStartLabel;
        private Label trimEndLabel;
        private Label trimDurationLabel;
        private Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditSoundForm));
            topPanel = new Panel();
            openButton = new CustomButton();
            playButton = new CustomButton();
            stopButton = new CustomButton();
            saveButton = new CustomButton();
            trimButton = new CustomButton();
            mergeButton = new CustomButton();
            waveformPanel = new Panel();
            controlPanel = new Panel();
            trimStartTrackBar = new ModernTrackBar();
            trimEndTrackBar = new ModernTrackBar();
            trimStartLabel = new Label();
            trimEndLabel = new Label();
            trimDurationLabel = new Label();
            statusLabel = new Label();
            timelinePanel = new Panel();
            positionTrackBar = new ModernTrackBar();
            currentTimeLabel = new Label();
            totalTimeLabel = new Label();
            topPanel.SuspendLayout();
            controlPanel.SuspendLayout();
            timelinePanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.Controls.Add(openButton);
            topPanel.Controls.Add(playButton);
            topPanel.Controls.Add(stopButton);
            topPanel.Controls.Add(saveButton);
            topPanel.Controls.Add(trimButton);
            topPanel.Controls.Add(mergeButton);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(10);
            topPanel.Size = new Size(1184, 80);
            topPanel.TabIndex = 3;
            // 
            // openButton
            // 
            openButton.BackColor = Color.Transparent;
            openButton.CornerRadius = 14;
            openButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            openButton.ImagePadding = new Padding(0);
            openButton.Location = new Point(10, 20);
            openButton.Name = "openButton";
            openButton.Size = new Size(80, 40);
            openButton.TabIndex = 0;
            openButton.Text = "Aç";
            openButton.Click += openButton_Click;
            // 
            // playButton
            // 
            playButton.BackColor = Color.Transparent;
            playButton.CornerRadius = 14;
            playButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            playButton.ImagePadding = new Padding(0);
            playButton.Location = new Point(100, 20);
            playButton.Name = "playButton";
            playButton.Size = new Size(60, 40);
            playButton.TabIndex = 1;
            playButton.Text = "▶";
            playButton.Click += playButton_Click;
            // 
            // stopButton
            // 
            stopButton.BackColor = Color.Transparent;
            stopButton.CornerRadius = 14;
            stopButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            stopButton.ImagePadding = new Padding(0);
            stopButton.Location = new Point(170, 20);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(60, 40);
            stopButton.TabIndex = 2;
            stopButton.Text = "⏹";
            stopButton.Click += stopButton_Click;
            // 
            // saveButton
            // 
            saveButton.BackColor = Color.Transparent;
            saveButton.CornerRadius = 14;
            saveButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            saveButton.ImagePadding = new Padding(0);
            saveButton.Location = new Point(240, 20);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(108, 40);
            saveButton.TabIndex = 3;
            saveButton.Text = "Kaydet";
            saveButton.Click += saveButton_Click;
            // 
            // trimButton
            // 
            trimButton.BackColor = Color.Transparent;
            trimButton.CornerRadius = 14;
            trimButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            trimButton.ImagePadding = new Padding(0);
            trimButton.Location = new Point(354, 20);
            trimButton.Name = "trimButton";
            trimButton.Size = new Size(80, 40);
            trimButton.TabIndex = 4;
            trimButton.Text = "✂ Kırp";
            trimButton.Click += trimButton_Click;
            // 
            // mergeButton
            // 
            mergeButton.BackColor = Color.Transparent;
            mergeButton.CornerRadius = 14;
            mergeButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            mergeButton.ImagePadding = new Padding(0);
            mergeButton.Location = new Point(440, 20);
            mergeButton.Name = "mergeButton";
            mergeButton.Size = new Size(144, 40);
            mergeButton.TabIndex = 8;
            mergeButton.Text = "Birleştir";
            mergeButton.Click += mergeButton_Click;
            // 
            // waveformPanel
            // 
            waveformPanel.Dock = DockStyle.Fill;
            waveformPanel.Location = new Point(0, 80);
            waveformPanel.Name = "waveformPanel";
            waveformPanel.Size = new Size(1184, 401);
            waveformPanel.TabIndex = 0;
            waveformPanel.Paint += waveformPanel_Paint;
            waveformPanel.MouseClick += waveformPanel_MouseClick;
            // 
            // controlPanel
            // 
            controlPanel.Controls.Add(trimStartTrackBar);
            controlPanel.Controls.Add(trimEndTrackBar);
            controlPanel.Controls.Add(trimStartLabel);
            controlPanel.Controls.Add(trimEndLabel);
            controlPanel.Controls.Add(trimDurationLabel);
            controlPanel.Controls.Add(statusLabel);
            controlPanel.Dock = DockStyle.Bottom;
            controlPanel.Location = new Point(0, 481);
            controlPanel.Name = "controlPanel";
            controlPanel.Padding = new Padding(10);
            controlPanel.Size = new Size(1184, 220);
            controlPanel.TabIndex = 1;
            // 
            // trimStartTrackBar
            // 
            trimStartTrackBar.BackColor = Color.Transparent;
            trimStartTrackBar.BackgroundColor = Color.Black;
            trimStartTrackBar.BorderColor = Color.FromArgb(180, 80, 220);
            trimStartTrackBar.BorderThickness = 2;
            trimStartTrackBar.Location = new Point(10, 70);
            trimStartTrackBar.Maximum = 100000;
            trimStartTrackBar.Minimum = 0;
            trimStartTrackBar.Name = "trimStartTrackBar";
            trimStartTrackBar.Size = new Size(300, 30);
            trimStartTrackBar.TabIndex = 2;
            trimStartTrackBar.ThumbBorderColor = Color.FromArgb(180, 80, 220);
            trimStartTrackBar.ThumbColor = Color.White;
            trimStartTrackBar.TrackBackColor = Color.FromArgb(120, 80, 200);
            trimStartTrackBar.TrackProgressColor = Color.FromArgb(180, 80, 220);
            trimStartTrackBar.Value = 0;
            trimStartTrackBar.ValueChanged += trimStartTrackBar_ValueChanged;
            // 
            // trimEndTrackBar
            // 
            trimEndTrackBar.BackColor = Color.Transparent;
            trimEndTrackBar.BackgroundColor = Color.Black;
            trimEndTrackBar.BorderColor = Color.FromArgb(180, 80, 220);
            trimEndTrackBar.BorderThickness = 2;
            trimEndTrackBar.Location = new Point(320, 70);
            trimEndTrackBar.Maximum = 100000;
            trimEndTrackBar.Minimum = 0;
            trimEndTrackBar.Name = "trimEndTrackBar";
            trimEndTrackBar.Size = new Size(300, 30);
            trimEndTrackBar.TabIndex = 3;
            trimEndTrackBar.ThumbBorderColor = Color.FromArgb(180, 80, 220);
            trimEndTrackBar.ThumbColor = Color.White;
            trimEndTrackBar.TrackBackColor = Color.FromArgb(120, 80, 200);
            trimEndTrackBar.TrackProgressColor = Color.FromArgb(180, 80, 220);
            trimEndTrackBar.Value = 100000;
            trimEndTrackBar.ValueChanged += trimEndTrackBar_ValueChanged;
            // 
            // trimStartLabel
            // 
            trimStartLabel.ForeColor = Color.FromArgb(0, 200, 100);
            trimStartLabel.Location = new Point(10, 50);
            trimStartLabel.Name = "trimStartLabel";
            trimStartLabel.Size = new Size(200, 20);
            trimStartLabel.TabIndex = 4;
            trimStartLabel.Text = "Başlangıç: 00:00.00";
            // 
            // trimEndLabel
            // 
            trimEndLabel.ForeColor = Color.FromArgb(255, 80, 80);
            trimEndLabel.Location = new Point(320, 50);
            trimEndLabel.Name = "trimEndLabel";
            trimEndLabel.Size = new Size(200, 20);
            trimEndLabel.TabIndex = 5;
            trimEndLabel.Text = "Bitiş: 00:00.00";
            // 
            // trimDurationLabel
            // 
            trimDurationLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            trimDurationLabel.ForeColor = Color.FromArgb(100, 255, 100);
            trimDurationLabel.Location = new Point(10, 110);
            trimDurationLabel.Name = "trimDurationLabel";
            trimDurationLabel.Size = new Size(300, 20);
            trimDurationLabel.TabIndex = 6;
            trimDurationLabel.Text = "Seçilen Süre: 00:00.00";
            // 
            // statusLabel
            // 
            statusLabel.ForeColor = Color.White;
            statusLabel.Location = new Point(10, 140);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(400, 20);
            statusLabel.TabIndex = 7;
            statusLabel.Text = "Hazır";
            statusLabel.Visible = false;
            // 
            // timelinePanel
            // 
            timelinePanel.Controls.Add(positionTrackBar);
            timelinePanel.Controls.Add(currentTimeLabel);
            timelinePanel.Controls.Add(totalTimeLabel);
            timelinePanel.Dock = DockStyle.Bottom;
            timelinePanel.Location = new Point(0, 701);
            timelinePanel.Name = "timelinePanel";
            timelinePanel.Padding = new Padding(10, 5, 10, 5);
            timelinePanel.Size = new Size(1184, 60);
            timelinePanel.TabIndex = 2;
            // 
            // positionTrackBar
            // 
            positionTrackBar.BackColor = Color.Transparent;
            positionTrackBar.BackgroundColor = Color.Black;
            positionTrackBar.BorderColor = Color.FromArgb(180, 80, 220);
            positionTrackBar.BorderThickness = 2;
            positionTrackBar.Dock = DockStyle.Fill;
            positionTrackBar.Location = new Point(10, 5);
            positionTrackBar.Maximum = 100000;
            positionTrackBar.Minimum = 0;
            positionTrackBar.Name = "positionTrackBar";
            positionTrackBar.Size = new Size(1164, 50);
            positionTrackBar.TabIndex = 0;
            positionTrackBar.ThumbBorderColor = Color.FromArgb(180, 80, 220);
            positionTrackBar.ThumbColor = Color.White;
            positionTrackBar.TrackBackColor = Color.FromArgb(120, 80, 200);
            positionTrackBar.TrackProgressColor = Color.FromArgb(180, 80, 220);
            positionTrackBar.Value = 0;
            positionTrackBar.ValueChanged += positionTrackBar_ValueChanged;
            // 
            // currentTimeLabel
            // 
            currentTimeLabel.Font = new Font("Consolas", 12F, FontStyle.Bold);
            currentTimeLabel.ForeColor = Color.White;
            currentTimeLabel.Location = new Point(10, 15);
            currentTimeLabel.Name = "currentTimeLabel";
            currentTimeLabel.Size = new Size(80, 20);
            currentTimeLabel.TabIndex = 1;
            currentTimeLabel.Text = "00:00.00";
            // 
            // totalTimeLabel
            // 
            totalTimeLabel.Font = new Font("Consolas", 12F, FontStyle.Bold);
            totalTimeLabel.ForeColor = Color.White;
            totalTimeLabel.Location = new Point(100, 15);
            totalTimeLabel.Name = "totalTimeLabel";
            totalTimeLabel.Size = new Size(80, 20);
            totalTimeLabel.TabIndex = 2;
            totalTimeLabel.Text = "00:00.00";
            // 
            // EditSoundForm
            // 
            ClientSize = new Size(1184, 761);
            Controls.Add(waveformPanel);
            Controls.Add(controlPanel);
            Controls.Add(timelinePanel);
            Controls.Add(topPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(800, 600);
            Name = "EditSoundForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ses Düzenleyici";
            topPanel.ResumeLayout(false);
            controlPanel.ResumeLayout(false);
            timelinePanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}