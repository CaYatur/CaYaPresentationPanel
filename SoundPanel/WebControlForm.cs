using Newtonsoft.Json;
using NAudio.Wave;
using QRCoder;  // NuGet'den ekleyin.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class WebControlForm : Form
    {
        private List<MediaItem> mediaItems;
        // Bu formdaki aktif ses oynatmalarını takip ediyoruz.
        private Dictionary<string, ActiveSound> activePlayers = new Dictionary<string, ActiveSound>();
        // Dönüştürülmüş dosyaları saklamak için.
        private Dictionary<string, string> convertedFiles = new Dictionary<string, string>();
        private string dataFilePath = "dataCY.json";

        // Bu form için global ses düzeyi (globalVolume):
        // Slider değeri 0-100; multiplier = (slider/50)^3.
        // Varsayılan slider = 50 → multiplier = 1.0, slider = 100 → multiplier = 8.0.
        private double globalVolume = 1.0;

        // Web sunucusu için HttpListener
        private HttpListener httpListener;
        private int webServerPort = 8080; // Varsayılan port
        private const int MaxPortTries = 10; // Alternatif port deneme sayısı

        // Form üzerindeki kontroller:
        private Panel panelButtons;
        private PictureBox pbQRCode;
        private CustomButton btnQRCode;
        private Label lblQRCodeLink;
        private ModernTrackBar globalVolumeTrackBar;
        private Label lblGlobalVolume;

        public WebControlForm()
        {
            InitializeComponent();
            // Yönetici kontrolü OnLoad içerisinde yapılacak.
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CheckAdminPrivileges();

            // Port başlatma ve hata yönetimi
            bool portReady = false;
            int tryPort = webServerPort;
            while (!portReady)
            {
                var result = TryStartWebServer(tryPort);
                if (result.success)
                {
                    webServerPort = result.port;
                    portReady = true;
                }
                else
                {
                    using (var dlg = new PortErrorDialog($"Port {tryPort} kullanılamıyor.\nFarklı bir port deneyin veya otomatik port arayın.", tryPort))
                    {
                        var dlgResult = dlg.ShowDialog();
                        if (dlg.Result == PortErrorDialog.PortDialogResult.RetryAuto)
                        {
                            tryPort++;
                            continue;
                        }
                        else if (dlg.Result == PortErrorDialog.PortDialogResult.EnterManually)
                        {
                            tryPort = dlg.EnteredPort;
                            continue;
                        }
                        else // Cancel
                        {
                            this.Close();
                            return;
                        }
                    }
                }
            }

            // Modern koyu tema
            this.BackColor = Color.FromArgb(24, 26, 34);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.Size = new Size(900, 700);

            // Medya butonları için panel
            panelButtons = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(420, this.ClientSize.Height - 40),
                AutoScroll = true,
                BackColor = Color.FromArgb(32, 34, 44),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(panelButtons);

            // QR kod için PictureBox (başlangıçta gizli)
            pbQRCode = new PictureBox
            {
                Location = new Point(panelButtons.Right + 30, 30),
                Size = new Size(200, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = false,
                BackColor = Color.FromArgb(40, 44, 55),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pbQRCode);

            // QR Kod butonu (CustomButton)
            btnQRCode = new CustomButton
            {
                Text = "QR Kod Göster",
                Size = new Size(200, 50),
                Location = new Point(panelButtons.Right + 30, pbQRCode.Bottom + 20),
                ButtonBackColor = Color.FromArgb(60, 130, 200),
                TextColor = Color.White,
                CornerRadius = 12,
                BorderThickness = 2,
                BorderColor = Color.FromArgb(80, 160, 220),
                HoverColor = Color.FromArgb(80, 160, 220),
                PressedColor = Color.FromArgb(40, 90, 160),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnQRCode.Click += BtnQRCode_Click;
            this.Controls.Add(btnQRCode);

            // QR kod linkini gösterecek Label (başlangıçta gizli)
            lblQRCodeLink = new Label
            {
                AutoSize = true,
                Location = new Point(panelButtons.Right + 30, btnQRCode.Bottom + 15),
                Visible = false,
                ForeColor = Color.FromArgb(180, 200, 255),
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblQRCodeLink);

            // Global ses düzeyi için ModernTrackBar ve Label
            globalVolumeTrackBar = new ModernTrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Width = 200,
                Height = 40,
                Location = new Point(panelButtons.Right + 30, lblQRCodeLink.Bottom + 40),
                Orientation = Orientation.Horizontal,
                TrackBackColor = Color.FromArgb(40, 44, 60),
                TrackProgressColor = Color.FromArgb(255, 204, 123),
                ThumbColor = Color.White,
                ThumbBorderColor = Color.FromArgb(255, 204, 123),
                BackgroundColor = Color.FromArgb(32, 34, 44),
                BorderColor = Color.FromArgb(60, 130, 200),
                BorderThickness = 2,
                CornerRadius = 10
            };
            globalVolumeTrackBar.ValueChanged += (s, ev) =>
            {
                globalVolume = Math.Pow(globalVolumeTrackBar.Value / 50.0, 3);
                lblGlobalVolume.Text = "Genel Ses: " + globalVolumeTrackBar.Value + "%";
                UpdateActivePlayersVolume();
            };
            this.Controls.Add(globalVolumeTrackBar);

            lblGlobalVolume = new Label
            {
                AutoSize = true,
                Location = new Point(globalVolumeTrackBar.Left, globalVolumeTrackBar.Bottom + 10),
                Text = "Genel Ses: 50%",
                ForeColor = Color.FromArgb(255, 204, 123),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblGlobalVolume);

            LoadMediaItems();
            CreateButtons();
        }

        /// <summary>
        /// Bu formdaki aktif medya öğelerinin ses düzeyini, ilgili medya öğesinin Volume değeri ile globalVolume'nun çarpımına göre günceller.
        /// </summary>
        private void UpdateActivePlayersVolume()
        {
            foreach (var active in activePlayers.Values)
            {
                if (active.SourceButton != null && active.SourceButton.Tag is MediaItem mi)
                {
                    active.AudioReader.Volume = (float)(mi.Volume * globalVolume);
                }
                else
                {
                    active.AudioReader.Volume = (float)globalVolume;
                }
            }
        }

        private void BtnQRCode_Click(object sender, EventArgs e)
        {
            bool newVisible = !pbQRCode.Visible;
            pbQRCode.Visible = newVisible;
            lblQRCodeLink.Visible = newVisible;
            if (newVisible)
            {
                GenerateAndShowQRCode();
                string ip = GetLocalIPv4();
                lblQRCodeLink.Text = "Link: http://" + ip + ":" + webServerPort;
                btnQRCode.Text = "QR Kod Gizle";
            }
            else
            {
                btnQRCode.Text = "QR Kod Göster";
            }
        }

        /// <summary>
        /// Yönetici kontrolü: Yönetici değilse kullanıcıya izin sorar; hayır derse kapanır.
        /// </summary>
        private void CheckAdminPrivileges()
        {
            if (!IsAdministrator())
            {
                DialogResult res = MessageBox.Show("Web ile kontrol yönetici hakları gerektirir. Yönetici olarak yeniden başlatılsın mı?",
                                                     "Yönetici İzni Gerekli", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    try
                    {
                        ProcessStartInfo proc = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            FileName = Application.ExecutablePath,
                            Verb = "runas"
                        };
                        Process.Start(proc);
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Yönetici olarak yeniden başlatılamadı: " + ex.Message,
                                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    this.Close();
                }
                else
                {
                    this.Close();
                }
            }
        }

        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return false;
        }

        private void LoadMediaItems()
        {
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                mediaItems = JsonConvert.DeserializeObject<List<MediaItem>>(json);
            }
            else
            {
                MessageBox.Show("dataCY.json dosyası bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMediaItems()
        {
            try
            {
                string json = JsonConvert.SerializeObject(mediaItems, Formatting.Indented);
                File.WriteAllText(dataFilePath, json);
            }
            catch { }
        }

        /// <summary>
        /// Panel içine medya butonlarını ekler. Buton metninde ses düzeyi yüzde olarak gösterilir.
        /// </summary>
        private void CreateButtons()
        {
            panelButtons.Controls.Clear();
            panelButtons.AutoScroll = true;
            int yOffset = 15;
            int panelWidth = panelButtons.Width - 24;
            int panelHeight = 90;
            int buttonWidth = panelWidth - 20;
            int buttonHeight = 50;

            // Listeyi Yenile Butonu (CustomButton)
            Panel refreshPanel = new Panel()
            {
                Width = panelWidth,
                Height = panelHeight,
                Top = yOffset,
                Left = 8,
                BackColor = Color.FromArgb(40, 50, 70),
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 0, 15)
            };
            refreshPanel.Padding = new Padding(10, 15, 10, 15);

            CustomButton refreshButton = new CustomButton()
            {
                Text = "🔄 Listeyi Yenile",
                Width = buttonWidth,
                Height = buttonHeight,
                Left = 0,
                Top = 0,
                ButtonBackColor = Color.FromArgb(70, 180, 70),
                TextColor = Color.White,
                CornerRadius = 12,
                BorderThickness = 2,
                BorderColor = Color.FromArgb(90, 200, 90),
                HoverColor = Color.FromArgb(90, 200, 90),
                PressedColor = Color.FromArgb(50, 140, 50),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            refreshButton.Click += (s, e) =>
            {
                LoadMediaItems();
                CreateButtons();
            };

            refreshPanel.Controls.Add(refreshButton);
            panelButtons.Controls.Add(refreshPanel);
            yOffset += panelHeight + 15;

            foreach (var item in mediaItems)
            {
                bool isInteractive = !(item.IsVideo || item.IsPresentation);

                // Oynatılıyorsa kontrol et
                string originalFilePath = Path.Combine(Application.StartupPath, item.Path);
                string effectiveFilePath = originalFilePath;
                if (Path.GetExtension(originalFilePath).ToLowerInvariant() != ".wav" && convertedFiles.ContainsKey(originalFilePath))
                    effectiveFilePath = convertedFiles[originalFilePath];
                bool isPlaying = activePlayers.ContainsKey(originalFilePath) || activePlayers.ContainsKey(effectiveFilePath);

                // Modern panel renkleri - oynatılıyorsa sarımsı, değilse türe göre
                Color panelBackColor;
                if (isPlaying)
                {
                    panelBackColor = Color.FromArgb(255, 204, 123); // Sarımsı arka plan (oynatiliyor)
                }
                else if (item.IsImage)
                {
                    panelBackColor = Color.FromArgb(50, 70, 130);
                }
                else if (item.IsVideo)
                {
                    panelBackColor = Color.FromArgb(90, 40, 70);
                }
                else if (item.IsPresentation)
                {
                    panelBackColor = Color.FromArgb(100, 70, 40);
                }
                else
                {
                    panelBackColor = Color.FromArgb(70, 50, 100);
                }

                // Container panel
                Panel containerPanel = new Panel()
                {
                    Width = panelWidth,
                    Height = panelHeight,
                    Top = yOffset,
                    Left = 8,
                    BackColor = panelBackColor,
                    BorderStyle = BorderStyle.None,
                    Margin = new Padding(0, 0, 0, 15)
                };
                containerPanel.Padding = new Padding(10, 8, 10, 8);

                // Ana buton (CustomButton) - oynatılıyorsa turuncu, değilse normal renk
                CustomButton btn = new CustomButton()
                {
                    Text = item.Name,
                    Width = buttonWidth,
                    Height = buttonHeight,
                    Left = 0,
                    Top = 0,
                    ButtonBackColor = isPlaying ? Color.FromArgb(255, 140, 0) : (isInteractive ? Color.FromArgb(60, 130, 200) : Color.FromArgb(80, 80, 80)),
                    TextColor = Color.White,
                    CornerRadius = 10,
                    BorderThickness = 2,
                    BorderColor = isPlaying ? Color.FromArgb(255, 165, 0) : (isInteractive ? Color.FromArgb(80, 160, 220) : Color.FromArgb(100, 100, 100)),
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Tag = item,
                    Enabled = isInteractive,
                    HoverColor = isInteractive ? Color.FromArgb(80, 160, 220) : Color.FromArgb(80, 80, 80),
                    PressedColor = isInteractive ? Color.FromArgb(40, 90, 160) : Color.FromArgb(80, 80, 80),
                    TextAlign = ContentAlignment.MiddleLeft,
                    DisabledBackColor = Color.FromArgb(60, 60, 60),
                    DisabledTextColor = Color.FromArgb(140, 140, 140)
                };
                if (isInteractive)
                {
                    btn.Click += Btn_Click;
                }

                // Açıklama label
                Label descLabel = new Label()
                {
                    Text = string.IsNullOrEmpty(item.Description) ? "Açıklama yok" : item.Description,
                    AutoSize = false,
                    Width = buttonWidth - 60,
                    Height = 24,
                    Left = 0,
                    Top = buttonHeight + 4,
                    ForeColor = isPlaying ? Color.FromArgb(50, 50, 50) : Color.FromArgb(220, 220, 220),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    BackColor = Color.Transparent
                };

                // Ses düzeyi yüzdesi (trackbar yok, sadece yazı)
                Label volLabel = new Label
                {
                    Text = $"{(item.Volume * 100):F0}%",
                    AutoSize = false,
                    Width = 50,
                    Height = 24,
                    Left = buttonWidth - 55,
                    Top = buttonHeight + 4,
                    ForeColor = isPlaying ? Color.FromArgb(120, 80, 0) : Color.FromArgb(255, 204, 123),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent
                };

                containerPanel.Controls.Add(btn);
                containerPanel.Controls.Add(descLabel);
                containerPanel.Controls.Add(volLabel);
                panelButtons.Controls.Add(containerPanel);
                yOffset += panelHeight + 15;
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            CustomButton clickedButton = sender as CustomButton;
            MediaItem mediaItem = (MediaItem)clickedButton.Tag;

            if (mediaItem.IsImage)
            {
                // Resim dosyası için: BackgroundImageForm ile resmi göster.
                if (BackgroundImageForm.SelectedScreen != null)
                {
                    if (BackgroundImageForm.Instance != null)
                    {
                        BackgroundImageForm.Instance.UpdateImage(mediaItem.Path);
                        BackgroundImageForm.Instance.Bounds = BackgroundImageForm.SelectedScreen.Bounds;
                        BackgroundImageForm.Instance.Focus();
                    }
                    else
                    {
                        BackgroundImageForm bgForm = new BackgroundImageForm(mediaItem.Path);
                        bgForm.StartPosition = FormStartPosition.Manual;
                        bgForm.Bounds = BackgroundImageForm.SelectedScreen.Bounds;
                        bgForm.Show();
                    }
                }
                else
                {
                    // Henüz ekran seçimi yapılmamışsa, kullanıcıya ekran seçim formu göster.
                    Screen[] screens = Screen.AllScreens;
                    Form screenSelectionForm = new Form
                    {
                        Text = "Ekran Seçin",
                        Width = 400,
                        Height = 300,
                        StartPosition = FormStartPosition.CenterScreen,
                        BackColor = Color.FromArgb(32, 34, 44),
                        ForeColor = Color.White
                    };

                    for (int i = 0; i < screens.Length; i++)
                    {
                        CustomButton screenButton = new CustomButton
                        {
                            Text = GetDisplayInfo(screens[i]),
                            Width = 350,
                            Height = 50,
                            Top = 60 * i + 10,
                            Left = 25,
                            Tag = screens[i],
                            ButtonBackColor = Color.FromArgb(60, 130, 200),
                            TextColor = Color.White,
                            CornerRadius = 8,
                            BorderThickness = 1,
                            BorderColor = Color.FromArgb(80, 160, 220),
                            Font = new Font("Segoe UI", 10F, FontStyle.Regular)
                        };
                        screenButton.Click += (s, args) =>
                        {
                            Screen selectedScreen = (Screen)((CustomButton)s).Tag;
                            BackgroundImageForm.SelectedScreen = selectedScreen;
                            if (BackgroundImageForm.Instance != null)
                            {
                                BackgroundImageForm.Instance.UpdateImage(mediaItem.Path);
                                BackgroundImageForm.Instance.Bounds = selectedScreen.Bounds;
                                BackgroundImageForm.Instance.Focus();
                            }
                            else
                            {
                                BackgroundImageForm bgForm = new BackgroundImageForm(mediaItem.Path);
                                bgForm.StartPosition = FormStartPosition.Manual;
                                bgForm.Bounds = selectedScreen.Bounds;
                                bgForm.Show();
                            }
                            screenSelectionForm.Close();
                        };
                        screenSelectionForm.Controls.Add(screenButton);
                    }
                    screenSelectionForm.ShowDialog();
                }
            }
            else
            {
                // Diğer medya türleri için (örneğin ses) mevcut TogglePlayback metodunu çağırıyoruz.
                TogglePlayback(mediaItem, null); // CustomButton Button'a cast edilemeyeceği için null gönderiyoruz
            }
        }


        /// <summary>
        /// Medya öğesi için oynatmayı/durdurmayı yönetir.
        /// JSON'dan çekilen Volume değeri, globalVolume ile çarpılarak AudioFileReader.Volume'a atanır.
        /// </summary>
        private void TogglePlayback(MediaItem mediaItem, Button sourceButton)
        {
            string relativePath = mediaItem.Path;
            string originalFilePath = Path.Combine(Application.StartupPath, relativePath);
            string ext = Path.GetExtension(originalFilePath).ToLowerInvariant();
            string filePath = originalFilePath;

            if (ext != ".wav")
            {
                if (convertedFiles.ContainsKey(originalFilePath))
                    filePath = convertedFiles[originalFilePath];
                else
                {
                    try
                    {
                        filePath = ConvertToWav(originalFilePath);
                        convertedFiles[originalFilePath] = filePath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Format dönüştürülemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            if (activePlayers.ContainsKey(filePath))
            {
                StopPlayback(filePath);
                if (sourceButton != null)
                {
                    sourceButton.BackColor = Color.LightGreen;
                }
                // Panel rengini güncelle
                CreateButtons();
            }
            else
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        AudioFileReader reader = new AudioFileReader(filePath)
                        {
                            Volume = (float)(mediaItem.Volume * globalVolume)
                        };
                        WaveOutEvent waveOut = new WaveOutEvent();
                        waveOut.Init(reader);

                        ActiveSound activeSound = new ActiveSound()
                        {
                            WaveOut = waveOut,
                            AudioReader = reader,
                            SourceButton = sourceButton
                        };

                        waveOut.PlaybackStopped += (s, ev) =>
                        {
                            if (this.IsHandleCreated)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (sourceButton != null)
                                    {
                                        sourceButton.BackColor = Color.LightGreen;
                                    }

                                    if (activePlayers.ContainsKey(filePath))
                                        activePlayers.Remove(filePath);
                                    
                                    // Panel rengini güncelle
                                    CreateButtons();
                                }));
                            }
                            else
                            {
                                if (sourceButton != null)
                                {
                                    sourceButton.BackColor = Color.LightGreen;
                                }

                                if (activePlayers.ContainsKey(filePath))
                                    activePlayers.Remove(filePath);
                            }
                            waveOut.Dispose();
                            reader.Dispose();
                        };

                        activePlayers[filePath] = activeSound;
                        if (sourceButton != null)
                        {
                            sourceButton.BackColor = Color.Orange;
                        }

                        waveOut.Play();
                        
                        // Panel rengini güncelle
                        CreateButtons();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ses oynatılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Ses dosyası bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void StopPlayback(string filePath)
        {
            if (activePlayers.ContainsKey(filePath))
            {
                ActiveSound activeSound = activePlayers[filePath];
                activeSound.WaveOut.Stop();
            }
        }

        private string ConvertToWav(string inputFile)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(inputFile) + "_converted.wav");
            using (var reader = new AudioFileReader(inputFile))
            {
                WaveFileWriter.CreateWaveFile(tempFile, reader);
            }
            return tempFile;
        }

        private string NormalizePath(string path)
        {
            return path.Replace("\\", "/").ToLowerInvariant();
        }

        /// <summary>
        /// Yerel IPv4 adresini döndürür.
        /// </summary>
        private string GetLocalIPv4()
        {
            string localIP = "";
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
            }
            catch
            {
                localIP = "127.0.0.1";
            }
            return localIP;
        }

        /// <summary>
        /// QR kod üretir ve pbQRCode içerisinde gösterir.
        /// </summary>
        private void GenerateAndShowQRCode()
        {
            string ip = GetLocalIPv4();
            string url = "http://" + ip + ":" + webServerPort;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    Bitmap qrCodeImage = qrCode.GetGraphic(20);
                    pbQRCode.Image = qrCodeImage;
                }
            }
        }

        #region Web Sunucusu

        /// <summary>
        /// Kullanıcıdan port numarası isteyen basit bir dialog.
        /// </summary>
        private bool PromptForPort()
        {
            using (Form portForm = new Form())
            {
                portForm.Text = "Web Sunucu Portu Seçin";
                portForm.Width = 350;
                portForm.Height = 180;
                portForm.StartPosition = FormStartPosition.CenterScreen;
                portForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                portForm.MaximizeBox = false;
                portForm.MinimizeBox = false;
                portForm.BackColor = Color.FromArgb(32, 34, 44);
                portForm.ForeColor = Color.White;

                Label lbl = new Label { Text = "Web arayüzü için port numarası girin:", AutoSize = true, Top = 20, Left = 20 };
                TextBox txt = new TextBox { Text = webServerPort.ToString(), Top = 50, Left = 20, Width = 120 };
                Button okBtn = new Button { Text = "Tamam", Top = 90, Left = 20, Width = 80 };
                Button cancelBtn = new Button { Text = "İptal", Top = 90, Left = 110, Width = 80 };

                okBtn.Click += (s, e) =>
                {
                    if (int.TryParse(txt.Text, out int port) && port > 0 && port < 65536)
                    {
                        webServerPort = port;
                        portForm.DialogResult = DialogResult.OK;
                        portForm.Close();
                    }
                    else
                    {
                        MessageBox.Show("Geçerli bir port girin (1-65535)", "Hatalı Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
                cancelBtn.Click += (s, e) => { portForm.DialogResult = DialogResult.Cancel; portForm.Close(); };

                portForm.Controls.Add(lbl);
                portForm.Controls.Add(txt);
                portForm.Controls.Add(okBtn);
                portForm.Controls.Add(cancelBtn);

                return portForm.ShowDialog() == DialogResult.OK;
            }
        }

        private void StartWebServer()
        {
            httpListener = new HttpListener();
            int tryCount = 0;
            bool started = false;
            int port = webServerPort;
            while (!started && tryCount < MaxPortTries)
            {
                try
                {
                    httpListener.Prefixes.Clear();
                    httpListener.Prefixes.Add($"http://*:{port}/");
                    httpListener.Start();
                    webServerPort = port; // Başarıyla başlatılan portu kaydet
                    started = true;
                }
                catch (HttpListenerException)
                {
                    port++;
                    tryCount++;
                }
            }
            if (!started)
            {
                MessageBox.Show($"Web sunucusu başlatılamadı. {webServerPort} ve sonraki {MaxPortTries} port denendi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            Task.Run(() =>
            {
                while (httpListener.IsListening)
                {
                    try
                    {
                        var context = httpListener.GetContext();
                        ProcessRequest(context);
                    }
                    catch { }
                }
            });
        }

        // Web sunucusunu başlatmayı dener, başarılıysa true ve portu döner
        private (bool success, int port) TryStartWebServer(int port)
        {
            try
            {
                if (httpListener != null)
                {
                    if (httpListener.IsListening) httpListener.Stop();
                    httpListener.Close();
                }
                httpListener = new HttpListener();
                httpListener.Prefixes.Clear();
                httpListener.Prefixes.Add($"http://*:{port}/");
                httpListener.Start();
                Task.Run(() =>
                {
                    while (httpListener.IsListening)
                    {
                        try
                        {
                            var context = httpListener.GetContext();
                            ProcessRequest(context);
                        }
                        catch { }
                    }
                });
                return (true, port);
            }
            catch
            {
                return (false, port);
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string responseString = "";
            string path = context.Request.Url.AbsolutePath.ToLowerInvariant();

            if (path == "/" || path == "/index.html")
            {
                responseString = GenerateModernWebPage();
            }
            else if (path == "/listhtml")
            {
                responseString = GenerateModernHtml();
            }
            else if (path == "/list")
            {
                responseString = JsonConvert.SerializeObject(mediaItems);
            }
            else if (path == "/toggle")
            {
                string fileParam = context.Request.QueryString["file"];
                if (string.IsNullOrEmpty(fileParam))
                {
                    responseString = "Dosya parametresi eksik.";
                }
                else
                {
                    MediaItem item = mediaItems.Find(mi => NormalizePath(mi.Path) == NormalizePath(fileParam));
                    if (item == null)
                    {
                        responseString = "Dosya bulunamadı.";
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            if (item.IsImage)
                            {
                                // Resim içeriği ise: BackgroundImageForm ile resmi aç
                                if (BackgroundImageForm.SelectedScreen != null)
                                {
                                    if (BackgroundImageForm.Instance != null)
                                    {
                                        BackgroundImageForm.Instance.UpdateImage(item.Path);
                                        BackgroundImageForm.Instance.Bounds = BackgroundImageForm.SelectedScreen.Bounds;
                                        BackgroundImageForm.Instance.Focus();
                                    }
                                    else
                                    {
                                        BackgroundImageForm bgForm = new BackgroundImageForm(item.Path);
                                        bgForm.StartPosition = FormStartPosition.Manual;
                                        bgForm.Bounds = BackgroundImageForm.SelectedScreen.Bounds;
                                        bgForm.Show();
                                    }
                                }
                                else
                                {
                                    // Henüz ekran seçimi yapılmamışsa, ekran seçim formunu aç
                                    Screen[] screens = Screen.AllScreens;
                                    Form screenSelectionForm = new Form
                                    {
                                        Text = "Ekran Seçin",
                                        Width = 400,
                                        Height = 300,
                                        StartPosition = FormStartPosition.CenterScreen,
                                        TopMost = true
                                    };
                                    for (int i = 0; i < screens.Length; i++)
                                    {
                                        Button screenButton = new Button
                                        {
                                            Width = 300,
                                            Height = 50,
                                            Top = 60 * i + 10,
                                            Left = 10,
                                            Tag = screens[i],
                                            Text = GetDisplayInfo(screens[i])
                                        };
                                        screenButton.Click += (s, args) =>
                                        {
                                            Screen selectedScreen = (Screen)((Button)s).Tag;
                                            BackgroundImageForm.SelectedScreen = selectedScreen;
                                            if (BackgroundImageForm.Instance != null)
                                            {
                                                BackgroundImageForm.Instance.UpdateImage(item.Path);
                                                BackgroundImageForm.Instance.Bounds = selectedScreen.Bounds;
                                                BackgroundImageForm.Instance.Focus();
                                            }
                                            else
                                            {
                                                BackgroundImageForm bgForm = new BackgroundImageForm(item.Path);
                                                bgForm.StartPosition = FormStartPosition.Manual;
                                                bgForm.Bounds = selectedScreen.Bounds;
                                                bgForm.Show();
                                            }
                                            screenSelectionForm.Close();
                                        };
                                        screenSelectionForm.Controls.Add(screenButton);
                                    }
                                    screenSelectionForm.ShowDialog();
                                }
                            }
                            else if (!(item.IsVideo || item.IsPresentation))
                            {
                                TogglePlayback(item, FindButtonForMediaItem(item));
                            }
                        }));
                        responseString = "İşlem tamamlandı.";
                    }
                }
            }
            else if (path == "/globalvolume")
            {
                responseString = "{\"globalVolume\": " + globalVolume.ToString("F2", CultureInfo.InvariantCulture) + "}";
            }
            else if (path == "/setglobalvolume")
            {
                string valParam = context.Request.QueryString["value"];
                if (string.IsNullOrEmpty(valParam))
                {
                    responseString = "Value eksik.";
                }
                else
                {
                    if (double.TryParse(valParam, NumberStyles.Any, CultureInfo.InvariantCulture, out double newVal))
                    {
                        globalVolume = Math.Pow(newVal / 50.0, 3);
                        responseString = "Ses seviyesi güncellendi.";
                        this.Invoke(new Action(() =>
                        {
                            int sliderVal = (int)(50 * Math.Pow(globalVolume, 1.0 / 3.0));
                            globalVolumeTrackBar.Value = sliderVal;
                            lblGlobalVolume.Text = "Genel Ses: " + sliderVal + "%";
                            UpdateActivePlayersVolume();
                        }));
                    }
                    else
                    {
                        responseString = "Geçersiz değer.";
                    }
                }
            }
            else
            {
                responseString = "Geçersiz istek.";
            }

            WriteResponse(context, responseString);
        }

        private string GenerateModernWebPage()
        {
            StringBuilder sb = new StringBuilder();
            
            // HTML yapısı
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='tr'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=no'>");
            sb.AppendLine("<title>🎵 Modern Medya Kontrol</title>");
            sb.AppendLine("<link href='https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap' rel='stylesheet'>");
            
            // CSS
            sb.AppendLine("<style>");
            sb.AppendLine(GetModernCSS());
            sb.AppendLine("</style>");
            
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<header class='header'>");
            sb.AppendLine("<h1>🎵 Modern Medya Kontrol</h1>");
            sb.AppendLine("<p>Ses, video ve sunum dosyalarınızı kolayca yönetin</p>");
            sb.AppendLine("</header>");
            
            sb.AppendLine("<div class='volume-control'>");
            sb.AppendLine("<div class='volume-label'>🔊 Genel Ses Düzeyi: <span class='volume-value' id='globalVolumeLabel'>50%</span></div>");
            sb.AppendLine("<input type='range' class='volume-slider' id='globalVolumeSlider' min='0' max='100' value='50' oninput='setGlobalVolume(this.value)' />");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<div id='mediaList'>");
            sb.AppendLine(GenerateModernHtml());
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            // JavaScript
            sb.AppendLine("<script>");
            sb.AppendLine(GetModernJavaScript());
            sb.AppendLine("</script>");
            
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        private string GetModernCSS()
        {
            return @"
:root {
    --bg-primary: #0f0f0f;
    --bg-secondary: #1a1a1a;
    --bg-tertiary: #2a2a2a;
    --text-primary: #ffffff;
    --text-secondary: #b3b3b3;
    --text-muted: #666666;
    --accent-primary: #1db954;
    --accent-secondary: #1ed760;
    --accent-danger: #e22134;
    --accent-warning: #f59e0b;
    --accent-info: #3b82f6;
    --border-color: #333333;
    --shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    --radius: 12px;
    --radius-small: 8px;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
    background: linear-gradient(135deg, var(--bg-primary) 0%, #1a1a1a 100%);
    color: var(--text-primary);
    min-height: 100vh;
    line-height: 1.6;
    overflow-x: hidden;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

.header {
    text-align: center;
    margin-bottom: 40px;
    padding: 30px 0;
    background: linear-gradient(135deg, var(--bg-secondary), var(--bg-tertiary));
    border-radius: var(--radius);
    box-shadow: var(--shadow);
}

.header h1 {
    font-size: clamp(1.8rem, 4vw, 2.5rem);
    font-weight: 700;
    margin-bottom: 10px;
    background: linear-gradient(45deg, var(--accent-primary), var(--accent-secondary));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.header p {
    color: var(--text-secondary);
    font-size: 1rem;
    font-weight: 400;
}

.volume-control {
    background: var(--bg-secondary);
    padding: 25px;
    border-radius: var(--radius);
    margin-bottom: 30px;
    box-shadow: var(--shadow);
    border: 1px solid var(--border-color);
}

.volume-label {
    font-size: 1.1rem;
    font-weight: 600;
    margin-bottom: 15px;
    color: var(--text-primary);
    display: flex;
    align-items: center;
    gap: 10px;
}

.volume-value {
    color: var(--accent-primary);
    font-weight: 700;
    font-size: 1.2rem;
}

.volume-slider {
    -webkit-appearance: none;
    width: 100%;
    height: 8px;
    border-radius: 4px;
    background: linear-gradient(to right, var(--accent-primary) 0%, var(--accent-primary) var(--progress, 50%), var(--bg-tertiary) var(--progress, 50%), var(--bg-tertiary) 100%);
    outline: none;
    border: none;
    cursor: pointer;
    transition: all 0.3s ease;
}

.volume-slider::-webkit-slider-thumb {
    -webkit-appearance: none;
    appearance: none;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    background: var(--text-primary);
    cursor: pointer;
    border: 3px solid var(--accent-primary);
    box-shadow: 0 2px 10px rgba(29, 185, 84, 0.3);
    transition: all 0.3s ease;
}

.volume-slider::-webkit-slider-thumb:hover {
    transform: scale(1.2);
    box-shadow: 0 4px 15px rgba(29, 185, 84, 0.5);
}

.volume-slider::-moz-range-thumb {
    width: 20px;
    height: 20px;
    border-radius: 50%;
    background: var(--text-primary);
    cursor: pointer;
    border: 3px solid var(--accent-primary);
    box-shadow: 0 2px 10px rgba(29, 185, 84, 0.3);
}

.media-grid {
    display: flex;
    flex-direction: column;
    gap: 20px;
    margin-top: 20px;
}

.media-card {
    background: var(--bg-secondary);
    border-radius: var(--radius);
    padding: 20px;
    border: 1px solid var(--border-color);
    box-shadow: var(--shadow);
    transition: all 0.3s ease;
    position: relative;
    overflow: hidden;
}

.media-card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: linear-gradient(90deg, transparent, var(--accent-primary), transparent);
    opacity: 0;
    transition: opacity 0.3s ease;
}

.media-card:hover::before {
    opacity: 1;
}

.media-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.4);
    border-color: var(--accent-primary);
}

.media-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 15px;
}

.media-icon {
    width: 40px;
    height: 40px;
    border-radius: var(--radius-small);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.2rem;
    font-weight: bold;
    color: white;
}

.media-icon.audio { background: linear-gradient(135deg, #c993e5, #b974d9); }
.media-icon.video { background: linear-gradient(135deg, #f47a8f, #e85d75); }
.media-icon.image { background: linear-gradient(135deg, #7994f7, #5c7cfa); }
.media-icon.presentation { background: linear-gradient(135deg, #f99c78, #f78166); }

.media-title {
    font-size: 1.1rem;
    font-weight: 600;
    color: var(--text-primary);
    line-height: 1.3;
}

.media-description {
    color: var(--text-secondary);
    font-size: 0.9rem;
    margin-bottom: 15px;
    line-height: 1.4;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.media-controls {
    display: flex;
    align-items: center;
    gap: 15px;
    margin-top: 15px;
}

.volume-info {
    display: flex;
    align-items: center;
    gap: 8px;
    color: var(--text-secondary);
    font-size: 0.9rem;
    margin-left: auto;
}

.btn {
    padding: 12px 24px;
    border: none;
    border-radius: var(--radius-small);
    font-weight: 600;
    font-size: 0.95rem;
    cursor: pointer;
    transition: all 0.3s ease;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    text-decoration: none;
    position: relative;
    overflow: hidden;
}

.btn::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
    transition: left 0.5s;
}

.btn:hover::before {
    left: 100%;
}

.btn-success {
    background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
    color: white;
}

.btn-success:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(29, 185, 84, 0.3);
}

.btn-danger {
    background: linear-gradient(135deg, var(--accent-danger), #dc2626);
    color: white;
}

.btn-danger:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(226, 33, 52, 0.3);
}

.btn-disabled {
    background: var(--bg-tertiary);
    color: var(--text-muted);
    cursor: not-allowed;
    transform: none !important;
    box-shadow: none !important;
}

.status-badge {
    position: absolute;
    top: 15px;
    right: 15px;
    padding: 4px 8px;
    border-radius: 20px;
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.status-playing {
    background: linear-gradient(135deg, var(--accent-danger), #dc2626);
    color: white;
}

.status-ready {
    background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
    color: white;
}

.loading {
    display: inline-block;
    width: 20px;
    height: 20px;
    border: 2px solid var(--bg-tertiary);
    border-radius: 50%;
    border-top-color: var(--accent-primary);
    animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

.empty-state {
    text-align: center;
    padding: 60px 20px;
    color: var(--text-secondary);
}

.empty-state h3 {
    font-size: 1.3rem;
    margin-bottom: 10px;
    color: var(--text-primary);
}

@media (max-width: 768px) {
    .container {
        padding: 15px;
    }
    
    .header {
        padding: 20px;
        margin-bottom: 25px;
    }
    
    .media-grid {
        gap: 15px;
    }
    
    .media-card {
        padding: 15px;
    }
    
    .media-controls {
        flex-direction: column;
        align-items: stretch;
        gap: 10px;
    }
    
    .volume-info {
        margin-left: 0;
        justify-content: center;
    }
    
    .btn {
        justify-content: center;
        padding: 14px 20px;
    }
}

@media (max-width: 480px) {
    .header h1 {
        font-size: 1.5rem;
    }
    
    .media-header {
        flex-direction: column;
        text-align: center;
        gap: 8px;
    }
    
    .media-title {
        font-size: 1rem;
    }
}
";
        }

        private string GetModernJavaScript()
        {
            // Use verbatim string literal and double double-quotes for embedded quotes
            return @"
async function setGlobalVolume(val) {
    try {
        const slider = document.getElementById(""globalVolumeSlider"");
        slider.style.setProperty('--progress', val + '%');
        await fetch('/setglobalvolume?value=' + encodeURIComponent(val));
        updateGlobalVolume();
    } catch (error) { 
        console.error('Ses seviyesi hatası:', error); 
    }
}

let lastVolume = null;

async function updateGlobalVolume() {
    try {
        const response = await fetch('/globalvolume');
        const data = await response.json();
        const sliderValue = 50 * Math.pow(data.globalVolume, 1/3);

        if (lastVolume !== sliderValue) {
            requestAnimationFrame(function() {
                const slider = document.getElementById(""globalVolumeSlider"");
                const label = document.getElementById(""globalVolumeLabel"");
                slider.value = sliderValue;
                slider.style.setProperty('--progress', sliderValue + '%');
                label.innerText = Math.round(sliderValue) + '%';
            });
            lastVolume = sliderValue;
        }
    } catch (error) { 
        console.error('Ses seviyesi güncellenemedi:', error); 
    }
}

async function toggleMedia(file) {
    try {
        const btn = document.querySelector('[data-file=""' + file + '""]');
        let oldHtml = null;
        // Sadece ses için işleniyor animasyonu göster
        if (btn && !btn.innerText.includes('Aç')) {
            oldHtml = btn.innerHTML;
            btn.innerHTML = '<span class=""loading""></span> İşleniyor...';
            btn.disabled = true;
        }
        
        // Görsel dosyalar için hızlı güncelleme
        const response = await fetch('/toggle?file=' + encodeURIComponent(file));
        
        // Hemen güncelle ve butonu eski haline döndür
        setTimeout(function() { 
            updateList(); 
        }, 100); // Çok kısa gecikme
        
    } catch (error) { 
        console.error('Medya aç/kapat hatası:', error);
        updateList();
    }
}

let lastMediaList = '';
let isUpdating = false;

async function updateList() {
    if (isUpdating) return;
    isUpdating = true;

    try {
        const response = await fetch('/listhtml');
        const newMediaList = await response.text();

        if (lastMediaList !== newMediaList) {
            requestAnimationFrame(function() {
                document.getElementById(""mediaList"").innerHTML = newMediaList;
            });
            lastMediaList = newMediaList;
        }
    } catch (error) { 
        console.error('Medya listesi güncellenemedi:', error); 
    }

    isUpdating = false;
}

// Sayfa yüklendiğinde slider'ı başlat
document.addEventListener('DOMContentLoaded', function() {
    const slider = document.getElementById(""globalVolumeSlider"");
    slider.style.setProperty('--progress', slider.value + '%');
});

setInterval(function() {
    updateList();
    updateGlobalVolume();
}, 1000);
";
        }

        // Modern HTML generator - card-based responsive design
        private string GenerateModernHtml()
        {
            StringBuilder sb = new StringBuilder();
            
            if (mediaItems == null || mediaItems.Count == 0)
            {
                sb.Append("<div class='empty-state'>");
                sb.Append("<h3>🎵 Henüz medya dosyası yok</h3>");
                sb.Append("<p>Medya dosyalarınız burada görünecek</p>");
                sb.Append("</div>");
                return sb.ToString();
            }

            sb.Append("<div class='media-grid'>\n"); // Her kartı ayrı satırda başlat
            
            foreach (var item in mediaItems)
            {
                string originalFilePath = Path.Combine(Application.StartupPath, item.Path);
                string effectiveFilePath = originalFilePath;
                if (Path.GetExtension(originalFilePath).ToLowerInvariant() != ".wav" && convertedFiles.ContainsKey(originalFilePath))
                    effectiveFilePath = convertedFiles[originalFilePath];

                bool isPlaying = activePlayers.ContainsKey(originalFilePath) || activePlayers.ContainsKey(effectiveFilePath);
                bool isInteractive = !(item.IsVideo || item.IsPresentation);
                
                string mediaType = "";
                string mediaIcon = "";
                if (item.IsImage)
                {
                    mediaType = "image";
                    mediaIcon = "🖼️";
                }
                else if (item.IsVideo)
                {
                    mediaType = "video";
                    mediaIcon = "🎬";
                }
                else if (item.IsPresentation)
                {
                    mediaType = "presentation";
                    mediaIcon = "📊";
                }
                else
                {
                    mediaType = "audio";
                    mediaIcon = "🎵";
                }

                string normalizedItemPath = item.Path.Replace("\\", "/");
                string statusBadge = "";
                string buttonHtml = "";
                string buttonClass = "";
                string cardStyle = isPlaying ? " style='background:linear-gradient(90deg,#ffe0b2,#ffd699,#fff3e0);border-color:#ff9800;'" : "";
                string btnStyle = isPlaying ? " style='background:linear-gradient(135deg,#ff9800,#ffb74d);color:#fff;'" : "";

                if (!isInteractive)
                {
                    statusBadge = "<div class='status-badge'>🚫 Web'de desteklenmiyor</div>";
                    buttonHtml = "<button class='btn btn-disabled' disabled>🚫 Desteklenmiyor</button>";
                }
                else if (item.IsImage)
                {
                    // Resimler için her zaman aktif buton ve hazır etiketi
                    statusBadge = "<div class='status-badge status-ready'>🖼️ Aç</div>";
                    buttonClass = "btn-success";
                    buttonHtml = $"<button class='btn {buttonClass}' data-file='{normalizedItemPath}' onclick=\"toggleMedia('{normalizedItemPath}')\">🖼️ Aç</button>";
                }
                else
                {
                    if (isPlaying)
                    {
                        statusBadge = "<div class='status-badge status-playing'>▶️ Oynatılıyor</div>";
                        buttonClass = "btn-danger";
                        buttonHtml = $"<button class='btn {buttonClass}' data-file='{normalizedItemPath}' onclick=\"toggleMedia('{normalizedItemPath}')\"{btnStyle}>⏹️ Durdur</button>";
                    }
                    else
                    {
                        statusBadge = "<div class='status-badge status-ready'>⏸️ Hazır</div>";
                        buttonClass = "btn-success";
                        buttonHtml = $"<button class='btn {buttonClass}' data-file='{normalizedItemPath}' onclick=\"toggleMedia('{normalizedItemPath}')\">▶️ Oynat</button>";
                    }
                }

                string description = string.IsNullOrEmpty(item.Description) ? "Açıklama bulunmuyor" : item.Description;
                string volumeText = $"{(item.Volume * 100):F0}%";

                sb.Append($"<div class='media-card' {cardStyle}>\n");
                sb.Append(statusBadge + "\n");
                sb.Append("<div class='media-header'>\n");
                sb.Append($"<div class='media-icon {mediaType}'>{mediaIcon}</div>\n");
                sb.Append($"<div class='media-title'>{System.Net.WebUtility.HtmlEncode(item.Name)}</div>\n");
                sb.Append("</div>\n");
                sb.Append($"<div class='media-description'>{System.Net.WebUtility.HtmlEncode(description)}</div>\n");
                sb.Append("<div class='media-controls'>\n");
                sb.Append(buttonHtml + "\n");
                sb.Append("<div class='volume-info'>\n");
                sb.Append($"🔊 {volumeText}\n");
                sb.Append("</div>\n");
                sb.Append("</div>\n");
                sb.Append("</div>\n");
            }
            
            sb.Append("</div>");
            return sb.ToString();
        }

        private string GetDisplayInfo(Screen screen)
        {
            return $"{screen.DeviceName} ({screen.Bounds.Width}x{screen.Bounds.Height})";
        }

        private void WriteResponse(HttpListenerContext context, string responseString)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private Button FindButtonForMediaItem(MediaItem item)
        {
            foreach (Control control in panelButtons.Controls)
            {
                if (control is Panel containerPanel)
                {
                    foreach (Control btnControl in containerPanel.Controls)
                    {
                        if (btnControl is Button btn && btn.Tag is MediaItem btnItem && btnItem == item)
                        {
                            return btn;
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Form kapanırken açık sesleri durdur.
            foreach (var active in activePlayers.Values)
            {
                try
                {
                    active.WaveOut.Stop();
                }
                catch { }
            }
            activePlayers.Clear();

            base.OnFormClosing(e);
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }
        }
    }

    public class MediaItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public bool IsVideo { get; set; }
        public bool IsImage { get; set; }
        public bool IsPresentation { get; set; }
        public double Volume { get; set; }
    }

    public class ActiveSound
    {
        public WaveOutEvent WaveOut { get; set; }
        public AudioFileReader AudioReader { get; set; }
        public Button SourceButton { get; set; }
    }
}
