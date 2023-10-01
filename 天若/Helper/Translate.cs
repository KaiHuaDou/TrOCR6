using System;
using System.Net;
using System.Web;
using CsharpHttpHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrOCR.Helper;

public static class Translate
{
    public static string Baidu(string Text)
    {
        string result = "";
        try
        {
            string from = "zh";
            string to = "en";
            if (StaticValue.Zh2En)
            {
                if (TextUtils.CountZh(Text.Trim( )) > TextUtils.CountEn(Text.Trim( )) || TextUtils.CountEn(result.Trim( )) == 1 && TextUtils.CountZh(result.Trim( )) == 1)
                {
                    from = "zh";
                    to = "en";
                }
                else
                {
                    from = "en";
                    to = "zh";
                }
            }
            if (StaticValue.Zh2Jp)
            {
                if (TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(Text.Trim( )))))
                {
                    from = "jp";
                    to = "zh";
                }
                else
                {
                    from = "zh";
                    to = "jp";
                }
            }
            if (StaticValue.Zh2Ko)
            {
                if (TextUtils.ContainKor(Text.Trim( )))
                {
                    from = "kor";
                    to = "zh";
                }
                else
                {
                    from = "zh";
                    to = "kor";
                }
            }
            HttpHelper httpHelper = new( );
            HttpItem httpItem = new( )
            {
                URL = "https://fanyi.baidu.com/basetrans",
                Method = "post",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Postdata = string.Concat(new string[]
                {
                        "query=",
                        HttpUtility.UrlEncode(Text.Trim()).Replace("+", "%20"),
                        "&from=",
                        from,
                        "&to=",
                        to
                }),
                UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Mobile Safari/537.36"
            };
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(httpHelper.GetHtml(httpItem).Html))["trans"].ToString( ));
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                result = result + jobject["dst"] + "\r\n";
            }
        }
        catch (Exception)
        {
            result = "[百度接口报错]：\r\n1.接口请求出现问题等待修复。";
        }
        return result;
    }

    public static string Google(string text)
    {
        string result = "";
        try
        {
            string from = "zh-CN";
            string to = "en";
            if (StaticValue.Zh2En)
            {
                if (TextUtils.CountZh(text.Trim( )) > TextUtils.CountEn(text.Trim( )) || TextUtils.CountEn(text.Trim( )) == 1 && TextUtils.CountZh(text.Trim( )) == 1)
                {
                    from = "zh-CN";
                    to = "en";
                }
                else
                {
                    from = "en";
                    to = "zh-CN";
                }
            }
            if (StaticValue.Zh2Jp)
            {
                if (TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(text.Trim( )))))
                {
                    from = "ja";
                    to = "zh-CN";
                }
                else
                {
                    from = "zh-CN";
                    to = "ja";
                }
            }
            if (StaticValue.Zh2Ko)
            {
                if (TextUtils.ContainKor(text.Trim( )))
                {
                    from = "ko";
                    to = "zh-CN";
                }
                else
                {
                    from = "zh-CN";
                    to = "ko";
                }
            }
            HttpHelper httpHelper = new( );
            HttpItem httpItem = new( )
            {
                URL = "https://translate.googleapis.com/translate_a/single",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Postdata = string.Concat(new string[]
                {
                        "client=gtx&sl=",
                        from,
                        "&tl=",
                        to,
                        "&dt=t&q=",
                        HttpUtility.UrlEncode(text).Replace("+", "%20")
                }),
                UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.2228.0 Safari/537.36",
                Accept = "*/*"
            };
            JArray jarray = (JArray) JsonConvert.DeserializeObject(httpHelper.GetHtml(httpItem).Html);
            int count = ((JArray) jarray[0]).Count;
            for (int i = 0; i < count; i++)
            {
                result += jarray[0][i][0].ToString( );
            }
        }
        catch (Exception)
        {
            result = "[谷歌接口报错]：\r\n1.网络错误或者文本过长。\r\n2.谷歌接口可能对于某些网络不能用.";
        }
        return result;
    }
}
