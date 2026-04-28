namespace SoundPanel
{
    partial class MediaEditMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MediaEditMenu));
            customButton1 = new CustomButton();
            customButton2 = new CustomButton();
            SuspendLayout();
            // 
            // customButton1
            // 
            customButton1.AllowTextWrapping = true;
            customButton1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            customButton1.BackColor = Color.Transparent;
            customButton1.ButtonBackColor = Color.FromArgb(201, 147, 229);
            customButton1.CornerRadius = 14;
            customButton1.DisabledBackColor = Color.FromArgb(156, 114, 178);
            customButton1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            customButton1.HoverColor = Color.FromArgb(156, 114, 178);
            customButton1.ImagePadding = new Padding(0);
            customButton1.Location = new Point(12, 12);
            customButton1.Name = "customButton1";
            customButton1.PressedColor = Color.FromArgb(88, 65, 101);
            customButton1.Size = new Size(165, 64);
            customButton1.TabIndex = 0;
            customButton1.Text = "Ses Düzenleyici";
            customButton1.Click += customButton1_Click;
            // 
            // customButton2
            // 
            customButton2.AllowTextWrapping = true;
            customButton2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            customButton2.BackColor = Color.Transparent;
            customButton2.ButtonBackColor = Color.FromArgb(244, 122, 143);
            customButton2.CornerRadius = 14;
            customButton2.DisabledBackColor = Color.FromArgb(193, 96, 113);
            customButton2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            customButton2.HoverColor = Color.FromArgb(193, 96, 113);
            customButton2.ImagePadding = new Padding(0);
            customButton2.Location = new Point(189, 12);
            customButton2.Name = "customButton2";
            customButton2.PressedColor = Color.FromArgb(116, 57, 68);
            customButton2.Size = new Size(165, 64);
            customButton2.TabIndex = 1;
            customButton2.Text = "Video Düzenleyici";
            customButton2.Click += customButton2_Click;
            // 
            // MediaEditMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 36, 51);
            ClientSize = new Size(366, 89);
            Controls.Add(customButton2);
            Controls.Add(customButton1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MediaEditMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bir Düzenleme Modu Seçin";
            ResumeLayout(false);
        }

        #endregion

        private CustomButton customButton1;
        private CustomButton customButton2;
    }
}