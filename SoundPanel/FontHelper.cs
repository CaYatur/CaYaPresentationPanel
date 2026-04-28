using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SoundPanel
{
    // Helper to load fonts from embedded resources (byte[] in Properties.Resources)
    internal static class FontHelper
    {
        private static readonly PrivateFontCollection _pfc = new PrivateFontCollection();
        private static readonly List<IntPtr> _allocated = new List<IntPtr>();
        private static readonly List<string> _tempFiles = new List<string>();

        // AddFontResourceEx flags
        private const uint FR_PRIVATE = 0x10;
        private const uint FR_NOT_ENUM = 0x20;

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool RemoveFontResourceEx(string name, uint fl, IntPtr pdv);

        // Register a font from raw byte[] (ttf/otf)
        public static void RegisterFontFromBytes(byte[] fontData)
        {
            if (fontData == null || fontData.Length == 0) return;
            try
            {
                // 1) Load into PrivateFontCollection (safe, does not rely on system-installed fonts)
                IntPtr dataPtr = Marshal.AllocCoTaskMem(fontData.Length);
                Marshal.Copy(fontData, 0, dataPtr, fontData.Length);
                _allocated.Add(dataPtr);
                _pfc.AddMemoryFont(dataPtr, fontData.Length);

                // 2) Also write temp file and register with AddFontResourceEx(FR_PRIVATE)
                // so that calls like new Font("Poppins", ...) will find the family name.
                string tempPath = Path.Combine(Path.GetTempPath(), "sp_font_" + Guid.NewGuid().ToString() + ".ttf");
                File.WriteAllBytes(tempPath, fontData);
                _tempFiles.Add(tempPath);

                int res = AddFontResourceEx(tempPath, FR_PRIVATE, IntPtr.Zero);
                if (res == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"AddFontResourceEx failed for {tempPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RegisterFontFromBytes failed: " + ex.Message);
            }
        }

        // Convenience: register known Poppins fonts from Properties.Resources if available
        public static void RegisterPoppinsFromResources()
        {
            try
            {
                // PoppinsBold exists in Resources.Designer as byte[]; register if present
                try
                {
                    var bold = Properties.Resources.PoppinsBold;
                    RegisterFontFromBytes(bold);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("PoppinsBold resource not found or failed: " + ex.Message);
                }

                // If you add other Poppins weights to resources (e.g. PoppinsRegular, PoppinsSemiBold)
                // you can register them here similarly, for example:
                // RegisterFontFromBytes(Properties.Resources.PoppinsRegular);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RegisterPoppinsFromResources error: " + ex.Message);
            }
        }

        // Get a Font object for a given family name. Falls back to system font if not registered.
        public static Font GetFont(string familyName, float emSize, FontStyle style = FontStyle.Regular)
        {
            try
            {
                if (!string.IsNullOrEmpty(familyName))
                {
                    // Check PrivateFontCollection families first
                    foreach (var f in _pfc.Families)
                    {
                        if (string.Equals(f.Name, familyName, StringComparison.OrdinalIgnoreCase))
                        {
                            return new Font(f, emSize, style, GraphicsUnit.Point);
                        }
                    }

                    // If not in PFC, try direct construction (works if AddFontResourceEx registered the font)
                    try
                    {
                        return new Font(familyName, emSize, style, GraphicsUnit.Point);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetFont error: " + ex.Message);
            }

            // Fallback
            try { return new Font("Segoe UI", emSize, style); }
            catch { return SystemFonts.DefaultFont; }
        }

        // Optional: get list of registered family names from private collection
        public static IEnumerable<string> GetRegisteredFamilyNames()
        {
            foreach (var f in _pfc.Families)
                yield return f.Name;
        }

        // Optional: cleanup allocated unmanaged memory and temp files (call when app exiting)
        public static void DisposeRegisteredFonts()
        {
            try
            {
                foreach (var tf in _tempFiles)
                {
                    try
                    {
                        RemoveFontResourceEx(tf, FR_PRIVATE, IntPtr.Zero);
                    }
                    catch { }

                    try { if (File.Exists(tf)) File.Delete(tf); } catch { }
                }
                _tempFiles.Clear();
            }
            catch { }

            try
            {
                _pfc.Dispose();
            }
            catch { }

            foreach (var p in _allocated)
            {
                try { Marshal.FreeCoTaskMem(p); } catch { }
            }
            _allocated.Clear();
        }
    }
}
