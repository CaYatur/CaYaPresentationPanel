namespace SoundPanel
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            modernTrackBar1 = new ModernTrackBar();
            SuspendLayout();
            // 
            // modernTrackBar1
            // 
            modernTrackBar1.BackColor = Color.Transparent;
            modernTrackBar1.BackgroundColor = Color.Orange;
            modernTrackBar1.BorderColor = Color.FromArgb(180, 80, 220);
            modernTrackBar1.BorderThickness = 5;
            modernTrackBar1.CornerRadius = 20;
            modernTrackBar1.Location = new Point(143, 96);
            modernTrackBar1.Maximum = 100;
            modernTrackBar1.Minimum = 0;
            modernTrackBar1.Name = "modernTrackBar1";
            modernTrackBar1.Orientation = Orientation.Vertical;
            modernTrackBar1.Size = new Size(43, 286);
            modernTrackBar1.TabIndex = 0;
            modernTrackBar1.ThumbBorderColor = Color.Red;
            modernTrackBar1.ThumbColor = Color.White;
            modernTrackBar1.TrackBackColor = Color.Yellow;
            modernTrackBar1.TrackProgressColor = Color.FromArgb(180, 80, 220);
            modernTrackBar1.Value = 50;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(modernTrackBar1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "TestForm";
            Text = "TestForm";
            Load += TestForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ModernTrackBar modernTrackBar1;
    }
}