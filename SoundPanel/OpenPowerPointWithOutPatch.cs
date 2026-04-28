using System;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SoundPanel
{
    public partial class OpenPowerPointWithOutPatch : Form
    {
        private string _filePath; // Açılacak PowerPoint dosya yolu
        private string localDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private int _pptProcessId = -1;

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public OpenPowerPointWithOutPatch(string filePath)
        {
            InitializeComponent();
            _filePath = Path.Combine(localDirectory, filePath);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!File.Exists(_filePath))
            {
                MessageBox.Show("Sunum dosyası bulunamadı:\n" + _filePath, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            PlayPowerPoint(_filePath);
        }

        private void PlayPowerPoint(string filePath)
        {
            Microsoft.Office.Interop.PowerPoint.Application pptApplication = null;
            Presentation presentation = null;

            try
            {
                try
                {
                    pptApplication = new Microsoft.Office.Interop.PowerPoint.Application();
                }
                catch (COMException comEx)
                {
                    MessageBox.Show(
                        "PowerPoint başlatılamadı. Lütfen Microsoft PowerPoint'in doğru şekilde yüklü olduğundan emin olun.\n\n" +
                        "Detay: " + comEx.Message,
                        "Office Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "PowerPoint başlatılırken beklenmeyen bir hata oluştu.\nDetay: " + ex.Message,
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return;
                }

                // Process ID alma
                uint processId = 0;
                GetWindowThreadProcessId((IntPtr)pptApplication.HWND, out processId);
                _pptProcessId = (int)processId;

                // Sunumu açmayı dene
                try
                {
                    presentation = pptApplication.Presentations.Open(
                        filePath,
                        ReadOnly: MsoTriState.msoTrue,
                        WithWindow: MsoTriState.msoFalse
                    );
                }
                catch (COMException comEx)
                {
                    MessageBox.Show(
                        "PowerPoint dosyası açılamadı. Dosya bozuk olabilir veya başka bir uygulama tarafından kullanılıyor olabilir.\n\n" +
                        "Detay: " + comEx.Message,
                        "Sunum Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    pptApplication.Quit();
                    Marshal.ReleaseComObject(pptApplication);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Sunum açılırken beklenmeyen bir hata oluştu.\nDetay: " + ex.Message,
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    pptApplication.Quit();
                    Marshal.ReleaseComObject(pptApplication);
                    return;
                }

                // Sunumu başlat
                try
                {
                    presentation.SlideShowSettings.StartingSlide = 1;
                    presentation.SlideShowSettings.EndingSlide = presentation.Slides.Count;
                    presentation.SlideShowSettings.Run();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Sunum başlatılırken hata oluştu. Sunum dosyası bozuk veya PowerPoint ile uyumsuz olabilir.\n\nDetay: " + ex.Message,
                        "Sunum Başlatma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    presentation.Close();
                    Marshal.ReleaseComObject(presentation);
                    pptApplication.Quit();
                    Marshal.ReleaseComObject(pptApplication);
                    return;
                }

                // Sunum bitene dek bekle
                while (pptApplication.SlideShowWindows.Count >= 1)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(
                //    "Bilinmeyen bir hata oluştu:\n" + ex.Message,
                //    "Genel Hata", MessageBoxButtons.OK, MessageBoxIcon.Error
                //);
            }
            finally
            {
                try
                {
                    if (presentation != null)
                    {
                        presentation.Close();
                        Marshal.ReleaseComObject(presentation);
                    }
                    if (pptApplication != null)
                    {
                        pptApplication.Quit();
                        Marshal.ReleaseComObject(pptApplication);
                    }
                }
                catch
                {
                    // yoksay
                }

                // PowerPoint processini zorla kapat (gerekirse)
                if (_pptProcessId > 0)
                {
                    try
                    {
                        Process pptProc = Process.GetProcessById(_pptProcessId);
                        if (!pptProc.HasExited)
                        {
                            pptProc.Kill();
                        }
                    }
                    catch
                    {
                        // Process zaten kapanmış olabilir
                    }
                }

                this.Close();
            }
        }
    }
}