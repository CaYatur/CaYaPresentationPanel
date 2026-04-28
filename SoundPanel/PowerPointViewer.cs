using System;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;

namespace SoundPanel
{
    public partial class PowerPointViewer : Form
    {
        public PowerPointViewer()
        {
            InitializeComponent();
            // Buton ve diğer bileşenler eklenmiş varsayılarak
            Button btnSelectPPT = new Button
            {
                Text = "PowerPoint Seç ve Oynat",
                Dock = DockStyle.Top
            };

            btnSelectPPT.Click += BtnSelectPPT_Click;
            Controls.Add(btnSelectPPT);
        }

        private void BtnSelectPPT_Click(object sender, EventArgs e)
        {
            // Open File Dialog to select PowerPoint file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PowerPoint Files|*.pptx;*.ppt",
                Title = "Bir PowerPoint Dosyası Seçin"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // PowerPoint dosyasını oynat
                PlayPowerPoint(filePath);
            }
        }

        private void PlayPowerPoint(string filePath)
        {
            // PowerPoint uygulamasını oluştur
            Microsoft.Office.Interop.PowerPoint.Application pptApplication = new Microsoft.Office.Interop.PowerPoint.Application();
            Presentation presentation = null;

            try
            {
                // Sunuyu aç
                presentation = pptApplication.Presentations.Open(filePath, WithWindow: MsoTriState.msoFalse);

                // Slayt gösterisini başlat
                presentation.SlideShowSettings.StartingSlide = 1;
                presentation.SlideShowSettings.EndingSlide = presentation.Slides.Count;
                presentation.SlideShowSettings.Run();

                // Sunu çalışırken bekle
                while (pptApplication.SlideShowWindows.Count >= 1)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                try
                {
                    // PowerPoint nesnelerini serbest bırak
                    if (presentation != null)
                    {

                        presentation.Close();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(presentation);
                    }
                    if (pptApplication != null)
                    {
                        pptApplication.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pptApplication);
                    }
                }
                catch
                {
                    Close();
                }
            }
        }

        private void PowerPointViewer_Load(object sender, EventArgs e)
        {
            Hide();
            // Open File Dialog to select PowerPoint file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PowerPoint Files|*.pptx;*.ppt",
                Title = "Bir PowerPoint Dosyası Seçin"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // PowerPoint dosyasını oynat
                PlayPowerPoint(filePath);
            }
        }
    }
}
