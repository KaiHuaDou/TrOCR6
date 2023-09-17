using System;
using System.Runtime.InteropServices;

namespace TrOCR.Ocr;

public static class AutoClosedMsgBox
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool EndDialog(IntPtr hDlg, int nResult);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxTimeout(IntPtr hwnd, string txt, string caption, int wtype, int wlange, int dwtimeout);

    public static int Show(string text, string caption, int milliseconds, MsgBoxStyle style)
        => MessageBoxTimeout(IntPtr.Zero, text, caption, (int) style, 0, milliseconds);

    public static int Show(string text, string caption, int milliseconds, int style)
        => MessageBoxTimeout(IntPtr.Zero, text, caption, style, 0, milliseconds);
}
