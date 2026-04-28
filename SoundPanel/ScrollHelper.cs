using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class ScrollHelper
{
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

    public const int WM_VSCROLL = 0x0115;
    public const int SB_THUMBPOSITION = 4;

    public static void SetVerticalScroll(FlowLayoutPanel panel, int value)
    {
        SendMessage(panel.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * value, 0);
    }
}