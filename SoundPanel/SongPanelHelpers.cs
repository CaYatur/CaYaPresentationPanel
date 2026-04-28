using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPanel
{
    internal static class SongPanelHelpers
    {
        // Helper metodu: ToggleTik
        public static void ToggleTik(CustomButton btnTik, ref bool toggleTik)
        {
            if (toggleTik)
            {
                toggleTik = false;
                btnTik.Text = "";
            }
            else
            {
                toggleTik = true;
                btnTik.Text = "✅";
            }
            btnTik.Invalidate();   // Kontrol yeniden çizilsin
            btnTik.Refresh();      // Hemen ekrana yansısın
        }
    }
}