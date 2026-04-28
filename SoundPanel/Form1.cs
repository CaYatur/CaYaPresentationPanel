using NAudio.Gui;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using SoundPanel.Properties;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Management;
using System.Media;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms;


namespace SoundPanel
{
    public partial class Form1 : Form
    {
        private string localDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private string songsFolder => Path.Combine(localDirectory, "songs");
        private string songsFilePath => Path.Combine(localDirectory, "dataCY.json");
        private List<Song> songs = new();
        private WaveOutEvent waveOutEvent;
        //private AudioFileReader audioFileReader;
        private MediaFoundationReader audioFileReader;
        private VolumeSampleProvider mainVolumeProvider;



        private System.Windows.Forms.Timer playbackTimer;
        private bool isLooping = false;
        private long lastPosition = 0; // Oynatılan son konum
        private float SVolume = 0.5f; // Varsayılan ses seviyesi
        int TotalSecondsTrackbar;


        private bool isDragging = false; // Kullanıcının sürüklediğini takip eder
        private bool wasPlayingBeforeDrag = false; // Sürüklemeden önce oynatma durumunu saklar

        int ListNumber = 0; // Listeleme numarası için başlangıç değeri


        // Eski WMP player'ları yerine
        private Dictionary<string, IWavePlayer> additionalPlayers2 = new Dictionary<string, IWavePlayer>();
        private Dictionary<string, AudioFileReader> additionalReaders2 = new Dictionary<string, AudioFileReader>();

        // Ana player için (eğer yoksa)
        private IWavePlayer mainWaveOut2;
        private AudioFileReader mainAudioReader2;



        // Form1 sınıfınızın içinde aşağıdaki alanları ekleyin: KRONOMETRE
        private System.Windows.Forms.Timer stopwatchTimer;
        private TimeSpan stopwatchElapsed = TimeSpan.Zero;
        private bool stopwatchRunning = false;

        private System.Windows.Forms.Timer clockTimer;

        private volatile bool userScrolled = false;

        private Panel lastMainPlayedPanel = null;

        // Yeni alanlar: Form1 sınıfında uygun bir yere ekleyin (diğer alanların yanında)
        private bool isUserResizing = false;
        private System.Windows.Forms.Timer resizeEndTimer;

        // Aşağıdaki public Form1() constructor'ını mevcut olanın yerine koyun (mevcut içeriğin büyük kısmı korunup
        // çift-tamponlama ve resize olayları eklendi).
        public Form1()
        {
            InitializeComponent();

            // Minimum boyutu burada belirledim
            this.MinimumSize = new Size(950, 678);

            // Double buffering: Form için
            this.SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint
                          | System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer
                          | System.Windows.Forms.ControlStyles.UserPaint, true);
            this.UpdateStyles();

            // Double buffering: FlowLayoutPanel için (korumalı özellik, reflection ile ayarlanır)
            try
            {
                var prop = typeof(FlowLayoutPanel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                prop?.SetValue(flowLayoutPanelSongs, true, null);
            }
            catch
            {
                // ignored
            }

            // Mevcut başlatma kodları
            //Directory.CreateDirectory(songsFolder); // İçeriklar için klasör oluştur
            LoadSongs();
            InitializePlayer();

            lblClock.TextAlign = ContentAlignment.MiddleCenter;
            lblStopwatch.TextAlign = ContentAlignment.MiddleCenter;

            // Form1 constructor'ına ekleyin:
            stopwatchTimer = new System.Windows.Forms.Timer();
            stopwatchTimer.Interval = 1000; // 1 saniye
            stopwatchTimer.Tick += StopwatchTimer_Tick;
            lblStopwatch.Text = "00:00:00";

            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000; // 1 saniye
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();

            // Resize handling: kullanıcı interaktif yeniden boyutlandırma sırasında layout'u askıya al
            this.ResizeBegin += Form1_ResizeBegin;
            this.ResizeEnd += Form1_ResizeEnd;

            // Debounce timer: sürekli Resize olaylarını gruplayıp sonrasında güncelleme yapacak
            resizeEndTimer = new System.Windows.Forms.Timer { Interval = 200 }; // 200ms boş bekleme
            resizeEndTimer.Tick += ResizeEndTimer_Tick;


            Application.AddMessageFilter(new MouseWheelRedirectFilter(flowLayoutPanelSongs, () => userScrolled = true, this));


            FontLoadLocal();
        }





        private async void FontLoadLocal()
        {
            //FontHelper.GetFont("Poppins", 10, FontStyle.Regular);
            //FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
            btnMore.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            btnAddNewSong.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button5.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button3.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button4.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button8.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button6.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button9.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            button7.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            MediaEditMenu.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            customSwitch1.Font = FontHelper.GetFont("Poppins", 7, FontStyle.Bold);
            lblStopwatch.Font = FontHelper.GetFont("Poppins", 16, FontStyle.Bold);
            lblClock.Font = FontHelper.GetFont("Poppins", 11, FontStyle.Bold);
            lblCurrentTime.Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
            lblTotalTime.Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
            label2.Font = FontHelper.GetFont("Poppins", 9, FontStyle.Bold);
            label1.Font = FontHelper.GetFont("Poppins", 9, FontStyle.Bold);
            label4.Font = FontHelper.GetFont("Poppins", 8, FontStyle.Bold);
            button10.Font = FontHelper.GetFont("Poppins", 48, FontStyle.Bold);
        }




        private class MouseWheelRedirectFilter : IMessageFilter
        {
            private readonly FlowLayoutPanel panel;
            private readonly Action markUserScrolled;
            private readonly Form owner;
            private const int WM_MOUSEWHEEL = 0x020A;

            [DllImport("user32.dll")]
            private static extern IntPtr WindowFromPoint(System.Drawing.Point p);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool IsChild(IntPtr hWndParent, IntPtr hWndChild);

            public MouseWheelRedirectFilter(FlowLayoutPanel panel, Action markUserScrolled, Form owner)
            {
                this.panel = panel;
                this.markUserScrolled = markUserScrolled;
                this.owner = owner;
            }

            public bool PreFilterMessage(ref Message m)
            {
                try
                {
                    if (m.Msg != WM_MOUSEWHEEL) return false;

                    // Fare pozisyonu ekran koordinatlarında
                    var cursorPos = Cursor.Position;

                    // Panel ekran dikdörtgeni
                    var panelRect = panel.RectangleToScreen(panel.ClientRectangle);
                    if (!panelRect.Contains(cursorPos)) return false;

                    // Üstte başka bir pencere olup olmadığını kontrol et
                    IntPtr hwndUnderMouse = WindowFromPoint(cursorPos);
                    if (hwndUnderMouse == IntPtr.Zero) return false;
                    if (hwndUnderMouse != owner.Handle && !IsChild(owner.Handle, hwndUnderMouse))
                        return false; // Üstte başka bir pencere var, mesajı tüketme

                    // Eğer panel veya form zaten odaklı ise mesajı tüketme: varsayılan kaydırma çalışsın
                    if (panel.ContainsFocus || (owner != null && owner.ContainsFocus))
                        return false;

                    // Wheel delta (pozitif yukarı, negatif aşağı)
                    int delta = (short)((int)m.WParam >> 16);
                    if (delta == 0) return false;

                    // Kaç satır / piksel kaydırılacağını hesapla
                    int lines = SystemInformation.MouseWheelScrollLines;
                    int pixelsPerLine = 20; // ayarlanabilir
                    int offset = (delta / 120) * lines * pixelsPerLine;

                    // UI thread'e taşı ve scroll uygula
                    panel.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            int max = panel.VerticalScroll.Maximum;
                            int min = panel.VerticalScroll.Minimum;
                            int current = panel.VerticalScroll.Value;
                            int target = Math.Max(min, Math.Min(max, current - offset));
                            ScrollHelper.SetVerticalScroll(panel, target);

                            // Kullanıcı kaydırdı diye işaretle
                            markUserScrolled?.Invoke();

                            // Formu aktive et yalnızca form aktif değilse (odak alma gerekliliğinde)
                            if (owner != null && owner != Form.ActiveForm)
                            {
                                try { owner.Activate(); } catch { /* ignore */ }
                            }
                        }
                        catch
                        {
                            // yoksay
                        }
                    }));

                    // Mesajı tüket: default işlem tekrarı olmaması için true döndür
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void flowLayoutPanelSongs_Scroll(object? sender, ScrollEventArgs e)
        {
            try
            {
                //MessageBox.Show("test");
                // Kullanıcının kaydırma yaptığını işaretle
                userScrolled = true;

                // Kullanıcı etkileşimini vurgulamak için formu aktive et (odak tamamen değişmiyorsa sorun yaratmaz)
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        try { this.Activate(); } catch { /* güvenli yoksay */ }
                    }));
                }
            }
            catch
            {
                // hata yoksay
            }
        }
        private void RestoreFlowScrollAsync(int savedScroll, Control? controlToCheck = null, bool force = false)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!flowLayoutPanelSongs.IsHandleCreated) return;

                    await Task.Delay(10); // küçük gecikme

                    int max = 0;
                    flowLayoutPanelSongs.Invoke(new Action(() => max = flowLayoutPanelSongs.VerticalScroll.Maximum));
                    int target = Math.Max(0, Math.Min(savedScroll, max));

                    // Eğer kullanıcı yakın zamanda manuel kaydırma yaptıysa ve force==false ise restore etmeyelim
                    if (userScrolled && !force)
                        return;

                    if (controlToCheck != null)
                    {
                        bool isVisible = false;
                        flowLayoutPanelSongs.Invoke(new Action(() =>
                        {
                            var viewport = new Rectangle(-flowLayoutPanelSongs.AutoScrollPosition.X,
                                                         -flowLayoutPanelSongs.AutoScrollPosition.Y,
                                                         flowLayoutPanelSongs.ClientSize.Width,
                                                         flowLayoutPanelSongs.ClientSize.Height);
                            isVisible = viewport.IntersectsWith(controlToCheck.Bounds);
                        }));
                        if (!isVisible && !force) return; // görünür değilse ve zorlanmıyorsa restore etme
                    }

                    // Birkaç kez deneme: bazen sistem odak/değişiklikleri scroll'u geri alabilir
                    const int maxAttempts = 6;
                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        int current = 0;
                        flowLayoutPanelSongs.Invoke(new Action(() => current = flowLayoutPanelSongs.VerticalScroll.Value));

                        // Eğer kullanıcı arada scroll'u değiştirdiyse ve biz force==false ise işlemden çekil
                        if (!force && current != savedScroll)
                            return;

                        if (current == target) break;

                        flowLayoutPanelSongs.Invoke(new Action(() =>
                        {
                            try { ScrollHelper.SetVerticalScroll(flowLayoutPanelSongs, target); } catch { }
                        }));

                        await Task.Delay(50);
                    }

                    // Programatik restore başarılıysa, kullanıcı scroll flag'ini sıfırla
                    userScrolled = false;
                }
                catch
                {
                    // hata yoksay (opsiyonel log eklenebilir)
                }
            });
        }


        // Yeni metodlar: Form1 sınıfına ekleyin

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            isUserResizing = true;
            //try
            //{
            //    this.SuspendLayout();
            //    flowLayoutPanelSongs.SuspendLayout();
            //}
            //catch { }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            isUserResizing = false;
            //try
            //{
            //    flowLayoutPanelSongs.ResumeLayout();
            //    this.ResumeLayout();
            //}
            //catch { }

            try
            {
                flowLayoutPanelSongs.Visible = true;
            }
            catch { }

            // Hemen güncelleme yap
            UpdatePanelsWidths();
        }

        private void ResizeEndTimer_Tick(object? sender, EventArgs e)
        {
            resizeEndTimer.Stop();
            if (!isUserResizing)
            {
                flowLayoutPanelSongs.Visible = true;
                UpdatePanelsWidths();
                //flowLayoutPanelSongs.Visible = true;
            }

        }

        private async void UpdatePanelsWidths()
        {
            await Task.Delay(100); // Küçük bir gecikme, gerekirse ayarlanabilir
            if (flowLayoutPanelSongs.InvokeRequired)
            {
                flowLayoutPanelSongs.BeginInvoke(new Action(UpdatePanelsWidths));
                return;
            }

            int newWidth = Math.Max(0, flowLayoutPanelSongs.ClientSize.Width - 20);
            foreach (Control control in flowLayoutPanelSongs.Controls)
            {
                if (control is Panel songPanel)
                {
                    songPanel.Width = newWidth;
                }
            }

            // Gerektiğinde kaydırma minima ayarı
            flowLayoutPanelSongs.AutoScrollMinSize = new Size(0, Math.Max(flowLayoutPanelSongs.Height, 1000));
            flowLayoutPanelSongs.Invalidate();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void StopwatchTimer_Tick(object sender, EventArgs e)
        {
            stopwatchElapsed = stopwatchElapsed.Add(TimeSpan.FromSeconds(1));
            lblStopwatch.Text = stopwatchElapsed.ToString(@"hh\:mm\:ss");
        }
        private void btnStartStopwatch_Click(object sender, EventArgs e)
        {
            // Eğer kronometre çalışmıyorsa başlat veya devam ettir
            if (!stopwatchRunning)
            {
                stopwatchRunning = true;
                stopwatchTimer.Start();
            }
        }

        private void btnStopStopwatch_Click(object sender, EventArgs e)
        {
            // Tamamen durdur ve sıfırla
            stopwatchRunning = false;
            stopwatchTimer.Stop();
            stopwatchElapsed = TimeSpan.Zero;
            lblStopwatch.Text = "00:00:00";
        }

        private void btnResumeStopwatch_Click(object sender, EventArgs e)
        {
            // Eğer kronometre çalışıyorsa duraklat
            if (stopwatchRunning)
            {
                stopwatchRunning = false;
                stopwatchTimer.Stop();
            }
            // Eğer zaten duraklatılmışsa hiçbir şey yapma
        }


        private void InitializePlayer()
        {
            playbackTimer = new System.Windows.Forms.Timer
            {
                Interval = 500 // 1 saniye
            };
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (audioFileReader != null && !isDragging) // Kullanıcı sürüklemiyorsa güncelle
            {
                double progress = (audioFileReader.Position / (double)audioFileReader.Length) * trackBarProgress.Maximum;
                int newValue = (int)Math.Round(progress);

                // TrackBar sınırlarını aşmaması için kontrol ekleyelim
                newValue = Math.Max(trackBarProgress.Minimum, Math.Min(trackBarProgress.Maximum, newValue));

                trackBarProgress.Value = newValue;

                lblCurrentTime.Text = TimeSpan.FromSeconds(audioFileReader.Position / audioFileReader.WaveFormat.AverageBytesPerSecond)
                    .ToString(@"mm\:ss");
            }
        }

        private void trackBarProgress_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;

            if (audioFileReader != null && waveOutEvent.PlaybackState == PlaybackState.Playing)
            {
                wasPlayingBeforeDrag = true;
                waveOutEvent.Pause(); // Sürükleme başladığında ses duraklat
            }
            else
            {
                wasPlayingBeforeDrag = false;
            }
        }

        private void trackBarProgress_MouseUp(object sender, MouseEventArgs e)
        {
            if (audioFileReader != null)
            {
                double newPosition = (trackBarProgress.Value / (double)trackBarProgress.Maximum) * audioFileReader.TotalTime.TotalSeconds;
                audioFileReader.Position = (long)(newPosition * audioFileReader.WaveFormat.AverageBytesPerSecond);

                if (wasPlayingBeforeDrag)
                {
                    waveOutEvent.Play(); // Eğer oynuyorduysa devam ettir
                }
            }

            isDragging = false;
        }




        private void trackBarProgress_ScrollStart(object sender, EventArgs e)
        {
            isDragging = true;
            playbackTimer?.Stop();

            if (audioFileReader != null && waveOutEvent != null && waveOutEvent.PlaybackState == PlaybackState.Playing)
            {
                wasPlayingBeforeDrag = true;
                try { waveOutEvent.Pause(); } catch { }
            }
            else
            {
                wasPlayingBeforeDrag = false;
            }
        }

        private async void trackBarProgress_ScrollEnd(object sender, EventArgs e)
        {
            isDragging = false;

            if (audioFileReader != null && waveOutEvent != null)
            {
                double newPosition = (trackBarProgress.Value / (double)trackBarProgress.Maximum) * audioFileReader.TotalTime.TotalSeconds;
                try
                {
                    audioFileReader.CurrentTime = TimeSpan.FromSeconds(Math.Max(0, Math.Min(newPosition, audioFileReader.TotalTime.TotalSeconds)));
                }
                catch { /* bazı formatlarda seek başarısız olabilir, hata yoksay */ }

                playbackTimer?.Start();

                if (wasPlayingBeforeDrag)
                {
                    waveOutEvent.Play();
                    float TempVolIN;
                    TempVolIN = waveOutEvent.Volume;
                    waveOutEvent.Volume = 0;
                    await Task.Delay(300); // Küçük bir gecikme
                    waveOutEvent.Volume = TempVolIN;
                }
            }
        }






        private List<Song> songList = new List<Song>(); // İçerik listesi


        private string currentSongPath = ""; // Geçerli şarkının yolunu tutan değişken
        //private Dictionary<string, float> songVolumes = new Dictionary<string, float>(); // Her şarkının özel ses düzeyi
        // Şarkılar için ses düzeyini tutan bir sözlük
        Dictionary<string, float> songVolumes = new Dictionary<string, float>();


        // playerLock nesnesi, asenkron erişimi senkronize etmek için kullanılır.
        private static readonly SemaphoreSlim playerSemaphore = new SemaphoreSlim(1, 1); // 1 thread'e izin veriyoruz
        private Dictionary<string, WMPLib.WindowsMediaPlayer> additionalPlayers = new Dictionary<string, WMPLib.WindowsMediaPlayer>();
        private HashSet<string> currentlyPlaying = new HashSet<string>(); // Çalan şarkılar listesi
        private const int MaxSongs = 3; // Maksimum şarkı sayısı


        private void CreateSongUI(Song song)
        {

            // Paneli oluştur
            Panel songPanel = new Panel
            {
                Width = flowLayoutPanelSongs.ClientSize.Width - 20,
                Height = 70, // YENİ: 60'dan 70'e çıkarıldı
                BackColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnListNumber = new CustomButton
            {
                Name = "btnListNumber",
                Text = "1", // Geçici değer
                Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold),
                Size = new Size(42, 42),
                Location = new Point(5, 15),
                CornerRadius = 2, // Daha yuvarlak
                BorderThickness = 0,
                ShowBorder = true,
                //BorderColor = ColorTranslator.FromHtml("#4e5772"),
                BorderColor = ColorTranslator.FromHtml("#515f8b"),
                ButtonBackColor = ColorTranslator.FromHtml("#515f8b"),
                HoverColor = ColorTranslator.FromHtml("#505e8a"),
                PressedColor = ColorTranslator.FromHtml("#505e8a"),
                TextColor = Color.White,
                //ButtonImage = Image.FromFile("youricon.png"),
                ImageAlign = ContentAlignment.MiddleLeft,
                ImagePadding = new Padding(10, 0, 0, 0),
                //Enabled = false, // Buton devre dışı bırakıldı
            };
            songPanel.Controls.Add(btnListNumber);
            UpdateListNumbers();

            var btnTik = new CustomButton
            {
                Text = "",
                Size = new Size(43, 42),
                Location = new Point(49, 15),
                CornerRadius = 2, // Daha yuvarlak
                BorderThickness = 0,
                ShowBorder = true,
                //BorderColor = ColorTranslator.FromHtml("#4e5772"),
                BorderColor = ColorTranslator.FromHtml("#63c6b3"),
                ButtonBackColor = ColorTranslator.FromHtml("#63c6b3"),
                HoverColor = ColorTranslator.FromHtml("#499384"), // Hover rengi
                PressedColor = ColorTranslator.FromHtml("#22453f"), // Basılı rengi
                TextColor = ColorTranslator.FromHtml("#1e2433"),
                //ButtonImage = Image.FromFile("youricon.png"),
                ImageAlign = ContentAlignment.MiddleLeft,
                ImagePadding = new Padding(10, 0, 0, 0),
                //Enabled = false, // Buton devre dışı bırakıldı
            };
            songPanel.Controls.Add(btnTik);
            bool toggleTik = false; // Tik butonunun durumunu takip etmek için
            btnTik.Click += (s, e) =>
            {
                // ToggleTik metodunu çağır
                SongPanelHelpers.ToggleTik(btnTik, ref toggleTik);
            };

            //btnTik.MouseUp += (s, e) =>
            //{
            //    if (e.Button == MouseButtons.Left)
            //    {
            //        if (toggleTik)
            //        {
            //            // Tik butonuna tıklandığında, tik işaretini kaldır
            //            toggleTik = false;
            //            btnTik.Text = "";
            //        }
            //        else
            //        {
            //            // Tik butonuna tıklandığında, tik işaretini ekle
            //            toggleTik = true;
            //            btnTik.Text = "✅";
            //        }
            //    }
            //};




            var btnContent = new CustomButton
            {
                Text = "",
                Size = new Size(42, 42),
                Location = new Point(94, 15),
                CornerRadius = 2, // Daha yuvarlak
                BorderThickness = 0,
                ShowBorder = true,
                //BorderColor = ColorTranslator.FromHtml("#4e5772"),
                BorderColor = ColorTranslator.FromHtml("#c2c6d3"),
                ButtonBackColor = ColorTranslator.FromHtml("#c2c6d3"),
                TextColor = Color.White,
                //ButtonImage = Image.FromFile("youricon.png"),
                ImageAlign = ContentAlignment.MiddleLeft,
                ImagePadding = new Padding(10, 0, 0, 0),
                //Enabled = false, // Buton devre dışı bırakıldı
            };
            //songPanel.Controls.Add(btnContent);


            //

            var btnPlay = new CustomButton
            {
                Text = song.Name,
                Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(150, 42),
                Location = new Point(138, 15),
                CornerRadius = 2, // Daha yuvarlak
                BorderThickness = 0,
                ShowBorder = true,
                Tag = song,
                //BorderColor = ColorTranslator.FromHtml("#4e5772"),
                BorderColor = ColorTranslator.FromHtml("#4fc8e5"),
                ButtonBackColor = ColorTranslator.FromHtml("#4fc8e5"),
                TextColor = Color.White,
                HoverColor = ColorTranslator.FromHtml("#3d9bb2"), // Hover rengi
                PressedColor = ColorTranslator.FromHtml("#3d9bb2"), // Basılı rengi
                //ButtonImage = Image.FromFile("youricon.png"),
                ImageAlign = ContentAlignment.MiddleLeft,
                ImagePadding = new Padding(10, 0, 0, 0),
                //Enabled = false, // Buton devre dışı bırakıldı
            };
            //songPanel.Controls.Add(btnContent);


            var btnPlayAdditional = new CustomButton
            {
                Text = "Ekle",
                Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(42, 42),
                Location = new Point(290, 15),
                CornerRadius = 2, // Daha yuvarlak
                BorderThickness = 0,
                ShowBorder = true,
                Tag = song,
                //BorderColor = ColorTranslator.FromHtml("#4e5772"),
                BorderColor = ColorTranslator.FromHtml("#9292ff"),
                ButtonBackColor = ColorTranslator.FromHtml("#9292ff"),
                TextColor = Color.White,
                HoverColor = ColorTranslator.FromHtml("#6a6aff"), // Hover rengi
                PressedColor = ColorTranslator.FromHtml("#6a6aff"), // Basılı rengi
                //ButtonImage = Image.FromFile("youricon.png"),
                ImageAlign = ContentAlignment.MiddleLeft,
                ImagePadding = new Padding(10, 0, 0, 0),
                //Enabled = false, // Buton devre dışı bırakıldı
            };
            //songPanel.Controls.Add(btnContent);

            //ListNumber = ListNumber + 1;
            //Listeleme Numaraları
            //Button btnListNumber = new Button
            //{
            //    Text = ListNumber.ToString(),
            //    Width = 42,
            //    Height = 42,
            //    Font = new Font("Poppins", 10, FontStyle.Bold),
            //    ForeColor = ColorTranslator.FromHtml("#ffffff"), // İçeriklar için yazı rengi
            //    Location = new Point(5, 15),
            //    BackColor = ColorTranslator.FromHtml("#ffffff"),
            //    FlatStyle = FlatStyle.Flat,
            //    Enabled = false,
            //    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            //};
            //btnListNumber.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#4e5772"); // Dış çerçeve rengi
            //btnListNumber.FlatAppearance.BorderSize = 2; // Çerçeve kalınlığı (isteğe bağlı)
            //btnListNumber.FlatAppearance.BorderSize = 1;
            //btnListNumber.Click += (s, e) =>
            //{

            //};

            //Tik Onay
            //Button btnTik = new Button
            //{
            //    Text = "✅",
            //    Width = 42,
            //    Height = 42, // YENİ: 30'dan 40'a çıkarıldı
            //    //Location = new Point(songPanel.Width - 50, 15),
            //    Location = new Point(48, 15),
            //    BackColor = Color.LightBlue,
            //    FlatStyle = FlatStyle.Flat,
            //    Enabled = false, // Buton devre dışı bırakıldı
            //    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            //};
            //btnTik.FlatAppearance.BorderSize = 1;
            //btnTik.Click += (s, e) =>
            //{

            //};

            ////İçerik işareti renkleri
            //Button btnContent = new Button
            //{
            //    Text = "",
            //    Width = 42,
            //    Height = 42, // YENİ: 30'dan 40'a çıkarıldı
            //    //Location = new Point(songPanel.Width - 50, 15),
            //    Location = new Point(92, 15),
            //    BackColor = Color.LightCoral,
            //    FlatStyle = FlatStyle.Flat,
            //    Enabled = false, // Buton devre dışı bırakıldı
            //    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            //};
            //btnContent.FlatAppearance.BorderSize = 1;
            //btnContent.Click += (s, e) =>
            //{

            //};


            //// Play button for audio or video
            //Button btnPlay = new Button
            //{
            //    Text = song.Name,
            //    Width = 150,
            //    Height = 40, // YENİ: 40'dan 50'ye çıkarıldı
            //    Location = new Point(135, 15),
            //    Tag = song,  // Store the entire Song object in the Tag
            //    BackColor = Color.LightGray,
            //    ForeColor = Color.Black,
            //    FlatStyle = FlatStyle.Flat
            //};
            //btnPlay.FlatAppearance.BorderSize = 1;


            //// **Yeni "Ek Oynat" butonu (Eklenen buton)**
            //Button btnPlayAdditional = new Button
            //{
            //    Text = "Ekle",
            //    Width = 40,
            //    Height = 40, // YENİ: 40'dan 50'ye çıkarıldı
            //    Location = new Point(287, 15), // Oynatma butonunun yanına koyduk
            //    Tag = song,
            //    BackColor = Color.LightBlue,
            //    ForeColor = Color.Black,
            //    FlatStyle = FlatStyle.Flat
            //};
            //btnPlayAdditional.FlatAppearance.BorderSize = 1;


            //// Açıklama etiketi
            //Label lblDescription = new Label
            //{
            //    Text = song.Description,
            //    Location = new Point(160, 15),
            //    AutoSize = true,
            //    ForeColor = Color.White,
            //    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            //};


            // Açıklama için RichTextBox
            RichTextBox rtbDescription = new RichTextBox
            {
                Text = song.Description,
                Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold),
                //Location = new Point(340, 10), // Sağda ve biraz yukarıda olacak şekilde
                Location = new Point(songPanel.Width - 500, 10),
                Size = new Size(330, 50), // YENİ: 40'dan 50'ye çıkarıldı
                ReadOnly = true,
                //Enabled = false,
                //BackColor = ColorTranslator.FromHtml("#171717"),
                //ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#ffffff"),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            //// "Açıklamayı Değiştir" butonunu ekleyelim
            //Button btnChangeDescription = new Button
            //{
            //    Text = "🖋️",
            //    Width = 35,
            //    Height = 35, // YENİ: 30'dan 40'a çıkarıldı
            //    // RichTextBox'ın sağ tarafına yerleştirelim
            //    Location = new Point(songPanel.Width - 90, 15),
            //    Tag = song,
            //    BackColor = Color.LightYellow,
            //    FlatStyle = FlatStyle.Flat,
            //    Anchor = AnchorStyles.Top | AnchorStyles.Right
            //};
            //btnChangeDescription.FlatAppearance.BorderSize = 1;
            ////btnChangeDescription.Click += (s, e) =>
            ////{
            ////    string newDesc = Prompt.ShowDialog("Yeni açıklamayı girin:", "Açıklamayı Değiştir");
            ////    if (!string.IsNullOrWhiteSpace(newDesc))
            ////    {
            ////        song.Description = newDesc;
            ////        rtbDescription.Text = newDesc;
            ////        SaveSongs();
            ////    }
            ////};
            //btnChangeDescription.Click += (s, e) =>
            //{
            //    // Mevcut açıklama, Prompt'a başlangıç değeri olarak gönderiliyor
            //    string newDesc = Prompt.ShowDialog("Yeni açıklamayı girin:", "Açıklamayı Değiştir", song.Description);
            //    if (!string.IsNullOrWhiteSpace(newDesc))
            //    {
            //        song.Description = newDesc;
            //        rtbDescription.Text = newDesc;
            //        SaveSongs();
            //    }
            //};
            //songPanel.Controls.Add(btnChangeDescription);


            //// Sil butonunu oluştur
            //Button btnRemove = new Button
            //{
            //    Text = "X",
            //    Width = 35,
            //    Height = 35, // YENİ: 30'dan 40'a çıkarıldı
            //    Location = new Point(songPanel.Width - 50, 15),
            //    Tag = song,
            //    BackColor = Color.LightCoral,
            //    FlatStyle = FlatStyle.Flat,
            //    Anchor = AnchorStyles.Top | AnchorStyles.Right
            //};
            //btnRemove.FlatAppearance.BorderSize = 1;
            //btnRemove.Click += (s, e) =>
            //{
            //    var result = MessageBox.Show("Silmek istediğinizden emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (result == DialogResult.Yes)
            //    {
            //        RemoveSong(song, songPanel);
            //    }
            //};


            //// Ok butonları
            //FlowLayoutPanel arrowPanel = new FlowLayoutPanel
            //{
            //    FlowDirection = FlowDirection.TopDown,
            //    Width = 60,
            //    Height = 60, // YENİ: 60'dan 70'e çıkarıldı
            //    Location = new Point(songPanel.Width - 125, 2),
            //    AutoSize = true,
            //    WrapContents = false,
            //    Anchor = AnchorStyles.Top | AnchorStyles.Right
            //};

            //Button btnUp = new Button
            //{
            //    Text = "↑",
            //    Width = 25,
            //    Height = 25, // YENİ: 25'ten 30'a çıkarıldı
            //    BackColor = Color.LightGreen,
            //    FlatStyle = FlatStyle.Flat
            //};
            //btnUp.FlatAppearance.BorderSize = 1;
            //btnUp.Click += (s, e) => MovePanelUp(songPanel);

            //Button btnDown = new Button
            //{
            //    Text = "↓",
            //    Width = 25,
            //    Height = 25, // YENİ: 25'ten 30'a çıkarıldı
            //    BackColor = Color.LightBlue,
            //    FlatStyle = FlatStyle.Flat
            //};
            //btnDown.FlatAppearance.BorderSize = 1;
            //btnDown.Click += (s, e) => MovePanelDown(songPanel);

            //btnUp.Location = new Point(0, 0);
            //btnDown.Location = new Point(0, 35); // YENİ: 30'dan 35'e çıkarıldı

            // "Açıklamayı Değiştir" butonunu ekleyelim
            CustomButton btnChangeDescription = new CustomButton
            {
                //Text = "Edit",
                ButtonImage = Properties.Resources.edit, // Düzeltme simgesi
                ImageScaleReduction = 17,
                Width = 35,
                Height = 35,
                // RichTextBox'ın sağ tarafına yerleştirelim
                Location = new Point(songPanel.Width - 90, 15),
                Tag = song,
                ButtonBackColor = Color.FromArgb(80, 172, 255), // Mavi
                //BorderColor = Color.FromArgb(78, 87, 114),
                BorderColor = Color.FromArgb(80, 172, 255),
                HoverColor = Color.FromArgb(64, 137, 204),
                PressedColor = Color.FromArgb(56, 120, 178),
                TextColor = Color.FromArgb(33, 33, 33), // Koyu gri
                ShowBorder = true,
                BorderThickness = 0,
                CornerRadius = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold),
            };

            btnChangeDescription.Click += (s, e) =>
            {
                // Mevcut açıklama, Prompt'a başlangıç değeri olarak gönderiliyor
                string newDesc = Prompt.ShowDialog("Yeni açıklamayı girin:", "Açıklamayı Değiştir", song.Description);
                if (!string.IsNullOrWhiteSpace(newDesc))
                {
                    song.Description = newDesc;
                    rtbDescription.Text = newDesc;
                    SaveSongs();
                }
            };
            songPanel.Controls.Add(btnChangeDescription);

            // Sil butonunu oluştur
            CustomButton btnRemove = new CustomButton
            {
                //Text = "X",
                ButtonImage = Properties.Resources.delete, // Silme simgesi
                ImageScaleReduction = 17,
                Width = 35,
                Height = 35,
                Location = new Point(songPanel.Width - 50, 15),
                Tag = song,
                ButtonBackColor = Color.FromArgb(244, 67, 54), // Kırmızı
                //BorderColor = Color.FromArgb(78, 87, 114),
                BorderColor = Color.FromArgb(244, 67, 54),
                HoverColor = Color.FromArgb(255, 87, 74),
                PressedColor = Color.FromArgb(183, 28, 28),
                TextColor = Color.White,
                ShowBorder = true,
                BorderThickness = 0,
                CornerRadius = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold),
            };

            btnRemove.Click += (s, e) =>
            {
                var result = MessageBox.Show("Silmek istediğinizden emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    RemoveSong(song, songPanel);
                }
            };
            songPanel.Controls.Add(btnRemove);

            // Ok butonları
            FlowLayoutPanel arrowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Width = 60,
                Height = 70, // YENİ: 60'dan 70'e çıkarıldı
                Location = new Point(songPanel.Width - 126, -1),
                AutoSize = true,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.Transparent
            };

            CustomButton btnUp = new CustomButton
            {
                //Text = "↑",
                ButtonImage = Properties.Resources.uparrow, // YENİ: Ok simgesi eklendi
                Width = 25,
                Height = 29, // YENİ: 25'ten 30'a çıkarıldı
                ButtonBackColor = Color.FromArgb(38, 44, 64), // Yeşil
                BorderColor = Color.FromArgb(78, 87, 114),
                HoverColor = Color.FromArgb(6, 7, 10),
                PressedColor = Color.FromArgb(6, 7, 10),
                TextColor = Color.White,
                ShowBorder = true,
                BorderThickness = 0,
                CornerRadius = 2,
                ImageScaleReduction = 7,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            btnUp.Click += (s, e) => MovePanelUp(songPanel);

            CustomButton btnDown = new CustomButton
            {
                //Text = "↓",
                ButtonImage = Properties.Resources.downarrow, // YENİ: Ok simgesi eklendi
                Width = 25,
                Height = 29, // YENİ: 25'ten 30'a çıkarıldı
                ButtonBackColor = Color.FromArgb(38, 44, 64),
                BorderColor = Color.FromArgb(78, 87, 114),
                HoverColor = Color.FromArgb(6, 7, 10),
                PressedColor = Color.FromArgb(6, 7, 10),
                TextColor = Color.White,
                ShowBorder = true,
                BorderThickness = 0,
                CornerRadius = 2,
                ImageScaleReduction = 7,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            btnDown.Click += (s, e) => MovePanelDown(songPanel);

            btnUp.Location = new Point(0, 0);
            btnDown.Location = new Point(0, 3); // YENİ: 30'dan 35'e çıkarıldı

            //songPanel.Controls.Add(btnListNumber);
            //songPanel.Controls.Add(btnTik);
            //songPanel.Controls.Add(btnContent);

            arrowPanel.Controls.Add(btnUp);
            arrowPanel.Controls.Add(btnDown);

            songPanel.Controls.Add(arrowPanel);
            songPanel.Controls.Add(btnRemove);
            songPanel.Controls.Add(btnPlay);
            //songPanel.Controls.Add(lblDescription);
            // RichTextBox'ı şarkı paneline ekle
            songPanel.Controls.Add(rtbDescription);

            flowLayoutPanelSongs.Controls.Add(songPanel);

            songPanel.Controls.Add(btnContent);

            UpdateArrowButtons();





            if (song.IsVideo)
            {


                btnContent.ButtonBackColor = ColorTranslator.FromHtml("#f47a8f");
                btnContent.HoverColor = ColorTranslator.FromHtml("#f47a8f");
                btnContent.PressedColor = ColorTranslator.FromHtml("#f47a8f");

                btnContent.ButtonImage = Properties.Resources.Video;

                songPanel.BackColor = ColorTranslator.FromHtml("#ffffff");
                rtbDescription.BackColor = ColorTranslator.FromHtml("#ffffff");
                //songPanel.BackColor = Color.GreenYellow;
                //rtbDescription.BackColor = Color.GreenYellow;
                //btnPlay.Click += (s, e) => PlayVideo(song.Path); // Play Video logic

                btnPlay.Click += async (s, e) =>
                {
                    int scrollValue = flowLayoutPanelSongs.VerticalScroll.Value;

                    // Odak/manipülasyon kaldırıldı (ActiveControl/Focus tetikleyicilerini engellemek için)
                    toggleTik = false; // Tik butonunu sıfırla
                    SongPanelHelpers.ToggleTik(btnTik, ref toggleTik);

                    btnPlay.Enabled = false;

                    // Video oynatmayı başlat (PlayVideo içinde ekran seçimi yapılıyor)
                    PlayVideo(song.Path, scrollValue);

                    // Kısa bekleme: layout / dialog işlemleri tamamlansın
                    await Task.Delay(1);

                    // Butonu tekrar etkinleştir ve scroll'u geri yükle
                    btnPlay.Enabled = true;
                    RestoreFlowScrollAsync(scrollValue, songPanel); // kontrolü veriyoruz; kullanıcı arada kaydırdıysa restore yapılmaz
                    flowLayoutPanelSongs.Invalidate();
                };

                songPanel.Click += async (s, e) =>
                {
                    //songPanel.BackColor = Color.GreenYellow;
                };


            }
            else if (song.IsImage)
            {
                btnContent.ButtonBackColor = ColorTranslator.FromHtml("#7994f7");
                btnContent.HoverColor = ColorTranslator.FromHtml("#7994f7");
                btnContent.PressedColor = ColorTranslator.FromHtml("#7994f7");

                btnContent.ButtonImage = Properties.Resources.Image;

                songPanel.BackColor = ColorTranslator.FromHtml("#ffffff");
                rtbDescription.BackColor = ColorTranslator.FromHtml("#ffffff");
                // Resim içerikleri için panel arka plan rengini turuncu yapalım
                //songPanel.BackColor = Color.Orange;
                //rtbDescription.BackColor = Color.Orange;
                // Resim dosyası için: Ekran seçimi yapılmışsa doğrudan arka planı güncelle,
                // yoksa ekran seçimi formunu göster

                btnPlay.Click += async (s, e) =>
                {
                    btnPlay.Enabled = false;
                    int scrollValue = flowLayoutPanelSongs.VerticalScroll.Value;
                    toggleTik = false; // Tik butonunu sıfırla
                    SongPanelHelpers.ToggleTik(btnTik, ref toggleTik);
                    await Task.Delay(1);

                    // --- EKRAN SEÇİMİ KONTROLÜ ---
                    if (BackgroundImageForm.SelectedScreen == null)
                    {
                        var selectedScreen = ShowScreenSelectionDialog();
                        if (selectedScreen == null)
                        {
                            // Kullanıcı iptal etti: butonu aktif et ve scroll'u geri yükle
                            btnPlay.Enabled = true;
                            RestoreFlowScrollAsync(scrollValue, songPanel); // kontrolü veriyoruz; kullanıcı arada kaydırdıysa restore yapılmaz
                            return;
                        }
                        BackgroundImageForm.SelectedScreen = selectedScreen; // Seçimi sakla!
                    }

                    var screen = BackgroundImageForm.SelectedScreen;
                    if (screen != null)
                    {
                        if (BackgroundImageForm.Instance != null)
                        {
                            BackgroundImageForm.Instance.UpdateImage(song.Path);
                            BackgroundImageForm.Instance.Bounds = screen.Bounds;
                            BackgroundImageForm.Instance.Tag = song.Path;
                            // odak değiştirmiyoruz (focus çağrıları kaldırıldı)
                        }
                        else
                        {
                            BackgroundImageForm bgForm = new BackgroundImageForm(song.Path)
                            {
                                StartPosition = FormStartPosition.Manual,
                                Bounds = screen.Bounds,
                                Tag = song.Path // yolu sakla ki manager göstersin
                            };
                            bgForm.Show();
                            // odak değiştirmiyoruz
                        }
                        // Manager penceresini aç veya ön plana getir (kapanış/kontrol için)
                        BackgroundImageControlForm.ShowOrFocus(this);
                    }

                    await Task.Delay(1); // layout/still işlemleri tamamlanana kadar kısa bekleme
                    btnPlay.Enabled = true;
                    RestoreFlowScrollAsync(scrollValue, songPanel); // kontrolü veriyoruz; kullanıcı arada kaydırdıysa restore yapılmaz
                };
                songPanel.Click += async (s, e) =>
                {
                    //songPanel.BackColor = Color.Orange;
                };

                //songPanel.Controls.Add(btnPlay);
            }
            else if (song.IsPresentation)
            {
                btnContent.ButtonBackColor = ColorTranslator.FromHtml("#f99c78");
                btnContent.HoverColor = ColorTranslator.FromHtml("#f99c78");
                btnContent.PressedColor = ColorTranslator.FromHtml("#f99c78");

                btnContent.ButtonImage = Properties.Resources.Presentation;

                songPanel.BackColor = ColorTranslator.FromHtml("#ffffff");
                rtbDescription.BackColor = ColorTranslator.FromHtml("#ffffff");
                // Sunum içerikleri için panel rengi (örneğin, MediumPurple)
                //songPanel.BackColor = Color.MediumPurple;
                //rtbDescription.BackColor = Color.MediumPurple;
                btnPlay.Click += async (s, e) =>
                {
                    int scrollValue = flowLayoutPanelSongs.VerticalScroll.Value;
                    await Task.Delay(1);
                    this.ActiveControl = null;
                    btnPlay.Enabled = false; // Butonu devre dışı bırak
                    //songPanel.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                    toggleTik = false; // Tik butonunu sıfırla
                    SongPanelHelpers.ToggleTik(btnTik, ref toggleTik);
                    //songPanel.BackColor = Color.Purple;
                    // Parametre alan oluşturucu kullanılarak sunum dosyası açılır.
                    // Eğer song.Path dolu ise, dosya seçimi yapılmaz.
                    OpenPowerPointWithOutPatch pptViewer = new OpenPowerPointWithOutPatch(song.Path);

                    pptViewer.FormClosed += async (s, e) =>
                    {
                        // Video oynatıcı kapandıktan sonra çalışacak kod
                        await Task.Delay(1000); // Küçük bir gecikme ekle
                        flowLayoutPanelSongs.VerticalScroll.Value = Math.Min(scrollValue, flowLayoutPanelSongs.VerticalScroll.Maximum);
                        //MessageBox.Show("Video oynatıcı kapandı. " + scrollValue, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };
                    flowLayoutPanelSongs.Invalidate();
                    pptViewer.Show();
                    await Task.Delay(1);
                    btnPlay.Enabled = true; // Butonu devre dışı bırak
                    flowLayoutPanelSongs.VerticalScroll.Value = Math.Min(scrollValue, flowLayoutPanelSongs.VerticalScroll.Maximum);
                    flowLayoutPanelSongs.Invalidate();
                };
                songPanel.Click += async (s, e) =>
                {
                    //songPanel.BackColor = Color.MediumPurple;
                };
                //songPanel.Controls.Add(btnPlay);
            }
            else
            {
                btnContent.ButtonBackColor = ColorTranslator.FromHtml("#c993e5");
                btnContent.HoverColor = ColorTranslator.FromHtml("#c993e5");
                btnContent.PressedColor = ColorTranslator.FromHtml("#c993e5");

                //songPanel.BackColor = ColorTranslator.FromHtml("#171717");
                songPanel.BackColor = ColorTranslator.FromHtml("#ffffff");
                rtbDescription.BackColor = ColorTranslator.FromHtml("#ffffff");

                btnContent.ButtonImage = Properties.Resources.song;


                string songPath = song.Path;


                btnPlay.Click += async (s, e) =>
                {
                    int scrollValue = flowLayoutPanelSongs.VerticalScroll.Value;

                    // Odak veya ActiveControl ile oynamayın — bu kaydırmayı sıfırlayabilir
                    toggleTik = false;
                    SongPanelHelpers.ToggleTik(btnTik, ref toggleTik);

                    btnPlay.Enabled = false;

                    // Ana oynatıcıyı başlat (mevcut PlaySong metodunuzu kullanmaya devam edin)
                    PlaySong(song.Path, songPanel, rtbDescription);

                    // Küçük bir bekleme ile scroll restore et (işlem async olabilir; BeginInvoke ile UI'ı bekliyoruz)
                    await Task.Delay(1);
                    RestoreFlowScrollAsync(scrollValue, songPanel); // kontrolü veriyoruz; kullanıcı arada kaydırdıysa restore yapılmaz

                    // UI güncellemeleri
                    btnPlay.Enabled = true;
                    songPanel.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                    rtbDescription.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                };

                songPanel.Click += async (s, e) =>
                {
                    //songPanel.BackColor = Color.Gray;
                    //songPanel.BackColor = ColorTranslator.FromHtml("#171717");
                };




                btnPlayAdditional.Click += async (s, e) =>
                {
                    toggleTik = false;
                    SongPanelHelpers.ToggleTik(btnTik, ref toggleTik);
                    btnPlayAdditional.Enabled = false;

                    await Task.Run(async () =>
                    {
                        await playerSemaphore.WaitAsync();
                        try
                        {
                            bool needDispose = false;
                            IWavePlayer player = null;
                            AudioFileReader reader = null;
                            bool playerExists = false;

                            lock (additionalPlayers2)
                            {
                                playerExists = additionalPlayers2.TryGetValue(songPath, out player) &&
                                              additionalReaders2.TryGetValue(songPath, out reader);
                            }

                            if (playerExists && player != null && reader != null)
                            {
                                if (player.PlaybackState == PlaybackState.Playing)
                                {
                                    // Durdur (arka planda). UI arkaplan resetini TryResetPanelBackground ile yapıyoruz
                                    player.Stop();
                                    needDispose = true;

                                    btnPlayAdditional.BeginInvoke(new Action(() =>
                                    {
                                        btnPlayAdditional.ButtonBackColor = ColorTranslator.FromHtml("#9292ff");
                                        btnPlayAdditional.HoverColor = ColorTranslator.FromHtml("#6a6aff");
                                        btnPlayAdditional.Text = "Ekle";
                                        btnPlayAdditional.Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
                                        btnPlayAdditional.Invalidate();

                                        // Panel arkaplanını doğrudan beyaza çevirmek yerine
                                        // ana oynatıcı / ek oynatıcı durumuna göre sıfırlayan yardımcı metodu çağır
                                        TryResetPanelBackground(songPath, songPanel, rtbDescription);
                                    }));
                                }
                                else
                                {
                                    // Oynat
                                    btnPlayAdditional.BeginInvoke(new Action(() =>
                                    {
                                        songPanel.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                                        rtbDescription.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                                    }));
                                    player.Play();

                                    btnPlayAdditional.BeginInvoke(new Action(() =>
                                    {
                                        btnPlayAdditional.ButtonBackColor = ColorTranslator.FromHtml("#ff4f6c");
                                        btnPlayAdditional.HoverColor = ColorTranslator.FromHtml("#cc3f56");
                                        btnPlayAdditional.Text = "Dur";
                                        btnPlayAdditional.Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
                                        btnPlayAdditional.Invalidate();
                                        UpdateAdditionalPlayerVolume2(songPath);
                                    }));
                                }
                            }
                            else
                            {
                                // Yeni player oluştur
                                var newReader = new AudioFileReader(songPath);
                                var newPlayer = new WaveOutEvent();

                                float songVolume = songVolumes.ContainsKey(songPath) ? songVolumes[songPath] : 1f;
                                float globalVolume = trackSVolume.Value / 100f;
                                newReader.Volume = songVolume * globalVolume;

                                newPlayer.Init(newReader);

                                lock (additionalPlayers2)
                                {
                                    additionalPlayers2[songPath] = newPlayer;
                                    additionalReaders2[songPath] = newReader;
                                }

                                // PlaybackStopped event'i ekle
                                newPlayer.PlaybackStopped += (sender, args) =>
                                {
                                    btnPlayAdditional.BeginInvoke(new Action(() =>
                                    {
                                        btnPlayAdditional.ButtonBackColor = ColorTranslator.FromHtml("#9292ff");
                                        btnPlayAdditional.HoverColor = ColorTranslator.FromHtml("#6a6aff");
                                        btnPlayAdditional.Text = "Ekle";
                                        btnPlayAdditional.Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
                                        btnPlayAdditional.Invalidate();

                                        // Çalmayı bitirdiğinde panelin arkaplanını yalnızca
                                        // ana oynatıcı çalmıyorsa sıfırlayacak şekilde kontrol et
                                        TryResetPanelBackground(songPath, songPanel, rtbDescription);
                                    }));

                                    Task.Run(() =>
                                    {
                                        try
                                        {
                                            newPlayer?.Dispose();
                                            newReader?.Dispose();
                                        }
                                        catch { }

                                        lock (additionalPlayers2)
                                        {
                                            additionalPlayers2.Remove(songPath);
                                            additionalReaders2.Remove(songPath);
                                        }
                                    });
                                };

                                newPlayer.Play();

                                btnPlayAdditional.BeginInvoke(new Action(() =>
                                {
                                    songPanel.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                                    rtbDescription.BackColor = ColorTranslator.FromHtml("#b2f9f7");
                                    btnPlayAdditional.ButtonBackColor = ColorTranslator.FromHtml("#ff4f6c");
                                    btnPlayAdditional.HoverColor = ColorTranslator.FromHtml("#cc3f56");
                                    btnPlayAdditional.Text = "Dur";
                                    btnPlayAdditional.Font = FontHelper.GetFont("Poppins", 10, FontStyle.Bold);
                                    btnPlayAdditional.Invalidate();
                                    UpdateAdditionalPlayerVolume2(songPath);
                                }));
                            }

                            if (needDispose && player != null && reader != null)
                            {
                                await Task.Delay(200);
                                try
                                {
                                    player.Dispose();
                                    reader.Dispose();
                                }
                                catch { }

                                lock (additionalPlayers2)
                                {
                                    additionalPlayers2.Remove(songPath);
                                    additionalReaders2.Remove(songPath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            btnPlayAdditional.BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show("Hata: " + ex.Message);
                            }));
                        }
                        finally
                        {
                            playerSemaphore.Release();
                            btnPlayAdditional.BeginInvoke(new Action(() =>
                            {
                                UpdateMainPlayerVolume2();
                                btnPlayAdditional.Enabled = true;
                            }));
                        }
                    });
                };


                //Çalışan eski buttonlarla.
                //btnPlayAdditional.Click += async (s, e) =>
                //{
                //    await playerSemaphore.WaitAsync();
                //    try
                //    {
                //        // Eğer bu şarkı için zaten bir oynatıcı varsa, durumunu kontrol edip toggle yap
                //        if (additionalPlayers.ContainsKey(songPath))
                //        {
                //            var player = additionalPlayers[songPath];
                //            if (player.playState == WMPLib.WMPPlayState.wmppsPlaying)
                //            {
                //                player.controls.stop();
                //                btnPlayAdditional.BackColor = Color.LightBlue;
                //            }
                //            else
                //            {
                //                player.controls.play();
                //                UpdateAdditionalPlayerVolume(songPath);
                //                btnPlayAdditional.BackColor = Color.Red;
                //            }
                //        }
                //        else
                //        {
                //            // Limit kontrolü kaldırıldı; her zaman yeni bir oynatıcı oluşturuyoruz
                //            var player = new WMPLib.WindowsMediaPlayer
                //            {
                //                URL = songPath,
                //                settings = { autoStart = false }
                //            };
                //            additionalPlayers[songPath] = player;
                //            player.controls.play();
                //            UpdateAdditionalPlayerVolume(songPath);
                //            btnPlayAdditional.BackColor = Color.Red;

                //            // Oynatma bittiğinde temizleme işlemi
                //            player.PlayStateChange += (int newState) =>
                //            {
                //                if ((WMPLib.WMPPlayState)newState == WMPLib.WMPPlayState.wmppsStopped)
                //                {
                //                    try { player.close(); } catch { }
                //                    additionalPlayers.Remove(songPath);
                //                    btnPlayAdditional.Invoke(new Action(() =>
                //                    {
                //                        btnPlayAdditional.BackColor = Color.LightBlue;
                //                    }));
                //                }
                //            };
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        MessageBox.Show("Hata: " + ex.Message);
                //    }
                //    finally
                //    {
                //        playerSemaphore.Release();
                //        UpdateMainPlayerVolume();
                //    }
                //};



                //btnPlayAdditional.Click += async (s, e) =>
                //{
                //    await playerSemaphore.WaitAsync();
                //    try
                //    {
                //        if (currentlyPlaying.Contains(songPath))
                //        {
                //            // Şarkı oynuyorsa; durdurma işlemi
                //            await Task.Run(() =>
                //            {
                //                try
                //                {
                //                    if (additionalPlayers.TryGetValue(songPath, out var player))
                //                    {
                //                        if (player.playState == WMPLib.WMPPlayState.wmppsPlaying)
                //                        {
                //                            player.controls.stop();
                //                            btnPlayAdditional.Invoke(new Action(() =>
                //                            {
                //                                btnPlayAdditional.BackColor = Color.LightBlue;
                //                            }));
                //                        }
                //                        else
                //                        {
                //                            // Tekrar çalma; ses ayarını hesapla
                //                            player.controls.play();
                //                            UpdateAdditionalPlayerVolume(songPath);
                //                            btnPlayAdditional.Invoke(new Action(() =>
                //                            {
                //                                btnPlayAdditional.BackColor = Color.Red;
                //                            }));
                //                        }
                //                    }
                //                }
                //                catch (Exception ex)
                //                {
                //                    Console.WriteLine("Hata: " + ex.Message);
                //                }
                //            });
                //        }
                //        else
                //        {
                //            if (currentlyPlaying.Count < MaxSongs)
                //            {
                //                await Task.Run(() =>
                //                {
                //                    try
                //                    {
                //                        var player = new WMPLib.WindowsMediaPlayer
                //                        {
                //                            URL = songPath,
                //                            settings = { autoStart = false }
                //                        };

                //                        player.controls.play();
                //                        additionalPlayers[songPath] = player;
                //                        currentlyPlaying.Add(songPath);

                //                        // Uygulanan şarkıya özel base sesi (varsayılan 1f ya da daha önce ayarlanmış değer) kullanılıyor:
                //                        UpdateAdditionalPlayerVolume(songPath);

                //                        btnPlayAdditional.Invoke(new Action(() =>
                //                        {
                //                            btnPlayAdditional.BackColor = Color.Red;
                //                        }));

                //                        // Oynatma bittiğinde temizlik ve UI güncelleme
                //                        player.PlayStateChange += (int newState) =>
                //                        {
                //                            if ((WMPLib.WMPPlayState)newState == WMPLib.WMPPlayState.wmppsStopped)
                //                            {
                //                                try
                //                                {
                //                                    player.close();
                //                                    additionalPlayers.Remove(songPath);
                //                                    currentlyPlaying.Remove(songPath);
                //                                    btnPlayAdditional.Invoke(new Action(() =>
                //                                    {
                //                                        btnPlayAdditional.BackColor = Color.LightBlue;
                //                                    }));
                //                                }
                //                                catch (Exception ex)
                //                                {
                //                                    Console.WriteLine("Hata (PlayStateChange): " + ex.Message);
                //                                }
                //                            }
                //                        };
                //                    }
                //                    catch (Exception ex)
                //                    {
                //                        Console.WriteLine("Genel Hata: " + ex.Message);
                //                    }
                //                });
                //            }
                //            else
                //            {
                //                MessageBox.Show("En fazla 2 ses çalabilirsiniz.");
                //                btnPlayAdditional.Invoke(new Action(() =>
                //                {
                //                    btnPlayAdditional.BackColor = Color.LightBlue;
                //                }));
                //            }
                //        }
                //    }
                //    finally
                //    {
                //        playerSemaphore.Release();
                //    }
                //};







                // ModernTrackBar ile değiştirildi!
                ModernTrackBar volumeSlider = new ModernTrackBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = songVolumes.ContainsKey(song.Path) ? (int)(songVolumes[song.Path] * 100) : 100,
                    Width = 20,//150
                    Height = 65, // YENİ: 60'dan 70'e çıkarıldı
                    //Location = new Point(580, 3),
                    Location = new Point(songPanel.Width - 152, 2),
                    Tag = song.Path,
                    Orientation = Orientation.Vertical,
                    CornerRadius = 2,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                    BorderThickness = 0,
                    TrackBackColor = Color.FromArgb(50, 56, 75),
                    BackgroundColor = Color.FromArgb(35, 41, 59),
                    BorderColor = Color.FromArgb(78, 87, 114),
                    TrackProgressColor = Color.FromArgb(255, 204, 123),
                    ThumbBorderColor = Color.White,

                    // Görsel ayarlarını da burada yapabilirsin:
                    // TrackBackColor = Color.FromArgb(120, 80, 200),
                    // TrackProgressColor = Color.FromArgb(180, 80, 220),
                    // ThumbColor = Color.White,
                    // BorderColor = Color.FromArgb(180, 80, 220),
                    // BorderThickness = 2,
                    // CornerRadius = 8
                };

                // Volume değişimini yakala
                volumeSlider.ValueChanged += (s, e) =>
                {
                    string songPath = (s as ModernTrackBar)?.Tag as string;
                    if (!string.IsNullOrEmpty(songPath))
                    {
                        float baseVolume = volumeSlider.Value / 100f;
                        songVolumes[songPath] = baseVolume;
                        SaveSongs();

                        // Anlık uygulama:
                        if (songPath == currentSongPath) // Ana player çalıyorsa
                        {
                            UpdateMainPlayerVolume();
                        }

                        // Eğer additional player çalıyorsa
                        if (additionalReaders2.ContainsKey(songPath))
                        {
                            UpdateAdditionalPlayerVolume2(songPath);
                        }
                    }
                };


                // TrackBar'ı oluşturuyoruz
                //TrackBar volumeSlider = new TrackBar
                //{
                //    Minimum = 0,
                //    Maximum = 100,
                //    Value = songVolumes.ContainsKey(song.Path) ? (int)(songVolumes[song.Path] * 100) : 50, // Varsayılan ses düzeyi %50
                //    Width = 150,
                //    Location = new Point(230, 15), // Örnek konum
                //    Tag = song.Path // Şarkı yolu ile ilişkilendiriyoruz
                //};

                // Yerel ses düzeyini ayarlamak için TrackBar scroll olayını kullanıyoruz
                //volumeSlider.Scroll += (s, e) =>
                //{
                //    string songPath = (s as TrackBar)?.Tag as string;
                //    if (!string.IsNullOrEmpty(songPath))
                //    {
                //        // Kullanıcının seçtiği base değeri (0-1) al
                //        float baseVolume = volumeSlider.Value / 100f;
                //        // Sözlüğe sadece base değeri kaydedilir
                //        if (songVolumes.ContainsKey(songPath))
                //            songVolumes[songPath] = baseVolume;
                //        else
                //            songVolumes.Add(songPath, baseVolume);

                //        SaveSongs();

                //        // Eğer ek oynatma yapılıyorsa, oynatıcıyı güncelle
                //        UpdateAdditionalPlayerVolume(songPath);
                //        // Eğer bu şarkı ana oynatıcıdaysa, ana oynatıcıyı güncelle
                //        if (songPath == currentSongPath)
                //            UpdateMainPlayerVolume();
                //    }
                //};


                bool active = false;
                // TrackBar'ın ValueChanged olayını yakalayın
                volumeSlider.ValueChanged += (s, e) =>
                {
                    if (!active)
                    {
                        Task.Run(async () =>
                        {
                            while (true)
                            {
                                // UI iş parçacığında işlem yapmak için Invoke kullanıyoruz
                                Invoke(new Action(() =>
                                {

                                    string songPath = (s as TrackBar)?.Tag as string; // TrackBar'a ilişkilendirilmiş şarkı yolunu al

                                    if (!string.IsNullOrEmpty(songPath))
                                    {
                                        // Yerel ses düzeyini al
                                        float localVolumeValue = volumeSlider.Value / 100f;

                                        // Şarkanın ses düzeyini kaydediyoruz
                                        songVolumes[songPath] = localVolumeValue;

                                        // Genel ses düzeyi ile orantılı olarak şarkanın ses düzeyini uygula
                                        SetSongVolume2(songPath, localVolumeValue);
                                    }


                                }));

                                // Her 100 ms'de bir kontrol yapalım
                                await Task.Delay(100);
                            }
                        });
                    }
                    active = true;




                };



                songPanel.Controls.Add(volumeSlider);
                songPanel.Controls.Add(btnPlayAdditional);





            }



        }


        private void UpdateSongListNumbers()
        {
            int number = 1;
            foreach (Control panel in flowLayoutPanelSongs.Controls)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control is CustomButton btn && btn.Size.Width == 42 && btn.Size.Height == 42)
                    {
                        btn.Text = number.ToString();
                        break; // Her panelde bir tane sıra numarası butonu olmalı
                    }
                }
                number++;
            }
        }

        private void UpdateArrowButtons()
        {
            for (int i = 0; i < flowLayoutPanelSongs.Controls.Count; i++)
            {
                Panel panel = flowLayoutPanelSongs.Controls[i] as Panel;
                if (panel != null)
                {
                    Button btnUp = panel.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "↑");
                    Button btnDown = panel.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "↓");

                    if (btnUp != null && btnDown != null)
                    {
                        if (i == 0)
                        {
                            btnUp.Enabled = false; // İlk eleman, yukarı ok devre dışı
                        }
                        else
                        {
                            btnUp.Enabled = true;
                        }

                        if (i == flowLayoutPanelSongs.Controls.Count - 1)
                        {
                            btnDown.Enabled = false; // Son eleman, aşağı ok devre dışı
                        }
                        else
                        {
                            btnDown.Enabled = true;
                        }
                    }
                }
            }
        }

        private void SaveOrderFromPanels()
        {
            var newList = new List<Song>();
            foreach (Control ctrl in flowLayoutPanelSongs.Controls)
            {
                if (ctrl is Panel panel)
                {
                    // Panel içindeki oynat butonunu bul (Tag içinde Song saklanıyor)
                    var playBtn = panel.Controls.OfType<CustomButton>().FirstOrDefault(b => b.Tag is Song);
                    if (playBtn != null && playBtn.Tag is Song s)
                    {
                        newList.Add(s);
                    }
                }
            }

            // Yeni sıralamayı ana listeye uygula ve kaydet
            songList = newList;
            try
            {
                SaveSongs();
            }
            catch
            {
                // Kaydetme başarısız olursa sessizce yoksay (isteğe bağlı log ekleyebilirsiniz)
            }
        }


        private void MovePanelUp(Panel panel)
        {
            int index = flowLayoutPanelSongs.Controls.GetChildIndex(panel);
            Console.WriteLine($"MovePanelUp: Panel index {index} -> {index - 1}");

            if (index > 0)
            {
                flowLayoutPanelSongs.Controls.SetChildIndex(panel, index - 1);
                UpdateArrowButtons();
                UpdateListNumbers();

                // Yeni sıra kalıcı olsun:
                SaveOrderFromPanels();

                Console.WriteLine("MovePanelUp sonrası numaralar:");
                for (int i = 0; i < flowLayoutPanelSongs.Controls.Count; i++)
                {
                    Panel p = flowLayoutPanelSongs.Controls[i] as Panel;
                    var btn = p?.Controls.Find("btnListNumber", false).FirstOrDefault() as CustomButton;
                    Console.WriteLine($"  Panel {i}: {btn?.Text ?? "NULL"}");
                }
            }
        }

        private void MovePanelDown(Panel panel)
        {
            int index = flowLayoutPanelSongs.Controls.GetChildIndex(panel);
            if (index < flowLayoutPanelSongs.Controls.Count - 1)
            {
                flowLayoutPanelSongs.Controls.SetChildIndex(panel, index + 1);
                UpdateArrowButtons();
                UpdateListNumbers();

                // Yeni sıra kalıcı olsun:
                SaveOrderFromPanels();
            }
        }

        // Bu metodu da ekle:
        private void UpdateListNumbers()
        {
            for (int i = 0; i < flowLayoutPanelSongs.Controls.Count; i++)
            {
                Panel panel = flowLayoutPanelSongs.Controls[i] as Panel;
                if (panel != null)
                {
                    var btnListNumber = panel.Controls.Find("btnListNumber", false).FirstOrDefault() as CustomButton;
                    if (btnListNumber != null)
                    {
                        btnListNumber.Text = (i + 1).ToString();

                        // BU SATIRLARI EKLEYİN:
                        btnListNumber.Invalidate(); // Butonu yeniden çiz
                        btnListNumber.Refresh();    // Hemen güncelle
                        panel.Invalidate();         // Panel'i de yenile
                    }
                }
            }
        }


        private void SetSongVolume(string songPath, float volume)
        {
            // Şarkı için ses düzeyini kaydet
            if (songVolumes.ContainsKey(songPath))
            {
                songVolumes[songPath] = volume;
            }
            else
            {
                songVolumes.Add(songPath, volume);
            }

            SaveSongs();

            // Şarkının sesini güncelle
            if (waveOutEvent != null)
            {
                // Şarkı için özel ses düzeyi
                float globalVolume = trackSVolume.Value / 100f; // Genel ses düzeyini al
                float finalVolume = globalVolume * volume; // Genel ve özel ses düzeyini çarp
                waveOutEvent.Volume = finalVolume;
            }
        }

        private async void SetSongVolume2(string songPath, float localVolume)
        {
            // Global ses düzeyini al (örneğin trackSVolume gibi bir kontrol olabilir)
            float globalVolume = trackSVolume.Value / 100f; // Global ses düzeyini al

            // Genel ve özel ses düzeyini çarpıyoruz
            float adjustedVolume = localVolume * globalVolume;

            // Şarkı için ses düzeyini kaydet
            if (songVolumes.ContainsKey(songPath))
            {
                songVolumes[songPath] = adjustedVolume;
            }
            else
            {
                songVolumes.Add(songPath, adjustedVolume);
            }

            SaveSongs();

            // Oynatıcıdaki ses düzeyini güncelliyoruz
            if (additionalPlayers.ContainsKey(songPath))
            {
                var player = additionalPlayers[songPath];

                // 0-100 aralığında ses ayarını, yeniden deneme mekanizmasıyla ayarla
                int targetVolume = (int)(adjustedVolume * 100);
                int attempts = 0;
                const int maxAttempts = 5;
                bool success = false;

                while (!success && attempts < maxAttempts)
                {
                    try
                    {
                        player.settings.volume = targetVolume;
                        success = true;
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        // RPC_E_CALL_REJECTED hatası (0x80010001) alınırsa, kısa süre bekle ve tekrar dene
                        if (ex.ErrorCode == unchecked((int)0x80010001))
                        {
                            attempts++;
                            await Task.Delay(100);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private void RefreshSongList()
        {
            // Scroll pozisyonunu al
            int scrollPos = flowLayoutPanelSongs.VerticalScroll.Value;

            flowLayoutPanelSongs.Controls.Clear();

            foreach (var song in songs)
            {
                CreateSongUI(song);
            }

            // Scroll pozisyonunu geri yükle
            ScrollHelper.SetVerticalScroll(flowLayoutPanelSongs, scrollPos);
        }
        private void UpdateSongList()
        {
            // 1. Scroll pozisyonunu al
            int scrollPos = flowLayoutPanelSongs.VerticalScroll.Value;

            // 2. Listeyi güncelle
            RefreshSongList();
            songList.Clear();
            foreach (Panel panel in flowLayoutPanelSongs.Controls)
            {
                Button playButton = panel.Controls.OfType<Button>().FirstOrDefault(b => b.Text != "Sil");
                if (playButton != null)
                {
                    Song song = playButton.Tag as Song;
                    if (song != null)
                    {
                        songList.Add(song);
                    }
                }
            }
            SaveSongs();

            // 3. Scroll pozisyonunu geri ver
            ScrollHelper.SetVerticalScroll(flowLayoutPanelSongs, scrollPos);
        }


        private void LoadSongs()
        {
            if (File.Exists(songsFilePath))
            {
                string json = File.ReadAllText(songsFilePath); // Read the JSON file
                songList = JsonConvert.DeserializeObject<List<Song>>(json) ?? new List<Song>(); // Deserialize into the song list

                // Şarkıların ses seviyelerini yükle
                foreach (var song in songList)
                {
                    songVolumes[song.Path] = song.Volume; // Ses seviyesini kaydet
                }
            }

            // After loading the songs, create the UI elements for each song
            foreach (var song in songList)
            {
                CreateSongUI(song);
            }
            //UpdateSongListNumbers();
            UpdateListNumbers();
        }


        private void SaveSongs()
        {
            try
            {
                // Her şarkının güncel ses seviyesini güncelle
                foreach (var song in songList)
                {
                    if (songVolumes.ContainsKey(song.Path))
                    {
                        song.Volume = songVolumes[song.Path]; // Şarkının güncel ses seviyesini ata
                    }
                }

                string json = JsonConvert.SerializeObject(songList, Formatting.Indented); // Serialize the entire song list
                File.WriteAllText(songsFilePath, json); // Save to a file
            }
            catch
            {
                //Error
            }

        }




        private void PlayVideo(string relativePath, int scrollValue)
        {
            string path = Path.Combine(localDirectory, relativePath);
            string videoPath = path;

            var selectedScreen = ShowScreenSelectionDialog();
            if (selectedScreen != null)
            {
                OpenVideoPlayerOnScreen(selectedScreen, videoPath, scrollValue);
            }
        }



        //private async void PlaySongw(string relativePath)
        //{
        //    string path = Path.Combine(songsFolder, relativePath);  // Yerel dizin yolunu tamamla

        //    if (File.Exists(path))
        //    {
        //        // Eğer önceden bir İçerik çalınıyorsa, durdur ve temizle
        //        if (waveOutEvent != null)
        //        {
        //            waveOutEvent.Stop();
        //            waveOutEvent.Dispose();
        //        }
        //        if (audioFileReader != null)
        //        {
        //            audioFileReader.Dispose();
        //            audioFileReader = null; // Referansı temizle
        //        }

        //        await Task.Delay(100);

        //        // Yeni İçerikyı yükle
        //        //audioFileReader = new AudioFileReader(path); // ----------------------!!!!!!!!!!
        //        waveOutEvent = new WaveOutEvent();
        //        waveOutEvent.Init(audioFileReader);
        //        waveOutEvent.PlaybackStopped += (s, e) => OnPlaybackStopped();

        //        // Döngü açık ise, en son konumdan devam et
        //        if (isLooping)
        //        {
        //            audioFileReader.Position = lastPosition; // Son konumu uygula
        //        }

        //        // Ses çalmaya başla
        //        waveOutEvent.Play();

        //        // Zamanlayıcıyı başlat (İçerik çalmaya başlamadan önce)
        //        playbackTimer.Start();

        //        // İçeriknın süresini etiketle göster
        //        lblTotalTime.Text = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds).ToString(@"mm\:ss");
        //        trackBarProgress.Maximum = 100;
        //        trackBarProgress.Value = 0;

        //        // Timer başlangıcında ilk konumdan itibaren zamanlamayı başlat
        //        lblCurrentTime.Text = "00:00";
        //    }
        //    else
        //    {
        //        MessageBox.Show("İçerik dosyası bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}




        private async void PlaySong(string relativePath, Panel songPanel, RichTextBox rtbDescription)
        {
            // Önceki ana panel / şarkı bilgisini yakala (gerekirse UI güncellemesi için)
            Panel previousPanel = lastMainPlayedPanel;
            string previousSongPath = null;
            if (previousPanel != null)
            {
                foreach (Control c in previousPanel.Controls)
                {
                    if (c is CustomButton btn && btn.Tag is Song s)
                    {
                        previousSongPath = s.Path;
                        break;
                    }
                }
            }

            // lastMainPlayedPanel'i hemen güncelle (yeni panel aktif olacak)
            lastMainPlayedPanel = songPanel;

            // full path ve kontrol
            string fullPath = Path.Combine(Application.StartupPath, relativePath);

            if (File.Exists(fullPath))
            {
                // Önceki ana oynatıcıları durdur / temizle (önceki şarkının loop ile yeniden başlamasını engellemek için)
                if (waveOutEvent != null)
                {
                    try
                    {
                        waveOutEvent.Stop();
                    }
                    catch { }
                    try
                    {
                        waveOutEvent.Dispose();
                    }
                    catch { }
                    waveOutEvent = null;
                }
                if (audioFileReader != null)
                {
                    try
                    {
                        audioFileReader.Dispose();
                    }
                    catch { }
                    audioFileReader = null;
                }

                // Önceki panelin arkaplanı gerekiyorsa burada resetle (additional çalma kontrolü TryResetPanelBackground içinde)
                if (previousPanel != null && previousPanel != songPanel && !string.IsNullOrEmpty(previousSongPath))
                {
                    // previousPanel içindeki RichTextBox'ı bul ve onu TryResetPanelBackground'a ver
                    var prevRtb = previousPanel.Controls.OfType<RichTextBox>().FirstOrDefault();
                    TryResetPanelBackground(previousSongPath, previousPanel, prevRtb);
                }


                // Yeni currentSongPath'i ayarla
                currentSongPath = relativePath;

                await Task.Delay(100); // Küçük gecikme

                try
                {
                    // MediaFoundationReader ile dosyayı yükle (tam yol kullanılır)
                    audioFileReader = new MediaFoundationReader(fullPath);

                    // Volume kontrolü için sarmalayıcı ekle
                    var volumeProvider = new VolumeSampleProvider(audioFileReader.ToSampleProvider());

                    waveOutEvent = new WaveOutEvent();
                    waveOutEvent.Init(volumeProvider);

                    // İlk ses değerini ayarla
                    float songVolume = songVolumes.ContainsKey(currentSongPath) ? songVolumes[currentSongPath] : 1f;
                    float globalVolume = trackSVolume.Value / 100f;
                    volumeProvider.Volume = songVolume * globalVolume;

                    // volumeProvider'ı saklayabilmek için bir alan ekle
                    mainVolumeProvider = volumeProvider;

                    // Şarkının ses ayarını, global ve şarkıya özel (base) değerin çarpımı şeklinde uygula
                    UpdateMainPlayerVolume();

                    waveOutEvent.Play();

                    // Zamanlayıcı ve etiket güncellemeleri
                    playbackTimer.Start();
                    lblTotalTime.Text = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds).ToString(@"mm\:ss");
                    trackBarProgress.Maximum = (int)Math.Round(audioFileReader.TotalTime.TotalSeconds);
                    trackBarProgress.Value = 0;
                    lblCurrentTime.Text = "00:00";

                    // Sadece panel parametresi ile çağırıyoruz:
                    waveOutEvent.PlaybackStopped += (s, e) => OnPlaybackStopped(songPanel, rtbDescription);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Çalma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Dosya bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //private async void PlaySong(string relativePath)
        //{
        //    // Göreceli yolu tam yola dönüştür
        //    string path = Path.Combine(Application.StartupPath, relativePath);

        //    // Şarkı yolunu güncelle
        //    currentSongPath = path;

        //    if (File.Exists(path))
        //    {
        //        // Önceki çalma işlemlerini durdur ve temizle
        //        if (waveOutEvent != null)
        //        {
        //            waveOutEvent.Stop();
        //            waveOutEvent.Dispose();
        //            waveOutEvent = null;
        //        }

        //        if (audioFileReader != null)
        //        {
        //            audioFileReader.Dispose();
        //            audioFileReader = null;
        //        }

        //        await Task.Delay(100); // Küçük bir gecikme ekle

        //        try
        //        {
        //            // MediaFoundationReader ile dosyayı yükle
        //            audioFileReader = new MediaFoundationReader(path);
        //            waveOutEvent = new WaveOutEvent();
        //            waveOutEvent.Init(audioFileReader);
        //            waveOutEvent.Play();
        //            waveOutEvent.Volume = SVolume;

        //            // Zamanlayıcıyı başlat (İçerik çalmaya başlamadan önce)
        //            playbackTimer.Start();

        //            // Toplam süreyi ve diğer bilgileri ayarla
        //            lblTotalTime.Text = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds).ToString(@"mm\:ss");
        //            trackBarProgress.Maximum = 100;
        //            trackBarProgress.Value = 0;
        //            lblCurrentTime.Text = "00:00";

        //            // Oynatma durumu için olay işleyici ekle
        //            waveOutEvent.PlaybackStopped += (s, e) => OnPlaybackStopped();

        //            waveOutEvent.Volume = SVolume;
        //            // Şarkının ses düzeyini ayarlama
        //            SetSongVolume(currentSongPath, songVolumes.ContainsKey(currentSongPath) ? songVolumes[currentSongPath] : 1f);





        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Hata: {ex.Message}", "Çalma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Dosya bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}



        private void TryResetPanelBackground(string songPath, Panel panel, RichTextBox? rtbDescription)
        {
            bool isMainPlaying = currentSongPath == songPath && waveOutEvent != null && waveOutEvent.PlaybackState == PlaybackState.Playing;
            bool isAdditionalPlaying = false;
            lock (additionalPlayers2)
            {
                if (additionalPlayers2.TryGetValue(songPath, out var player))
                {
                    isAdditionalPlaying = player.PlaybackState == PlaybackState.Playing;
                }
            }
            if (!isMainPlaying && !isAdditionalPlaying)
            {
                panel.BackColor = ColorTranslator.FromHtml("#ffffff");
                // rtbDescription null olabilir; varsa sıfırla
                if (rtbDescription != null)
                    rtbDescription.BackColor = ColorTranslator.FromHtml("#ffffff");
                //MessageBox.Show("İçerik bitti, arka plan sıfırlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }




        private void OnPlaybackStopped(Panel songPanel, RichTextBox rtbDescription)
        {
            if (isLooping)
            {
                try
                {
                    if (audioFileReader == null || waveOutEvent == null || mainVolumeProvider == null)
                        return;

                    // Baştan başlat
                    audioFileReader.Position = 0;

                    // Yeniden başlat (VolumeSampleProvider ile)
                    waveOutEvent.Stop();
                    waveOutEvent.Init(mainVolumeProvider);

                    // Ses düzeyini tekrar uygula
                    UpdateMainPlayerVolume();

                    waveOutEvent.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                playbackTimer.Stop();
                // Paneldeki şarkı yolunu bul
                string songPath = null;
                foreach (Control c in songPanel.Controls)
                {
                    if (c is CustomButton btn && btn.Tag is Song s)
                    {
                        songPath = s.Path;
                        break;
                    }
                }
                if (songPath != null)
                    TryResetPanelBackground(songPath, songPanel, rtbDescription);


                //playbackTimer.Stop();
                //songPanel.BackColor = ColorTranslator.FromHtml("#ffffff");
            }
        }







        private void RemoveSong(Song song, Panel panel)
        {
            try
            {
                if (File.Exists(song.Path))
                {
                    File.Delete(song.Path);
                }

                flowLayoutPanelSongs.Controls.Remove(panel);
                songList.Remove(song);
                SaveSongs();
            }
            catch (Exception ex)
            {
                if (waveOutEvent != null)
                {
                    waveOutEvent.Stop();
                    waveOutEvent.Dispose();
                    MessageBox.Show("Silerken bir hata oluştu yazılımı yeniden başlatıp tekrar deneyin hata kodu: " + ex.Message);
                }
                else
                {
                    MessageBox.Show("Silerken bir hata oluştu: " + ex.Message);
                }
            }
            //UpdateSongListNumbers();
            UpdateListNumbers();
        }


        private void btnAddNewSong_Click(object sender, EventArgs e)
        {
            try
            {
                // Tüm desteklenen uzantıları tek bir "All Supported" filtresinde toplayalım
                string supported = "*.mp3;*.wav;*.flac;*.m4a;*.mp4;*.mov;*.avi;*.flv;*.mkv;*.wmv;*.m4v;*.webm;*.ogg;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.ppt;*.pptx";

                using (OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Filter = $"All Supported|{supported}|Audio Files|*.mp3;*.wav;*.flac;*.m4a|Video Files|*.mp4;*.mov;*.avi;*.flv;*.mkv;*.wmv;*.m4v;*.webm;*.ogg|Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Presentation Files|*.pptx;*.ppt|All Files|*.*",
                    Title = "İçerik Seçin"
                })
                {
                    // Varsayılan olarak "All Supported" seçili olsun
                    openFileDialog.FilterIndex = 1;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        string fileExtension = Path.GetExtension(filePath).ToLower();

                        // Dosya adını ve Data dizinini ayarla
                        string fileName = Path.GetFileName(filePath);
                        string dataDirectory = Path.Combine(Application.StartupPath, "Data");

                        if (!Directory.Exists(dataDirectory))
                        {
                            Directory.CreateDirectory(dataDirectory);
                        }

                        // Data dizininde hedef dosya yolunu oluştur
                        string destinationFilePath = Path.Combine(dataDirectory, fileName);
                        int count = 1;
                        while (File.Exists(destinationFilePath))
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            destinationFilePath = Path.Combine(dataDirectory, $"{fileNameWithoutExtension} ({count}){extension}");
                            count++;
                        }

                        // Dosyayı Data dizinine kopyala
                        File.Copy(filePath, destinationFilePath);

                        // Hedef yolu relative (göreceli) hale getir
                        string relativePath = Path.GetRelativePath(Application.StartupPath, destinationFilePath);

                        // İçerik adı ve açıklaması için kullanıcıdan bilgi al (varsayılan olarak dosya adını kullan)
                        string songName = Prompt.ShowDialog("İçerik adı girin:", "İçerik Adı");
                        if (string.IsNullOrWhiteSpace(songName))
                            songName = Path.GetFileNameWithoutExtension(fileName);

                        string description = Prompt.ShowDialog("İçerik açıklaması girin:", "İçerik Açıklaması");
                        if (string.IsNullOrWhiteSpace(description))
                            description = "";

                        // Dosya türünü belirle
                        bool isVideo = fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi" || fileExtension == ".flv" || fileExtension == ".mkv" || fileExtension == ".wmv" || fileExtension == ".m4v" || fileExtension == ".webm";
                        bool isImage = fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" ||
                                       fileExtension == ".bmp" || fileExtension == ".gif";
                        bool isPresentation = fileExtension == ".ppt" || fileExtension == ".pptx";

                        // Yeni Song nesnesini oluştur
                        Song song = new Song
                        {
                            Name = songName,
                            Path = relativePath,
                            Description = description,
                            IsVideo = isVideo,
                            IsImage = isImage,
                            IsPresentation = isPresentation
                        };

                        songList.Add(song);
                        SaveSongs();
                        CreateSongUI(song); // Yeni içeriğin arayüzünü oluştur
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the song: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateListNumbers();
        }






        private void ConvertMp3ToWav(string mp3Path, string wavPath)
        {
            using var reader = new NAudio.Wave.MediaFoundationReader(mp3Path);
            using var resampler = new NAudio.Wave.MediaFoundationResampler(reader, NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100, reader.WaveFormat.Channels))
            {
                ResamplerQuality = 60
            };

            // WAV dosyasına yazmak için WaveFileWriter kullanıyoruz
            using var writer = new WaveFileWriter(wavPath, resampler.WaveFormat);

            // Resampler'dan okuyarak WAV dosyasına yaz
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
            }
        }

        // İleri ve geri sarma çubuğu
        private void trackBarProgress_Scroll(object sender, EventArgs e)
        {
            //if (audioFileReader != null)
            //{
            //    double newPosition = (trackBarProgress.Value / 100.0) * audioFileReader.TotalTime.TotalSeconds;
            //    audioFileReader.Position = (long)(newPosition * audioFileReader.WaveFormat.AverageBytesPerSecond);
            //}
        }

        // Döngü modu
        private void btnLoop_Click(object sender, EventArgs e)
        {
            isLooping = !isLooping;
            //btnLoop.Text = isLooping ? "Döngü: Açık" : "Döngü: Kapalı";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (waveOutEvent != null)
            {
                if (isLooping == true)
                {
                    //isLooping = !isLooping;
                    //customSwitch1.Checked = !customSwitch1.Checked;
                    customSwitch1.Checked = false;
                }
                waveOutEvent.Stop();
                waveOutEvent.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Formun sağ tarafını 5 piksel artırmak için
            this.Width += 20;
            this.BackColor = ColorTranslator.FromHtml("#1e2333");
            flowLayoutPanelSongs.BackColor = ColorTranslator.FromHtml("#272d42");

            //this.BackColor = ColorTranslator.FromHtml("#212121");
            //flowLayoutPanelSongs.BackColor = ColorTranslator.FromHtml("#303030");

            // Form1.cs (constructor veya Load da olabilir)
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F2)
                {
                    var sysCheckForm = Application.OpenForms.OfType<SystemCheck>().FirstOrDefault();
                    if (sysCheckForm == null)
                        sysCheckForm = new SystemCheck();
                    sysCheckForm.ShowManually();
                }
            };
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (waveOutEvent != null)
            {
                if (waveOutEvent.PlaybackState == PlaybackState.Playing)
                {
                    waveOutEvent.Pause();
                }
                else if (waveOutEvent.PlaybackState == PlaybackState.Paused)
                {
                    waveOutEvent.Play();
                }
            }

        }

        // Mevcut flowLayoutPanelSongs_Resize metodunun içeriğini aşağıdaki ile değiştirin
        private void flowLayoutPanelSongs_Resize(object sender, EventArgs e)
        {
            // Eğer kullanıcı interaktif yeniden boyutlandırma yapıyorsa işi ertele (debounce)
            if (isUserResizing)
            {
                flowLayoutPanelSongs.Visible = false;
                // Timer'ı resetle
                resizeEndTimer.Stop();
                resizeEndTimer.Start();
                return;
            }

            // Normal durumda hemen güncelle
            UpdatePanelsWidths();
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



        /// DİĞER /////// ---------------------------------------------------->
        /// DİĞER /////// ---------------------------------------------------->


        private void btnVideo_Click(object sender, EventArgs e)
        {
            try
            {
                // Kullanıcıdan video dosyası seçmesini iste
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov|All Files|*.*";  // Video dosya formatları
                    openFileDialog.Title = "Video Seçin";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string videoPath = openFileDialog.FileName;

                        var selectedScreen = ShowScreenSelectionDialog();
                        if (selectedScreen != null)
                        {
                            int scrollValue = flowLayoutPanelSongs.VerticalScroll.Value; // Scroll değerini al
                            OpenVideoPlayerOnScreen(selectedScreen, videoPath, scrollValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata meydana geldi: " + ex.Message);
            }
        }

        private void OpenVideoPlayerOnScreen(Screen selectedScreen, string videoPath, int scrollValue)
        {
            var videoPlayer = new VideoPlayerCY(videoPath)
            {
                StartPosition = FormStartPosition.Manual,
                Bounds = selectedScreen.Bounds,
                WindowState = FormWindowState.Maximized
            };

            // Form kapatıldığında çalışacak işlemi burada tanımla
            videoPlayer.FormClosed += async (s, e) =>
            {
                // Video oynatıcı kapandıktan sonra çalışacak kod
                await Task.Delay(100); // Küçük bir gecikme ekle
                flowLayoutPanelSongs.VerticalScroll.Value = Math.Min(scrollValue, flowLayoutPanelSongs.VerticalScroll.Maximum);
                //MessageBox.Show("Video oynatıcı kapandı. " + scrollValue, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            videoPlayer.Show(); // ShowDialog DEĞİL! Show() non-blocking, hemen devam eder
                                // Buradan sonra kodun çalışmaya devam eder
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
        //------------------SS--BB------<<--



        private void btnBackGround_Click(object sender, EventArgs e)
        {
            BackGroundSystem bgs = new BackGroundSystem();
            bgs.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WaitSongSystem wss = new WaitSongSystem();
            wss.Show();
        }

        private void trackSVolume_Scroll(object sender, EventArgs e)
        {
            // Global değeri güncelle
            float globalVolume = GetGlobalVolume();

            // Ana oynatıcı güncellemesi (eğer varsa)
            UpdateMainPlayerVolume();

            // Ek oynatma yapan tüm oynatıcıları güncelle

            foreach (var songPath in additionalReaders2.Keys)
            {
                UpdateAdditionalPlayerVolume2(songPath);
            }


            label1.Text = $"🔊 Ses Düzeyi: %{trackSVolume.Value}";
        }



        private void button6_Click(object sender, EventArgs e)
        {
            PowerPointViewer ppv = new PowerPointViewer();
            ppv.Show();
        }


        ///--------------------------------------------------------->
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                // Kullanıcıdan program (.exe) dosyası seçmesini iste
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Uygulama Dosyaları (*.exe)|*.exe|Tüm Dosyalar (*.*)|*.*";
                    openFileDialog.Title = "Program Seçin";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string programPath = openFileDialog.FileName;

                        // Ekran seçimi için formu oluştur
                        Screen[] screens = Screen.AllScreens;

                        Form screenSelectionForm = new Form
                        {
                            Text = "Ekran Seçin",
                            Width = 400,
                            Height = 300,
                            StartPosition = FormStartPosition.CenterScreen
                        };

                        // Ekranları temsil etmek için dinamik butonlar oluştur
                        for (int i = 0; i < screens.Length; i++)
                        {
                            Button screenButton = new Button
                            {
                                Width = 300,
                                Height = 50,
                                Top = 60 * i + 10, // Butonların dikey olarak yerleşimi
                                Left = 10,
                                Tag = screens[i]  // Butona ekran bilgisi ekle
                            };

                            string displayInfo = GetDisplayInfo2(screens[i]);

                            screenButton.Text = displayInfo;

                            // Butona tıklama olayı ekle
                            screenButton.Click += (s, args) =>
                            {
                                Screen selectedScreen = (Screen)((Button)s).Tag; // Tıklanan butondan ekranı al
                                OpenProgramOnScreen2(selectedScreen, programPath);  // Seçilen ekranda program çalıştır
                                screenSelectionForm.Close(); // Ekran seçim formunu kapat
                            };

                            // Butonu form üzerine ekle
                            screenSelectionForm.Controls.Add(screenButton);
                        }

                        // Ekran seçim formunu aç
                        screenSelectionForm.ShowDialog(); // ShowDialog() ile modal olarak aç
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata meydana geldi: " + ex.Message);
            }
        }

        private void OpenProgramOnScreen2(Screen selectedScreen, string programPath)
        {
            try
            {
                // Kullanıcıya tam ekran mı yoksa pencere modunda mı açılacağını sor
                DialogResult result = MessageBox.Show(
                    "Program tam ekran mı açılsın?",
                    "Ekran Modu Seçimi",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                {
                    MessageBox.Show("İşlem iptal edildi.");
                    return;
                }

                bool isFullScreen = result == DialogResult.Yes;

                // Process başlatma için ayarları yapılandır
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = programPath,
                    UseShellExecute = true
                };

                // Process'i başlat
                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    // Ana pencerenin yüklenmesini beklemek için bir döngü
                    for (int i = 0; i < 10; i++) // Maksimum 10 deneme
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                            break;

                        Thread.Sleep(500); // 500ms bekle
                    }

                    // Pencere tanımlandıysa, taşı
                    IntPtr handle = process.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        Rectangle screenBounds = selectedScreen.Bounds;

                        if (isFullScreen)
                        {
                            // Tam ekran aç
                            SetWindowPos(handle, IntPtr.Zero, screenBounds.X, screenBounds.Y,
                                         screenBounds.Width, screenBounds.Height,
                                         SWP_NOZORDER | SWP_SHOWWINDOW);
                        }
                        else
                        {
                            // Pencereyi ekranın ortasına yerleştir ve boyutlandır
                            int windowWidth = screenBounds.Width / 2;
                            int windowHeight = screenBounds.Height / 2;
                            int windowX = screenBounds.X + (screenBounds.Width - windowWidth) / 2;
                            int windowY = screenBounds.Y + (screenBounds.Height - windowHeight) / 2;

                            SetWindowPos(handle, IntPtr.Zero, windowX, windowY,
                                         windowWidth, windowHeight,
                                         SWP_NOZORDER | SWP_SHOWWINDOW);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Pencere taşıma başarısız. Ana pencere tanımlanamadı.");
                    }
                }
                else
                {
                    MessageBox.Show("Program başlatılamadı.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Program çalıştırılırken bir hata oluştu: " + ex.Message);
            }
        }

        // Yardımcı Metot: Pencereyi Seçilen Ekrana Taşıma
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;


        // Ekran bilgilerini almak için kullanılan yöntemler (GetDisplayInfo2 ve GetMonitorModelInfo2)
        // Bu bölümler aynen korunabilir.



        private string GetDisplayInfo2(Screen screen)
        {
            // Varsayılan ekran bilgisi
            string displayInfo = screen.DeviceName;

            // Varsayılan ekranın tespiti
            if (screen.Equals(Screen.PrimaryScreen))
            {
                displayInfo += " (Varsayılan Ekran)";
            }

            // Ekran markasını ve modelini almak için WMI sorgusu
            string monitorInfo = GetMonitorModelInfo2();
            if (!string.IsNullOrEmpty(monitorInfo))
            {
                displayInfo += " - " + monitorInfo;
            }

            return displayInfo;
        }

        private string GetMonitorModelInfo2()
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





























        // Global ses değerini (0-1) trackSVolume kontrolünden alır.
        private float GetGlobalVolume()
        {
            return trackSVolume.Value / 100f;
        }

        // Şarkıya özel (base) ses değerini döndürür, eğer ayarlanmamışsa varsayılan 1f kullanır.
        private float GetSongBaseVolume(string songPath)
        {
            return songVolumes.TryGetValue(songPath, out float vol) ? vol : 1f;
        }

        // Ana oynatıcı için ses güncelleme: final ses = global * şarkı base değeri.
        private void UpdateMainPlayerVolume()
        {
            if (mainVolumeProvider != null)
            {
                float songVolume = songVolumes.ContainsKey(currentSongPath) ? songVolumes[currentSongPath] : 1f;
                float globalVolume = trackSVolume.Value / 100f;
                mainVolumeProvider.Volume = songVolume * globalVolume;
            }
        }






        // Ek oynatma için (WindowsMediaPlayer) ses güncelleme.
        //private void UpdateAdditionalPlayerVolume(string songPath)
        //{
        //    if (additionalPlayers.ContainsKey(songPath))
        //    {
        //        var player = additionalPlayers[songPath];
        //        float finalVolume = GetGlobalVolume() * GetSongBaseVolume(songPath);
        //        player.settings.volume = (int)(finalVolume * 100);
        //    }
        //}

        private void UpdateAdditionalPlayerVolume(string songPath)
        {
            if (additionalPlayers.ContainsKey(songPath))
            {
                var player = additionalPlayers[songPath];
                // Global ses değeri (0-1 aralığında, trackSVolume kontrolünden)
                float globalVolume = trackSVolume.Value / 100f;
                // Şarkıya özel ses değeri, eğer ayarlanmadıysa varsayılan 1.0f
                float songVolume = songVolumes.ContainsKey(songPath) ? songVolumes[songPath] : 1f;
                // İki değerin çarpımı final ses seviyesini verir
                float finalVolume = globalVolume * songVolume;
                // player.settings.volume 0-100 aralığında olduğu için çeviriyoruz.
                player.settings.volume = (int)(finalVolume * 100);
            }
        }


        private void UpdateMainPlayerVolume2()
        {
            if (mainWaveOut2 != null && mainAudioReader2 != null && !string.IsNullOrEmpty(currentSongPath))
            {
                float finalVolume = GetGlobalVolume() * GetSongBaseVolume(currentSongPath);
                mainAudioReader2.Volume = finalVolume; // 0.0f - 1.0f aralığında
            }
        }

        private void UpdateAdditionalPlayerVolume2(string songPath)
        {
            lock (additionalReaders2)
            {
                if (additionalReaders2.TryGetValue(songPath, out var reader))
                {
                    // Şarkıya özel baz ses
                    float songVolume = songVolumes.ContainsKey(songPath) ? songVolumes[songPath] : 1f;
                    // Global ses
                    float globalVolume = trackSVolume.Value / 100f;
                    // Reader volume ayarı (her şarkı bağımsız)
                    reader.Volume = songVolume * globalVolume;
                }
            }
        }






        private void button8_Click(object sender, EventArgs e)
        {
            WebControlForm wcf = new WebControlForm();
            wcf.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            VisualizerFormV2 vf = new VisualizerFormV2();
            vf.Show();
        }

        private void customSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            isLooping = !isLooping;
        }

        private void MediaEditMenu_Click(object sender, EventArgs e)
        {
            MediaEditMenu mem = new MediaEditMenu();
            mem.ShowDialog();
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            MoreSection ms = new MoreSection();
            ms.ShowDialog();
        }

        private void customButton1_Click(object sender, EventArgs e)
        {
            string url = "https://cayadev.com";

            try
            {
                // Varsayılan tarayıcıda bağlantıyı aç
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Modern işletim sistemlerinde bu gerekli
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantıyı açarken bir hata oluştu: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            Environment.Exit(0);
        }
    }

    public class Song
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public bool IsVideo { get; set; } // New property to indicate if it's a video
        public bool IsImage { get; set; } // Resim dosyalarını belirtir.
        public bool IsPresentation { get; set; } // Sunum dosyaları için
        public float Volume { get; set; } = 1.0f; // Varsayılan ses düzeyi 1.0 (maksimum)
    }


    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form
            {
                Width = 400,
                Height = 200,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent
            };

            Label label = new Label { Text = text, Dock = DockStyle.Top };
            TextBox inputBox = new TextBox { Dock = DockStyle.Top, Margin = new Padding(10), Text = defaultValue };
            Label previewLabel = new Label { Text = "Girdiğiniz metin: " + defaultValue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(10) };
            Button confirmation = new Button { Text = "Tamam", Dock = DockStyle.Bottom };

            string result = null;

            // Kullanıcının girdiği metni anında göster
            inputBox.TextChanged += (sender, e) =>
            {
                previewLabel.Text = "Girdiğiniz metin: " + inputBox.Text;
            };

            confirmation.Click += (sender, e) =>
            {
                result = inputBox.Text;
                prompt.Close();
            };

            prompt.Controls.Add(label);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(previewLabel);
            prompt.Controls.Add(confirmation);
            prompt.ShowDialog();

            return result;
        }
    }


    public static class ControlExtensions
    {
        public static Task InvokeAsync(this Control control, Action action)
        {
            if (control.InvokeRequired)
                return Task.Factory.StartNew(() => control.Invoke(action));
            else
            {
                action();
                return Task.CompletedTask;
            }
        }
    }






}
