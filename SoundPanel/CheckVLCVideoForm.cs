using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace SoundPanel
{
    public class CheckVLCVideoForm : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnCancel;
        private bool cancelled = false;

        public CheckVLCVideoForm()
        {
            InitializeComponents();
            Shown += CheckVLCVideoForm_Shown;
        }

        private void InitializeComponents()
        {
            Width = 480;
            Height = 140;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Text = "LibVLC Kontrolü ve Ýndirme";
            MaximizeBox = false;
            MinimizeBox = false;

            lblStatus = new Label()
            {
                Text = "Kontrol ediliyor...",
                AutoSize = false,
                Width = ClientSize.Width - 20,
                Height = 30,
                Top = 10,
                Left = 10
            };

            progressBar = new ProgressBar()
            {
                Style = ProgressBarStyle.Continuous,
                Width = ClientSize.Width - 20,
                Height = 20,
                Top = lblStatus.Bottom + 6,
                Left = 10
            };

            btnCancel = new Button()
            {
                Text = "Ýptal",
                Width = 80,
                Height = 26,
                Top = progressBar.Bottom + 8,
                Left = ClientSize.Width - 90
            };
            btnCancel.Click += (s, e) => { cancelled = true; btnCancel.Enabled = false; };

            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnCancel);
        }

        private async void CheckVLCVideoForm_Shown(object? sender, EventArgs e)
        {
            await Task.Yield();
            await CheckAndDownloadLibVLC();
        }

        private async Task CheckAndDownloadLibVLC()
        {
            try
            {
                string targetDir = Path.Combine(Application.StartupPath, "libvlc");

                // If already exists and contains files, open EditVideoForm
                if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    OpenEditor();
                    return;
                }

                // Not present - ask user to download
                var res = MessageBox.Show("LibVLC klasörü bulunamadý. Gerekli libvlc dosyalarýný internetten indirmek ister misiniz?", "LibVLC Bulunamadý", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res != DialogResult.Yes)
                {
                    MessageBox.Show("LibVLC yüklü deðil. Editör açýlamýyor.", "Ýþlem Ýptal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                lblStatus.Text = "Ýndiriliyor... Lütfen bekleyin";

                string zipUrl = "https://github.com/CaYatur/CaYaPresentationPanel/archive/2ae8d1a53d75748226f27a2453e49bab40c153bb.zip";
                string tempDir = Path.Combine(Path.GetTempPath(), "CheckVLCDownload");
                Directory.CreateDirectory(tempDir);
                string tempZip = Path.Combine(tempDir, "repo.zip");

                bool downloadOk = await DownloadFileAsync(zipUrl, tempZip);
                if (!downloadOk || cancelled)
                {
                    MessageBox.Show("Ýndirme baþarýsýz veya iptal edildi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                lblStatus.Text = "Arþiv açýlýyor...";

                string extractDir = Path.Combine(tempDir, "extracted");
                if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
                Directory.CreateDirectory(extractDir);

                ZipFile.ExtractToDirectory(tempZip, extractDir);

                // Repo root folder (name may contain commit hash), find libvlc folder inside
                var libDirs = Directory.GetDirectories(extractDir, "libvlc", SearchOption.AllDirectories);
                if (libDirs == null || libDirs.Length == 0)
                {
                    MessageBox.Show("Arþiv içinde libvlc klasörü bulunamadý.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanupTemp(tempDir);
                    Close();
                    return;
                }

                string foundLib = libDirs[0];
                lblStatus.Text = "Dosyalar kopyalanýyor...";

                // Ensure target dir exists
                Directory.CreateDirectory(targetDir);

                CopyDirectory(foundLib, targetDir, overwrite: true);

                // Verify
                if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    lblStatus.Text = "Kurulum tamamlandý.";
                    CleanupTemp(tempDir);
                    MessageBox.Show("LibVLC indirildi ve kuruldu.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OpenEditor();
                    return;
                }
                else
                {
                    MessageBox.Show("LibVLC kurulumu baþarýsýz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanupTemp(tempDir);
                    Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private async Task<bool> DownloadFileAsync(string url, string destinationPath)
        {
            try
            {
                using (var http = new HttpClient())
                using (var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!resp.IsSuccessStatusCode) return false;

                    var total = resp.Content.Headers.ContentLength ?? -1L;
                    using (var stream = await resp.Content.ReadAsStreamAsync())
                    using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[81920];
                        long totalRead = 0;
                        int read;
                        progressBar.Style = total <= 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;

                        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            if (cancelled) return false;
                            await fs.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (total > 0)
                            {
                                int percent = (int)((totalRead * 100) / total);
                                progressBar.Value = Math.Max(0, Math.Min(100, percent));
                                lblStatus.Text = $"Ýndiriliyor... {percent}%";
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir, bool overwrite = false)
        {
            foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var newDir = dirPath.Replace(sourceDir, targetDir);
                if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);
            }

            foreach (var filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                var newFile = filePath.Replace(sourceDir, targetDir);
                try
                {
                    File.Copy(filePath, newFile, overwrite);
                }
                catch { }
            }
        }

        private void CleanupTemp(string tempDir)
        {
            try
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
            catch { }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckVLCVideoForm));
            SuspendLayout();
            // 
            // CheckVLCVideoForm
            // 
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "CheckVLCVideoForm";
            ResumeLayout(false);

        }

        private void OpenEditor()
        {
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    var edit = new EditVideoForm();
                    edit.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"EditVideoForm açýlamadý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Close();
                }
            }));
        }
    }
}
