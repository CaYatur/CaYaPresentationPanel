using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundPanel
{
    public class VolumeInfo
    {
        public float Minimum { get; set; }
        public float Maximum { get; set; }
        public float Current { get; set; }
    }

    public class VolumeClientManager
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private bool isConnected = false;
        private CancellationTokenSource cancellationTokenSource;
        private string serverIP;
        private int serverPort;
        private string serverPassword;

        public event EventHandler<string> ConnectionStateChanged;
        public event EventHandler<float> VolumeReceived;

        public VolumeClientManager(string ip, int port, string password)
        {
            serverIP = ip;
            serverPort = port;
            serverPassword = password;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIP, serverPort);
                networkStream = tcpClient.GetStream();

                // Þifreyi gönder
                byte[] passwordBytes = Encoding.UTF8.GetBytes(serverPassword);
                await networkStream.WriteAsync(passwordBytes, 0, passwordBytes.Length);

                // Sunucudan yanýt bekle
                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                if (response == "OK")
                {
                    isConnected = true;
                    ConnectionStateChanged?.Invoke(this, "Sunucuya baþarýyla baðlandý");
                    return true;
                }
                else
                {
                    ConnectionStateChanged?.Invoke(this, "Þifre yanlýþ veya sunucu hatasý");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, $"Baðlantý hatasý: {ex.Message}");
                return false;
            }
        }

        public async Task<VolumeInfo> GetVolumeInfoAsync()
        {
            if (!isConnected || networkStream == null)
                return null;

            try
            {
                string command = "GET_VOLUME_INFO";
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                await networkStream.WriteAsync(commandBytes, 0, commandBytes.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                if (response.StartsWith("VOLUME_INFO:"))
                {
                    string infoStr = response.Substring("VOLUME_INFO:".Length);
                    string[] parts = infoStr.Split(',');
                    
                    if (parts.Length == 3)
                    {
                        if (float.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float min) &&
                            float.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float max) &&
                            float.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float current))
                        {
                            return new VolumeInfo
                            {
                                Minimum = min,
                                Maximum = max,
                                Current = current
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, $"Ses bilgisi alma hatasý: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> SetVolumeAsync(float volume)
        {
            if (!isConnected || networkStream == null)
            {
                ConnectionStateChanged?.Invoke(this, "Sunucuya baðlý deðil");
                return false;
            }

            try
            {
                volume = Math.Max(0, Math.Min(1, volume));
                // Daha hassas formatlama ve InvariantCulture kullanýmý
                string command = $"SET_VOLUME:{volume.ToString("F4", CultureInfo.InvariantCulture)}";
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                
                await networkStream.WriteAsync(commandBytes, 0, commandBytes.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                return response == "OK";
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, $"Ses ayarlama hatasý: {ex.Message}");
                return false;
            }
        }

        public async Task<float?> GetVolumeAsync()
        {
            if (!isConnected || networkStream == null)
                return null;

            try
            {
                string command = "GET_VOLUME";
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                await networkStream.WriteAsync(commandBytes, 0, commandBytes.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                if (response.StartsWith("VOLUME:"))
                {
                    string volumeStr = response.Substring("VOLUME:".Length).Trim();
                    // InvariantCulture ile güvenilir parse
                    if (float.TryParse(volumeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float volume))
                    {
                        return volume;
                    }
                }
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, $"Ses alma hatasý: {ex.Message}");
            }

            return null;
        }

        public void Disconnect()
        {
            try
            {
                isConnected = false;
                cancellationTokenSource.Cancel();
                networkStream?.Dispose();
                tcpClient?.Close();
                tcpClient?.Dispose();
                ConnectionStateChanged?.Invoke(this, "Baðlantý kesildi");
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, $"Kesilme hatasý: {ex.Message}");
            }
        }

        public bool IsConnected => isConnected;
    }
}
