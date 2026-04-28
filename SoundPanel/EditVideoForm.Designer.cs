namespace SoundPanel
{
    partial class EditVideoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditVideoForm));
            vlcControl = new LibVLCSharp.WinForms.VideoView();
            panelControls = new Panel();
            tabControlFeatures = new TabControl();
            tabPageBasic = new TabPage();
            panelVolumeControl = new Panel();
            lblVolumeValue = new Label();
            trackBarVolume = new TrackBar();
            lblVolume = new Label();
            lblDetailedInfo = new Label();
            lblFileInfo = new Label();
            lblShortcuts = new Label();
            progressBar = new ProgressBar();
            lblTrimInfo = new Label();
            btnExport = new CustomButton();
            btnTrimVideo = new CustomButton();
            btnSetEnd = new CustomButton();
            btnSetStart = new CustomButton();
            lblCurrentTime = new Label();
            lblDuration = new Label();
            btnStop = new CustomButton();
            btnPause = new CustomButton();
            btnPlay = new CustomButton();
            lblVideoPath = new Label();
            btnOpenVideo = new CustomButton();
            tabPageOverlays = new TabPage();
            splitContainer1 = new SplitContainer();
            groupBoxOverlayList = new GroupBox();
            listBoxOverlays = new ListBox();
            panelOverlayButtons = new Panel();
            btnToggleOverlayEdit = new CustomButton();
            btnRemoveOverlay = new CustomButton();
            btnAddText = new CustomButton();
            btnAddImage = new CustomButton();
            groupBoxOverlayEdit = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            numericStartTime = new NumericUpDown();
            numericEndTime = new NumericUpDown();
            numericX = new NumericUpDown();
            numericY = new NumericUpDown();
            numericWidth = new NumericUpDown();
            numericHeight = new NumericUpDown();
            label7 = new Label();
            textBoxContent = new TextBox();
            groupBoxTextProperties = new GroupBox();
            buttonTextColor = new Button();
            label10 = new Label();
            comboBoxFont = new ComboBox();
            label9 = new Label();
            numericFontSize = new NumericUpDown();
            label8 = new Label();
            tabPageMerge = new TabPage();
            splitContainer2 = new SplitContainer();
            groupBoxVideoList = new GroupBox();
            listBoxVideos = new ListBox();
            panelVideoButtons = new Panel();
            btnMoveVideoDown = new CustomButton();
            btnMoveVideoUp = new CustomButton();
            btnRemoveVideo = new CustomButton();
            btnAddVideo = new CustomButton();
            groupBoxMergeOptions = new GroupBox();
            lblMergeInfo = new Label();
            btnMergeVideos = new CustomButton();
            panelTimeline = new Panel();
            panelMain = new Panel();
            ((System.ComponentModel.ISupportInitialize)vlcControl).BeginInit();
            panelControls.SuspendLayout();
            tabControlFeatures.SuspendLayout();
            tabPageBasic.SuspendLayout();
            panelVolumeControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolume).BeginInit();
            tabPageOverlays.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBoxOverlayList.SuspendLayout();
            panelOverlayButtons.SuspendLayout();
            groupBoxOverlayEdit.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericStartTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericEndTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericHeight).BeginInit();
            groupBoxTextProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericFontSize).BeginInit();
            tabPageMerge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBoxVideoList.SuspendLayout();
            panelVideoButtons.SuspendLayout();
            groupBoxMergeOptions.SuspendLayout();
            panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // vlcControl
            // 
            vlcControl.BackColor = Color.Black;
            vlcControl.Dock = DockStyle.Fill;
            vlcControl.Location = new Point(0, 0);
            vlcControl.Margin = new Padding(4, 3, 4, 3);
            vlcControl.MediaPlayer = null;
            vlcControl.Name = "vlcControl";
            vlcControl.Size = new Size(1370, 391);
            vlcControl.TabIndex = 0;
            vlcControl.Text = "videoView1";
            // 
            // panelControls
            // 
            panelControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelControls.BackColor = Color.FromArgb(45, 45, 48);
            panelControls.BorderStyle = BorderStyle.FixedSingle;
            panelControls.Controls.Add(tabControlFeatures);
            panelControls.Location = new Point(14, 469);
            panelControls.Margin = new Padding(4, 3, 4, 3);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(1372, 278);
            panelControls.TabIndex = 1;
            // 
            // tabControlFeatures
            // 
            tabControlFeatures.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlFeatures.Controls.Add(tabPageBasic);
            tabControlFeatures.Controls.Add(tabPageOverlays);
            tabControlFeatures.Controls.Add(tabPageMerge);
            tabControlFeatures.Location = new Point(0, 16);
            tabControlFeatures.Margin = new Padding(4, 3, 4, 3);
            tabControlFeatures.Name = "tabControlFeatures";
            tabControlFeatures.SelectedIndex = 0;
            tabControlFeatures.Size = new Size(1370, 260);
            tabControlFeatures.TabIndex = 0;
            // 
            // tabPageBasic
            // 
            tabPageBasic.BackColor = Color.FromArgb(45, 45, 48);
            tabPageBasic.Controls.Add(panelVolumeControl);
            tabPageBasic.Controls.Add(lblDetailedInfo);
            tabPageBasic.Controls.Add(lblFileInfo);
            tabPageBasic.Controls.Add(lblShortcuts);
            tabPageBasic.Controls.Add(progressBar);
            tabPageBasic.Controls.Add(lblTrimInfo);
            tabPageBasic.Controls.Add(btnExport);
            tabPageBasic.Controls.Add(btnTrimVideo);
            tabPageBasic.Controls.Add(btnSetEnd);
            tabPageBasic.Controls.Add(btnSetStart);
            tabPageBasic.Controls.Add(lblCurrentTime);
            tabPageBasic.Controls.Add(lblDuration);
            tabPageBasic.Controls.Add(btnStop);
            tabPageBasic.Controls.Add(btnPause);
            tabPageBasic.Controls.Add(btnPlay);
            tabPageBasic.Controls.Add(lblVideoPath);
            tabPageBasic.Controls.Add(btnOpenVideo);
            tabPageBasic.Location = new Point(4, 24);
            tabPageBasic.Margin = new Padding(4, 3, 4, 3);
            tabPageBasic.Name = "tabPageBasic";
            tabPageBasic.Padding = new Padding(4, 3, 4, 3);
            tabPageBasic.Size = new Size(1362, 232);
            tabPageBasic.TabIndex = 0;
            tabPageBasic.Text = "🎬 Temel Düzenleme";
            // 
            // panelVolumeControl
            // 
            panelVolumeControl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelVolumeControl.Controls.Add(lblVolumeValue);
            panelVolumeControl.Controls.Add(trackBarVolume);
            panelVolumeControl.Controls.Add(lblVolume);
            panelVolumeControl.Location = new Point(1108, 17);
            panelVolumeControl.Margin = new Padding(4, 3, 4, 3);
            panelVolumeControl.Name = "panelVolumeControl";
            panelVolumeControl.Size = new Size(245, 69);
            panelVolumeControl.TabIndex = 16;
            // 
            // lblVolumeValue
            // 
            lblVolumeValue.AutoSize = true;
            lblVolumeValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblVolumeValue.ForeColor = Color.Yellow;
            lblVolumeValue.Location = new Point(134, 2);
            lblVolumeValue.Margin = new Padding(4, 0, 4, 0);
            lblVolumeValue.Name = "lblVolumeValue";
            lblVolumeValue.Size = new Size(38, 15);
            lblVolumeValue.TabIndex = 2;
            lblVolumeValue.Text = "100%";
            // 
            // trackBarVolume
            // 
            trackBarVolume.AutoSize = false;
            trackBarVolume.BackColor = Color.FromArgb(45, 45, 48);
            trackBarVolume.Enabled = false;
            trackBarVolume.Location = new Point(4, 23);
            trackBarVolume.Margin = new Padding(4, 3, 4, 3);
            trackBarVolume.Maximum = 500;
            trackBarVolume.Name = "trackBarVolume";
            trackBarVolume.Size = new Size(233, 35);
            trackBarVolume.TabIndex = 1;
            trackBarVolume.TickFrequency = 50;
            trackBarVolume.Value = 100;
            trackBarVolume.Scroll += trackBarVolume_Scroll;
            // 
            // lblVolume
            // 
            lblVolume.AutoSize = true;
            lblVolume.ForeColor = Color.White;
            lblVolume.Location = new Point(4, 2);
            lblVolume.Margin = new Padding(4, 0, 4, 0);
            lblVolume.Name = "lblVolume";
            lblVolume.Size = new Size(92, 15);
            lblVolume.TabIndex = 0;
            lblVolume.Text = "🔊 Ses (0-500%):";
            // 
            // lblDetailedInfo
            // 
            lblDetailedInfo.AutoSize = true;
            lblDetailedInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDetailedInfo.ForeColor = Color.Cyan;
            lblDetailedInfo.Location = new Point(18, 191);
            lblDetailedInfo.Margin = new Padding(4, 0, 4, 0);
            lblDetailedInfo.Name = "lblDetailedInfo";
            lblDetailedInfo.Size = new Size(323, 15);
            lblDetailedInfo.TabIndex = 15;
            lblDetailedInfo.Text = "📊 Kesim: 00:00:000 / 00:00:000 (0.0%) | Kalan: 00:00:000";
            // 
            // lblFileInfo
            // 
            lblFileInfo.AutoSize = true;
            lblFileInfo.ForeColor = Color.LightGray;
            lblFileInfo.Location = new Point(18, 52);
            lblFileInfo.Margin = new Padding(4, 0, 4, 0);
            lblFileInfo.Name = "lblFileInfo";
            lblFileInfo.Size = new Size(257, 15);
            lblFileInfo.TabIndex = 14;
            lblFileInfo.Text = "📁 Dosya: 0 MB | 📅 Oluşturma: 15.07.2025 03:03";
            // 
            // lblShortcuts
            // 
            lblShortcuts.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblShortcuts.AutoSize = true;
            lblShortcuts.ForeColor = Color.FromArgb(180, 180, 180);
            lblShortcuts.Location = new Point(18, 211);
            lblShortcuts.Margin = new Padding(4, 0, 4, 0);
            lblShortcuts.Name = "lblShortcuts";
            lblShortcuts.Size = new Size(527, 15);
            lblShortcuts.TabIndex = 13;
            lblShortcuts.Text = "Kısayollar: Space=Oynat/Duraklat | ←→=5sn Sar | ↑↓=Ses±10% | Home/End=Başla/Bitir | F1=Yardım";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressBar.Location = new Point(974, 188);
            progressBar.Margin = new Padding(4, 3, 4, 3);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(368, 27);
            progressBar.TabIndex = 12;
            progressBar.Visible = false;
            // 
            // lblTrimInfo
            // 
            lblTrimInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTrimInfo.ForeColor = Color.White;
            lblTrimInfo.Location = new Point(18, 151);
            lblTrimInfo.Margin = new Padding(4, 0, 4, 0);
            lblTrimInfo.Name = "lblTrimInfo";
            lblTrimInfo.Size = new Size(583, 35);
            lblTrimInfo.TabIndex = 11;
            lblTrimInfo.Text = "✂️ Başlangıç: 00:00:00 | 🏁 Bitiş: 00:00:00\r\n⏱️ Kesim Süresi: 00:00:00 (0.0% orijinalin)";
            // 
            // btnExport
            // 
            btnExport.BackColor = Color.Transparent;
            btnExport.BorderColor = Color.FromArgb(255, 140, 0);
            btnExport.BorderThickness = 2;
            btnExport.ButtonBackColor = Color.FromArgb(255, 140, 0);
            btnExport.CornerRadius = 8;
            btnExport.Cursor = Cursors.Hand;
            btnExport.Enabled = false;
            btnExport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExport.HoverColor = Color.FromArgb(235, 120, 0);
            btnExport.ImagePadding = new Padding(0);
            btnExport.Location = new Point(729, 95);
            btnExport.Margin = new Padding(4, 3, 4, 3);
            btnExport.Name = "btnExport";
            btnExport.PressedColor = Color.FromArgb(215, 100, 0);
            btnExport.Size = new Size(93, 37);
            btnExport.TabIndex = 10;
            btnExport.Text = "🎯 Dışa Aktar\r\n(Ctrl+E)";
            btnExport.Click += btnExport_Click;
            // 
            // btnTrimVideo
            // 
            btnTrimVideo.BackColor = Color.Transparent;
            btnTrimVideo.BorderColor = Color.FromArgb(0, 122, 204);
            btnTrimVideo.BorderThickness = 2;
            btnTrimVideo.ButtonBackColor = Color.FromArgb(0, 122, 204);
            btnTrimVideo.CornerRadius = 8;
            btnTrimVideo.Cursor = Cursors.Hand;
            btnTrimVideo.Enabled = false;
            btnTrimVideo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTrimVideo.HoverColor = Color.FromArgb(0, 100, 180);
            btnTrimVideo.ImagePadding = new Padding(0);
            btnTrimVideo.Location = new Point(618, 95);
            btnTrimVideo.Margin = new Padding(4, 3, 4, 3);
            btnTrimVideo.Name = "btnTrimVideo";
            btnTrimVideo.PressedColor = Color.FromArgb(0, 80, 160);
            btnTrimVideo.Size = new Size(104, 37);
            btnTrimVideo.TabIndex = 9;
            btnTrimVideo.Text = "Kırp. Oynat\r\n(Ctrl+P)";
            btnTrimVideo.Click += btnTrimVideo_Click;
            // 
            // btnSetEnd
            // 
            btnSetEnd.BackColor = Color.Transparent;
            btnSetEnd.BorderColor = Color.FromArgb(204, 0, 0);
            btnSetEnd.BorderThickness = 2;
            btnSetEnd.ButtonBackColor = Color.FromArgb(204, 0, 0);
            btnSetEnd.CornerRadius = 8;
            btnSetEnd.Cursor = Cursors.Hand;
            btnSetEnd.Enabled = false;
            btnSetEnd.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSetEnd.HoverColor = Color.FromArgb(180, 0, 0);
            btnSetEnd.ImagePadding = new Padding(0);
            btnSetEnd.Location = new Point(531, 95);
            btnSetEnd.Margin = new Padding(4, 3, 4, 3);
            btnSetEnd.Name = "btnSetEnd";
            btnSetEnd.PressedColor = Color.FromArgb(160, 0, 0);
            btnSetEnd.Size = new Size(80, 37);
            btnSetEnd.TabIndex = 8;
            btnSetEnd.Text = "🏁 Bitiş\r\n(Ctrl+O)";
            btnSetEnd.Click += btnSetEnd_Click;
            // 
            // btnSetStart
            // 
            btnSetStart.BackColor = Color.Transparent;
            btnSetStart.BorderColor = Color.FromArgb(0, 204, 0);
            btnSetStart.BorderThickness = 2;
            btnSetStart.ButtonBackColor = Color.FromArgb(0, 204, 0);
            btnSetStart.CornerRadius = 8;
            btnSetStart.Cursor = Cursors.Hand;
            btnSetStart.Enabled = false;
            btnSetStart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSetStart.HoverColor = Color.FromArgb(0, 180, 0);
            btnSetStart.ImagePadding = new Padding(0);
            btnSetStart.Location = new Point(443, 95);
            btnSetStart.Margin = new Padding(4, 3, 4, 3);
            btnSetStart.Name = "btnSetStart";
            btnSetStart.PressedColor = Color.FromArgb(0, 160, 0);
            btnSetStart.Size = new Size(80, 37);
            btnSetStart.TabIndex = 7;
            btnSetStart.Text = "⏰ Başlangıç\r\n(Ctrl+I)";
            btnSetStart.Click += btnSetStart_Click;
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.AutoSize = true;
            lblCurrentTime.Font = new Font("Consolas", 16F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblCurrentTime.ForeColor = Color.Yellow;
            lblCurrentTime.Location = new Point(298, 97);
            lblCurrentTime.Margin = new Padding(4, 0, 4, 0);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Size = new Size(108, 26);
            lblCurrentTime.TabIndex = 6;
            lblCurrentTime.Text = "00:00:00";
            // 
            // lblDuration
            // 
            lblDuration.AutoSize = true;
            lblDuration.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDuration.ForeColor = Color.White;
            lblDuration.Location = new Point(298, 127);
            lblDuration.Margin = new Padding(4, 0, 4, 0);
            lblDuration.Name = "lblDuration";
            lblDuration.Size = new Size(117, 15);
            lblDuration.TabIndex = 5;
            lblDuration.Text = "🎬 Toplam: 00:00:00";
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.Transparent;
            btnStop.BorderColor = Color.FromArgb(62, 62, 66);
            btnStop.BorderThickness = 2;
            btnStop.ButtonBackColor = Color.FromArgb(62, 62, 66);
            btnStop.CornerRadius = 8;
            btnStop.Cursor = Cursors.Hand;
            btnStop.Enabled = false;
            btnStop.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnStop.HoverColor = Color.FromArgb(80, 80, 84);
            btnStop.ImagePadding = new Padding(0);
            btnStop.Location = new Point(238, 111);
            btnStop.Margin = new Padding(4, 3, 4, 3);
            btnStop.Name = "btnStop";
            btnStop.PressedColor = Color.FromArgb(50, 50, 54);
            btnStop.Size = new Size(52, 37);
            btnStop.TabIndex = 4;
            btnStop.Text = "⏹";
            btnStop.Click += btnStop_Click;
            // 
            // btnPause
            // 
            btnPause.BackColor = Color.Transparent;
            btnPause.BorderColor = Color.FromArgb(62, 62, 66);
            btnPause.BorderThickness = 2;
            btnPause.ButtonBackColor = Color.FromArgb(62, 62, 66);
            btnPause.CornerRadius = 8;
            btnPause.Cursor = Cursors.Hand;
            btnPause.Enabled = false;
            btnPause.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnPause.HoverColor = Color.FromArgb(80, 80, 84);
            btnPause.ImagePadding = new Padding(0);
            btnPause.Location = new Point(178, 111);
            btnPause.Margin = new Padding(4, 3, 4, 3);
            btnPause.Name = "btnPause";
            btnPause.PressedColor = Color.FromArgb(50, 50, 54);
            btnPause.Size = new Size(52, 37);
            btnPause.TabIndex = 3;
            btnPause.Text = "⏸";
            btnPause.Click += btnPause_Click;
            // 
            // btnPlay
            // 
            btnPlay.BackColor = Color.Transparent;
            btnPlay.BorderColor = Color.FromArgb(62, 62, 66);
            btnPlay.BorderThickness = 2;
            btnPlay.ButtonBackColor = Color.FromArgb(62, 62, 66);
            btnPlay.CornerRadius = 8;
            btnPlay.Cursor = Cursors.Hand;
            btnPlay.Enabled = false;
            btnPlay.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnPlay.HoverColor = Color.FromArgb(80, 80, 84);
            btnPlay.ImagePadding = new Padding(0);
            btnPlay.Location = new Point(119, 111);
            btnPlay.Margin = new Padding(4, 3, 4, 3);
            btnPlay.Name = "btnPlay";
            btnPlay.PressedColor = Color.FromArgb(50, 50, 54);
            btnPlay.Size = new Size(52, 37);
            btnPlay.TabIndex = 2;
            btnPlay.Text = "▶";
            btnPlay.Click += btnPlay_Click;
            // 
            // lblVideoPath
            // 
            lblVideoPath.AutoSize = true;
            lblVideoPath.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblVideoPath.ForeColor = Color.White;
            lblVideoPath.Location = new Point(18, 25);
            lblVideoPath.Margin = new Padding(4, 0, 4, 0);
            lblVideoPath.Name = "lblVideoPath";
            lblVideoPath.Size = new Size(127, 19);
            lblVideoPath.TabIndex = 1;
            lblVideoPath.Text = "Video seçilmedi...";
            // 
            // btnOpenVideo
            // 
            btnOpenVideo.BackColor = Color.Transparent;
            btnOpenVideo.BorderColor = Color.FromArgb(0, 122, 204);
            btnOpenVideo.BorderThickness = 2;
            btnOpenVideo.ButtonBackColor = Color.FromArgb(0, 122, 204);
            btnOpenVideo.CornerRadius = 8;
            btnOpenVideo.Cursor = Cursors.Hand;
            btnOpenVideo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOpenVideo.HoverColor = Color.FromArgb(0, 100, 180);
            btnOpenVideo.ImagePadding = new Padding(0);
            btnOpenVideo.Location = new Point(18, 111);
            btnOpenVideo.Margin = new Padding(4, 3, 4, 3);
            btnOpenVideo.Name = "btnOpenVideo";
            btnOpenVideo.PressedColor = Color.FromArgb(0, 80, 160);
            btnOpenVideo.Size = new Size(93, 37);
            btnOpenVideo.TabIndex = 0;
            btnOpenVideo.Text = "📁 Video Aç\r\n(Ctrl+O)";
            btnOpenVideo.Click += btnOpenVideo_Click;
            // 
            // tabPageOverlays
            // 
            tabPageOverlays.BackColor = Color.FromArgb(45, 45, 48);
            tabPageOverlays.Controls.Add(splitContainer1);
            tabPageOverlays.Location = new Point(4, 24);
            tabPageOverlays.Margin = new Padding(4, 3, 4, 3);
            tabPageOverlays.Name = "tabPageOverlays";
            tabPageOverlays.Padding = new Padding(4, 3, 4, 3);
            tabPageOverlays.Size = new Size(1362, 232);
            tabPageOverlays.TabIndex = 1;
            tabPageOverlays.Text = "🎨 Resim & Yazı Overlay";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(4, 3);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(groupBoxOverlayList);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(groupBoxOverlayEdit);
            splitContainer1.Size = new Size(1354, 226);
            splitContainer1.SplitterDistance = 408;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // groupBoxOverlayList
            // 
            groupBoxOverlayList.Controls.Add(listBoxOverlays);
            groupBoxOverlayList.Controls.Add(panelOverlayButtons);
            groupBoxOverlayList.Dock = DockStyle.Fill;
            groupBoxOverlayList.ForeColor = Color.White;
            groupBoxOverlayList.Location = new Point(0, 0);
            groupBoxOverlayList.Margin = new Padding(4, 3, 4, 3);
            groupBoxOverlayList.Name = "groupBoxOverlayList";
            groupBoxOverlayList.Padding = new Padding(4, 3, 4, 3);
            groupBoxOverlayList.Size = new Size(408, 226);
            groupBoxOverlayList.TabIndex = 0;
            groupBoxOverlayList.TabStop = false;
            groupBoxOverlayList.Text = "📋 Overlay Listesi (F2: Edit Modu)";
            // 
            // listBoxOverlays
            // 
            listBoxOverlays.BackColor = Color.FromArgb(60, 60, 60);
            listBoxOverlays.BorderStyle = BorderStyle.None;
            listBoxOverlays.Dock = DockStyle.Fill;
            listBoxOverlays.ForeColor = Color.White;
            listBoxOverlays.FormattingEnabled = true;
            listBoxOverlays.Location = new Point(4, 54);
            listBoxOverlays.Margin = new Padding(4, 3, 4, 3);
            listBoxOverlays.Name = "listBoxOverlays";
            listBoxOverlays.Size = new Size(400, 169);
            listBoxOverlays.TabIndex = 1;
            // 
            // panelOverlayButtons
            // 
            panelOverlayButtons.Controls.Add(btnToggleOverlayEdit);
            panelOverlayButtons.Controls.Add(btnRemoveOverlay);
            panelOverlayButtons.Controls.Add(btnAddText);
            panelOverlayButtons.Controls.Add(btnAddImage);
            panelOverlayButtons.Dock = DockStyle.Top;
            panelOverlayButtons.Location = new Point(4, 19);
            panelOverlayButtons.Margin = new Padding(4, 3, 4, 3);
            panelOverlayButtons.Name = "panelOverlayButtons";
            panelOverlayButtons.Size = new Size(400, 35);
            panelOverlayButtons.TabIndex = 0;
            // 
            // btnToggleOverlayEdit
            // 
            btnToggleOverlayEdit.BackColor = Color.Transparent;
            btnToggleOverlayEdit.BorderColor = Color.FromArgb(128, 128, 128);
            btnToggleOverlayEdit.BorderThickness = 1;
            btnToggleOverlayEdit.ButtonBackColor = Color.FromArgb(128, 128, 128);
            btnToggleOverlayEdit.CornerRadius = 5;
            btnToggleOverlayEdit.Cursor = Cursors.Hand;
            btnToggleOverlayEdit.Enabled = false;
            btnToggleOverlayEdit.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            btnToggleOverlayEdit.HoverColor = Color.FromArgb(0, 204, 0);
            btnToggleOverlayEdit.ImagePadding = new Padding(0);
            btnToggleOverlayEdit.Location = new Point(303, 3);
            btnToggleOverlayEdit.Margin = new Padding(4, 3, 4, 3);
            btnToggleOverlayEdit.Name = "btnToggleOverlayEdit";
            btnToggleOverlayEdit.PressedColor = Color.FromArgb(0, 160, 0);
            btnToggleOverlayEdit.Size = new Size(93, 28);
            btnToggleOverlayEdit.TabIndex = 3;
            btnToggleOverlayEdit.Text = "🎯 Edit Modu";
            btnToggleOverlayEdit.Click += btnToggleOverlayEdit_Click;
            // 
            // btnRemoveOverlay
            // 
            btnRemoveOverlay.BackColor = Color.Transparent;
            btnRemoveOverlay.BorderColor = Color.FromArgb(204, 0, 0);
            btnRemoveOverlay.BorderThickness = 1;
            btnRemoveOverlay.ButtonBackColor = Color.FromArgb(204, 0, 0);
            btnRemoveOverlay.CornerRadius = 5;
            btnRemoveOverlay.Cursor = Cursors.Hand;
            btnRemoveOverlay.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnRemoveOverlay.HoverColor = Color.FromArgb(180, 0, 0);
            btnRemoveOverlay.ImagePadding = new Padding(0);
            btnRemoveOverlay.Location = new Point(203, 3);
            btnRemoveOverlay.Margin = new Padding(4, 3, 4, 3);
            btnRemoveOverlay.Name = "btnRemoveOverlay";
            btnRemoveOverlay.PressedColor = Color.FromArgb(160, 0, 0);
            btnRemoveOverlay.Size = new Size(93, 28);
            btnRemoveOverlay.TabIndex = 2;
            btnRemoveOverlay.Text = "🗑️ Sil";
            btnRemoveOverlay.Click += btnRemoveOverlay_Click;
            // 
            // btnAddText
            // 
            btnAddText.BackColor = Color.Transparent;
            btnAddText.BorderColor = Color.FromArgb(0, 204, 0);
            btnAddText.BorderThickness = 1;
            btnAddText.ButtonBackColor = Color.FromArgb(0, 204, 0);
            btnAddText.CornerRadius = 5;
            btnAddText.Cursor = Cursors.Hand;
            btnAddText.Enabled = false;
            btnAddText.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnAddText.HoverColor = Color.FromArgb(0, 180, 0);
            btnAddText.ImagePadding = new Padding(0);
            btnAddText.Location = new Point(103, 3);
            btnAddText.Margin = new Padding(4, 3, 4, 3);
            btnAddText.Name = "btnAddText";
            btnAddText.PressedColor = Color.FromArgb(0, 160, 0);
            btnAddText.Size = new Size(93, 28);
            btnAddText.TabIndex = 1;
            btnAddText.Text = "📝 Yazı Ekle";
            btnAddText.Click += btnAddText_Click;
            // 
            // btnAddImage
            // 
            btnAddImage.BackColor = Color.Transparent;
            btnAddImage.BorderColor = Color.FromArgb(0, 122, 204);
            btnAddImage.BorderThickness = 1;
            btnAddImage.ButtonBackColor = Color.FromArgb(0, 122, 204);
            btnAddImage.CornerRadius = 5;
            btnAddImage.Cursor = Cursors.Hand;
            btnAddImage.Enabled = false;
            btnAddImage.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnAddImage.HoverColor = Color.FromArgb(0, 100, 180);
            btnAddImage.ImagePadding = new Padding(0);
            btnAddImage.Location = new Point(4, 3);
            btnAddImage.Margin = new Padding(4, 3, 4, 3);
            btnAddImage.Name = "btnAddImage";
            btnAddImage.PressedColor = Color.FromArgb(0, 80, 160);
            btnAddImage.Size = new Size(93, 28);
            btnAddImage.TabIndex = 0;
            btnAddImage.Text = "🖼️ Resim Ekle";
            btnAddImage.Click += btnAddImage_Click;
            // 
            // groupBoxOverlayEdit
            // 
            groupBoxOverlayEdit.Controls.Add(tableLayoutPanel1);
            groupBoxOverlayEdit.Controls.Add(groupBoxTextProperties);
            groupBoxOverlayEdit.Dock = DockStyle.Fill;
            groupBoxOverlayEdit.Enabled = false;
            groupBoxOverlayEdit.ForeColor = Color.White;
            groupBoxOverlayEdit.Location = new Point(0, 0);
            groupBoxOverlayEdit.Margin = new Padding(4, 3, 4, 3);
            groupBoxOverlayEdit.Name = "groupBoxOverlayEdit";
            groupBoxOverlayEdit.Padding = new Padding(4, 3, 4, 3);
            groupBoxOverlayEdit.Size = new Size(941, 226);
            groupBoxOverlayEdit.TabIndex = 0;
            groupBoxOverlayEdit.TabStop = false;
            groupBoxOverlayEdit.Text = "⚙️ Overlay Düzenle (Drag & Drop ile Video Üzerinde)";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 2, 0);
            tableLayoutPanel1.Controls.Add(label4, 3, 0);
            tableLayoutPanel1.Controls.Add(label5, 0, 2);
            tableLayoutPanel1.Controls.Add(label6, 1, 2);
            tableLayoutPanel1.Controls.Add(numericStartTime, 0, 1);
            tableLayoutPanel1.Controls.Add(numericEndTime, 1, 1);
            tableLayoutPanel1.Controls.Add(numericX, 2, 1);
            tableLayoutPanel1.Controls.Add(numericY, 3, 1);
            tableLayoutPanel1.Controls.Add(numericWidth, 0, 3);
            tableLayoutPanel1.Controls.Add(numericHeight, 1, 3);
            tableLayoutPanel1.Controls.Add(label7, 2, 2);
            tableLayoutPanel1.Controls.Add(textBoxContent, 2, 3);
            tableLayoutPanel1.Location = new Point(7, 22);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel1.Size = new Size(583, 143);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.White;
            label1.Location = new Point(4, 0);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(91, 15);
            label1.TabIndex = 0;
            label1.Text = "⏰ Başlangıç (s):";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.White;
            label2.Location = new Point(149, 0);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 1;
            label2.Text = "🏁 Bitiş (s):";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.White;
            label3.Location = new Point(294, 0);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(32, 15);
            label3.TabIndex = 2;
            label3.Text = "📍 X:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.White;
            label4.Location = new Point(439, 0);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(32, 15);
            label4.TabIndex = 3;
            label4.Text = "📍 Y:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.White;
            label5.Location = new Point(4, 52);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(66, 15);
            label5.TabIndex = 4;
            label5.Text = "📏 Genişlik:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = Color.White;
            label6.Location = new Point(149, 52);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(74, 15);
            label6.TabIndex = 5;
            label6.Text = "📐 Yükseklik:";
            // 
            // numericStartTime
            // 
            numericStartTime.BackColor = Color.FromArgb(60, 60, 60);
            numericStartTime.DecimalPlaces = 1;
            numericStartTime.ForeColor = Color.White;
            numericStartTime.Location = new Point(4, 26);
            numericStartTime.Margin = new Padding(4, 3, 4, 3);
            numericStartTime.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numericStartTime.Name = "numericStartTime";
            numericStartTime.Size = new Size(137, 23);
            numericStartTime.TabIndex = 6;
            numericStartTime.ValueChanged += numericStartTime_ValueChanged;
            // 
            // numericEndTime
            // 
            numericEndTime.BackColor = Color.FromArgb(60, 60, 60);
            numericEndTime.DecimalPlaces = 1;
            numericEndTime.ForeColor = Color.White;
            numericEndTime.Location = new Point(149, 26);
            numericEndTime.Margin = new Padding(4, 3, 4, 3);
            numericEndTime.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numericEndTime.Name = "numericEndTime";
            numericEndTime.Size = new Size(137, 23);
            numericEndTime.TabIndex = 7;
            numericEndTime.ValueChanged += numericEndTime_ValueChanged;
            // 
            // numericX
            // 
            numericX.BackColor = Color.FromArgb(60, 60, 60);
            numericX.ForeColor = Color.White;
            numericX.Location = new Point(294, 26);
            numericX.Margin = new Padding(4, 3, 4, 3);
            numericX.Maximum = new decimal(new int[] { 1920, 0, 0, 0 });
            numericX.Name = "numericX";
            numericX.Size = new Size(137, 23);
            numericX.TabIndex = 8;
            numericX.ValueChanged += numericX_ValueChanged;
            // 
            // numericY
            // 
            numericY.BackColor = Color.FromArgb(60, 60, 60);
            numericY.ForeColor = Color.White;
            numericY.Location = new Point(439, 26);
            numericY.Margin = new Padding(4, 3, 4, 3);
            numericY.Maximum = new decimal(new int[] { 1080, 0, 0, 0 });
            numericY.Name = "numericY";
            numericY.Size = new Size(139, 23);
            numericY.TabIndex = 9;
            numericY.ValueChanged += numericY_ValueChanged;
            // 
            // numericWidth
            // 
            numericWidth.BackColor = Color.FromArgb(60, 60, 60);
            numericWidth.ForeColor = Color.White;
            numericWidth.Location = new Point(4, 78);
            numericWidth.Margin = new Padding(4, 3, 4, 3);
            numericWidth.Maximum = new decimal(new int[] { 1920, 0, 0, 0 });
            numericWidth.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericWidth.Name = "numericWidth";
            numericWidth.Size = new Size(137, 23);
            numericWidth.TabIndex = 10;
            numericWidth.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numericWidth.ValueChanged += numericWidth_ValueChanged;
            // 
            // numericHeight
            // 
            numericHeight.BackColor = Color.FromArgb(60, 60, 60);
            numericHeight.ForeColor = Color.White;
            numericHeight.Location = new Point(149, 78);
            numericHeight.Margin = new Padding(4, 3, 4, 3);
            numericHeight.Maximum = new decimal(new int[] { 1080, 0, 0, 0 });
            numericHeight.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericHeight.Name = "numericHeight";
            numericHeight.Size = new Size(137, 23);
            numericHeight.TabIndex = 11;
            numericHeight.Value = new decimal(new int[] { 50, 0, 0, 0 });
            numericHeight.ValueChanged += numericHeight_ValueChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.ForeColor = Color.White;
            label7.Location = new Point(294, 52);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(90, 15);
            label7.TabIndex = 12;
            label7.Text = "📄 İçerik/Dosya:";
            // 
            // textBoxContent
            // 
            textBoxContent.BackColor = Color.FromArgb(60, 60, 60);
            tableLayoutPanel1.SetColumnSpan(textBoxContent, 2);
            textBoxContent.ForeColor = Color.White;
            textBoxContent.Location = new Point(294, 78);
            textBoxContent.Margin = new Padding(4, 3, 4, 3);
            textBoxContent.Multiline = true;
            textBoxContent.Name = "textBoxContent";
            textBoxContent.Size = new Size(284, 62);
            textBoxContent.TabIndex = 13;
            textBoxContent.TextChanged += textBoxContent_TextChanged;
            // 
            // groupBoxTextProperties
            // 
            groupBoxTextProperties.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBoxTextProperties.Controls.Add(buttonTextColor);
            groupBoxTextProperties.Controls.Add(label10);
            groupBoxTextProperties.Controls.Add(comboBoxFont);
            groupBoxTextProperties.Controls.Add(label9);
            groupBoxTextProperties.Controls.Add(numericFontSize);
            groupBoxTextProperties.Controls.Add(label8);
            groupBoxTextProperties.ForeColor = Color.White;
            groupBoxTextProperties.Location = new Point(598, 22);
            groupBoxTextProperties.Margin = new Padding(4, 3, 4, 3);
            groupBoxTextProperties.Name = "groupBoxTextProperties";
            groupBoxTextProperties.Padding = new Padding(4, 3, 4, 3);
            groupBoxTextProperties.Size = new Size(338, 104);
            groupBoxTextProperties.TabIndex = 0;
            groupBoxTextProperties.TabStop = false;
            groupBoxTextProperties.Text = "🔤 Yazı Özellikleri";
            // 
            // buttonTextColor
            // 
            buttonTextColor.BackColor = Color.White;
            buttonTextColor.Cursor = Cursors.Hand;
            buttonTextColor.FlatStyle = FlatStyle.Flat;
            buttonTextColor.Location = new Point(257, 58);
            buttonTextColor.Margin = new Padding(4, 3, 4, 3);
            buttonTextColor.Name = "buttonTextColor";
            buttonTextColor.Size = new Size(70, 27);
            buttonTextColor.TabIndex = 5;
            buttonTextColor.UseVisualStyleBackColor = false;
            buttonTextColor.Click += buttonTextColor_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.ForeColor = Color.White;
            label10.Location = new Point(257, 39);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(51, 15);
            label10.TabIndex = 4;
            label10.Text = "🎨 Renk:";
            // 
            // comboBoxFont
            // 
            comboBoxFont.BackColor = Color.FromArgb(60, 60, 60);
            comboBoxFont.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxFont.ForeColor = Color.White;
            comboBoxFont.FormattingEnabled = true;
            comboBoxFont.Location = new Point(105, 58);
            comboBoxFont.Margin = new Padding(4, 3, 4, 3);
            comboBoxFont.Name = "comboBoxFont";
            comboBoxFont.Size = new Size(139, 23);
            comboBoxFont.TabIndex = 3;
            comboBoxFont.SelectedIndexChanged += comboBoxFont_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.ForeColor = Color.White;
            label9.Location = new Point(105, 39);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(49, 15);
            label9.TabIndex = 2;
            label9.Text = "🔤 Font:";
            // 
            // numericFontSize
            // 
            numericFontSize.BackColor = Color.FromArgb(60, 60, 60);
            numericFontSize.ForeColor = Color.White;
            numericFontSize.Location = new Point(12, 58);
            numericFontSize.Margin = new Padding(4, 3, 4, 3);
            numericFontSize.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numericFontSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numericFontSize.Name = "numericFontSize";
            numericFontSize.Size = new Size(82, 23);
            numericFontSize.TabIndex = 1;
            numericFontSize.Value = new decimal(new int[] { 24, 0, 0, 0 });
            numericFontSize.ValueChanged += numericFontSize_ValueChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.ForeColor = Color.White;
            label8.Location = new Point(12, 39);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(56, 15);
            label8.TabIndex = 0;
            label8.Text = "📏 Boyut:";
            // 
            // tabPageMerge
            // 
            tabPageMerge.BackColor = Color.FromArgb(45, 45, 48);
            tabPageMerge.Controls.Add(splitContainer2);
            tabPageMerge.Location = new Point(4, 24);
            tabPageMerge.Margin = new Padding(4, 3, 4, 3);
            tabPageMerge.Name = "tabPageMerge";
            tabPageMerge.Padding = new Padding(4, 3, 4, 3);
            tabPageMerge.Size = new Size(1362, 232);
            tabPageMerge.TabIndex = 2;
            tabPageMerge.Text = "🔗 Video Birleştirme";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(4, 3);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBoxVideoList);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBoxMergeOptions);
            splitContainer2.Size = new Size(1354, 226);
            splitContainer2.SplitterDistance = 700;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // groupBoxVideoList
            // 
            groupBoxVideoList.Controls.Add(listBoxVideos);
            groupBoxVideoList.Controls.Add(panelVideoButtons);
            groupBoxVideoList.Dock = DockStyle.Fill;
            groupBoxVideoList.ForeColor = Color.White;
            groupBoxVideoList.Location = new Point(0, 0);
            groupBoxVideoList.Margin = new Padding(4, 3, 4, 3);
            groupBoxVideoList.Name = "groupBoxVideoList";
            groupBoxVideoList.Padding = new Padding(4, 3, 4, 3);
            groupBoxVideoList.Size = new Size(700, 226);
            groupBoxVideoList.TabIndex = 0;
            groupBoxVideoList.TabStop = false;
            groupBoxVideoList.Text = "📋 Video Listesi (Sırayla Birleştirilir)";
            // 
            // listBoxVideos
            // 
            listBoxVideos.BackColor = Color.FromArgb(60, 60, 60);
            listBoxVideos.BorderStyle = BorderStyle.None;
            listBoxVideos.Dock = DockStyle.Fill;
            listBoxVideos.ForeColor = Color.White;
            listBoxVideos.FormattingEnabled = true;
            listBoxVideos.Location = new Point(4, 54);
            listBoxVideos.Margin = new Padding(4, 3, 4, 3);
            listBoxVideos.Name = "listBoxVideos";
            listBoxVideos.Size = new Size(692, 169);
            listBoxVideos.TabIndex = 1;
            // 
            // panelVideoButtons
            // 
            panelVideoButtons.Controls.Add(btnMoveVideoDown);
            panelVideoButtons.Controls.Add(btnMoveVideoUp);
            panelVideoButtons.Controls.Add(btnRemoveVideo);
            panelVideoButtons.Controls.Add(btnAddVideo);
            panelVideoButtons.Dock = DockStyle.Top;
            panelVideoButtons.Location = new Point(4, 19);
            panelVideoButtons.Margin = new Padding(4, 3, 4, 3);
            panelVideoButtons.Name = "panelVideoButtons";
            panelVideoButtons.Size = new Size(692, 35);
            panelVideoButtons.TabIndex = 0;
            // 
            // btnMoveVideoDown
            // 
            btnMoveVideoDown.BackColor = Color.Transparent;
            btnMoveVideoDown.BorderColor = Color.FromArgb(128, 128, 128);
            btnMoveVideoDown.BorderThickness = 1;
            btnMoveVideoDown.ButtonBackColor = Color.FromArgb(128, 128, 128);
            btnMoveVideoDown.CornerRadius = 5;
            btnMoveVideoDown.Cursor = Cursors.Hand;
            btnMoveVideoDown.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnMoveVideoDown.HoverColor = Color.FromArgb(100, 100, 100);
            btnMoveVideoDown.ImagePadding = new Padding(0);
            btnMoveVideoDown.Location = new Point(308, 3);
            btnMoveVideoDown.Margin = new Padding(4, 3, 4, 3);
            btnMoveVideoDown.Name = "btnMoveVideoDown";
            btnMoveVideoDown.PressedColor = Color.FromArgb(80, 80, 80);
            btnMoveVideoDown.Size = new Size(93, 28);
            btnMoveVideoDown.TabIndex = 3;
            btnMoveVideoDown.Text = "⬇️ Aşağı";
            btnMoveVideoDown.Click += btnMoveVideoDown_Click;
            // 
            // btnMoveVideoUp
            // 
            btnMoveVideoUp.BackColor = Color.Transparent;
            btnMoveVideoUp.BorderColor = Color.FromArgb(128, 128, 128);
            btnMoveVideoUp.BorderThickness = 1;
            btnMoveVideoUp.ButtonBackColor = Color.FromArgb(128, 128, 128);
            btnMoveVideoUp.CornerRadius = 5;
            btnMoveVideoUp.Cursor = Cursors.Hand;
            btnMoveVideoUp.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnMoveVideoUp.HoverColor = Color.FromArgb(100, 100, 100);
            btnMoveVideoUp.ImagePadding = new Padding(0);
            btnMoveVideoUp.Location = new Point(208, 3);
            btnMoveVideoUp.Margin = new Padding(4, 3, 4, 3);
            btnMoveVideoUp.Name = "btnMoveVideoUp";
            btnMoveVideoUp.PressedColor = Color.FromArgb(80, 80, 80);
            btnMoveVideoUp.Size = new Size(93, 28);
            btnMoveVideoUp.TabIndex = 2;
            btnMoveVideoUp.Text = "⬆️ Yukarı";
            btnMoveVideoUp.Click += btnMoveVideoUp_Click;
            // 
            // btnRemoveVideo
            // 
            btnRemoveVideo.BackColor = Color.Transparent;
            btnRemoveVideo.BorderColor = Color.FromArgb(204, 0, 0);
            btnRemoveVideo.BorderThickness = 1;
            btnRemoveVideo.ButtonBackColor = Color.FromArgb(204, 0, 0);
            btnRemoveVideo.CornerRadius = 5;
            btnRemoveVideo.Cursor = Cursors.Hand;
            btnRemoveVideo.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnRemoveVideo.HoverColor = Color.FromArgb(180, 0, 0);
            btnRemoveVideo.ImagePadding = new Padding(0);
            btnRemoveVideo.Location = new Point(107, 3);
            btnRemoveVideo.Margin = new Padding(4, 3, 4, 3);
            btnRemoveVideo.Name = "btnRemoveVideo";
            btnRemoveVideo.PressedColor = Color.FromArgb(160, 0, 0);
            btnRemoveVideo.Size = new Size(93, 28);
            btnRemoveVideo.TabIndex = 1;
            btnRemoveVideo.Text = "🗑️ Kaldır";
            btnRemoveVideo.Click += btnRemoveVideo_Click;
            // 
            // btnAddVideo
            // 
            btnAddVideo.BackColor = Color.Transparent;
            btnAddVideo.BorderColor = Color.FromArgb(0, 122, 204);
            btnAddVideo.BorderThickness = 1;
            btnAddVideo.ButtonBackColor = Color.FromArgb(0, 122, 204);
            btnAddVideo.CornerRadius = 5;
            btnAddVideo.Cursor = Cursors.Hand;
            btnAddVideo.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnAddVideo.HoverColor = Color.FromArgb(0, 100, 180);
            btnAddVideo.ImagePadding = new Padding(0);
            btnAddVideo.Location = new Point(7, 3);
            btnAddVideo.Margin = new Padding(4, 3, 4, 3);
            btnAddVideo.Name = "btnAddVideo";
            btnAddVideo.PressedColor = Color.FromArgb(0, 80, 160);
            btnAddVideo.Size = new Size(93, 28);
            btnAddVideo.TabIndex = 0;
            btnAddVideo.Text = "📥 Video Ekle";
            btnAddVideo.Click += btnAddVideo_Click;
            // 
            // groupBoxMergeOptions
            // 
            groupBoxMergeOptions.Controls.Add(lblMergeInfo);
            groupBoxMergeOptions.Controls.Add(btnMergeVideos);
            groupBoxMergeOptions.Dock = DockStyle.Fill;
            groupBoxMergeOptions.ForeColor = Color.White;
            groupBoxMergeOptions.Location = new Point(0, 0);
            groupBoxMergeOptions.Margin = new Padding(4, 3, 4, 3);
            groupBoxMergeOptions.Name = "groupBoxMergeOptions";
            groupBoxMergeOptions.Padding = new Padding(4, 3, 4, 3);
            groupBoxMergeOptions.Size = new Size(649, 226);
            groupBoxMergeOptions.TabIndex = 0;
            groupBoxMergeOptions.TabStop = false;
            groupBoxMergeOptions.Text = "⚙️ Birleştirme Seçenekleri";
            // 
            // lblMergeInfo
            // 
            lblMergeInfo.Font = new Font("Segoe UI", 9F);
            lblMergeInfo.ForeColor = Color.LightGray;
            lblMergeInfo.Location = new Point(18, 29);
            lblMergeInfo.Margin = new Padding(4, 0, 4, 0);
            lblMergeInfo.Name = "lblMergeInfo";
            lblMergeInfo.Size = new Size(525, 81);
            lblMergeInfo.TabIndex = 1;
            lblMergeInfo.Text = resources.GetString("lblMergeInfo.Text");
            // 
            // btnMergeVideos
            // 
            btnMergeVideos.BackColor = Color.Transparent;
            btnMergeVideos.BorderColor = Color.FromArgb(255, 140, 0);
            btnMergeVideos.BorderThickness = 2;
            btnMergeVideos.ButtonBackColor = Color.FromArgb(255, 140, 0);
            btnMergeVideos.CornerRadius = 8;
            btnMergeVideos.Cursor = Cursors.Hand;
            btnMergeVideos.Enabled = false;
            btnMergeVideos.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMergeVideos.HoverColor = Color.FromArgb(235, 120, 0);
            btnMergeVideos.ImagePadding = new Padding(0);
            btnMergeVideos.Location = new Point(21, 121);
            btnMergeVideos.Margin = new Padding(4, 3, 4, 3);
            btnMergeVideos.Name = "btnMergeVideos";
            btnMergeVideos.PressedColor = Color.FromArgb(215, 100, 0);
            btnMergeVideos.Size = new Size(175, 40);
            btnMergeVideos.TabIndex = 0;
            btnMergeVideos.Text = "🔗 Birleştir";
            btnMergeVideos.Click += btnMergeVideos_Click;
            // 
            // panelTimeline
            // 
            panelTimeline.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelTimeline.BackColor = Color.Gray;
            panelTimeline.BorderStyle = BorderStyle.FixedSingle;
            panelTimeline.Location = new Point(15, 413);
            panelTimeline.Margin = new Padding(4, 3, 4, 3);
            panelTimeline.Name = "panelTimeline";
            panelTimeline.Size = new Size(1372, 50);
            panelTimeline.TabIndex = 2;
            panelTimeline.Paint += panelTimeline_Paint;
            panelTimeline.MouseDown += panelTimeline_MouseDown;
            panelTimeline.MouseMove += panelTimeline_MouseMove;
            panelTimeline.MouseUp += panelTimeline_MouseUp;
            // 
            // panelMain
            // 
            panelMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelMain.BackColor = Color.Black;
            panelMain.BorderStyle = BorderStyle.FixedSingle;
            panelMain.Controls.Add(vlcControl);
            panelMain.Location = new Point(14, 14);
            panelMain.Margin = new Padding(4, 3, 4, 3);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1372, 393);
            panelMain.TabIndex = 0;
            // 
            // EditVideoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(1400, 762);
            Controls.Add(panelTimeline);
            Controls.Add(panelControls);
            Controls.Add(panelMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(1397, 756);
            Name = "EditVideoForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🎬 Video Editörü";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)vlcControl).EndInit();
            panelControls.ResumeLayout(false);
            tabControlFeatures.ResumeLayout(false);
            tabPageBasic.ResumeLayout(false);
            tabPageBasic.PerformLayout();
            panelVolumeControl.ResumeLayout(false);
            panelVolumeControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolume).EndInit();
            tabPageOverlays.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBoxOverlayList.ResumeLayout(false);
            panelOverlayButtons.ResumeLayout(false);
            groupBoxOverlayEdit.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericStartTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericEndTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericHeight).EndInit();
            groupBoxTextProperties.ResumeLayout(false);
            groupBoxTextProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericFontSize).EndInit();
            tabPageMerge.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBoxVideoList.ResumeLayout(false);
            panelVideoButtons.ResumeLayout(false);
            groupBoxMergeOptions.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private LibVLCSharp.WinForms.VideoView vlcControl;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.TabControl tabControlFeatures;
        private System.Windows.Forms.TabPage tabPageBasic;
        private System.Windows.Forms.Panel panelVolumeControl;
        private System.Windows.Forms.Label lblVolumeValue;
        private System.Windows.Forms.TrackBar trackBarVolume;
        private System.Windows.Forms.Label lblVolume;
        private System.Windows.Forms.Label lblDetailedInfo;
        private System.Windows.Forms.Label lblFileInfo;
        private System.Windows.Forms.Label lblShortcuts;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblTrimInfo;
        private CustomButton btnExport;
        private CustomButton btnTrimVideo;
        private CustomButton btnSetEnd;
        private CustomButton btnSetStart;
        private System.Windows.Forms.Label lblCurrentTime;
        private System.Windows.Forms.Label lblDuration;
        private CustomButton btnStop;
        private CustomButton btnPause;
        private CustomButton btnPlay;
        private System.Windows.Forms.Label lblVideoPath;
        private CustomButton btnOpenVideo;
        private System.Windows.Forms.TabPage tabPageOverlays;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBoxOverlayList;
        private System.Windows.Forms.ListBox listBoxOverlays;
        private System.Windows.Forms.Panel panelOverlayButtons;
        private CustomButton btnToggleOverlayEdit;
        private CustomButton btnRemoveOverlay;
        private CustomButton btnAddText;
        private CustomButton btnAddImage;
        private System.Windows.Forms.GroupBox groupBoxOverlayEdit;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericStartTime;
        private System.Windows.Forms.NumericUpDown numericEndTime;
        private System.Windows.Forms.NumericUpDown numericX;
        private System.Windows.Forms.NumericUpDown numericY;
        private System.Windows.Forms.NumericUpDown numericWidth;
        private System.Windows.Forms.NumericUpDown numericHeight;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxContent;
        private System.Windows.Forms.GroupBox groupBoxTextProperties;
        private System.Windows.Forms.Button buttonTextColor;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBoxFont;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericFontSize;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPageMerge;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBoxVideoList;
        private System.Windows.Forms.ListBox listBoxVideos;
        private System.Windows.Forms.Panel panelVideoButtons;
        private CustomButton btnMoveVideoDown;
        private CustomButton btnMoveVideoUp;
        private CustomButton btnRemoveVideo;
        private CustomButton btnAddVideo;
        private System.Windows.Forms.GroupBox groupBoxMergeOptions;
        private System.Windows.Forms.Label lblMergeInfo;
        private CustomButton btnMergeVideos;
        private System.Windows.Forms.Panel panelTimeline;
        private Panel panelMain;
    }
}