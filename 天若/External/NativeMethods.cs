using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TrOCR.External;
internal static class NativeMethods
{
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    public static extern int GetPrivateProfileString(string sectionName, string key, string defaultValue, byte[] returnBuffer, int size, string filePath);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    public static extern long WritePrivateProfileString(string sectionName, string key, string value, string filePath);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetWindowDC(IntPtr handle);

    [DllImport("user32")]
    public static extern bool AnimateWindow(IntPtr whnd, int dwtime, int dwflag);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessageA(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("User32.dll")]
    public static extern bool ReleaseCapture( );

    [DllImport("user32")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [DllImport("gdi32.dll")]
    public static extern int CreateRoundRectRgn(int x1, int y1, int x2, int y2, int x3, int y3);

    [DllImport("User32.dll")]
    public static extern int SetWindowRgn(IntPtr hwnd, int hRgn, bool bRedraw);

    [DllImport("user32")]
    public static extern int GetWindowLong(IntPtr hwnd, int nIndex);

    [DllImport("User32.dll")]
    public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("User32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

    [DllImport("User32.dll", ExactSpelling = true)]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int DeleteObject(IntPtr hObj);

    [DllImport("User32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr ExtCreateRegion(IntPtr lpXform, uint nCount, IntPtr rgnData);

    [DllImport("User32.dll")]
    public static extern int CreateCaret(IntPtr hwnd, int hBitmap, int nWidth, int nHeight);

    [DllImport("User32.dll")]
    public static extern bool ShowCaret(IntPtr hWnd);

    [DllImport("User32.dll")]
    public static extern bool SetCaretPos(int x, int y);

    [DllImport("User32.dll")]
    public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32.dll")]
    public static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32.dll")]
    public static extern uint GetCaretBlinkTime( );

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow( );

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
    public static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam) => SendMessage(hWnd, msg, wParam, "");

    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("User32.dll")]
    public static extern int SetClipboardViewer(int hWndNewViewer);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow( );

    [DllImport("user32", SetLastError = true)]
    public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, int wParam);

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern long mciSendString(string command, StringBuilder returnString, int returnSize, IntPtr hwndCallback);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

    [DllImport("user32.dll ", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll ", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClassLong(IntPtr hwnd, int nIndex);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

    [DllImport("kernel32.dll")]
    public static extern uint GetTickCount( );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern uint WinExec(string lpCmdLine, uint uCmdShow);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    public static Rectangle GetWindowRect(IntPtr hwnd)
    {
        GetWindowRect(hwnd, out RECT rect);
        return (Rectangle) rect;
    }

    public struct RECT
    {
        public static explicit operator Rectangle(RECT rect)
            => new(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);

        public int left;
        public int top;
        public int right;
        public int bottom;

        public static Rectangle ToRectangle(RECT left, RECT right) => throw new NotImplementedException( );
    }

    public static bool CaptureWindow(Control control, ref Bitmap bitmap)
    {
        Graphics graphics = Graphics.FromImage(bitmap);
        IntPtr intPtr = new(12);
        IntPtr hdc = graphics.GetHdc( );
        SendMessage(control.Handle, 791, hdc, intPtr);
        graphics.ReleaseHdc(hdc);
        graphics.Dispose( );
        return true;
    }

    public const int WM_CONTEXTMENU = 123;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 32;
    public const int WS_EX_LAYERED = 524288;
    public const byte AC_SRC_OVER = 0;
    public const int ULW_ALPHA = 2;
    public const byte AC_SRC_ALPHA = 1;
    public const int AW_HOR_POSITIVE = 1;
    public const int AW_HOR_NEGATIVE = 2;
    public const int AW_VER_POSITIVE = 4;
    public const int AW_VER_NEGATIVE = 8;
    public const int AW_CENTER = 16;
    public const int AW_HIDE = 65536;
    public const int AW_ACTIVATE = 131072;
    public const int AW_SLIDE = 262144;
    public const int AW_BLEND = 524288;
    public const int WM_MOUSEMOVE = 512;
    public const int WM_LBUTTONDOWN = 513;
    public const int WM_LBUTTONUP = 514;
    public const int WM_RBUTTONDOWN = 516;
    public const int WM_LBUTTONDBLCLK = 515;
    public const int WM_MOUSELEAVE = 675;
    public const int WM_PAINT = 15;
    public const int WM_ERASEBKGND = 20;
    public const int WM_PRINT = 791;
    public const int WM_HSCROLL = 276;
    public const int WM_VSCROLL = 277;
    public const int EM_GETSEL = 176;
    public const int EM_LINEINDEX = 187;
    public const int EM_LINEFROMCHAR = 201;
    public const int EM_POSFROMCHAR = 214;
    public const int WM_PRINTCLIENT = 792;
    public const long PRF_CHECKVISIBLE = 1L;
    public const long PRF_NONCLIENT = 2L;
    public const long PRF_CLIENT = 4L;
    public const long PRF_ERASEBKGND = 8L;
    public const long PRF_CHILDREN = 16L;
    public const long PRF_OWNED = 32L;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }
}
