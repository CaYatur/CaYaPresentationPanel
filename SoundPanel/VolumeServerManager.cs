using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SoundPanel
{
    public class VolumeServerManager
    {
        private TcpListener tcpListener;
        private bool isRunning = false;
        private int port;
        private string password;
        private Action<float> onVolumeChange;
        private Func<float> getVolume;
        private CancellationTokenSource cancellationTokenSource;

        public string ServerIP { get; private set; }
        public int ServerPort => port;
        public string ServerPassword => password;

        public event EventHandler<string> ServerStateChanged;

        public VolumeServerManager(Action<float> volumeChangeCallback, Func<float> getVolumeCallback)
        {
            onVolumeChange = volumeChangeCallback;
            getVolume = getVolumeCallback;
            cancellationTokenSource = new CancellationTokenSource();
            GeneratePassword();
        }

        private void GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            password = new string(Enumerable.Range(0, 6)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        public async Task<bool> StartServerAsync()
        {
            try
            {
                port = FindFreePort();
                ServerIP = GetLocalIPAddress();

                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                isRunning = true;

                ServerStateChanged?.Invoke(this, $"✅ Sunucu başlatıldı: {ServerIP}:{port}");
                
                _ = AcceptClientsAsync();
                return true;
            }
            catch (Exception ex)
            {
                ServerStateChanged?.Invoke(this, $"❌ Sunucu başlatma hatası: {ex.Message}");
                return false;
            }
        }

        public void StopServer()
        {
            try
            {
                isRunning = false;
                cancellationTokenSource.Cancel();
                tcpListener?.Stop();
                tcpListener?.Dispose();
                ServerStateChanged?.Invoke(this, "🛑 Sunucu durduruldu");
            }
            catch (Exception ex)
            {
                ServerStateChanged?.Invoke(this, $"❌ Sunucu durdurma hatası: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (isRunning && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    ServerStateChanged?.Invoke(this, $"🔗 Yeni bağlantı denemesi: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ServerStateChanged?.Invoke(this, $"❌ İstemci kabul hatası: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            try
            {
                using (var stream = client.GetStream())
                {
                    // Şifre taraması
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        ServerStateChanged?.Invoke(this, $"⚠️ İstemci {clientIP} hiçbir veri göndermedi");
                        client.Close();
                        return;
                    }

                    string receivedPassword = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    if (receivedPassword != password)
                    {
                        ServerStateChanged?.Invoke(this, $"🔒 Şifre hatasının: {clientIP} (gelen: {receivedPassword})");
                        byte[] response = Encoding.UTF8.GetBytes("ERROR:InvalidPassword");
                        await stream.WriteAsync(response, 0, response.Length);
                        client.Close();
                        return;
                    }

                    // Şifre doğru
                    byte[] okResponse = Encoding.UTF8.GetBytes("OK");
                    await stream.WriteAsync(okResponse, 0, okResponse.Length);

                    ServerStateChanged?.Invoke(this, $"✅ İstemci bağlandı: {clientIP}");

                    // İstemci komutlarını dinle
                    while (isRunning && client.Connected)
                    {
                        buffer = new byte[1024];
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            ServerStateChanged?.Invoke(this, $"⚠️ İstemci {clientIP} bağlantıyı kesti");
                            break;
                        }

                        string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        await ProcessCommandAsync(stream, command, clientIP);
                    }
                }
            }
            catch (IOException ex)
            {
                ServerStateChanged?.Invoke(this, $"❌ I/O Hatası ({clientIP}): {ex.Message}");
            }
            catch (Exception ex)
            {
                ServerStateChanged?.Invoke(this, $"❌ İstemci işleme hatası ({clientIP}): {ex.Message}");
            }
            finally
            {
                try
                {
                    client.Close();
                    client.Dispose();
                    ServerStateChanged?.Invoke(this, $"🔌 İstemci kesildi: {clientIP}");
                }
                catch { }
            }
        }

        private async Task ProcessCommandAsync(NetworkStream stream, string command, string clientIP)
        {
            try
            {
                if (command.StartsWith("SET_VOLUME:"))
                {
                    string volumeStr = command.Substring("SET_VOLUME:".Length);
                    if (float.TryParse(volumeStr.Replace(",", "."), System.Globalization.NumberStyles.Any, 
                        CultureInfo.InvariantCulture, out float volume))
                    {
                        volume = Math.Max(0, Math.Min(1, volume));
                        onVolumeChange?.Invoke(volume);
                        
                        byte[] response = Encoding.UTF8.GetBytes("OK");
                        await stream.WriteAsync(response, 0, response.Length);
                        
                        ServerStateChanged?.Invoke(this, $"🔊 SET_VOLUME ({clientIP}): {volume:F2}");
                    }
                    else
                    {
                        byte[] response = Encoding.UTF8.GetBytes("ERROR:InvalidVolume");
                        await stream.WriteAsync(response, 0, response.Length);
                        ServerStateChanged?.Invoke(this, $"⚠️ Geçersiz ses değeri ({clientIP}): {volumeStr}");
                    }
                }
                else if (command == "GET_VOLUME")
                {
                    float currentVolume = getVolume?.Invoke() ?? 0.5f;
                    byte[] response = Encoding.UTF8.GetBytes($"VOLUME:{currentVolume.ToString("F4", CultureInfo.InvariantCulture)}");
                    await stream.WriteAsync(response, 0, response.Length);
                    ServerStateChanged?.Invoke(this, $"📊 GET_VOLUME ({clientIP}): {currentVolume:F2}");
                }
                else if (command == "GET_VOLUME_INFO")
                {
                    float currentVolume = getVolume?.Invoke() ?? 0.5f;
                    const float minVolume = 0.0f;
                    const float maxVolume = 1.0f;
                    
                    string volumeInfo = $"VOLUME_INFO:{minVolume.ToString("F4", CultureInfo.InvariantCulture)}," +
                                       $"{maxVolume.ToString("F4", CultureInfo.InvariantCulture)}," +
                                       $"{currentVolume.ToString("F4", CultureInfo.InvariantCulture)}";
                    byte[] response = Encoding.UTF8.GetBytes(volumeInfo);
                    await stream.WriteAsync(response, 0, response.Length);
                    ServerStateChanged?.Invoke(this, $"ℹ️ GET_VOLUME_INFO ({clientIP}): Min=0, Max=1, Şu an={currentVolume:F2}");
                }
                else if (command == "PING")
                {
                    byte[] response = Encoding.UTF8.GetBytes("PONG");
                    await stream.WriteAsync(response, 0, response.Length);
                    ServerStateChanged?.Invoke(this, $"💓 PING ({clientIP})");
                }
                else
                {
                    ServerStateChanged?.Invoke(this, $"⚠️ Bilinmeyen komut ({clientIP}): {command}");
                }
            }
            catch (Exception ex)
            {
                ServerStateChanged?.Invoke(this, $"❌ Komut işleme hatası ({clientIP}): {ex.Message}");
            }
        }

        private int FindFreePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            listener.Dispose();
            return port;
        }

        private string GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString() ?? "127.0.0.1";
                }
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}
