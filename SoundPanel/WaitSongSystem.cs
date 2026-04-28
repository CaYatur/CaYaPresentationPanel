using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Lame;

namespace SoundPanel
{
    public partial class WaitSongSystem : Form
    {
        private CancellationTokenSource cancellationTokenSource;
        private IWavePlayer wavePlayer;
        private AudioFileReader audioFileReader;
        private List<string> songPaths = new List<string>();
        private int currentIndex = 0;
        private float volume = 1.0f; // 0.0 - 1.0 arası
        private bool isPlaying = false;
        private bool isUpdating = false;
        private bool isDraggingSeek = false;
        private bool isChangingSong = false;
        private readonly object playerLock = new object();
        private readonly object songChangeLock = new object();

        public WaitSongSystem()
        {
            InitializeComponent();
            LoadSongsFromFolder();

            // NAudio player başlat
            InitializeAudioPlayer();
        }

        private void InitializeAudioPlayer()
        {
            try
            {
                // WaveOut kullanarak ses kartı üzerinden çal
                wavePlayer = new WaveOutEvent();
                wavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audio player başlatma hatası: {ex.Message}");
            }
        }

        private void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                // Şarkı bitti - otomatik geçiş
                if (isPlaying && !isChangingSong)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (songPaths.Count > 0)
                            {
                                currentIndex = (currentIndex + 1) % songPaths.Count;
                                await PlaySongAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Otomatik geçiş hatası: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PlaybackStopped hatası: {ex.Message}");
            }
        }

        private async void WaitSongSystem_Load(object sender, EventArgs e)
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                isUpdating = true;

                _ = Task.Run(() => UpdatePlaybackDetailsAsync(cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Form load hatası: {ex.Message}");
            }
        }

        private void LoadSongsFromFolder()
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "wait");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                songPaths = Directory.GetFiles(folderPath, "*.*")
                    .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".flac", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateSongList()));
                }
                else
                {
                    UpdateSongList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şarkı yükleme hatası: {ex.Message}");
            }
        }

        private void UpdateSongList()
        {
            lstSongs.Items.Clear();
            foreach (var song in songPaths)
                lstSongs.Items.Add(Path.GetFileName(song));
        }

        // NAudio ile şarkı çalma - PERFORMANSLI VE STABIL
        private async Task PlaySongAsync()
        {
            if (isChangingSong) return;

            try
            {
                lock (songChangeLock)
                {
                    if (isChangingSong) return;
                    isChangingSong = true;
                }

                if (isPlaying && songPaths.Count > 0 && currentIndex >= 0 && currentIndex < songPaths.Count)
                {
                    string songPath = songPaths[currentIndex];
                    string songName = Path.GetFileName(songPath);

                    // UI güncelle
                    BeginInvoke(new Action(() =>
                    {
                        lblNowPlaying.Text = $"🔄 Yükleniyor: {songName}";
                        if (currentIndex < lstSongs.Items.Count)
                        {
                            lstSongs.SelectedIndex = currentIndex;
                        }
                    }));

                    // Audio işlemlerini arka planda yap
                    await Task.Run(() =>
                    {
                        try
                        {
                            lock (playerLock)
                            {
                                // Mevcut çalmayı durdur
                                wavePlayer?.Stop();
                                audioFileReader?.Dispose();

                                // Yeni dosyayı yükle
                                audioFileReader = new AudioFileReader(songPath);
                                audioFileReader.Volume = volume;

                                // Player'a bağla ve başlat
                                wavePlayer.Init(audioFileReader);
                                wavePlayer.Play();

                                Console.WriteLine($"NAudio ile çalıyor: {songName}");
                            }

                            // UI güncelle
                            BeginInvoke(new Action(() =>
                            {
                                lblNowPlaying.Text = $"🎵 Şuanda Oynatılan: {songName}";
                            }));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"NAudio çalma hatası: {ex.Message}");

                            BeginInvoke(new Action(() =>
                            {
                                lblNowPlaying.Text = $"❌ Hata: {songName}";
                            }));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PlaySongAsync hatası: {ex.Message}");
            }
            finally
            {
                lock (songChangeLock)
                {
                    isChangingSong = false;
                }
            }
        }

        private async void btnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                if (songPaths.Count == 0)
                {
                    MessageBox.Show("Oynatılacak şarkı bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                isPlaying = true;
                await PlaySongAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Play button hatası: {ex.Message}");
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        lock (playerLock)
                        {
                            if (wavePlayer.PlaybackState == PlaybackState.Playing)
                            {
                                wavePlayer.Pause();
                                BeginInvoke(new Action(() =>
                                {
                                    lblNowPlaying.Text = "⏸ Şarkı Duraklatıldı";
                                }));
                            }
                            else if (wavePlayer.PlaybackState == PlaybackState.Paused)
                            {
                                wavePlayer.Play();
                                BeginInvoke(new Action(() =>
                                {
                                    lblNowPlaying.Text = $"🎵 Devam Ediyor: {(currentIndex < songPaths.Count ? Path.GetFileName(songPaths[currentIndex]) : "Bilinmeyen")}";
                                }));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Pause hatası: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pause button hatası: {ex.Message}");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        lock (playerLock)
                        {
                            wavePlayer?.Stop();
                            isPlaying = false;
                        }

                        BeginInvoke(new Action(() =>
                        {
                            lblNowPlaying.Text = "⏹ Şarkı Durduruldu";
                        }));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Stop hatası: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stop button hatası: {ex.Message}");
            }
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (songPaths.Count > 0 && !isChangingSong)
                {
                    currentIndex = (currentIndex + 1) % songPaths.Count;
                    if (isPlaying)
                    {
                        _ = Task.Run(async () => await PlaySongAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Next button hatası: {ex.Message}");
            }
        }

        private async void btnPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                if (songPaths.Count > 0 && !isChangingSong)
                {
                    currentIndex = (currentIndex - 1 + songPaths.Count) % songPaths.Count;
                    if (isPlaying)
                    {
                        _ = Task.Run(async () => await PlaySongAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Previous button hatası: {ex.Message}");
            }
        }

        private async Task UpdatePlaybackDetailsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && isUpdating)
            {
                try
                {
                    await Task.Delay(500, token);

                    if (isChangingSong) continue;

                    if (!token.IsCancellationRequested && !IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    if (!isChangingSong && audioFileReader != null)
                                    {
                                        lock (playerLock)
                                        {
                                            var currentTime = audioFileReader.CurrentTime;
                                            var totalTime = audioFileReader.TotalTime;

                                            lblCurrentTime.Text = currentTime.ToString(@"mm\:ss");
                                            lblTotalTime.Text = totalTime.ToString(@"mm\:ss");

                                            if (!isDraggingSeek && totalTime.TotalSeconds > 0)
                                            {
                                                trackBarSeek.Maximum = (int)totalTime.TotalSeconds;
                                                trackBarSeek.Value = Math.Min((int)currentTime.TotalSeconds, trackBarSeek.Maximum);
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }));
                        }
                        catch (InvalidOperationException)
                        {
                            break;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Update loop hatası: {ex.Message}");
                }
            }
        }

        private void trackBarVolume_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        volume = trackBarVolume.Value / 100.0f; // 0.0 - 1.0 arası

                        lock (playerLock)
                        {
                            if (audioFileReader != null)
                            {
                                audioFileReader.Volume = volume;
                            }
                        }

                        if (!IsDisposed)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                Volume.Text = $"🔊 Ses Düzeyi: {trackBarVolume.Value}%";
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Volume değişiklik hatası: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TrackBar volume hatası: {ex.Message}");
            }
        }

        private void trackBarSeek_ScrollStart(object sender, EventArgs e)
        {
            isDraggingSeek = true;
        }

        private void trackBarSeek_ScrollEnd(object sender, EventArgs e)
        {
            try
            {
                isDraggingSeek = false;

                _ = Task.Run(() =>
                {
                    try
                    {
                        lock (playerLock)
                        {
                            if (audioFileReader != null)
                            {
                                audioFileReader.CurrentTime = TimeSpan.FromSeconds(trackBarSeek.Value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Seek hatası: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TrackBar seek hatası: {ex.Message}");
            }
        }

        private async void lstSongs_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (lstSongs.SelectedIndex >= 0 && lstSongs.SelectedIndex < songPaths.Count && !isChangingSong)
                {
                    isPlaying = true;
                    currentIndex = lstSongs.SelectedIndex;
                    _ = Task.Run(async () => await PlaySongAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Liste double click hatası: {ex.Message}");
            }
        }

        private void btnAddSongs_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Audio Files|*.mp3;*.wav;*.flac;*.m4a|MP3 Files|*.mp3|WAV Files|*.wav|FLAC Files|*.flac|M4A Files|*.m4a";
                    ofd.Multiselect = true;
                    ofd.Title = "Müzik Dosyalarını Seçin";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "wait");

                        _ = Task.Run(() =>
                        {
                            try
                            {
                                foreach (var file in ofd.FileNames)
                                {
                                    string destPath = Path.Combine(folderPath, Path.GetFileName(file));
                                    if (!File.Exists(destPath))
                                    {
                                        File.Copy(file, destPath, true);
                                    }
                                }

                                Invoke(new Action(() => LoadSongsFromFolder()));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Dosya kopyalama hatası: {ex.Message}");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şarkı ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void WaitSongSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                isUpdating = false;
                isPlaying = false;
                isChangingSong = true;

                cancellationTokenSource?.Cancel();

                _ = Task.Run(() =>
                {
                    try
                    {
                        lock (playerLock)
                        {
                            wavePlayer?.Stop();
                            wavePlayer?.Dispose();
                            audioFileReader?.Dispose();
                        }
                    }
                    catch { }
                });

                lblNowPlaying.Text = "⏹ Sistem Kapatılıyor...";

                await Task.Delay(100);

                cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Form kapatma hatası: {ex.Message}");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "wait");

                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    MessageBox.Show("Klasör yolu geçersiz veya klasör mevcut değil.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klasör açılırken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                LoadSongsFromFolder();
                MessageBox.Show("Liste başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Liste güncelleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}