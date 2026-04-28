using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class TimingEditDialog : Form
    {
        public double StartTime { get; private set; }
        public double EndTime { get; private set; }

        private NumericUpDown numericStartTime;
        private NumericUpDown numericEndTime;
        private Button buttonOK;
        private Button buttonCancel;

        public TimingEditDialog(double startTime, double endTime)
        {
            InitializeComponent();

            StartTime = startTime;
            EndTime = endTime;

            LoadValues();
        }

        private void InitializeComponent()
        {
            this.numericStartTime = new NumericUpDown();
            this.numericEndTime = new NumericUpDown();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.numericStartTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericEndTime)).BeginInit();
            this.SuspendLayout();

            // numericStartTime
            this.numericStartTime.DecimalPlaces = 1;
            this.numericStartTime.Location = new Point(15, 30);
            this.numericStartTime.Maximum = 99999;
            this.numericStartTime.Size = new Size(100, 20);

            // numericEndTime
            this.numericEndTime.DecimalPlaces = 1;
            this.numericEndTime.Location = new Point(140, 30);
            this.numericEndTime.Maximum = 99999;
            this.numericEndTime.Size = new Size(100, 20);

            // buttonOK
            this.buttonOK.Location = new Point(85, 80);
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.Text = "Tamam";
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonCancel
            this.buttonCancel.Location = new Point(165, 80);
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.Text = "İptal";
            this.buttonCancel.DialogResult = DialogResult.Cancel;

            // Form
            this.ClientSize = new Size(255, 120);
            this.Text = "⏰ Zamanlama Düzenle";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Labels
            var lblStart = new Label { Text = "⏰ Başlangıç (s):", Location = new Point(15, 10), Size = new Size(100, 15) };
            var lblEnd = new Label { Text = "🏁 Bitiş (s):", Location = new Point(140, 10), Size = new Size(80, 15) };

            this.Controls.AddRange(new Control[]
            {
                lblStart, this.numericStartTime,
                lblEnd, this.numericEndTime,
                this.buttonOK, this.buttonCancel
            });

            ((System.ComponentModel.ISupportInitialize)(this.numericStartTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericEndTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadValues()
        {
            numericStartTime.Value = (decimal)StartTime;
            numericEndTime.Value = (decimal)EndTime;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            StartTime = (double)numericStartTime.Value;
            EndTime = (double)numericEndTime.Value;

            if (EndTime <= StartTime)
            {
                MessageBox.Show("Bitiş zamanı başlangıç zamanından büyük olmalıdır!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
    }
}