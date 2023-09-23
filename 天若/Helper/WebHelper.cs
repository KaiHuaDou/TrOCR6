using System;
using System.IO;
using System.Net;
using System.Text;

namespace TrOCR.Helper;

public static class WebHelper
{
    public static string GetHtmlContent(string url)
    {
        try
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(new Uri(url));
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 5000;
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;*/*";
            httpWebRequest.UserAgent = "\r\nMozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36";
            HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( );
            return
                httpWebResponse.StatusCode == HttpStatusCode.OK
                ? new StreamReader(httpWebResponse.GetResponseStream( ), Encoding.UTF8).ReadToEnd( )
                : "";
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString( ));
            return string.Empty;
        }
    }
}
