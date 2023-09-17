using System;
using System.IO;
using System.Net;
using System.Text;

namespace TrOCR.Helper;

public class WebHelper
{
    public static string GetHtmlContent(string url)
    {
        string text;
        try
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(new Uri(url));
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 5000;
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;*/*";
            httpWebRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( );
            text = httpWebResponse.StatusCode == HttpStatusCode.OK
            ? new StreamReader(httpWebResponse.GetResponseStream( ), Encoding.UTF8).ReadToEnd( )
            : "";
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString( ));
            text = "";
        }
        return text;
    }
}
