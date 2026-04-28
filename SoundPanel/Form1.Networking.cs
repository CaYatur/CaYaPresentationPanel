using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SoundPanel
{
    /// <summary>
    /// Form1 için uzaktan kontrol uyumluluğu sağlayan partial sınıf
    /// RemotePlaybackManager tarafından erişilecek public metodlar içerir
    /// </summary>
    public partial class Form1 : Form
    {
        #region Playback Control Methods

        /// <summary>
        /// Belirtilen şarkı yoluna sahip şarkıyı oynatmaya başlar
        /// </summary>
        public void RemotePlaySong(string songPath)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    // Panel ve RichTextBox'ı bul
                    var panel = FindSongPanel(songPath);
                    var rtb = panel?.Controls.OfType<RichTextBox>().FirstOrDefault();

                    if (panel != null)
                    {
                        PlaySong(songPath, panel, rtb);
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemotePlaySong Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Şu anda çalan şarkıyı duraklatır
        /// </summary>
        public void RemotePauseSong()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    if (waveOutEvent?.PlaybackState == PlaybackState.Playing)
                    {
                        waveOutEvent.Pause();
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemotePauseSong Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Duraklatılmış şarkıyı oynatmaya devam ettirir
        /// </summary>
        public void RemoteResumeSong()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    if (waveOutEvent?.PlaybackState == PlaybackState.Paused)
                    {
                        waveOutEvent.Play();
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoteResumeSong Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Şu anda çalan şarkıyı durdurur
        /// </summary>
        public void RemoteStopSong()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    if (waveOutEvent != null)
                    {
                        waveOutEvent.Stop();
                        playbackTimer?.Stop();
                        lblCurrentTime.Text = "00:00";
                        trackBarProgress.Value = 0;
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoteStopSong Error: {ex.Message}");
            }
        }

        #endregion

        #region Volume Control Methods

        /// <summary>
        /// Genel ses düzeyini (0-100) ayarlar - RemotePlaybackManager için
        /// </summary>
        public void RemoteSetVolume(int volumePercent)
        {
            try
            {
                int clampedVolume = Math.Max(0, Math.Min(100, volumePercent));
                this.Invoke(new Action(() =>
                {
                    if (trackSVolume != null)
                    {
                        trackSVolume.Value = clampedVolume;
                        label1.Text = $"🔊 Ses Düzeyi: %{clampedVolume}";

                        // Ana oynatıcı güncellemesi
                        UpdateMainPlayerVolume();

                        // Ek oynatıcıları güncelle
                        foreach (var songPath in additionalReaders2.Keys.ToList())
                        {
                            UpdateAdditionalPlayerVolume2(songPath);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoteSetVolume Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mevcut ses düzeyini (0-100) döndürür - RemotePlaybackManager için
        /// </summary>
        public int RemoteGetVolume()
        {
            try
            {
                int result = 50;
                this.Invoke(new Action(() =>
                {
                    if (trackSVolume != null)
                    {
                        result = trackSVolume.Value;
                    }
                }));
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoteGetVolume Error: {ex.Message}");
                return 50;
            }
        }

        /// <summary>
        /// Sunucudan gelen ses düzeyini ayarlar - VolumeServerForm uyumluluğu için
        /// </summary>
        public void SetVolumeFromServer(int volumePercent)
        {
            try
            {
                int clampedVolume = Math.Max(0, Math.Min(100, volumePercent));
                this.Invoke(new Action(() =>
                {
                    if (trackSVolume != null)
                    {
                        trackSVolume.Value = clampedVolume;
                        label1.Text = $"🔊 Ses Düzeyi: %{clampedVolume}";

                        // Ana oynatıcı güncellemesi
                        UpdateMainPlayerVolume();

                        // Ek oynatıcıları güncelle
                        foreach (var songPath in additionalReaders2.Keys.ToList())
                        {
                            UpdateAdditionalPlayerVolume2(songPath);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetVolumeFromServer Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mevcut ses düzeyini (0.0-1.0) döndürür - VolumeServerForm uyumluluğu için
        /// </summary>
        public float GetCurrentVolume()
        {
            try
            {
                float result = 0.5f;
                this.Invoke(new Action(() =>
                {
                    if (trackSVolume != null)
                    {
                        result = trackSVolume.Value / 100f;
                    }
                }));
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentVolume Error: {ex.Message}");
                return 0.5f;
            }
        }

        #endregion

        #region Status Methods

        /// <summary>
        /// Oynatıcının mevcut durumunu döndürür
        /// </summary>
        public PlaybackState RemoteGetPlaybackState()
        {
            try
            {
                return waveOutEvent?.PlaybackState ?? PlaybackState.Stopped;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoteGetPlaybackState Error: {ex.Message}");
                return PlaybackState.Stopped;
            }
        }

        /// <summary>
        /// Şu anda çalan şarkının yolunu döndürür
        /// </summary>
        public string RemoteGetCurrentSongPath()
        {
            return currentSongPath ?? "";
        }

        /// <summary>
        /// Şu anda çalan şarkının adını döndürür
        /// </summary>
        public string RemoteGetCurrentSongName()
        {
            try
            {
                if (string.IsNullOrEmpty(currentSongPath))
                    return "";

                var song = songList.FirstOrDefault(s => s.Path == currentSongPath);
                return song?.Name ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Şu anda çalan şarkının açıklamasını döndürür
        /// </summary>
        public string RemoteGetCurrentSongDescription()
        {
            try
            {
                if (string.IsNullOrEmpty(currentSongPath))
                    return "";

                var song = songList.FirstOrDefault(s => s.Path == currentSongPath);
                return song?.Description ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Tüm şarkıları döndürür
        /// </summary>
        public List<Song> RemoteGetAllSongs()
        {
            return new List<Song>(songList);
        }

        /// <summary>
        /// Döngü modunun durumunu döndürür
        /// </summary>
        public bool RemoteGetLoopingState()
        {
            return isLooping;
        }

        /// <summary>
        /// Döngü modunu açıp kapatır
        /// </summary>
        public void RemoteToggleLooping()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    isLooping = !isLooping;
                    customSwitch1.Checked = isLooping;
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoteToggleLooping Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mevcut oynatma konumunu saniye cinsinden döndürür
        /// </summary>
        public long RemoteGetCurrentPosition()
        {
            try
            {
                return audioFileReader?.Position ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Şarkının toplam süresini saniye cinsinden döndürür
        /// </summary>
        public long RemoteGetTotalDuration()
        {
            try
            {
                return audioFileReader?.Length ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Belirtilen şarkı yoluna sahip paneli bulur
        /// </summary>
        private Panel FindSongPanel(string songPath)
        {
            try
            {
                foreach (Control ctrl in flowLayoutPanelSongs.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        var playBtn = panel.Controls.OfType<CustomButton>()
                            .FirstOrDefault(b => b.Tag is Song s && s.Path == songPath);

                        if (playBtn != null)
                        {
                            return panel;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Bir şarkının bilgisini yol ile elde eder
        /// </summary>
        public Song RemoteGetSongByPath(string songPath)
        {
            try
            {
                return songList.FirstOrDefault(s => s.Path == songPath);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}