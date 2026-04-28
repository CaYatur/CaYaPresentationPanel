using NAudio.Lame;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class EditSoundForm : Form
    {
        private WaveFileReader currentAudioFile;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;
        private System.Windows.Forms.Timer playbackTimer;
        private System.Windows.Forms.Timer waveformTimer;
        private bool isPlaying = false;
        private bool isDarkMode = true;
        private float currentPosition = 0;
        private float totalDuration = 0;
        private float startTrim = 0;
        private float endTrim = 0;
        private int zoomLevel = 1;
        private float[] audioData;
        private string currentFilePath = "";
        private string currentWorkingWavFile = ""; // Çalışma WAV dosyası
        private List<string> undoStack = new List<string>();
        private int undoIndex = -1;

        // Waveform çizimi için
        private Bitmap waveformBitmap;
        private Bitmap staticWaveformBitmap; // Statik waveform (playback çizgisi olmadan)
        private bool needsWaveformRedraw = true;
        private bool isDrawingWaveform = false;
        private float lastPlaybackPosition = -1;

        // Ön izleme için
        private bool showTrimPreview = false;

        // Ses işleme için
        private bool hasAudioModifications = false; // Audio data'da değişiklik var mı?

        public EditSoundForm()
        {
            InitializeComponent();
            InitializeAudio();
            SetupDarkTheme();
            SetupTimers();
            SetupKeyboardShortcuts();
            SetupTrimPreview();
        }

        private void InitializeAudio()
        {
            MediaFoundationApi.Startup();
            outputDevice = new WaveOutEvent();

            playbackTimer = new System.Windows.Forms.Timer();
            playbackTimer.Interval = 50; // 20 FPS için smooth update
            playbackTimer.Tick += PlaybackTimer_Tick;

            waveformTimer = new System.Windows.Forms.Timer();
            waveformTimer.Interval = 100;
            waveformTimer.Tick += WaveformTimer_Tick;
        }

        private void SetupTrimPreview()
        {
            // Trim trackbar'ları için event ekle
            trimStartTrackBar.ScrollStart += (s, e) => { showTrimPreview = true; needsWaveformRedraw = true; };
            trimStartTrackBar.ScrollEnd += (s, e) => { showTrimPreview = false; needsWaveformRedraw = true; };
            trimEndTrackBar.ScrollStart += (s, e) => { showTrimPreview = true; needsWaveformRedraw = true; };
            trimEndTrackBar.ScrollEnd += (s, e) => { showTrimPreview = false; needsWaveformRedraw = true; };

            // Ön izleme butonu ekle
            //previewTrimButton.Click += PreviewTrimButton_Click;
        }

        private void PreviewTrimButton_Click(object sender, EventArgs e)
        {
            showTrimPreview = !showTrimPreview;
            //previewTrimButton.Text = showTrimPreview ? "👁 Ön İzleme: AÇIK" : "👁 Ön İzleme";
            //previewTrimButton.ButtonBackColor = showTrimPreview ?
                //Color.FromArgb(0, 150, 100) : Color.FromArgb(45, 45, 45);
            needsWaveformRedraw = true;
        }

        private void SetupDarkTheme()
        {
            // Ana form renkleri
            BackColor = Color.FromArgb(25, 25, 25);
            ForeColor = Color.White;

            // Panel renkleri
            topPanel.BackColor = Color.FromArgb(35, 35, 35);
            waveformPanel.BackColor = Color.FromArgb(20, 20, 20);
            controlPanel.BackColor = Color.FromArgb(30, 30, 30);
            timelinePanel.BackColor = Color.FromArgb(40, 40, 40);

            // TrackBar'lar için renk ayarları
            //volumeTrackBar.BackgroundColor = Color.FromArgb(30, 30, 30);
            //volumeTrackBar.TrackBackColor = Color.FromArgb(60, 60, 60);
            //volumeTrackBar.TrackProgressColor = Color.FromArgb(120, 80, 200);
            //volumeTrackBar.ThumbColor = Color.White;
            //volumeTrackBar.BorderColor = Color.FromArgb(180, 80, 220);

            positionTrackBar.BackgroundColor = Color.FromArgb(30, 30, 30);
            positionTrackBar.TrackBackColor = Color.FromArgb(60, 60, 60);
            positionTrackBar.TrackProgressColor = Color.FromArgb(255, 140, 0);
            positionTrackBar.ThumbColor = Color.White;
            positionTrackBar.BorderColor = Color.FromArgb(255, 140, 0);

            trimStartTrackBar.BackgroundColor = Color.FromArgb(30, 30, 30);
            trimStartTrackBar.TrackBackColor = Color.FromArgb(60, 60, 60);
            trimStartTrackBar.TrackProgressColor = Color.FromArgb(0, 200, 100);
            trimStartTrackBar.ThumbColor = Color.White;
            trimStartTrackBar.BorderColor = Color.FromArgb(0, 200, 100);

            trimEndTrackBar.BackgroundColor = Color.FromArgb(30, 30, 30);
            trimEndTrackBar.TrackBackColor = Color.FromArgb(60, 60, 60);
            trimEndTrackBar.TrackProgressColor = Color.FromArgb(255, 80, 80);
            trimEndTrackBar.ThumbColor = Color.White;
            trimEndTrackBar.BorderColor = Color.FromArgb(255, 80, 80);

            // Button renkleri
            Color buttonBg = Color.FromArgb(45, 45, 45);
            Color buttonBorder = Color.FromArgb(80, 80, 80);

            foreach (Control control in GetAllControls(this).OfType<CustomButton>())
            {
                var btn = control as CustomButton;
                btn.ButtonBackColor = buttonBg;
                btn.BorderColor = buttonBorder;
                btn.TextColor = Color.White;
            }
        }

        private IEnumerable<Control> GetAllControls(Control container)
        {
            List<Control> controlList = new List<Control>();
            foreach (Control c in container.Controls)
            {
                controlList.AddRange(GetAllControls(c));
                controlList.Add(c);
            }
            return controlList;
        }

        private void SetupTimers()
        {
            playbackTimer.Tick += (s, e) =>
            {
                if (isPlaying && audioFileReader != null)
                {
                    float oldPosition = currentPosition;
                    currentPosition = (float)audioFileReader.CurrentTime.TotalSeconds;
                    positionTrackBar.Value = (int)(currentPosition * 1000);
                    UpdateTimeLabels();

                    // Playback pozisyonu değiştiyse güncelle
                    if (Math.Abs(currentPosition - oldPosition) > 0.02f) // 20ms threshold
                    {
                        UpdatePlaybackLineOnly();
                    }
                }
            };

            waveformTimer.Tick += (s, e) =>
            {
                if (needsWaveformRedraw && !isDrawingWaveform)
                {
                    DrawCompleteWaveform();
                    needsWaveformRedraw = false;
                }
            };
        }

        // Sadece playback çizgisini güncelleyen optimized fonksiyon
        private void UpdatePlaybackLineOnly()
        {
            if (staticWaveformBitmap == null || totalDuration <= 0 || isDrawingWaveform)
                return;

            try
            {
                // Ana bitmap'i static waveform'dan kopyala (temiz başlangıç)
                if (waveformBitmap?.Size != waveformPanel.Size)
                {
                    waveformBitmap?.Dispose();
                    waveformBitmap = new Bitmap(waveformPanel.Width, waveformPanel.Height);
                }

                using (Graphics g = Graphics.FromImage(waveformBitmap))
                {
                    // Önce temiz static waveform'u kopyala
                    g.DrawImage(staticWaveformBitmap, 0, 0);

                    // Şimdi sadece yeni playback çizgisini ekle
                    if (totalDuration > 0)
                    {
                        float playbackX = (currentPosition / totalDuration) * waveformPanel.Width;

                        using (Pen playbackPen = new Pen(Color.FromArgb(255, 140, 0), 3f))
                        {
                            g.DrawLine(playbackPen, playbackX, 0, playbackX, waveformPanel.Height);

                            // Üstte küçük üçgen
                            Point[] triangle = new Point[]
                            {
                                new Point((int)playbackX - 5, 0),
                                new Point((int)playbackX + 5, 0),
                                new Point((int)playbackX, 10)
                            };
                            using (SolidBrush triangleBrush = new SolidBrush(Color.FromArgb(255, 140, 0)))
                            {
                                g.FillPolygon(triangleBrush, triangle);
                            }
                        }
                    }
                }

                // Sadece playback alanını invalidate et
                waveformPanel.Invalidate();
            }
            catch (Exception ex)
            {
                // Hata durumunda full redraw
                System.Diagnostics.Debug.WriteLine($"UpdatePlaybackLineOnly Error: {ex.Message}");
                needsWaveformRedraw = true;
            }
        }

        private void SetupKeyboardShortcuts()
        {
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Space:
                        PlayPauseToggle();
                        e.Handled = true;
                        break;
                    case Keys.S when e.Control:
                        SaveAudio();
                        e.Handled = true;
                        break;
                    case Keys.O when e.Control:
                        OpenAudioFile();
                        e.Handled = true;
                        break;
                    case Keys.Z when e.Control:
                        Undo();
                        e.Handled = true;
                        break;
                    case Keys.Home:
                        SeekToPosition(0);
                        e.Handled = true;
                        break;
                    case Keys.End:
                        SeekToPosition(totalDuration);
                        e.Handled = true;
                        break;
                    case Keys.Left:
                        SeekToPosition(Math.Max(0, currentPosition - 1));
                        e.Handled = true;
                        break;
                    case Keys.Right:
                        SeekToPosition(Math.Min(totalDuration, currentPosition + 1));
                        e.Handled = true;
                        break;
                    case Keys.Oemplus:
                        ZoomIn();
                        e.Handled = true;
                        break;
                    case Keys.OemMinus:
                        ZoomOut();
                        e.Handled = true;
                        break;
                    case Keys.P when e.Control:
                        PreviewTrimButton_Click(null, null);
                        e.Handled = true;
                        break;
                }
            };
        }

        private void OpenAudioFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Audio Files|*.wav;*.mp3;*.flac;*.aac;*.m4a;*.wma|All Files|*.*";
                ofd.Title = "Ses Dosyası Seçin";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadAudioFile(ofd.FileName);
                }
            }
        }

        private async void LoadAudioFile(string filePath)
        {
            try
            {
                statusLabel.Text = "Dosya yükleniyor...";
                statusLabel.Visible = true;

                // Önceki dosyayı temizle
                CleanupAudio();
                CleanupTempFiles();

                currentFilePath = filePath;
                string fileExtension = Path.GetExtension(filePath).ToLower();

                // MP3 ve diğer formatları WAV'a çevir
                if (fileExtension != ".wav")
                {
                    statusLabel.Text = "Dosya WAV formatına çevriliyor...";
                    await Task.Run(() => ConvertToWav(filePath));
                }
                else
                {
                    currentWorkingWavFile = filePath;
                }

                // WAV dosyasını yükle
                audioFileReader = new AudioFileReader(currentWorkingWavFile);
                totalDuration = (float)audioFileReader.TotalTime.TotalSeconds;

                // TrackBar ayarları
                positionTrackBar.Maximum = (int)(totalDuration * 1000);
                positionTrackBar.Value = 0;
                trimStartTrackBar.Maximum = (int)(totalDuration * 1000);
                trimStartTrackBar.Value = 0;
                trimEndTrackBar.Maximum = (int)(totalDuration * 1000);
                trimEndTrackBar.Value = (int)(totalDuration * 1000);

                // Trim değerlerini sıfırla
                startTrim = 0;
                endTrim = totalDuration;

                // Audio data yükle
                statusLabel.Text = "Ses verisi analiz ediliyor...";
                await Task.Run(() => LoadAudioData());

                // Modification flag'i sıfırla
                hasAudioModifications = false;

                // Waveform çiz
                needsWaveformRedraw = true;
                waveformTimer.Start();

                UpdateTimeLabels();
                UpdateUI();

                // Undo stack'e ekle
                AddToUndoStack(currentWorkingWavFile);

                Text = $"Audio Editor - {Path.GetFileName(filePath)} - CaYatur";
                statusLabel.Text = "Hazır";

                // 2 saniye sonra status'u gizle
                System.Windows.Forms.Timer hideTimer = new System.Windows.Forms.Timer { Interval = 2000 };
                hideTimer.Tick += (s, e) => { statusLabel.Visible = false; hideTimer.Dispose(); };
                hideTimer.Start();
            }
            catch (Exception ex)
            {
                statusLabel.Visible = false;
                MessageBox.Show($"Dosya yüklenirken hata oluştu: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConvertToWav(string inputFile)
        {
            // Temp WAV dosyası oluştur
            string tempDir = Path.GetTempPath();
            string fileName = Path.GetFileNameWithoutExtension(inputFile);
            currentWorkingWavFile = Path.Combine(tempDir, $"AudioEditor_{fileName}_{DateTime.Now.Ticks}.wav");

            using (var reader = new AudioFileReader(inputFile))
            {
                // 44.1kHz, 16-bit, stereo olarak normalize et
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(44100, 16, reader.WaveFormat.Channels));
                WaveFileWriter.CreateWaveFile(currentWorkingWavFile, resampler);
                resampler.Dispose();
            }
        }

        private void LoadAudioData()
        {
            if (audioFileReader == null) return;

            audioFileReader.Position = 0;
            var sampleProvider = audioFileReader.ToSampleProvider();

            // Mono'ya çevir
            if (sampleProvider.WaveFormat.Channels > 1)
            {
                sampleProvider = sampleProvider.ToMono();
            }

            List<float> samples = new List<float>();
            float[] buffer = new float[4096];
            int samplesRead;

            while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    samples.Add(buffer[i]);
                }
            }

            audioData = samples.ToArray();
            audioFileReader.Position = 0;
        }

        // Ana waveform çizim fonksiyonu (static bitmap oluşturur)
        private void DrawCompleteWaveform()
        {
            if (audioData == null || waveformPanel.Width <= 0 || waveformPanel.Height <= 0)
                return;

            isDrawingWaveform = true;

            try
            {
                // Static waveform bitmap'ini oluştur (playback çizgisi olmadan)
                if (staticWaveformBitmap?.Size != waveformPanel.Size)
                {
                    staticWaveformBitmap?.Dispose();
                    staticWaveformBitmap = new Bitmap(waveformPanel.Width, waveformPanel.Height);
                }

                // Ana waveform bitmap'ini oluştur
                if (waveformBitmap?.Size != waveformPanel.Size)
                {
                    waveformBitmap?.Dispose();
                    waveformBitmap = new Bitmap(waveformPanel.Width, waveformPanel.Height);
                }

                using (Graphics g = Graphics.FromImage(staticWaveformBitmap))
                {
                    g.Clear(Color.FromArgb(20, 20, 20));
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    int width = waveformPanel.Width;
                    int height = waveformPanel.Height;
                    int centerY = height / 2;

                    // Trim alanlarını hesapla
                    float trimStartRatio = startTrim / totalDuration;
                    float trimEndRatio = endTrim / totalDuration;
                    int trimStartX = (int)(trimStartRatio * width);
                    int trimEndX = (int)(trimEndRatio * width);

                    // Ön izleme aktifse, kırpılacak alanları çiz
                    if (showTrimPreview || trimStartTrackBar.Value > 0 || trimEndTrackBar.Value < (int)(totalDuration * 1000))
                    {
                        // Sol kırpılacak alan (saydam kırmızı)
                        if (trimStartX > 0)
                        {
                            using (SolidBrush trimBrush = new SolidBrush(Color.FromArgb(80, 255, 100, 100)))
                            {
                                g.FillRectangle(trimBrush, 0, 0, trimStartX, height);
                            }

                            // Çerçeve
                            using (Pen trimPen = new Pen(Color.FromArgb(150, 255, 100, 100), 1))
                            {
                                g.DrawRectangle(trimPen, 0, 0, trimStartX - 1, height - 1);
                            }
                        }

                        // Sağ kırpılacak alan (saydam kırmızı)
                        if (trimEndX < width)
                        {
                            using (SolidBrush trimBrush = new SolidBrush(Color.FromArgb(80, 255, 100, 100)))
                            {
                                g.FillRectangle(trimBrush, trimEndX, 0, width - trimEndX, height);
                            }

                            // Çerçeve
                            using (Pen trimPen = new Pen(Color.FromArgb(150, 255, 100, 100), 1))
                            {
                                g.DrawRectangle(trimPen, trimEndX, 0, width - trimEndX - 1, height - 1);
                            }
                        }

                        // Korunacak alan göstergesi
                        if (trimStartX > 0 || trimEndX < width)
                        {
                            int keepStartX = Math.Max(0, trimStartX);
                            int keepEndX = Math.Min(width, trimEndX);

                            if (keepEndX > keepStartX)
                            {
                                using (Pen keepPen = new Pen(Color.FromArgb(150, 100, 255, 100), 2))
                                {
                                    keepPen.DashStyle = DashStyle.Dash;
                                    g.DrawRectangle(keepPen, keepStartX, 2, keepEndX - keepStartX - 1, height - 5);
                                }
                            }
                        }
                    }

                    // Waveform çiz
                    for (int x = 0; x < width; x++)
                    {
                        float progress = (float)x / width;
                        int sampleIndex = (int)(progress * audioData.Length);

                        if (sampleIndex < audioData.Length)
                        {
                            float amplitude = audioData[sampleIndex];
                            int y = centerY - (int)(amplitude * centerY * 0.8f);

                            // Kırpılacak alanlarda waveform'u soluk göster
                            Color waveColor = Color.FromArgb(100, 150, 255);
                            if ((x < trimStartX || x > trimEndX) && (showTrimPreview || trimStartTrackBar.Value > 0 || trimEndTrackBar.Value < (int)(totalDuration * 1000)))
                            {
                                waveColor = Color.FromArgb(60, 80, 120); // Daha soluk
                            }

                            using (Pen wavePen = new Pen(waveColor, 1f))
                            {
                                g.DrawLine(wavePen, x, centerY, x, y);
                            }
                        }
                    }

                    // Grid çizgileri
                    DrawGrid(g, width, height, centerY);

                    // Trim çizgilerini çiz
                    DrawTrimLines(g, width, height, trimStartX, trimEndX);

                    // Ön izleme açıksa bilgi metni
                    if (showTrimPreview)
                    {
                        DrawPreviewInfo(g, width);
                    }
                }

                // Şimdi playback çizgisini ekleyerek ana bitmap'i oluştur
                using (Graphics g = Graphics.FromImage(waveformBitmap))
                {
                    // Static waveform'u kopyala
                    g.DrawImage(staticWaveformBitmap, 0, 0);

                    // Playback position çizgisi ekle
                    if (totalDuration > 0)
                    {
                        float playbackX = (currentPosition / totalDuration) * waveformPanel.Width;
                        using (Pen playbackPen = new Pen(Color.FromArgb(255, 140, 0), 3f))
                        {
                            g.DrawLine(playbackPen, playbackX, 0, playbackX, waveformPanel.Height);

                            // Üstte küçük üçgen
                            Point[] triangle = new Point[]
                            {
                                new Point((int)playbackX - 5, 0),
                                new Point((int)playbackX + 5, 0),
                                new Point((int)playbackX, 10)
                            };
                            using (SolidBrush triangleBrush = new SolidBrush(Color.FromArgb(255, 140, 0)))
                            {
                                g.FillPolygon(triangleBrush, triangle);
                            }
                        }
                    }
                }

                waveformPanel.Invalidate();
            }
            finally
            {
                isDrawingWaveform = false;
            }
        }

        private void DrawGrid(Graphics g, int width, int height, int centerY)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(40, 40, 40), 1f))
            {
                gridPen.DashStyle = DashStyle.Dot;

                // Yatay çizgiler
                g.DrawLine(gridPen, 0, centerY, width, centerY);
                g.DrawLine(gridPen, 0, centerY / 2, width, centerY / 2);
                g.DrawLine(gridPen, 0, centerY + centerY / 2, width, centerY + centerY / 2);

                // Dikey çizgiler (zaman işaretleri)
                for (int i = 1; i < 10; i++)
                {
                    int x = (width * i) / 10;
                    g.DrawLine(gridPen, x, 0, x, height);

                    // Zaman etiketi
                    if (totalDuration > 0)
                    {
                        float timeSeconds = (totalDuration * i) / 10;
                        string timeText = TimeSpan.FromSeconds(timeSeconds).ToString(@"mm\:ss");
                        using (SolidBrush timeBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
                        using (Font timeFont = new Font("Segoe UI", 8))
                        {
                            g.DrawString(timeText, timeFont, timeBrush, x + 2, height - 20);
                        }
                    }
                }
            }
        }

        private void DrawTrimLines(Graphics g, int width, int height, int trimStartX, int trimEndX)
        {
            // Trim başlangıç çizgisi
            if (startTrim > 0)
            {
                using (Pen trimPen = new Pen(Color.FromArgb(0, 200, 100), 2f))
                {
                    g.DrawLine(trimPen, trimStartX, 0, trimStartX, height);

                    // Label
                    string startText = $"Başlangıç: {TimeSpan.FromSeconds(startTrim):mm\\:ss\\.ff}";
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(0, 200, 100)))
                    using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
                    {
                        SizeF textSize = g.MeasureString(startText, font);
                        float textX = Math.Max(5, Math.Min(width - textSize.Width - 5, trimStartX - textSize.Width / 2));
                        g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), textX - 2, 5, textSize.Width + 4, textSize.Height + 2);
                        g.DrawString(startText, font, textBrush, textX, 6);
                    }
                }
            }

            // Trim bitiş çizgisi
            if (endTrim < totalDuration)
            {
                using (Pen trimPen = new Pen(Color.FromArgb(255, 80, 80), 2f))
                {
                    g.DrawLine(trimPen, trimEndX, 0, trimEndX, height);

                    // Label
                    string endText = $"Bitiş: {TimeSpan.FromSeconds(endTrim):mm\\:ss\\.ff}";
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(255, 80, 80)))
                    using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
                    {
                        SizeF textSize = g.MeasureString(endText, font);
                        float textX = Math.Max(5, Math.Min(width - textSize.Width - 5, trimEndX - textSize.Width / 2));
                        g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), textX - 2, height - textSize.Height - 8, textSize.Width + 4, textSize.Height + 2);
                        g.DrawString(endText, font, textBrush, textX, height - textSize.Height - 7);
                    }
                }
            }
        }

        private void DrawPreviewInfo(Graphics g, int width)
        {
            string infoText = "ÖN İZLEME MODU - Kırmızı alanlar silinecek";
            using (SolidBrush infoBrush = new SolidBrush(Color.FromArgb(255, 255, 100)))
            using (Font infoFont = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                SizeF infoSize = g.MeasureString(infoText, infoFont);
                float infoX = (width - infoSize.Width) / 2;
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), infoX - 5, 30, infoSize.Width + 10, infoSize.Height + 4);
                g.DrawString(infoText, infoFont, infoBrush, infoX, 32);
            }
        }

        private void PlayPauseToggle()
        {
            if (audioFileReader == null) return;

            if (isPlaying)
            {
                PauseAudio();
            }
            else
            {
                PlayAudio();
            }
        }

        private void PlayAudio()
        {
            if (audioFileReader == null) return;

            try
            {
                outputDevice?.Stop();
                outputDevice?.Dispose();
                outputDevice = new WaveOutEvent();

                // Eğer audio data'da değişiklik varsa, modifiye edilmiş audio'yu oynat
                ISampleProvider sampleProvider;

                if (hasAudioModifications)
                {
                    // Modifiye edilmiş audioData'dan sample provider oluştur
                    sampleProvider = new ArraySampleProvider(audioData, 44100, 1);
                }
                else
                {
                    // Orijinal dosyayı oynat
                    sampleProvider = audioFileReader.ToSampleProvider();
                }

                // Volume ayarla (sadece playback için, kalıcı değil)
                float volume = 100 / 100f;
                var volumeProvider = new VolumeSampleProvider(sampleProvider)
                {
                    Volume = volume
                };

                outputDevice.Init(volumeProvider);
                outputDevice.Play();

                isPlaying = true;
                playbackTimer.Start();

                playButton.Text = "⏸";
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oynatma hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PauseAudio()
        {
            outputDevice?.Pause();
            isPlaying = false;
            playbackTimer.Stop();

            playButton.Text = "▶";
            UpdateUI();
        }

        private void StopAudio()
        {
            outputDevice?.Stop();
            isPlaying = false;
            playbackTimer.Stop();

            currentPosition = 0;
            if (audioFileReader != null)
                audioFileReader.CurrentTime = TimeSpan.Zero;

            positionTrackBar.Value = 0;
            playButton.Text = "▶";
            UpdateTimeLabels();
            UpdateUI();

            // Stop'ta da playback çizgisini güncelle
            UpdatePlaybackLineOnly();
        }

        private void SeekToPosition(float seconds)
        {
            if (audioFileReader == null) return;

            seconds = Math.Max(0, Math.Min(totalDuration, seconds));

            // Eğer audio data'da değişiklik yoksa, audioFileReader'ı kullan
            if (!hasAudioModifications && audioFileReader != null)
            {
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(seconds);
            }

            currentPosition = seconds;
            positionTrackBar.Value = (int)(seconds * 1000);
            UpdateTimeLabels();

            // Seek'te de playback çizgisini güncelle
            UpdatePlaybackLineOnly();
        }

        // DÜZELTME: Ses seviyesini KALICI olarak artıran fonksiyon
        private async void ApplyVolumeBoost(float multiplier)
        {
            if (audioData == null) return;

            try
            {
                statusLabel.Text = $"Ses seviyesi %{multiplier * 100:F0} oranında ayarlanıyor...";
                statusLabel.Visible = true;

                // Audio durduruluyor
                bool wasPlaying = isPlaying;
                if (isPlaying) PauseAudio();

                // Ses seviyesini KALICI olarak audioData'ya uygula
                await Task.Run(() =>
                {
                    for (int i = 0; i < audioData.Length; i++)
                    {
                        audioData[i] = Math.Max(-1f, Math.Min(1f, audioData[i] * multiplier));
                    }
                });

                // Audio modifikasyon flag'i set et
                hasAudioModifications = true;

                // Yeni audio data'yı working file'a kaydet
                string newWorkingFile = Path.GetTempFileName().Replace(".tmp", ".wav");
                await Task.Run(() => SaveProcessedAudio(newWorkingFile));

                // Eski working file'ı temizle
                CleanupTempFiles();
                currentWorkingWavFile = newWorkingFile;

                // AudioFileReader'ı yeniden oluştur (yeni dosyayla)
                audioFileReader?.Dispose();
                audioFileReader = new AudioFileReader(currentWorkingWavFile);

                // Waveform'u yeniden çiz
                needsWaveformRedraw = true;

                // Undo stack'e ekle
                AddToUndoStack(currentWorkingWavFile);

                statusLabel.Text = "Hazır";
                MessageBox.Show($"Ses seviyesi %{(multiplier * 100):F0} oranında KALICI olarak ayarlandı.\nArtık kaydettiğinizde bu seviyede kaydedilecektir.", "İşlem Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Eğer oynuyordu, tekrar oynat
                if (wasPlaying) PlayAudio();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ses seviyesi ayarlanırken hata: {ex.Message}", "Hata");
            }
            finally
            {
                statusLabel.Visible = false;
            }
        }

        private void TrimAudio()
        {
            if (audioData == null) return;

            try
            {
                startTrim = trimStartTrackBar.Value / 1000f;
                endTrim = trimEndTrackBar.Value / 1000f;

                if (startTrim >= endTrim)
                {
                    MessageBox.Show("Başlangıç zamanı bitiş zamanından küçük olmalıdır!", "Hata");
                    return;
                }

                statusLabel.Text = "Ses kırpılıyor...";
                statusLabel.Visible = true;

                // Audio durduruluyor
                bool wasPlaying = isPlaying;
                if (isPlaying) PauseAudio();

                // Sample indekslerini hesapla
                int sampleRate = 44100; // WAV formatımız 44.1kHz
                int startSample = (int)(startTrim * sampleRate);
                int endSample = (int)(endTrim * sampleRate);

                startSample = Math.Max(0, Math.Min(audioData.Length - 1, startSample));
                endSample = Math.Max(startSample + 1, Math.Min(audioData.Length, endSample));

                // Yeni audio data oluştur
                int newLength = endSample - startSample;
                float[] newAudioData = new float[newLength];
                Array.Copy(audioData, startSample, newAudioData, 0, newLength);

                audioData = newAudioData;
                hasAudioModifications = true;

                // Yeni süreyi hesapla
                totalDuration = audioData.Length / 44100f;

                // TrackBar'ları güncelle
                positionTrackBar.Maximum = (int)(totalDuration * 1000);
                positionTrackBar.Value = 0;
                trimStartTrackBar.Maximum = (int)(totalDuration * 1000);
                trimStartTrackBar.Value = 0;
                trimEndTrackBar.Maximum = (int)(totalDuration * 1000);
                trimEndTrackBar.Value = (int)(totalDuration * 1000);

                // Trim değerlerini sıfırla
                currentPosition = 0;
                startTrim = 0;
                endTrim = totalDuration;

                // Yeni audio data'yı working file'a kaydet
                string newWorkingFile = Path.GetTempFileName().Replace(".tmp", ".wav");
                SaveProcessedAudio(newWorkingFile);

                // Eski working file'ı temizle
                CleanupTempFiles();
                currentWorkingWavFile = newWorkingFile;

                // AudioFileReader'ı yeniden oluştur
                audioFileReader?.Dispose();
                audioFileReader = new AudioFileReader(currentWorkingWavFile);

                // Waveform'u yeniden çiz
                needsWaveformRedraw = true;

                // UI'yı güncelle
                UpdateTimeLabels();
                AddToUndoStack(currentWorkingWavFile);

                MessageBox.Show("Ses başarıyla kırpıldı.", "İşlem Tamamlandı");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kırpma işleminde hata: {ex.Message}", "Hata");
            }
            finally
            {
                statusLabel.Visible = false;
            }
        }

        private void SaveProcessedAudio(string filePath)
        {
            if (audioData == null) return;

            var format = new WaveFormat(44100, 16, 1);

            using (var writer = new WaveFileWriter(filePath, format))
            {
                for (int i = 0; i < audioData.Length; i++)
                {
                    writer.WriteSample(audioData[i]);
                }
            }
        }

        private void SaveAudio()
        {
            if (audioData == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "WAV Files|*.wav|MP3 Files|*.mp3|All Files|*.*";
                sfd.Title = "Ses Dosyasını Kaydet";
                sfd.FileName = Path.GetFileNameWithoutExtension(currentFilePath) + "_edited";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        statusLabel.Text = "Dosya kaydediliyor...";
                        statusLabel.Visible = true;

                        string extension = Path.GetExtension(sfd.FileName).ToLower();

                        if (extension == ".wav")
                        {
                            // İşlenmiş audio data'yı kaydet (ses seviyesi değişiklikleri dahil)
                            SaveProcessedAudio(sfd.FileName);
                        }
                        else if (extension == ".mp3")
                        {
                            // MP3 kaydetme - önce geçici WAV oluştur
                            string tempWav = Path.GetTempFileName().Replace(".tmp", ".wav");
                            SaveProcessedAudio(tempWav);

                            using (var reader = new AudioFileReader(tempWav))
                            {
                                MediaFoundationEncoder.EncodeToMp3(reader, sfd.FileName, 128000);
                            }

                            File.Delete(tempWav);
                        }

                        MessageBox.Show("Dosya başarıyla kaydedildi.\nTüm ses seviyesi değişiklikleri kalıcı olarak uygulandı.", "Başarılı");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata");
                    }
                    finally
                    {
                        statusLabel.Visible = false;
                    }
                }
            }
        }

        private void CleanupTempFiles()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentWorkingWavFile) &&
                    currentWorkingWavFile != currentFilePath &&
                    File.Exists(currentWorkingWavFile))
                {
                    File.Delete(currentWorkingWavFile);
                }
            }
            catch { /* Ignore cleanup errors */ }
        }

        private void ZoomIn()
        {
            zoomLevel = Math.Min(10, zoomLevel + 1);
            needsWaveformRedraw = true;
        }

        private void ZoomOut()
        {
            zoomLevel = Math.Max(1, zoomLevel - 1);
            needsWaveformRedraw = true;
        }

        private void AddToUndoStack(string filePath)
        {
            undoStack.Add(filePath);
            undoIndex = undoStack.Count - 1;

            // Maksimum 10 undo tut
            if (undoStack.Count > 10)
            {
                string oldFile = undoStack[0];
                if (oldFile != currentFilePath && File.Exists(oldFile))
                {
                    try { File.Delete(oldFile); } catch { }
                }
                undoStack.RemoveAt(0);
                undoIndex--;
            }
        }

        private void Undo()
        {
            if (undoIndex > 0)
            {
                undoIndex--;
                LoadAudioFile(undoStack[undoIndex]);
            }
        }

        private void UpdateTimeLabels()
        {
            TimeSpan current = TimeSpan.FromSeconds(currentPosition);
            TimeSpan total = TimeSpan.FromSeconds(totalDuration);

            currentTimeLabel.Text = $"{current:mm\\:ss\\.ff}";
            totalTimeLabel.Text = $"{total:mm\\:ss\\.ff}";

            float startSeconds = trimStartTrackBar.Value / 1000f;
            float endSeconds = trimEndTrackBar.Value / 1000f;

            trimStartLabel.Text = $"Başlangıç: {TimeSpan.FromSeconds(startSeconds):mm\\:ss\\.ff}";
            trimEndLabel.Text = $"Bitiş: {TimeSpan.FromSeconds(endSeconds):mm\\:ss\\.ff}";

            // Kırpılacak süreyi göster
            float trimDuration = endSeconds - startSeconds;
            trimDurationLabel.Text = $"Seçilen Süre: {TimeSpan.FromSeconds(trimDuration):mm\\:ss\\.ff}";
        }

        private void UpdateUI()
        {
            bool hasAudio = audioFileReader != null;

            playButton.Enabled = hasAudio;
            stopButton.Enabled = hasAudio;
            saveButton.Enabled = hasAudio;
            trimButton.Enabled = hasAudio;
            //boost500Button.Enabled = hasAudio;
            //previewTrimButton.Enabled = hasAudio;
            positionTrackBar.Enabled = hasAudio;
            trimStartTrackBar.Enabled = hasAudio;
            trimEndTrackBar.Enabled = hasAudio;

            // Başlık güncellemesi - modifikasyon varsa göster
            if (hasAudioModifications)
            {
                Text = $"Audio Editor - {Path.GetFileName(currentFilePath)} * - CaYatur";
            }
        }

        private void CleanupAudio()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            audioFileReader?.Dispose();
            currentAudioFile?.Dispose();

            isPlaying = false;
            lastPlaybackPosition = -1;
            playbackTimer?.Stop();
            waveformTimer?.Stop();
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (isPlaying && audioFileReader != null)
            {
                if (audioFileReader.CurrentTime >= audioFileReader.TotalTime)
                {
                    StopAudio();
                }
            }
        }

        private void WaveformTimer_Tick(object sender, EventArgs e)
        {
            if (needsWaveformRedraw && !isDrawingWaveform)
            {
                DrawCompleteWaveform();
                needsWaveformRedraw = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            CleanupAudio();
            CleanupTempFiles();
            waveformBitmap?.Dispose();
            staticWaveformBitmap?.Dispose();
            MediaFoundationApi.Shutdown();
            base.OnFormClosed(e);
        }

        // Event handlers for controls
        private void openButton_Click(object sender, EventArgs e) => OpenAudioFile();
        private void playButton_Click(object sender, EventArgs e) => PlayPauseToggle();
        private void stopButton_Click(object sender, EventArgs e) => StopAudio();
        private void saveButton_Click(object sender, EventArgs e) => SaveAudio();
        private void trimButton_Click(object sender, EventArgs e) => TrimAudio();
        private void boost500Button_Click(object sender, EventArgs e) => ApplyVolumeBoost(5.0f);
        private void darkModeButton_Click(object sender, EventArgs e) => ToggleDarkMode();
        private void mergeButton_Click(object sender, EventArgs e) => OpenMergeForm();

        private void ToggleDarkMode()
        {
            isDarkMode = !isDarkMode;
            SetupDarkTheme();
        }

        private void OpenMergeForm()
        {
            var mergeForm = new AudioMergeForm();
            mergeForm.ShowDialog();
        }

        private void positionTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (!isPlaying && !isDrawingWaveform)
            {
                float seconds = positionTrackBar.Value / 1000f;
                SeekToPosition(seconds);
            }
        }

        private void volumeTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // Volume değiştiğinde real-time update (sadece playback için)
            //volumeLabel.Text = $"Ses: %{volumeTrackBar.Value}";
        }

        private void trimStartTrackBar_ValueChanged(object sender, EventArgs e)
        {
            startTrim = trimStartTrackBar.Value / 1000f;
            UpdateTimeLabels();
            needsWaveformRedraw = true;
        }

        private void trimEndTrackBar_ValueChanged(object sender, EventArgs e)
        {
            endTrim = trimEndTrackBar.Value / 1000f;
            UpdateTimeLabels();
            needsWaveformRedraw = true;
        }

        private void waveformPanel_Paint(object sender, PaintEventArgs e)
        {
            if (waveformBitmap != null)
            {
                e.Graphics.DrawImage(waveformBitmap, 0, 0);
            }
        }

        private void waveformPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (totalDuration > 0 && !isDrawingWaveform)
            {
                float clickPosition = (float)e.X / waveformPanel.Width;
                float seekTime = clickPosition * totalDuration;
                SeekToPosition(seekTime);
            }
        }
    }

    // Yardımcı sınıf: Array'den SampleProvider oluşturmak için
    public class ArraySampleProvider : ISampleProvider
    {
        private readonly float[] audioData;
        private readonly WaveFormat waveFormat;
        private long position = 0;

        public ArraySampleProvider(float[] audioData, int sampleRate, int channels)
        {
            this.audioData = audioData;
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        public WaveFormat WaveFormat => waveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = 0;
            for (int i = 0; i < count && position < audioData.Length; i++)
            {
                buffer[offset + i] = audioData[position];
                position++;
                samplesRead++;
            }
            return samplesRead;
        }
    }
}