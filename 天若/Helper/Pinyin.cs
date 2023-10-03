using System.Collections;
using System.Text;

namespace TrOCR.Helper;

public static partial class Pinyin
{
    public static string Convert(string str)
    {
        StringBuilder result = new( );
        string text = "";
        foreach (char c in str)
        {
            string text2 = c.ToString( );
            if (TextUtils.ContainsZh(c.ToString( )))
                text2 = hashtable[c.ToString( )] as string;
            result = TextUtils.ContainEn(c.ToString( ))
                ? result.Append(text2)
                : TextUtils.ContainEn(text)
                ? result.Append(" " + text2 + " ")
                : result.Append(text2 + " ");
            text = text2;
        }
        return result.ToString( ).Replace("  ", " ").Replace("\n ", "\n")
            .Replace(" \n", "\n")
            .Trim( );
    }
}
