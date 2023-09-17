using System;
using System.Threading;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Ocr;
public static class TextUtils
{
    public static bool IsSplited(string text)
        => "。？！?!：".IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;

    public static string ToSimplified(string source)
    {
        string text = new(' ', source.Length);
        LCMapString(2048, 33554432, source, source.Length, text, text.Length);
        return text;
    }

    public static string ToTraditional(string source)
    {
        string text = new(' ', source.Length);
        LCMapString(2048, 67108864, source, source.Length, text, source.Length);
        return text;
    }
    public static string GetTextFromClipboard( )
    {
        if (Thread.CurrentThread.GetApartmentState( ) > ApartmentState.STA)
        {
            Thread thread = new(( ) =>
            {
                SendKeys.SendWait("^C");
                SendKeys.Flush( );
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start( );
            thread.Join( );
        }
        string text = Clipboard.GetText( );
        text = string.IsNullOrWhiteSpace(text) ? null : text;
        if (text != null)
            Clipboard.Clear( );
        return text;
    }
}
