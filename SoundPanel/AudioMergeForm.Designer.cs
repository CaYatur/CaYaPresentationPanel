namespace SoundPanel
{
    partial class AudioMergeForm
    {
        private System.ComponentModel.IContainer components = null;
        private ListBox fileListBox;
        private CustomButton addFileButton;
        private CustomButton removeFileButton;
        private CustomButton moveUpButton;
        private CustomButton moveDownButton;
        private CustomButton mergeButton;
        private CustomButton clearAllButton;
        private Label fileCountLabel;
        private Label statusLabel;
        private ProgressBar progressBar;
        private Panel buttonPanel;
        private Panel listPanel;
        private Panel bottomPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioMergeForm));
            buttonPanel = new Panel();
            addFileButton = new CustomButton();
            removeFileButton = new CustomButton();
            moveUpButton = new CustomButton();
            moveDownButton = new CustomButton();
            clearAllButton = new CustomButton();
            listPanel = new Panel();
            fileListBox = new ListBox();
            bottomPanel = new Panel();
            mergeButton = new CustomButton();
            fileCountLabel = new Label();
            statusLabel = new Label();
            progressBar = new ProgressBar();
            buttonPanel.SuspendLayout();
            listPanel.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(addFileButton);
            buttonPanel.Controls.Add(removeFileButton);
            buttonPanel.Controls.Add(moveUpButton);
            buttonPanel.Controls.Add(moveDownButton);
            buttonPanel.Controls.Add(clearAllButton);
            buttonPanel.Dock = DockStyle.Right;
            buttonPanel.Location = new Point(468, 0);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(10);
            buttonPanel.Size = new Size(143, 381);
            buttonPanel.TabIndex = 1;
            // 
            // addFileButton
            // 
            addFileButton.BackColor = Color.Transparent;
            addFileButton.CornerRadius = 14;
            addFileButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            addFileButton.ImagePadding = new Padding(0);
            addFileButton.Location = new Point(10, 10);
            addFileButton.Name = "addFileButton";
            addFileButton.Size = new Size(120, 35);
            addFileButton.TabIndex = 0;
            addFileButton.Text = "+ Ekle";
            addFileButton.Click += addFileButton_Click;
            // 
            // removeFileButton
            // 
            removeFileButton.BackColor = Color.Transparent;
            removeFileButton.CornerRadius = 14;
            removeFileButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            removeFileButton.ImagePadding = new Padding(0);
            removeFileButton.Location = new Point(10, 55);
            removeFileButton.Name = "removeFileButton";
            removeFileButton.Size = new Size(120, 35);
            removeFileButton.TabIndex = 1;
            removeFileButton.Text = "- Kaldır";
            removeFileButton.Click += removeFileButton_Click;
            // 
            // moveUpButton
            // 
            moveUpButton.BackColor = Color.Transparent;
            moveUpButton.CornerRadius = 14;
            moveUpButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            moveUpButton.ImagePadding = new Padding(0);
            moveUpButton.Location = new Point(10, 100);
            moveUpButton.Name = "moveUpButton";
            moveUpButton.Size = new Size(120, 35);
            moveUpButton.TabIndex = 2;
            moveUpButton.Text = "⬆ Yukarı";
            moveUpButton.Click += moveUpButton_Click;
            // 
            // moveDownButton
            // 
            moveDownButton.BackColor = Color.Transparent;
            moveDownButton.CornerRadius = 14;
            moveDownButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            moveDownButton.ImagePadding = new Padding(0);
            moveDownButton.Location = new Point(10, 145);
            moveDownButton.Name = "moveDownButton";
            moveDownButton.Size = new Size(120, 35);
            moveDownButton.TabIndex = 3;
            moveDownButton.Text = "⬇ Aşağı";
            moveDownButton.Click += moveDownButton_Click;
            // 
            // clearAllButton
            // 
            clearAllButton.BackColor = Color.Transparent;
            clearAllButton.CornerRadius = 14;
            clearAllButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            clearAllButton.ImagePadding = new Padding(0);
            clearAllButton.Location = new Point(10, 190);
            clearAllButton.Name = "clearAllButton";
            clearAllButton.Size = new Size(120, 35);
            clearAllButton.TabIndex = 4;
            clearAllButton.Text = "Hepsini Sil";
            clearAllButton.Click += clearAllButton_Click;
            // 
            // listPanel
            // 
            listPanel.Controls.Add(fileListBox);
            listPanel.Dock = DockStyle.Fill;
            listPanel.Location = new Point(0, 0);
            listPanel.Name = "listPanel";
            listPanel.Padding = new Padding(10);
            listPanel.Size = new Size(468, 381);
            listPanel.TabIndex = 0;
            // 
            // fileListBox
            // 
            fileListBox.Dock = DockStyle.Fill;
            fileListBox.HorizontalScrollbar = true;
            fileListBox.Location = new Point(10, 10);
            fileListBox.Name = "fileListBox";
            fileListBox.Size = new Size(448, 361);
            fileListBox.TabIndex = 0;
            fileListBox.SelectedIndexChanged += fileListBox_SelectedIndexChanged;
            fileListBox.DoubleClick += fileListBox_DoubleClick;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(mergeButton);
            bottomPanel.Controls.Add(fileCountLabel);
            bottomPanel.Controls.Add(statusLabel);
            bottomPanel.Controls.Add(progressBar);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 381);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(10);
            bottomPanel.Size = new Size(611, 80);
            bottomPanel.TabIndex = 2;
            // 
            // mergeButton
            // 
            mergeButton.BackColor = Color.Transparent;
            mergeButton.CornerRadius = 14;
            mergeButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            mergeButton.ImagePadding = new Padding(0);
            mergeButton.Location = new Point(10, 10);
            mergeButton.Name = "mergeButton";
            mergeButton.Size = new Size(120, 40);
            mergeButton.TabIndex = 0;
            mergeButton.Text = "Birleştir";
            mergeButton.Click += mergeButton_Click;
            // 
            // fileCountLabel
            // 
            fileCountLabel.ForeColor = Color.White;
            fileCountLabel.Location = new Point(150, 20);
            fileCountLabel.Name = "fileCountLabel";
            fileCountLabel.Size = new Size(150, 20);
            fileCountLabel.TabIndex = 1;
            fileCountLabel.Text = "Toplam Dosya: 0";
            // 
            // statusLabel
            // 
            statusLabel.ForeColor = Color.White;
            statusLabel.Location = new Point(150, 45);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(300, 20);
            statusLabel.TabIndex = 2;
            statusLabel.Text = "Hazır";
            statusLabel.Visible = false;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(320, 15);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(200, 20);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 3;
            progressBar.Visible = false;
            // 
            // AudioMergeForm
            // 
            ClientSize = new Size(611, 461);
            Controls.Add(listPanel);
            Controls.Add(buttonPanel);
            Controls.Add(bottomPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimumSize = new Size(500, 400);
            Name = "AudioMergeForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ses Dosyası Birleştirici";
            buttonPanel.ResumeLayout(false);
            listPanel.ResumeLayout(false);
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}