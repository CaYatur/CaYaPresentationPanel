using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Lame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SoundPanel
{
    public partial class AudioMergeForm : Form
    {
        private List<string> audioFiles = new List<string>();
        private bool isDarkMode = true;

        public AudioMergeForm()
        {
            InitializeComponent();
            SetupDarkTheme();
            UpdateUI();
        }

        private void SetupDarkTheme()
        {
            BackColor = Color.FromArgb(25, 25, 25);
            ForeColor = Color.White;

            fileListBox.BackColor = Color.FromArgb(35, 35, 35);
            fileListBox.ForeColor = Color.White;

            // Button renklerini ayarla
            Color buttonBg = Color.FromArgb(45, 45, 45);
            Color buttonBorder = Color.FromArgb(80, 80, 80);

            addFileButton.ButtonBackColor = buttonBg;
            addFileButton.BorderColor = buttonBorder;
            addFileButton.TextColor = Color.White;

            removeFileButton.ButtonBackColor = buttonBg;
            removeFileButton.BorderColor = buttonBorder;
            removeFileButton.TextColor = Color.White;

            moveUpButton.ButtonBackColor = buttonBg;
            moveUpButton.BorderColor = buttonBorder;
            moveUpButton.TextColor = Color.White;

            moveDownButton.ButtonBackColor = buttonBg;
            moveDownButton.BorderColor = buttonBorder;
            moveDownButton.TextColor = Color.White;

            mergeButton.ButtonBackColor = Color.FromArgb(120, 80, 200);
            mergeButton.BorderColor = Color.FromArgb(180, 80, 220);
            mergeButton.TextColor = Color.White;

            clearAllButton.ButtonBackColor = Color.FromArgb(180, 60, 60);
            clearAllButton.BorderColor = Color.FromArgb(220, 80, 80);
            clearAllButton.TextColor = Color.White;
        }

        private void addFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Audio Files|*.wav;*.mp3;*.flac;*.aac;*.m4a;*.wma|All Files|*.*";
                ofd.Title = "Birleştirilecek Ses Dosyalarını Seçin";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string fileName in ofd.FileNames)
                    {
                        if (!audioFiles.Contains(fileName))
                        {
                            audioFiles.Add(fileName);
                            fileListBox.Items.Add(Path.GetFileName(fileName));
                        }
                    }
                    UpdateUI();
                }
            }
        }

        private void removeFileButton_Click(object sender, EventArgs e)
        {
            if (fileListBox.SelectedIndex >= 0)
            {
                int index = fileListBox.SelectedIndex;
                audioFiles.RemoveAt(index);
                fileListBox.Items.RemoveAt(index);
                UpdateUI();
            }
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            int index = fileListBox.SelectedIndex;
            if (index > 0)
            {
                // Swap files
                string temp = audioFiles[index];
                audioFiles[index] = audioFiles[index - 1];
                audioFiles[index - 1] = temp;

                // Swap listbox items
                object tempItem = fileListBox.Items[index];
                fileListBox.Items[index] = fileListBox.Items[index - 1];
                fileListBox.Items[index - 1] = tempItem;

                fileListBox.SelectedIndex = index - 1;
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            int index = fileListBox.SelectedIndex;
            if (index >= 0 && index < audioFiles.Count - 1)
            {
                // Swap files
                string temp = audioFiles[index];
                audioFiles[index] = audioFiles[index + 1];
                audioFiles[index + 1] = temp;

                // Swap listbox items
                object tempItem = fileListBox.Items[index];
                fileListBox.Items[index] = fileListBox.Items[index + 1];
                fileListBox.Items[index + 1] = tempItem;

                fileListBox.SelectedIndex = index + 1;
            }
        }

        private void clearAllButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Tüm dosyaları listeden kaldırmak istediğinizden emin misiniz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                audioFiles.Clear();
                fileListBox.Items.Clear();
                UpdateUI();
            }
        }

        private async void mergeButton_Click(object sender, EventArgs e)
        {
            if (audioFiles.Count < 2)
            {
                MessageBox.Show("En az 2 ses dosyası seçmelisiniz!", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "WAV Files|*.wav|MP3 Files|*.mp3";
                sfd.Title = "Birleştirilmiş Dosyayı Kaydet";
                sfd.FileName = "merged_audio";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Progress göster
                        progressBar.Visible = true;
                        progressBar.Value = 0;
                        statusLabel.Text = "Ses dosyaları birleştiriliyor...";
                        statusLabel.Visible = true;

                        await Task.Run(() => MergeAudioFiles(sfd.FileName));

                        progressBar.Visible = false;
                        statusLabel.Visible = false;

                        MessageBox.Show("Ses dosyaları başarıyla birleştirildi!", "Başarılı",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        progressBar.Visible = false;
                        statusLabel.Visible = false;
                        MessageBox.Show($"Birleştirme hatası: {ex.Message}", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MergeAudioFiles(string outputPath)
        {
            List<ISampleProvider> audioProviders = new List<ISampleProvider>();
            WaveFormat outputFormat = null;

            // Tüm ses dosyalarını yükle
            for (int i = 0; i < audioFiles.Count; i++)
            {
                try
                {
                    var audioFile = new AudioFileReader(audioFiles[i]);

                    // İlk dosyanın formatını kullan
                    if (outputFormat == null)
                    {
                        outputFormat = audioFile.WaveFormat;
                    }

                    // Formatı standardize et
                    ISampleProvider sampleProvider = audioFile.ToSampleProvider();

                    // Mono'ya çevir
                    if (sampleProvider.WaveFormat.Channels > 1)
                    {
                        sampleProvider = sampleProvider.ToMono();
                    }

                    // Resample if needed
                    if (sampleProvider.WaveFormat.SampleRate != 44100)
                    {
                        sampleProvider = new WdlResamplingSampleProvider(sampleProvider, 44100);
                    }

                    audioProviders.Add(sampleProvider);

                    // Progress güncelle
                    int progress = (i * 50) / audioFiles.Count;
                    this.Invoke(new Action(() => progressBar.Value = progress));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Dosya yüklenirken hata ({Path.GetFileName(audioFiles[i])}): {ex.Message}");
                }
            }

            // Dosyaları birleştir
            var concatenated = new ConcatenatingSampleProvider(audioProviders);

            // Çıktı formatı ayarla
            var outputWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

            try
            {
                string extension = Path.GetExtension(outputPath).ToLower();

                if (extension == ".wav")
                {
                    // WAV olarak kaydet
                    WaveFileWriter.CreateWaveFile16(outputPath, concatenated);
                }
                else if (extension == ".mp3")
                {
                    // MP3 olarak kaydet (geçici WAV oluştur)
                    string tempWav = Path.GetTempFileName().Replace(".tmp", ".wav");
                    WaveFileWriter.CreateWaveFile16(tempWav, concatenated);

                    using (var reader = new AudioFileReader(tempWav))
                    {
                        using (var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, 128))
                        {
                            reader.CopyTo(writer);
                        }
                    }

                    File.Delete(tempWav);
                }

                this.Invoke(new Action(() => progressBar.Value = 100));
            }
            catch (Exception ex)
            {
                throw new Exception($"Kaydetme hatası: {ex.Message}");
            }
            finally
            {
                // Cleanup
                foreach (var provider in audioProviders)
                {
                    if (provider is AudioFileReader reader)
                    {
                        reader.Dispose();
                    }
                }
            }
        }

        private void UpdateUI()
        {
            bool hasFiles = audioFiles.Count > 0;
            bool hasSelection = fileListBox.SelectedIndex >= 0;
            bool canMerge = audioFiles.Count >= 2;

            removeFileButton.Enabled = hasSelection;
            moveUpButton.Enabled = hasSelection && fileListBox.SelectedIndex > 0;
            moveDownButton.Enabled = hasSelection && fileListBox.SelectedIndex < audioFiles.Count - 1;
            clearAllButton.Enabled = hasFiles;
            mergeButton.Enabled = canMerge;

            fileCountLabel.Text = $"Toplam Dosya: {audioFiles.Count}";
        }

        private void fileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void fileListBox_DoubleClick(object sender, EventArgs e)
        {
            if (fileListBox.SelectedIndex >= 0)
            {
                string filePath = audioFiles[fileListBox.SelectedIndex];
                string info = GetAudioFileInfo(filePath);
                MessageBox.Show(info, "Dosya Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetAudioFileInfo(string filePath)
        {
            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                {
                    TimeSpan duration = audioFile.TotalTime;
                    WaveFormat format = audioFile.WaveFormat;
                    FileInfo fileInfo = new FileInfo(filePath);

                    return $"Dosya: {Path.GetFileName(filePath)}\n" +
                           $"Süre: {duration:mm\\:ss}\n" +
                           $"Sample Rate: {format.SampleRate} Hz\n" +
                           $"Kanal: {format.Channels}\n" +
                           $"Bit Depth: {format.BitsPerSample} bit\n" +
                           $"Boyut: {fileInfo.Length / 1024 / 1024:F2} MB";
                }
            }
            catch (Exception ex)
            {
                return $"Dosya bilgisi alınamadı: {ex.Message}";
            }
        }
    }
}