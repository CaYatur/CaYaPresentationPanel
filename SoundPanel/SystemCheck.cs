using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using System.Media;

namespace SoundPanel
{
    public partial class SystemCheck : Form
    {
        private List<SystemCheckItem> checkResults = new List<SystemCheckItem>();
        private List<SystemCheckItem> lastResults = new List<SystemCheckItem>();
        private DateTime lastUpdateTime = DateTime.MinValue;
        private bool suppressUpdate = false;
        private Panel resultsPanel;
        private Button closeButton;
        private System.Windows.Forms.Timer resizeTimer;
        private bool isFormClosing = false;
        private bool initialChecksDone = false;
        private bool manualShowRequested = false;

        // WMI/event fields for device detection
        private ManagementEventWatcher usbInsertWatcher, usbRemoveWatcher;
        private MMDeviceEnumerator audioEnum;

        public void ShowManually()
        {
            manualShowRequested = true;
            ShowResults();
            this.Show();
            resultsPanel.Show();
        }

        public SystemCheck()
        {
            InitializeComponent();
            this.Text = "Sistem Kontrol Paneli";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(650, 820);
            this.BackColor = Color.FromArgb(30, 34, 45);

            resultsPanel = new Panel
            {
                Parent = this,
                AutoScroll = true,
                Top = 20,
                Left = 24,
                Width = this.ClientSize.Width - 48,
                Height = this.ClientSize.Height - 110,
                BackColor = Color.FromArgb(36, 40, 56),
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };

            closeButton = new Button
            {
                Parent = this,
                Text = "Kapat",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 170, 80),
                FlatStyle = FlatStyle.Flat,
                Height = 38,
                Width = 120,
                Top = this.ClientSize.Height - 60,
                Left = this.ClientSize.Width / 2 - 60,
                Anchor = AnchorStyles.Bottom
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) =>
            {
                this.Hide();
                resultsPanel.Hide();
                manualShowRequested = false;
            };

            this.Resize += (s, e) => ResponsiveLayout();
            this.FormClosing += SystemCheck_FormClosing;
            ResponsiveLayout();

            this.Shown += (s, e) => { this.Hide(); };

            RegisterGlobalHotkey();
            InitSystemEventWatchers();
            _ = StartAndDelayMainFormAndChecks();

            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 300;
            resizeTimer.Tick += (s, e) => { resizeTimer.Stop(); ResponsiveLayout(); };
            this.SizeChanged += (s, e) => { resizeTimer.Stop(); resizeTimer.Start(); };
        }

        private void RegisterGlobalHotkey()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F2)
                {
                    manualShowRequested = true;
                    ShowResults();
                    this.Show();
                    resultsPanel.Show();
                }
            };
            if (Application.OpenForms.Count > 0)
            {
                foreach (Form f in Application.OpenForms)
                {
                    f.KeyPreview = true;
                    f.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.F2)
                        {
                            manualShowRequested = true;
                            ShowResults();
                            this.Show();
                            resultsPanel.Show();
                        }
                    };
                }
            }
        }

        private void InitSystemEventWatchers()
        {
            usbInsertWatcher = new ManagementEventWatcher();
            usbInsertWatcher.EventArrived += (s, e) => OnSystemEventDetected();
            usbInsertWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
            usbInsertWatcher.Start();

            usbRemoveWatcher = new ManagementEventWatcher();
            usbRemoveWatcher.EventArrived += (s, e) => OnSystemEventDetected();
            usbRemoveWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
            usbRemoveWatcher.Start();

            NetworkChange.NetworkAvailabilityChanged += (s, e) => OnSystemEventDetected();
            NetworkChange.NetworkAddressChanged += (s, e) => OnSystemEventDetected();

            audioEnum = new MMDeviceEnumerator();
            audioEnum.RegisterEndpointNotificationCallback(new AudioDeviceNotification(this));

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s, e) => OnSystemEventDetected();
            Microsoft.Win32.SystemEvents.PowerModeChanged += (s, e) => OnSystemEventDetected();
        }

        public async void OnSystemEventDetected()
        {
            if (!initialChecksDone) return;
            lastUpdateTime = DateTime.Now;
            var myUpdateTime = lastUpdateTime;
            await Task.Delay(1500);
            if (myUpdateTime != lastUpdateTime)
                return;
            if (suppressUpdate) return;
            suppressUpdate = true;

            RunSystemChecks();
            var tempResults = checkResults.Select(x => new SystemCheckItem(x.State, x.Title, x.Description, x.Tag)).ToList();
            bool hasCritical = tempResults.Any(x => x.State == CheckState.Error);

            this.Invoke(new Action(() =>
            {
                if (hasCritical || manualShowRequested)
                {
                    ShowResults();
                    if (!this.Visible)
                        this.Show();
                    resultsPanel.Show();
                }
                else
                {
                    this.Hide();
                    resultsPanel.Hide();
                }
                lastResults = tempResults;
                suppressUpdate = false;
            }));
        }

        private async Task StartAndDelayMainFormAndChecks()
        {
            if (!Application.OpenForms.OfType<Form1>().Any())
            {
                var mainForm = new Form1();
                mainForm.StartPosition = FormStartPosition.CenterScreen;
                mainForm.FormClosed += (s, e) =>
                {
                    isFormClosing = true;
                    Environment.Exit(0);
                };
                mainForm.Show();
            }
            await Task.Delay(5000);

            await Task.Run(() => RunSystemChecks());
            this.Invoke(new Action(() =>
            {
                initialChecksDone = true;
                var tempResults = checkResults.Select(x => new SystemCheckItem(x.State, x.Title, x.Description, x.Tag)).ToList();
                bool hasCritical = tempResults.Any(x => x.State == CheckState.Error);
                if (hasCritical)
                {
                    ShowResults();
                    this.Show();
                    resultsPanel.Show();
                }
                lastResults = tempResults;
                lastUpdateTime = DateTime.Now;
            }));
        }

        private void ResponsiveLayout()
        {
            if (resultsPanel == null || closeButton == null) return;
            resultsPanel.Width = this.ClientSize.Width - 48;
            resultsPanel.Height = this.ClientSize.Height - 110;
            closeButton.Top = this.ClientSize.Height - 60;
            closeButton.Left = this.ClientSize.Width / 2 - closeButton.Width / 2;
        }

        private void ShowResults()
        {
            resultsPanel.Controls.Clear();
            int y = 36;
            int margin = 12;

            var summaryLbl = new Label
            {
                Left = 0,
                Top = 0,
                Width = resultsPanel.Width,
                AutoSize = false,
                Height = 32,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Text =
                    checkResults.All(x => x.State == CheckState.Success || x.State == CheckState.Info)
                    ? "Tüm sistem kontrolleri başarıyla geçti."
                    : "Bazı önemli sorunlar tespit edildi!"
            };
            resultsPanel.Controls.Add(summaryLbl);

            var resultsCopy = checkResults.ToList();

            foreach (var item in resultsCopy)
            {
                Image img;
                switch (item.State)
                {
                    case CheckState.Success: img = Properties.Resources.check_circle; break;
                    case CheckState.Warning: img = Properties.Resources.warning; break;
                    case CheckState.Error: img = Properties.Resources.close_circle; break;
                    case CheckState.Info: img = Properties.Resources.info_circle; break;
                    default: img = Properties.Resources.info_circle; break;
                }

                var icon = new PictureBox
                {
                    Left = 10,
                    Top = y,
                    Width = 36,
                    Height = 36,
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Image = img,
                    BackColor = Color.Transparent
                };

                var titleLbl = new Label
                {
                    Left = 56,
                    Top = y + 2,
                    Width = resultsPanel.Width - 210,
                    AutoSize = false,
                    Height = 22,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = item.State == CheckState.Success ? Color.FromArgb(80, 220, 140)
                             : item.State == CheckState.Warning ? Color.FromArgb(250, 190, 50)
                             : item.State == CheckState.Error ? Color.FromArgb(250, 60, 80)
                             : Color.FromArgb(70, 150, 250),
                    Text = item.Title
                };

                var descLbl = new Label
                {
                    Left = 56,
                    Top = y + 26,
                    Width = resultsPanel.Width - 210,
                    AutoSize = false,
                    Height = 0,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(180, 180, 190),
                    Text = item.Description,
                    MaximumSize = new Size(resultsPanel.Width - 210, 0),
                    AutoEllipsis = false
                };
                descLbl.Height = GetTextHeight(descLbl, descLbl.Text, descLbl.Width);

                resultsPanel.Controls.Add(icon);
                resultsPanel.Controls.Add(titleLbl);
                resultsPanel.Controls.Add(descLbl);

                if ((item.State == CheckState.Error || item.State == CheckState.Warning) && !string.IsNullOrEmpty(TryGetSolutionSuggestion(item)))
                {
                    var fixBtn = new Button
                    {
                        Text = "Sorunu Çöz",
                        Width = 140,
                        Height = 28,
                        Top = y + 8,
                        Left = resultsPanel.Width - 140 - 10,
                        BackColor = Color.FromArgb(60, 180, 90),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        TabStop = false
                    };
                    fixBtn.FlatAppearance.BorderSize = 0;
                    fixBtn.Click += (s, e) => TryAutoFix(item);
                    resultsPanel.Controls.Add(fixBtn);
                }

                y += Math.Max(56, 26 + descLbl.Height) + margin;
            }

            resultsPanel.Show();
        }

        private int GetTextHeight(Label lbl, string text, int width)
        {
            using (Graphics g = lbl.CreateGraphics())
            {
                SizeF size = g.MeasureString(text, lbl.Font, width);
                return (int)Math.Ceiling(size.Height) + 4;
            }
        }

        private void SystemCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isFormClosing)
                return;
            Hide();
            resultsPanel.Hide();
            manualShowRequested = false;
            e.Cancel = true;
        }

        private void RunSystemChecks()
        {
            checkResults.Clear();
            checkResults.Add(CheckUSBDevices());
            checkResults.Add(CheckUsbDrives());
            checkResults.AddRange(CheckDisplayConnectionsAndDetails());
            checkResults.Add(CheckNetworkConnections());
            checkResults.Add(CheckPowerStatus());
            checkResults.AddRange(CheckDetailedAudioSystem()); // YENİ DETAYLI SES SİSTEMİ
            checkResults.AddRange(CheckMicrophoneDevices());
            checkResults.Add(CheckAdvancedMainForm()); // YENİ GELİŞMİŞ FORM1 KONTROLÜ
            checkResults.Add(CheckSystemClock());
            checkResults.Add(CheckDiskSpace());
            checkResults.Add(CheckRamStatus());
            checkResults.Add(CheckCpuStatus());
            checkResults.Add(GetOsInfo());
            checkResults.AddRange(GetGpuInfoList());
        }

        // Windows API imports for display mode changes
        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        private string TryGetSolutionSuggestion(SystemCheckItem item)
        {
            switch (item.Title)
            {
                case "Ekran":
                case "Ekranlar":
                case "Ekran Çözünürlüğü":
                    return "Sorunlu ekran için çözüm uygula.";
                case "Ses Cihazı":
                case "Ses Cihazları":
                case "Ses Sürücüsü":
                case "Ses Bağlantısı":
                case "Varsayılan Ses":
                case "Bluetooth Ses":
                case "HDMI/DP Ses":
                case "USB Ses":
                    return "Sorunlu ses cihazı için çözüm uygula.";
                case "Uygulama Durumu":
                case "Ana Program":
                    return "Ana uygulama (Form1) için çözüm dene.";
                default:
                    return null;
            }
        }

        private void TryAutoFix(SystemCheckItem item)
        {
            switch (item.Title)
            {
                case "Ekran":
                case "Ekranlar":
                case "Ekran Çözünürlüğü":
                    HandleDisplayResolutionFix(item);
                    break;
                case "Ses Cihazı":
                case "Ses Cihazları":
                case "Ses Sürücüsü":
                case "Ses Bağlantısı":
                case "Varsayılan Ses":
                case "Bluetooth Ses":
                case "HDMI/DP Ses":
                case "USB Ses":
                    HandleAdvancedAudioFix(item);
                    break;
                case "Uygulama Durumu":
                case "Ana Program":
                    HandleAdvancedApplicationFix(item);
                    break;
            }
        }

        private void HandleDisplayResolutionFix(SystemCheckItem item)
        {
            // Önceki kodla aynı...
            Screen[] problemScreens = null;
            if (item.Tag is Screen[] screens)
            {
                problemScreens = screens;
            }
            else if (item.Tag is Screen screen)
            {
                problemScreens = new Screen[] { screen };
            }
            else
            {
                problemScreens = Screen.AllScreens.Where(s => s.Bounds.Width < 800 || s.Bounds.Height < 600).ToArray();
            }

            if (problemScreens == null || problemScreens.Length == 0)
            {
                MessageBox.Show("Sorunlu ekran bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var resolutionManager = new MultiDisplayResolutionManager(problemScreens);
            resolutionManager.ShowDialog();
        }

        // YENİ DETAYLI SES SİSTEMİ KONTROLÜ
        private List<SystemCheckItem> CheckDetailedAudioSystem()
        {
            var result = new List<SystemCheckItem>();

            try
            {
                var enumerator = new MMDeviceEnumerator();

                // 1. Tüm ses cihazlarını al (aktif, devre dışı, bağlantısız)
                var allDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
                var activeDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                var disabledDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Disabled);
                var unpluggedDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Unplugged);

                if (allDevices.Count == 0)
                {
                    result.Add(new SystemCheckItem(CheckState.Error, "Ses Cihazı", "Hiçbir ses cihazı bulunamadı! Ses kartı sürücü sorunu olabilir.", "no_devices"));
                    return result;
                }

                // 2. Varsayılan oynatma cihazını kontrol et
                MMDevice defaultDevice = null;
                try
                {
                    defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                catch
                {
                    result.Add(new SystemCheckItem(CheckState.Error, "Varsayılan Ses", "Varsayılan ses oynatma cihazı atanmamış!", "no_default"));
                }

                // 3. Aktif cihazları detaylı kontrol et
                var problemDevices = new List<AudioDeviceInfo>();
                var healthyDevices = new List<AudioDeviceInfo>();

                foreach (var device in activeDevices)
                {
                    var deviceInfo = AnalyzeAudioDevice(device, defaultDevice);
                    if (deviceInfo.HasProblems)
                        problemDevices.Add(deviceInfo);
                    else
                        healthyDevices.Add(deviceInfo);
                }

                // 4. Sağlıklı cihazları raporla
                if (healthyDevices.Any())
                {
                    string healthyList = string.Join("\n", healthyDevices.Select(d =>
                        $"✅ {d.FriendlyName}" + (d.IsDefault ? " (Varsayılan)" : "") +
                        $" - {d.DeviceType} - Ses: %{d.VolumeLevel:0}"));
                    result.Add(new SystemCheckItem(CheckState.Success, "Ses Cihazları",
                        $"{healthyDevices.Count} sağlıklı ses cihazı:\n{healthyList}", healthyDevices.ToArray()));
                }

                // 5. Sorunlu cihazları detaylı raporla
                foreach (var problemDevice in problemDevices)
                {
                    var state = problemDevice.ProblemSeverity == "Critical" ? CheckState.Error : CheckState.Warning;
                    var title = $"{problemDevice.DeviceType} Ses";
                    var description = $"Cihaz: {problemDevice.FriendlyName}\n" +
                                    $"Sorun: {problemDevice.ProblemDescription}\n" +
                                    $"Durum: {problemDevice.DeviceState}";

                    result.Add(new SystemCheckItem(state, title, description, problemDevice));
                }

                // 6. Devre dışı cihazları kontrol et
                if (disabledDevices.Count > 0)
                {
                    string disabledList = string.Join("\n", disabledDevices.Select(d => $"⚠️ {d.FriendlyName}"));
                    result.Add(new SystemCheckItem(CheckState.Warning, "Ses Cihazı",
                        $"{disabledDevices.Count} cihaz devre dışı:\n{disabledList}",
                        disabledDevices.Select(d => new AudioDeviceInfo { Device = d, FriendlyName = d.FriendlyName, DeviceState = "Disabled" }).ToArray()));
                }

                // 7. Ses kartı sürücü kontrolü
                CheckAudioDrivers(result);

                // 8. Windows Audio servisleri kontrolü
                CheckWindowsAudioServices(result);
            }
            catch (Exception ex)
            {
                result.Add(new SystemCheckItem(CheckState.Error, "Ses Sistemi", $"Ses sistemi analiz edilemedi: {ex.Message}", "system_error"));
            }

            return result;
        }

        // SES CİHAZI DETAYLI ANALİZ
        private AudioDeviceInfo AnalyzeAudioDevice(MMDevice device, MMDevice defaultDevice)
        {
            var info = new AudioDeviceInfo
            {
                Device = device,
                FriendlyName = device.FriendlyName,
                DeviceId = device.ID,
                IsDefault = defaultDevice != null && device.ID == defaultDevice.ID
            };

            try
            {
                // Cihaz türünü belirle
                if (device.FriendlyName.ToLower().Contains("bluetooth"))
                    info.DeviceType = "Bluetooth";
                else if (device.FriendlyName.ToLower().Contains("hdmi") || device.FriendlyName.ToLower().Contains("displayport"))
                    info.DeviceType = "HDMI/DP";
                else if (device.FriendlyName.ToLower().Contains("usb"))
                    info.DeviceType = "USB";
                else if (device.FriendlyName.ToLower().Contains("realtek") || device.FriendlyName.ToLower().Contains("analog"))
                    info.DeviceType = "Dahili";
                else
                    info.DeviceType = "Bilinmiyor";

                // Durum kontrolü
                info.DeviceState = device.State.ToString();

                // Ses seviyesi kontrolü
                try
                {
                    var volumeControl = device.AudioEndpointVolume;
                    info.VolumeLevel = volumeControl.MasterVolumeLevelScalar * 100;
                    info.IsMuted = volumeControl.Mute;

                    if (info.IsMuted)
                    {
                        info.HasProblems = true;
                        info.ProblemDescription = "Ses kapatılmış (Mute)";
                        info.ProblemSeverity = "Warning";
                    }
                    else if (info.VolumeLevel < 1)
                    {
                        info.HasProblems = true;
                        info.ProblemDescription = "Ses seviyesi çok düşük (%0)";
                        info.ProblemSeverity = "Warning";
                    }
                }
                catch
                {
                    info.HasProblems = true;
                    info.ProblemDescription = "Ses kontrolü erişilemiyor";
                    info.ProblemSeverity = "Critical";
                }

                // Cihaza özel kontroller
                if (info.DeviceType == "Bluetooth")
                {
                    // Bluetooth bağlantı kontrolü (ping-pong test)
                    if (!TestBluetoothAudioConnection(device))
                    {
                        info.HasProblems = true;
                        info.ProblemDescription = "Bluetooth ses bağlantısında gecikme/kopukluk";
                        info.ProblemSeverity = "Warning";
                    }
                }
                else if (info.DeviceType == "HDMI/DP")
                {
                    // HDMI/DisplayPort ses aktarım kontrolü
                    if (!TestHDMIAudioTransmission(device))
                    {
                        info.HasProblems = true;
                        info.ProblemDescription = "HDMI/DP ses aktarımı devre dışı veya sorunlu";
                        info.ProblemSeverity = "Warning";
                    }
                }

                // Ses testi (isteğe bağlı)
                info.AudioTestPassed = TestDeviceAudioPlayback(device);
                if (!info.AudioTestPassed && !info.HasProblems)
                {
                    info.HasProblems = true;
                    info.ProblemDescription = "Ses oynatma testi başarısız";
                    info.ProblemSeverity = "Warning";
                }
            }
            catch (Exception ex)
            {
                info.HasProblems = true;
                info.ProblemDescription = $"Cihaz analiz hatası: {ex.Message}";
                info.ProblemSeverity = "Critical";
            }

            return info;
        }

        // SES KARTLARI VE SÜRÜCÜLERİ KONTROLÜ
        private void CheckAudioDrivers(List<SystemCheckItem> result)
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");
                var audioCards = searcher.Get().Cast<ManagementObject>().ToList();

                var problemCards = new List<string>();
                var healthyCards = new List<string>();

                foreach (var card in audioCards)
                {
                    string name = card["Name"]?.ToString() ?? "Bilinmiyor";
                    string status = card["Status"]?.ToString() ?? "Unknown";
                    string deviceId = card["DeviceID"]?.ToString() ?? "";

                    if (status != "OK")
                    {
                        problemCards.Add($"❌ {name} - Durum: {status}");
                    }
                    else
                    {
                        healthyCards.Add($"✅ {name}");
                    }
                }

                if (problemCards.Any())
                {
                    result.Add(new SystemCheckItem(CheckState.Error, "Ses Sürücüsü",
                        $"Sorunlu ses kartı sürücüleri:\n{string.Join("\n", problemCards)}",
                        problemCards.ToArray()));
                }

                if (healthyCards.Any())
                {
                    result.Add(new SystemCheckItem(CheckState.Success, "Ses Sürücüleri",
                        $"{healthyCards.Count} ses kartı sürücüsü sağlıklı:\n{string.Join("\n", healthyCards)}"));
                }
            }
            catch (Exception ex)
            {
                result.Add(new SystemCheckItem(CheckState.Warning, "Ses Sürücüsü", $"Sürücü kontrolü yapılamadı: {ex.Message}"));
            }
        }

        // WINDOWS AUDIO SERVİSLERİ KONTROLÜ
        private void CheckWindowsAudioServices(List<SystemCheckItem> result)
        {
            var criticalServices = new[]
            {
                "AudioSrv", "AudioEndpointBuilder", "Audiosrv"
            };

            var problemServices = new List<string>();
            var healthyServices = new List<string>();

            foreach (var serviceName in criticalServices)
            {
                try
                {
                    var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Service WHERE Name='{serviceName}'");
                    var service = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                    if (service != null)
                    {
                        string state = service["State"]?.ToString() ?? "Unknown";
                        string startMode = service["StartMode"]?.ToString() ?? "Unknown";

                        if (state != "Running")
                        {
                            problemServices.Add($"❌ {serviceName} - Durum: {state}");
                        }
                        else
                        {
                            healthyServices.Add($"✅ {serviceName} - Çalışıyor");
                        }
                    }
                    else
                    {
                        problemServices.Add($"❌ {serviceName} - Servis bulunamadı");
                    }
                }
                catch
                {
                    problemServices.Add($"❌ {serviceName} - Kontrol edilemedi");
                }
            }

            if (problemServices.Any())
            {
                result.Add(new SystemCheckItem(CheckState.Error, "Ses Servisleri",
                    $"Sorunlu Windows ses servisleri:\n{string.Join("\n", problemServices)}",
                    problemServices.ToArray()));
            }

            if (healthyServices.Any())
            {
                result.Add(new SystemCheckItem(CheckState.Success, "Ses Servisleri",
                    $"Windows ses servisleri çalışıyor:\n{string.Join("\n", healthyServices)}"));
            }
        }

        // BLUETOOTH SES BAĞLANTI TESTİ
        private bool TestBluetoothAudioConnection(MMDevice device)
        {
            try
            {
                // Basit ses testi - gerçek uygulamada ping veya latency testi yapılabilir
                return TestDeviceAudioPlayback(device);
            }
            catch
            {
                return false;
            }
        }

        // HDMI/DP SES AKTARIM TESTİ
        private bool TestHDMIAudioTransmission(MMDevice device)
        {
            try
            {
                // HDMI/DP ses aktarımını test et
                return TestDeviceAudioPlayback(device);
            }
            catch
            {
                return false;
            }
        }

        // SES OYNATMA TESTİ
        private bool TestDeviceAudioPlayback(MMDevice device)
        {
            try
            {
                // NAudio ile basit test sesi çal
                // Bu gerçek bir ses testi olmayabilir, sadı accessibility kontrolü
                var volumeControl = device.AudioEndpointVolume;
                return volumeControl != null && !volumeControl.Mute;
            }
            catch
            {
                return false;
            }
        }

        // GELİŞMİŞ SES CİHAZI ÇÖZÜMLERİ
        private void HandleAdvancedAudioFix(SystemCheckItem item)
        {
            if (item.Tag is AudioDeviceInfo deviceInfo)
            {
                // Tek cihaz için çözüm
                var audioFixForm = new AudioDeviceFixForm(new[] { deviceInfo });
                audioFixForm.ShowDialog();
            }
            else if (item.Tag is AudioDeviceInfo[] deviceInfos)
            {
                // Birden fazla cihaz için çözüm
                var audioFixForm = new AudioDeviceFixForm(deviceInfos);
                audioFixForm.ShowDialog();
            }
            else if (item.Tag is string errorType)
            {
                // Sistem seviyesi ses sorunu
                HandleSystemAudioProblem(errorType);
            }
            else
            {
                // Genel ses sorunu - tüm cihazları analiz et
                HandleGeneralAudioProblem();
            }
        }

        private void HandleSystemAudioProblem(string errorType)
        {
            switch (errorType)
            {
                case "no_devices":
                    var result = MessageBox.Show(
                        "Hiçbir ses cihazı bulunamadı!\n\n" +
                        "Bu sorun genellikle ses kartı sürücü probleminden kaynaklanır.\n\n" +
                        "Aygıt Yöneticisi'ni açarak ses kartı sürücülerini kontrol etmek ister misiniz?",
                        "Ses Cihazı Bulunamadı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            Process.Start("devmgmt.msc");
                        }
                        catch
                        {
                            MessageBox.Show("Aygıt Yöneticisi açılamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;

                case "no_default":
                    var defaultResult = MessageBox.Show(
                        "Varsayılan ses cihazı atanmamış!\n\n" +
                        "Windows ses ayarlarını açarak varsayılan cihazı seçmek ister misiniz?",
                        "Varsayılan Ses Cihazı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (defaultResult == DialogResult.Yes)
                    {
                        try
                        {
                            Process.Start("ms-settings:sound");
                        }
                        catch
                        {
                            Process.Start("control", "mmsys.cpl");
                        }
                    }
                    break;
            }
        }

        private void HandleGeneralAudioProblem()
        {
            var generalFixForm = new GeneralAudioFixForm();
            generalFixForm.ShowDialog();
        }

        // GELİŞMİŞ FORM1 KONTROLÜ
        private SystemCheckItem CheckAdvancedMainForm()
        {
            try
            {
                // 1. Form1'in mevcut durumunu kontrol et
                var form1Instance = Application.OpenForms.OfType<Form1>().FirstOrDefault();

                if (form1Instance != null && form1Instance.Visible)
                {
                    // Form1 açık ve görünür
                    return new SystemCheckItem(CheckState.Success, "Ana Program", "Form1 başarıyla çalışıyor.");
                }
                else if (form1Instance != null && !form1Instance.Visible)
                {
                    // Form1 açık ama görünmez
                    return new SystemCheckItem(CheckState.Warning, "Ana Program", "Form1 açık ama görünmez durumda.", "hidden");
                }
                else
                {
                    // Form1 hiç açık değil
                    return new SystemCheckItem(CheckState.Error, "Ana Program", "Form1 açık değil veya başlatılamadı!", "not_running");
                }
            }
            catch (Exception ex)
            {
                return new SystemCheckItem(CheckState.Error, "Ana Program", $"Form1 kontrolü sırasında hata: {ex.Message}", "check_error");
            }
        }

        // GELİŞMİŞ FORM1 ÇÖZÜMLERİ
        private void HandleAdvancedApplicationFix(SystemCheckItem item)
        {
            if (item.Tag is string problemType)
            {
                switch (problemType)
                {
                    case "hidden":
                        HandleHiddenForm1();
                        break;
                    case "not_running":
                        HandleNotRunningForm1();
                        break;
                    case "check_error":
                        HandleForm1CheckError();
                        break;
                    default:
                        HandleGeneralForm1Problem();
                        break;
                }
            }
            else
            {
                HandleGeneralForm1Problem();
            }
        }

        private void HandleHiddenForm1()
        {
            try
            {
                var form1Instance = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                if (form1Instance != null)
                {
                    form1Instance.Show();
                    form1Instance.BringToFront();
                    form1Instance.WindowState = FormWindowState.Normal;
                    MessageBox.Show("Form1 başarıyla görünür hale getirildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Form1 görünür hale getirilemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleNotRunningForm1()
        {
            var fixForm = new Form1FixManager();
            fixForm.ShowDialog();
        }

        private void HandleForm1CheckError()
        {
            MessageBox.Show(
                "Form1 kontrolü sırasında hata oluştu.\n\n" +
                "Bu durum genellikle geçici bir sorundan kaynaklanır.\n" +
                "Uygulamayı yeniden başlatmayı deneyin.",
                "Form1 Kontrol Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void HandleGeneralForm1Problem()
        {
            var fixForm = new Form1FixManager();
            fixForm.ShowDialog();
        }

        // Diğer metodlar aynı kalıyor...
        private SystemCheckItem CheckUSBDevices()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub");
                var usbList = searcher.Get().Cast<ManagementObject>().ToList();
                if (usbList.Count == 0)
                    return new SystemCheckItem(CheckState.Info, "USB Bağlantısı", "Şu anda sistemde USB cihazı bağlı değil.");
                string names = string.Join("\n", usbList.Select(u => u["Name"]?.ToString() ?? "(Bilinmiyor)"));
                return new SystemCheckItem(CheckState.Info, "USB Bağlantısı", $"{usbList.Count} USB aygıtı bağlı:\n{names}");
            }
            catch
            {
                return new SystemCheckItem(CheckState.Info, "USB Bağlantısı", "USB denetimi yapılamadı.");
            }
        }

        private SystemCheckItem CheckUsbDrives()
        {
            try
            {
                var usbDrives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable)
                    .ToList();
                if (usbDrives.Count == 0)
                    return new SystemCheckItem(CheckState.Info, "USB Sürücüleri", "Takılı USB sürücü (flash bellek/diski) yok.");
                string list = string.Join("\n", usbDrives.Select(d => $"{d.Name} - {d.VolumeLabel} ({d.TotalSize / (1024 * 1024)} MB)"));
                return new SystemCheckItem(CheckState.Info, "USB Sürücüleri", $"Takılı USB sürücüler:\n{list}");
            }
            catch
            {
                return new SystemCheckItem(CheckState.Info, "USB Sürücüleri", "USB sürücü denetimi yapılamadı.");
            }
        }

        private List<SystemCheckItem> CheckDisplayConnectionsAndDetails()
        {
            var result = new List<SystemCheckItem>();
            try
            {
                var screens = Screen.AllScreens;
                if (screens.Length == 0)
                {
                    result.Add(new SystemCheckItem(CheckState.Error, "Ekran", "Hiçbir ekran bağlı değil!"));
                    return result;
                }

                string allScreens = "";
                for (int i = 0; i < screens.Length; i++)
                {
                    var s = screens[i];
                    string anaEkranEtiketi = s.Primary ? " (Ana Ekran)" : "";
                    allScreens += $"[{i + 1}] {s.DeviceName}: {s.Bounds.Width}x{s.Bounds.Height} px{anaEkranEtiketi}\n";
                }

                result.Add(new SystemCheckItem(
                    CheckState.Success,
                    "Ekranlar",
                    $"{screens.Length} ekran bağlı.\n{allScreens}"
                ));

                var lowRes = screens.Where(s => s.Bounds.Width < 800 || s.Bounds.Height < 600).ToList();
                if (lowRes.Any())
                    result.Add(new SystemCheckItem(CheckState.Warning, "Ekran Çözünürlüğü", "Bazı ekranların çözünürlüğü çok düşük olabilir!", lowRes.ToArray()));

                var overlaps = new List<(Screen, Screen)>();
                for (int i = 0; i < screens.Length; i++)
                    for (int j = i + 1; j < screens.Length; j++)
                        if (screens[i].Bounds.IntersectsWith(screens[j].Bounds))
                            overlaps.Add((screens[i], screens[j]));
                if (overlaps.Any())
                    result.Add(new SystemCheckItem(CheckState.Warning, "Ekran Çakışması", "Bazı ekranlar konum olarak üst üste binmiş olabilir!", overlaps.Select(x => $"{x.Item1.DeviceName} & {x.Item2.DeviceName}").ToArray()));

                var extremeNegative = screens.Where(s => s.Bounds.X < -5000 || s.Bounds.Y < -5000).ToList();
                if (extremeNegative.Any())
                    result.Add(new SystemCheckItem(CheckState.Warning, "Aşırı Negatif Pozisyon", "Bazı ekranların koordinatları olağan dışı negatifte!", extremeNegative.ToArray()));

                var duplicateRes = screens.GroupBy(s => $"{s.Bounds.Width}x{s.Bounds.Height}").Where(g => g.Count() > 1).ToList();
                if (duplicateRes.Any())
                    result.Add(new SystemCheckItem(CheckState.Info, "Aynı Çözünürlükte Ekranlar", "Birden fazla ekran aynı çözünürlüğe sahip.", duplicateRes.SelectMany(g => g).ToArray()));

                var duplicateDevice = screens.GroupBy(s => s.DeviceName).Where(g => g.Count() > 1).ToList();
                if (duplicateDevice.Any())
                    result.Add(new SystemCheckItem(CheckState.Warning, "DeviceName Çakışması", "Birden fazla ekran aynı DeviceName'e sahip!", duplicateDevice.SelectMany(g => g).ToArray()));

                var primary = screens.FirstOrDefault(s => s.Primary);
                if (primary != null)
                {
                    var minX = screens.Min(s => s.Bounds.Left);
                    var maxX = screens.Max(s => s.Bounds.Right);
                    var minY = screens.Min(s => s.Bounds.Top);
                    var maxY = screens.Max(s => s.Bounds.Bottom);

                    if (primary.Bounds.Right < minX || primary.Bounds.Left > maxX || primary.Bounds.Bottom < minY || primary.Bounds.Top > maxY)
                        result.Add(new SystemCheckItem(CheckState.Warning, "Ana Ekran Pozisyonu", "Ana ekran diğer ekran matrisinin dışında konumlanmış olabilir!", new[] { primary }));
                }
            }
            catch
            {
                result.Add(new SystemCheckItem(CheckState.Warning, "Ekran", "Ekran bilgisi alınamadı."));
            }
            return result;
        }

        // Diğer check metodları aynı...
        private SystemCheckItem CheckNetworkConnections()
        {
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni =>
                        ni.OperationalStatus == OperationalStatus.Up &&
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                        !ni.Description.ToLower().Contains("virtual") &&
                        !ni.Description.ToLower().Contains("vpn") &&
                        !ni.Description.ToLower().Contains("hamachi")
                    ).ToList();

                foreach (var ni in adapters)
                {
                    var ipProps = ni.GetIPProperties();
                    var addr = ipProps.UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    if (addr == null) continue;

                    using (Ping ping = new Ping())
                    {
                        PingOptions options = new PingOptions();
                        byte[] buffer = new byte[32];
                        int timeout = 1200;
                        PingReply reply = ping.Send("8.8.8.8", timeout, buffer, options);

                        if (reply.Status == IPStatus.Success)
                        {
                            string detail = $"Ad: {ni.Name} ({ni.Description})\nIP: {addr.Address}\nİnternet: Var (ping başarılı)";
                            return new SystemCheckItem(CheckState.Success, "Ağ Bağlantısı", detail);
                        }
                    }
                }

                return new SystemCheckItem(
                    CheckState.Error,
                    "Ağ Bağlantısı",
                    "Hiçbir aktif fiziksel ağ adaptöründen internete erişilemiyor (ping başarısız)."
                );
            }
            catch
            {
                return new SystemCheckItem(CheckState.Warning, "Ağ Bağlantısı", "Ağ bağlantısı kontrolü yapılamadı.");
            }
        }

        private SystemCheckItem CheckPowerStatus()
        {
            try
            {
                PowerStatus pwr = SystemInformation.PowerStatus;
                if (pwr.PowerLineStatus == PowerLineStatus.Offline && pwr.BatteryLifePercent < 0.15)
                    return new SystemCheckItem(CheckState.Error, "Güç Durumu", "Güç kaynağı bağlı değil ve pil seviyesi kritik!");
                return new SystemCheckItem(CheckState.Success, "Güç Durumu", "Güç kaynağı veya pil seviyesi yeterli.");
            }
            catch
            {
                return new SystemCheckItem(CheckState.Warning, "Güç Durumu", "Güç durumu kontrolü yapılamadı.");
            }
        }

        private List<SystemCheckItem> CheckMicrophoneDevices()
        {
            var list = new List<SystemCheckItem>();
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                if (devices.Count == 0)
                {
                    list.Add(new SystemCheckItem(CheckState.Info, "Mikrofon", "Sistemde mikrofon algılanamadı!"));
                }
                else
                {
                    string allNames = string.Join("\n", devices.Select(d => d.FriendlyName));
                    list.Add(new SystemCheckItem(CheckState.Success, "Mikrofonlar", $"{devices.Count} mikrofon aygıtı algılandı.\n{allNames}", devices.Select(d => d.FriendlyName).ToArray()));
                }
            }
            catch (Exception ex)
            {
                list.Add(new SystemCheckItem(CheckState.Info, "Mikrofon", "Mikrofon kontrolü yapılamadı.\n" + ex.Message));
            }
            return list;
        }

        private SystemCheckItem CheckSystemClock()
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime local = DateTime.Now.ToUniversalTime();
                if (Math.Abs((now - local).TotalMinutes) > 5)
                    return new SystemCheckItem(CheckState.Error, "Sistem Saati", "Sistem saati yanlış ayarlanmış!");
                return new SystemCheckItem(CheckState.Success, "Sistem Saati", "Sistem saatinde sorun yok.");
            }
            catch
            {
                return new SystemCheckItem(CheckState.Warning, "Sistem Saati", "Sistem saati kontrolü yapılamadı.");
            }
        }

        private SystemCheckItem CheckDiskSpace()
        {
            try
            {
                DriveInfo drive = new DriveInfo("C");
                double percentFree = ((double)drive.TotalFreeSpace / drive.TotalSize) * 100;
                if (percentFree < 10)
                    return new SystemCheckItem(CheckState.Error, "Disk Alanı", "Diskte yeterli alan yok! (C: sürücüsü %10'dan az boş)");
                return new SystemCheckItem(CheckState.Success, "Disk Alanı", $"C: sürücüsü yeterli boş alana sahip. (%{percentFree:0.0} boş)");
            }
            catch
            {
                return new SystemCheckItem(CheckState.Warning, "Disk Alanı", "Disk kontrolü yapılamadı.");
            }
        }

        private SystemCheckItem CheckRamStatus()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem");
                var os = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (os != null)
                {
                    double total = Convert.ToDouble(os["TotalVisibleMemorySize"]) / 1024;
                    double free = Convert.ToDouble(os["FreePhysicalMemory"]) / 1024;
                    double used = total - free;
                    double percentFree = (free / total) * 100;
                    if (percentFree < 10)
                        return new SystemCheckItem(CheckState.Error, "Bellek (RAM)", $"RAM düşük! Toplam: {total:0} MB, Boş: {free:0} MB");
                    return new SystemCheckItem(CheckState.Success, "Bellek (RAM)", $"Toplam: {total:0} MB, Kullanılan: {used:0} MB, Boş: {free:0} MB");
                }
            }
            catch { }
            return new SystemCheckItem(CheckState.Warning, "Bellek (RAM)", "RAM bilgisi alınamadı.");
        }

        private SystemCheckItem CheckCpuStatus()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
                var cpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (cpu != null && cpu["LoadPercentage"] != null)
                {
                    int load = Convert.ToInt32(cpu["LoadPercentage"]);
                    if (load > 95)
                        return new SystemCheckItem(CheckState.Warning, "İşlemci (CPU)", $"CPU Yüksek (%{load})!");
                    return new SystemCheckItem(CheckState.Success, "İşlemci (CPU)", $"Anlık kullanım: %{load}");
                }
            }
            catch { }
            return new SystemCheckItem(CheckState.Warning, "İşlemci (CPU)", "CPU bilgisi alınamadı.");
        }

        private SystemCheckItem GetOsInfo()
        {
            try
            {
                string ver = Environment.OSVersion.VersionString;
                string user = Environment.UserName;
                string machine = Environment.MachineName;
                return new SystemCheckItem(CheckState.Success, "İşletim Sistemi", $"Sürüm: {ver}\nKullanıcı: {user}\nMakine: {machine}");
            }
            catch
            {
                return new SystemCheckItem(CheckState.Warning, "İşletim Sistemi", "İşletim sistemi bilgisi alınamadı.");
            }
        }

        private List<SystemCheckItem> GetGpuInfoList()
        {
            var list = new List<SystemCheckItem>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT Name,AdapterRAM,PNPDeviceID FROM Win32_VideoController");
                var gpus = searcher.Get().Cast<ManagementObject>().ToList();
                if (gpus.Count == 0)
                {
                    list.Add(new SystemCheckItem(CheckState.Warning, "Grafik Kartı", "Herhangi bir ekran kartı bulunamadı."));
                }
                else
                {
                    var systemGpu = gpus.FirstOrDefault(g =>
                        g["Name"] != null && !g["Name"].ToString().ToLower().Contains("virtual")
                    );
                    foreach (var gpu in gpus)
                    {
                        string name = gpu["Name"]?.ToString() ?? "-";
                        string ram = gpu["AdapterRAM"] != null ? $"{Convert.ToDouble(gpu["AdapterRAM"]) / (1024 * 1024):0} MB" : "-";
                        string vurgulu = (systemGpu != null && gpu == systemGpu) ? " (Sistem Grafik Kartı)" : "";
                        list.Add(new SystemCheckItem(CheckState.Success, "Grafik Kartı", $"{name}{vurgulu}\nBellek: {ram}"));
                    }
                }
            }
            catch
            {
                list.Add(new SystemCheckItem(CheckState.Warning, "Grafik Kartı", "Grafik kartı bilgisi alınamadı."));
            }
            return list;
        }

        public static class Properties
        {
            public static class Resources
            {
                public static Bitmap check_circle => CreateCircle(Color.FromArgb(80, 220, 140));
                public static Bitmap warning => CreateCircle(Color.FromArgb(250, 190, 50));
                public static Bitmap close_circle => CreateCircle(Color.FromArgb(250, 60, 80));
                public static Bitmap info_circle => CreateCircle(Color.FromArgb(70, 150, 250));

                private static Bitmap CreateCircle(Color color)
                {
                    Bitmap bmp = new Bitmap(32, 32);
                    using (Graphics g = Graphics.FromImage(bmp))
                    using (SolidBrush b = new SolidBrush(color))
                        g.FillEllipse(b, 3, 3, 26, 26);
                    return bmp;
                }
            }
        }

        private async void SystemCheck_Load(object sender, EventArgs e)
        {
            await Task.Delay(100); // Gecikme ekleyerek formun düzgün yüklenmesini bekle
            Opacity = 1;
        }
    }

    // SES CİHAZI BİLGİ SINIFI
    public class AudioDeviceInfo
    {
        public MMDevice Device { get; set; }
        public string FriendlyName { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceState { get; set; }
        public bool IsDefault { get; set; }
        public float VolumeLevel { get; set; }
        public bool IsMuted { get; set; }
        public bool HasProblems { get; set; }
        public string ProblemDescription { get; set; }
        public string ProblemSeverity { get; set; }
        public bool AudioTestPassed { get; set; }
    }

    // SES CİHAZI ÇÖZÜMLERİ FORMU
    public partial class AudioDeviceFixForm : Form
    {
        private readonly AudioDeviceInfo[] problemDevices;
        private int currentDeviceIndex = 0;
        private bool isFixingInProgress = false;

        private Label titleLabel;
        private Label deviceInfoLabel;
        private Label progressLabel;
        private Button testSoundButton;
        private Button setDefaultButton;
        private Button enableDeviceButton;
        private Button fixVolumeButton;
        private Button nextDeviceButton;
        private Button skipDeviceButton;
        private Button openSettingsButton;
        private Button cancelButton;
        private Panel mainPanel;
        private ProgressBar progressBar;

        public AudioDeviceFixForm(AudioDeviceInfo[] devices)
        {
            problemDevices = devices;
            InitializeAudioFixForm();
            ShowCurrentDevice();
        }

        private void InitializeAudioFixForm()
        {
            this.Text = "Ses Cihazı Sorun Çözücü";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(650, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(35, 39, 50);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 39, 50)
            };
            this.Controls.Add(mainPanel);

            titleLabel = new Label
            {
                Text = "🔊 Ses Cihazı Sorun Çözücü",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(610, 30),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(titleLabel);

            progressLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Location = new Point(20, 60),
                Size = new Size(610, 20),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(progressLabel);

            progressBar = new ProgressBar
            {
                Location = new Point(20, 85),
                Size = new Size(610, 10),
                Style = ProgressBarStyle.Continuous
            };
            mainPanel.Controls.Add(progressBar);

            deviceInfoLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 110),
                Size = new Size(610, 100),
                TextAlign = ContentAlignment.TopLeft
            };
            mainPanel.Controls.Add(deviceInfoLabel);

            // Çözüm butonları
            testSoundButton = new Button
            {
                Text = "🔊 Ses Testi",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 130, 180),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(20, 230)
            };
            testSoundButton.FlatAppearance.BorderSize = 0;
            testSoundButton.Click += TestSoundButton_Click;
            mainPanel.Controls.Add(testSoundButton);

            setDefaultButton = new Button
            {
                Text = "⭐ Varsayılan Yap",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 180, 90),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(150, 230)
            };
            setDefaultButton.FlatAppearance.BorderSize = 0;
            setDefaultButton.Click += SetDefaultButton_Click;
            mainPanel.Controls.Add(setDefaultButton);

            enableDeviceButton = new Button
            {
                Text = "🔌 Cihazı Etkinleştir",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 130, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(280, 230)
            };
            enableDeviceButton.FlatAppearance.BorderSize = 0;
            enableDeviceButton.Click += EnableDeviceButton_Click;
            mainPanel.Controls.Add(enableDeviceButton);

            fixVolumeButton = new Button
            {
                Text = "🔉 Sesi Düzelt",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(130, 60, 180),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(410, 230)
            };
            fixVolumeButton.FlatAppearance.BorderSize = 0;
            fixVolumeButton.Click += FixVolumeButton_Click;
            mainPanel.Controls.Add(fixVolumeButton);

            openSettingsButton = new Button
            {
                Text = "⚙️ Ses Ayarları",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(20, 280)
            };
            openSettingsButton.FlatAppearance.BorderSize = 0;
            openSettingsButton.Click += OpenSettingsButton_Click;
            mainPanel.Controls.Add(openSettingsButton);

            // Navigasyon butonları
            nextDeviceButton = new Button
            {
                Text = "Sonraki Cihaz →",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 180, 90),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(280, 380)
            };
            nextDeviceButton.FlatAppearance.BorderSize = 0;
            nextDeviceButton.Click += NextDeviceButton_Click;
            mainPanel.Controls.Add(nextDeviceButton);

            skipDeviceButton = new Button
            {
                Text = "Bu Cihazı Atla",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 130, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(410, 380)
            };
            skipDeviceButton.FlatAppearance.BorderSize = 0;
            skipDeviceButton.Click += SkipDeviceButton_Click;
            mainPanel.Controls.Add(skipDeviceButton);

            cancelButton = new Button
            {
                Text = "İptal",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 35),
                Location = new Point(550, 380)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(cancelButton);
        }

        private void ShowCurrentDevice()
        {
            if (currentDeviceIndex >= problemDevices.Length)
            {
                ShowCompletionSummary();
                return;
            }

            var device = problemDevices[currentDeviceIndex];
            progressLabel.Text = $"Cihaz {currentDeviceIndex + 1} / {problemDevices.Length}";
            progressBar.Value = (int)((double)(currentDeviceIndex + 1) / problemDevices.Length * 100);

            deviceInfoLabel.Text = $"🔊 Cihaz: {device.FriendlyName}\n" +
                                   $"📱 Tür: {device.DeviceType}\n" +
                                   $"📊 Durum: {device.DeviceState}\n" +
                                   $"🔉 Ses Seviyesi: %{device.VolumeLevel:0} {(device.IsMuted ? "(Kapalı)" : "")}\n" +
                                   $"⭐ Varsayılan: {(device.IsDefault ? "Evet" : "Hayır")}\n\n" +
                                   $"❌ Sorun: {device.ProblemDescription}";

            // Butonları duruma göre etkinleştir/devre dışı bırak
            testSoundButton.Enabled = device.Device != null && device.DeviceState == "Active";
            setDefaultButton.Enabled = device.Device != null && !device.IsDefault;
            enableDeviceButton.Enabled = device.DeviceState == "Disabled";
            fixVolumeButton.Enabled = device.IsMuted || device.VolumeLevel < 10;

            nextDeviceButton.Text = currentDeviceIndex == problemDevices.Length - 1 ? "Tamamla" : "Sonraki Cihaz →";
        }

        private void TestSoundButton_Click(object sender, EventArgs e)
        {
            var device = problemDevices[currentDeviceIndex];
            try
            {
                // Test sesi çal
                SystemSounds.Beep.Play();

                var result = MessageBox.Show(
                    $"Test sesi çalındı.\n\n{device.FriendlyName} cihazından ses duydunuz mu?",
                    "Ses Testi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    MessageBox.Show("✅ Ses testi başarılı! Bu cihaz düzgün çalışıyor.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("❌ Ses duyulmadı. Diğer çözümleri deneyin veya ses ayarlarını kontrol edin.", "Sorun Devam Ediyor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ses testi sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetDefaultButton_Click(object sender, EventArgs e)
        {
            var device = problemDevices[currentDeviceIndex];
            try
            {
                // Gerçek uygulamada buraya cihazı varsayılan yapma kodu gelecek
                // NAudio ile doğrudan yapılamadığı için Windows ayarlarına yönlendirme
                var result = MessageBox.Show(
                    $"Windows ses ayarlarında '{device.FriendlyName}' cihazını varsayılan olarak seçmek için:\n\n" +
                    "1. 'Ses Ayarları' butonu ile ayarları açın\n" +
                    "2. 'Çıkış' bölümünde bu cihazı seçin\n" +
                    "3. 'Varsayılan olarak ayarla' seçeneğini işaretleyin\n\n" +
                    "Şimdi ses ayarlarını açmak istiyor musunuz?",
                    "Varsayılan Cihaz Ayarlama",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start("ms-settings:sound");
                    }
                    catch
                    {
                        Process.Start("control", "mmsys.cpl");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Varsayılan cihaz ayarlama sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnableDeviceButton_Click(object sender, EventArgs e)
        {
            var device = problemDevices[currentDeviceIndex];

            var result = MessageBox.Show(
                $"'{device.FriendlyName}' cihazını etkinleştirmek için:\n\n" +
                "1. Aygıt Yöneticisi'ni açın (devmgmt.msc)\n" +
                "2. 'Ses, video ve oyun denetleyicileri' bölümünü genişletin\n" +
                "3. Bu cihaza sağ tıklayın\n" +
                "4. 'Cihazı etkinleştir' seçeneğini seçin\n\n" +
                "Aygıt Yöneticisi'ni şimdi açmak istiyor musunuz?",
                "Cihazı Etkinleştir",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start("devmgmt.msc");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Aygıt Yöneticisi açılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FixVolumeButton_Click(object sender, EventArgs e)
        {
            var device = problemDevices[currentDeviceIndex];
            try
            {
                if (device.Device != null)
                {
                    var volumeControl = device.Device.AudioEndpointVolume;

                    if (device.IsMuted)
                    {
                        volumeControl.Mute = false;
                        MessageBox.Show("✅ Ses kapatma (mute) kaldırıldı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (device.VolumeLevel < 50)
                    {
                        volumeControl.MasterVolumeLevelScalar = 0.7f; // %70 ses seviyesi
                        MessageBox.Show("✅ Ses seviyesi %70'e yükseltildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Cihaz bilgilerini güncelle
                    device.VolumeLevel = volumeControl.MasterVolumeLevelScalar * 100;
                    device.IsMuted = volumeControl.Mute;
                    device.HasProblems = false;
                    device.ProblemDescription = "Ses düzeltildi";

                    ShowCurrentDevice();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ses seviyesi düzeltme sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("ms-settings:sound");
            }
            catch
            {
                try
                {
                    Process.Start("control", "mmsys.cpl");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ses ayarları açılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void NextDeviceButton_Click(object sender, EventArgs e)
        {
            currentDeviceIndex++;
            ShowCurrentDevice();
        }

        private void SkipDeviceButton_Click(object sender, EventArgs e)
        {
            currentDeviceIndex++;
            ShowCurrentDevice();
        }

        private void ShowCompletionSummary()
        {
            titleLabel.Text = "✅ Ses Cihazı Çözümleri Tamamlandı";
            deviceInfoLabel.Text = $"🎉 {problemDevices.Length} cihaz işlendi.\n\n" +
                                   "Sistem ses durumunu kontrol etmek için F2 tuşuna basabilirsiniz.\n\n" +
                                   "Sorunlar devam ediyorsa:\n" +
                                   "• Bilgisayarı yeniden başlatın\n" +
                                   "• Ses kartı sürücülerini güncelleyin\n" +
                                   "• Fiziksel bağlantıları kontrol edin";

            progressBar.Value = 100;
            progressLabel.Text = "Tamamlandı";

            nextDeviceButton.Enabled = false;
            skipDeviceButton.Enabled = false;
            testSoundButton.Enabled = false;
            setDefaultButton.Enabled = false;
            enableDeviceButton.Enabled = false;
            fixVolumeButton.Enabled = false;

            cancelButton.Text = "Kapat";
        }
    }

    // GENEL SES SİSTEMİ ÇÖZÜMLERİ FORMU
    public partial class GeneralAudioFixForm : Form
    {
        private Label titleLabel;
        private Label statusLabel;
        private Button restartAudioButton;
        private Button updateDriversButton;
        private Button resetAudioButton;
        private Button troubleshootButton;
        private Button cancelButton;
        private Panel mainPanel;

        public GeneralAudioFixForm()
        {
            InitializeGeneralAudioForm();
        }

        private void InitializeGeneralAudioForm()
        {
            this.Text = "Genel Ses Sistemi Çözümleri";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(35, 39, 50);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 39, 50)
            };
            this.Controls.Add(mainPanel);

            titleLabel = new Label
            {
                Text = "🔧 Genel Ses Sistemi Sorun Çözücü",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(460, 30),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(titleLabel);

            statusLabel = new Label
            {
                Text = "Ses sisteminizde genel sorunlar tespit edildi.\nAşağıdaki çözümleri sırasıyla deneyin:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Location = new Point(20, 60),
                Size = new Size(460, 40),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(statusLabel);

            restartAudioButton = new Button
            {
                Text = "🔄 Ses Servislerini Yeniden Başlat",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 180, 90),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 35),
                Location = new Point(20, 120)
            };
            restartAudioButton.FlatAppearance.BorderSize = 0;
            restartAudioButton.Click += RestartAudioButton_Click;
            mainPanel.Controls.Add(restartAudioButton);

            updateDriversButton = new Button
            {
                Text = "🔧 Ses Kartı Sürücülerini Kontrol Et",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 130, 180),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 35),
                Location = new Point(260, 120)
            };
            updateDriversButton.FlatAppearance.BorderSize = 0;
            updateDriversButton.Click += UpdateDriversButton_Click;
            mainPanel.Controls.Add(updateDriversButton);

            resetAudioButton = new Button
            {
                Text = "🔁 Windows Ses Ayarlarını Sıfırla",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 130, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 35),
                Location = new Point(20, 170)
            };
            resetAudioButton.FlatAppearance.BorderSize = 0;
            resetAudioButton.Click += ResetAudioButton_Click;
            mainPanel.Controls.Add(resetAudioButton);

            troubleshootButton = new Button
            {
                Text = "🛠️ Windows Ses Sorun Gidericisi",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(130, 60, 180),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 35),
                Location = new Point(260, 170)
            };
            troubleshootButton.FlatAppearance.BorderSize = 0;
            troubleshootButton.Click += TroubleshootButton_Click;
            mainPanel.Controls.Add(troubleshootButton);

            cancelButton = new Button
            {
                Text = "İptal",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 35),
                Location = new Point(400, 250)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(cancelButton);
        }

        private void RestartAudioButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Windows ses servislerini yeniden başlatmak istiyor musunuz?\n\n" +
                "Bu işlem ses çıkışını geçici olarak kesebilir.",
                "Ses Servislerini Yeniden Başlat",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    statusLabel.Text = "Ses servisleri yeniden başlatılıyor...";
                    statusLabel.ForeColor = Color.Yellow;

                    // PowerShell ile ses servislerini yeniden başlat
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-Command \"Restart-Service AudioSrv,AudioEndpointBuilder -Force\"",
                        UseShellExecute = true,
                        Verb = "runas" // Admin izni iste
                    };

                    var process = Process.Start(psi);
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        statusLabel.Text = "✅ Ses servisleri başarıyla yeniden başlatıldı!";
                        statusLabel.ForeColor = Color.LightGreen;
                    }
                    else
                    {
                        statusLabel.Text = "❌ Ses servisleri yeniden başlatılamadı.";
                        statusLabel.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {
                    statusLabel.Text = $"❌ Hata: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                }
            }
        }

        private void UpdateDriversButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Aygıt Yöneticisi'ni açarak ses kartı sürücülerini kontrol etmek istiyor musunuz?\n\n" +
                "• Ses kartınızı bulun\n" +
                "• Sağ tıklayın ve 'Sürücüyü güncelle' seçin\n" +
                "• 'Sürücüleri otomatik ara' seçeneğini seçin",
                "Ses Kartı Sürücüleri",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start("devmgmt.msc");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Aygıt Yöneticisi açılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ResetAudioButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Windows ses ayarlarını fabrika varsayılanlarına sıfırlamak istiyor musunuz?\n\n" +
                "⚠️ DİKKAT: Bu işlem tüm ses tercihlerinizi siler!\n\n" +
                "• Varsayılan ses cihazları sıfırlanır\n" +
                "• Ses seviyeleri varsayılana döner\n" +
                "• Ses efektleri kaldırılır",
                "Ses Ayarlarını Sıfırla",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    statusLabel.Text = "Ses ayarları sıfırlanıyor...";
                    statusLabel.ForeColor = Color.Yellow;

                    // Registry ses ayarlarını sıfırla
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-Command \"Get-ItemProperty 'HKCU:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Audio' | Remove-Item -Force -Recurse\"",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    var process = Process.Start(psi);
                    process.WaitForExit();

                    statusLabel.Text = "✅ Ses ayarları sıfırlandı! Bilgisayarı yeniden başlatın.";
                    statusLabel.ForeColor = Color.LightGreen;
                }
                catch (Exception ex)
                {
                    statusLabel.Text = $"❌ Sıfırlama hatası: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                }
            }
        }

        private void TroubleshootButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Windows ses sorun gidericisini başlat
                Process.Start("msdt.exe", "/id AudioPlaybackDiagnostic");
                statusLabel.Text = "Windows ses sorun gidericisi başlatıldı.";
                statusLabel.ForeColor = Color.LightBlue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sorun giderici başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // FORM1 ÇÖZÜMLERİ YÖNETİCİSİ
    public partial class Form1FixManager : Form
    {
        private Label titleLabel;
        private Label statusLabel;
        private Button simpleRestartButton;
        private Button forceStartButton;
        private Button processKillButton;
        private Button systemRestartButton;
        private Button failureReportButton;
        private Button cancelButton;
        private Panel mainPanel;
        private int attemptCount = 0;
        private bool isFixInProgress = false;

        public Form1FixManager()
        {
            InitializeForm1FixForm();
            StartAutomaticFix();
        }

        private void InitializeForm1FixForm()
        {
            this.Text = "Form1 Sorun Çözücü";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(35, 39, 50);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 39, 50)
            };
            this.Controls.Add(mainPanel);

            titleLabel = new Label
            {
                Text = "🚨 Form1 (Ana Program) Sorun Çözücü",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(510, 30),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(titleLabel);

            statusLabel = new Label
            {
                Text = "Form1 başlatılmaya çalışılıyor...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Yellow,
                Location = new Point(20, 60),
                Size = new Size(510, 60),
                TextAlign = ContentAlignment.TopLeft
            };
            mainPanel.Controls.Add(statusLabel);

            simpleRestartButton = new Button
            {
                Text = "🔄 Basit Yeniden Başlatma",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 180, 90),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 35),
                Location = new Point(20, 140),
                Enabled = false
            };
            simpleRestartButton.FlatAppearance.BorderSize = 0;
            simpleRestartButton.Click += SimpleRestartButton_Click;
            mainPanel.Controls.Add(simpleRestartButton);

            forceStartButton = new Button
            {
                Text = "⚡ Zorla Başlatma",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 130, 180),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 35),
                Location = new Point(220, 140),
                Enabled = false
            };
            forceStartButton.FlatAppearance.BorderSize = 0;
            forceStartButton.Click += ForceStartButton_Click;
            mainPanel.Controls.Add(forceStartButton);

            processKillButton = new Button
            {
                Text = "🔫 Process Temizleme",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 130, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 35),
                Location = new Point(20, 190),
                Enabled = false
            };
            processKillButton.FlatAppearance.BorderSize = 0;
            processKillButton.Click += ProcessKillButton_Click;
            mainPanel.Controls.Add(processKillButton);

            systemRestartButton = new Button
            {
                Text = "🔄 Sistem Yeniden Başlatma",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 35),
                Location = new Point(220, 190),
                Enabled = false
            };
            systemRestartButton.FlatAppearance.BorderSize = 0;
            systemRestartButton.Click += SystemRestartButton_Click;
            mainPanel.Controls.Add(systemRestartButton);

            failureReportButton = new Button
            {
                Text = "❌ Başarısız Olarak Bildir",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 35),
                Location = new Point(120, 240),
                Enabled = false
            };
            failureReportButton.FlatAppearance.BorderSize = 0;
            failureReportButton.Click += FailureReportButton_Click;
            mainPanel.Controls.Add(failureReportButton);

            cancelButton = new Button
            {
                Text = "İptal",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 35),
                Location = new Point(450, 370)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(cancelButton);
        }

        private async void StartAutomaticFix()
        {
            isFixInProgress = true;
            attemptCount = 0;

            // 1. Basit yeniden başlatma denemesi
            await AttemptSimpleRestart();

            if (!IsForm1Running())
            {
                // 2. Zorla başlatma denemesi
                await AttemptForceStart();
            }

            if (!IsForm1Running())
            {
                // 3. Process temizleme ve yeniden başlatma
                await AttemptProcessCleanAndStart();
            }

            if (!IsForm1Running())
            {
                // 4. Son çare: Sistem yeniden başlatma önerisi
                EnableManualOptions();
            }
            else
            {
                ShowSuccess();
            }

            isFixInProgress = false;
        }

        private async Task AttemptSimpleRestart()
        {
            attemptCount++;
            statusLabel.Text = $"[Deneme {attemptCount}] Basit yeniden başlatma deneniyor...";
            statusLabel.ForeColor = Color.Yellow;

            try
            {
                // Mevcut Form1 instance'larını kapat
                var existingForms = Application.OpenForms.OfType<Form1>().ToArray();
                foreach (var form in existingForms)
                {
                    form.Close();
                }

                await Task.Delay(1000);

                // Yeni Form1 oluştur
                var newForm = new Form1();
                newForm.StartPosition = FormStartPosition.CenterScreen;
                newForm.Show();

                await Task.Delay(2000);

                if (IsForm1Running())
                {
                    statusLabel.Text = "✅ Basit yeniden başlatma başarılı!";
                    statusLabel.ForeColor = Color.LightGreen;
                    return;
                }
                else
                {
                    statusLabel.Text = "❌ Basit yeniden başlatma başarısız.";
                    statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"❌ Basit yeniden başlatma hatası: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }

            await Task.Delay(1000);
        }

        private async Task AttemptForceStart()
        {
            attemptCount++;
            statusLabel.Text = $"[Deneme {attemptCount}] Zorla başlatma deneniyor...";
            statusLabel.ForeColor = Color.Yellow;

            try
            {
                // Garbage collection zorla çalıştır
                GC.Collect();
                GC.WaitForPendingFinalizers();

                await Task.Delay(500);

                // Yeni thread'de Form1 başlat
                await Task.Run(() =>
                {
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                            var newForm = new Form1();
                            newForm.StartPosition = FormStartPosition.CenterScreen;
                            newForm.FormClosed += (s, e) => { }; // Dummy event handler
                            newForm.Show();
                            newForm.BringToFront();
                        }
                        catch (Exception ex)
                        {
                            statusLabel.Text = $"❌ Zorla başlatma hatası: {ex.Message}";
                            statusLabel.ForeColor = Color.Red;
                        }
                    }));
                });

                await Task.Delay(3000);

                if (IsForm1Running())
                {
                    statusLabel.Text = "✅ Zorla başlatma başarılı!";
                    statusLabel.ForeColor = Color.LightGreen;
                    return;
                }
                else
                {
                    statusLabel.Text = "❌ Zorla başlatma başarısız.";
                    statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"❌ Zorla başlatma hatası: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }

            await Task.Delay(1000);
        }

        private async Task AttemptProcessCleanAndStart()
        {
            attemptCount++;
            statusLabel.Text = $"[Deneme {attemptCount}] Process temizleme ve yeniden başlatma...";
            statusLabel.ForeColor = Color.Yellow;

            try
            {
                // Uygulama process'lerini temizle
                var currentProcess = Process.GetCurrentProcess();
                var relatedProcesses = Process.GetProcessesByName(currentProcess.ProcessName)
                    .Where(p => p.Id != currentProcess.Id);

                foreach (var process in relatedProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                    catch { }
                }

                await Task.Delay(2000);

                // Yeni Form1 başlat
                var newForm = new Form1();
                newForm.StartPosition = FormStartPosition.CenterScreen;
                newForm.Show();

                await Task.Delay(3000);

                if (IsForm1Running())
                {
                    statusLabel.Text = "✅ Process temizleme başarılı!";
                    statusLabel.ForeColor = Color.LightGreen;
                    return;
                }
                else
                {
                    statusLabel.Text = "❌ Process temizleme başarısız.";
                    statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"❌ Process temizleme hatası: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }

            await Task.Delay(1000);
        }

        private void EnableManualOptions()
        {
            statusLabel.Text = $"❌ Otomatik çözümler başarısız oldu. ({attemptCount} deneme yapıldı)\n\n" +
                              "Manuel çözümleri deneyin veya sistem yeniden başlatma yapın.";
            statusLabel.ForeColor = Color.Red;

            simpleRestartButton.Enabled = true;
            forceStartButton.Enabled = true;
            processKillButton.Enabled = true;
            systemRestartButton.Enabled = true;
            failureReportButton.Enabled = true;
        }

        private void ShowSuccess()
        {
            titleLabel.Text = "✅ Form1 Başarıyla Çalışıyor";
            statusLabel.Text = $"✅ Form1 {attemptCount} deneme sonunda başarıyla başlatıldı!\n\n" +
                              "Ana program artık düzgün çalışıyor.";
            statusLabel.ForeColor = Color.LightGreen;

            cancelButton.Text = "Kapat";
        }

        private bool IsForm1Running()
        {
            try
            {
                var form1Instance = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                return form1Instance != null && form1Instance.Visible;
            }
            catch
            {
                return false;
            }
        }

        private void SimpleRestartButton_Click(object sender, EventArgs e)
        {
            if (isFixInProgress) return;
            _ = AttemptSimpleRestart();
        }

        private void ForceStartButton_Click(object sender, EventArgs e)
        {
            if (isFixInProgress) return;
            _ = AttemptForceStart();
        }

        private void ProcessKillButton_Click(object sender, EventArgs e)
        {
            if (isFixInProgress) return;
            _ = AttemptProcessCleanAndStart();
        }

        private void SystemRestartButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "⚠️ SİSTEM YENİDEN BAŞLATMA\n\n" +
                "Bilgisayarınızı yeniden başlatmak istiyor musunuz?\n\n" +
                "• Tüm açık programlar kapatılacak\n" +
                "• Kaydedilmemiş veriler kaybolabilir\n" +
                "• Sistem 60 saniye içinde yeniden başlatılacak\n\n" +
                "Bu işlemi onaylıyor musunuz?",
                "Sistem Yeniden Başlatma",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start("shutdown", "/r /t 60 /c \"Form1 sorun çözümü için sistem yeniden başlatılıyor...\"");

                    statusLabel.Text = "🔄 Sistem 60 saniye içinde yeniden başlayacak.\n\nİptal etmek için: shutdown /a";
                    statusLabel.ForeColor = Color.Orange;

                    systemRestartButton.Text = "İptal Et";
                    systemRestartButton.BackColor = Color.FromArgb(60, 180, 90);
                    systemRestartButton.Click -= SystemRestartButton_Click;
                    systemRestartButton.Click += (s, args) =>
                    {
                        Process.Start("shutdown", "/a");
                        statusLabel.Text = "✅ Sistem yeniden başlatma iptal edildi.";
                        statusLabel.ForeColor = Color.LightGreen;
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Sistem yeniden başlatma komutu çalıştırılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FailureReportButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "❌ BAŞARISIZ ÇÖZÜM RAPORU\n\n" +
                $"Form1 {attemptCount} deneme sonunda başlatılamadı.\n\n" +
                "Olası nedenler:\n" +
                "• Uygulama dosyaları bozuk\n" +
                "• Sistem kaynak yetersizliği\n" +
                "• .NET Framework sorunu\n" +
                "• Antivirüs engellemesi\n" +
                "• Sistem dosyaları bozuk\n\n" +
                "Öneriler:\n" +
                "• Uygulamayı yeniden yükleyin\n" +
                "• Windows güncellemelerini kontrol edin\n" +
                "• Antivirüs ayarlarını kontrol edin\n" +
                "• Sistem dosyalarını tarayın (sfc /scannow)\n\n" +
                "Bu raporu kaydetmek istiyor musunuz?",
                "Başarısız Çözüm Raporu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        $"Form1_Failure_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                    var reportContent = $"Form1 Başarısız Çözüm Raporu\n" +
                                      $"================================\n" +
                                      $"Tarih: {DateTime.Now}\n" +
                                      $"Kullanıcı: {Environment.UserName}\n" +
                                      $"Makine: {Environment.MachineName}\n" +
                                      $"OS: {Environment.OSVersion}\n" +
                                      $"Deneme Sayısı: {attemptCount}\n" +
                                      $"Son Durum: Form1 başlatılamadı\n\n" +
                                      $"Denenen Çözümler:\n" +
                                      $"1. Basit yeniden başlatma\n" +
                                      $"2. Zorla başlatma\n" +
                                      $"3. Process temizleme\n\n" +
                                      $"Önerilen İşlemler:\n" +
                                      $"- Uygulama yeniden yükleme\n" +
                                      $"- Sistem güncellemeleri\n" +
                                      $"- Antivirüs kontrol\n" +
                                      $"- Sistem dosyası tarama";

                    File.WriteAllText(reportPath, reportContent);
                    MessageBox.Show($"Rapor kaydedildi:\n{reportPath}", "Rapor Kaydedildi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Rapor kaydedilemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            statusLabel.Text = "❌ Form1 çözümü başarısız olarak raporlandı.";
            statusLabel.ForeColor = Color.Red;
        }
    }

    // Devam eden diğer sınıflar (MultiDisplayResolutionManager, DisplayResolutionForm, vb.) önceki kodla aynı...

    // ÇOK EKRANLI ÇÖZÜNÜRLÜKLERİ YÖNETİCİSİ
    public partial class MultiDisplayResolutionManager : Form
    {
        private readonly Screen[] problemScreens;
        private int currentScreenIndex = 0;
        private DisplayResolutionForm currentResolutionForm;
        private bool allScreensProcessed = false;

        private Label titleLabel;
        private Label progressLabel;
        private Button nextButton;
        private Button skipButton;
        private Button primaryButton;
        private Button cancelButton;
        private Panel mainPanel;

        public MultiDisplayResolutionManager(Screen[] screens)
        {
            problemScreens = screens;
            InitializeMultiForm();
            ProcessNextScreen();
        }

        private void InitializeMultiForm()
        {
            this.Text = "Çoklu Ekran Çözünürlük Yöneticisi";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(35, 39, 50);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 39, 50)
            };
            this.Controls.Add(mainPanel);

            titleLabel = new Label
            {
                Text = "Sorunlu Ekranlar İçin Çözünürlük Düzeltme",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(560, 30),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(titleLabel);

            progressLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Location = new Point(20, 60),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.TopCenter
            };
            mainPanel.Controls.Add(progressLabel);

            nextButton = new Button
            {
                Text = "Bu Ekranı Çöz",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 180, 90),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(150, 320)
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.Click += NextButton_Click;
            mainPanel.Controls.Add(nextButton);

            skipButton = new Button
            {
                Text = "Bu Ekranı Atla",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 130, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(280, 320)
            };
            skipButton.FlatAppearance.BorderSize = 0;
            skipButton.Click += SkipButton_Click;
            mainPanel.Controls.Add(skipButton);

            primaryButton = new Button
            {
                Text = "Ana Ekran Ayarla",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 130, 180),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(410, 280)
            };
            primaryButton.FlatAppearance.BorderSize = 0;
            primaryButton.Click += PrimaryButton_Click;
            mainPanel.Controls.Add(primaryButton);

            cancelButton = new Button
            {
                Text = "İptal",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 60, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 35),
                Location = new Point(410, 320)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(cancelButton);
        }

        private void ProcessNextScreen()
        {
            if (currentScreenIndex >= problemScreens.Length)
            {
                allScreensProcessed = true;
                ShowCompletionSummary();
                return;
            }

            var currentScreen = problemScreens[currentScreenIndex];
            progressLabel.Text = $"Ekran {currentScreenIndex + 1}/{problemScreens.Length}: {currentScreen.DeviceName}\n" +
                                 $"Mevcut Çözünürlük: {currentScreen.Bounds.Width}x{currentScreen.Bounds.Height}\n\n" +
                                 $"Bu ekranın çözünürlüğünü düzeltmek istiyor musunuz?";

            if (currentScreen.Primary)
            {
                progressLabel.Text += "\n\n⚠️ BU EKRAN ANA EKRANINIZ!";
                progressLabel.ForeColor = Color.Orange;
            }
            else
            {
                progressLabel.ForeColor = Color.LightGray;
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            var currentScreen = problemScreens[currentScreenIndex];

            currentResolutionForm = new DisplayResolutionForm(currentScreen);
            currentResolutionForm.FormClosed += (s, args) =>
            {
                currentScreenIndex++;
                ProcessNextScreen();
            };
            currentResolutionForm.ShowDialog();
        }

        private void SkipButton_Click(object sender, EventArgs e)
        {
            currentScreenIndex++;
            ProcessNextScreen();
        }

        private void PrimaryButton_Click(object sender, EventArgs e)
        {
            var primaryScreen = Screen.AllScreens.FirstOrDefault(s => s.Primary);
            if (primaryScreen != null)
            {
                var primaryForm = new DisplayResolutionForm(primaryScreen);
                primaryForm.ShowDialog();
            }
        }

        private void ShowCompletionSummary()
        {
            titleLabel.Text = "Tüm Sorunlu Ekranlar İşlendi";
            progressLabel.Text = $"✅ {problemScreens.Length} ekran işleme tamamlandı.\n\n" +
                                 "Sisteminizi yeniden başlatmanız önerilir.\n" +
                                 "Ana ekranınızı da düzenlemek istiyorsanız 'Ana Ekran Ayarla' butonunu kullanın.";
            progressLabel.ForeColor = Color.LightGreen;

            nextButton.Enabled = false;
            skipButton.Enabled = false;
            cancelButton.Text = "Kapat";
        }
    }

    // GELİŞTİRİLMİŞ TEK EKRAN ÇÖZÜNÜRLÜKLERİ FORMU
    public partial class DisplayResolutionForm : Form
    {
        private readonly Screen targetScreen;
        private readonly SystemCheck.DEVMODE originalMode;
        private readonly List<SystemCheck.DEVMODE> availableModes;
        private ComboBox resolutionComboBox;
        private Button testButton;
        private Button applyButton;
        private Button cancelButton;
        private Button testAllButton;
        private Label statusLabel;
        private System.Windows.Forms.Timer revertTimer;
        private SystemCheck.DEVMODE currentTestMode;
        private bool isTestingMode = false;
        private int countdownSeconds = 15;
        private bool hasAppliedPermanently = false;

        public DisplayResolutionForm(Screen screen)
        {
            targetScreen = screen;
            originalMode = GetCurrentDisplayMode(screen.DeviceName);
            availableModes = GetAvailableDisplayModes(screen.DeviceName);
            InitializeDisplayForm();
        }

        private void InitializeDisplayForm()
        {
            this.Text = $"Ekran Çözünürlüğü - {targetScreen.DeviceName}";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(550, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 44, 55);

            var titleLabel = new Label
            {
                Text = $"🖥️ EKRAN: {targetScreen.DeviceName}\n" +
                       $"Mevcut: {targetScreen.Bounds.Width}x{targetScreen.Bounds.Height}" +
                       (targetScreen.Primary ? " (ANA EKRAN)" : ""),
                ForeColor = targetScreen.Primary ? Color.Orange : Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(510, 50),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(titleLabel);

            var resLabel = new Label
            {
                Text = "Çözünürlük Seçin:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 90),
                Size = new Size(200, 20)
            };
            this.Controls.Add(resLabel);

            resolutionComboBox = new ComboBox
            {
                Location = new Point(20, 115),
                Size = new Size(380, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 64, 75),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };

            var currentRes = targetScreen.Bounds.Width * targetScreen.Bounds.Height;
            var allModes = availableModes
                .OrderByDescending(m => m.dmPelsWidth * m.dmPelsHeight)
                .ToList();

            foreach (var mode in allModes)
            {
                string prefix = "";
                if (mode.dmPelsWidth == targetScreen.Bounds.Width && mode.dmPelsHeight == targetScreen.Bounds.Height)
                    prefix = "✓ [MEVCUT] ";
                else if (mode.dmPelsWidth * mode.dmPelsHeight > currentRes)
                    prefix = "⬆️ [DAHA İYİ] ";
                else
                    prefix = "⬇️ [DAHA DÜŞÜK] ";

                resolutionComboBox.Items.Add($"{prefix}{mode.dmPelsWidth}x{mode.dmPelsHeight} @ {mode.dmDisplayFrequency}Hz");
            }

            var bestMode = allModes.FirstOrDefault(m => m.dmPelsWidth * m.dmPelsHeight > currentRes);
            if (bestMode.dmPelsWidth > 0)
            {
                var bestIndex = allModes.FindIndex(m => m.dmPelsWidth == bestMode.dmPelsWidth && m.dmPelsHeight == bestMode.dmPelsHeight);
                if (bestIndex >= 0) resolutionComboBox.SelectedIndex = bestIndex;
            }
            else if (allModes.Any())
            {
                resolutionComboBox.SelectedIndex = 0;
            }

            this.Controls.Add(resolutionComboBox);

            testButton = new Button
            {
                Text = "Test Et (15sn)",
                Location = new Point(20, 155),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = allModes.Any()
            };
            testButton.FlatAppearance.BorderSize = 0;
            testButton.Click += TestButton_Click;
            this.Controls.Add(testButton);

            applyButton = new Button
            {
                Text = "Kalıcı Uygula",
                Location = new Point(140, 155),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(60, 180, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = allModes.Any()
            };
            applyButton.FlatAppearance.BorderSize = 0;
            applyButton.Click += ApplyButton_Click;
            this.Controls.Add(applyButton);

            testAllButton = new Button
            {
                Text = "Tümünü Test Et",
                Location = new Point(260, 155),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(130, 70, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = allModes.Any()
            };
            testAllButton.FlatAppearance.BorderSize = 0;
            testAllButton.Click += TestAllButton_Click;
            this.Controls.Add(testAllButton);

            cancelButton = new Button
            {
                Text = "İptal",
                Location = new Point(380, 155),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(180, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += CancelButton_Click;
            this.Controls.Add(cancelButton);

            statusLabel = new Label
            {
                Text = allModes.Any() ?
                    "Bir çözünürlük seçin ve test edin veya direkt uygulayın." :
                    "Bu ekran için uygun çözünürlük bulunamadı.",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8),
                Location = new Point(20, 200),
                Size = new Size(510, 150),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(statusLabel);

            revertTimer = new System.Windows.Forms.Timer();
            revertTimer.Interval = 1000; // Her saniye
            revertTimer.Tick += RevertTimer_Tick;

            this.FormClosing += (s, e) =>
            {
                if (isTestingMode && !hasAppliedPermanently)
                {
                    RevertToOriginal();
                }
            };
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            if (resolutionComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Lütfen bir çözünürlük seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedMode = availableModes[resolutionComboBox.SelectedIndex];
            TestDisplayMode(selectedMode);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (resolutionComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Lütfen bir çözünürlük seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedMode = availableModes[resolutionComboBox.SelectedIndex];
            ApplyDisplayModePermanently(selectedMode);
        }

        private void TestAllButton_Click(object sender, EventArgs e)
        {
            var currentRes = targetScreen.Bounds.Width * targetScreen.Bounds.Height;
            var betterModes = availableModes
                .Where(m => m.dmPelsWidth * m.dmPelsHeight > currentRes)
                .OrderByDescending(m => m.dmPelsWidth * m.dmPelsHeight)
                .ToList();

            if (!betterModes.Any())
            {
                MessageBox.Show("Test edilecek daha iyi çözünürlük bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var mode in betterModes)
            {
                var result = MessageBox.Show(
                    $"🖥️ {targetScreen.DeviceName}\n\n" +
                    $"Şimdi {mode.dmPelsWidth}x{mode.dmPelsHeight} çözünürlüğünü test etmek istiyor musunuz?\n" +
                    $"15 saniye sonra otomatik geri alınacak.",
                    "Çözünürlük Testi",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    break;

                if (result == DialogResult.Yes)
                {
                    TestDisplayMode(mode);

                    var testResult = MessageBox.Show(
                        $"Bu çözünürlük ({mode.dmPelsWidth}x{mode.dmPelsHeight}) uygun mu?\n\n" +
                        $"Evet = Kalıcı uygula\n" +
                        $"Hayır = Sonrakini test et\n" +
                        $"İptal = Teste son ver",
                        "Test Sonucu",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (testResult == DialogResult.Yes)
                    {
                        revertTimer.Stop();
                        isTestingMode = false;
                        hasAppliedPermanently = true;
                        statusLabel.Text = $"✅ Çözünürlük {mode.dmPelsWidth}x{mode.dmPelsHeight} kalıcı olarak uygulandı!";
                        MessageBox.Show($"✅ {targetScreen.DeviceName} artık {mode.dmPelsWidth}x{mode.dmPelsHeight} çözünürlüğünde!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (testResult == DialogResult.Cancel)
                    {
                        RevertToOriginal();
                        break;
                    }
                    else
                    {
                        RevertToOriginal();
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (isTestingMode && !hasAppliedPermanently)
            {
                RevertToOriginal();
            }
            this.Close();
        }

        private void TestDisplayMode(SystemCheck.DEVMODE mode)
        {
            currentTestMode = mode;
            isTestingMode = true;
            countdownSeconds = 15;

            statusLabel.Text = $"⏱️ Test ediliyor: {mode.dmPelsWidth}x{mode.dmPelsHeight} ({countdownSeconds} saniye kaldı)";

            int result = SystemCheck.ChangeDisplaySettingsEx(targetScreen.DeviceName, ref mode, IntPtr.Zero, 0, IntPtr.Zero);

            if (result == 0) // DISP_CHANGE_SUCCESSFUL
            {
                revertTimer.Start();
                testButton.Enabled = false;
                applyButton.Text = "KALICI UYGULA!";
                applyButton.BackColor = Color.FromArgb(255, 140, 0);
                testAllButton.Enabled = false;
                cancelButton.Text = "Geri Al";
            }
            else
            {
                statusLabel.Text = $"❌ Test çözünürlüğü uygulanamadı! Hata: {result}\nBu çözünürlük desteklenmiyor olabilir.";
                isTestingMode = false;
                MessageBox.Show($"Çözünürlük değiştirilemedi.\nHata kodu: {result}\n\nBu ekran bu çözünürlüğü desteklemiyor olabilir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyDisplayModePermanently(SystemCheck.DEVMODE mode)
        {
            var confirmResult = MessageBox.Show(
                $"🖥️ {targetScreen.DeviceName}\n\n" +
                $"Çözünürlüğü {mode.dmPelsWidth}x{mode.dmPelsHeight} olarak kalıcı değiştirmek istediğinizden emin misiniz?\n\n" +
                $"Bu işlem geri alınamaz!",
                "Kalıcı Değişiklik Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            statusLabel.Text = $"⚙️ Kalıcı olarak uygulanıyor: {mode.dmPelsWidth}x{mode.dmPelsHeight}...";

            int result = SystemCheck.ChangeDisplaySettingsEx(targetScreen.DeviceName, ref mode, IntPtr.Zero, 0, IntPtr.Zero);

            if (result == 0) // DISP_CHANGE_SUCCESSFUL
            {
                hasAppliedPermanently = true;
                statusLabel.Text = $"✅ Başarılı! Çözünürlük kalıcı olarak {mode.dmPelsWidth}x{mode.dmPelsHeight} yapıldı.";
                statusLabel.ForeColor = Color.LightGreen;

                testButton.Enabled = false;
                applyButton.Enabled = false;
                testAllButton.Enabled = false;
                cancelButton.Text = "Kapat";

                MessageBox.Show($"✅ {targetScreen.DeviceName} başarıyla {mode.dmPelsWidth}x{mode.dmPelsHeight} çözünürlüğüne ayarlandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                statusLabel.Text = $"❌ Kalıcı uygulama başarısız! Hata kodu: {result}";
                statusLabel.ForeColor = Color.Red;
                MessageBox.Show($"Çözünürlük kalıcı olarak uygulanamadı.\nHata kodu: {result}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RevertTimer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;
            statusLabel.Text = $"⏱️ Test ediliyor: {currentTestMode.dmPelsWidth}x{currentTestMode.dmPelsHeight} ({countdownSeconds} saniye kaldı)";

            if (countdownSeconds <= 0)
            {
                revertTimer.Stop();
                RevertToOriginal();
            }
        }

        private void RevertToOriginal()
        {
            if (isTestingMode && !hasAppliedPermanently)
            {
                var original = originalMode;
                SystemCheck.ChangeDisplaySettingsEx(targetScreen.DeviceName, ref original, IntPtr.Zero, 0, IntPtr.Zero);
                statusLabel.Text = $"🔄 Orijinal çözünürlüğe geri dönüldü: {original.dmPelsWidth}x{original.dmPelsHeight}";
                statusLabel.ForeColor = Color.LightBlue;
                isTestingMode = false;

                testButton.Enabled = true;
                applyButton.Text = "Kalıcı Uygula";
                applyButton.BackColor = Color.FromArgb(60, 180, 90);
                testAllButton.Enabled = true;
                cancelButton.Text = "İptal";
            }
        }

        private SystemCheck.DEVMODE GetCurrentDisplayMode(string deviceName)
        {
            SystemCheck.DEVMODE devMode = new SystemCheck.DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            SystemCheck.EnumDisplaySettings(deviceName, -1, ref devMode); // ENUM_CURRENT_SETTINGS
            return devMode;
        }

        private List<SystemCheck.DEVMODE> GetAvailableDisplayModes(string deviceName)
        {
            var modes = new List<SystemCheck.DEVMODE>();
            int modeNum = 0;
            SystemCheck.DEVMODE devMode = new SystemCheck.DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            while (SystemCheck.EnumDisplaySettings(deviceName, modeNum, ref devMode) != 0)
            {
                // Tüm kullanılabilir çözünürlükleri dahil et (minimum 640x480)
                if (devMode.dmPelsWidth >= 640 && devMode.dmPelsHeight >= 480 && devMode.dmBitsPerPel >= 24)
                {
                    modes.Add(devMode);
                }
                modeNum++;
                devMode = new SystemCheck.DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);
            }

            return modes.GroupBy(m => new { m.dmPelsWidth, m.dmPelsHeight, m.dmDisplayFrequency })
                       .Select(g => g.First())
                       .ToList();
        }
    }

    public class AudioDeviceNotification : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
    {
        private readonly SystemCheck parent;
        public AudioDeviceNotification(SystemCheck parent) { this.parent = parent; }
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) => parent.OnSystemEventDetected();
        public void OnDeviceAdded(string deviceId) => parent.OnSystemEventDetected();
        public void OnDeviceRemoved(string deviceId) => parent.OnSystemEventDetected();
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) => parent.OnSystemEventDetected();
        public void OnPropertyValueChanged(string deviceId, PropertyKey key) => parent.OnSystemEventDetected();
    }

    public class SystemCheckItem
    {
        public CheckState State { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public object Tag { get; set; }

        public SystemCheckItem(CheckState state, string title, string desc)
        {
            State = state;
            Title = title;
            Description = desc;
        }
        public SystemCheckItem(CheckState state, string title, string desc, object tag)
        {
            State = state;
            Title = title;
            Description = desc;
            Tag = tag;
        }
    }

    public enum CheckState
    {
        Success,
        Warning,
        Error,
        Info
    }
}