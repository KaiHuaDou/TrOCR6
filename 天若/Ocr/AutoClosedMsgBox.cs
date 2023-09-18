using System;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Ocr;

public static class AutoClosedMsgBox
{
    public static int Show(string text, string caption, int milliseconds, MsgBoxStyle style)
        => MessageBoxTimeout(IntPtr.Zero, text, caption, (int) style, 0, milliseconds);

    public static int Show(string text, string caption, int milliseconds, int style)
        => MessageBoxTimeout(IntPtr.Zero, text, caption, style, 0, milliseconds);
}
