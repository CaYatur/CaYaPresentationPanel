namespace SoundPanel
{
    partial class WaitSongSystem
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitSongSystem));
            lblCurrentTime = new Label();
            lblTotalTime = new Label();
            lblNowPlaying = new Label();
            lstSongs = new ListBox();
            button1 = new CustomButton();
            button2 = new CustomButton();
            button3 = new CustomButton();
            button4 = new CustomButton();
            button5 = new CustomButton();
            button6 = new CustomButton();
            timer1 = new System.Windows.Forms.Timer(components);
            Volume = new Label();
            button7 = new CustomButton();
            button8 = new CustomButton();
            trackBarVolume = new ModernTrackBar();
            trackBarSeek = new ModernTrackBar();
            SuspendLayout();
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCurrentTime.AutoSize = true;
            lblCurrentTime.BackColor = Color.FromArgb(33, 150, 243);
            lblCurrentTime.BorderStyle = BorderStyle.FixedSingle;
            lblCurrentTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCurrentTime.ForeColor = Color.White;
            lblCurrentTime.Location = new Point(12, 402);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Padding = new Padding(5);
            lblCurrentTime.Size = new Size(57, 31);
            lblCurrentTime.TabIndex = 0;
            lblCurrentTime.Text = "00:00";
            // 
            // lblTotalTime
            // 
            lblTotalTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTotalTime.AutoSize = true;
            lblTotalTime.BackColor = Color.FromArgb(156, 39, 176);
            lblTotalTime.BorderStyle = BorderStyle.FixedSingle;
            lblTotalTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalTime.ForeColor = Color.White;
            lblTotalTime.Location = new Point(793, 402);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Padding = new Padding(5);
            lblTotalTime.Size = new Size(57, 31);
            lblTotalTime.TabIndex = 1;
            lblTotalTime.Text = "00:00";
            // 
            // lblNowPlaying
            // 
            lblNowPlaying.AutoSize = true;
            lblNowPlaying.Font = new Font("Poppins", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblNowPlaying.ForeColor = Color.FromArgb(33, 150, 243);
            lblNowPlaying.Location = new Point(12, 9);
            lblNowPlaying.Name = "lblNowPlaying";
            lblNowPlaying.Size = new Size(143, 22);
            lblNowPlaying.TabIndex = 2;
            lblNowPlaying.Text = "🎵 Mevcut şarkı yok!";
            // 
            // lstSongs
            // 
            lstSongs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstSongs.BackColor = Color.FromArgb(80, 80, 86);
            lstSongs.BorderStyle = BorderStyle.None;
            lstSongs.Font = new Font("Poppins", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 162);
            lstSongs.ForeColor = Color.White;
            lstSongs.FormattingEnabled = true;
            lstSongs.Location = new Point(12, 35);
            lstSongs.Name = "lstSongs";
            lstSongs.Size = new Size(722, 276);
            lstSongs.TabIndex = 3;
            lstSongs.DoubleClick += lstSongs_DoubleClick;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.Transparent;
            button1.BorderColor = Color.FromArgb(56, 142, 60);
            button1.BorderThickness = 2;
            button1.ButtonBackColor = Color.FromArgb(76, 175, 80);
            button1.CornerRadius = 8;
            button1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button1.HoverColor = Color.FromArgb(102, 187, 106);
            button1.ImagePadding = new Padding(0);
            button1.Location = new Point(740, 35);
            button1.Name = "button1";
            button1.PressedColor = Color.FromArgb(46, 125, 50);
            button1.Size = new Size(109, 38);
            button1.TabIndex = 6;
            button1.Text = "➕ Şarkı Ekle";
            button1.Click += btnAddSongs_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button2.BackColor = Color.Transparent;
            button2.BorderColor = Color.FromArgb(56, 142, 60);
            button2.BorderThickness = 2;
            button2.ButtonBackColor = Color.FromArgb(76, 175, 80);
            button2.CornerRadius = 8;
            button2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button2.HoverColor = Color.FromArgb(102, 187, 106);
            button2.ImagePadding = new Padding(0);
            button2.Location = new Point(12, 352);
            button2.Name = "button2";
            button2.PressedColor = Color.FromArgb(46, 125, 50);
            button2.Size = new Size(81, 38);
            button2.TabIndex = 7;
            button2.Text = "▶ Oynat";
            button2.Click += btnPlay_Click;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button3.BackColor = Color.Transparent;
            button3.BorderColor = Color.FromArgb(245, 124, 0);
            button3.BorderThickness = 2;
            button3.ButtonBackColor = Color.FromArgb(255, 152, 0);
            button3.CornerRadius = 8;
            button3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button3.HoverColor = Color.FromArgb(255, 183, 77);
            button3.ImagePadding = new Padding(0);
            button3.Location = new Point(99, 352);
            button3.Name = "button3";
            button3.PressedColor = Color.FromArgb(230, 126, 34);
            button3.Size = new Size(100, 38);
            button3.TabIndex = 8;
            button3.Text = "⏸ Duraklat";
            button3.Click += btnPause_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button4.BackColor = Color.Transparent;
            button4.BorderColor = Color.FromArgb(198, 40, 40);
            button4.BorderThickness = 2;
            button4.ButtonBackColor = Color.FromArgb(244, 67, 54);
            button4.CornerRadius = 8;
            button4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button4.HoverColor = Color.FromArgb(255, 87, 74);
            button4.ImagePadding = new Padding(0);
            button4.Location = new Point(205, 352);
            button4.Name = "button4";
            button4.PressedColor = Color.FromArgb(183, 28, 28);
            button4.Size = new Size(81, 38);
            button4.TabIndex = 9;
            button4.Text = "⏹ Durdur";
            button4.Click += btnStop_Click;
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button5.BackColor = Color.Transparent;
            button5.BorderColor = Color.FromArgb(25, 118, 210);
            button5.BorderThickness = 2;
            button5.ButtonBackColor = Color.FromArgb(33, 150, 243);
            button5.CornerRadius = 8;
            button5.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button5.HoverColor = Color.FromArgb(66, 165, 245);
            button5.ImagePadding = new Padding(0);
            button5.Location = new Point(542, 352);
            button5.Name = "button5";
            button5.PressedColor = Color.FromArgb(21, 101, 192);
            button5.Size = new Size(81, 38);
            button5.TabIndex = 10;
            button5.Text = "⏭ Sonraki";
            button5.Click += btnNext_Click;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button6.BackColor = Color.Transparent;
            button6.BorderColor = Color.FromArgb(25, 118, 210);
            button6.BorderThickness = 2;
            button6.ButtonBackColor = Color.FromArgb(33, 150, 243);
            button6.CornerRadius = 8;
            button6.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button6.HoverColor = Color.FromArgb(66, 165, 245);
            button6.ImagePadding = new Padding(0);
            button6.Location = new Point(455, 352);
            button6.Name = "button6";
            button6.PressedColor = Color.FromArgb(21, 101, 192);
            button6.Size = new Size(81, 38);
            button6.TabIndex = 11;
            button6.Text = "⏮ Önceki";
            button6.Click += btnPrevious_Click;
            // 
            // Volume
            // 
            Volume.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Volume.AutoSize = true;
            Volume.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            Volume.ForeColor = Color.FromArgb(33, 150, 243);
            Volume.Location = new Point(629, 334);
            Volume.Name = "Volume";
            Volume.Size = new Size(157, 20);
            Volume.TabIndex = 12;
            Volume.Text = "🔊 Ses Düzeyi: 100%";
            // 
            // button7
            // 
            button7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button7.BackColor = Color.Transparent;
            button7.BorderColor = Color.FromArgb(123, 31, 162);
            button7.BorderThickness = 2;
            button7.ButtonBackColor = Color.FromArgb(156, 39, 176);
            button7.CornerRadius = 8;
            button7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button7.HoverColor = Color.FromArgb(171, 71, 188);
            button7.ImagePadding = new Padding(0);
            button7.Location = new Point(740, 79);
            button7.Name = "button7";
            button7.PressedColor = Color.FromArgb(106, 27, 154);
            button7.Size = new Size(109, 38);
            button7.TabIndex = 13;
            button7.Text = "Kayıt Yolunu Aç";
            button7.Click += button7_Click;
            // 
            // button8
            // 
            button8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button8.BackColor = Color.Transparent;
            button8.BorderColor = Color.FromArgb(0, 151, 167);
            button8.BorderThickness = 2;
            button8.ButtonBackColor = Color.FromArgb(0, 188, 212);
            button8.CornerRadius = 8;
            button8.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button8.HoverColor = Color.FromArgb(77, 208, 225);
            button8.ImagePadding = new Padding(0);
            button8.Location = new Point(740, 123);
            button8.Name = "button8";
            button8.PressedColor = Color.FromArgb(0, 131, 143);
            button8.Size = new Size(109, 38);
            button8.TabIndex = 14;
            button8.Text = "Listeyi Güncelle";
            button8.Click += button8_Click;
            // 
            // trackBarVolume
            // 
            trackBarVolume.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            trackBarVolume.BackColor = Color.Transparent;
            trackBarVolume.BackgroundColor = Color.FromArgb(37, 37, 38);
            trackBarVolume.BorderColor = Color.FromArgb(33, 150, 243);
            trackBarVolume.BorderThickness = 0;
            trackBarVolume.CornerRadius = 6;
            trackBarVolume.Location = new Point(629, 357);
            trackBarVolume.Maximum = 100;
            trackBarVolume.Minimum = 0;
            trackBarVolume.Name = "trackBarVolume";
            trackBarVolume.Size = new Size(220, 28);
            trackBarVolume.TabIndex = 15;
            trackBarVolume.ThumbBorderColor = Color.FromArgb(33, 150, 243);
            trackBarVolume.ThumbColor = Color.White;
            trackBarVolume.ThumbSizeRatio = 1.5F;
            trackBarVolume.TrackBackColor = Color.FromArgb(60, 60, 60);
            trackBarVolume.TrackProgressColor = Color.FromArgb(33, 150, 243);
            trackBarVolume.TrackThicknessRatio = 0.4F;
            trackBarVolume.Value = 100;
            trackBarVolume.ValueChanged += trackBarVolume_ValueChanged;
            // 
            // trackBarSeek
            // 
            trackBarSeek.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackBarSeek.BackColor = Color.Transparent;
            trackBarSeek.BackgroundColor = Color.FromArgb(37, 37, 38);
            trackBarSeek.BorderColor = Color.FromArgb(156, 39, 176);
            trackBarSeek.BorderThickness = 0;
            trackBarSeek.CornerRadius = 6;
            trackBarSeek.Location = new Point(12, 437);
            trackBarSeek.Maximum = 100;
            trackBarSeek.Minimum = 0;
            trackBarSeek.Name = "trackBarSeek";
            trackBarSeek.Size = new Size(837, 25);
            trackBarSeek.TabIndex = 16;
            trackBarSeek.ThumbBorderColor = Color.FromArgb(156, 39, 176);
            trackBarSeek.ThumbColor = Color.White;
            trackBarSeek.ThumbSizeRatio = 1.3F;
            trackBarSeek.TrackBackColor = Color.FromArgb(60, 60, 60);
            trackBarSeek.TrackProgressColor = Color.FromArgb(156, 39, 176);
            trackBarSeek.TrackThicknessRatio = 0.5F;
            trackBarSeek.Value = 0;
            trackBarSeek.ScrollStart += trackBarSeek_ScrollStart;
            trackBarSeek.ScrollEnd += trackBarSeek_ScrollEnd;
            // 
            // WaitSongSystem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 48);
            ClientSize = new Size(861, 474);
            Controls.Add(trackBarSeek);
            Controls.Add(trackBarVolume);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(Volume);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(lstSongs);
            Controls.Add(lblNowPlaying);
            Controls.Add(lblTotalTime);
            Controls.Add(lblCurrentTime);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "WaitSongSystem";
            Text = "🎵 Bekleme Müzik Sistemi";
            FormClosing += WaitSongSystem_FormClosing;
            Load += WaitSongSystem_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCurrentTime;
        private Label lblTotalTime;
        private Label lblNowPlaying;
        private ListBox lstSongs;
        private CustomButton button1;
        private CustomButton button2;
        private CustomButton button3;
        private CustomButton button4;
        private CustomButton button5;
        private CustomButton button6;
        private System.Windows.Forms.Timer timer1;
        private Label Volume;
        private CustomButton button7;
        private CustomButton button8;
        private ModernTrackBar trackBarVolume;
        private ModernTrackBar trackBarSeek;
    }
}