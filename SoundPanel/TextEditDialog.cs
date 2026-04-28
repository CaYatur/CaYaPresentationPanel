using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class TextEditDialog : Form
    {
        public string Text { get; private set; }
        public string FontName { get; private set; }
        public int FontSize { get; private set; }
        public Color TextColor { get; private set; }

        private TextBox textBoxText;
        private ComboBox comboBoxFont;
        private NumericUpDown numericFontSize;
        private Button buttonColor;
        private Button buttonOK;
        private Button buttonCancel;

        public TextEditDialog(string text, string fontName, int fontSize, Color textColor)
        {
            InitializeComponent();

            Text = text;
            FontName = fontName;
            FontSize = fontSize;
            TextColor = textColor;

            SetupControls();
            LoadValues();
        }

        private void InitializeComponent()
        {
            this.textBoxText = new TextBox();
            this.comboBoxFont = new ComboBox();
            this.numericFontSize = new NumericUpDown();
            this.buttonColor = new Button();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.numericFontSize)).BeginInit();
            this.SuspendLayout();

            // textBoxText
            this.textBoxText.Location = new Point(15, 30);
            this.textBoxText.Multiline = true;
            this.textBoxText.Size = new Size(360, 80);
            this.textBoxText.ScrollBars = ScrollBars.Vertical;

            // comboBoxFont
            this.comboBoxFont.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxFont.Location = new Point(15, 130);
            this.comboBoxFont.Size = new Size(150, 21);

            // numericFontSize
            this.numericFontSize.Location = new Point(180, 130);
            this.numericFontSize.Minimum = 8;
            this.numericFontSize.Maximum = 200;
            this.numericFontSize.Size = new Size(60, 20);

            // buttonColor
            this.buttonColor.Location = new Point(255, 130);
            this.buttonColor.Size = new Size(60, 23);
            this.buttonColor.Text = "Renk";
            this.buttonColor.UseVisualStyleBackColor = false;
            this.buttonColor.Click += ButtonColor_Click;

            // buttonOK
            this.buttonOK.Location = new Point(220, 170);
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.Text = "Tamam";
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Click += ButtonOK_Click;

            // buttonCancel
            this.buttonCancel.Location = new Point(300, 170);
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.Text = "İptal";
            this.buttonCancel.DialogResult = DialogResult.Cancel;

            // Form
            this.ClientSize = new Size(390, 210);
            this.Text = "📝 Metin Düzenle";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Labels
            var lblText = new Label { Text = "📄 Metin:", Location = new Point(15, 10), Size = new Size(100, 15) };
            var lblFont = new Label { Text = "🔤 Font:", Location = new Point(15, 115), Size = new Size(40, 15) };
            var lblSize = new Label { Text = "📏 Boyut:", Location = new Point(180, 115), Size = new Size(50, 15) };

            this.Controls.AddRange(new Control[]
            {
                lblText, this.textBoxText,
                lblFont, this.comboBoxFont,
                lblSize, this.numericFontSize,
                this.buttonColor,
                this.buttonOK, this.buttonCancel
            });

            ((System.ComponentModel.ISupportInitialize)(this.numericFontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SetupControls()
        {
            // Font listesini doldur
            foreach (FontFamily font in FontFamily.Families)
            {
                comboBoxFont.Items.Add(font.Name);
            }
        }

        private void LoadValues()
        {
            textBoxText.Text = Text;
            comboBoxFont.SelectedItem = FontName;
            numericFontSize.Value = FontSize;
            buttonColor.BackColor = TextColor;
        }

        private void ButtonColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = buttonColor.BackColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    buttonColor.BackColor = cd.Color;
                }
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Text = textBoxText.Text;
            FontName = comboBoxFont.SelectedItem?.ToString() ?? "Arial";
            FontSize = (int)numericFontSize.Value;
            TextColor = buttonColor.BackColor;
        }
    }
}