using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

using mshtml;
using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class AliTable : Form
{
    public AliTable( )
    {
        string fileName = Path.GetFileName(Application.ExecutablePath);
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
        if (registryKey != null)
        {
            registryKey.SetValue(fileName, 11001, RegistryValueKind.DWord);
            registryKey.SetValue(fileName, 11001, RegistryValueKind.DWord);
        }
        registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
        if (registryKey != null)
        {
            registryKey.SetValue(fileName, 11001, RegistryValueKind.DWord);
            registryKey.SetValue(fileName, 11001, RegistryValueKind.DWord);
        }
        InitializeComponent( );
    }

    private static string GetCookieString(string url)
    {
        int num = 256;
        StringBuilder stringBuilder = new(num);
        if (!InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, null))
        {
            if (num < 0)
            {
                return null;
            }
            stringBuilder = new StringBuilder(num);
            if (!InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, null))
            {
                return null;
            }
        }
        return stringBuilder.ToString( );
    }

    private void BrowserLoaded(object o, WebBrowserDocumentCompletedEventArgs e)
    {
        try
        {
            count++;
            CookiesBox.Text = GetCookieString(e.Url.ToString( ));
            webBrowser1.Document.Window.ScrollTo(10000, 145);
            webBrowser1.Document.Body.SetAttribute("scroll", "no");
            webBrowser1.Document.GetElementById("guid-762944").OuterHtml = "";
            if (count <= 10)
            {
                timer1.Interval = 500;
                timer1.Start( );
            }
        }
        catch
        {
        }
    }

    private void FormLoad(object o, EventArgs e)
        => webBrowser1.Url = new Uri("https://data.aliyun.com/ai/ocr-other#/ocr-other");

    private void CookiesBoxTextChange(object o, EventArgs e)
    {
        if (CookiesBox.Text.Contains("login_aliyunid=\""))
        {
            webBrowser1.Url = new Uri("https://data.aliyun.com/ai/ocr-other#/ocr-other");
            Config.Set("特殊", "ali_cookie", CookiesBox.Text);
            Hide( );
        }
    }

    private void TimerTick(object o, EventArgs e)
    {
        if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
        {
            try
            {
                CClick( );
            }
            catch
            {
            }
            if (count >= 2)
            {
                count = 0;
                Show( );
            }
            timer1.Stop( );
        }
    }

    public string GetCookie
    {
        get => CookiesBox.Text;
        set => webBrowser1.Url = new Uri("https://data.aliyun.com/ai/ocr-other#/ocr-other");
    }

    public void CClick( )
    {
        try
        {
            if (!string.IsNullOrEmpty(Config.Get("特殊", "ali_account").Trim( )) && !string.IsNullOrEmpty(Config.Get("特殊", "ali_password").Trim( )))
            {
                Web.GetDocumentFromWindow(webBrowser1.Document.Window.Frames["alibaba-login-box"].DomWindow as IHTMLWindow2).getElementById("fm-login-id").setAttribute("value", Config.Get("特殊", "ali_account"), 1);
                Web.GetDocumentFromWindow(webBrowser1.Document.Window.Frames["alibaba-login-box"].DomWindow as IHTMLWindow2).getElementById("fm-login-password").setAttribute("value", Config.Get("特殊", "ali_password"), 1);
            }
        }
        catch { }
    }

    private int count;
}
