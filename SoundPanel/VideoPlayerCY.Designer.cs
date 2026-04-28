namespace SoundPanel
{
    partial class VideoPlayerCY
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoPlayerCY));
            label1 = new Label();
            CloseBtnW = new CustomButton();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 24F, FontStyle.Regular, GraphicsUnit.Point, 162);
            label1.ForeColor = SystemColors.HotTrack;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(511, 45);
            label1.TabIndex = 0;
            label1.Text = "CaYaSystem: Oynatıcı hazırlanıyor...";
            // 
            // CloseBtnW
            // 
            CloseBtnW.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CloseBtnW.BackColor = Color.Transparent;
            CloseBtnW.BorderColor = Color.Silver;
            CloseBtnW.ButtonBackColor = Color.Transparent;
            CloseBtnW.CornerRadius = 4;
            CloseBtnW.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            CloseBtnW.ImagePadding = new Padding(0);
            CloseBtnW.Location = new Point(701, 9);
            CloseBtnW.Name = "CloseBtnW";
            CloseBtnW.PressedColor = Color.DimGray;
            CloseBtnW.Size = new Size(83, 48);
            CloseBtnW.TabIndex = 1;
            CloseBtnW.Text = "Kapat";
            CloseBtnW.Click += CloseBtnW_Click;
            // 
            // VideoPlayerCY
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(796, 459);
            Controls.Add(CloseBtnW);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "VideoPlayerCY";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "VideoPlayerCY";
            Load += VideoPlayerCY_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button button1;
        private CustomButton CloseBtnW;
    }
}