using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using static TrOCR.External.NativeMethods;
namespace TrOCR.Helper;

public static class Web
{
    public static string ContentLength(string text, string fromlang, string tolang)
        => $"&source={fromlang}&target={tolang}&sourceText={HttpUtility.UrlEncode(text).Replace("+", "%20")}";

    public static string CookieToStr(CookieCollection cookies)
    {
        if (cookies == null)
            return string.Empty;
        string result = string.Empty;
        foreach (Cookie cookie in cookies)
        {
            result += string.Format("{0}={1};", cookie.Name, cookie.Value);
        }
        return result;
    }

    public static string GetCookies(string url)
    {
        uint num = 1024U;
        StringBuilder result = new((int) num);
        if (!InternetGetCookieEx(url, null, result, ref num, 8192, IntPtr.Zero))
        {
            if (num < 0U)
            {
                return null;
            }
            result = new StringBuilder((int) num);
            if (!InternetGetCookieEx(url, null, result, ref num, 8192, IntPtr.Zero))
            {
                return null;
            }
        }
        return result.ToString( );
    }

    public static string GetHtml(string url)
    {
        string result = "";
        HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";
        try
        {
            using (HttpWebResponse res = (HttpWebResponse) req.GetResponse( ))
            {
                using StreamReader reader = new(res.GetResponseStream( ), Encoding.UTF8);
                result = reader.ReadToEnd( );
                reader.Close( );
                res.Close( );
            }
            req.Abort( );
        }
        catch
        {
            result = "";
        }
        return result;
    }

    private static string PostCompressContent(string url, string content)
    {
        HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
        req.Method = "POST";
        req.Timeout = 3000;
        req.ContentType = "application/x-www-form-urlencoded";
        req.Headers.Add("Accept-Encoding: gzip, deflate");
        req.Headers.Add("Accept-Language: zh-CN,en,*");
        return HttpReq(content, req);
    }

    public static string PostHtml(string url, string content)
    {
        HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
        req.Method = "POST";
        req.Timeout = 6000;
        req.ContentType = "application/x-www-form-urlencoded";
        req.Headers.Add("Accept-Language: zh-CN,en,*");
        return HttpReq(content, req);
    }

    public static string HttpReq(string content, HttpWebRequest req)
    {
        string result = "";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        try
        {
            using (Stream reqStream = req.GetRequestStream( ))
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }
            Stream resStream = ((HttpWebResponse) req.GetResponse( )).GetResponseStream( );
            StreamReader reader = new(resStream, Encoding.GetEncoding("utf-8"));
            result = reader.ReadToEnd( );
            resStream.Close( );
            reader.Close( );
            req.Abort( );
        }
        catch { }
        return result;
    }

    public static string PostHtmlFinal(string url, string postStr, string CookieContainer)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(postStr);
        string text = "";
        HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
        req.Method = "POST";
        req.Accept = "*/*";
        req.Timeout = 5000;
        req.Headers.Add("Accept-Language:zh-CN,zh;q=0.9");
        req.ContentType = "text/plain";
        req.Headers.Add("Cookie:" + CookieContainer);
        try
        {
            using (Stream reqStream = req.GetRequestStream( ))
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }
            Stream resStream = ((HttpWebResponse) req.GetResponse( )).GetResponseStream( );
            StreamReader reader = new(resStream, Encoding.GetEncoding("utf-8"));
            text = reader.ReadToEnd( );
            resStream.Close( );
            reader.Close( );
            req.Abort( );
        }
        catch { }
        return text;
    }

    private static string GetBaiduHtml(string url, CookieContainer cookie, string refer, string contentLength)
    {
        string result;
        try
        {
            string text = "";
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "POST";
            req.Referer = refer;
            req.Timeout = 1500;
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            byte[] bytes = Encoding.UTF8.GetBytes(contentLength);
            Stream reqStream = req.GetRequestStream( );
            reqStream.Write(bytes, 0, bytes.Length);
            reqStream.Close( );
            using (HttpWebResponse res = (HttpWebResponse) req.GetResponse( ))
            {
                using StreamReader reader = new(res.GetResponseStream( ), Encoding.UTF8);
                text = reader.ReadToEnd( );
                reader.Close( );
                res.Close( );
            }
            result = text;
        }
        catch
        {
            result = GetBaiduHtml(url, cookie, refer, contentLength);
        }
        return result;
    }
}
