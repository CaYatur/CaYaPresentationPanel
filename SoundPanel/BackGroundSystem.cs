using System.Management;
using System;
using System.Management;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class BackGroundSystem : Form
    {
        private bool Canceled = false;
        public BackGroundSystem()
        {
            InitializeComponent();
        }

        // Buton tıklama ile dosya seçme işlemi başlatılacak
        private void btnSelectMedia_Click(object sender, EventArgs e)
        {
            // Varsayılan olarak desteklenen formatlar (sadece video / gif / resim)
            string supported = "*.mp4;*.mov;*.avi;*.flv;*.mkv;*.wmv;*.m4v;*.webm;*.gif;*.jpg;*.jpeg;*.png;*.bmp";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = $"All Supported|{supported}|Video Files|*.mp4;*.mov;*.avi;*.flv;*.mkv;*.wmv;*.m4v;*.webm|GIF Files|*.gif|Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                openFileDialog.Title = "Medya Seçin (Video / GIF / Resim)";
                openFileDialog.FilterIndex = 1; // Varsayılan: All Supported

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    ShowMedia(selectedFile);
                }
                else
                {
                    //this.Close(); // Kullanıcı iptal ederse formu kapat
                    Canceled = true;
                }
            }
        }

        // Dosya seçildikten sonra uygun şekilde gösterilmesi sağlanacak
        private void ShowMedia(string mediaPath)
        {
            string fileExtension = System.IO.Path.GetExtension(mediaPath).ToLower();

            // Video dosyası ise
            if (fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mkv")
            {
                ShowVideo(mediaPath);
            }
            // GIF dosyası ise
            else if (fileExtension == ".gif")
            {
                ShowGif(mediaPath);
            }
            // Resim dosyası ise
            else if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
            {
                ShowImage(mediaPath);
            }
        }

        // Video dosyasını göster
        private void ShowVideo(string videoPath)
        {
            VideoPCYBGS videoForm = new VideoPCYBGS(videoPath);
            videoForm.FormClosing += (sender, e) => { this.Close(); };  // Panel kapandığında ana formu kapat
            videoForm.Show();
        }

        // GIF dosyasını göster
        private void ShowGif(string gifPath)
        {
            // GIF'i ana formda bir PictureBox'ta göstereceğiz
            PictureBox pictureBox = new PictureBox();
            pictureBox.ImageLocation = gifPath;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Dock = DockStyle.Fill;
            this.Controls.Add(pictureBox);
        }

        // Resim dosyasını göster
        private void ShowImage(string imagePath)
        {
            // Resmi ana formda bir PictureBox'ta göstereceğiz
            PictureBox pictureBox = new PictureBox();
            pictureBox.ImageLocation = imagePath;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Dock = DockStyle.Fill;
            this.Controls.Add(pictureBox);
        }

        private async void BackGroundSystem_Load(object sender, EventArgs e)
        {


            try
            {
                // Aynı desteklenen formatları yükleme sırasında da varsayılan yapıyoruz
                string supported = "*.mp4;*.mov;*.avi;*.flv;*.mkv;*.wmv;*.m4v;*.webm;*.gif;*.jpg;*.jpeg;*.png;*.bmp";

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = $"All Supported|{supported}|Video Files|*.mp4;*.mov;*.avi;*.flv;*.mkv;*.wmv;*.m4v;*.webm|GIF Files|*.gif|Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                    openFileDialog.Title = "Video / GIF / Resim Seçin";
                    openFileDialog.FilterIndex = 1; // Varsayılan: All Supported

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        var selectedScreen = ShowScreenSelectionDialog();
                        if (selectedScreen != null)
                        {
                            OpenVideoPlayerOnScreen(selectedScreen, filePath);
                        }
                    }
                    else
                    {
                        Canceled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Canceled = true;
                MessageBox.Show("Bir hata meydana geldi: " + ex.Message);
            }





            //// Dosya seçme penceresi oluştur
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mkv|GIF Files|*.gif|Image Files|*.jpg;*.jpeg;*.png";

            //// Dosya seçildiyse
            //if (openFileDialog.ShowDialog() == DialogResult.OK)
            //{
            //    string selectedFile = openFileDialog.FileName;
            //    ShowMedia(selectedFile);
            //}


            //await Task.Delay(5000); // Küçük bir gecikme ekle

            if (Canceled == true)
            {
                this.Close(); // İptal edildiyse formu kapat
                //MessageBox.Show("İşlem iptal edildi."); 
            }
        }

        private async void OpenVideoPlayerOnScreen(Screen selectedScreen, string videoPath)
        {
            // VideoPCYBGS formunu seçilen ekranda tam ekran aç
            VideoPCYBGS videoPlayer = new VideoPCYBGS(videoPath);
            videoPlayer.StartPosition = FormStartPosition.Manual;
            videoPlayer.Bounds = selectedScreen.Bounds;  // Seçilen ekranın tüm alanını kullan
            videoPlayer.WindowState = FormWindowState.Maximized;  // Tam ekran yap
            videoPlayer.Show();
        }



        private string GetDisplayInfo(Screen screen)
        {
            // Varsayılan ekran bilgisi
            string displayInfo = screen.DeviceName;

            // Varsayılan ekranın tespiti
            if (screen.Equals(Screen.PrimaryScreen))
            {
                displayInfo += " (Varsayılan Ekran)";
            }

            // Ekran markasını ve modelini almak için WMI sorgusu
            string monitorInfo = GetMonitorModelInfo();
            if (!string.IsNullOrEmpty(monitorInfo))
            {
                displayInfo += " - " + monitorInfo;
            }

            return displayInfo;
        }

        private string GetMonitorModelInfo()
        {
            // Ekran markası ve modelini almak için WMI sorgusu
            string monitorModel = string.Empty;

            // WMI sorgusu ile monitörün model bilgisini al
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
            foreach (ManagementObject obj in searcher.Get())
            {
                monitorModel = obj["Caption"]?.ToString();
            }

            return monitorModel;
        }

        private void BackGroundSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            // VideoPCYBGS açık mı kontrol et
            if (VideoPCYBGS.Instance != null)
            {
                VideoPCYBGS.Instance.Close(); // Formu kapat
            }
        }

        private Screen? ShowScreenSelectionDialog()
        {
            using (var form = new ScreenSelectionForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.SelectedScreen;
                }
            }
            return null;
        }




    }
}
