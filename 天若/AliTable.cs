using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using mshtml;

namespace TrOCR
{
	public partial class AliTable : Form
	{
		public AliTable()
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
			this.InitializeComponent();
		}

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref int pcchCookieData, int dwFlags, object lpReserved);

		private string GetCookieString(string url)
		{
			int num = 256;
			StringBuilder stringBuilder = new StringBuilder(num);
			if (!AliTable.InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, null))
			{
				if (num < 0)
				{
					return null;
				}
				stringBuilder = new StringBuilder(num);
				if (!AliTable.InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, null))
				{
					return null;
				}
			}
			return stringBuilder.ToString();
		}

		private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			try
			{
				this.count++;
				this.textBox1.Text = this.GetCookieString(e.Url.ToString());
				this.webBrowser1.Document.Window.ScrollTo(10000, 145);
				this.webBrowser1.Document.Body.SetAttribute("scroll", "no");
				this.webBrowser1.Document.GetElementById("guid-762944").OuterHtml = "";
				if (this.count <= 10)
				{
					this.timer1.Interval = 500;
					this.timer1.Start();
				}
			}
			catch
			{
			}
		}

		private void Form2_Load(object sender, EventArgs e)
		{
			this.webBrowser1.Url = new Uri("https://data.aliyun.com/ai/ocr-other#/ocr-other");
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			if (this.textBox1.Text.Contains("login_aliyunid=\""))
			{
				this.webBrowser1.Url = new Uri("https://data.aliyun.com/ai/ocr-other#/ocr-other");
				IniHelp.SetValue("特殊", "ali_cookie", this.textBox1.Text);
				base.Hide();
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (this.webBrowser1.ReadyState == WebBrowserReadyState.Complete)
			{
				try
				{
					this.cclick();
				}
				catch
				{
				}
				if (this.count >= 2)
				{
					this.count = 0;
					base.Show();
				}
				this.timer1.Stop();
			}
		}

		public string getcookie
		{
			get
			{
				return this.textBox1.Text;
			}
			set
			{
				this.webBrowser1.Url = new Uri("https://data.aliyun.com/ai/ocr-other#/ocr-other");
			}
		}

		private bool ComposeEncrypt_onclick()
		{
			IHTMLDocument3 documentFromWindow = WebBrowserHelper.GetDocumentFromWindow(this.webBrowser1.Document.Window.Frames["alibaba-login-box"].DomWindow as IHTMLWindow2);
			string text = documentFromWindow.getElementById("fm-login-id").getAttribute("value", 0).ToString();
			string text2 = documentFromWindow.getElementById("fm-login-password").getAttribute("value", 0).ToString();
			IniHelp.SetValue("特殊", "ali_account", text);
			IniHelp.SetValue("特殊", "ali_password", text2);
			this.timer1.Stop();
			return true;
		}

		public void cclick()
		{
			try
			{
				if (IniHelp.GetValue("特殊", "ali_account").Trim() != "" && IniHelp.GetValue("特殊", "ali_password").Trim() != "")
				{
					WebBrowserHelper.GetDocumentFromWindow(this.webBrowser1.Document.Window.Frames["alibaba-login-box"].DomWindow as IHTMLWindow2).getElementById("fm-login-id").setAttribute("value", IniHelp.GetValue("特殊", "ali_account"), 1);
					WebBrowserHelper.GetDocumentFromWindow(this.webBrowser1.Document.Window.Frames["alibaba-login-box"].DomWindow as IHTMLWindow2).getElementById("fm-login-password").setAttribute("value", IniHelp.GetValue("特殊", "ali_password"), 1);
				}
			}
			catch
			{
			}
		}

		private int count;
	}
}
