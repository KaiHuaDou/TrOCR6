using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Controls;
public static class TextUtils
{
    public static bool ContainEn(string str) => Regex.IsMatch(str, "[a-zA-Z]");

    public static bool ContainJap(string str)
        => Regex.IsMatch(str, "[\\u3040-\\u309F]") || Regex.IsMatch(str, "[\\u30A0-\\u30FF]");

    public static bool ContainKor(string str)
        => Regex.IsMatch(str, "[\\uac00-\\ud7ff]");

    public static bool Contains(string text, string subStr)
        => text.Contains(subStr);

    public static bool ContainsZh(string str)
        => Regex.IsMatch(str, "[\\u4e00-\\u9fa5]");

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

    public static bool HasPunctuation(string str)
    {
        string text = ContainsZh(str) ? "[\\；\\，\\。\\！\\？]" : "[\\;\\,\\.\\!\\?]";
        return Regex.IsMatch(str, text);
    }

    public static bool IsSplited(string text)
        => "。？！?!：".IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;

    public static string PunctuationQuotation(string pStr)
    {
        pStr = pStr.Replace("“", "\"").Replace("”", "\"");
        string[] array = pStr.Split(new char[] { '"' });
        string text = "";
        for (int i = 1; i <= array.Length; i++)
        {
            text = i % 2 == 0 ? text + array[i - 1] + "”" : text + array[i - 1] + "“";
        }
        return text.Substring(0, text.Length - 1);
    }

    public static string Quotation(string str)
        => Regex.Replace(str.Replace("“", "\"").Replace("”", "\""), "(?<=\")([^\\\"\\“\\”]+)(?=\")", "$1_测_$2");

    public static string RemoveSpace(string text)
    {
        text = Regex.Replace(text, "([\\p{P}]+)", "**&&**$1**&&**");
        char[] array = null;
        text = text.TrimEnd(array).Replace(" **&&**", "").Replace("**&&** ", "")
            .Replace("**&&**", "");
        return text;
    }

    public static string RemoveZh(string str)
    {
        string text = str;
        if (Regex.IsMatch(str, "[\\u4e00-\\u9fa5]"))
        {
            text = string.Empty;
            char[] array = str.ToCharArray( );
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] is < '一' or > '龥')
                {
                    text += array[i].ToString( );
                }
            }
        }
        return text;
    }

    public static string RenameFile(string folder, string file)
    {
        string text = folder + "\\" + file;
        int num = text.LastIndexOf('.');
        text = text.Insert(num, "_{0}");
        int num2 = 1;
        string text2 = string.Format(text, num2);
        while (File.Exists(text2))
        {
            text2 = string.Format(text, num2);
            num2++;
        }
        return Path.GetFileName(text2);
    }

    public static string RepalceStr(string hexData)
        => Regex.Replace(hexData, "[\\p{P}+~$`^=|__～\uff40＄\uff3e＋＝｜＜＞￥×┊ ]", "").ToUpper(System.Globalization.CultureInfo.CurrentCulture);

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
}
