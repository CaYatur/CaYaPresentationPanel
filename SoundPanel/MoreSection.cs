using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SoundPanel
{
    public partial class MoreSection : Form
    {
        public MoreSection()
        {
            InitializeComponent();
        }

        private void btnContentShow_Click(object sender, EventArgs e)
        {
            CustomTextShowForm ctsf = new CustomTextShowForm();
            ctsf.Show();
        }

        private void VirtualCableTransfer_Click(object sender, EventArgs e)
        {
            string exePath = Application.ExecutablePath;
            int mevcutPid = Process.GetCurrentProcess().Id;
            string args = $"--voicecable --PID:{mevcutPid}";
            string batchPath = Path.Combine(Path.GetTempPath(), "start_app.bat");

            // Batch dosyasýný oluþtur
            string batchContent = $"@echo off\ncd /d C:\\\nstart \"\" \"{exePath}\" {args}\ndel \"%~f0\"\nexit";
            File.WriteAllText(batchPath, batchContent);

            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/C \"{batchPath}\"")
            {
                UseShellExecute = false, // CMD için gizli çalýþtýrma
                CreateNoWindow = true, // CMD penceresini gizle
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false
            };

            try
            {
                Process process = Process.Start(psi);
                if (process != null)
                {
                    process.Dispose(); // Süreci hemen serbest býrak
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama baþlatýlamadý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
