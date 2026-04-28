using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SoundPanel
{
    internal static class Program
    {

        // Kullanilacak olan DPI Awareness API'leri
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        // Sabit DPI icin kullanilan deger
        private const int DPI_AWARENESS_CONTEXT_UNAWARE = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Uygulamanin her zaman 100% DPI (96)
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_UNAWARE);

            // Register embedded fonts (Poppins etc.) from resources
            try
            {
                FontHelper.RegisterPoppinsFromResources();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Font registration failed: " + ex.Message);
            }

            // VISUAL STYLES VE RENDER QUALITY
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // If libvlc not present and embedded zip exists, show extraction UI
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var targetDir = Path.Combine(baseDir, "libvlc");
                bool needExtract = true;
                try { needExtract = !(Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any()); } catch { needExtract = true; }

                var asm = Assembly.GetExecutingAssembly();
                var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("libvlc.zip", StringComparison.OrdinalIgnoreCase));

                if (needExtract && resourceName != null)
                {
                    // Show extraction form which will perform extraction with progress
                    Application.Run(new ExtractionForm());
                }
                else
                {
                    // If no embedded resource found, still try silent extraction (backwards compatible)
                    try { EnsureEmbeddedLibVlcExtracted(); } catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LibVLC extraction failed: " + ex.Message);
            }

            // COMMAND LINE ARGUMENT HANDLING
            if (args.Length >= 2 && args[0] == "--online" && args[1] == "--volume")
            {
                if (args.Length >= 3 && args[2] == "--connect")
                {
                    // Client modu - uzaktan ses kontrol istemcisi
                    Application.Run(new VolumeClientForm());
                    return;
                }
                else
                {
                    // Server modu - Form1'i baþlat ve sunucuyu aç
                    Form1 mainForm = new Form1();
                    mainForm.Show();
                    
                    // Sunucu formunu aç
                    VolumeServerForm serverForm = new VolumeServerForm(mainForm);
                    serverForm.Show();
                    
                    Application.Run();
                    return;
                }
            }

            // Eski VoiceCable argümanlarý uyumluluðu
            if (args.Length >= 2 && args[0] == "--voicecable" && args[1].StartsWith("--PID:"))
            {
                if (uint.TryParse(args[1].Substring(6), out uint pid))
                {
                    Application.Run(new VirtualCableTransferForm(pid));
                    return;
                }
            }

            // ANA UYGULAMA
            Application.Run(new SystemCheck());
        }

        // Older silent extraction preserved for compatibility
        private static void EnsureEmbeddedLibVlcExtracted()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var targetDir = Path.Combine(baseDir, "libvlc");

            // If already present and not empty, nothing to do
            try
            {
                if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                    return;
            }
            catch { }

            var asm = Assembly.GetExecutingAssembly();
            var resourceName = asm.GetManifestResourceNames()
                                  .FirstOrDefault(n => n.EndsWith("libvlc.zip", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                return; // no embedded zip

            var tempDir = Path.Combine(Path.GetTempPath(), "SoundPanel_libvlc");
            Directory.CreateDirectory(tempDir);
            var tempZip = Path.Combine(tempDir, "libvlc_embedded.zip");

            using (var rs = asm.GetManifestResourceStream(resourceName))
            {
                if (rs == null) return;
                using (var fs = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    rs.CopyTo(fs);
                }
            }

            var extractDir = Path.Combine(tempDir, "extracted");
            if (Directory.Exists(extractDir))
            {
                try { Directory.Delete(extractDir, true); } catch { }
            }
            Directory.CreateDirectory(extractDir);

            try
            {
                ZipFile.ExtractToDirectory(tempZip, extractDir);
            }
            catch
            {
                try { File.Delete(tempZip); } catch { }
                try { Directory.Delete(tempDir, true); } catch { }
                return;
            }

            // Search for a libvlc folder inside the extracted content
            string foundLib = null;
            try
            {
                var libDirs = Directory.GetDirectories(extractDir, "libvlc", SearchOption.AllDirectories);
                if (libDirs != null && libDirs.Length > 0)
                {
                    foundLib = libDirs[0];
                }
            }
            catch { }

            // Ensure target directory exists
            Directory.CreateDirectory(targetDir);

            try
            {
                if (!string.IsNullOrEmpty(foundLib))
                {
                    CopyDirectory(foundLib, targetDir, overwrite: true);
                }
                else
                {
                    CopyDirectory(extractDir, targetDir, overwrite: true);
                }
            }
            catch { }

            try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
            try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
        }

        // New extraction method with progress and cancellation support used by ExtractionForm
        internal static bool ExtractEmbeddedLibVlcWithProgress(IProgress<int> progress, Action<string> setStatus, CancellationToken token, out string error)
        {
            error = null;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var targetDir = Path.Combine(baseDir, "libvlc");

            try
            {
                if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    progress?.Report(100);
                    setStatus?.Invoke("Zaten mevcut.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // continue
                System.Diagnostics.Debug.WriteLine("Error checking target dir: " + ex.Message);
            }

            var asm = Assembly.GetExecutingAssembly();
            var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("libvlc.zip", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
            {
                error = "Gömülü libvlc.zip bulunamadý.";
                return false;
            }

            try
            {
                using (var rs = asm.GetManifestResourceStream(resourceName))
                {
                    if (rs == null)
                    {
                        error = "Gömülü kaynak akýþý açýlamadý.";
                        return false;
                    }

                    using (var archive = new ZipArchive(rs, ZipArchiveMode.Read, leaveOpen: true))
                    {
                        var fileEntries = archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)).ToArray();
                        long totalBytes = fileEntries.Sum(e => e.Length);
                        long copied = 0;

                        // If totalBytes == 0, we can't show percent; set indeterminate
                        bool hasTotal = totalBytes > 0;

                        foreach (var entry in fileEntries)
                        {
                            if (token.IsCancellationRequested)
                            {
                                error = "Ýþlem kullanýcý tarafýndan iptal edildi.";
                                return false;
                            }

                            var entryPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                            var destPath = Path.Combine(targetDir, entryPath);
                            var destDir = Path.GetDirectoryName(destPath);
                            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                            try
                            {
                                using (var entryStream = entry.Open())
                                using (var outFs = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    var buffer = new byte[81920];
                                    int read;
                                    while ((read = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        if (token.IsCancellationRequested)
                                        {
                                            error = "Ýþlem kullanýcý tarafýndan iptal edildi.";
                                            return false;
                                        }

                                        outFs.Write(buffer, 0, read);
                                        copied += read;

                                        if (hasTotal)
                                        {
                                            int percent = (int)((copied * 100) / totalBytes);
                                            try { progress?.Report(Math.Max(0, Math.Min(100, percent))); } catch { }
                                        }
                                        else
                                        {
                                            try { progress?.Report(0); } catch { }
                                        }

                                        setStatus?.Invoke("Çýkarýlýyor: " + entry.Name);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // If writing a file fails, log and continue
                                System.Diagnostics.Debug.WriteLine("Failed to extract entry " + entry.FullName + ": " + ex.Message);
                            }
                        }

                        // Finalize
                        try { progress?.Report(100); } catch { }
                        setStatus?.Invoke("Tamamlandý.");

                        return Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any();
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir, bool overwrite = false)
        {
            // Create directories
            foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var newDir = dirPath.Replace(sourceDir, targetDir);
                if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);
            }

            // Copy files
            foreach (var filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                var newFile = filePath.Replace(sourceDir, targetDir);
                try
                {
                    File.Copy(filePath, newFile, overwrite);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed copying file " + filePath + " -> " + newFile + ": " + ex.Message);
                }
            }
        }
    }
}
