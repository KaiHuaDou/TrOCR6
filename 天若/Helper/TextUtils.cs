using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Helper;
public static class TextUtils
{
    public static string CheckStr(string text)
    {
        if (ContainsZh(text.Trim( )))
        {
            text = PunctuationEnZh(text.Trim( ));
            text = CheckZhEn(text.Trim( ));
        }
        else
        {
            text = PunctuationChEn(text.Trim( ));
            if (Contains(text, ".")
                && (Contains(text, ",")
                || Contains(text, "!")
                || Contains(text, "(")
                || Contains(text, ")")
                || Contains(text, "'")))
            {
                text = PunctuationRemoveSpace(text);
            }
        }
        return text;
    }
    public static string CheckZhEn(string text)
    {
        char[] array = text.ToCharArray( );
        for (int i = 0; i < array.Length; i++)
        {
            int num = "：".IndexOf(array[i]);
            if (num == -1 || i - 1 < 0 || i + 1 >= array.Length || !ContainEn(array[i - 1].ToString( )))
                continue;
            if (ContainEn(array[i + 1].ToString( )))
                array[i] = ":"[num];
            if (ContainPunctuation(array[i + 1].ToString( )))
                array[i] = ":"[num];
        }
        return new string(array);
    }
    public static bool ContainEn(string str)
        => Regex.IsMatch(str, "[a-zA-Z]");

    public static bool ContainJap(string str)
        => Regex.IsMatch(str, "[\\u3040-\\u309F]") || Regex.IsMatch(str, "[\\u30A0-\\u30FF]");

    public static bool ContainKor(string str)
        => Regex.IsMatch(str, "[\\uac00-\\ud7ff]");

    public static bool ContainPunctuation(string str)
        => Regex.IsMatch(str, "\\p{P}");

    public static bool Contains(string text, string subStr)
        => text.Contains(subStr);

    public static bool ContainsZh(string str)
        => Regex.IsMatch(str, "[\\u4e00-\\u9fa5]");

    public static int CountEn(string text)
        => Regex.Matches(text, "\\s+").Count + 1;

    public static int CountZh(string str)
    {
        int num = 0;
        Regex regex = new("^[\\u4E00-\\u9FA5]{0,}$");
        for (int i = 0; i < str.Length; i++)
        {
            if (regex.IsMatch(str[i].ToString( )))
                num++;
        }
        return num;
    }

    public static int GetFirstNum(string str)
        => Convert.ToInt32(str.Split(',')[0]);

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

    public static bool HasBasicPunctuation(string text)
        => ",;，；、<>《》()-（）".IndexOf(text) != -1;

    public static bool IsNum(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] is < '0' or > '9')
                return false;
        }
        return true;
    }

    public static bool IsPunctuation(string text)
        => ",;:，（）、；".IndexOf(text) != -1;

    public static bool IsSplited(string text)
        => "。？！?!：".IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;

    public static string PunctuationChEn(string text)
    {
        char[] array = text.ToCharArray( );
        for (int i = 0; i < array.Length; i++)
        {
            int num = "：。；，？！“”‘’【】（）".IndexOf(array[i]);
            if (num != -1)
            {
                array[i] = ":.;,?!\"\"''[]()"[num];
            }
        }
        return new string(array);
    }

    public static string PunctuationEnZh(string text)
    {
        char[] array = text.ToCharArray( );
        for (int i = 0; i < array.Length; i++)
        {
            int num = ":;,?!()".IndexOf(array[i]);
            if (num != -1)
            {
                array[i] = "：；，？！（）"[num];
            }
        }
        return new string(array);
    }

    public static string PunctuationEnZhX(string text)
    {
        char[] array = text.ToCharArray( );
        for (int i = 0; i < array.Length; i++)
        {
            int num = ".:;,?![]()".IndexOf(array[i]);
            if (num != -1)
            {
                array[i] = "。：；，？！【】（）"[num];
            }
        }
        return new string(array);
    }

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

    public static string PunctuationRemoveSpace(string text)
    {
        string regex = "(?<=.)([^\\*]+)(?=.)";
        string result;
        if (Regex.Match(text, regex).ToString( ).IndexOf(" ") >= 0)
        {
            text = Regex.Replace(text, "(?<=[\\p{P}*])([a-zA-Z])(?=[a-zA-Z])", " $1");
            char[] array = null;
            text = text.TrimEnd(array).Replace("- ", "-").Replace("_ ", "_")
                .Replace("( ", "(")
                .Replace("/ ", "/")
                .Replace("\" ", "\"");
            result = text;
        }
        else
        {
            result = text;
        }
        return result;
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
                    text += array[i].ToString( );
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
        => Regex.Replace(hexData, "[\\p{P}+~$`^=|__～\uff40＄\uff3e＋＝｜＜＞￥×┊ ]", "").ToUpperInvariant( );

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

    public static void TextCheck(JArray jarray, int lastlength, string words, Action<JArray, string, string, string> finalize)
    {
        int num = 0;
        for (int i = 0; i < jarray.Count; i++)
        {
            int length = JObject.Parse(jarray[i].ToString( ))[words].ToString( ).Length;
            if (length > num)
            {
                num = length;
            }
        }
        string text = "";
        string text2 = "";
        for (int j = 0; j < jarray.Count - 1; j++)
        {
            JObject jobject = JObject.Parse(jarray[j].ToString( ));
            char[] array = jobject[words].ToString( ).ToCharArray( );
            JObject jobject2 = JObject.Parse(jarray[j + 1].ToString( ));
            char[] array2 = jobject2[words].ToString( ).ToCharArray( );
            int length2 = jobject[words].ToString( ).Length;
            int length3 = jobject2[words].ToString( ).Length;
            if (Math.Abs(length2 - length3) <= 0 || (IsSplited(array[array.Length - lastlength].ToString( )) && Math.Abs(length2 - length3) <= 1))
            {
                if ((IsSplited(array[array.Length - lastlength].ToString( )) && ContainEn(array2[0].ToString( )))
                    || (IsSplited(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
                    || (IsSplited(array[array.Length - lastlength].ToString( )) && IsPunctuation(array2[0].ToString( ))))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else
                {
                    text2 += jobject[words].ToString( ).Trim( );
                }
            }
            else if (!(!ContainsZh(array[array.Length - lastlength].ToString( )) || length2 > num / 2)
                  || !(!ContainsZh(array[array.Length - lastlength].ToString( )) || !IsNum(array2[0].ToString( )) || length3 - length2 >= 4 || array2[1].ToString( ) != "."))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            else if (ContainsZh(array[array.Length - lastlength].ToString( )) && ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (ContainEn(array[array.Length - lastlength].ToString( )) && ContainEn(array2[0].ToString( )))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + " ";
            }
            else if ((ContainsZh(array[array.Length - lastlength].ToString( )) && ContainEn(array2[0].ToString( )))
                || (ContainEn(array[array.Length - lastlength].ToString( )) && ContainsZh(array2[0].ToString( )))
                || (ContainsZh(array[array.Length - lastlength].ToString( )) && IsPunctuation(array2[0].ToString( )))
                || (IsPunctuation(array[array.Length - lastlength].ToString( )) && ContainsZh(array2[0].ToString( ))))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (IsPunctuation(array[array.Length - lastlength].ToString( )) && ContainEn(array2[0].ToString( )))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + " ";
            }
            else if ((ContainsZh(array[array.Length - lastlength].ToString( ))
                && IsNum(array2[0].ToString( ))) || (IsNum(array[array.Length - lastlength].ToString( )) && ContainsZh(array2[0].ToString( ))) || IsNum(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            if (HasBasicPunctuation(jobject[words].ToString( )))
            {
                text2 += "\r\n";
            }
            text = text + jobject[words].ToString( ).Trim( ) + "\r\n";
        }
        finalize(jarray, words, text, text2);
    }
}
