namespace SoundPanel
{
    partial class MoreSection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoreSection));
            btnContentShow = new CustomButton();
            VirtualCableTransfer = new CustomButton();
            SuspendLayout();
            // 
            // btnContentShow
            // 
            btnContentShow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnContentShow.BackColor = Color.Transparent;
            btnContentShow.BorderThickness = 2;
            btnContentShow.ButtonBackColor = Color.FromArgb(37, 43, 63);
            btnContentShow.CornerRadius = 4;
            btnContentShow.DisabledBackColor = Color.FromArgb(24, 26, 34);
            btnContentShow.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            btnContentShow.DisabledTextColor = Color.FromArgb(90, 95, 110);
            btnContentShow.Font = new Font("Poppins", 9F, FontStyle.Bold);
            btnContentShow.HoverColor = Color.FromArgb(27, 31, 46);
            btnContentShow.ImagePadding = new Padding(0);
            btnContentShow.ImageScaleReduction = 10;
            btnContentShow.Location = new Point(12, 12);
            btnContentShow.Name = "btnContentShow";
            btnContentShow.PressedColor = Color.FromArgb(22, 25, 37);
            btnContentShow.Size = new Size(130, 70);
            btnContentShow.TabIndex = 27;
            btnContentShow.Text = "Ýçerik Gösterici";
            btnContentShow.TextBelowImage = true;
            btnContentShow.Click += btnContentShow_Click;
            // 
            // VirtualCableTransfer
            // 
            VirtualCableTransfer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            VirtualCableTransfer.BackColor = Color.Transparent;
            VirtualCableTransfer.BorderThickness = 2;
            VirtualCableTransfer.ButtonBackColor = Color.FromArgb(37, 43, 63);
            VirtualCableTransfer.CornerRadius = 4;
            VirtualCableTransfer.DisabledBackColor = Color.FromArgb(24, 26, 34);
            VirtualCableTransfer.DisabledBorderColor = Color.FromArgb(54, 60, 80);
            VirtualCableTransfer.DisabledTextColor = Color.FromArgb(90, 95, 110);
            VirtualCableTransfer.Font = new Font("Poppins", 9F, FontStyle.Bold);
            VirtualCableTransfer.HoverColor = Color.FromArgb(27, 31, 46);
            VirtualCableTransfer.ImagePadding = new Padding(0);
            VirtualCableTransfer.ImageScaleReduction = 10;
            VirtualCableTransfer.Location = new Point(148, 12);
            VirtualCableTransfer.Name = "VirtualCableTransfer";
            VirtualCableTransfer.PressedColor = Color.FromArgb(22, 25, 37);
            VirtualCableTransfer.Size = new Size(130, 70);
            VirtualCableTransfer.TabIndex = 28;
            VirtualCableTransfer.Text = "Sanal Ses";
            VirtualCableTransfer.TextBelowImage = true;
            VirtualCableTransfer.Click += VirtualCableTransfer_Click;
            // 
            // MoreSection
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(37, 43, 63);
            ClientSize = new Size(478, 250);
            Controls.Add(VirtualCableTransfer);
            Controls.Add(btnContentShow);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MoreSection";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Daha Fazlasý";
            ResumeLayout(false);
        }

        #endregion

        private CustomButton btnContentShow;
        private CustomButton VirtualCableTransfer;
    }
}