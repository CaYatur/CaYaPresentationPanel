namespace SoundPanel
{
    partial class VirtualCableTransferForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VirtualCableTransferForm));
            cmbDevices = new ComboBox();
            btnStart = new Button();
            btnStop = new Button();
            lblStatus = new Label();
            lblInfo = new Label();
            lblCableWarning = new Label();
            SuspendLayout();
            // 
            // cmbDevices
            // 
            cmbDevices.BackColor = Color.FromArgb(40, 40, 40);
            cmbDevices.ForeColor = Color.White;
            cmbDevices.FormattingEnabled = true;
            cmbDevices.Location = new Point(14, 14);
            cmbDevices.Margin = new Padding(4, 3, 4, 3);
            cmbDevices.Name = "cmbDevices";
            cmbDevices.Size = new Size(233, 23);
            cmbDevices.TabIndex = 0;
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.FromArgb(45, 45, 45);
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(254, 12);
            btnStart.Margin = new Padding(4, 3, 4, 3);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(88, 27);
            btnStart.TabIndex = 1;
            btnStart.Text = "Başlat";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.FromArgb(45, 45, 45);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.ForeColor = Color.White;
            btnStop.Location = new Point(349, 12);
            btnStop.Margin = new Padding(4, 3, 4, 3);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(88, 27);
            btnStop.TabIndex = 2;
            btnStop.Text = "Durdur";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.White;
            lblStatus.Location = new Point(14, 46);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(47, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Durum:";
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = Color.FromArgb(255, 180, 80);
            lblInfo.Location = new Point(14, 70);
            lblInfo.Margin = new Padding(4, 0, 4, 0);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(349, 15);
            lblInfo.TabIndex = 4;
            lblInfo.Text = "Not: Bu özellik yalnızca Voice-Cable (Sanal Ses Kablosu) ile çalışır.";
            // 
            // lblCableWarning
            // 
            lblCableWarning.AutoSize = true;
            lblCableWarning.ForeColor = Color.FromArgb(255, 80, 80);
            lblCableWarning.Location = new Point(14, 90);
            lblCableWarning.Margin = new Padding(4, 0, 4, 0);
            lblCableWarning.Name = "lblCableWarning";
            lblCableWarning.Size = new Size(284, 15);
            lblCableWarning.TabIndex = 5;
            lblCableWarning.Text = "Pencereyi kapattığınızda ses aktarımı da sonlandırılır.";
            // 
            // VirtualCableTransferForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 25, 25);
            ClientSize = new Size(467, 120);
            Controls.Add(lblCableWarning);
            Controls.Add(lblInfo);
            Controls.Add(lblStatus);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Controls.Add(cmbDevices);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "VirtualCableTransferForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ses Aktarma Paneli";
            Load += VirtualCableTransferForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.ComboBox cmbDevices;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblStatus;

        #endregion

        private Label lblInfo;
        private Label lblCableWarning;
    }
}