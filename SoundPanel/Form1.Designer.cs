namespace SoundPanel
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            flowLayoutPanelSongs = new FlowLayoutPanel();
            btnAddNewSong = new CustomButton();
            lblCurrentTime = new Label();
            lblTotalTime = new Label();
            button1 = new CustomButton();
            button2 = new CustomButton();
            button3 = new CustomButton();
            button4 = new CustomButton();
            button5 = new CustomButton();
            label1 = new Label();
            button6 = new CustomButton();
            button7 = new CustomButton();
            button8 = new CustomButton();
            button9 = new CustomButton();
            trackSVolume = new ModernTrackBar();
            trackBarProgress = new ModernTrackBar();
            panel1 = new Panel();
            label2 = new Label();
            panel2 = new Panel();
            MediaEditMenu = new CustomButton();
            customSwitch1 = new CustomSwitch();
            panel3 = new Panel();
            btnMore = new CustomButton();
            btnResumeStopwatch = new CustomButton();
            btnStopStopwatch = new CustomButton();
            btnStartStopwatch = new CustomButton();
            panel4 = new Panel();
            label4 = new Label();
            lblStopwatch = new Label();
            lblClock = new Label();
            customButton1 = new CustomButton();
            button10 = new Button();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanelSongs
            // 
            flowLayoutPanelSongs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanelSongs.AutoScroll = true;
            flowLayoutPanelSongs.BackColor = SystemColors.GrayText;
            flowLayoutPanelSongs.BackgroundImageLayout = ImageLayout.Stretch;
            flowLayoutPanelSongs.BorderStyle = BorderStyle.FixedSingle;
            flowLayoutPanelSongs.Location = new Point(12, 12);
            flowLayoutPanelSongs.Name = "flowLayoutPanelSongs";
            flowLayoutPanelSongs.Size = new Size(875, 508);
            flowLayoutPanelSongs.TabIndex = 0;
            flowLayoutPanelSongs.Scroll += flowLayoutPanelSongs_Scroll;
            flowLayoutPanelSongs.Resize += flowLayoutPanelSongs_Resize;
            // 
            // btnAddNewSong
            // 
            btnAddNewSong.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddNewSong.BackColor = Color.Transparent;
            btnAddNewSong.BorderThickness = 2;
            btnAddNewSong.ButtonBackColor = Color.FromArgb(37, 43, 63);
            btnAddNewSong.ButtonImage = Properties.Resources.add;
            btnAddNewSong.CornerRadius = 4;
            btnAddNewSong.DisabledBackColor = Color.FromArgb(24, 26, 34);
            btnAddNewSong.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            btnAddNewSong.DisabledTextColor = Color.FromArgb(90, 95, 110);
            btnAddNewSong.Font = new Font("Poppins", 7F, FontStyle.Bold);
            btnAddNewSong.HoverColor = Color.FromArgb(27, 31, 46);
            btnAddNewSong.ImagePadding = new Padding(0);
            btnAddNewSong.ImageScaleReduction = -4;
            btnAddNewSong.ImageTextPadding = 6;
            btnAddNewSong.Location = new Point(81, 3);
            btnAddNewSong.Name = "btnAddNewSong";
            btnAddNewSong.PressedColor = Color.FromArgb(22, 25, 37);
            btnAddNewSong.Size = new Size(70, 70);
            btnAddNewSong.TabIndex = 20;
            btnAddNewSong.Text = "Ekle";
            btnAddNewSong.TextBelowImage = true;
            btnAddNewSong.Click += btnAddNewSong_Click;
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCurrentTime.BackColor = Color.Transparent;
            lblCurrentTime.Font = new Font("Poppins", 10F, FontStyle.Bold);
            lblCurrentTime.ForeColor = Color.FromArgb(255, 79, 108);
            lblCurrentTime.Location = new Point(3, 70);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Padding = new Padding(5);
            lblCurrentTime.Size = new Size(101, 29);
            lblCurrentTime.TabIndex = 4;
            lblCurrentTime.Text = "00:00";
            // 
            // lblTotalTime
            // 
            lblTotalTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTotalTime.BackColor = Color.Transparent;
            lblTotalTime.Font = new Font("Poppins", 10F, FontStyle.Bold);
            lblTotalTime.ForeColor = Color.FromArgb(146, 146, 255);
            lblTotalTime.Location = new Point(597, 70);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Padding = new Padding(5);
            lblTotalTime.RightToLeft = RightToLeft.No;
            lblTotalTime.Size = new Size(108, 29);
            lblTotalTime.TabIndex = 6;
            lblTotalTime.Text = "00:00";
            lblTotalTime.TextAlign = ContentAlignment.TopRight;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.BackColor = Color.Transparent;
            button1.BorderThickness = 2;
            button1.ButtonBackColor = Color.FromArgb(255, 79, 108);
            button1.ButtonImage = Properties.Resources.stop;
            button1.CornerRadius = 4;
            button1.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button1.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button1.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button1.Font = new Font("Poppins", 11F, FontStyle.Bold);
            button1.HoverColor = Color.FromArgb(255, 99, 128);
            button1.ImagePadding = new Padding(0);
            button1.ImageScaleReduction = 25;
            button1.Location = new Point(728, 528);
            button1.Name = "button1";
            button1.PressedColor = Color.FromArgb(200, 60, 90);
            button1.Size = new Size(45, 45);
            button1.TabIndex = 7;
            button1.Click += btnStop_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.BackColor = Color.Transparent;
            button2.BorderThickness = 2;
            button2.ButtonBackColor = Color.FromArgb(146, 146, 255);
            button2.ButtonImage = Properties.Resources.playpause;
            button2.CornerRadius = 4;
            button2.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button2.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button2.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button2.Font = new Font("Poppins", 10F, FontStyle.Bold);
            button2.HoverColor = Color.FromArgb(166, 166, 255);
            button2.ImagePadding = new Padding(0);
            button2.ImageScaleReduction = 25;
            button2.Location = new Point(779, 528);
            button2.Name = "button2";
            button2.PressedColor = Color.FromArgb(100, 100, 200);
            button2.Size = new Size(45, 45);
            button2.TabIndex = 8;
            button2.Click += btnPause_Click;
            // 
            // button3
            // 
            button3.AllowTextWrapping = true;
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.BackColor = Color.Transparent;
            button3.BorderThickness = 2;
            button3.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button3.ButtonImage = Properties.Resources.Video;
            button3.CornerRadius = 4;
            button3.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button3.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button3.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button3.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button3.HoverColor = Color.FromArgb(27, 31, 46);
            button3.ImagePadding = new Padding(0);
            button3.ImageScaleReduction = -15;
            button3.ImageTextGap = 0;
            button3.ImageTextPadding = 0;
            button3.ImageTextScaleReduction = 22;
            button3.Location = new Point(81, 79);
            button3.Name = "button3";
            button3.PressedColor = Color.FromArgb(22, 25, 37);
            button3.Size = new Size(70, 70);
            button3.TabIndex = 10;
            button3.Text = "Video Oynatıcı";
            button3.TextBelowImage = true;
            button3.Click += btnVideo_Click;
            // 
            // button4
            // 
            button4.AllowTextWrapping = true;
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button4.BackColor = Color.Transparent;
            button4.BorderThickness = 2;
            button4.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button4.ButtonImage = Properties.Resources.Image;
            button4.CornerRadius = 4;
            button4.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button4.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button4.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button4.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button4.HoverColor = Color.FromArgb(27, 31, 46);
            button4.ImagePadding = new Padding(0);
            button4.ImageScaleReduction = -10;
            button4.ImageTextPadding = 7;
            button4.Location = new Point(5, 155);
            button4.Name = "button4";
            button4.PressedColor = Color.FromArgb(22, 25, 37);
            button4.Size = new Size(70, 70);
            button4.TabIndex = 11;
            button4.Text = "Arkaplan";
            button4.TextBelowImage = true;
            button4.Click += btnBackGround_Click;
            // 
            // button5
            // 
            button5.AllowTextWrapping = true;
            button5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button5.BackColor = Color.Transparent;
            button5.BorderThickness = 2;
            button5.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button5.ButtonImage = Properties.Resources.waitmusic;
            button5.CornerRadius = 4;
            button5.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button5.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button5.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button5.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button5.HoverColor = Color.FromArgb(27, 31, 46);
            button5.ImagePadding = new Padding(0);
            button5.ImageScaleReduction = -15;
            button5.ImageTextGap = 0;
            button5.ImageTextPadding = 0;
            button5.ImageTextScaleReduction = 22;
            button5.Location = new Point(5, 79);
            button5.Name = "button5";
            button5.PressedColor = Color.FromArgb(22, 25, 37);
            button5.Size = new Size(70, 70);
            button5.TabIndex = 12;
            button5.Text = "Bekleme Müzikleri";
            button5.TextBelowImage = true;
            button5.Click += button5_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Poppins", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
            label1.ForeColor = Color.White;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(130, 22);
            label1.TabIndex = 14;
            label1.Text = "🔊 Ses Düzeyi: %50";
            // 
            // button6
            // 
            button6.AllowTextWrapping = true;
            button6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button6.BackColor = Color.Transparent;
            button6.BorderThickness = 2;
            button6.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button6.ButtonImage = Properties.Resources.Presentation;
            button6.CornerRadius = 4;
            button6.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button6.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button6.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button6.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button6.HoverColor = Color.FromArgb(27, 31, 46);
            button6.ImagePadding = new Padding(0);
            button6.ImageScaleReduction = -15;
            button6.ImageTextPadding = 7;
            button6.Location = new Point(5, 231);
            button6.Name = "button6";
            button6.PressedColor = Color.FromArgb(22, 25, 37);
            button6.Size = new Size(70, 70);
            button6.TabIndex = 15;
            button6.Text = "Sunu Aç";
            button6.TextBelowImage = true;
            button6.Click += button6_Click;
            // 
            // button7
            // 
            button7.AllowTextWrapping = true;
            button7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button7.BackColor = Color.Transparent;
            button7.BorderThickness = 2;
            button7.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button7.ButtonImage = Properties.Resources.Program;
            button7.CornerRadius = 4;
            button7.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button7.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button7.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button7.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button7.HoverColor = Color.FromArgb(27, 31, 46);
            button7.ImagePadding = new Padding(0);
            button7.ImageScaleReduction = 5;
            button7.ImageTextGap = 2;
            button7.ImageTextPadding = 0;
            button7.Location = new Point(5, 307);
            button7.Name = "button7";
            button7.PressedColor = Color.FromArgb(22, 25, 37);
            button7.Size = new Size(70, 70);
            button7.TabIndex = 18;
            button7.Text = "Program Aç";
            button7.TextBelowImage = true;
            button7.Click += button7_Click;
            // 
            // button8
            // 
            button8.AllowTextWrapping = true;
            button8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button8.BackColor = Color.Transparent;
            button8.BorderThickness = 2;
            button8.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button8.ButtonImage = Properties.Resources.WBC;
            button8.CornerRadius = 4;
            button8.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button8.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button8.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button8.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button8.HoverColor = Color.FromArgb(27, 31, 46);
            button8.ImagePadding = new Padding(0);
            button8.ImageScaleReduction = -15;
            button8.ImageTextGap = 1;
            button8.ImageTextPadding = 7;
            button8.Location = new Point(81, 155);
            button8.Name = "button8";
            button8.PressedColor = Color.FromArgb(22, 25, 37);
            button8.Size = new Size(70, 70);
            button8.TabIndex = 19;
            button8.Text = "Uzaktan Kontrol";
            button8.TextBelowImage = true;
            button8.Click += button8_Click;
            // 
            // button9
            // 
            button9.AllowTextWrapping = true;
            button9.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button9.BackColor = Color.Transparent;
            button9.BorderThickness = 2;
            button9.ButtonBackColor = Color.FromArgb(37, 43, 63);
            button9.ButtonImage = Properties.Resources.SoundVisu;
            button9.CornerRadius = 4;
            button9.DisabledBackColor = Color.FromArgb(24, 26, 34);
            button9.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            button9.DisabledTextColor = Color.FromArgb(90, 95, 110);
            button9.Font = new Font("Poppins", 7F, FontStyle.Bold);
            button9.HoverColor = Color.FromArgb(27, 31, 46);
            button9.ImagePadding = new Padding(0);
            button9.ImageTextGap = 0;
            button9.ImageTextPadding = 0;
            button9.ImageTextScaleReduction = 0;
            button9.Location = new Point(81, 231);
            button9.Name = "button9";
            button9.PressedColor = Color.FromArgb(22, 25, 37);
            button9.Size = new Size(70, 70);
            button9.TabIndex = 20;
            button9.Text = "Ses Görseli";
            button9.TextBelowImage = true;
            button9.Click += button9_Click;
            // 
            // trackSVolume
            // 
            trackSVolume.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            trackSVolume.BackColor = Color.Transparent;
            trackSVolume.BackgroundColor = Color.FromArgb(35, 41, 59);
            trackSVolume.BorderColor = Color.FromArgb(78, 87, 114);
            trackSVolume.BorderThickness = 2;
            trackSVolume.CornerRadius = 2;
            trackSVolume.Location = new Point(3, 24);
            trackSVolume.Maximum = 100;
            trackSVolume.Minimum = 0;
            trackSVolume.Name = "trackSVolume";
            trackSVolume.Size = new Size(311, 20);
            trackSVolume.TabIndex = 21;
            trackSVolume.ThumbBorderColor = Color.White;
            trackSVolume.ThumbColor = Color.White;
            trackSVolume.TrackBackColor = Color.FromArgb(50, 56, 75);
            trackSVolume.TrackProgressColor = Color.FromArgb(255, 204, 123);
            trackSVolume.Value = 50;
            trackSVolume.ValueChanged += trackSVolume_Scroll;
            // 
            // trackBarProgress
            // 
            trackBarProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackBarProgress.BackColor = Color.Transparent;
            trackBarProgress.BackgroundColor = Color.FromArgb(35, 41, 59);
            trackBarProgress.BorderColor = Color.FromArgb(78, 87, 114);
            trackBarProgress.BorderThickness = 2;
            trackBarProgress.CornerRadius = 2;
            trackBarProgress.Location = new Point(3, 25);
            trackBarProgress.Maximum = 100;
            trackBarProgress.Minimum = 0;
            trackBarProgress.Name = "trackBarProgress";
            trackBarProgress.Size = new Size(702, 45);
            trackBarProgress.TabIndex = 22;
            trackBarProgress.ThumbBorderColor = Color.White;
            trackBarProgress.ThumbColor = Color.White;
            trackBarProgress.TrackBackColor = Color.FromArgb(50, 56, 75);
            trackBarProgress.TrackProgressColor = Color.FromArgb(79, 199, 228);
            trackBarProgress.Value = 0;
            trackBarProgress.ValueChanged += trackBarProgress_Scroll;
            trackBarProgress.ScrollStart += trackBarProgress_ScrollStart;
            trackBarProgress.ScrollEnd += trackBarProgress_ScrollEnd;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(37, 43, 63);
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(trackBarProgress);
            panel1.Controls.Add(lblCurrentTime);
            panel1.Controls.Add(lblTotalTime);
            panel1.Controls.Add(label2);
            panel1.Location = new Point(12, 528);
            panel1.Name = "panel1";
            panel1.Size = new Size(710, 101);
            panel1.TabIndex = 23;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Font = new Font("Poppins", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
            label2.ForeColor = Color.White;
            label2.Location = new Point(3, 3);
            label2.Name = "label2";
            label2.Size = new Size(101, 22);
            label2.TabIndex = 23;
            label2.Text = "Medya Kontrol";
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(37, 43, 63);
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(trackSVolume);
            panel2.Controls.Add(label1);
            panel2.Location = new Point(730, 579);
            panel2.Name = "panel2";
            panel2.Size = new Size(319, 50);
            panel2.TabIndex = 24;
            // 
            // MediaEditMenu
            // 
            MediaEditMenu.AllowTextWrapping = true;
            MediaEditMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            MediaEditMenu.BackColor = Color.Transparent;
            MediaEditMenu.BorderThickness = 2;
            MediaEditMenu.ButtonBackColor = Color.FromArgb(37, 43, 63);
            MediaEditMenu.ButtonImage = Properties.Resources.MediaEdit;
            MediaEditMenu.CornerRadius = 4;
            MediaEditMenu.DisabledBackColor = Color.FromArgb(24, 26, 34);
            MediaEditMenu.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            MediaEditMenu.DisabledTextColor = Color.FromArgb(90, 95, 110);
            MediaEditMenu.Font = new Font("Poppins", 7F, FontStyle.Bold);
            MediaEditMenu.HoverColor = Color.FromArgb(27, 31, 46);
            MediaEditMenu.ImagePadding = new Padding(0);
            MediaEditMenu.ImageTextGap = 1;
            MediaEditMenu.ImageTextPadding = 2;
            MediaEditMenu.ImageTextScaleReduction = 1;
            MediaEditMenu.Location = new Point(81, 307);
            MediaEditMenu.Name = "MediaEditMenu";
            MediaEditMenu.PressedColor = Color.FromArgb(22, 25, 37);
            MediaEditMenu.Size = new Size(70, 70);
            MediaEditMenu.TabIndex = 25;
            MediaEditMenu.Text = "Medya Düzenleme";
            MediaEditMenu.TextBelowImage = true;
            MediaEditMenu.Click += MediaEditMenu_Click;
            // 
            // customSwitch1
            // 
            customSwitch1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            customSwitch1.BackColor = Color.Transparent;
            customSwitch1.BorderColor = Color.FromArgb(78, 87, 114);
            customSwitch1.BorderThickness = 2;
            customSwitch1.CornerRadius = 4;
            customSwitch1.DisabledBackColor = Color.FromArgb(24, 26, 34);
            customSwitch1.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            customSwitch1.DisabledTextColor = Color.FromArgb(90, 95, 110);
            customSwitch1.Font = new Font("Poppins", 7F, FontStyle.Bold);
            customSwitch1.HoverColor = Color.FromArgb(27, 31, 46);
            customSwitch1.Location = new Point(830, 528);
            customSwitch1.Name = "customSwitch1";
            customSwitch1.OffColor = Color.FromArgb(31, 37, 53);
            customSwitch1.OffText = "Döngü Kapalı";
            customSwitch1.OffTextColor = Color.White;
            customSwitch1.OffThumbBorderColor = Color.FromArgb(180, 60, 60);
            customSwitch1.OffThumbColor = Color.FromArgb(220, 80, 80);
            customSwitch1.OnColor = Color.FromArgb(31, 37, 53);
            customSwitch1.OnOffTextFontSize = 0F;
            customSwitch1.OnText = "Döngü Açık";
            customSwitch1.OnTextColor = Color.White;
            customSwitch1.OnThumbBorderColor = Color.FromArgb(73, 147, 132);
            customSwitch1.OnThumbColor = Color.FromArgb(99, 198, 179);
            customSwitch1.PressedColor = Color.FromArgb(22, 25, 37);
            customSwitch1.ShowBorder = true;
            customSwitch1.Size = new Size(90, 45);
            customSwitch1.SwitchBackColor = Color.FromArgb(37, 43, 63);
            customSwitch1.TabIndex = 26;
            customSwitch1.Text = "customSwitch1";
            customSwitch1.TextColor = Color.White;
            customSwitch1.CheckedChanged += customSwitch1_CheckedChanged;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel3.BackColor = Color.FromArgb(37, 43, 63);
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(btnMore);
            panel3.Controls.Add(btnAddNewSong);
            panel3.Controls.Add(button3);
            panel3.Controls.Add(MediaEditMenu);
            panel3.Controls.Add(button4);
            panel3.Controls.Add(button5);
            panel3.Controls.Add(button6);
            panel3.Controls.Add(button9);
            panel3.Controls.Add(button7);
            panel3.Controls.Add(button8);
            panel3.Location = new Point(893, 13);
            panel3.Name = "panel3";
            panel3.Size = new Size(156, 386);
            panel3.TabIndex = 27;
            // 
            // btnMore
            // 
            btnMore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMore.BackColor = Color.Transparent;
            btnMore.BorderThickness = 2;
            btnMore.ButtonBackColor = Color.FromArgb(37, 43, 63);
            btnMore.CornerRadius = 4;
            btnMore.DisabledBackColor = Color.FromArgb(24, 26, 34);
            btnMore.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            btnMore.DisabledTextColor = Color.FromArgb(90, 95, 110);
            btnMore.Font = new Font("Poppins", 7F, FontStyle.Bold);
            btnMore.HoverColor = Color.FromArgb(27, 31, 46);
            btnMore.ImagePadding = new Padding(0);
            btnMore.ImageScaleReduction = 10;
            btnMore.ImageTextPadding = 7;
            btnMore.Location = new Point(5, 3);
            btnMore.Name = "btnMore";
            btnMore.PressedColor = Color.FromArgb(22, 25, 37);
            btnMore.Size = new Size(70, 70);
            btnMore.TabIndex = 26;
            btnMore.Text = "Daha Fazla";
            btnMore.TextBelowImage = true;
            btnMore.Click += btnMore_Click;
            // 
            // btnResumeStopwatch
            // 
            btnResumeStopwatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnResumeStopwatch.BackColor = Color.Transparent;
            btnResumeStopwatch.BorderThickness = 2;
            btnResumeStopwatch.ButtonBackColor = Color.FromArgb(146, 146, 255);
            btnResumeStopwatch.ButtonImage = Properties.Resources.pause;
            btnResumeStopwatch.CornerRadius = 4;
            btnResumeStopwatch.DisabledBackColor = Color.FromArgb(24, 26, 34);
            btnResumeStopwatch.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            btnResumeStopwatch.DisabledTextColor = Color.FromArgb(90, 95, 110);
            btnResumeStopwatch.Font = new Font("Poppins", 10F, FontStyle.Bold);
            btnResumeStopwatch.HoverColor = Color.FromArgb(166, 166, 255);
            btnResumeStopwatch.ImagePadding = new Padding(0);
            btnResumeStopwatch.ImageScaleReduction = 20;
            btnResumeStopwatch.Location = new Point(60, 52);
            btnResumeStopwatch.Name = "btnResumeStopwatch";
            btnResumeStopwatch.PressedColor = Color.FromArgb(100, 100, 200);
            btnResumeStopwatch.Size = new Size(35, 35);
            btnResumeStopwatch.TabIndex = 29;
            btnResumeStopwatch.Click += btnResumeStopwatch_Click;
            // 
            // btnStopStopwatch
            // 
            btnStopStopwatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnStopStopwatch.BackColor = Color.Transparent;
            btnStopStopwatch.BorderThickness = 2;
            btnStopStopwatch.ButtonBackColor = Color.FromArgb(255, 79, 108);
            btnStopStopwatch.ButtonImage = Properties.Resources.stop;
            btnStopStopwatch.CornerRadius = 4;
            btnStopStopwatch.DisabledBackColor = Color.FromArgb(24, 26, 34);
            btnStopStopwatch.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            btnStopStopwatch.DisabledTextColor = Color.FromArgb(90, 95, 110);
            btnStopStopwatch.Font = new Font("Poppins", 11F, FontStyle.Bold);
            btnStopStopwatch.HoverColor = Color.FromArgb(255, 99, 128);
            btnStopStopwatch.ImagePadding = new Padding(0);
            btnStopStopwatch.ImageScaleReduction = 20;
            btnStopStopwatch.Location = new Point(19, 52);
            btnStopStopwatch.Name = "btnStopStopwatch";
            btnStopStopwatch.PressedColor = Color.FromArgb(200, 60, 90);
            btnStopStopwatch.Size = new Size(35, 35);
            btnStopStopwatch.TabIndex = 28;
            btnStopStopwatch.Click += btnStopStopwatch_Click;
            // 
            // btnStartStopwatch
            // 
            btnStartStopwatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnStartStopwatch.BackColor = Color.Transparent;
            btnStartStopwatch.BorderThickness = 2;
            btnStartStopwatch.ButtonBackColor = Color.FromArgb(79, 200, 229);
            btnStartStopwatch.ButtonImage = Properties.Resources.play;
            btnStartStopwatch.CornerRadius = 4;
            btnStartStopwatch.DisabledBackColor = Color.FromArgb(24, 26, 34);
            btnStartStopwatch.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            btnStartStopwatch.DisabledTextColor = Color.FromArgb(90, 95, 110);
            btnStartStopwatch.Font = new Font("Poppins", 10F, FontStyle.Bold);
            btnStartStopwatch.HoverColor = Color.FromArgb(61, 155, 178);
            btnStartStopwatch.ImagePadding = new Padding(0);
            btnStartStopwatch.ImageScaleReduction = 20;
            btnStartStopwatch.Location = new Point(101, 52);
            btnStartStopwatch.Name = "btnStartStopwatch";
            btnStartStopwatch.PressedColor = Color.FromArgb(43, 110, 126);
            btnStartStopwatch.Size = new Size(35, 35);
            btnStartStopwatch.TabIndex = 30;
            btnStartStopwatch.Click += btnStartStopwatch_Click;
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel4.BackColor = Color.FromArgb(37, 43, 63);
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(btnStopStopwatch);
            panel4.Controls.Add(btnStartStopwatch);
            panel4.Controls.Add(btnResumeStopwatch);
            panel4.Controls.Add(label4);
            panel4.Controls.Add(lblStopwatch);
            panel4.Controls.Add(lblClock);
            panel4.Location = new Point(893, 404);
            panel4.Name = "panel4";
            panel4.Size = new Size(156, 116);
            panel4.TabIndex = 31;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Poppins", 8F, FontStyle.Bold);
            label4.ForeColor = Color.FromArgb(78, 87, 114);
            label4.Location = new Point(38, 3);
            label4.Name = "label4";
            label4.Size = new Size(78, 19);
            label4.TabIndex = 32;
            label4.Text = "Kronometre";
            // 
            // lblStopwatch
            // 
            lblStopwatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblStopwatch.AutoSize = true;
            lblStopwatch.BackColor = Color.Transparent;
            lblStopwatch.Font = new Font("Poppins", 16F, FontStyle.Bold);
            lblStopwatch.ForeColor = Color.White;
            lblStopwatch.Location = new Point(16, 13);
            lblStopwatch.Name = "lblStopwatch";
            lblStopwatch.Padding = new Padding(5);
            lblStopwatch.RightToLeft = RightToLeft.No;
            lblStopwatch.Size = new Size(123, 49);
            lblStopwatch.TabIndex = 31;
            lblStopwatch.Text = "00:00:00";
            lblStopwatch.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblClock
            // 
            lblClock.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblClock.BackColor = Color.Transparent;
            lblClock.Font = new Font("Poppins", 11F, FontStyle.Bold);
            lblClock.ForeColor = Color.LightSeaGreen;
            lblClock.Location = new Point(32, 88);
            lblClock.Name = "lblClock";
            lblClock.RightToLeft = RightToLeft.No;
            lblClock.Size = new Size(90, 24);
            lblClock.TabIndex = 33;
            lblClock.Text = "00:00:00";
            lblClock.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // customButton1
            // 
            customButton1.AllowTextWrapping = true;
            customButton1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            customButton1.BackColor = Color.Transparent;
            customButton1.BorderThickness = 2;
            customButton1.ButtonBackColor = Color.FromArgb(37, 43, 63);
            customButton1.ButtonImage = Properties.Resources.CaYaDEV;
            customButton1.CornerRadius = 4;
            customButton1.DisabledBackColor = Color.FromArgb(24, 26, 34);
            customButton1.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            customButton1.DisabledTextColor = Color.FromArgb(90, 95, 110);
            customButton1.Font = new Font("Poppins", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 162);
            customButton1.HoverColor = Color.FromArgb(27, 31, 46);
            customButton1.ImagePadding = new Padding(0);
            customButton1.ImageScaleReduction = -65;
            customButton1.ImageTextGap = 0;
            customButton1.Location = new Point(926, 528);
            customButton1.Name = "customButton1";
            customButton1.PressedColor = Color.FromArgb(22, 25, 37);
            customButton1.Size = new Size(123, 45);
            customButton1.TabIndex = 32;
            customButton1.TextBelowImage = true;
            customButton1.Click += customButton1_Click;
            // 
            // button10
            // 
            button10.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button10.Font = new Font("Poppins", 48F, FontStyle.Bold);
            button10.Location = new Point(12, 13);
            button10.Name = "button10";
            button10.Size = new Size(875, 507);
            button10.TabIndex = 33;
            button10.Text = "İçerik Alanı";
            button10.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 48);
            ClientSize = new Size(1061, 639);
            Controls.Add(customButton1);
            Controls.Add(panel4);
            Controls.Add(panel3);
            Controls.Add(customSwitch1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(flowLayoutPanelSongs);
            Controls.Add(button10);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🎬 CaYa Gösterim Paneli";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanelSongs;
        private CustomButton btnAddNewSong;
        private Label lblCurrentTime;
        private Label lblTotalTime;
        private CustomButton button1;
        private CustomButton button2;
        private CustomButton button3;
        private CustomButton button4;
        private CustomButton button5;
        private Label label1;
        private CustomButton button6;
        private CustomButton button7;
        private CustomButton button8;
        private CustomButton button9;
        private ModernTrackBar trackSVolume;
        private ModernTrackBar trackBarProgress;
        private Panel panel1;
        private Panel panel2;
        private CustomButton MediaEditMenu;
        private Label label2;
        private CustomSwitch customSwitch1;
        private Panel panel3;
        private CustomButton btnResumeStopwatch;
        private CustomButton btnStopStopwatch;
        private CustomButton btnStartStopwatch;
        private Panel panel4;
        private Label lblStopwatch;
        private Label label4;
        private Label lblClock;
        private CustomButton btnMore;
        private CustomButton customButton1;
        private Button button10;
    }
}