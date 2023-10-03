using System;
using System.Web;
using CsharpHttpHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrOCR.Controls;

namespace TrOCR.Helper;

public static class Translate
{
    public static string Baidu(string text)
    {
        string result = "";
        try
        {
            (string from, string to) = ParseFromTo(text, result);
            HttpHelper httpHelper = new( );
            HttpItem httpItem = new( )
            {
                URL = "https://fanyi.baidu.com/basetrans",
                Method = "post",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Postdata = string.Concat(new string[]
                {
                    "query=",
                    HttpUtility.UrlEncode(text.Trim()).Replace("+", "%20"),
                    "&from=", from, "&to=", to
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
            (string from, string to) = ParseFromTo(text, result, ZhId: "zh-CN");
            HttpHelper httpHelper = new( );
            HttpItem httpItem = new( )
            {
                URL = "https://translate.googleapis.com/translate_a/single",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Postdata = string.Concat(new string[]
                {
                    "client=gtx&sl=",from, "&tl=", to, "&dt=t&q=",
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

    public static string Tencent(string text)
    {
        string result = "";
        try
        {
            (string from, string to) = ParseFromTo(text, result);
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(FmMain.TencentPOST("https://fanyi.qq.com/api/translate", Web.ContentLength(text, from, to))))["translate"]["records"].ToString( ));
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                result += jobject["targetText"].ToString( );
            }
        }
        catch (Exception)
        {
            result = "[腾讯接口报错]：\r\n1.接口请求出现问题等待修复。";
        }
        return result;
    }

    private static (string, string) ParseFromTo(string text, string result, string ZhId = "zh")
    {
        string from = "", to = "";
        switch (Defaults.TransType)
        {
            case TranslateType.ZhEn:
            {
                bool flag = TextUtils.CountZh(text.Trim( )) > TextUtils.CountEn(text.Trim( ))
                            || (TextUtils.CountEn(result.Trim( )) == 1 && TextUtils.CountZh(result.Trim( )) == 1);
                from = flag ? ZhId : "en";
                to = flag ? "en" : ZhId;
                break;
            }
            case TranslateType.ZhJp:
            {
                bool flag = TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(text.Trim( ))));
                from = flag ? ZhId : "jp";
                to = flag ? "jp" : ZhId;
                break;
            }
            case TranslateType.ZhKo:
            {
                bool flag = TextUtils.ContainKor(text.Trim( ));
                from = flag ? ZhId : "ko";
                to = flag ? "ko" : ZhId;
                break;
            }
        }
        return (from, to);
    }

    public static string TranslateAsConfig(string text)
    {
        string transInterface = Config.Get("配置", "翻译接口");
        if (transInterface == "谷歌")
            return Helper.Translate.Google(text);
        else if (transInterface == "百度")
            return Helper.Translate.Baidu(text);
        else if (transInterface == "腾讯")
            return Helper.Translate.Tencent(text);
        return "";
    }
}
