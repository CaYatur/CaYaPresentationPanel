namespace SoundPanel
{
    partial class SystemCheck
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemCheck));
            SuspendLayout();
            // 
            // SystemCheck
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(199, 151);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SystemCheck";
            Opacity = 0D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SystemCheck";
            FormClosing += SystemCheck_FormClosing;
            Load += SystemCheck_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}