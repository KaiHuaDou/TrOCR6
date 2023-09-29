using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TrOCR.External;
internal static class NativeMethods
{
    #region DllImports
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int DeleteObject(IntPtr hObj);

    [DllImport("dwmapi.dll")]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("dwmapi.dll")]
    public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow( );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetPrivateProfileString(string sectionName, string key, string defaultValue, byte[] returnBuffer, int size, string filePath);

    [DllImport("kernel32.dll")]
    public static extern uint GetTickCount( );

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("wininet.dll")]
    public static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

    [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref int pcchCookieData, int dwFlags, object lpReserved);

    [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref uint pcchCookieData, int dwFlags, IntPtr lpReserved);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    public static extern long mciSendString(string command, StringBuilder returnString, int returnSize, IntPtr hwndCallback);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBoxTimeout(IntPtr hwnd, string txt, string caption, int wtype, int wlange, int dwtimeout);

    [DllImport("user32.dll ", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture( );

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    public static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam) => SendMessage(hWnd, msg, wParam, "");

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT lp);

    [DllImport("user32.dll")]
    public static extern int SetClipboardViewer(int hWndNewViewer);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware( );

    [DllImport("user32.dll ", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern long WritePrivateProfileString(string sectionName, string key, string value, string filePath);
    #endregion

    public const byte AC_SRC_ALPHA = 1;
    public const byte AC_SRC_OVER = 0;
    public const int AW_ACTIVATE = 131072;
    public const int AW_BLEND = 524288;
    public const int AW_CENTER = 16;
    public const int AW_HIDE = 65536;
    public const int AW_HOR_NEGATIVE = 2;
    public const int AW_HOR_POSITIVE = 1;
    public const int AW_SLIDE = 262144;
    public const int AW_VER_NEGATIVE = 8;
    public const int AW_VER_POSITIVE = 4;
    public const int CS_DROPSHADOW = 131072;
    public const int EM_GETSEL = 176;
    public const int EM_LINEFROMCHAR = 201;
    public const int EM_LINEINDEX = 187;
    public const int EM_POSFROMCHAR = 214;
    public const int GWL_EXSTYLE = -20;
    public const int HTCAPTION = 2;
    public const int HTCLIENT = 1;
    public const long PRF_CHECKVISIBLE = 1L;
    public const long PRF_CHILDREN = 16L;
    public const long PRF_CLIENT = 4L;
    public const long PRF_ERASEBKGND = 8L;
    public const long PRF_NONCLIENT = 2L;
    public const long PRF_OWNED = 32L;
    public const int ULW_ALPHA = 2;
    public const int WM_ACTIVATEAPP = 28;
    public const int WM_CONTEXTMENU = 123;
    public const int WM_ERASEBKGND = 20;
    public const int WM_HSCROLL = 276;
    public const int WM_LBUTTONDBLCLK = 515;
    public const int WM_LBUTTONDOWN = 513;
    public const int WM_LBUTTONUP = 514;
    public const int WM_MOUSELEAVE = 675;
    public const int WM_MOUSEMOVE = 512;
    public const int WM_NCHITTEST = 132;
    public const int WM_NCPAINT = 133;
    public const int WM_PAINT = 15;
    public const int WM_PRINT = 791;
    public const int WM_PRINTCLIENT = 792;
    public const int WM_RBUTTONDOWN = 516;
    public const int WM_VSCROLL = 277;
    public const int WS_EX_LAYERED = 524288;
    public const int WS_EX_TRANSPARENT = 32;

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
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

    public static Rectangle GetWindowRect(IntPtr hwnd)
    {
        GetWindowRect(hwnd, out RECT rect);
        return (Rectangle) rect;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    public struct MARGINS
    {
        public int bottomHeight;
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
    }

    public struct PARAFORMAT
    {
        public byte bLineSpacingRule;
        public byte bOutlineLevel;
        public int cbSize;
        public short cTabCount;
        public uint dwMask;
        public int dxOffset;
        public int dxRightIndent;
        public int dxStartIndent;
        public int dyLineSpacing;
        public int dySpaceAfter;
        public int dySpaceBefore;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] rgxTabs;

        public short sStyle;
        public short wAlignment;
        public short wBorders;
        public short wBorderSpace;
        public short wBorderWidth;
        public short wNumbering;
        public short wNumberingStart;
        public short wNumberingStyle;
        public short wNumberingTab;
        public short wReserved;
        public short wShadingStyle;
        public short wShadingWeight;
    }

    public struct RECT
    {
        public int bottom;
        public int left;
        public int right;
        public int top;

        public static explicit operator Rectangle(RECT rect)
                                            => new(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        public static Rectangle ToRectangle(RECT left, RECT right) => throw new NotImplementedException( );
    }
}
