using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using CsharpHttpHelper;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MSScriptControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareX.ScreenCaptureLib;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace TrOCR
{
	public partial class FmMain : Form
	{
		public FmMain()
		{
			this.set_merge = false;
			this.set_split = false;
			this.set_split = false;
			StaticValue.截图排斥 = false;
			this.pinyin_flag = false;
			this.tranclick = false;
			this.are = new AutoResetEvent(false);
			this.imagelist = new List<Image>();
			StaticValue.v_notecount = Convert.ToInt32(IniHelp.GetValue("配置", "记录数目"));
			this.baidu_flags = "";
			this.esc = "";
			this.voice_count = 0;
			this.fmnote = new Fmnote();
			this.fmflags = new FmFlags();
			this.pubnote = new string[StaticValue.v_notecount];
			for (int i = 0; i < StaticValue.v_notecount; i++)
			{
				this.pubnote[i] = "";
			}
			StaticValue.v_note = this.pubnote;
			StaticValue.mainhandle = base.Handle;
			this.Font = new Font(this.Font.Name, 9f / StaticValue.Dpifactor, this.Font.Style, this.Font.Unit, this.Font.GdiCharSet, this.Font.GdiVerticalFont);
			this.googleTranslate_txt = "";
			this.num_ok = 0;
			this.F_factor = Program.factor;
			this.components = null;
			this.InitializeComponent();
			this.nextClipboardViewer = (IntPtr)HelpWin32.SetClipboardViewer((int)base.Handle);
			this.InitMinimize();
			this.readIniFile();
			base.WindowState = FormWindowState.Minimized;
			base.Visible = false;
			this.split_txt = "";
			this.MinimumSize = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
			FmMain.speak_copy = false;
			this.OCR_foreach("");
		}

		private void Load_Click(object sender, EventArgs e)
		{
			base.WindowState = FormWindowState.Minimized;
			base.Visible = false;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 953)
			{
				this.speaking = false;
			}
			if (m.Msg == 274 && (int)m.WParam == 61536)
			{
				base.WindowState = FormWindowState.Minimized;
				base.Visible = false;
				return;
			}
			if (m.Msg == 600 && (int)m.WParam == 725)
			{
				if (IniHelp.GetValue("工具栏", "顶置") == "True")
				{
					base.TopMost = true;
					return;
				}
				base.TopMost = false;
				return;
			}
			else
			{
				if (m.Msg == 786 && m.WParam.ToInt32() == 530 && this.RichBoxBody.Text != null)
				{
					this.p_note(this.RichBoxBody.Text);
					StaticValue.v_note = this.pubnote;
					if (this.fmnote.Created)
					{
						this.fmnote.Text_note = "";
					}
				}
				if (m.Msg == 786 && m.WParam.ToInt32() == 520)
				{
					this.fmnote.Show();
					this.fmnote.Focus();
					this.fmnote.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.fmnote.Width, Screen.PrimaryScreen.WorkingArea.Height - this.fmnote.Height);
					this.fmnote.WindowState = FormWindowState.Normal;
					return;
				}
				if (m.Msg == 786 && m.WParam.ToInt32() == 580)
				{
					HelpWin32.UnregisterHotKey(base.Handle, 205);
					this.change_QQ_screenshot = false;
					base.FormBorderStyle = FormBorderStyle.None;
					base.Hide();
					if (this.transtalate_fla == "开启")
					{
						this.form_width = base.Width / 2;
					}
					else
					{
						this.form_width = base.Width;
					}
					this.form_height = base.Height;
					this.minico.Visible = false;
					this.minico.Visible = true;
					this.menu.Close();
					this.menu_copy.Close();
					this.auto_fla = "开启";
					this.split_txt = "";
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.RichBoxBody_T.Text = "";
					this.typeset_txt = "";
					this.transtalate_fla = "关闭";
					this.Trans_close.PerformClick();
					base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
					base.FormBorderStyle = FormBorderStyle.Sizable;
					StaticValue.截图排斥 = true;
					this.image_screen = StaticValue.image_OCR;
					if (IniHelp.GetValue("工具栏", "分栏") == "True")
					{
						this.minico.Visible = true;
						this.thread = new Thread(new ThreadStart(this.ShowLoading));
						this.thread.Start();
						this.ts = new TimeSpan(DateTime.Now.Ticks);
						Image image = this.image_screen;
						Bitmap bitmap = new Bitmap(image.Width, image.Height);
						Graphics graphics = Graphics.FromImage(bitmap);
						graphics.DrawImage(image, 0, 0, image.Width, image.Height);
						graphics.Save();
						graphics.Dispose();
						this.image_ori = bitmap;
						((Bitmap)this.FindBundingBox_fences((Bitmap)image)).Save("Data\\分栏预览图.jpg");
					}
					else
					{
						this.minico.Visible = true;
						this.thread = new Thread(new ThreadStart(this.ShowLoading));
						this.thread.Start();
						this.ts = new TimeSpan(DateTime.Now.Ticks);
						Messageload messageload = new Messageload();
						messageload.ShowDialog();
						if (messageload.DialogResult == DialogResult.OK)
						{
							this.esc_thread = new Thread(new ThreadStart(this.Main_OCR_Thread));
							this.esc_thread.Start();
						}
					}
				}
				if (m.Msg == 786 && m.WParam.ToInt32() == 590 && this.speak_copyb == "朗读")
				{
					this.TTS();
					return;
				}
				if (m.Msg == 786 && m.WParam.ToInt32() == 511)
				{
					base.MinimumSize = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
					this.transtalate_fla = "关闭";
					this.RichBoxBody.Dock = DockStyle.Fill;
					this.RichBoxBody_T.Visible = false;
					this.PictureBox1.Visible = false;
					this.RichBoxBody_T.Text = "";
					if (base.WindowState == FormWindowState.Maximized)
					{
						base.WindowState = FormWindowState.Normal;
					}
					base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
				}
				if (m.Msg == 786 && m.WParam.ToInt32() == 512)
				{
					this.transtalate_Click();
				}
				if (m.Msg == 786 && m.WParam.ToInt32() == 518)
				{
					if (base.ActiveControl.Name == "htmlTextBoxBody")
					{
						this.htmltxt = this.RichBoxBody.Text;
					}
					if (base.ActiveControl.Name == "rich_trans")
					{
						this.htmltxt = this.RichBoxBody_T.Text;
					}
					if (this.htmltxt == "")
					{
						return;
					}
					this.TTS();
				}
				if (m.Msg == 161)
				{
					HelpWin32.SetForegroundWindow(base.Handle);
					base.WndProc(ref m);
					return;
				}
				if (m.Msg != 163)
				{
					if (m.Msg == 786 && m.WParam.ToInt32() == 222)
					{
						try
						{
							StaticValue.截图排斥 = false;
							this.esc = "退出";
							this.fmloading.fml_close = "窗体已关闭";
							this.esc_thread.Abort();
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message);
						}
						base.FormBorderStyle = FormBorderStyle.Sizable;
						base.Visible = true;
						base.Show();
						base.WindowState = FormWindowState.Normal;
						if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
						{
							string value = IniHelp.GetValue("快捷键", "翻译文本");
							string text = "None";
							string text2 = "F9";
							this.SetHotkey(text, text2, value, 205);
						}
						HelpWin32.UnregisterHotKey(base.Handle, 222);
					}
					if (m.Msg == 786 && m.WParam.ToInt32() == 200)
					{
						HelpWin32.UnregisterHotKey(base.Handle, 205);
						this.menu.Hide();
						this.RichBoxBody.Hide = "";
						this.RichBoxBody_T.Hide = "";
						this.Main_OCR_Quickscreenshots();
					}
					if (m.Msg == 786 && m.WParam.ToInt32() == 206)
					{
						if (!this.fmnote.Visible || base.Focused)
						{
							this.fmnote.Show();
							this.fmnote.WindowState = FormWindowState.Normal;
							this.fmnote.Visible = true;
						}
						else
						{
							this.fmnote.Hide();
							this.fmnote.WindowState = FormWindowState.Minimized;
							this.fmnote.Visible = false;
						}
					}
					if (m.Msg == 786 && m.WParam.ToInt32() == 235)
					{
						if (!base.Visible)
						{
							base.TopMost = true;
							base.Show();
							base.WindowState = FormWindowState.Normal;
							base.Visible = true;
							Thread.Sleep(100);
							if (IniHelp.GetValue("工具栏", "顶置") == "False")
							{
								base.TopMost = false;
								return;
							}
						}
						else
						{
							base.Hide();
							base.Visible = false;
						}
					}
					if (m.Msg == 786 && m.WParam.ToInt32() == 205)
					{
						this.翻译文本();
					}
					base.WndProc(ref m);
					return;
				}
				if (this.transtalate_fla == "开启")
				{
					base.WindowState = FormWindowState.Normal;
					base.Size = new Size((int)this.font_base.Width * 23 * 2, (int)this.font_base.Height * 24);
					base.Location = (Point)new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10 * 2, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
					return;
				}
				base.WindowState = FormWindowState.Normal;
				base.Location = (Point)new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
				base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
				return;
			}
		}

		private void Form1_FormClosing(object sender, FormClosedEventArgs e)
		{
			base.WindowState = FormWindowState.Minimized;
			base.Visible = false;
		}

		public void InitMinimize()
		{
			MenuItem menuItem = new MenuItem();
			MenuItem menuItem2 = new MenuItem();
			new MenuItem();
			MenuItem menuItem3 = new MenuItem();
			MenuItem menuItem4 = new MenuItem();
			new MenuItem();
			MenuItem menuItem5 = new MenuItem();
			try
			{
				MenuItem[] array = new MenuItem[] { menuItem, menuItem2, menuItem3, menuItem5, menuItem4 };
				menuItem.Text = "显示";
				menuItem.Click += this.tray_show_Click;
				menuItem2.Text = "设置";
				menuItem2.Click += this.tray_Set_Click;
				menuItem3.Text = "更新";
				menuItem3.Click += this.tray_update_Click;
				menuItem5.Text = "帮助";
				menuItem5.Click += this.tray_help_Click;
				menuItem4.Text = "退出";
				menuItem4.Click += this.tray_exit_Click;
				this.minico.ContextMenu = new ContextMenu(array);
			}
			catch (Exception ex)
			{
				MessageBox.Show("InitMinimize()" + ex.Message);
			}
		}

		private void tray_show_Click(object sender, EventArgs e)
		{
			base.Show();
			base.Activate();
			base.Visible = true;
			base.WindowState = FormWindowState.Normal;
			if (IniHelp.GetValue("工具栏", "顶置") == "True")
			{
				base.TopMost = true;
				return;
			}
			base.TopMost = false;
		}

		private void tray_exit_Click(object sender, EventArgs e)
		{
			this.minico.Dispose();
			this.saveIniFile();
			Process.GetCurrentProcess().Kill();
		}

		private void setting_Click(object sender, EventArgs e)
		{
		}

		private void Form1_LostFocus(object sender, EventArgs e)
		{
		}

		private void Main_copy_Click(object sender, EventArgs e)
		{
			this.RichBoxBody.Focus();
			this.RichBoxBody.richTextBox1.Copy();
		}

		public static string punctuation_en_ch(string text)
		{
			char[] array = text.ToCharArray();
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

		private void Main_SelectAll_Click(object sender, EventArgs e)
		{
			this.RichBoxBody.Focus();
			this.RichBoxBody.richTextBox1.SelectAll();
		}

		private void Main_paste_Click(object sender, EventArgs e)
		{
			this.RichBoxBody.Focus();
			this.RichBoxBody.richTextBox1.Paste();
		}

		public void Split_Click(object sender, EventArgs e)
		{
			this.RichBoxBody.Text = this.split_txt;
		}

		public static byte[] copybyte(byte[] a, byte[] b)
		{
			byte[] array = new byte[a.Length + b.Length];
			a.CopyTo(array, 0);
			b.CopyTo(array, a.Length);
			return array;
		}

		public void OCR_sougou()
		{
			try
			{
				this.split_txt = "";
				Image image = this.image_screen;
				int i = image.Width;
				int j = image.Height;
				if (i < 300)
				{
					while (i < 300)
					{
						j *= 2;
						i *= 2;
					}
				}
				if (j < 120)
				{
					while (j < 120)
					{
						j *= 2;
						i *= 2;
					}
				}
				Bitmap bitmap = new Bitmap(i, j);
				Graphics graphics = Graphics.FromImage(bitmap);
				graphics.DrawImage(image, 0, 0, i, j);
				graphics.Save();
				graphics.Dispose();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(this.OCR_sougou_SogouOCR(bitmap)))["result"].ToString());
				bitmap.Dispose();
				this.checked_txt(jarray, 2, "content");
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public byte[] ImageTobyte(Image imgPhoto)
		{
			MemoryStream memoryStream = new MemoryStream();
			imgPhoto.Save(memoryStream, ImageFormat.Jpeg);
			byte[] array = new byte[memoryStream.Length];
			memoryStream.Position = 0L;
			memoryStream.Read(array, 0, array.Length);
			memoryStream.Close();
			return array;
		}

		private Bitmap GetPlus(Bitmap bm, double times)
		{
			int num = (int)((double)bm.Width / times);
			int num2 = (int)((double)bm.Height / times);
			Bitmap bitmap = new Bitmap(num, num2);
			if (times >= 1.0 && times <= 1.1)
			{
				bitmap = bm;
			}
			else
			{
				Graphics graphics = Graphics.FromImage(bitmap);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.DrawImage(bm, new Rectangle(0, 0, num, num2), new Rectangle(0, 0, bm.Width, bm.Height), GraphicsUnit.Pixel);
				graphics.Dispose();
			}
			return bitmap;
		}

		public void OCR_Tencent()
		{
			try
			{
				this.split_txt = "";
				string text = "------WebKitFormBoundaryRDEqU0w702X9cWPJ";
				Image image = this.image_screen;
				if (image.Width > 90 && image.Height < 90)
				{
					Bitmap bitmap = new Bitmap(image.Width, 300);
					Graphics graphics = Graphics.FromImage(bitmap);
					graphics.DrawImage(image, 5, 0, image.Width, image.Height);
					graphics.Save();
					graphics.Dispose();
					image = new Bitmap(bitmap);
				}
				else if (image.Width <= 90 && image.Height >= 90)
				{
					Bitmap bitmap2 = new Bitmap(300, image.Height);
					Graphics graphics2 = Graphics.FromImage(bitmap2);
					graphics2.DrawImage(image, 0, 5, image.Width, image.Height);
					graphics2.Save();
					graphics2.Dispose();
					image = new Bitmap(bitmap2);
				}
				else if (image.Width < 90 && image.Height < 90)
				{
					Bitmap bitmap3 = new Bitmap(300, 300);
					Graphics graphics3 = Graphics.FromImage(bitmap3);
					graphics3.DrawImage(image, 5, 5, image.Width, image.Height);
					graphics3.Save();
					graphics3.Dispose();
					image = new Bitmap(bitmap3);
				}
				else
				{
					image = this.image_screen;
				}
				byte[] array = this.OCR_ImgToByte(image);
				string text2 = text + "\r\nContent-Disposition: form-data; name=\"image_file\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
				string text3 = "\r\n" + text + "--\r\n";
				byte[] bytes = Encoding.ASCII.GetBytes(text2);
				byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
				byte[] array2 = FmMain.Mergebyte(bytes, array, bytes2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://ai.qq.com/cgi-bin/appdemo_generalocr");
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = "http://ai.qq.com/product/ocr.shtml";
				httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
				httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
				httpWebRequest.Timeout = 8000;
				httpWebRequest.ReadWriteTimeout = 2000;
				byte[] array3 = array2;
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(array3, 0, array2.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text4))["data"]["item_list"].ToString());
				this.checked_txt(jarray, 1, "itemstring");
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public void OCR_baidu_bak()
		{
			this.split_txt = "";
			try
			{
				string text = "CHN_ENG";
				this.split_txt = "";
				Image image = this.image_screen;
				MemoryStream memoryStream = new MemoryStream();
				image.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				if (this.interface_flag == "中英")
				{
					text = "CHN_ENG";
				}
				if (this.interface_flag == "日语")
				{
					text = "JAP";
				}
				if (this.interface_flag == "韩语")
				{
					text = "KOR";
				}
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string text5 = "";
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					char[] array2 = jobject["words"].ToString().ToCharArray();
					if (!char.IsPunctuation(array2[array2.Length - 1]))
					{
						if (!FmMain.contain_ch(jobject["words"].ToString()))
						{
							text5 = text5 + jobject["words"].ToString().Trim() + " ";
						}
						else
						{
							text5 += jobject["words"].ToString();
						}
					}
					else if (this.own_punctuation(array2[array2.Length - 1].ToString()))
					{
						if (!FmMain.contain_ch(jobject["words"].ToString()))
						{
							text5 = text5 + jobject["words"].ToString().Trim() + " ";
						}
						else
						{
							text5 += jobject["words"].ToString();
						}
					}
					else
					{
						text5 = text5 + jobject["words"] + "\r\n";
					}
					text4 = text4 + jobject["words"] + "\r\n";
				}
				this.split_txt = text4;
				this.typeset_txt = text5;
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		private void OCR_sougou_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("搜狗");
		}

		private void OCR_tencent_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("腾讯");
		}

		private void OCR_baidu_Click(object sender, EventArgs e)
		{
		}

		public void OCR_youdao()
		{
			try
			{
				this.split_txt = "";
				Image image = this.image_screen;
				if (image.Width > 90 && image.Height < 90)
				{
					Bitmap bitmap = new Bitmap(image.Width, 200);
					Graphics graphics = Graphics.FromImage(bitmap);
					graphics.DrawImage(image, 5, 0, image.Width, image.Height);
					graphics.Save();
					graphics.Dispose();
					image = new Bitmap(bitmap);
				}
				else if (image.Width <= 90 && image.Height >= 90)
				{
					Bitmap bitmap2 = new Bitmap(200, image.Height);
					Graphics graphics2 = Graphics.FromImage(bitmap2);
					graphics2.DrawImage(image, 0, 5, image.Width, image.Height);
					graphics2.Save();
					graphics2.Dispose();
					image = new Bitmap(bitmap2);
				}
				else if (image.Width < 90 && image.Height < 90)
				{
					Bitmap bitmap3 = new Bitmap(200, 200);
					Graphics graphics3 = Graphics.FromImage(bitmap3);
					graphics3.DrawImage(image, 5, 5, image.Width, image.Height);
					graphics3.Save();
					graphics3.Dispose();
					image = new Bitmap(bitmap3);
				}
				else
				{
					image = this.image_screen;
				}
				int i = image.Width;
				int j = image.Height;
				if (i < 600)
				{
					while (i < 600)
					{
						j *= 2;
						i *= 2;
					}
				}
				if (j < 120)
				{
					while (j < 120)
					{
						j *= 2;
						i *= 2;
					}
				}
				Bitmap bitmap4 = new Bitmap(i, j);
				Graphics graphics4 = Graphics.FromImage(bitmap4);
				graphics4.DrawImage(image, 0, 0, i, j);
				graphics4.Save();
				graphics4.Dispose();
				image = new Bitmap(bitmap4);
				byte[] array = this.OCR_ImgToByte(image);
				string text = "imgBase=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&lang=auto&company=";
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://aidemo.youdao.com/ocrapi1");
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = "http://aidemo.youdao.com/ocrdemo";
				httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest.Timeout = 8000;
				httpWebRequest.ReadWriteTimeout = 2000;
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text2 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text2))["lines"].ToString());
				this.checked_txt(jarray, 1, "words");
				image.Dispose();
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public void OCR_youdao_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("有道");
		}

		public void change_Chinese_Click(object sender, EventArgs e)
		{
			this.language = "中文标点";
			if (this.typeset_txt != "")
			{
				this.RichBoxBody.Text = FmMain.punctuation_en_ch_x(this.RichBoxBody.Text);
				this.RichBoxBody.Text = this.punctuation_quotation(this.RichBoxBody.Text);
			}
		}

		public void change_English_Click(object sender, EventArgs e)
		{
			this.language = "英文标点";
			if (this.typeset_txt != "")
			{
				this.RichBoxBody.Text = FmMain.punctuation_ch_en(this.RichBoxBody.Text);
			}
		}

		public static string punctuation_ch_en(string text)
		{
			char[] array = text.ToCharArray();
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

		public void saveIniFile()
		{
			IniHelp.SetValue("配置", "接口", this.interface_flag);
		}

		public void readIniFile()
		{
			this.Proxy_flag = IniHelp.GetValue("代理", "代理类型");
			this.Proxy_url = IniHelp.GetValue("代理", "服务器");
			this.Proxy_port = IniHelp.GetValue("代理", "端口");
			this.Proxy_name = IniHelp.GetValue("代理", "服务器账号");
			this.Proxy_password = IniHelp.GetValue("代理", "服务器密码");
			if (this.Proxy_flag == "不使用代理")
			{
				WebRequest.DefaultWebProxy = null;
			}
			if (this.Proxy_flag == "系统代理")
			{
				WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
			}
			if (this.Proxy_flag == "自定义代理")
			{
				try
				{
					WebProxy webProxy = new WebProxy(this.Proxy_url, Convert.ToInt32(this.Proxy_port));
					if (this.Proxy_name != "" && this.Proxy_password != "")
					{
						ICredentials credentials = new NetworkCredential(this.Proxy_name, this.Proxy_password);
						webProxy.Credentials = credentials;
					}
					WebRequest.DefaultWebProxy = webProxy;
				}
				catch
				{
					MessageBox.Show("请检查代理设置！");
				}
			}
			this.interface_flag = IniHelp.GetValue("配置", "接口");
			if (this.interface_flag == "发生错误")
			{
				IniHelp.SetValue("配置", "接口", "搜狗");
				this.OCR_foreach("搜狗");
			}
			else
			{
				this.OCR_foreach(this.interface_flag);
			}
			string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
			if (IniHelp.GetValue("快捷键", "文字识别") != "请按下快捷键")
			{
				string value = IniHelp.GetValue("快捷键", "文字识别");
				string text2 = "None";
				string text3 = "F4";
				this.SetHotkey(text2, text3, value, 200);
			}
			if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
			{
				string value2 = IniHelp.GetValue("快捷键", "翻译文本");
				string text4 = "None";
				string text5 = "F7";
				this.SetHotkey(text4, text5, value2, 205);
			}
			if (IniHelp.GetValue("快捷键", "记录界面") != "请按下快捷键")
			{
				string value3 = IniHelp.GetValue("快捷键", "记录界面");
				string text6 = "None";
				string text7 = "F8";
				this.SetHotkey(text6, text7, value3, 206);
			}
			if (IniHelp.GetValue("快捷键", "识别界面") != "请按下快捷键")
			{
				string value4 = IniHelp.GetValue("快捷键", "识别界面");
				string text8 = "None";
				string text9 = "F11";
				this.SetHotkey(text8, text9, value4, 235);
			}
			StaticValue.baiduAPI_ID = HelpWin32.IniFileHelper.GetValue("密钥_百度", "secret_id", text);
			if (HelpWin32.IniFileHelper.GetValue("密钥_百度", "secret_id", text) == "发生错误")
			{
				StaticValue.baiduAPI_ID = "请输入secret_id";
			}
			StaticValue.baiduAPI_key = HelpWin32.IniFileHelper.GetValue("密钥_百度", "secret_key", text);
			if (HelpWin32.IniFileHelper.GetValue("密钥_百度", "secret_key", text) == "发生错误")
			{
				StaticValue.baiduAPI_key = "请输入secret_key";
			}
		}

		public static string check_ch_en(string text)
		{
			char[] array = text.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				int num = "：".IndexOf(array[i]);
				if (num != -1 && i - 1 >= 0 && i + 1 < array.Length && FmMain.contain_en(array[i - 1].ToString()) && FmMain.contain_en(array[i + 1].ToString()))
				{
					array[i] = ":"[num];
				}
				if (num != -1 && i - 1 >= 0 && i + 1 < array.Length && FmMain.contain_en(array[i - 1].ToString()) && FmMain.contain_punctuation(array[i + 1].ToString()))
				{
					array[i] = ":"[num];
				}
			}
			return new string(array);
		}

		public void tray_Set_Click(object sender, EventArgs e)
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			HelpWin32.UnregisterHotKey(base.Handle, 200);
			HelpWin32.UnregisterHotKey(base.Handle, 205);
			HelpWin32.UnregisterHotKey(base.Handle, 206);
			HelpWin32.UnregisterHotKey(base.Handle, 235);
			base.WindowState = FormWindowState.Minimized;
			FmSetting fmSetting = new FmSetting();
			fmSetting.TopMost = true;
			fmSetting.ShowDialog();
			if (fmSetting.DialogResult == DialogResult.OK)
			{
				string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
				StaticValue.v_notecount = Convert.ToInt32(HelpWin32.IniFileHelper.GetValue("配置", "记录数目", text));
				this.pubnote = new string[StaticValue.v_notecount];
				for (int i = 0; i < StaticValue.v_notecount; i++)
				{
					this.pubnote[i] = "";
				}
				StaticValue.v_note = this.pubnote;
				this.fmnote.Text_note_change = "";
				this.fmnote.Location = new Point(Screen.AllScreens[0].WorkingArea.Width - this.fmnote.Width, Screen.AllScreens[0].WorkingArea.Height - this.fmnote.Height);
				if (IniHelp.GetValue("快捷键", "文字识别") != "请按下快捷键")
				{
					string value = IniHelp.GetValue("快捷键", "文字识别");
					string text2 = "None";
					string text3 = "F4";
					this.SetHotkey(text2, text3, value, 200);
				}
				if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
				{
					string value2 = IniHelp.GetValue("快捷键", "翻译文本");
					string text4 = "None";
					string text5 = "F9";
					this.SetHotkey(text4, text5, value2, 205);
				}
				if (IniHelp.GetValue("快捷键", "记录界面") != "请按下快捷键")
				{
					string value3 = IniHelp.GetValue("快捷键", "记录界面");
					string text6 = "None";
					string text7 = "F8";
					this.SetHotkey(text6, text7, value3, 206);
				}
				if (IniHelp.GetValue("快捷键", "识别界面") != "请按下快捷键")
				{
					string value4 = IniHelp.GetValue("快捷键", "识别界面");
					string text8 = "None";
					string text9 = "F11";
					this.SetHotkey(text8, text9, value4, 235);
				}
				this.Proxy_flag = IniHelp.GetValue("代理", "代理类型");
				this.Proxy_url = IniHelp.GetValue("代理", "服务器");
				this.Proxy_port = IniHelp.GetValue("代理", "端口");
				this.Proxy_name = IniHelp.GetValue("代理", "服务器账号");
				this.Proxy_password = IniHelp.GetValue("代理", "服务器密码");
				StaticValue.baiduAPI_ID = IniHelp.GetValue("密钥_百度", "secret_id");
				StaticValue.baiduAPI_key = IniHelp.GetValue("密钥_百度", "secret_key");
				if (this.Proxy_flag == "不使用代理")
				{
					WebRequest.DefaultWebProxy = null;
				}
				if (this.Proxy_flag == "系统代理")
				{
					WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
				}
				if (this.Proxy_flag == "自定义代理")
				{
					try
					{
						WebProxy webProxy = new WebProxy(this.Proxy_url, Convert.ToInt32(this.Proxy_port));
						if (this.Proxy_name != "" && this.Proxy_password != "")
						{
							ICredentials credentials = new NetworkCredential(this.Proxy_name, this.Proxy_password);
							webProxy.Credentials = credentials;
						}
						WebRequest.DefaultWebProxy = webProxy;
					}
					catch
					{
						MessageBox.Show("请检查代理设置！");
					}
				}
				if (IniHelp.GetValue("更新", "更新间隔") == "True")
				{
					Program.checkTimer.Enabled = true;
					Program.checkTimer.Interval = 3600000.0 * (double)Convert.ToInt32(IniHelp.GetValue("更新", "间隔时间"));
					Program.checkTimer.Elapsed += Program.CheckTimer_Elapsed;
					Program.checkTimer.Start();
				}
			}
		}

		public void tray_limit_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(this.about)).Start();
		}

		public static bool IsNum(string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] < '0' || str[i] > '9')
				{
					return false;
				}
			}
			return true;
		}

		public bool own_punctuation(string text)
		{
			return ",;，、<>《》()-（）.。".IndexOf(text) != -1;
		}

		public static string punctuation_Del_space(string text)
		{
			string text2 = "(?<=.)([^\\*]+)(?=.)";
			string text3;
			if (Regex.Match(text, text2).ToString().IndexOf(" ") >= 0)
			{
				text = Regex.Replace(text, "(?<=[\\p{P}*])([a-zA-Z])(?=[a-zA-Z])", " $1");
				char[] array = null;
				text = text.TrimEnd(array).Replace("- ", "-").Replace("_ ", "_")
					.Replace("( ", "(")
					.Replace("/ ", "/")
					.Replace("\" ", "\"");
				text3 = text;
			}
			else
			{
				text3 = text;
			}
			return text3;
		}

		public static bool contain_ch(string str)
		{
			return Regex.IsMatch(str, "[\\u4e00-\\u9fa5]");
		}

		public void transtalate_Click()
		{
			this.typeset_txt = this.RichBoxBody.Text;
			this.RichBoxBody_T.Visible = true;
			base.WindowState = FormWindowState.Normal;
			this.transtalate_fla = "开启";
			this.RichBoxBody.Dock = DockStyle.None;
			this.RichBoxBody_T.Dock = DockStyle.None;
			this.RichBoxBody_T.BorderStyle = BorderStyle.Fixed3D;
			this.RichBoxBody_T.Text = "";
			this.RichBoxBody.Focus();
			if (this.num_ok == 0)
			{
				this.RichBoxBody.Size = new Size(base.ClientRectangle.Width, base.ClientRectangle.Height);
				base.Size = new Size(this.RichBoxBody.Width * 2, this.RichBoxBody.Height);
				this.RichBoxBody_T.Size = new Size(this.RichBoxBody.Width, this.RichBoxBody.Height);
				this.RichBoxBody_T.Location = (Point)new Size(this.RichBoxBody.Width, 0);
				this.RichBoxBody_T.Name = "rich_trans";
				this.RichBoxBody_T.TabIndex = 1;
				this.RichBoxBody_T.Text_flag = "我是翻译文本框";
				this.RichBoxBody_T.ImeMode = ImeMode.On;
			}
			this.num_ok++;
			this.PictureBox1.Visible = true;
			this.PictureBox1.BringToFront();
			this.MinimumSize = new Size((int)this.font_base.Width * 23 * 2, (int)this.font_base.Height * 24);
			base.Size = new Size((int)this.font_base.Width * 23 * 2, (int)this.font_base.Height * 24);
			Control.CheckForIllegalCrossThreadCalls = false;
			new Thread(new ThreadStart(this.trans_Calculate)).Start();
		}

		private void Form_Resize(object sender, EventArgs e)
		{
			if (this.RichBoxBody.Dock != DockStyle.Fill)
			{
				this.RichBoxBody.Size = new Size(base.ClientRectangle.Width / 2, base.ClientRectangle.Height);
				this.RichBoxBody_T.Size = new Size(this.RichBoxBody.Width, base.ClientRectangle.Height);
				this.RichBoxBody_T.Location = (Point)new Size(this.RichBoxBody.Width, 0);
			}
		}

		public void Trans_copy_Click(object sender, EventArgs e)
		{
			this.RichBoxBody_T.Focus();
			this.RichBoxBody_T.richTextBox1.Copy();
		}

		public void Trans_paste_Click(object sender, EventArgs e)
		{
			this.RichBoxBody_T.Focus();
			this.RichBoxBody_T.richTextBox1.Paste();
		}

		public void Trans_SelectAll_Click(object sender, EventArgs e)
		{
			this.RichBoxBody_T.Focus();
			this.RichBoxBody_T.richTextBox1.SelectAll();
		}

		public void trans_Calculate()
		{
			if (this.pinyin_flag)
			{
				this.googleTranslate_txt = FmMain.PinYin.ToPinYin(this.typeset_txt);
			}
			else if (this.typeset_txt == "")
			{
				this.googleTranslate_txt = "";
			}
			else
			{
				if (this.interface_flag == "韩语")
				{
					StaticValue.zh_en = false;
					StaticValue.zh_jp = false;
					StaticValue.zh_ko = true;
					this.RichBoxBody_T.set_language = "韩语";
				}
				if (this.interface_flag == "日语")
				{
					StaticValue.zh_en = false;
					StaticValue.zh_jp = true;
					StaticValue.zh_ko = false;
					this.RichBoxBody_T.set_language = "日语";
				}
				if (this.interface_flag == "中英")
				{
					StaticValue.zh_en = true;
					StaticValue.zh_jp = false;
					StaticValue.zh_ko = false;
					this.RichBoxBody_T.set_language = "中英";
				}
				if (IniHelp.GetValue("配置", "翻译接口") == "谷歌")
				{
					this.googleTranslate_txt = this.Translate_Google(this.typeset_txt);
				}
				if (IniHelp.GetValue("配置", "翻译接口") == "百度")
				{
					this.googleTranslate_txt = this.Translate_baidu(this.typeset_txt);
				}
				if (IniHelp.GetValue("配置", "翻译接口") == "腾讯")
				{
					this.googleTranslate_txt = this.Translate_Tencent(this.typeset_txt);
				}
			}
			this.PictureBox1.Visible = false;
			this.PictureBox1.SendToBack();
			base.Invoke(new FmMain.translate(this.translate_child));
			this.pinyin_flag = false;
		}

		public void Trans_close_Click(object sender, EventArgs e)
		{
			base.MinimumSize = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
			this.transtalate_fla = "关闭";
			this.RichBoxBody.Dock = DockStyle.Fill;
			this.RichBoxBody_T.Visible = false;
			this.PictureBox1.Visible = false;
			this.RichBoxBody_T.Text = "";
			if (base.WindowState == FormWindowState.Maximized)
			{
				base.WindowState = FormWindowState.Normal;
			}
			base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
		}

		private void ShowLoading()
		{
			try
			{
				this.fmloading = new Fmloading();
				Application.Run(this.fmloading);
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				this.thread.Abort();
			}
		}

		public bool contain(string text, string subStr)
		{
			return text.Contains(subStr);
		}

		public static bool contain_en(string str)
		{
			return Regex.IsMatch(str, "[a-zA-Z]");
		}

		public static bool punctuation_has_punctuation(string str)
		{
			string text;
			if (FmMain.contain_ch(str))
			{
				text = "[\\；\\，\\。\\！\\？]";
			}
			else
			{
				text = "[\\;\\,\\.\\!\\?]";
			}
			return Regex.IsMatch(str, text);
		}

		public string quotation(string str)
		{
			return Regex.Replace(str.Replace("“", "\"").Replace("”", "\""), "(?<=\")([^\\\"\\“\\”]+)(?=\")", "$1_测_$2");
		}

		private string punctuation_quotation(string pStr)
		{
			pStr = pStr.Replace("“", "\"").Replace("”", "\"");
			string[] array = pStr.Split(new char[] { '"' });
			string text = "";
			for (int i = 1; i <= array.Length; i++)
			{
				if (i % 2 == 0)
				{
					text = text + array[i - 1] + "”";
				}
				else
				{
					text = text + array[i - 1] + "“";
				}
			}
			return text.Substring(0, text.Length - 1);
		}

		public static bool HasenPunctuation(string str)
		{
			string text = "[\\;\\,\\.\\!\\?]";
			return Regex.IsMatch(str, text);
		}

		public static string Del_Space(string text)
		{
			text = Regex.Replace(text, "([\\p{P}]+)", "**&&**$1**&&**");
			char[] array = null;
			text = text.TrimEnd(array).Replace(" **&&**", "").Replace("**&&** ", "")
				.Replace("**&&**", "");
			return text;
		}

		public void TTS()
		{
			new Thread(new ThreadStart(this.TTS_thread)).Start();
		}

		public void about()
		{
			base.WindowState = FormWindowState.Minimized;
			Control.CheckForIllegalCrossThreadCalls = false;
			new Thread(new ThreadStart(this.ThreadFun)).Start();
		}

		private void ThreadFun()
		{
		}

		private void translate_child()
		{
			this.RichBoxBody_T.Text = this.googleTranslate_txt;
			this.googleTranslate_txt = "";
		}

		public void TTS_thread()
		{
			try
			{
				Stream responseStream = ((HttpWebResponse)((HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", "http://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=iQekhH39WqHoxur5ss59GpU4&client_secret=8bcee1cee76ed60cdfaed1f2c038584d"))).GetResponse()).GetResponseStream();
				string text = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				string text2;
				if (!FmMain.contain_ch(this.htmltxt))
				{
					text2 = "zh";
				}
				else
				{
					text2 = "zh";
				}
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Concat(new string[]
				{
					"http://tsn.baidu.com/text2audio?lan=" + text2 + "&ctp=1&cuid=abcdxxx&tok=",
					((JObject)JsonConvert.DeserializeObject(text))["access_token"].ToString(),
					"&tex=",
					HttpUtility.UrlEncode(this.htmltxt.Replace("***", "")),
					"&vol=9&per=0&spd=5&pit=5"
				}));
				httpWebRequest.Method = "POST";
				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				byte[] array = new byte[16384];
				byte[] array2;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					int num;
					while ((num = httpWebResponse.GetResponseStream().Read(array, 0, array.Length)) > 0)
					{
						memoryStream.Write(array, 0, num);
					}
					array2 = memoryStream.ToArray();
				}
				this.ttsData = array2;
				if (this.speak_copyb == "朗读" || this.voice_count == 0)
				{
					base.Invoke(new FmMain.translate(this.Speak_child));
					this.speak_copyb = "";
				}
				else
				{
					base.Invoke(new FmMain.translate(this.TTS_child));
				}
				this.voice_count++;
			}
			catch (Exception ex)
			{
				if (ex.ToString().IndexOf("Null") <= -1)
				{
					MessageBox.Show("文本过长，请使用右键菜单中的选中朗读！", "提醒");
				}
			}
		}

		public void TTS_child()
		{
			if (this.RichBoxBody.Text != null || this.RichBoxBody_T.Text != "")
			{
				if (this.speaking)
				{
					HelpWin32.mciSendString("close media", null, 0, IntPtr.Zero);
					this.speaking = false;
					return;
				}
				string tempPath = Path.GetTempPath();
				string text = tempPath + "\\声音.mp3";
				try
				{
					File.WriteAllBytes(text, this.ttsData);
				}
				catch
				{
					text = tempPath + "\\声音1.mp3";
					File.WriteAllBytes(text, this.ttsData);
				}
				this.PlaySong(text);
				this.speaking = true;
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 134217728;
				return createParams;
			}
		}

		public string GetGoogletHtml(string url, CookieContainer cookie, string refer)
		{
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "GET";
			httpWebRequest.CookieContainer = cookie;
			httpWebRequest.Referer = refer;
			httpWebRequest.Timeout = 5000;
			httpWebRequest.Headers.Add("X-Requested-With:XMLHttpRequest");
			httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
			httpWebRequest.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 65.0.3325.146 Safari / 537.36";
			string text2;
			try
			{
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				text2 = text;
			}
			catch
			{
				text2 = null;
			}
			return text2;
		}

		public string Translate_Google(string text)
		{
			string text2 = "";
			try
			{
				string text3 = "zh-CN";
				string text4 = "en";
				if (StaticValue.zh_en)
				{
					if (this.ch_count(text.Trim()) > this.en_count(text.Trim()) || (this.en_count(text.Trim()) == 1 && this.ch_count(text.Trim()) == 1))
					{
						text3 = "zh-CN";
						text4 = "en";
					}
					else
					{
						text3 = "en";
						text4 = "zh-CN";
					}
				}
				if (StaticValue.zh_jp)
				{
					if (FmMain.contain_jap(FmMain.repalceStr(FmMain.Del_ch(text.Trim()))))
					{
						text3 = "ja";
						text4 = "zh-CN";
					}
					else
					{
						text3 = "zh-CN";
						text4 = "ja";
					}
				}
				if (StaticValue.zh_ko)
				{
					if (FmMain.contain_kor(text.Trim()))
					{
						text3 = "ko";
						text4 = "zh-CN";
					}
					else
					{
						text3 = "zh-CN";
						text4 = "ko";
					}
				}
				HttpHelper httpHelper = new HttpHelper();
				HttpItem httpItem = new HttpItem
				{
					URL = "https://translate.googleapis.com/translate_a/single",
					Method = "POST",
					ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
					Postdata = string.Concat(new string[]
					{
						"client=gtx&sl=",
						text3,
						"&tl=",
						text4,
						"&dt=t&q=",
						HttpUtility.UrlEncode(text).Replace("+", "%20")
					}),
					UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.2228.0 Safari/537.36",
					Accept = "*/*"
				};
				JArray jarray = (JArray)JsonConvert.DeserializeObject(httpHelper.GetHtml(httpItem).Html);
				int count = ((JArray)jarray[0]).Count;
				for (int i = 0; i < count; i++)
				{
					text2 += jarray[0][i][0].ToString();
				}
			}
			catch (Exception)
			{
				text2 = "[谷歌接口报错]：\r\n1.网络错误或者文本过长。\r\n2.谷歌接口可能对于某些网络不能用，具体不清楚。可以尝试挂VPN试试。\r\n3.这个问题我没办法修复，请右键菜单更换百度、腾讯翻译接口。";
			}
			return text2;
		}

		public static string CookieCollectionToStrCookie(CookieCollection cookie)
		{
			string text;
			if (cookie == null)
			{
				text = string.Empty;
			}
			else
			{
				string text2 = string.Empty;
				foreach (object obj in cookie)
				{
					Cookie cookie2 = (Cookie)obj;
					text2 += string.Format("{0}={1};", cookie2.Name, cookie2.Value);
				}
				text = text2;
			}
			return text;
		}

		public string ScanQRCode()
		{
			string text = "";
			try
			{
				BinaryBitmap binaryBitmap = new BinaryBitmap(new HybridBinarizer(new BitmapLuminanceSource((Bitmap)this.image_screen)));
				Result result = new QRCodeReader().decode(binaryBitmap);
				if (result != null)
				{
					text = result.Text;
				}
			}
			catch
			{
			}
			return text;
		}

		public void Main_baidu_search(object sender, EventArgs e)
		{
			if (this.RichBoxBody.SelectText == "")
			{
				Process.Start("https://www.baidu.com/");
				return;
			}
			Process.Start("https://www.baidu.com/s?wd=" + this.RichBoxBody.SelectText);
		}

		public void tray_update_Click(object sender, EventArgs e)
		{
			Program.CheckUpdate();
		}

		public static bool contain_jap(string str)
		{
			return Regex.IsMatch(str, "[\\u3040-\\u309F]") || Regex.IsMatch(str, "[\\u30A0-\\u30FF]");
		}

		public static bool contain_kor(string str)
		{
			return Regex.IsMatch(str, "[\\uac00-\\ud7ff]");
		}

		public static string Del_ch(string str)
		{
			string text = str;
			if (Regex.IsMatch(str, "[\\u4e00-\\u9fa5]"))
			{
				text = string.Empty;
				char[] array = str.ToCharArray();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] < '一' || array[i] > '龥')
					{
						text += array[i].ToString();
					}
				}
			}
			return text;
		}

		public static string repalceStr(string hexData)
		{
			return Regex.Replace(hexData, "[\\p{P}+~$`^=|<>～\uff40＄\uff3e＋＝｜＜＞￥×┊ ]", "").ToUpper();
		}

		public static string RemovePunctuation(string str)
		{
			str = str.Replace(",", "").Replace("，", "").Replace(".", "")
				.Replace("。", "")
				.Replace("!", "")
				.Replace("！", "")
				.Replace("?", "")
				.Replace("？", "")
				.Replace(":", "")
				.Replace("：", "")
				.Replace(";", "")
				.Replace("；", "")
				.Replace("～", "")
				.Replace("-", "")
				.Replace("_", "")
				.Replace("——", "")
				.Replace("—", "")
				.Replace("--", "")
				.Replace("【", "")
				.Replace("】", "")
				.Replace("\\", "")
				.Replace("(", "")
				.Replace(")", "")
				.Replace("（", "")
				.Replace("）", "")
				.Replace("#", "")
				.Replace("$", "")
				.Replace("、", "")
				.Replace("‘", "")
				.Replace("’", "")
				.Replace("“", "")
				.Replace("”", "");
			return str;
		}

		public static string GetUniqueFileName(string fullName)
		{
			string text;
			if (!File.Exists(fullName))
			{
				text = fullName;
			}
			else
			{
				string directoryName = Path.GetDirectoryName(fullName);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullName);
				string extension = Path.GetExtension(fullName);
				int num = 1;
				string text2;
				do
				{
					text2 = Path.Combine(directoryName, string.Format("{0}[{1}].{2}", fileNameWithoutExtension, num++, extension));
				}
				while (File.Exists(text2));
				text = text2;
			}
			return text;
		}

		public static string ReFileName(string strFolderPath, string strFileName)
		{
			string text = strFolderPath + "\\" + strFileName;
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

		public void PlaySong(string file)
		{
			HelpWin32.mciSendString("close media", null, 0, IntPtr.Zero);
			HelpWin32.mciSendString("open \"" + file + "\" type mpegvideo alias media", null, 0, IntPtr.Zero);
			HelpWin32.mciSendString("play media notify", null, 0, base.Handle);
		}

		public void Main_Voice_Click(object sender, EventArgs e)
		{
			this.RichBoxBody.Focus();
			this.speak_copyb = "朗读";
			this.htmltxt = this.RichBoxBody.SelectText;
			HelpWin32.SendMessage(base.Handle, 786, 590);
		}

		public void Trans_Voice_Click(object sender, EventArgs e)
		{
			this.RichBoxBody_T.Focus();
			this.speak_copyb = "朗读";
			this.htmltxt = this.RichBoxBody_T.SelectText;
			HelpWin32.SendMessage(base.Handle, 786, 590);
		}

		public void Speak_child()
		{
			if (this.RichBoxBody.Text != null || this.RichBoxBody_T.Text != "")
			{
				string tempPath = Path.GetTempPath();
				string text = tempPath + "\\声音.mp3";
				try
				{
					File.WriteAllBytes(text, this.ttsData);
				}
				catch
				{
					text = tempPath + "\\声音1.mp3";
					File.WriteAllBytes(text, this.ttsData);
				}
				this.PlaySong(text);
				this.speaking = true;
			}
		}

		public static string ToSimplified(string source)
		{
			string text = new string(' ', source.Length);
			HelpWin32.LCMapString(2048, 33554432, source, source.Length, text, source.Length);
			return text;
		}

		public static string ToTraditional(string source)
		{
			string text = new string(' ', source.Length);
			HelpWin32.LCMapString(2048, 67108864, source, source.Length, text, source.Length);
			return text;
		}

		public void change_zh_tra_Click(object sender, EventArgs e)
		{
			if (this.RichBoxBody.Text != null)
			{
				this.RichBoxBody.Text = FmMain.ToTraditional(this.RichBoxBody.Text);
			}
		}

		public void change_tra_zh_Click(object sender, EventArgs e)
		{
			if (this.RichBoxBody.Text != null)
			{
				this.RichBoxBody.Text = FmMain.ToSimplified(this.RichBoxBody.Text);
			}
		}

		public void change_str_Upper_Click(object sender, EventArgs e)
		{
			if (this.RichBoxBody.Text != null)
			{
				this.RichBoxBody.Text = this.RichBoxBody.Text.ToUpper();
			}
		}

		public void change_Upper_str_Click(object sender, EventArgs e)
		{
			if (this.RichBoxBody.Text != null)
			{
				this.RichBoxBody.Text = this.RichBoxBody.Text.ToLower();
			}
		}

		public string[] hotkey(string text, string text2, string value)
		{
			string[] array = (value + "+").Split(new char[] { '+' });
			if (array.Length == 3)
			{
				text = array[0];
				text2 = array[1];
			}
			if (array.Length == 2)
			{
				text = "None";
				text2 = value;
			}
			return new string[] { text, text2 };
		}

		public void SetHotkey(string text, string text2, string value, int flag)
		{
			string[] array = (value + "+").Split(new char[] { '+' });
			if (array.Length == 3)
			{
				text = array[0];
				text2 = array[1];
			}
			if (array.Length == 2)
			{
				text = "None";
				text2 = value;
			}
			string[] array2 = new string[] { text, text2 };
			if (!HelpWin32.RegisterHotKey(base.Handle, flag, (HelpWin32.KeyModifiers)Enum.Parse(typeof(HelpWin32.KeyModifiers), array2[0].Trim()), (Keys)Enum.Parse(typeof(Keys), array2[1].Trim())))
			{
				this.fmflags.Show();
				this.fmflags.DrawStr("快捷键冲突，请更换！");
			}
			HelpWin32.RegisterHotKey(base.Handle, flag, (HelpWin32.KeyModifiers)Enum.Parse(typeof(HelpWin32.KeyModifiers), array2[0].Trim()), (Keys)Enum.Parse(typeof(Keys), array2[1].Trim()));
		}

		public void bool_error()
		{
		}

		public void p_note(string a)
		{
			for (int i = 0; i < StaticValue.v_notecount; i++)
			{
				if (i == StaticValue.v_notecount - 1)
				{
					this.pubnote[StaticValue.v_notecount - 1] = a;
				}
				else
				{
					this.pubnote[i] = this.pubnote[i + 1];
				}
			}
		}

		public void tray_note_Click(object sender, EventArgs e)
		{
			this.fmnote.Show();
			this.fmnote.WindowState = FormWindowState.Normal;
			this.fmnote.Visible = true;
		}

		public string Google_Hotkey(string text)
		{
			string text2 = "";
			try
			{
				string text3;
				string text4;
				if (FmMain.contain_ch(this.trans_hotkey.Trim()))
				{
					text3 = "zh-CN";
					text4 = "en";
				}
				else
				{
					text3 = "en";
					text4 = "zh-CN";
				}
				string text5 = string.Concat(new string[]
				{
					"https://translate.google.cn/translate_a/single?client=gtx&sl=",
					text3,
					"&tl=",
					text4,
					"&dt=t&q=",
					HttpUtility.UrlEncode(text).Replace("+", "%20")
				});
				JArray jarray = (JArray)JsonConvert.DeserializeObject(this.Get_GoogletHtml(text5));
				int count = ((JArray)jarray[0]).Count;
				for (int i = 0; i < count; i++)
				{
					text2 += jarray[0][i][0].ToString();
				}
			}
			catch (Exception ex)
			{
				text2 = "[Error]:" + ex.Message;
			}
			return text2;
		}

		private string GetTextFromClipboard()
		{
			if (Thread.CurrentThread.GetApartmentState() > ApartmentState.STA)
			{
				Thread thread = new Thread(new ThreadStart(FmMain.<>c.<>9.<GetTextFromClipboard>b__89_0));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
			}
			else
			{
				SendKeys.SendWait("^c");
				SendKeys.Flush();
			}
			string text = Clipboard.GetText();
			text = (string.IsNullOrWhiteSpace(text) ? null : text);
			if (text != null)
			{
				Clipboard.Clear();
			}
			return text;
		}

		public static string Get_html(string url)
		{
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			try
			{
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				httpWebRequest.Abort();
			}
			catch
			{
				text = "";
			}
			return text;
		}

		public CookieContainer Post_Html_Getcookie(string url, string post_str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Timeout = 5000;
			httpWebRequest.Headers.Add("Accept-Language:zh-CN,zh;q=0.8");
			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			httpWebRequest.CookieContainer = new CookieContainer();
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
				streamReader.ReadToEnd();
				responseStream.Close();
				streamReader.Close();
				httpWebRequest.Abort();
			}
			catch
			{
			}
			return httpWebRequest.CookieContainer;
		}

		public string Post_Html_Reccookie(string url, string post_str, CookieContainer CookieContainer)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Timeout = 6000;
			httpWebRequest.Headers.Add("Accept-Language:zh-CN,zh;q=0.8");
			httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");
			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			httpWebRequest.CookieContainer = CookieContainer;
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
				text = streamReader.ReadToEnd();
				responseStream.Close();
				streamReader.Close();
				httpWebRequest.Abort();
			}
			catch
			{
			}
			return text;
		}

		public string Post_Html(string url, string post_str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Timeout = 6000;
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			httpWebRequest.Headers.Add("Accept-Language: zh-CN,en,*");
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
				text = streamReader.ReadToEnd();
				responseStream.Close();
				streamReader.Close();
				httpWebRequest.Abort();
			}
			catch
			{
			}
			return text;
		}

		public void Main_OCR_Quickscreenshots()
		{
			if (!StaticValue.截图排斥)
			{
				try
				{
					this.change_QQ_screenshot = false;
					base.FormBorderStyle = FormBorderStyle.None;
					base.Visible = false;
					Thread.Sleep(100);
					if (this.transtalate_fla == "开启")
					{
						this.form_width = base.Width / 2;
					}
					else
					{
						this.form_width = base.Width;
					}
					this.shupai_Right_txt = "";
					this.shupai_Left_txt = "";
					this.form_height = base.Height;
					this.minico.Visible = false;
					this.minico.Visible = true;
					this.menu.Close();
					this.menu_copy.Close();
					this.auto_fla = "开启";
					this.split_txt = "";
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.RichBoxBody_T.Text = "";
					this.typeset_txt = "";
					this.transtalate_fla = "关闭";
					if (IniHelp.GetValue("工具栏", "翻译") == "False")
					{
						this.Trans_close.PerformClick();
					}
					base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
					base.FormBorderStyle = FormBorderStyle.Sizable;
					StaticValue.截图排斥 = true;
					string mode_flag;
					Point point;
					Rectangle[] array;
					this.image_screen = RegionCaptureTasks.GetRegionImage_Mo(new RegionCaptureOptions
					{
						ShowMagnifier = false,
						UseSquareMagnifier = false,
						MagnifierPixelCount = 15,
						MagnifierPixelSize = 10
					}, out mode_flag, out point, out array);
					if (mode_flag == "高级截图")
					{
						RegionCaptureMode regionCaptureMode = RegionCaptureMode.Annotation;
						RegionCaptureOptions regionCaptureOptions = new RegionCaptureOptions();
						using (RegionCaptureForm regionCaptureForm = new RegionCaptureForm(regionCaptureMode, regionCaptureOptions))
						{
							regionCaptureForm.Image_get = false;
							regionCaptureForm.Prepare(this.image_screen);
							regionCaptureForm.ShowDialog();
							this.image_screen = null;
							this.image_screen = regionCaptureForm.GetResultImage();
							mode_flag = regionCaptureForm.Mode_flag;
						}
					}
					HelpWin32.RegisterHotKey(base.Handle, 222, HelpWin32.KeyModifiers.None, Keys.Escape);
					if (mode_flag == "贴图")
					{
						Point point2 = new Point(point.X, point.Y);
						new FmScreenPaste(this.image_screen, point2).Show();
						if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
						{
							string value = IniHelp.GetValue("快捷键", "翻译文本");
							string text = "None";
							string text2 = "F9";
							this.SetHotkey(text, text2, value, 205);
						}
						HelpWin32.UnregisterHotKey(base.Handle, 222);
						StaticValue.截图排斥 = false;
					}
					else if (mode_flag == "区域多选")
					{
						if (this.image_screen == null)
						{
							if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
							{
								string value2 = IniHelp.GetValue("快捷键", "翻译文本");
								string text3 = "None";
								string text4 = "F9";
								this.SetHotkey(text3, text4, value2, 205);
							}
							HelpWin32.UnregisterHotKey(base.Handle, 222);
							StaticValue.截图排斥 = false;
						}
						else
						{
							this.minico.Visible = true;
							this.thread = new Thread(new ThreadStart(this.ShowLoading));
							this.thread.Start();
							this.ts = new TimeSpan(DateTime.Now.Ticks);
							this.getSubPics_ocr(this.image_screen, array);
						}
					}
					else if (mode_flag == "取色")
					{
						if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
						{
							string value3 = IniHelp.GetValue("快捷键", "翻译文本");
							string text5 = "None";
							string text6 = "F9";
							this.SetHotkey(text5, text6, value3, 205);
						}
						HelpWin32.UnregisterHotKey(base.Handle, 222);
						StaticValue.截图排斥 = false;
						this.fmflags.Show();
						this.fmflags.DrawStr("已复制颜色");
					}
					else if (this.image_screen == null)
					{
						if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
						{
							string value4 = IniHelp.GetValue("快捷键", "翻译文本");
							string text7 = "None";
							string text8 = "F9";
							this.SetHotkey(text7, text8, value4, 205);
						}
						HelpWin32.UnregisterHotKey(base.Handle, 222);
						StaticValue.截图排斥 = false;
					}
					else
					{
						if (mode_flag == "百度")
						{
							this.baidu_flags = "百度";
						}
						if (mode_flag == "拆分")
						{
							this.set_merge = false;
							this.set_split = true;
						}
						if (mode_flag == "合并")
						{
							this.set_merge = true;
							this.set_split = false;
						}
						if (mode_flag == "截图")
						{
							Clipboard.SetImage(this.image_screen);
							if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
							{
								string value5 = IniHelp.GetValue("快捷键", "翻译文本");
								string text9 = "None";
								string text10 = "F9";
								this.SetHotkey(text9, text10, value5, 205);
							}
							HelpWin32.UnregisterHotKey(base.Handle, 222);
							StaticValue.截图排斥 = false;
							if (IniHelp.GetValue("截图音效", "粘贴板") == "True")
							{
								this.PlaySong(IniHelp.GetValue("截图音效", "音效路径"));
							}
							this.fmflags.Show();
							this.fmflags.DrawStr("已复制截图");
						}
						else if (mode_flag == "自动保存" && IniHelp.GetValue("配置", "自动保存") == "True")
						{
							string text11 = IniHelp.GetValue("配置", "截图位置") + "\\" + FmMain.ReFileName(IniHelp.GetValue("配置", "截图位置"), "图片.Png");
							this.image_screen.Save(text11, ImageFormat.Png);
							StaticValue.截图排斥 = false;
							if (IniHelp.GetValue("截图音效", "自动保存") == "True")
							{
								this.PlaySong(IniHelp.GetValue("截图音效", "音效路径"));
							}
							this.fmflags.Show();
							this.fmflags.DrawStr("已保存图片");
						}
						else if (mode_flag == "多区域自动保存" && IniHelp.GetValue("配置", "自动保存") == "True")
						{
							this.getSubPics(this.image_screen, array);
							StaticValue.截图排斥 = false;
							if (IniHelp.GetValue("截图音效", "自动保存") == "True")
							{
								this.PlaySong(IniHelp.GetValue("截图音效", "音效路径"));
							}
							this.fmflags.Show();
							this.fmflags.DrawStr("已保存图片");
						}
						else if (mode_flag == "保存")
						{
							SaveFileDialog saveFileDialog = new SaveFileDialog();
							saveFileDialog.Filter = "png图片(*.png)|*.png|jpg图片(*.jpg)|*.jpg|bmp图片(*.bmp)|*.bmp";
							saveFileDialog.AddExtension = false;
							saveFileDialog.FileName = string.Concat(new string[]
							{
								"tianruo_",
								DateTime.Now.Year.ToString(),
								"-",
								DateTime.Now.Month.ToString(),
								"-",
								DateTime.Now.Day.ToString(),
								"-",
								DateTime.Now.Ticks.ToString()
							});
							saveFileDialog.Title = "保存图片";
							saveFileDialog.FilterIndex = 1;
							saveFileDialog.RestoreDirectory = true;
							if (saveFileDialog.ShowDialog() == DialogResult.OK)
							{
								string extension = Path.GetExtension(saveFileDialog.FileName);
								if (extension.Equals(".jpg"))
								{
									this.image_screen.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
								}
								if (extension.Equals(".png"))
								{
									this.image_screen.Save(saveFileDialog.FileName, ImageFormat.Png);
								}
								if (extension.Equals(".bmp"))
								{
									this.image_screen.Save(saveFileDialog.FileName, ImageFormat.Bmp);
								}
							}
							if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
							{
								string value6 = IniHelp.GetValue("快捷键", "翻译文本");
								string text12 = "None";
								string text13 = "F9";
								this.SetHotkey(text12, text13, value6, 205);
							}
							HelpWin32.UnregisterHotKey(base.Handle, 222);
							StaticValue.截图排斥 = false;
						}
						else if (this.image_screen != null)
						{
							if (IniHelp.GetValue("工具栏", "分栏") == "True")
							{
								this.minico.Visible = true;
								this.thread = new Thread(new ThreadStart(this.ShowLoading));
								this.thread.Start();
								this.ts = new TimeSpan(DateTime.Now.Ticks);
								Image image = this.image_screen;
								Graphics graphics = Graphics.FromImage(new Bitmap(image.Width, image.Height));
								graphics.DrawImage(image, 0, 0, image.Width, image.Height);
								graphics.Save();
								graphics.Dispose();
								((Bitmap)this.FindBundingBox_fences((Bitmap)image)).Save("Data\\分栏预览图.jpg");
								image.Dispose();
								this.image_screen.Dispose();
							}
							else
							{
								this.minico.Visible = true;
								this.thread = new Thread(new ThreadStart(this.ShowLoading));
								this.thread.Start();
								this.ts = new TimeSpan(DateTime.Now.Ticks);
								Messageload messageload = new Messageload();
								messageload.ShowDialog();
								if (messageload.DialogResult == DialogResult.OK)
								{
									this.esc_thread = new Thread(new ThreadStart(this.Main_OCR_Thread));
									this.esc_thread.Start();
								}
							}
						}
					}
				}
				catch
				{
					StaticValue.截图排斥 = false;
				}
			}
		}

		public void Main_OCR_Thread()
		{
			if (this.ScanQRCode() != "")
			{
				this.typeset_txt = this.ScanQRCode();
				this.RichBoxBody.Text = this.typeset_txt;
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
				return;
			}
			if (this.interface_flag == "搜狗")
			{
				this.OCR_sougou2();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
				return;
			}
			if (this.interface_flag == "腾讯")
			{
				this.OCR_Tencent();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
				return;
			}
			if (this.interface_flag == "有道")
			{
				this.OCR_youdao();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
				return;
			}
			if (this.interface_flag == "公式")
			{
				this.OCR_Math();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
				return;
			}
			if (this.interface_flag == "百度表格")
			{
				this.OCR_baidu_table();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_table));
				return;
			}
			if (this.interface_flag == "阿里表格")
			{
				this.OCR_ali_table();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_table));
				return;
			}
			if (this.interface_flag == "日语" || this.interface_flag == "中英" || this.interface_flag == "韩语")
			{
				this.OCR_baidu();
				this.fmloading.fml_close = "窗体已关闭";
				base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
			}
			if (this.interface_flag == "从左向右" || this.interface_flag == "从右向左")
			{
				this.shupai_Right_txt = "";
				Image image = this.image_screen;
				Bitmap bitmap = new Bitmap(image.Width, image.Height);
				Graphics graphics = Graphics.FromImage(bitmap);
				graphics.DrawImage(image, 0, 0, image.Width, image.Height);
				graphics.Save();
				graphics.Dispose();
				this.image_ori = bitmap;
				Image<Gray, byte> image2 = new Image<Gray, byte>(bitmap);
				Image<Gray, byte> image3 = new Image<Gray, byte>((Bitmap)this.FindBundingBox(image2.ToBitmap()));
				Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>();
				Image<Gray, byte> image5 = image3.Clone();
				CvInvoke.Canny(image3, image5, 0.0, 0.0, 5, true);
				this.select_image(image5, image4);
				bitmap.Dispose();
				image2.Dispose();
				image3.Dispose();
			}
			this.image_screen.Dispose();
			GC.Collect();
		}

		public void Main_OCR_Thread_last()
		{
			this.image_screen.Dispose();
			GC.Collect();
			StaticValue.截图排斥 = false;
			string text = this.typeset_txt;
			text = this.check_str(text);
			this.split_txt = this.check_str(this.split_txt);
			if (!FmMain.punctuation_has_punctuation(text))
			{
				text = this.split_txt;
			}
			if (FmMain.contain_ch(text.Trim()))
			{
				text = FmMain.Del_Space(text);
			}
			if (text != "")
			{
				this.RichBoxBody.Text = text;
			}
			StaticValue.v_Split = this.split_txt;
			if (bool.Parse(IniHelp.GetValue("工具栏", "拆分")) || this.set_split)
			{
				this.set_split = false;
				this.RichBoxBody.Text = this.split_txt;
			}
			if (bool.Parse(IniHelp.GetValue("工具栏", "合并")) || this.set_merge)
			{
				this.set_merge = false;
				this.RichBoxBody.Text = text.Replace("\n", "").Replace("\r", "");
			}
			TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
			TimeSpan timeSpan2 = timeSpan.Subtract(this.ts).Duration();
			string text2 = string.Concat(new string[]
			{
				timeSpan2.Seconds.ToString(),
				".",
				Convert.ToInt32(timeSpan2.TotalMilliseconds).ToString(),
				"秒"
			});
			if (this.RichBoxBody.Text != null)
			{
				this.p_note(this.RichBoxBody.Text);
				StaticValue.v_note = this.pubnote;
				if (this.fmnote.Created)
				{
					this.fmnote.Text_note = "";
				}
			}
			if (StaticValue.v_topmost)
			{
				base.TopMost = true;
			}
			else
			{
				base.TopMost = false;
			}
			this.Text = "耗时：" + text2;
			this.minico.Visible = true;
			if (this.interface_flag == "从右向左")
			{
				this.RichBoxBody.Text = this.shupai_Right_txt;
			}
			if (this.interface_flag == "从左向右")
			{
				this.RichBoxBody.Text = this.shupai_Left_txt;
			}
			Clipboard.SetDataObject(this.RichBoxBody.Text);
			if (this.baidu_flags == "百度")
			{
				base.FormBorderStyle = FormBorderStyle.Sizable;
				base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
				base.Visible = false;
				base.WindowState = FormWindowState.Minimized;
				base.Show();
				Process.Start("https://www.baidu.com/s?wd=" + this.RichBoxBody.Text);
				this.baidu_flags = "";
				if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
				{
					string value = IniHelp.GetValue("快捷键", "翻译文本");
					string text3 = "None";
					string text4 = "F9";
					this.SetHotkey(text3, text4, value, 205);
				}
				HelpWin32.UnregisterHotKey(base.Handle, 222);
				return;
			}
			if (IniHelp.GetValue("配置", "识别弹窗") == "False")
			{
				base.FormBorderStyle = FormBorderStyle.Sizable;
				base.Size = new Size((int)this.font_base.Width * 23, (int)this.font_base.Height * 24);
				base.Visible = false;
				this.fmflags.Show();
				if (this.RichBoxBody.Text == "***该区域未发现文本***")
				{
					this.fmflags.DrawStr("无文本");
				}
				else
				{
					this.fmflags.DrawStr("已识别");
				}
				if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
				{
					string value2 = IniHelp.GetValue("快捷键", "翻译文本");
					string text5 = "None";
					string text6 = "F9";
					this.SetHotkey(text5, text6, value2, 205);
				}
				HelpWin32.UnregisterHotKey(base.Handle, 222);
				return;
			}
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.Visible = true;
			base.Show();
			base.WindowState = FormWindowState.Normal;
			base.Size = new Size(this.form_width, this.form_height);
			HelpWin32.SetForegroundWindow(base.Handle);
			StaticValue.v_googleTranslate_txt = this.RichBoxBody.Text;
			if (bool.Parse(IniHelp.GetValue("工具栏", "翻译")))
			{
				try
				{
					this.auto_fla = "";
					base.Invoke(new FmMain.translate(this.transtalate_Click));
				}
				catch
				{
				}
			}
			if (bool.Parse(IniHelp.GetValue("工具栏", "检查")))
			{
				try
				{
					this.RichBoxBody.Find = "";
				}
				catch
				{
				}
			}
			if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
			{
				string value3 = IniHelp.GetValue("快捷键", "翻译文本");
				string text7 = "None";
				string text8 = "F9";
				this.SetHotkey(text7, text8, value3, 205);
			}
			HelpWin32.UnregisterHotKey(base.Handle, 222);
			this.RichBoxBody.Refresh();
		}

		private void OCR_baidu_Ch_and_En_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("中英");
		}

		private void OCR_baidu_Jap_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("日语");
		}

		private void OCR_baidu_Kor_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("韩语");
		}

		public string Get_GoogletHtml(string url)
		{
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "GET";
			httpWebRequest.Timeout = 5000;
			httpWebRequest.Headers.Add("Accept-Language: zh-CN;q=0.8,en-US;q=0.6,en;q=0.4");
			httpWebRequest.Headers.Add("Accept-Encoding: gzip,deflate");
			httpWebRequest.Headers.Add("Accept-Charset: utf-8");
			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
			httpWebRequest.Host = "translate.google.cn";
			httpWebRequest.Accept = "*/*";
			httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)";
			string text2;
			try
			{
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				text2 = text;
			}
			catch
			{
				text2 = null;
			}
			return text2;
		}

		public void OCR_baidu()
		{
			this.split_txt = "";
			try
			{
				this.baidu_vip = FmMain.Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + StaticValue.baiduAPI_ID + "&client_secret=" + StaticValue.baiduAPI_key));
				if (this.baidu_vip == "")
				{
					MessageBox.Show("请检查密钥输入是否正确！", "提醒");
				}
				else
				{
					string text = "CHN_ENG";
					this.split_txt = "";
					Image image = this.image_screen;
					byte[] array = this.OCR_ImgToByte(image);
					if (this.interface_flag == "中英")
					{
						text = "CHN_ENG";
					}
					if (this.interface_flag == "日语")
					{
						text = "JAP";
					}
					if (this.interface_flag == "韩语")
					{
						text = "KOR";
					}
					string text2 = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array)) + "&language_type=" + text;
					byte[] bytes = Encoding.UTF8.GetBytes(text2);
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + ((JObject)JsonConvert.DeserializeObject(this.baidu_vip))["access_token"]);
					httpWebRequest.Method = "POST";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
					httpWebRequest.Timeout = 8000;
					httpWebRequest.ReadWriteTimeout = 5000;
					using (Stream requestStream = httpWebRequest.GetRequestStream())
					{
						requestStream.Write(bytes, 0, bytes.Length);
					}
					Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
					string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
					responseStream.Close();
					JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["words_result"].ToString());
					this.checked_txt(jarray, 1, "words");
				}
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本或者密钥次数用尽***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public string check_str(string text)
		{
			if (FmMain.contain_ch(text.Trim()))
			{
				text = FmMain.punctuation_en_ch(text.Trim());
				text = FmMain.check_ch_en(text.Trim());
			}
			else
			{
				text = FmMain.punctuation_ch_en(text.Trim());
				if (this.contain(text, ".") && (this.contain(text, ",") || this.contain(text, "!") || this.contain(text, "(") || this.contain(text, ")") || this.contain(text, "'")))
				{
					text = FmMain.punctuation_Del_space(text);
				}
			}
			return text;
		}

		public static string punctuation_en_ch_x(string text)
		{
			char[] array = text.ToCharArray();
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

		public string OCR_sougou_SogouPost(string url, CookieContainer cookie, byte[] content)
		{
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.CookieContainer = cookie;
			httpWebRequest.Timeout = 10000;
			httpWebRequest.Referer = "http://pic.sogou.com/resource/pic/shitu_intro/index.html";
			httpWebRequest.ContentType = "multipart/form-data; boundary=----WebKitFormBoundary1ZZDB9E4sro7pf0g";
			httpWebRequest.Accept = "*/*";
			httpWebRequest.Headers.Add("Origin: http://pic.sogou.com");
			httpWebRequest.Headers.Add("Accept-Encoding: gzip,deflate");
			httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)";
			httpWebRequest.ServicePoint.Expect100Continue = false;
			httpWebRequest.ProtocolVersion = new Version(1, 1);
			httpWebRequest.ContentLength = (long)content.Length;
			Stream requestStream = httpWebRequest.GetRequestStream();
			requestStream.Write(content, 0, content.Length);
			requestStream.Close();
			string text2;
			try
			{
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					Stream stream = httpWebResponse.GetResponseStream();
					if (httpWebResponse.ContentEncoding.ToLower().Contains("gzip"))
					{
						stream = new GZipStream(stream, CompressionMode.Decompress);
					}
					using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				text2 = text;
			}
			catch
			{
				text2 = null;
			}
			return text2;
		}

		public string OCR_sougou_SogouGet(string url, CookieContainer cookie, string refer)
		{
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "GET";
			httpWebRequest.CookieContainer = cookie;
			httpWebRequest.Referer = refer;
			httpWebRequest.Timeout = 10000;
			httpWebRequest.Accept = "application/json";
			httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
			httpWebRequest.Headers.Add("Accept-Encoding: gzip,deflate");
			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)";
			httpWebRequest.ServicePoint.Expect100Continue = false;
			httpWebRequest.ProtocolVersion = new Version(1, 1);
			string text2;
			try
			{
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					Stream stream = httpWebResponse.GetResponseStream();
					if (httpWebResponse.ContentEncoding.ToLower().Contains("gzip"))
					{
						stream = new GZipStream(stream, CompressionMode.Decompress);
					}
					using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				text2 = text;
			}
			catch
			{
				text2 = null;
			}
			return text2;
		}

		public string OCR_sougou_SogouOCR(Image img)
		{
			CookieContainer cookieContainer = new CookieContainer();
			string text = "http://pic.sogou.com/pic/upload_pic.jsp";
			string text2 = this.OCR_sougou_SogouPost(text, cookieContainer, this.OCR_sougou_Content_Length(img));
			string text3 = "http://pic.sogou.com/pic/ocr/ocrOnline.jsp?query=" + text2;
			string text4 = "http://pic.sogou.com/resource/pic/shitu_intro/word_1.html?keyword=" + text2;
			return this.OCR_sougou_SogouGet(text3, cookieContainer, text4);
		}

		private byte[] OCR_ImgToByte(Image img)
		{
			byte[] array2;
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				img.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				array2 = array;
			}
			catch
			{
				array2 = null;
			}
			return array2;
		}

		public byte[] OCR_sougou_Content_Length(Image img)
		{
			byte[] bytes = Encoding.UTF8.GetBytes("------WebKitFormBoundary1ZZDB9E4sro7pf0g\r\nContent-Disposition: form-data; name=\"pic_path\"; filename=\"test2018.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n");
			byte[] array = this.OCR_ImgToByte(img);
			byte[] bytes2 = Encoding.UTF8.GetBytes("\r\n------WebKitFormBoundary1ZZDB9E4sro7pf0g--\r\n");
			byte[] array2 = new byte[bytes.Length + array.Length + bytes2.Length];
			bytes.CopyTo(array2, 0);
			array.CopyTo(array2, bytes.Length);
			bytes2.CopyTo(array2, bytes.Length + array.Length);
			return array2;
		}

		public void OCR_sougou2()
		{
			try
			{
				this.split_txt = "";
				string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
				Image image = this.ZoomImage((Bitmap)this.image_screen, 120, 120);
				byte[] array = this.OCR_ImgToByte(image);
				string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
				string text3 = "\r\n" + text + "--\r\n";
				byte[] bytes = Encoding.ASCII.GetBytes(text2);
				byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
				byte[] array2 = FmMain.Mergebyte(bytes, array, bytes2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ocr.shouji.sogou.com/v2/ocr/json");
				httpWebRequest.Timeout = 8000;
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(array2, 0, array2.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text4))["result"].ToString());
				if (IniHelp.GetValue("工具栏", "分段") == "True")
				{
					this.checked_location_sougou(jarray, 2, "content", "frame");
				}
				else
				{
					this.checked_txt(jarray, 2, "content");
				}
				image.Dispose();
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public static byte[] Mergebyte(byte[] a, byte[] b, byte[] c)
		{
			byte[] array = new byte[a.Length + b.Length + c.Length];
			a.CopyTo(array, 0);
			b.CopyTo(array, a.Length);
			c.CopyTo(array, a.Length + b.Length);
			return array;
		}

		public static bool contain_punctuation(string str)
		{
			return Regex.IsMatch(str, "\\p{P}");
		}

		private void tray_help_Click(object sender, EventArgs e)
		{
			base.WindowState = FormWindowState.Minimized;
			new FmHelp().Show();
		}

		public bool Is_punctuation(string text)
		{
			return ",;:，（）、；".IndexOf(text) != -1;
		}

		public bool has_punctuation(string text)
		{
			return ",;，；、<>《》()-（）".IndexOf(text) != -1;
		}

		public void checked_txt(JArray jarray, int lastlength, string words)
		{
			int num = 0;
			for (int i = 0; i < jarray.Count; i++)
			{
				int length = JObject.Parse(jarray[i].ToString())[words].ToString().Length;
				if (length > num)
				{
					num = length;
				}
			}
			string text = "";
			string text2 = "";
			for (int j = 0; j < jarray.Count - 1; j++)
			{
				JObject jobject = JObject.Parse(jarray[j].ToString());
				char[] array = jobject[words].ToString().ToCharArray();
				JObject jobject2 = JObject.Parse(jarray[j + 1].ToString());
				char[] array2 = jobject2[words].ToString().ToCharArray();
				int length2 = jobject[words].ToString().Length;
				int length3 = jobject2[words].ToString().Length;
				if (Math.Abs(length2 - length3) <= 0)
				{
					if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && this.Is_punctuation(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else
					{
						text2 += jobject[words].ToString().Trim();
					}
				}
				else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && Math.Abs(length2 - length3) <= 1)
				{
					if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && this.Is_punctuation(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else
					{
						text2 += jobject[words].ToString().Trim();
					}
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && length2 <= num / 2)
				{
					text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()) && length3 - length2 < 4 && array2[1].ToString() == ".")
				{
					text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.contain_en(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
				{
					text2 = text2 + jobject[words].ToString().Trim() + " ";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.contain_en(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && this.Is_punctuation(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (this.Is_punctuation(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (this.Is_punctuation(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
				{
					text2 = text2 + jobject[words].ToString().Trim() + " ";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.IsNum(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.IsNum(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else
				{
					text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
				}
				if (this.has_punctuation(jobject[words].ToString()))
				{
					text2 += "\r\n";
				}
				text = text + jobject[words].ToString().Trim() + "\r\n";
			}
			this.split_txt = text + JObject.Parse(jarray[jarray.Count - 1].ToString())[words];
			this.typeset_txt = text2.Replace("\r\n\r\n", "\r\n") + JObject.Parse(jarray[jarray.Count - 1].ToString())[words];
		}

		private void OCR_foreach(string name)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
			if (name == "韩语")
			{
				this.interface_flag = "韩语";
				this.Refresh();
				this.baidu.Text = "百度√";
				this.kor.Text = "韩语√";
			}
			if (name == "日语")
			{
				this.interface_flag = "日语";
				this.Refresh();
				this.baidu.Text = "百度√";
				this.jap.Text = "日语√";
			}
			if (name == "中英")
			{
				this.interface_flag = "中英";
				this.Refresh();
				this.baidu.Text = "百度√";
				this.ch_en.Text = "中英√";
			}
			if (name == "搜狗")
			{
				this.interface_flag = "搜狗";
				this.Refresh();
				this.sougou.Text = "搜狗√";
			}
			if (name == "腾讯")
			{
				this.interface_flag = "腾讯";
				this.Refresh();
				this.tencent.Text = "腾讯√";
			}
			if (name == "有道")
			{
				this.interface_flag = "有道";
				this.Refresh();
				this.youdao.Text = "有道√";
			}
			if (name == "公式")
			{
				this.interface_flag = "公式";
				this.Refresh();
				this.Mathfuntion.Text = "公式√";
			}
			if (name == "百度表格")
			{
				this.interface_flag = "百度表格";
				this.Refresh();
				this.ocr_table.Text = "表格√";
				this.baidu_table.Text = "百度√";
			}
			if (name == "阿里表格")
			{
				this.interface_flag = "阿里表格";
				this.Refresh();
				this.ocr_table.Text = "表格√";
				this.ali_table.Text = "阿里√";
			}
			if (name == "从左向右")
			{
				if (!File.Exists("cvextern.dll"))
				{
					MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\r\n将下载后的文件与 天若.exe 这个文件放在一起。");
					Process.Start("https://www.lanzous.com/i1ab3vg");
				}
				else
				{
					this.interface_flag = "从左向右";
					this.Refresh();
					this.shupai.Text = "竖排√";
					this.left_right.Text = "从左向右√";
				}
			}
			if (name == "从右向左")
			{
				if (!File.Exists("cvextern.dll"))
				{
					MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\r\n将下载后的文件与 天若.exe 这个文件放在一起。");
					Process.Start("https://www.lanzous.com/i1ab3vg");
					return;
				}
				this.interface_flag = "从右向左";
				this.Refresh();
				this.shupai.Text = "竖排√";
				this.righ_left.Text = "从右向左√";
			}
			HelpWin32.IniFileHelper.SetValue("配置", "接口", this.interface_flag, text);
		}

		private void OCR_shupai_Click(object sender, EventArgs e)
		{
		}

		private void OCR_write_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("手写");
		}

		private void OCR_lefttoright_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("从左向右");
		}

		private void OCR_righttoleft_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("从右向左");
		}

		public void OCR_baidu_acc()
		{
			this.split_txt = "";
			string text = "";
			try
			{
				this.baidu_vip = FmMain.Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + StaticValue.baiduAPI_ID + "&client_secret=" + StaticValue.baiduAPI_key));
				if (this.baidu_vip == "")
				{
					MessageBox.Show("请检查密钥输入是否正确！", "提醒");
				}
				else
				{
					this.split_txt = "";
					Image image = this.image_screen;
					byte[] array = this.OCR_ImgToByte(image);
					string text2 = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array));
					byte[] bytes = Encoding.UTF8.GetBytes(text2);
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + ((JObject)JsonConvert.DeserializeObject(this.baidu_vip))["access_token"]);
					httpWebRequest.Method = "POST";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
					httpWebRequest.Timeout = 8000;
					httpWebRequest.ReadWriteTimeout = 5000;
					ServicePointManager.DefaultConnectionLimit = 512;
					using (Stream requestStream = httpWebRequest.GetRequestStream())
					{
						requestStream.Write(bytes, 0, bytes.Length);
					}
					Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
					string text3;
					text = (text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd());
					responseStream.Close();
					JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["words_result"].ToString());
					string text4 = "";
					for (int i = 0; i < jarray.Count; i++)
					{
						JObject jobject = JObject.Parse(jarray[i].ToString());
						text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					}
					this.shupai_Right_txt = this.shupai_Right_txt + text4 + "\r\n";
					Thread.Sleep(600);
				}
			}
			catch
			{
				MessageBox.Show(text, "提醒");
				StaticValue.截图排斥 = false;
				this.esc = "退出";
				this.fmloading.fml_close = "窗体已关闭";
				this.esc_thread.Abort();
			}
		}

		public void OCR_Tencent_handwriting()
		{
			try
			{
				this.split_txt = "";
				string text = "------WebKitFormBoundaryRDEqU0w702X9cWPJ";
				Image image = this.image_screen;
				if (image.Width > 90 && image.Height < 90)
				{
					Bitmap bitmap = new Bitmap(image.Width, 300);
					Graphics graphics = Graphics.FromImage(bitmap);
					graphics.DrawImage(image, 5, 0, image.Width, image.Height);
					graphics.Save();
					graphics.Dispose();
					image = new Bitmap(bitmap);
				}
				else if (image.Width <= 90 && image.Height >= 90)
				{
					Bitmap bitmap2 = new Bitmap(300, image.Height);
					Graphics graphics2 = Graphics.FromImage(bitmap2);
					graphics2.DrawImage(image, 0, 5, image.Width, image.Height);
					graphics2.Save();
					graphics2.Dispose();
					image = new Bitmap(bitmap2);
				}
				else if (image.Width < 90 && image.Height < 90)
				{
					Bitmap bitmap3 = new Bitmap(300, 300);
					Graphics graphics3 = Graphics.FromImage(bitmap3);
					graphics3.DrawImage(image, 5, 5, image.Width, image.Height);
					graphics3.Save();
					graphics3.Dispose();
					image = new Bitmap(bitmap3);
				}
				else
				{
					image = this.image_screen;
				}
				byte[] array = this.OCR_ImgToByte(image);
				string text2 = text + "\r\nContent-Disposition: form-data; name=\"image_file\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
				string text3 = "\r\n" + text + "--\r\n";
				byte[] bytes = Encoding.ASCII.GetBytes(text2);
				byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
				byte[] array2 = FmMain.Mergebyte(bytes, array, bytes2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://ai.qq.com/cgi-bin/appdemo_handwritingocr");
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = "http://ai.qq.com/product/ocr.shtml";
				httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
				httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
				httpWebRequest.Timeout = 8000;
				httpWebRequest.ReadWriteTimeout = 2000;
				byte[] array3 = array2;
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(array3, 0, array2.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text4))["data"]["item_list"].ToString());
				this.checked_txt(jarray, 1, "itemstring");
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public Image BoundingBox(Image<Gray, byte> src, Image<Bgr, byte> draw)
		{
			Image image2;
			using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default(Point));
				Image image = draw.ToBitmap();
				Graphics graphics = Graphics.FromImage(image);
				int size = vectorOfVectorOfPoint.Size;
				for (int i = 0; i < size; i++)
				{
					using (VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i])
					{
						Rectangle rectangle = CvInvoke.BoundingRectangle(vectorOfPoint);
						int x = rectangle.Location.X;
						int y = rectangle.Location.Y;
						int width = rectangle.Size.Width;
						int height = rectangle.Size.Height;
						if (width > 5 || height > 5)
						{
							graphics.FillRectangle(Brushes.White, x, 0, width, image.Size.Height);
						}
					}
				}
				graphics.Dispose();
				Bitmap bitmap = new Bitmap(image.Width + 2, image.Height + 2);
				Graphics graphics2 = Graphics.FromImage(bitmap);
				graphics2.DrawImage(image, 1, 1, image.Width, image.Height);
				graphics2.Save();
				graphics2.Dispose();
				image2 = bitmap;
			}
			return image2;
		}

		public void select_image(Image<Gray, byte> src, Image<Bgr, byte> draw)
		{
			try
			{
				using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new VectorOfVectorOfPoint())
				{
					CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default(Point));
					int num = vectorOfVectorOfPoint.Size / 2;
					this.imagelist_lenght = num;
					this.bool_image_count(num);
					if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data\\image_temp"))
					{
						Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Data\\image_temp");
					}
					this.OCR_baidu_a = "";
					this.OCR_baidu_b = "";
					this.OCR_baidu_c = "";
					this.OCR_baidu_d = "";
					this.OCR_baidu_e = "";
					for (int i = 0; i < num; i++)
					{
						using (VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i])
						{
							Rectangle rectangle = CvInvoke.BoundingRectangle(vectorOfPoint);
							if (rectangle.Size.Width > 1 && rectangle.Size.Height > 1)
							{
								int x = rectangle.Location.X;
								int y = rectangle.Location.Y;
								int width = rectangle.Size.Width;
								int height = rectangle.Size.Height;
								new Point(x, 0);
								new Point(x, this.image_ori.Size.Height);
								Rectangle rectangle2 = new Rectangle(x, 0, width, this.image_ori.Size.Height);
								Bitmap bitmap = new Bitmap(width + 70, rectangle2.Size.Height);
								Graphics graphics = Graphics.FromImage(bitmap);
								graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Size.Width, bitmap.Size.Height);
								graphics.DrawImage(this.image_ori, 30, 0, rectangle2, GraphicsUnit.Pixel);
								Bitmap bitmap2 = Image.FromHbitmap(bitmap.GetHbitmap());
								bitmap2.Save("Data\\image_temp\\" + i + ".jpg", ImageFormat.Jpeg);
								bitmap2.Dispose();
								bitmap.Dispose();
								graphics.Dispose();
							}
						}
					}
					Messageload messageload = new Messageload();
					messageload.ShowDialog();
					if (messageload.DialogResult == DialogResult.OK)
					{
						ManualResetEvent[] array = new ManualResetEvent[]
						{
							new ManualResetEvent(false)
						};
						ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoWork), array[0]);
					}
				}
			}
			catch
			{
				this.exit_thread();
			}
		}

		public Image FindBundingBox(Bitmap bitmap)
		{
			Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
			Image<Gray, byte> image2 = new Image<Gray, byte>(image.Width, image.Height);
			CvInvoke.CvtColor(image, image2, ColorConversion.Bgra2Gray, 0);
			Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(1, 1));
			CvInvoke.Erode(image2, image2, structuringElement, new Point(0, 2), 1, BorderType.Reflect101, default(MCvScalar));
			CvInvoke.Threshold(image2, image2, 100.0, 255.0, (ThresholdType)9);
			Image<Gray, byte> image3 = new Image<Gray, byte>(image2.ToBitmap());
			Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>();
			Image<Gray, byte> image5 = image3.Clone();
			CvInvoke.Canny(image3, image5, 255.0, 255.0, 5, true);
			return this.BoundingBox(image5, image4);
		}

		public void Captureimage(int width, Image g_image, string saveFilePath, Rectangle rect)
		{
			Bitmap bitmap = new Bitmap(width + 70, g_image.Size.Height);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Size.Width, bitmap.Size.Height);
			graphics.DrawImage(g_image, 30, 0, rect, GraphicsUnit.Pixel);
			Bitmap bitmap2 = Image.FromHbitmap(bitmap.GetHbitmap());
			bitmap2.Save(saveFilePath, ImageFormat.Jpeg);
			this.image_screen = bitmap2;
			this.OCR_baidu_use();
			bitmap2.Dispose();
			bitmap.Dispose();
			graphics.Dispose();
		}

		public void OCR_baidu_use()
		{
			this.split_txt = "";
			try
			{
				string text = "CHN_ENG";
				this.split_txt = "";
				Image image = this.image_screen;
				MemoryStream memoryStream = new MemoryStream();
				image.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				this.shupai_Right_txt = (this.shupai_Right_txt + text4 + "\r\n").Replace("\r\n\r\n", "");
				this.shupai_Left_txt = text5.Replace("\r\n\r\n", "");
				MessageBox.Show(this.shupai_Left_txt);
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void OCR_sougou_use()
		{
			try
			{
				this.split_txt = "";
				string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
				Image image = this.image_screen;
				int i = image.Width;
				int j = image.Height;
				if (i < 300)
				{
					while (i < 300)
					{
						j *= 2;
						i *= 2;
					}
				}
				if (j < 120)
				{
					while (j < 120)
					{
						j *= 2;
						i *= 2;
					}
				}
				Bitmap bitmap = new Bitmap(i, j);
				Graphics graphics = Graphics.FromImage(bitmap);
				graphics.DrawImage(image, 0, 0, i, j);
				graphics.Save();
				graphics.Dispose();
				image = new Bitmap(bitmap);
				byte[] array = this.OCR_ImgToByte(image);
				string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
				string text3 = "\r\n" + text + "--\r\n";
				byte[] bytes = Encoding.ASCII.GetBytes(text2);
				byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
				byte[] array2 = FmMain.Mergebyte(bytes, array, bytes2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ocr.shouji.sogou.com/v2/ocr/json");
				httpWebRequest.Timeout = 8000;
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(array2, 0, array2.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text4))["result"].ToString());
				string text5 = "";
				for (int k = 0; k < jarray.Count; k++)
				{
					JObject jobject = JObject.Parse(jarray[k].ToString());
					text5 += jobject["content"].ToString().Replace("\r", "").Replace("\n", "");
				}
				this.shupai_Right_txt = this.shupai_Right_txt + text5 + "\r\n";
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public bool split_paragraph(string text)
		{
			return "。？！?!：".IndexOf(text) != -1;
		}

		public void baidu_image_a(object objEvent)
		{
			try
			{
				for (int i = 0; i < this.image_num[0]; i++)
				{
					Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
					this.OCR_baidu_use_A(Image.FromStream(stream));
					stream.Close();
				}
				((ManualResetEvent)objEvent).Set();
			}
			catch
			{
				this.exit_thread();
			}
		}

		public void baidu_image_b(object objEvent)
		{
			try
			{
				for (int i = this.image_num[0]; i < this.image_num[1]; i++)
				{
					Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
					this.OCR_baidu_use_B(Image.FromStream(stream));
					stream.Close();
				}
				((ManualResetEvent)objEvent).Set();
			}
			catch
			{
				this.exit_thread();
			}
		}

		private void DoWork(object state)
		{
			ManualResetEvent[] array = new ManualResetEvent[5];
			array[0] = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.baidu_image_a), array[0]);
			array[1] = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.baidu_image_b), array[1]);
			array[2] = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.baidu_image_c), array[2]);
			array[3] = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.baidu_image_d), array[3]);
			array[4] = new ManualResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.baidu_image_e), array[4]);
			WaitHandle[] array2 = array;
			WaitHandle.WaitAll(array2);
			this.shupai_Right_txt = string.Concat(new string[] { this.OCR_baidu_a, this.OCR_baidu_b, this.OCR_baidu_c, this.OCR_baidu_d, this.OCR_baidu_e }).Replace("\r\n\r\n", "");
			string text = this.shupai_Right_txt.TrimEnd(new char[] { '\n' }).TrimEnd(new char[] { '\r' }).TrimEnd(new char[] { '\n' });
			if (text.Split(Environment.NewLine.ToCharArray()).Length > 1)
			{
				string[] array3 = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				string text2 = "";
				for (int i = 0; i < array3.Length; i++)
				{
					text2 = text2 + array3[array3.Length - i - 1].Replace("\r", "").Replace("\n", "") + "\r\n";
				}
				this.shupai_Left_txt = text2;
			}
			this.fmloading.fml_close = "窗体已关闭";
			base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
			try
			{
				this.DeleteFile("Data\\image_temp");
			}
			catch
			{
				this.exit_thread();
			}
			this.image_ori.Dispose();
		}

		public void OCR_baidu_use_B(Image imagearr)
		{
			try
			{
				string text = "CHN_ENG";
				MemoryStream memoryStream = new MemoryStream();
				imagearr.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				this.OCR_baidu_b = (this.OCR_baidu_b + text4 + "\r\n").Replace("\r\n\r\n", "");
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void OCR_baidu_use_A(Image imagearr)
		{
			try
			{
				string text = "CHN_ENG";
				MemoryStream memoryStream = new MemoryStream();
				imagearr.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				this.OCR_baidu_a = (this.OCR_baidu_a + text4 + "\r\n").Replace("\r\n\r\n", "");
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void DeleteFile(string path)
		{
			if (File.GetAttributes(path) == FileAttributes.Directory)
			{
				Directory.Delete(path, true);
				return;
			}
			File.Delete(path);
		}

		public void OCR_baidu_image(Image imagearr, string str_image)
		{
			try
			{
				string text = "CHN_ENG";
				MemoryStream memoryStream = new MemoryStream();
				imagearr.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				str_image = (str_image + text4 + "\r\n").Replace("\r\n\r\n", "");
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void OCR_baidu_use_E(Image imagearr)
		{
			try
			{
				string text = "CHN_ENG";
				MemoryStream memoryStream = new MemoryStream();
				imagearr.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				this.OCR_baidu_e = (this.OCR_baidu_e + text4 + "\r\n").Replace("\r\n\r\n", "");
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void OCR_baidu_use_D(Image imagearr)
		{
			try
			{
				string text = "CHN_ENG";
				MemoryStream memoryStream = new MemoryStream();
				imagearr.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				this.OCR_baidu_d = (this.OCR_baidu_d + text4 + "\r\n").Replace("\r\n\r\n", "");
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void OCR_baidu_use_C(Image imagearr)
		{
			try
			{
				string text = "CHN_ENG";
				MemoryStream memoryStream = new MemoryStream();
				imagearr.Save(memoryStream, ImageFormat.Jpeg);
				byte[] array = new byte[memoryStream.Length];
				memoryStream.Position = 0L;
				memoryStream.Read(array, 0, (int)memoryStream.Length);
				memoryStream.Close();
				string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.GetResponse().Close();
				HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create("http://ai.baidu.com/aidemo");
				httpWebRequest2.Method = "POST";
				httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
				httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest2.Timeout = 8000;
				httpWebRequest2.ReadWriteTimeout = 5000;
				httpWebRequest2.Headers.Add("Cookie:" + FmMain.CookieCollectionToStrCookie(((HttpWebResponse)httpWebRequest.GetResponse()).Cookies));
				using (Stream requestStream = httpWebRequest2.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest2.GetResponse()).GetResponseStream();
				string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString());
				string text4 = "";
				string[] array2 = new string[jarray.Count];
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text4 += jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
					array2[jarray.Count - 1 - i] = jobject["words"].ToString().Replace("\r", "").Replace("\n", "");
				}
				string text5 = "";
				for (int j = 0; j < array2.Length; j++)
				{
					text5 += array2[j];
				}
				this.OCR_baidu_c = (this.OCR_baidu_c + text4 + "\r\n").Replace("\r\n\r\n", "");
				Thread.Sleep(10);
			}
			catch
			{
			}
		}

		public void baidu_image_c(object objEvent)
		{
			try
			{
				for (int i = this.image_num[1]; i < this.image_num[2]; i++)
				{
					Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
					this.OCR_baidu_use_C(Image.FromStream(stream));
					stream.Close();
				}
				((ManualResetEvent)objEvent).Set();
			}
			catch
			{
				this.exit_thread();
			}
		}

		public void baidu_image_d(object objEvent)
		{
			try
			{
				for (int i = this.image_num[2]; i < this.image_num[3]; i++)
				{
					Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
					this.OCR_baidu_use_D(Image.FromStream(stream));
					stream.Close();
				}
				((ManualResetEvent)objEvent).Set();
			}
			catch
			{
				this.exit_thread();
			}
		}

		public void baidu_image_e(object objEvent)
		{
			try
			{
				for (int i = this.image_num[3]; i < this.image_num[4]; i++)
				{
					Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
					this.OCR_baidu_use_E(Image.FromStream(stream));
					stream.Close();
				}
				((ManualResetEvent)objEvent).Set();
			}
			catch
			{
				this.exit_thread();
			}
		}

		public void bool_image_count(int num)
		{
			if (num >= 5)
			{
				this.image_num = new int[num];
				if (num - num / 5 * 5 == 0)
				{
					this.image_num[0] = num / 5;
					this.image_num[1] = num / 5 * 2;
					this.image_num[2] = num / 5 * 3;
					this.image_num[3] = num / 5 * 4;
					this.image_num[4] = num;
				}
				if (num - num / 5 * 5 == 1)
				{
					this.image_num[0] = num / 5 + 1;
					this.image_num[1] = num / 5 * 2;
					this.image_num[2] = num / 5 * 3;
					this.image_num[3] = num / 5 * 4;
					this.image_num[4] = num;
				}
				if (num - num / 5 * 5 == 2)
				{
					this.image_num[0] = num / 5 + 1;
					this.image_num[1] = num / 5 * 2 + 1;
					this.image_num[2] = num / 5 * 3;
					this.image_num[3] = num / 5 * 4;
					this.image_num[4] = num;
				}
				if (num - num / 5 * 5 == 3)
				{
					this.image_num[0] = num / 5 + 1;
					this.image_num[1] = num / 5 * 2 + 1;
					this.image_num[2] = num / 5 * 3 + 1;
					this.image_num[3] = num / 5 * 4;
					this.image_num[4] = num;
				}
				if (num - num / 5 * 5 == 4)
				{
					this.image_num[0] = num / 5 + 1;
					this.image_num[1] = num / 5 * 2 + 1;
					this.image_num[2] = num / 5 * 3 + 1;
					this.image_num[3] = num / 5 * 4 + 1;
					this.image_num[4] = num;
				}
			}
			if (num == 4)
			{
				this.image_num = new int[5];
				this.image_num[0] = 1;
				this.image_num[1] = 2;
				this.image_num[2] = 3;
				this.image_num[3] = 4;
				this.image_num[4] = 0;
			}
			if (num == 3)
			{
				this.image_num = new int[5];
				this.image_num[0] = 1;
				this.image_num[1] = 2;
				this.image_num[2] = 3;
				this.image_num[3] = 0;
				this.image_num[4] = 0;
			}
			if (num == 2)
			{
				this.image_num = new int[5];
				this.image_num[0] = 1;
				this.image_num[1] = 2;
				this.image_num[2] = 0;
				this.image_num[3] = 0;
				this.image_num[4] = 0;
			}
			if (num == 1)
			{
				this.image_num = new int[5];
				this.image_num[0] = 1;
				this.image_num[1] = 0;
				this.image_num[2] = 0;
				this.image_num[3] = 0;
				this.image_num[4] = 0;
			}
			if (num == 0)
			{
				this.image_num = new int[5];
				this.image_num[0] = 0;
				this.image_num[1] = 0;
				this.image_num[2] = 0;
				this.image_num[3] = 0;
				this.image_num[4] = 0;
			}
		}

		public void exit_thread()
		{
			try
			{
				StaticValue.截图排斥 = false;
				this.esc = "退出";
				this.fmloading.fml_close = "窗体已关闭";
				this.esc_thread.Abort();
			}
			catch
			{
			}
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.Visible = true;
			base.Show();
			base.WindowState = FormWindowState.Normal;
			if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
			{
				string value = IniHelp.GetValue("快捷键", "翻译文本");
				string text = "None";
				string text2 = "F9";
				this.SetHotkey(text, text2, value, 205);
			}
			HelpWin32.UnregisterHotKey(base.Handle, 222);
		}

		private Image<Gray, byte> randon(Image<Gray, byte> imageInput)
		{
			int width = imageInput.Width;
			int height = imageInput.Height;
			int num = 0;
			int[] array = new int[height];
			Image<Gray, byte> image = imageInput;
			for (int i = -20; i < 20; i++)
			{
				Image<Gray, byte> image2 = imageInput.Rotate((double)i, new Gray(1.0));
				for (int j = 0; j < height; j++)
				{
					int num2 = 0;
					for (int k = 0; k < width; k++)
					{
						num2 += (int)image2.Data[j, k, 0];
					}
					array[j] = num2;
				}
				int num3 = 0;
				for (int l = 0; l < height - 1; l++)
				{
					num3 += Math.Abs(array[l] - array[l + 1]);
				}
				if (num3 > num)
				{
					image = image2;
					num = num3;
				}
			}
			return image;
		}

		public void SetGlobalProxy()
		{
			WebRequest.DefaultWebProxy = null;
		}

		private void tray_null_Proxy_Click(object sender, EventArgs e)
		{
			this.null_Proxy.Text = "不使用代理√";
			this.customize_Proxy.Text = "自定义代理";
			this.system_Proxy.Text = "系统代理";
			this.Proxy_flag = "关闭";
			WebRequest.DefaultWebProxy = null;
		}

		private void tray_system_Proxy_Click(object sender, EventArgs e)
		{
			this.null_Proxy.Text = "不使用代理";
			this.customize_Proxy.Text = "自定义代理";
			this.system_Proxy.Text = "系统代理√";
			this.Proxy_flag = "系统";
			WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
		}

		public void change_pinyin_Click(object sender, EventArgs e)
		{
			this.pinyin_flag = true;
			this.transtalate_Click();
		}

		public string Post_Html_pinyin(string url, string post_str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Timeout = 6000;
			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
				text = streamReader.ReadToEnd();
				responseStream.Close();
				streamReader.Close();
				httpWebRequest.Abort();
			}
			catch
			{
			}
			return text;
		}

		private Bitmap ZoomImage(Bitmap bitmap1, int destHeight, int destWidth)
		{
			double num = (double)bitmap1.Width;
			double num2 = (double)bitmap1.Height;
			if (num < (double)destHeight)
			{
				while (num < (double)destHeight)
				{
					num2 *= 1.1;
					num *= 1.1;
				}
			}
			if (num2 < (double)destWidth)
			{
				while (num2 < (double)destWidth)
				{
					num2 *= 1.1;
					num *= 1.1;
				}
			}
			int num3 = (int)num;
			int num4 = (int)num2;
			Bitmap bitmap2 = new Bitmap(num3, num4);
			Graphics graphics = Graphics.FromImage(bitmap2);
			graphics.DrawImage(bitmap1, 0, 0, num3, num4);
			graphics.Save();
			graphics.Dispose();
			return new Bitmap(bitmap2);
		}

		public void 翻译文本()
		{
			if (IniHelp.GetValue("配置", "快速翻译") == "True")
			{
				string text = "";
				try
				{
					this.trans_hotkey = this.GetTextFromClipboard();
					if (IniHelp.GetValue("配置", "翻译接口") == "谷歌")
					{
						text = this.Translate_Google(this.trans_hotkey);
					}
					if (IniHelp.GetValue("配置", "翻译接口") == "百度")
					{
						text = this.Translate_baidu(this.trans_hotkey);
					}
					if (IniHelp.GetValue("配置", "翻译接口") == "腾讯")
					{
						text = this.Translate_Tencent(this.trans_hotkey);
					}
					Clipboard.SetData(DataFormats.UnicodeText, text);
					SendKeys.SendWait("^v");
					return;
				}
				catch
				{
					Clipboard.SetData(DataFormats.UnicodeText, text);
					SendKeys.SendWait("^v");
					return;
				}
			}
			SendKeys.SendWait("^c");
			SendKeys.Flush();
			this.RichBoxBody.Text = Clipboard.GetText();
			this.transtalate_Click();
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.Visible = true;
			HelpWin32.SetForegroundWindow(StaticValue.mainhandle);
			base.Show();
			base.WindowState = FormWindowState.Normal;
			if (IniHelp.GetValue("工具栏", "顶置") == "True")
			{
				base.TopMost = true;
				return;
			}
			base.TopMost = false;
		}

		public Rectangle[] GetRects(Bitmap pic)
		{
			List<Rectangle> list = new List<Rectangle>();
			bool[][] colors = this.getColors(pic);
			for (int i = 0; i < pic.Height; i++)
			{
				for (int j = 0; j < pic.Width; j++)
				{
					if (this.Exist(colors, i, j))
					{
						Rectangle rect = this.GetRect(colors, i, j);
						if (rect.Width > 10 && rect.Height > 10)
						{
							list.Add(rect);
						}
					}
				}
			}
			return list.ToArray();
		}

		public Bitmap GetRect(Image pic, Rectangle Rect)
		{
			Rectangle rectangle = new Rectangle(0, 0, Rect.Width, Rect.Height);
			Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.FromArgb(0, 0, 0, 0));
			graphics.DrawImage(pic, rectangle, Rect, GraphicsUnit.Pixel);
			graphics.Dispose();
			return bitmap;
		}

		private Bitmap[] getSubPics(Image buildPic, Rectangle[] buildRects)
		{
			Bitmap[] array = new Bitmap[buildRects.Length];
			for (int i = 0; i < buildRects.Length; i++)
			{
				array[i] = this.GetRect(buildPic, buildRects[i]);
				string text = IniHelp.GetValue("配置", "截图位置") + "\\" + FmMain.ReFileName(IniHelp.GetValue("配置", "截图位置"), "图片.Png");
				array[i].Save(text, ImageFormat.Png);
			}
			return array;
		}

		public bool[][] getColors(Bitmap pic)
		{
			bool[][] array = new bool[pic.Height][];
			for (int i = 0; i < pic.Height; i++)
			{
				array[i] = new bool[pic.Width];
				for (int j = 0; j < pic.Width; j++)
				{
					Color pixel = pic.GetPixel(j, i);
					int num = 0;
					if (pixel.R < 4)
					{
						num++;
					}
					if (pixel.G < 4)
					{
						num++;
					}
					if (pixel.B < 4)
					{
						num++;
					}
					if (pixel.A < 3 || (num >= 2 && pixel.A < 30))
					{
						array[i][j] = false;
					}
					else
					{
						array[i][j] = true;
					}
				}
			}
			return array;
		}

		public bool Exist(bool[][] Colors, int x, int y)
		{
			return x >= 0 && y >= 0 && x < Colors.Length && y < Colors[0].Length && Colors[x][y];
		}

		public bool R_Exist(bool[][] Colors, Rectangle Rect)
		{
			if (Rect.Right >= Colors[0].Length || Rect.Left < 0)
			{
				return false;
			}
			for (int i = 0; i < Rect.Height; i++)
			{
				if (this.Exist(Colors, Rect.Top + i, Rect.Right + 1))
				{
					return true;
				}
			}
			return false;
		}

		public bool D_Exist(bool[][] Colors, Rectangle Rect)
		{
			if (Rect.Bottom >= Colors.Length || Rect.Top < 0)
			{
				return false;
			}
			for (int i = 0; i < Rect.Width; i++)
			{
				if (this.Exist(Colors, Rect.Bottom + 1, Rect.Left + i))
				{
					return true;
				}
			}
			return false;
		}

		public bool L_Exist(bool[][] Colors, Rectangle Rect)
		{
			if (Rect.Right >= Colors[0].Length || Rect.Left < 0)
			{
				return false;
			}
			for (int i = 0; i < Rect.Height; i++)
			{
				if (this.Exist(Colors, Rect.Top + i, Rect.Left - 1))
				{
					return true;
				}
			}
			return false;
		}

		public bool U_Exist(bool[][] Colors, Rectangle Rect)
		{
			if (Rect.Bottom >= Colors.Length || Rect.Top < 0)
			{
				return false;
			}
			for (int i = 0; i < Rect.Width; i++)
			{
				if (this.Exist(Colors, Rect.Top - 1, Rect.Left + i))
				{
					return true;
				}
			}
			return false;
		}

		public Rectangle GetRect(bool[][] Colors, int x, int y)
		{
			Rectangle rectangle = new Rectangle(new Point(y, x), new Size(1, 1));
			bool flag;
			int num;
			do
			{
				flag = false;
				while (this.R_Exist(Colors, rectangle))
				{
					num = rectangle.Width;
					rectangle.Width = num + 1;
					flag = true;
				}
				while (this.D_Exist(Colors, rectangle))
				{
					num = rectangle.Height;
					rectangle.Height = num + 1;
					flag = true;
				}
				while (this.L_Exist(Colors, rectangle))
				{
					num = rectangle.Width;
					rectangle.Width = num + 1;
					num = rectangle.X;
					rectangle.X = num - 1;
					flag = true;
				}
				while (this.U_Exist(Colors, rectangle))
				{
					num = rectangle.Height;
					rectangle.Height = num + 1;
					num = rectangle.Y;
					rectangle.Y = num - 1;
					flag = true;
				}
			}
			while (flag);
			this.clearRect(Colors, rectangle);
			num = rectangle.Width;
			rectangle.Width = num + 1;
			num = rectangle.Height;
			rectangle.Height = num + 1;
			return rectangle;
		}

		public void clearRect(bool[][] Colors, Rectangle Rect)
		{
			for (int i = Rect.Top; i <= Rect.Bottom; i++)
			{
				for (int j = Rect.Left; j <= Rect.Right; j++)
				{
					Colors[i][j] = false;
				}
			}
		}

		public static string ReFileNamekey(string strFilePath)
		{
			int num = strFilePath.LastIndexOf('.');
			string text = strFilePath.Insert(num, "_{0}");
			int num2 = 1;
			string text2 = string.Format(text, num2);
			while (File.Exists(text2))
			{
				text2 = string.Format(text, num2);
				num2++;
			}
			return text2;
		}

		private Bitmap[] getSubPics_ocr(Image buildPic, Rectangle[] buildRects)
		{
			string text = "";
			Bitmap[] array = new Bitmap[buildRects.Length];
			string text2 = "";
			for (int i = 0; i < buildRects.Length; i++)
			{
				array[i] = this.GetRect(buildPic, buildRects[i]);
				this.image_screen = array[i];
				Messageload messageload = new Messageload();
				messageload.ShowDialog();
				if (messageload.DialogResult == DialogResult.OK)
				{
					if (this.interface_flag == "搜狗")
					{
						this.OCR_sougou2();
					}
					if (this.interface_flag == "腾讯")
					{
						this.OCR_Tencent();
					}
					if (this.interface_flag == "有道")
					{
						this.OCR_youdao();
					}
					if (this.interface_flag == "日语" || this.interface_flag == "中英" || this.interface_flag == "韩语")
					{
						this.OCR_baidu();
					}
					messageload.Dispose();
				}
				if (IniHelp.GetValue("工具栏", "分栏") == "True")
				{
					if (this.paragraph)
					{
						text = text + "\r\n" + this.typeset_txt.Trim();
						text2 = text2 + "\r\n" + this.split_txt.Trim() + "\r\n";
					}
					else
					{
						text += this.typeset_txt.Trim();
						text2 = text2 + "\r\n" + this.split_txt.Trim() + "\r\n";
					}
				}
				else if (this.paragraph)
				{
					text = text + "\r\n" + this.typeset_txt.Trim() + "\r\n";
					text2 = text2 + "\r\n" + this.split_txt.Trim() + "\r\n";
				}
				else
				{
					text = text + this.typeset_txt.Trim() + "\r\n";
					text2 = text2 + "\r\n" + this.split_txt.Trim() + "\r\n";
				}
			}
			this.typeset_txt = text.Replace("\r\n\r\n", "\r\n");
			this.split_txt = text2.Replace("\r\n\r\n", "\r\n");
			this.fmloading.fml_close = "窗体已关闭";
			base.Invoke(new FmMain.ocr_thread(this.Main_OCR_Thread_last));
			return array;
		}

		public void OCR_sougou_bat(Bitmap image_screen)
		{
			try
			{
				this.split_txt = "";
				string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
				Image image = this.ZoomImage(image_screen, 120, 120);
				byte[] array = this.OCR_ImgToByte(image);
				string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
				string text3 = "\r\n" + text + "--\r\n";
				byte[] bytes = Encoding.ASCII.GetBytes(text2);
				byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
				byte[] array2 = FmMain.Mergebyte(bytes, array, bytes2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://ocr.shouji.sogou.com/v2/ocr/json");
				httpWebRequest.Timeout = 8000;
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(array2, 0, array2.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(text4))["result"].ToString());
				this.checked_txt(jarray, 2, "content");
				image.Dispose();
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public Image BoundingBox_fences(Image<Gray, byte> src, Image<Bgr, byte> draw)
		{
			Image image2;
			using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default(Point));
				Image image = draw.ToBitmap();
				Graphics graphics = Graphics.FromImage(image);
				int size = vectorOfVectorOfPoint.Size;
				for (int i = 0; i < size; i++)
				{
					using (VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i])
					{
						Rectangle rectangle = CvInvoke.BoundingRectangle(vectorOfPoint);
						int x = rectangle.Location.X;
						int y = rectangle.Location.Y;
						int width = rectangle.Size.Width;
						int height = rectangle.Size.Height;
						graphics.FillRectangle(Brushes.White, x, 0, width, draw.Height);
					}
				}
				graphics.Dispose();
				Bitmap bitmap = new Bitmap(image.Width + 2, image.Height + 2);
				Graphics graphics2 = Graphics.FromImage(bitmap);
				graphics2.DrawImage(image, 1, 1, image.Width, image.Height);
				graphics2.Save();
				graphics2.Dispose();
				image.Dispose();
				src.Dispose();
				image2 = bitmap;
			}
			return image2;
		}

		public Image FindBundingBox_fences(Bitmap bitmap)
		{
			Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
			Image<Gray, byte> image2 = new Image<Gray, byte>(image.Width, image.Height);
			CvInvoke.CvtColor(image, image2, ColorConversion.Bgra2Gray, 0);
			Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(6, 20), new Point(1, 1));
			CvInvoke.Erode(image2, image2, structuringElement, new Point(0, 2), 1, BorderType.Reflect101, default(MCvScalar));
			CvInvoke.Threshold(image2, image2, 100.0, 255.0, (ThresholdType)9);
			Image<Gray, byte> image3 = new Image<Gray, byte>(image2.ToBitmap());
			Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>();
			Image<Gray, byte> image5 = image3.Clone();
			CvInvoke.Canny(image3, image5, 255.0, 255.0, 5, true);
			Image image6 = this.BoundingBox_fences(image5, image4);
			Image<Gray, byte> image7 = new Image<Gray, byte>((Bitmap)image6);
			this.BoundingBox_fences_Up(image7);
			image.Dispose();
			image2.Dispose();
			image3.Dispose();
			image7.Dispose();
			return image6;
		}

		public void BoundingBox_fences_Up(Image<Gray, byte> src)
		{
			using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default(Point));
				int size = vectorOfVectorOfPoint.Size;
				Rectangle[] array = new Rectangle[size];
				for (int i = 0; i < size; i++)
				{
					using (VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i])
					{
						array[size - 1 - i] = CvInvoke.BoundingRectangle(vectorOfPoint);
					}
				}
				this.getSubPics_ocr(this.image_screen, array);
			}
		}

		public void checked_location_txt(JArray jarray, int lastlength, string words)
		{
			int num = 0;
			for (int i = 0; i < jarray.Count; i++)
			{
				int length = JObject.Parse(jarray[i].ToString())[words].ToString().Length;
				if (length > num)
				{
					num = length;
				}
			}
			string text = "";
			string text2 = "";
			for (int j = 0; j < jarray.Count - 1; j++)
			{
				JObject jobject = JObject.Parse(jarray[j].ToString());
				char[] array = jobject[words].ToString().ToCharArray();
				JObject jobject2 = JObject.Parse(jarray[j + 1].ToString());
				char[] array2 = jobject2[words].ToString().ToCharArray();
				int length2 = jobject[words].ToString().Length;
				int length3 = jobject2[words].ToString().Length;
				if (Math.Abs(length2 - length3) <= 0)
				{
					if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && this.Is_punctuation(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else
					{
						text2 += jobject[words].ToString().Trim();
					}
				}
				else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && Math.Abs(length2 - length3) <= 1)
				{
					if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else if (this.split_paragraph(array[array.Length - lastlength].ToString()) && this.Is_punctuation(array2[0].ToString()))
					{
						text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
					}
					else
					{
						text2 += jobject[words].ToString().Trim();
					}
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && length2 <= num / 2)
				{
					text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()) && length3 - length2 < 4 && array2[1].ToString() == ".")
				{
					text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.contain_en(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
				{
					text2 = text2 + jobject[words].ToString().Trim() + " ";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.contain_en(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && this.Is_punctuation(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (this.Is_punctuation(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (this.Is_punctuation(array[array.Length - lastlength].ToString()) && FmMain.contain_en(array2[0].ToString()))
				{
					text2 = text2 + jobject[words].ToString().Trim() + " ";
				}
				else if (FmMain.contain_ch(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.IsNum(array[array.Length - lastlength].ToString()) && FmMain.contain_ch(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else if (FmMain.IsNum(array[array.Length - lastlength].ToString()) && FmMain.IsNum(array2[0].ToString()))
				{
					text2 += jobject[words].ToString().Trim();
				}
				else
				{
					text2 = text2 + jobject[words].ToString().Trim() + "\r\n";
				}
				if (this.has_punctuation(jobject[words].ToString()))
				{
					text2 += "\r\n";
				}
				text = text + jobject[words].ToString().Trim() + "\r\n";
			}
			this.split_txt = text + JObject.Parse(jarray[jarray.Count - 1].ToString())[words];
			this.typeset_txt = text2.Replace("\r\n\r\n", "\r\n") + JObject.Parse(jarray[jarray.Count - 1].ToString())[words];
		}

		public void checked_location_sougou(JArray jarray, int lastlength, string words, string location)
		{
			this.paragraph = false;
			int num = 20000;
			int num2 = 0;
			for (int i = 0; i < jarray.Count; i++)
			{
				JObject jobject = JObject.Parse(jarray[i].ToString());
				int num3 = this.split_char_x(jobject[location][1].ToString()) - this.split_char_x(jobject[location][0].ToString());
				if (num3 > num2)
				{
					num2 = num3;
				}
				int num4 = this.split_char_x(jobject[location][0].ToString());
				if (num4 < num)
				{
					num = num4;
				}
			}
			JObject jobject2 = JObject.Parse(jarray[0].ToString());
			if (Math.Abs(this.split_char_x(jobject2[location][0].ToString()) - num) > 10)
			{
				this.paragraph = true;
			}
			string text = "";
			string text2 = "";
			for (int j = 0; j < jarray.Count; j++)
			{
				JObject jobject3 = JObject.Parse(jarray[j].ToString());
				char[] array = jobject3[words].ToString().ToCharArray();
				JObject jobject4 = JObject.Parse(jarray[j].ToString());
				bool flag = Math.Abs(this.split_char_x(jobject4[location][1].ToString()) - this.split_char_x(jobject4[location][0].ToString()) - num2) > 20;
				bool flag2 = Math.Abs(this.split_char_x(jobject4[location][0].ToString()) - num) > 10;
				if (flag && flag2)
				{
					text = text.Trim() + "\r\n" + jobject4[words].ToString().Trim();
				}
				else if (FmMain.IsNum(array[0].ToString()) && !FmMain.contain_ch(array[1].ToString()) && flag)
				{
					text = text.Trim() + "\r\n" + jobject4[words].ToString().Trim() + "\r\n";
				}
				else
				{
					text += jobject4[words].ToString().Trim();
				}
				if (FmMain.contain_en(array[array.Length - lastlength].ToString()))
				{
					text = text + jobject3[words].ToString().Trim() + " ";
				}
				text2 = text2 + jobject4[words].ToString().Trim() + "\r\n";
			}
			this.split_txt = text2.Replace("\r\n\r\n", "\r\n");
			this.typeset_txt = text;
		}

		public int split_char_x(string split_char)
		{
			return Convert.ToInt32(split_char.Split(new char[] { ',' })[0]);
		}

		private void tray_double_Click(object sender, EventArgs e)
		{
			HelpWin32.UnregisterHotKey(base.Handle, 205);
			this.menu.Hide();
			this.RichBoxBody.Hide = "";
			this.RichBoxBody_T.Hide = "";
			this.Main_OCR_Quickscreenshots();
		}

		public int en_count(string text)
		{
			return Regex.Matches(text, "\\s+").Count + 1;
		}

		public int ch_count(string str)
		{
			int num = 0;
			Regex regex = new Regex("^[\\u4E00-\\u9FA5]{0,}$");
			for (int i = 0; i < str.Length; i++)
			{
				if (regex.IsMatch(str[i].ToString()))
				{
					num++;
				}
			}
			return num;
		}

		public void checked_location_youdao(JArray jarray, int lastlength, string words, string location)
		{
			this.paragraph = false;
			int num = 20000;
			int num2 = 0;
			for (int i = 0; i < jarray.Count; i++)
			{
				JObject jobject = JObject.Parse(jarray[i].ToString());
				int num3 = this.split_char_youdao(jobject[location].ToString(), 3) - this.split_char_youdao(jobject[location].ToString(), 1);
				if (num3 > num2)
				{
					num2 = num3;
				}
				int num4 = this.split_char_youdao(jobject[location].ToString(), 1);
				if (num4 < num)
				{
					num = num4;
				}
			}
			JObject jobject2 = JObject.Parse(jarray[0].ToString());
			if (Math.Abs(this.split_char_youdao(jobject2[location].ToString(), 1) - num) > 10)
			{
				this.paragraph = true;
			}
			string text = "";
			string text2 = "";
			for (int j = 0; j < jarray.Count; j++)
			{
				JObject jobject3 = JObject.Parse(jarray[j].ToString());
				char[] array = jobject3[words].ToString().ToCharArray();
				JObject jobject4 = JObject.Parse(jarray[j].ToString());
				bool flag = Math.Abs(this.split_char_youdao(jobject4[location].ToString(), 3) - this.split_char_youdao(jobject4[location].ToString(), 1) - num2) > 20;
				bool flag2 = Math.Abs(this.split_char_youdao(jobject4[location].ToString(), 1) - num) > 10;
				if (flag && flag2)
				{
					text = text.Trim() + "\r\n" + jobject4[words].ToString().Trim();
				}
				else if (FmMain.IsNum(array[0].ToString()) && !FmMain.contain_ch(array[1].ToString()) && flag)
				{
					text = text.Trim() + "\r\n" + jobject4[words].ToString().Trim() + "\r\n";
				}
				else
				{
					text += jobject4[words].ToString().Trim();
				}
				if (FmMain.contain_en(array[array.Length - lastlength].ToString()))
				{
					text = text + jobject3[words].ToString().Trim() + " ";
				}
				text2 = text2 + jobject4[words].ToString().Trim() + "\r\n";
			}
			this.split_txt = text2.Replace("\r\n\r\n", "\r\n");
			this.typeset_txt = text;
		}

		public int split_char_youdao(string split_char, int i)
		{
			return Convert.ToInt32(split_char.Split(new char[] { ',' })[i - 1]);
		}

		public void Trans_google_Click(object sender, EventArgs e)
		{
			this.Trans_foreach("谷歌");
		}

		public void Trans_baidu_Click(object sender, EventArgs e)
		{
			this.Trans_foreach("百度");
		}

		private void Trans_foreach(string name)
		{
			if (name == "百度")
			{
				this.trans_baidu.Text = "百度√";
				this.trans_google.Text = "谷歌";
				this.trans_tencent.Text = "腾讯";
				IniHelp.SetValue("配置", "翻译接口", "百度");
			}
			if (name == "谷歌")
			{
				this.trans_baidu.Text = "百度";
				this.trans_google.Text = "谷歌√";
				this.trans_tencent.Text = "腾讯";
				IniHelp.SetValue("配置", "翻译接口", "谷歌");
			}
			if (name == "腾讯")
			{
				this.trans_google.Text = "谷歌";
				this.trans_baidu.Text = "百度";
				this.trans_tencent.Text = "腾讯√";
				IniHelp.SetValue("配置", "翻译接口", "腾讯");
			}
		}

		public string GetBaiduHtml(string url, CookieContainer cookie, string refer, string content_length)
		{
			string text2;
			try
			{
				string text = "";
				HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = refer;
				httpWebRequest.Timeout = 1500;
				httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				byte[] bytes = Encoding.UTF8.GetBytes(content_length);
				Stream requestStream = httpWebRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				text2 = text;
			}
			catch
			{
				text2 = this.GetBaiduHtml(url, cookie, refer, content_length);
			}
			return text2;
		}

		private string Translate_baidu(string Text)
		{
			string text = "";
			try
			{
				new CookieContainer();
				string text2 = "zh";
				string text3 = "en";
				if (StaticValue.zh_en)
				{
					if (this.ch_count(Text.Trim()) > this.en_count(Text.Trim()) || (this.en_count(text.Trim()) == 1 && this.ch_count(text.Trim()) == 1))
					{
						text2 = "zh";
						text3 = "en";
					}
					else
					{
						text2 = "en";
						text3 = "zh";
					}
				}
				if (StaticValue.zh_jp)
				{
					if (FmMain.contain_jap(FmMain.repalceStr(FmMain.Del_ch(Text.Trim()))))
					{
						text2 = "jp";
						text3 = "zh";
					}
					else
					{
						text2 = "zh";
						text3 = "jp";
					}
				}
				if (StaticValue.zh_ko)
				{
					if (FmMain.contain_kor(Text.Trim()))
					{
						text2 = "kor";
						text3 = "zh";
					}
					else
					{
						text2 = "zh";
						text3 = "kor";
					}
				}
				HttpHelper httpHelper = new HttpHelper();
				HttpItem httpItem = new HttpItem
				{
					URL = "https://fanyi.baidu.com/basetrans",
					Method = "post",
					ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
					Postdata = string.Concat(new string[]
					{
						"query=",
						HttpUtility.UrlEncode(Text.Trim()).Replace("+", "%20"),
						"&from=",
						text2,
						"&to=",
						text3
					}),
					UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Mobile Safari/537.36"
				};
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(httpHelper.GetHtml(httpItem).Html))["trans"].ToString());
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text = text + jobject["dst"] + "\r\n";
				}
			}
			catch (Exception)
			{
				text = "[百度接口报错]：\r\n1.接口请求出现问题等待修复。";
			}
			return text;
		}

		public void Trans_tencent_Click(object sender, EventArgs e)
		{
			this.Trans_foreach("腾讯");
		}

		public string Content_Length(string text, string fromlang, string tolang)
		{
			return string.Concat(new string[]
			{
				"&source=",
				fromlang,
				"&target=",
				tolang,
				"&sourceText=",
				HttpUtility.UrlEncode(text).Replace("+", "%20")
			});
		}

		public string TencentPOST(string url, string content)
		{
			string text2;
			try
			{
				string text = "";
				HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = "https://fanyi.qq.com/";
				httpWebRequest.Timeout = 5000;
				httpWebRequest.Accept = "application/json, text/javascript, */*; q=0.01";
				httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
				httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest.Headers.Add("Accept-Language: zh-CN,zh;q=0.9");
				httpWebRequest.Headers.Add("cookie:" + FmMain.GetCookies("http://fanyi.qq.com"));
				byte[] bytes = Encoding.UTF8.GetBytes(content);
				httpWebRequest.ContentLength = (long)bytes.Length;
				Stream requestStream = httpWebRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
					{
						text = streamReader.ReadToEnd();
						streamReader.Close();
						httpWebResponse.Close();
					}
				}
				text2 = text;
				if (text.Contains("\"records\":[]"))
				{
					Thread.Sleep(8);
					return this.TencentPOST(url, content);
				}
			}
			catch
			{
				text2 = "[腾讯接口报错]：\r\n请切换其它接口或再次尝试。";
			}
			return text2;
		}

		private string Translate_Tencent(string strtrans)
		{
			string text = "";
			try
			{
				string text2 = "zh";
				string text3 = "en";
				if (StaticValue.zh_en)
				{
					if (this.ch_count(strtrans.Trim()) > this.en_count(strtrans.Trim()) || (this.en_count(text.Trim()) == 1 && this.ch_count(text.Trim()) == 1))
					{
						text2 = "zh";
						text3 = "en";
					}
					else
					{
						text2 = "en";
						text3 = "zh";
					}
				}
				if (StaticValue.zh_jp)
				{
					if (FmMain.contain_jap(FmMain.repalceStr(FmMain.Del_ch(strtrans.Trim()))))
					{
						text2 = "jp";
						text3 = "zh";
					}
					else
					{
						text2 = "zh";
						text3 = "jp";
					}
				}
				if (StaticValue.zh_ko)
				{
					if (FmMain.contain_kor(strtrans.Trim()))
					{
						text2 = "kr";
						text3 = "zh";
					}
					else
					{
						text2 = "zh";
						text3 = "kr";
					}
				}
				JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(this.TencentPOST("https://fanyi.qq.com/api/translate", this.Content_Length(strtrans, text2, text3))))["translate"]["records"].ToString());
				for (int i = 0; i < jarray.Count; i++)
				{
					JObject jobject = JObject.Parse(jarray[i].ToString());
					text += jobject["targetText"].ToString();
				}
			}
			catch (Exception)
			{
				text = "[腾讯接口报错]：\r\n1.接口请求出现问题等待修复。";
			}
			return text;
		}

		public void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
		}

		private static string GetCookies(string url)
		{
			uint num = 1024U;
			StringBuilder stringBuilder = new StringBuilder((int)num);
			if (!FmMain.InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, IntPtr.Zero))
			{
				if (num < 0U)
				{
					return null;
				}
				stringBuilder = new StringBuilder((int)num);
				if (!FmMain.InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, IntPtr.Zero))
				{
					return null;
				}
			}
			return stringBuilder.ToString();
		}

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref uint pcchCookieData, int dwFlags, IntPtr lpReserved);

		public string Post_GoogletHtml(string post_str)
		{
			string text = "";
			string text2 = "https://translate.google.cn/translate_a/single";
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			HttpWebRequest httpWebRequest = WebRequest.Create(text2) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Timeout = 5000;
			httpWebRequest.Host = "translate.google.cn";
			httpWebRequest.Accept = "*/*";
			httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)";
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
				text = streamReader.ReadToEnd();
				responseStream.Close();
				streamReader.Close();
				httpWebRequest.Abort();
			}
			catch
			{
			}
			return text;
		}

		public void httpDownload(string URL, string filename)
		{
			try
			{
				HttpWebResponse httpWebResponse = (HttpWebResponse)((HttpWebRequest)WebRequest.Create(URL)).GetResponse();
				long contentLength = httpWebResponse.ContentLength;
				Stream responseStream = httpWebResponse.GetResponseStream();
				Stream stream = new FileStream(filename, FileMode.Create);
				long num = 0L;
				byte[] array = new byte[2048];
				for (int i = responseStream.Read(array, 0, array.Length); i > 0; i = responseStream.Read(array, 0, array.Length))
				{
					num = (long)i + num;
					stream.Write(array, 0, i);
				}
				stream.Close();
				responseStream.Close();
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void OCR_baidu_table()
		{
			this.typeset_txt = "[消息]：表格已下载！";
			this.split_txt = "";
			try
			{
				this.baidu_vip = FmMain.Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + StaticValue.baiduAPI_ID + "&client_secret=" + StaticValue.baiduAPI_key));
				if (this.baidu_vip == "")
				{
					MessageBox.Show("请检查密钥输入是否正确！", "提醒");
				}
				else
				{
					this.split_txt = "";
					Image image = this.image_screen;
					MemoryStream memoryStream = new MemoryStream();
					image.Save(memoryStream, ImageFormat.Jpeg);
					byte[] array = new byte[memoryStream.Length];
					memoryStream.Position = 0L;
					memoryStream.Read(array, 0, (int)memoryStream.Length);
					memoryStream.Close();
					string text = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array));
					byte[] bytes = Encoding.UTF8.GetBytes(text);
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://aip.baidubce.com/rest/2.0/solution/v1/form_ocr/request?access_token=" + ((JObject)JsonConvert.DeserializeObject(this.baidu_vip))["access_token"]);
					httpWebRequest.Proxy = null;
					httpWebRequest.Method = "POST";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded";
					httpWebRequest.Timeout = 8000;
					httpWebRequest.ReadWriteTimeout = 5000;
					using (Stream requestStream = httpWebRequest.GetRequestStream())
					{
						requestStream.Write(bytes, 0, bytes.Length);
					}
					Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
					string text2 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
					responseStream.Close();
					string text3 = "request_id=" + JObject.Parse(JArray.Parse(((JObject)JsonConvert.DeserializeObject(text2))["result"].ToString())[0].ToString())["request_id"].ToString().Trim() + "&result_type=json";
					string text4 = "";
					while (!text4.Contains("已完成"))
					{
						if (text4.Contains("image recognize error"))
						{
							this.RichBoxBody.Text = "[消息]：未发现表格！";
							break;
						}
						Thread.Sleep(120);
						text4 = this.Post_Html("https://aip.baidubce.com/rest/2.0/solution/v1/form_ocr/get_request_result?access_token=" + ((JObject)JsonConvert.DeserializeObject(this.baidu_vip))["access_token"], text3);
					}
					if (!text4.Contains("image recognize error"))
					{
						this.get_table(text4);
					}
				}
			}
			catch
			{
				this.RichBoxBody.Text = "[消息]：免费百度密钥50次已经耗完！请更换自己的密钥继续使用！";
			}
		}

		public void OCR_table_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("表格");
		}

		private void get_table(string str)
		{
			JArray jarray = JArray.Parse(((JObject)JsonConvert.DeserializeObject(((JObject)JsonConvert.DeserializeObject(str))["result"]["result_data"].ToString().Replace("\\", "")))["forms"][0]["body"].ToString());
			int[] array = new int[jarray.Count];
			int[] array2 = new int[jarray.Count];
			for (int i = 0; i < jarray.Count; i++)
			{
				JObject jobject = JObject.Parse(jarray[i].ToString());
				string text = jobject["column"].ToString().Replace("[", "").Replace("]", "")
					.Replace("\r", "")
					.Replace("\n", "")
					.Trim();
				string text2 = jobject["row"].ToString().Replace("[", "").Replace("]", "")
					.Replace("\r", "")
					.Replace("\n", "")
					.Trim();
				array[i] = Convert.ToInt32(text);
				array2[i] = Convert.ToInt32(text2);
			}
			string[,] array3 = new string[array2.Max() + 1, array.Max() + 1];
			for (int j = 0; j < jarray.Count; j++)
			{
				JObject jobject2 = JObject.Parse(jarray[j].ToString());
				string text3 = jobject2["column"].ToString().Replace("[", "").Replace("]", "")
					.Replace("\r", "")
					.Replace("\n", "")
					.Trim();
				string text4 = jobject2["row"].ToString().Replace("[", "").Replace("]", "")
					.Replace("\r", "")
					.Replace("\n", "")
					.Trim();
				array[j] = Convert.ToInt32(text3);
				array2[j] = Convert.ToInt32(text4);
				string text5 = jobject2["word"].ToString().Replace("[", "").Replace("]", "")
					.Replace("\r", "")
					.Replace("\n", "")
					.Trim();
				array3[Convert.ToInt32(text4), Convert.ToInt32(text3)] = text5;
			}
			Graphics graphics = base.CreateGraphics();
			int[] array4 = new int[array.Max() + 1];
			int num = 0;
			SizeF sizeF = new SizeF(10f, 10f);
			int num2 = Screen.PrimaryScreen.Bounds.Width / 4;
			for (int k = 0; k < array3.GetLength(1); k++)
			{
				for (int l = 0; l < array3.GetLength(0); l++)
				{
					sizeF = graphics.MeasureString(array3[l, k], new Font("宋体", 12f));
					if (num < (int)sizeF.Width)
					{
						num = (int)sizeF.Width;
					}
					if (num > num2)
					{
						num = num2;
					}
				}
				array4[k] = num;
				num = 0;
			}
			graphics.Dispose();
			this.setClipboard_Table(array3, array4);
		}

		private void setClipboard_table(string[,] wordo)
		{
			string text = "{\\rtf1\\ansi\\ansicpg936\\deff0\\deflang1033\\deflangfe2052{\\fonttbl{\\f0\\fnil\\fprq2\\fcharset134";
			text += "\\'cb\\'ce\\'cc\\'e5;}{\\f1\\fnil\\fcharset134 \\'cb\\'ce\\'cc\\'e5;}}\\viewkind4\\uc1\\trowd\\trgaph108\\trleft-108";
			text += "\\trbrdrt\\brdrs\\brdrw10 \\trbrdrl\\brdrs\\brdrw10 \\trbrdrb\\brdrs\\brdrw10 \\trbrdrb\\brdrs\\brdrw10 ";
			for (int i = 1; i <= wordo.GetLength(1); i++)
			{
				text = text + "\\clbrdrt\\brdrw15\\brdrs\\clbrdrl\\brdrw15\\brdrs\\clbrdrb\\brdrw15\\brdrs\\clbrdrr\\brdrw15\\brdrs \\cellx" + i * 1800;
			}
			string text2 = "";
			string text3 = "\\pard\\intbl\\kerning2\\f0";
			string text4 = "\\row\\pard\\lang2052\\kerning0\\f1\\fs18\\par}";
			for (int j = 0; j < wordo.GetLength(0); j++)
			{
				for (int k = 0; k < wordo.GetLength(1); k++)
				{
					if (k == 0)
					{
						text2 = text2 + "\\fs24 " + wordo[j, k];
					}
					else
					{
						text2 = text2 + "\\cell " + wordo[j, k];
					}
				}
				if (j != wordo.GetLength(0) - 1)
				{
					text2 += "\\row\\intbl";
				}
			}
			this.RichBoxBody.rtf = text + text3 + text2 + text4;
		}

		public void Main_OCR_Thread_table()
		{
			this.ailibaba = new AliTable();
			TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
			TimeSpan timeSpan2 = timeSpan.Subtract(this.ts).Duration();
			string text = string.Concat(new string[]
			{
				timeSpan2.Seconds.ToString(),
				".",
				Convert.ToInt32(timeSpan2.TotalMilliseconds).ToString(),
				"秒"
			});
			if (StaticValue.v_topmost)
			{
				base.TopMost = true;
			}
			else
			{
				base.TopMost = false;
			}
			this.Text = "耗时：" + text;
			if (this.interface_flag == "百度表格")
			{
				DataObject dataObject = new DataObject();
				dataObject.SetData(DataFormats.Rtf, this.RichBoxBody.rtf);
				dataObject.SetData(DataFormats.UnicodeText, this.RichBoxBody.Text);
				this.RichBoxBody.Text = "[消息]：表格已复制到粘贴板！";
				Clipboard.SetDataObject(dataObject);
			}
			this.image_screen.Dispose();
			GC.Collect();
			StaticValue.截图排斥 = false;
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.Visible = true;
			base.Show();
			base.WindowState = FormWindowState.Normal;
			base.Size = new Size(this.form_width, this.form_height);
			HelpWin32.SetForegroundWindow(base.Handle);
			if (this.interface_flag == "阿里表格")
			{
				if (this.split_txt == "弹出cookie")
				{
					this.split_txt = "";
					this.ailibaba.TopMost = true;
					this.ailibaba.getcookie = "";
					IniHelp.SetValue("特殊", "ali_cookie", this.ailibaba.getcookie);
					this.ailibaba.ShowDialog();
					HelpWin32.SetForegroundWindow(this.ailibaba.Handle);
					return;
				}
				Clipboard.SetDataObject(this.typeset_txt);
				this.CopyHtmlToClipBoard(this.typeset_txt);
			}
		}

		private void setClipboard_Table(string[,] wordo, int[] cc)
		{
			string text = "{\\rtf1\\ansi\\ansicpg936\\deff0\\deflang1033\\deflangfe2052{\\fonttbl{\\f0\\fnil\\fprq2\\fcharset134";
			text += "\\'cb\\'ce\\'cc\\'e5;}{\\f1\\fnil\\fcharset134 \\'cb\\'ce\\'cc\\'e5;}}\\viewkind4\\uc1\\trowd\\trgaph108\\trleft-108";
			text += "\\trbrdrt\\brdrs\\brdrw10 \\trbrdrl\\brdrs\\brdrw10 \\trbrdrb\\brdrs\\brdrw10 \\trbrdrb\\brdrs\\brdrw10 ";
			int num = 0;
			for (int i = 1; i <= cc.Length; i++)
			{
				num += cc[i - 1] * 17;
				text = text + "\\clbrdrt\\brdrw15\\brdrs\\clbrdrl\\brdrw15\\brdrs\\clbrdrb\\brdrw15\\brdrs\\clbrdrr\\brdrw15\\brdrs \\cellx" + num;
			}
			string text2 = "";
			string text3 = "\\pard\\intbl\\kerning2\\f0";
			string text4 = "\\row\\pard\\lang2052\\kerning0\\f1\\fs18\\par}";
			for (int j = 0; j < wordo.GetLength(0); j++)
			{
				for (int k = 0; k < wordo.GetLength(1); k++)
				{
					if (k == 0)
					{
						text2 = text2 + "\\fs24 " + wordo[j, k];
					}
					else
					{
						text2 = text2 + "\\cell " + wordo[j, k];
					}
				}
				if (j != wordo.GetLength(0) - 1)
				{
					text2 += "\\row\\intbl";
				}
			}
			this.RichBoxBody.rtf = text + text3 + text2 + text4;
		}

		public string Translate_Googlekey(string text)
		{
			string text2 = "";
			try
			{
				string text3 = "zh-CN";
				string text4 = "en";
				if (StaticValue.zh_en)
				{
					if (this.ch_count(this.typeset_txt.Trim()) > this.en_count(this.typeset_txt.Trim()))
					{
						text3 = "zh-CN";
						text4 = "en";
					}
					else
					{
						text3 = "en";
						text4 = "zh-CN";
					}
				}
				if (StaticValue.zh_jp)
				{
					if (FmMain.contain_jap(FmMain.repalceStr(FmMain.Del_ch(this.typeset_txt.Trim()))))
					{
						text3 = "ja";
						text4 = "zh-CN";
					}
					else
					{
						text3 = "zh-CN";
						text4 = "ja";
					}
				}
				if (StaticValue.zh_ko)
				{
					if (FmMain.contain_kor(this.typeset_txt.Trim()))
					{
						text3 = "ko";
						text4 = "zh-CN";
					}
					else
					{
						text3 = "zh-CN";
						text4 = "ko";
					}
				}
				string text5 = string.Concat(new string[]
				{
					"client=gtx&sl=",
					text3,
					"&tl=",
					text4,
					"&dt=t&q=",
					HttpUtility.UrlEncode(text).Replace("+", "%20")
				});
				JArray jarray = (JArray)JsonConvert.DeserializeObject(this.Post_GoogletHtml(text5));
				int count = ((JArray)jarray[0]).Count;
				for (int i = 0; i < count; i++)
				{
					text2 += jarray[0][i][0].ToString();
				}
			}
			catch (Exception)
			{
				text2 = "[谷歌接口报错]：\r\n出现这个提示文字，表示您当前的网络不适合使用谷歌接口，使用方法开启设置中的系统代理，看是否可行，仍不可行的话，请自行挂VPN，多的不再说，这个问题不要再和我反馈了，个人能力有限解决不了。\r\n请放弃使用谷歌接口，腾讯，百度接口都可以正常使用。";
			}
			return text2;
		}

		public void OCR_baidutable_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("百度表格");
		}

		public void OCR_ailitable_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("阿里表格");
		}

		private new void Refresh()
		{
			this.sougou.Text = "搜狗";
			this.tencent.Text = "腾讯";
			this.baidu.Text = "百度";
			this.youdao.Text = "有道";
			this.shupai.Text = "竖排";
			this.ocr_table.Text = "表格";
			this.ch_en.Text = "中英";
			this.jap.Text = "日语";
			this.kor.Text = "韩语";
			this.left_right.Text = "从左向右";
			this.righ_left.Text = "从右向左";
			this.baidu_table.Text = "百度";
			this.ali_table.Text = "阿里";
			this.Mathfuntion.Text = "公式";
		}

		public static byte[] ImageToByteArray(Image img)
		{
			return (byte[])new ImageConverter().ConvertTo(img, typeof(byte[]));
		}

		public static Stream BytesToStream(byte[] bytes)
		{
			return new MemoryStream(bytes);
		}

		public void OCR_ali_table()
		{
			string text = "";
			this.split_txt = "";
			try
			{
				string value = IniHelp.GetValue("特殊", "ali_cookie");
				Stream stream = FmMain.BytesToStream(FmMain.ImageToByteArray(this.BWPic((Bitmap)this.image_screen)));
				string text2 = Convert.ToBase64String(new BinaryReader(stream).ReadBytes(Convert.ToInt32(stream.Length)));
				stream.Close();
				string text3 = "{\n\t\"image\": \"" + text2 + "\",\n\t\"configure\": \"{\\\"format\\\":\\\"html\\\", \\\"finance\\\":false}\"\n}";
				string text4 = "https://predict-pai.data.aliyun.com/dp_experience_mall/ocr/ocr_table_parse";
				text = this.Post_Html_final(text4, text3, value);
				this.typeset_txt = ((JObject)JsonConvert.DeserializeObject(this.Post_Html_final(text4, text3, value)))["tables"].ToString().Replace("table tr td { border: 1px solid blue }", "table tr td {border: 0.5px black solid }").Replace("table { border: 1px solid blue }", "table { border: 0.5px black solid; border-collapse : collapse}\r\n");
				this.RichBoxBody.Text = "[消息]：表格已复制到粘贴板！";
			}
			catch
			{
				this.RichBoxBody.Text = "[消息]：阿里表格识别出错！";
				if (text.Contains("NEED_LOGIN"))
				{
					this.split_txt = "弹出cookie";
				}
			}
		}

		public Bitmap BWPic(Bitmap mybm)
		{
			Bitmap bitmap = new Bitmap(mybm.Width, mybm.Height);
			for (int i = 0; i < mybm.Width; i++)
			{
				for (int j = 0; j < mybm.Height; j++)
				{
					Color pixel = mybm.GetPixel(i, j);
					int num = (int)((pixel.R + pixel.G + pixel.B) / 3);
					bitmap.SetPixel(i, j, Color.FromArgb(num, num, num));
				}
			}
			return bitmap;
		}

		public string Post_Html_final(string url, string post_str, string CookieContainer)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Accept = "*/*";
			httpWebRequest.Timeout = 5000;
			httpWebRequest.Headers.Add("Accept-Language:zh-CN,zh;q=0.9");
			httpWebRequest.ContentType = "text/plain";
			httpWebRequest.Headers.Add("Cookie:" + CookieContainer);
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
				text = streamReader.ReadToEnd();
				responseStream.Close();
				streamReader.Close();
				httpWebRequest.Abort();
			}
			catch
			{
			}
			return text;
		}

		public void CopyHtmlToClipBoard(string html)
		{
			Encoding utf = Encoding.UTF8;
			string text = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";
			string text2 = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=" + utf.WebName + "\">\r\n<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n<!--StartFragment-->";
			string text3 = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";
			string text4 = string.Format(text, new object[] { 0, 0, 0, 0 });
			int byteCount = utf.GetByteCount(text4);
			int byteCount2 = utf.GetByteCount(text2);
			int byteCount3 = utf.GetByteCount(html);
			int byteCount4 = utf.GetByteCount(text3);
			string text5 = string.Format(text, new object[]
			{
				byteCount,
				byteCount + byteCount2 + byteCount3 + byteCount4,
				byteCount + byteCount2,
				byteCount + byteCount2 + byteCount3
			}) + text2 + html + text3;
			DataObject dataObject = new DataObject();
			dataObject.SetData(DataFormats.Html, new MemoryStream(utf.GetBytes(text5)));
			string text6 = new FmMain.HtmlToText().Convert(html);
			dataObject.SetData(DataFormats.Text, text6);
			Clipboard.SetDataObject(dataObject);
		}

		public static string Encript(string functionName, object[] pams)
		{
			string text = File.ReadAllText("sign.js");
			ScriptControlClass scriptControlClass = new ScriptControlClass();
			((IScriptControl)scriptControlClass).Language = "javascript";
			((IScriptControl)scriptControlClass).AddCode(text);
			return ((IScriptControl)scriptControlClass).Run(functionName, ref pams).ToString();
		}

		private object ExecuteScript(string sExpression, string sCode)
		{
			ScriptControl scriptControl = new ScriptControlClass();
			scriptControl.UseSafeSubset = true;
			scriptControl.Language = "JScript";
			scriptControl.AddCode(sCode);
			try
			{
				return scriptControl.Eval(sExpression);
			}
			catch (Exception)
			{
			}
			return null;
		}

		private void OCR_Mathfuntion_Click(object sender, EventArgs e)
		{
			this.OCR_foreach("公式");
		}

		public void OCR_Math()
		{
			this.split_txt = "";
			try
			{
				Image image = this.image_screen;
				byte[] array = this.OCR_ImgToByte(image);
				string text = "{\t\"formats\": [\"latex_styled\", \"text\"],\t\"metadata\": {\t\t\"count\": 1,\t\t\"platform\": \"windows 10\",\t\t\"skip_recrop\": true,\t\t\"user_id\": \"123ab2a82ea246a0b011a37183c87bab\",\t\t\"version\": \"snip.windows@00.00.0083\"\t},\t\"ocr\": [\"text\", \"math\"],\t\"src\": \"data:image/jpeg;base64," + Convert.ToBase64String(array) + "\"}";
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.mathpix.com/v3/latex");
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "application/json";
				httpWebRequest.Timeout = 8000;
				httpWebRequest.ReadWriteTimeout = 5000;
				httpWebRequest.Headers.Add("app_id: mathpix_chrome");
				httpWebRequest.Headers.Add("app_key: 85948264c5d443573286752fbe8df361");
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
				Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
				string text2 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
				responseStream.Close();
				string text3 = "$" + ((JObject)JsonConvert.DeserializeObject(text2))["latex_styled"] + "$";
				this.split_txt = text3;
				this.typeset_txt = text3;
			}
			catch
			{
				if (this.esc != "退出")
				{
					this.RichBoxBody.Text = "***该区域未发现文本或者密钥次数用尽***";
				}
				else
				{
					this.RichBoxBody.Text = "***该区域未发现文本***";
					this.esc = "";
				}
			}
		}

		public string interface_flag;

		public string language;

		public string split_txt;

		public string note;

		public string spacechar;

		public string richTextBox1_note;

		public string transtalate_fla;

		public Fmloading fmloading;

		public Thread thread;

		public MenuItem Set;

		public string googleTranslate_txt;

		public int num_ok;

		public bool bolActive;

		public bool tencent_vip_f;

		public string auto_fla;

		public string baidu_vip;

		public string htmltxt;

		public static string TipText;

		public bool speaking;

		public static bool speak_copy;

		public string speak_copyb;

		public string speak_stop;

		public byte[] ttsData;

		public string[] pubnote;

		public Fmnote fmnote;

		public Image image_screen;

		public int voice_count;

		public int form_width;

		public int form_height;

		public bool change_QQ_screenshot;

		private FmFlags fmflags;

		public string trans_hotkey;

		public TimeSpan ts;

		public Timer esc_timer;

		public Thread esc_thread;

		public string esc;

		private string languagle_flag;

		public static string GetTkkJS;

		public string typeset_txt;

		public string baidu_flags;

		public bool 截图排斥;

		private Image image_ori;

		public string shupai_Right_txt;

		private AutoResetEvent are;

		public string baiducookies;

		public string shupai_Left_txt;

		public Image[] image_arr;

		public string OCR_baidu_a;

		public string OCR_baidu_b;

		public List<Image> imgArr;

		public List<Image> imagelist;

		public int imagelist_lenght;

		public string OCR_baidu_d;

		public string OCR_baidu_c;

		public string OCR_baidu_e;

		public int[] image_num;

		public string Proxy_flag;

		public string Proxy_url;

		public string Proxy_port;

		public string Proxy_name;

		public string Proxy_password;

		public bool pinyin_flag;

		public bool set_split;

		public bool set_merge;

		public bool tranclick;

		public string myjsTextBox;

		private string flags_ocrorder;

		public int first_line;

		public bool paragraph;

		public WebBrowser webBrowser;

		public string tencent_cookie;

		private AliTable ailibaba;

		public delegate void translate();

		public delegate void ocr_thread();

		public delegate int Dllinput(string command);

		public class AutoClosedMsgBox
		{
			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

			[DllImport("user32.dll")]
			private static extern bool EndDialog(IntPtr hDlg, int nResult);

			[DllImport("user32.dll")]
			private static extern int MessageBoxTimeout(IntPtr hwnd, string txt, string caption, int wtype, int wlange, int dwtimeout);

			public static int Show(string text, string caption, int milliseconds, FmMain.MsgBoxStyle style)
			{
				return FmMain.AutoClosedMsgBox.MessageBoxTimeout(IntPtr.Zero, text, caption, (int)style, 0, milliseconds);
			}

			public static int Show(string text, string caption, int milliseconds, int style)
			{
				return FmMain.AutoClosedMsgBox.MessageBoxTimeout(IntPtr.Zero, text, caption, style, 0, milliseconds);
			}

			private const int WM_CLOSE = 16;
		}

		public enum MsgBoxStyle
		{
			OK,
			OKCancel,
			AbortRetryIgnore,
			YesNoCancel,
			YesNo,
			RetryCancel,
			CancelRetryContinue,
			RedCritical_OK = 16,
			RedCritical_OKCancel,
			RedCritical_AbortRetryIgnore,
			RedCritical_YesNoCancel,
			RedCritical_YesNo,
			RedCritical_RetryCancel,
			RedCritical_CancelRetryContinue,
			BlueQuestion_OK = 32,
			BlueQuestion_OKCancel,
			BlueQuestion_AbortRetryIgnore,
			BlueQuestion_YesNoCancel,
			BlueQuestion_YesNo,
			BlueQuestion_RetryCancel,
			BlueQuestion_CancelRetryContinue,
			YellowAlert_OK = 48,
			YellowAlert_OKCancel,
			YellowAlert_AbortRetryIgnore,
			YellowAlert_YesNoCancel,
			YellowAlert_YesNo,
			YellowAlert_RetryCancel,
			YellowAlert_CancelRetryContinue,
			BlueInfo_OK = 64,
			BlueInfo_OKCancel,
			BlueInfo_AbortRetryIgnore,
			BlueInfo_YesNoCancel,
			BlueInfo_YesNo,
			BlueInfo_RetryCancel,
			BlueInfo_CancelRetryContinue
		}

		private class PinYin
		{
			static PinYin()
			{
				string text = "吖,ā|阿,ā|啊,ā|锕,ā|錒,ā|嗄,á|厑,ae|哎,āi|哀,āi|唉,āi|埃,āi|挨,āi|溾,āi|锿,āi|鎄,āi|啀,ái|捱,ái|皑,ái|凒,ái|嵦,ái|溰,ái|嘊,ái|敱,ái|敳,ái|皚,ái|癌,ái|娾,ái|隑,ái|剴,ái|騃,ái|毐,ǎi|昹,ǎi|矮,ǎi|蔼,ǎi|躷,ǎi|濭,ǎi|藹,ǎi|譪,ǎi|霭,ǎi|靄,ǎi|鯦,ǎi|噯,ài|艾,ài|伌,ài|爱,ài|砹,ài|硋,ài|隘,ài|嗌,ài|塧,ài|嫒,ài|愛,ài|碍,ài|叆,ài|暧,ài|瑷,ài|僾,ài|壒,ài|嬡,ài|懓,ài|薆,ài|懝,ài|曖,ài|賹,ài|餲,ài|鴱,ài|皧,ài|瞹,ài|馤,ài|礙,ài|譺,ài|鑀,ài|鱫,ài|靉,ài|閡,ài|欬,ài|焥,ài|堨,ài|乂,ài|嗳,ài|璦,ài|安,ān|侒,ān|峖,ān|桉,ān|氨,ān|庵,ān|谙,ān|媕,ān|萻,ān|葊,ān|痷,ān|腤,ān|鹌,ān|蓭,ān|誝,ān|鞌,ān|鞍,ān|盦,ān|闇,ān|馣,ān|鮟,ān|盫,ān|鵪,ān|韽,ān|鶕,ān|啽,ān|厰,ān|鴳,ān|諳,ān|玵,án|雸,án|儑,án|垵,ǎn|俺,ǎn|唵,ǎn|埯,ǎn|铵,ǎn|隌,ǎn|揞,ǎn|晻,ǎn|罯,ǎn|銨,ǎn|碪,ǎn|犴,àn|岸,àn|按,àn|洝,àn|荌,àn|案,àn|胺,àn|豻,àn|堓,àn|婩,àn|貋,àn|錌,àn|黯,àn|頇,àn|屽,àn|垾,àn|遃,àn|暗,àn|肮,āng|骯,āng|岇,áng|昂,áng|昻,áng|卬,áng|枊,àng|盎,àng|醠,àng|凹,āo|垇,āo|柪,āo|軪,āo|爊,āo|熝,āo|眑,āo|泑,āo|梎,āo|敖,áo|厫,áo|隞,áo|嗷,áo|嗸,áo|嶅,áo|廒,áo|滶,áo|獒,áo|獓,áo|遨,áo|摮,áo|璈,áo|蔜,áo|磝,áo|翱,áo|聱,áo|螯,áo|翶,áo|謷,áo|翺,áo|鳌,áo|鏖,áo|鰲,áo|鷔,áo|鼇,áo|慠,áo|鏕,áo|嚻,áo|熬,áo|抝,ǎo|芺,ǎo|袄,ǎo|媪,ǎo|镺,ǎo|媼,ǎo|襖,ǎo|郩,ǎo|鴁,ǎo|蝹,ǎo|坳,ào|岙,ào|扷,ào|岰,ào|傲,ào|奡,ào|奥,ào|嫯,ào|奧,ào|澚,ào|墺,ào|嶴,ào|澳,ào|懊,ào|擙,ào|謸,ào|鏊,ào|驁,ào|骜,ào|吧,ba|八,bā|仈,bā|巴,bā|叭,bā|扒,bā|朳,bā|玐,bā|夿,bā|岜,bā|芭,bā|疤,bā|哵,bā|捌,bā|笆,bā|粑,bā|紦,bā|羓,bā|蚆,bā|釟,bā|鲃,bā|魞,bā|鈀,bā|柭,bā|丷,bā|峇,bā|豝,bā|叐,bá|犮,bá|抜,bá|坺,bá|妭,bá|拔,bá|茇,bá|炦,bá|癹,bá|胈,bá|釛,bá|菝,bá|詙,bá|跋,bá|軷,bá|颰,bá|魃,bá|墢,bá|鼥,bá|把,bǎ|钯,bǎ|靶,bǎ|坝,bà|弝,bà|爸,bà|罢,bà|鲅,bà|罷,bà|鮁,bà|覇,bà|矲,bà|霸,bà|壩,bà|灞,bà|欛,bà|鲌,bà|鮊,bà|皅,bà|挀,bāi|掰,bāi|白,bái|百,bǎi|佰,bǎi|柏,bǎi|栢,bǎi|捭,bǎi|竡,bǎi|粨,bǎi|絔,bǎi|摆,bǎi|擺,bǎi|襬,bǎi|庍,bài|拝,bài|败,bài|拜,bài|敗,bài|稗,bài|粺,bài|鞁,bài|薭,bài|贁,bài|韛,bài|扳,bān|攽,bān|朌,bān|班,bān|般,bān|颁,bān|斑,bān|搬,bān|斒,bān|頒,bān|瘢,bān|螁,bān|螌,bān|褩,bān|癍,bān|辬,bān|籓,bān|肦,bān|鳻,bān|搫,bān|阪,bǎn|坂,bǎn|岅,bǎn|昄,bǎn|板,bǎn|版,bǎn|钣,bǎn|粄,bǎn|舨,bǎn|鈑,bǎn|蝂,bǎn|魬,bǎn|覂,bǎn|瓪,bǎn|办,bàn|半,bàn|伴,bàn|扮,bàn|姅,bàn|怑,bàn|拌,bàn|绊,bàn|秚,bàn|湴,bàn|絆,bàn|鉡,bàn|靽,bàn|辦,bàn|瓣,bàn|跘,bàn|邦,bāng|峀,bāng|垹,bāng|帮,bāng|捠,bāng|梆,bāng|浜,bāng|邫,bāng|幚,bāng|縍,bāng|幫,bāng|鞤,bāng|幇,bāng|绑,bǎng|綁,bǎng|榜,bǎng|牓,bǎng|膀,bǎng|騯,bǎng|玤,bàng|蚌,bàng|傍,bàng|棒,bàng|棓,bàng|硥,bàng|谤,bàng|塝,bàng|徬,bàng|稖,bàng|蒡,bàng|蜯,bàng|镑,bàng|艕,bàng|謗,bàng|鎊,bàng|埲,bàng|蚄,bàng|蛖,bàng|嫎,bàng|勹,bāo|包,bāo|佨,bāo|孢,bāo|胞,bāo|剝,bāo|笣,bāo|煲,bāo|龅,bāo|蕔,bāo|褒,bāo|闁,bāo|襃,bāo|齙,bāo|剥,bāo|枹,bāo|裦,bāo|苞,bāo|窇,báo|嫑,báo|雹,báo|铇,báo|薄,báo|宝,bǎo|怉,bǎo|饱,bǎo|保,bǎo|鸨,bǎo|珤,bǎo|堡,bǎo|堢,bǎo|媬,bǎo|葆,bǎo|寚,bǎo|飹,bǎo|飽,bǎo|褓,bǎo|駂,bǎo|鳵,bǎo|緥,bǎo|賲,bǎo|藵,bǎo|寳,bǎo|寶,bǎo|靌,bǎo|宀,bǎo|鴇,bǎo|勽,bào|报,bào|抱,bào|豹,bào|菢,bào|袌,bào|報,bào|鉋,bào|鲍,bào|靤,bào|骲,bào|暴,bào|髱,bào|虣,bào|鮑,bào|儤,bào|曓,bào|爆,bào|忁,bào|鑤,bào|蚫,bào|瀑,bào|萡,be|呗,bei|唄,bei|陂,bēi|卑,bēi|盃,bēi|桮,bēi|悲,bēi|揹,bēi|碑,bēi|鹎,bēi|藣,bēi|鵯,bēi|柸,bēi|錍,bēi|椑,bēi|諀,bēi|杯,bēi|喺,béi|北,běi|鉳,běi|垻,bèi|贝,bèi|狈,bèi|貝,bèi|邶,bèi|备,bèi|昁,bèi|牬,bèi|苝,bèi|背,bèi|钡,bèi|俻,bèi|倍,bèi|悖,bèi|狽,bèi|被,bèi|偝,bèi|偹,bèi|梖,bèi|珼,bèi|備,bèi|僃,bèi|惫,bèi|焙,bèi|琲,bèi|軰,bèi|辈,bèi|愂,bèi|碚,bèi|禙,bèi|蓓,bèi|蛽,bèi|犕,bèi|褙,bèi|誖,bèi|骳,bèi|輩,bèi|鋇,bèi|憊,bèi|糒,bèi|鞴,bèi|鐾,bèi|鐴,bèi|杮,bèi|韝,bèi|棑,bèi|哱,bèi|鄁,bèi|奔,bēn|泍,bēn|贲,bēn|倴,bēn|渀,bēn|逩,bēn|犇,bēn|賁,bēn|錛,bēn|喯,bēn|锛,bēn|本,běn|苯,běn|奙,běn|畚,běn|楍,běn|翉,běn|夲,běn|坌,bèn|捹,bèn|桳,bèn|笨,bèn|撪,bèn|獖,bèn|輽,bèn|炃,bèn|燌,bèn|夯,bèn|伻,bēng|祊,bēng|奟,bēng|崩,bēng|绷,bēng|絣,bēng|閍,bēng|嵭,bēng|痭,bēng|嘣,bēng|綳,bēng|繃,bēng|嗙,bēng|挷,bēng|傰,bēng|搒,bēng|甭,béng|埄,běng|菶,běng|琣,běng|鞛,běng|琫,běng|泵,bèng|迸,bèng|逬,bèng|跰,bèng|塴,bèng|甏,bèng|镚,bèng|蹦,bèng|鏰,bèng|錋,bèng|皀,bī|屄,bī|偪,bī|毴,bī|逼,bī|豍,bī|螕,bī|鲾,bī|鎞,bī|鵖,bī|鰏,bī|悂,bī|鈚,bī|柲,bí|荸,bí|鼻,bí|嬶,bí|匕,bǐ|比,bǐ|夶,bǐ|朼,bǐ|佊,bǐ|妣,bǐ|沘,bǐ|疕,bǐ|彼,bǐ|柀,bǐ|秕,bǐ|俾,bǐ|笔,bǐ|粃,bǐ|粊,bǐ|舭,bǐ|啚,bǐ|筆,bǐ|鄙,bǐ|聛,bǐ|貏,bǐ|箄,bǐ|崥,bǐ|魮,bǐ|娝,bǐ|箃,bǐ|吡,bǐ|匂,bì|币,bì|必,bì|毕,bì|闭,bì|佖,bì|坒,bì|庇,bì|诐,bì|邲,bì|妼,bì|怭,bì|枈,bì|畀,bì|苾,bì|哔,bì|毖,bì|珌,bì|疪,bì|胇,bì|荜,bì|陛,bì|毙,bì|狴,bì|畢,bì|袐,bì|铋,bì|婢,bì|庳,bì|敝,bì|梐,bì|萆,bì|萞,bì|閇,bì|閉,bì|堛,bì|弻,bì|弼,bì|愊,bì|愎,bì|湢,bì|皕,bì|禆,bì|筚,bì|貱,bì|赑,bì|嗶,bì|彃,bì|楅,bì|滗,bì|滭,bì|煏,bì|痹,bì|痺,bì|腷,bì|蓖,bì|蓽,bì|蜌,bì|裨,bì|跸,bì|鉍,bì|閟,bì|飶,bì|幣,bì|弊,bì|熚,bì|獙,bì|碧,bì|稫,bì|箅,bì|箆,bì|綼,bì|蔽,bì|馝,bì|幤,bì|潷,bì|獘,bì|罼,bì|襅,bì|駜,bì|髲,bì|壁,bì|嬖,bì|廦,bì|篦,bì|篳,bì|縪,bì|薜,bì|觱,bì|避,bì|鮅,bì|斃,bì|濞,bì|臂,bì|蹕,bì|鞞,bì|髀,bì|奰,bì|璧,bì|鄨,bì|饆,bì|繴,bì|襞,bì|鏎,bì|鞸,bì|韠,bì|躃,bì|躄,bì|魓,bì|贔,bì|驆,bì|鷝,bì|鷩,bì|鼊,bì|咇,bì|鮩,bì|畐,bì|踾,bì|鶝,bì|闬,bì|閈,bì|祕,bì|鴓,bì|怶,bì|旇,bì|翍,bì|肶,bì|笓,bì|鸊,bì|肸,bì|畁,bì|詖,bì|鄪,bì|襣,bì|边,biān|砭,biān|笾,biān|猵,biān|编,biān|萹,biān|煸,biān|牑,biān|甂,biān|箯,biān|編,biān|蝙,biān|獱,biān|邉,biān|鍽,biān|鳊,biān|邊,biān|鞭,biān|鯿,biān|籩,biān|糄,biān|揙,biān|臱,biān|鯾,biān|炞,biǎn|贬,biǎn|扁,biǎn|窆,biǎn|匾,biǎn|貶,biǎn|惼,biǎn|碥,biǎn|稨,biǎn|褊,biǎn|鴘,biǎn|藊,biǎn|釆,biǎn|辧,biǎn|疺,biǎn|覵,biǎn|鶣,biǎn|卞,biàn|弁,biàn|忭,biàn|抃,biàn|汳,biàn|汴,biàn|苄,biàn|峅,biàn|便,biàn|变,biàn|変,biàn|昪,biàn|覍,biàn|缏,biàn|遍,biàn|閞,biàn|辡,biàn|緶,biàn|艑,biàn|辨,biàn|辩,biàn|辫,biàn|辮,biàn|辯,biàn|變,biàn|彪,biāo|标,biāo|飑,biāo|骉,biāo|髟,biāo|淲,biāo|猋,biāo|脿,biāo|墂,biāo|幖,biāo|滮,biāo|蔈,biāo|骠,biāo|標,biāo|熛,biāo|膘,biāo|麃,biāo|瘭,biāo|镖,biāo|飙,biāo|飚,biāo|儦,biāo|颷,biāo|瀌,biāo|藨,biāo|謤,biāo|爂,biāo|臕,biāo|贆,biāo|鏢,biāo|穮,biāo|镳,biāo|飆,biāo|飇,biāo|飈,biāo|飊,biāo|驃,biāo|鑣,biāo|驫,biāo|摽,biāo|膔,biāo|篻,biāo|僄,biāo|徱,biāo|表,biǎo|婊,biǎo|裱,biǎo|褾,biǎo|錶,biǎo|檦,biǎo|諘,biǎo|俵,biào|鳔,biào|鰾,biào|憋,biē|鳖,biē|鱉,biē|鼈,biē|虌,biē|龞,biē|蟞,biē|別,bié|别,bié|莂,bié|蛂,bié|徶,bié|襒,bié|蹩,bié|穪,bié|瘪,biě|癟,biě|彆,biè|汃,bīn|邠,bīn|砏,bīn|宾,bīn|彬,bīn|斌,bīn|椕,bīn|滨,bīn|缤,bīn|槟,bīn|瑸,bīn|豩,bīn|賓,bīn|賔,bīn|镔,bīn|儐,bīn|濒,bīn|濱,bīn|濵,bīn|虨,bīn|豳,bīn|璸,bīn|瀕,bīn|霦,bīn|繽,bīn|蠙,bīn|鑌,bīn|顮,bīn|檳,bīn|玢,bīn|訜,bīn|傧,bīn|氞,bìn|摈,bìn|殡,bìn|膑,bìn|髩,bìn|擯,bìn|鬂,bìn|臏,bìn|髌,bìn|鬓,bìn|髕,bìn|鬢,bìn|殯,bìn|仌,bīng|氷,bīng|冰,bīng|兵,bīng|栟,bīng|掤,bīng|梹,bīng|鋲,bīng|幷,bīng|丙,bǐng|邴,bǐng|陃,bǐng|怲,bǐng|抦,bǐng|秉,bǐng|苪,bǐng|昞,bǐng|昺,bǐng|柄,bǐng|炳,bǐng|饼,bǐng|眪,bǐng|窉,bǐng|蛃,bǐng|禀,bǐng|鈵,bǐng|鉼,bǐng|鞆,bǐng|餅,bǐng|餠,bǐng|燷,bǐng|庰,bǐng|偋,bǐng|寎,bǐng|綆,bǐng|稟,bǐng|癛,bǐng|癝,bǐng|琕,bǐng|棅,bǐng|并,bìng|並,bìng|併,bìng|垪,bìng|倂,bìng|栤,bìng|病,bìng|竝,bìng|傡,bìng|摒,bìng|誁,bìng|靐,bìng|疒,bìng|啵,bo|蔔,bo|卜,bo|噃,bo|趵,bō|癶,bō|拨,bō|波,bō|玻,bō|袚,bō|袯,bō|钵,bō|饽,bō|紴,bō|缽,bō|菠,bō|碆,bō|鉢,bō|僠,bō|嶓,bō|撥,bō|播,bō|餑,bō|磻,bō|蹳,bō|驋,bō|鱍,bō|帗,bō|盋,bō|脖,bó|仢,bó|伯,bó|孛,bó|犻,bó|驳,bó|帛,bó|泊,bó|狛,bó|苩,bó|侼,bó|勃,bó|胉,bó|郣,bó|亳,bó|挬,bó|浡,bó|瓟,bó|秡,bó|钹,bó|铂,bó|桲,bó|淿,bó|舶,bó|博,bó|渤,bó|湐,bó|葧,bó|鹁,bó|愽,bó|搏,bó|猼,bó|鈸,bó|鉑,bó|馎,bó|僰,bó|煿,bó|箔,bó|膊,bó|艊,bó|馛,bó|駁,bó|踣,bó|鋍,bó|镈,bó|壆,bó|馞,bó|駮,bó|豰,bó|嚗,bó|懪,bó|礡,bó|簙,bó|鎛,bó|餺,bó|鵓,bó|犦,bó|髆,bó|髉,bó|欂,bó|襮,bó|礴,bó|鑮,bó|肑,bó|茀,bó|袹,bó|穛,bó|彴,bó|瓝,bó|牔,bó|蚾,bǒ|箥,bǒ|跛,bǒ|簸,bò|孹,bò|擘,bò|檗,bò|糪,bò|譒,bò|蘗,bò|襎,bò|檘,bò|蔢,bò|峬,bū|庯,bū|逋,bū|钸,bū|晡,bū|鈽,bū|誧,bū|餔,bū|鵏,bū|秿,bū|陠,bū|鯆,bū|轐,bú|醭,bú|不,bú|輹,bú|卟,bǔ|补,bǔ|哺,bǔ|捕,bǔ|補,bǔ|鳪,bǔ|獛,bǔ|鸔,bǔ|擈,bǔ|佈,bù|吥,bù|步,bù|咘,bù|怖,bù|歨,bù|歩,bù|钚,bù|勏,bù|埗,bù|悑,bù|捗,bù|荹,bù|部,bù|埠,bù|瓿,bù|鈈,bù|廍,bù|蔀,bù|踄,bù|郶,bù|篰,bù|餢,bù|簿,bù|尃,bù|箁,bù|抪,bù|柨,bù|布,bù|擦,cā|攃,cā|礤,cǎ|礸,cǎ|遪,cà|偲,cāi|猜,cāi|揌,cāi|才,cái|材,cái|财,cái|財,cái|戝,cái|裁,cái|采,cǎi|倸,cǎi|埰,cǎi|婇,cǎi|寀,cǎi|彩,cǎi|採,cǎi|睬,cǎi|跴,cǎi|綵,cǎi|踩,cǎi|菜,cài|棌,cài|蔡,cài|縩,cài|乲,cal|参,cān|參,cān|飡,cān|骖,cān|喰,cān|湌,cān|傪,cān|嬠,cān|餐,cān|驂,cān|嵾,cān|飱,cān|残,cán|蚕,cán|惭,cán|殘,cán|慚,cán|蝅,cán|慙,cán|蠶,cán|蠺,cán|惨,cǎn|慘,cǎn|噆,cǎn|憯,cǎn|黪,cǎn|黲,cǎn|灿,càn|粲,càn|儏,càn|澯,càn|薒,càn|燦,càn|璨,càn|爘,càn|謲,càn|仓,cāng|沧,cāng|苍,cāng|倉,cāng|舱,cāng|凔,cāng|嵢,cāng|滄,cāng|獊,cāng|蒼,cāng|濸,cāng|艙,cāng|螥,cāng|罉,cāng|藏,cáng|欌,cáng|鑶,cáng|賶,càng|撡,cāo|操,cāo|糙,cāo|曺,cáo|嘈,cáo|嶆,cáo|漕,cáo|蓸,cáo|槽,cáo|褿,cáo|艚,cáo|螬,cáo|鏪,cáo|慒,cáo|曹,cáo|艹,cǎo|艸,cǎo|草,cǎo|愺,cǎo|懆,cǎo|騲,cǎo|慅,cǎo|肏,cào|鄵,cào|襙,cào|冊,cè|册,cè|侧,cè|厕,cè|恻,cè|拺,cè|测,cè|荝,cè|敇,cè|側,cè|粣,cè|萗,cè|廁,cè|惻,cè|測,cè|策,cè|萴,cè|筞,cè|蓛,cè|墄,cè|箣,cè|憡,cè|刂,cè|厠,cè|膥,cēn|岑,cén|梣,cén|涔,cén|硶,cén|噌,cēng|层,céng|層,céng|竲,céng|驓,céng|曾,céng|蹭,cèng|硛,ceok|硳,ceok|岾,ceom|猠,ceon|乽,ceor|嚓,chā|叉,chā|扠,chā|芆,chā|杈,chā|肞,chā|臿,chā|訍,chā|偛,chā|嗏,chā|插,chā|銟,chā|锸,chā|艖,chā|疀,chā|鍤,chā|鎈,chā|垞,chá|查,chá|査,chá|茬,chá|茶,chá|嵖,chá|猹,chá|靫,chá|槎,chá|察,chá|碴,chá|褨,chá|檫,chá|搽,chá|衩,chǎ|镲,chǎ|鑔,chǎ|奼,chà|汊,chà|岔,chà|侘,chà|诧,chà|剎,chà|姹,chà|差,chà|紁,chà|詫,chà|拆,chāi|钗,chāi|釵,chāi|犲,chái|侪,chái|柴,chái|祡,chái|豺,chái|儕,chái|喍,chái|虿,chài|袃,chài|瘥,chài|蠆,chài|囆,chài|辿,chān|觇,chān|梴,chān|掺,chān|搀,chān|覘,chān|裧,chān|摻,chān|鋓,chān|幨,chān|襜,chān|攙,chān|嚵,chān|脠,chān|婵,chán|谗,chán|孱,chán|棎,chán|湹,chán|禅,chán|馋,chán|嬋,chán|煘,chán|缠,chán|獑,chán|蝉,chán|誗,chán|鋋,chán|儃,chán|廛,chán|潹,chán|潺,chán|緾,chán|磛,chán|禪,chán|毚,chán|鄽,chán|瀍,chán|蟬,chán|儳,chán|劖,chán|蟾,chán|酁,chán|壥,chán|巉,chán|瀺,chán|纏,chán|纒,chán|躔,chán|艬,chán|讒,chán|鑱,chán|饞,chán|繟,chán|澶,chán|镵,chán|产,chǎn|刬,chǎn|旵,chǎn|丳,chǎn|浐,chǎn|剗,chǎn|谄,chǎn|產,chǎn|産,chǎn|铲,chǎn|阐,chǎn|蒇,chǎn|剷,chǎn|嵼,chǎn|摌,chǎn|滻,chǎn|幝,chǎn|蕆,chǎn|諂,chǎn|閳,chǎn|燀,chǎn|簅,chǎn|冁,chǎn|醦,chǎn|闡,chǎn|囅,chǎn|灛,chǎn|讇,chǎn|墠,chǎn|骣,chǎn|鏟,chǎn|忏,chàn|硟,chàn|摲,chàn|懴,chàn|颤,chàn|懺,chàn|羼,chàn|韂,chàn|顫,chàn|伥,chāng|昌,chāng|倀,chāng|娼,chāng|淐,chāng|猖,chāng|菖,chāng|阊,chāng|晿,chāng|椙,chāng|琩,chāng|裮,chāng|锠,chāng|錩,chāng|閶,chāng|鲳,chāng|鯧,chāng|鼚,chāng|兏,cháng|肠,cháng|苌,cháng|尝,cháng|偿,cháng|常,cháng|徜,cháng|瓺,cháng|萇,cháng|甞,cháng|腸,cháng|嘗,cháng|嫦,cháng|瑺,cháng|膓,cháng|鋿,cháng|償,cháng|嚐,cháng|蟐,cháng|鲿,cháng|鏛,cháng|鱨,cháng|棖,cháng|尙,cháng|厂,chǎng|场,chǎng|昶,chǎng|場,chǎng|敞,chǎng|僘,chǎng|廠,chǎng|氅,chǎng|鋹,chǎng|惝,chǎng|怅,chàng|玚,chàng|畅,chàng|倡,chàng|鬯,chàng|唱,chàng|悵,chàng|暢,chàng|畼,chàng|誯,chàng|韔,chàng|抄,chāo|弨,chāo|怊,chāo|欩,chāo|钞,chāo|焯,chāo|超,chāo|鈔,chāo|繛,chāo|樔,chāo|绰,chāo|綽,chāo|綤,chāo|牊,cháo|巢,cháo|巣,cháo|朝,cháo|鄛,cháo|漅,cháo|嘲,cháo|潮,cháo|窲,cháo|罺,cháo|轈,cháo|晁,cháo|吵,chǎo|炒,chǎo|眧,chǎo|煼,chǎo|麨,chǎo|巐,chǎo|粆,chǎo|仦,chào|耖,chào|觘,chào|趠,chào|车,chē|車,chē|砗,chē|唓,chē|硨,chē|蛼,chē|莗,chē|扯,chě|偖,chě|撦,chě|彻,chè|坼,chè|迠,chè|烢,chè|聅,chè|掣,chè|硩,chè|頙,chè|徹,chè|撤,chè|澈,chè|勶,chè|瞮,chè|爡,chè|喢,chè|賝,chen|伧,chen|傖,chen|抻,chēn|郴,chēn|棽,chēn|琛,chēn|嗔,chēn|綝,chēn|諃,chēn|尘,chén|臣,chén|忱,chén|沉,chén|辰,chén|陈,chén|茞,chén|宸,chén|烥,chén|莐,chén|陳,chén|敐,chén|晨,chén|訦,chén|谌,chén|揨,chén|煁,chén|蔯,chén|塵,chén|樄,chén|瘎,chén|霃,chén|螴,chén|諶,chén|麎,chén|曟,chén|鷐,chén|薼,chén|趻,chěn|碜,chěn|墋,chěn|夦,chěn|磣,chěn|踸,chěn|贂,chěn|衬,chèn|疢,chèn|龀,chèn|趁,chèn|榇,chèn|齓,chèn|齔,chèn|嚫,chèn|谶,chèn|襯,chèn|讖,chèn|瀋,chèn|称,chēng|稱,chēng|阷,chēng|泟,chēng|柽,chēng|爯,chēng|棦,chēng|浾,chēng|偁,chēng|蛏,chēng|铛,chēng|牚,chēng|琤,chēng|赪,chēng|憆,chēng|摚,chēng|靗,chēng|撐,chēng|撑,chēng|緽,chēng|橕,chēng|瞠,chēng|赬,chēng|頳,chēng|檉,chēng|竀,chēng|蟶,chēng|鏳,chēng|鏿,chēng|饓,chēng|鐺,chēng|丞,chéng|成,chéng|呈,chéng|承,chéng|枨,chéng|诚,chéng|郕,chéng|乗,chéng|城,chéng|娍,chéng|宬,chéng|峸,chéng|洆,chéng|荿,chéng|乘,chéng|埕,chéng|挰,chéng|珹,chéng|掁,chéng|窚,chéng|脭,chéng|铖,chéng|堘,chéng|惩,chéng|椉,chéng|程,chéng|筬,chéng|絾,chéng|裎,chéng|塖,chéng|溗,chéng|碀,chéng|誠,chéng|畻,chéng|酲,chéng|鋮,chéng|澄,chéng|橙,chéng|檙,chéng|鯎,chéng|瀓,chéng|懲,chéng|騬,chéng|塍,chéng|悜,chěng|逞,chěng|骋,chěng|庱,chěng|睈,chěng|騁,chěng|秤,chèng|吃,chī|妛,chī|杘,chī|侙,chī|哧,chī|蚩,chī|鸱,chī|瓻,chī|眵,chī|笞,chī|訵,chī|嗤,chī|媸,chī|摛,chī|痴,chī|瞝,chī|螭,chī|鴟,chī|鵄,chī|癡,chī|魑,chī|齝,chī|攡,chī|麶,chī|彲,chī|黐,chī|蚳,chī|摴,chī|彨,chī|弛,chí|池,chí|驰,chí|迟,chí|岻,chí|茌,chí|持,chí|竾,chí|淔,chí|筂,chí|貾,chí|遅,chí|馳,chí|墀,chí|踟,chí|遲,chí|篪,chí|謘,chí|尺,chǐ|叺,chǐ|呎,chǐ|肔,chǐ|卶,chǐ|齿,chǐ|垑,chǐ|胣,chǐ|恥,chǐ|耻,chǐ|蚇,chǐ|豉,chǐ|欼,chǐ|歯,chǐ|裭,chǐ|鉹,chǐ|褫,chǐ|齒,chǐ|侈,chǐ|彳,chì|叱,chì|斥,chì|灻,chì|赤,chì|饬,chì|抶,chì|勅,chì|恜,chì|炽,chì|翄,chì|翅,chì|烾,chì|痓,chì|啻,chì|湁,chì|飭,chì|傺,chì|痸,chì|腟,chì|鉓,chì|雴,chì|憏,chì|翤,chì|遫,chì|慗,chì|瘛,chì|翨,chì|熾,chì|懘,chì|趩,chì|饎,chì|鶒,chì|鷘,chì|餝,chì|歗,chì|敕,chì|充,chōng|冲,chōng|忡,chōng|茺,chōng|珫,chōng|翀,chōng|舂,chōng|嘃,chōng|摏,chōng|憃,chōng|憧,chōng|衝,chōng|罿,chōng|艟,chōng|蹖,chōng|褈,chōng|傭,chōng|浺,chōng|虫,chóng|崇,chóng|崈,chóng|隀,chóng|蟲,chóng|宠,chǒng|埫,chǒng|寵,chǒng|沖,chòng|铳,chòng|銃,chòng|抽,chōu|紬,chōu|瘳,chōu|篘,chōu|犨,chōu|犫,chōu|跾,chōu|掫,chōu|仇,chóu|俦,chóu|栦,chóu|惆,chóu|绸,chóu|菗,chóu|畴,chóu|絒,chóu|愁,chóu|皗,chóu|稠,chóu|筹,chóu|酧,chóu|酬,chóu|綢,chóu|踌,chóu|儔,chóu|雔,chóu|嬦,chóu|懤,chóu|雠,chóu|疇,chóu|籌,chóu|躊,chóu|讎,chóu|讐,chóu|擣,chóu|燽,chóu|丑,chǒu|丒,chǒu|吜,chǒu|杽,chǒu|侴,chǒu|瞅,chǒu|醜,chǒu|矁,chǒu|魗,chǒu|臭,chòu|遚,chòu|殠,chòu|榋,chu|橻,chu|屮,chū|出,chū|岀,chū|初,chū|樗,chū|貙,chū|齣,chū|刍,chú|除,chú|厨,chú|滁,chú|蒢,chú|豠,chú|锄,chú|耡,chú|蒭,chú|蜍,chú|趎,chú|鉏,chú|雏,chú|犓,chú|廚,chú|篨,chú|鋤,chú|橱,chú|懨,chú|幮,chú|櫉,chú|蟵,chú|躇,chú|雛,chú|櫥,chú|蹰,chú|鶵,chú|躕,chú|媰,chú|杵,chǔ|础,chǔ|储,chǔ|楮,chǔ|禇,chǔ|楚,chǔ|褚,chǔ|濋,chǔ|儲,chǔ|檚,chǔ|璴,chǔ|礎,chǔ|齭,chǔ|齼,chǔ|処,chǔ|椘,chǔ|亍,chù|处,chù|竌,chù|怵,chù|拀,chù|绌,chù|豖,chù|竐,chù|俶,chù|敊,chù|珿,chù|絀,chù|處,chù|傗,chù|琡,chù|搐,chù|触,chù|踀,chù|閦,chù|儊,chù|憷,chù|斶,chù|歜,chù|臅,chù|黜,chù|觸,chù|矗,chù|觕,chù|畜,chù|鄐,chù|搋,chuāi|揣,chuāi|膗,chuái|嘬,chuài|踹,chuài|膪,chuài|巛,chuān|川,chuān|氚,chuān|穿,chuān|剶,chuān|瑏,chuān|传,chuán|舡,chuán|船,chuán|猭,chuán|遄,chuán|傳,chuán|椽,chuán|歂,chuán|暷,chuán|輲,chuán|甎,chuán|舛,chuǎn|荈,chuǎn|喘,chuǎn|僢,chuǎn|堾,chuǎn|踳,chuǎn|汌,chuàn|串,chuàn|玔,chuàn|钏,chuàn|釧,chuàn|賗,chuàn|刅,chuāng|炊,chuī|龡,chuī|圌,chuí|垂,chuí|桘,chuí|陲,chuí|捶,chuí|菙,chuí|棰,chuí|槌,chuí|锤,chuí|箠,chuí|顀,chuí|錘,chuí|鰆,chun|旾,chūn|杶,chūn|春,chūn|萅,chūn|媋,chūn|暙,chūn|椿,chūn|槆,chūn|瑃,chūn|箺,chūn|蝽,chūn|橁,chūn|輴,chūn|櫄,chūn|鶞,chūn|纯,chún|陙,chún|唇,chún|浱,chún|純,chún|莼,chún|淳,chún|脣,chún|犉,chún|滣,chún|鹑,chún|漘,chún|醇,chún|醕,chún|鯙,chún|鶉,chún|蒓,chún|偆,chǔn|萶,chǔn|惷,chǔn|睶,chǔn|賰,chǔn|蠢,chǔn|踔,chuō|戳,chuō|啜,chuò|辵,chuò|娕,chuò|娖,chuò|惙,chuò|涰,chuò|逴,chuò|辍,chuò|酫,chuò|龊,chuò|擉,chuò|磭,chuò|歠,chuò|嚽,chuò|齪,chuò|鑡,chuò|齱,chuò|婼,chuò|鋜,chuò|輟,chuò|呲,cī|玼,cī|疵,cī|趀,cī|偨,cī|縒,cī|跐,cī|髊,cī|齹,cī|枱,cī|词,cí|珁,cí|垐,cí|柌,cí|祠,cí|茨,cí|瓷,cí|詞,cí|辝,cí|慈,cí|甆,cí|辞,cí|鈶,cí|雌,cí|鹚,cí|糍,cí|辤,cí|飺,cí|餈,cí|嬨,cí|濨,cí|鴜,cí|礠,cí|辭,cí|鶿,cí|鷀,cí|磁,cí|此,cǐ|佌,cǐ|皉,cǐ|朿,cì|次,cì|佽,cì|刺,cì|刾,cì|庛,cì|茦,cì|栨,cì|莿,cì|絘,cì|赐,cì|螆,cì|賜,cì|蛓,cì|嗭,cis|囱,cōng|匆,cōng|囪,cōng|苁,cōng|忩,cōng|枞,cōng|茐,cōng|怱,cōng|悤,cōng|棇,cōng|焧,cōng|葱,cōng|楤,cōng|漗,cōng|聡,cōng|蔥,cōng|骢,cōng|暰,cōng|樅,cōng|樬,cōng|瑽,cōng|璁,cōng|聪,cōng|瞛,cōng|篵,cōng|聰,cōng|蟌,cōng|繱,cōng|鏦,cōng|騘,cōng|驄,cōng|聦,cōng|从,cóng|從,cóng|丛,cóng|従,cóng|婃,cóng|孮,cóng|徖,cóng|悰,cóng|淙,cóng|琮,cóng|漎,cóng|誴,cóng|賨,cóng|賩,cóng|樷,cóng|藂,cóng|叢,cóng|灇,cóng|欉,cóng|爜,cóng|憁,còng|謥,còng|凑,còu|湊,còu|楱,còu|腠,còu|辏,còu|輳,còu|粗,cū|麁,cū|麄,cū|麤,cū|徂,cú|殂,cú|蔖,cǔ|促,cù|猝,cù|媨,cù|瘄,cù|蔟,cù|誎,cù|趗,cù|憱,cù|醋,cù|瘯,cù|簇,cù|縬,cù|鼀,cù|蹴,cù|蹵,cù|顣,cù|蹙,cù|汆,cuān|撺,cuān|镩,cuān|蹿,cuān|攛,cuān|躥,cuān|鑹,cuān|攅,cuán|櫕,cuán|巑,cuán|攢,cuán|窜,cuàn|熶,cuàn|篡,cuàn|殩,cuàn|篹,cuàn|簒,cuàn|竄,cuàn|爨,cuàn|乼,cui|崔,cuī|催,cuī|凗,cuī|墔,cuī|摧,cuī|榱,cuī|獕,cuī|磪,cuī|鏙,cuī|漼,cuī|慛,cuī|璀,cuǐ|皠,cuǐ|熣,cuǐ|繀,cuǐ|忰,cuì|疩,cuì|翆,cuì|脃,cuì|脆,cuì|啐,cuì|啛,cuì|悴,cuì|淬,cuì|萃,cuì|毳,cuì|焠,cuì|瘁,cuì|粹,cuì|膵,cuì|膬,cuì|竁,cuì|臎,cuì|琗,cuì|粋,cuì|脺,cuì|翠,cuì|邨,cūn|村,cūn|皴,cūn|澊,cūn|竴,cūn|存,cún|刌,cǔn|忖,cǔn|寸,cùn|籿,cùn|襊,cuō|搓,cuō|瑳,cuō|遳,cuō|磋,cuō|撮,cuō|蹉,cuō|醝,cuō|虘,cuó|嵯,cuó|痤,cuó|矬,cuó|蒫,cuó|鹾,cuó|鹺,cuó|嵳,cuó|脞,cuǒ|剉,cuò|剒,cuò|厝,cuò|夎,cuò|挫,cuò|莝,cuò|莡,cuò|措,cuò|逪,cuò|棤,cuò|锉,cuò|蓌,cuò|错,cuò|銼,cuò|錯,cuò|疸,da|咑,dā|哒,dā|耷,dā|畣,dā|搭,dā|嗒,dā|噠,dā|撘,dā|鎝,dā|笚,dā|矺,dā|褡,dā|墶,dá|达,dá|迏,dá|迖,dá|妲,dá|怛,dá|垯,dá|炟,dá|羍,dá|荅,dá|荙,dá|剳,dá|匒,dá|笪,dá|逹,dá|溚,dá|答,dá|詚,dá|達,dá|跶,dá|瘩,dá|靼,dá|薘,dá|鞑,dá|燵,dá|蟽,dá|鎉,dá|躂,dá|鐽,dá|韃,dá|龖,dá|龘,dá|搨,dá|繨,dá|打,dǎ|觰,dǎ|大,dà|亣,dà|眔,dà|橽,dà|汏,dà|呆,dāi|獃,dāi|懛,dāi|歹,dǎi|傣,dǎi|逮,dǎi|代,dài|轪,dài|侢,dài|垈,dài|岱,dài|帒,dài|甙,dài|绐,dài|迨,dài|带,dài|待,dài|柋,dài|殆,dài|玳,dài|贷,dài|帯,dài|軑,dài|埭,dài|帶,dài|紿,dài|蚮,dài|袋,dài|軚,dài|貸,dài|軩,dài|瑇,dài|廗,dài|叇,dài|曃,dài|緿,dài|鮘,dài|鴏,dài|戴,dài|艜,dài|黛,dài|簤,dài|蹛,dài|瀻,dài|霴,dài|襶,dài|靆,dài|螮,dài|蝳,dài|跢,dài|箉,dài|骀,dài|怠,dài|黱,dài|愖,dān|丹,dān|妉,dān|单,dān|担,dān|単,dān|眈,dān|砃,dān|耼,dān|耽,dān|郸,dān|聃,dān|躭,dān|酖,dān|單,dān|媅,dān|殚,dān|瘅,dān|匰,dān|箪,dān|褝,dān|鄲,dān|頕,dān|儋,dān|勯,dān|擔,dān|殫,dān|癉,dān|襌,dān|簞,dān|瓭,dān|卩,dān|亻,dān|娊,dān|噡,dān|聸,dān|伔,dǎn|刐,dǎn|狚,dǎn|玬,dǎn|胆,dǎn|衴,dǎn|紞,dǎn|掸,dǎn|亶,dǎn|馾,dǎn|撣,dǎn|澸,dǎn|黕,dǎn|膽,dǎn|丼,dǎn|抌,dǎn|赕,dǎn|賧,dǎn|黵,dǎn|黮,dǎn|繵,dàn|譂,dàn|旦,dàn|但,dàn|帎,dàn|沊,dàn|泹,dàn|诞,dàn|柦,dàn|疍,dàn|啖,dàn|啗,dàn|弹,dàn|惮,dàn|淡,dàn|蛋,dàn|啿,dàn|氮,dàn|腅,dàn|蜑,dàn|觛,dàn|窞,dàn|誕,dàn|僤,dàn|噉,dàn|髧,dàn|嘾,dàn|彈,dàn|憚,dàn|憺,dàn|澹,dàn|禫,dàn|餤,dàn|駳,dàn|鴠,dàn|甔,dàn|癚,dàn|嚪,dàn|贉,dàn|霮,dàn|饏,dàn|蟺,dàn|倓,dàn|惔,dàn|弾,dàn|醈,dàn|撢,dàn|萏,dàn|当,dāng|珰,dāng|裆,dāng|筜,dāng|儅,dāng|噹,dāng|澢,dāng|璫,dāng|襠,dāng|簹,dāng|艡,dāng|蟷,dāng|當,dāng|挡,dǎng|党,dǎng|谠,dǎng|擋,dǎng|譡,dǎng|黨,dǎng|灙,dǎng|欓,dǎng|讜,dǎng|氹,dàng|凼,dàng|圵,dàng|宕,dàng|砀,dàng|垱,dàng|荡,dàng|档,dàng|菪,dàng|瓽,dàng|逿,dàng|潒,dàng|碭,dàng|瞊,dàng|蕩,dàng|趤,dàng|壋,dàng|檔,dàng|璗,dàng|盪,dàng|礑,dàng|簜,dàng|蘯,dàng|闣,dàng|愓,dàng|嵣,dàng|偒,dàng|雼,dàng|裯,dāo|刀,dāo|叨,dāo|屶,dāo|忉,dāo|氘,dāo|舠,dāo|釖,dāo|鱽,dāo|魛,dāo|虭,dāo|捯,dáo|导,dǎo|岛,dǎo|陦,dǎo|倒,dǎo|宲,dǎo|捣,dǎo|祷,dǎo|禂,dǎo|搗,dǎo|隝,dǎo|嶋,dǎo|嶌,dǎo|槝,dǎo|導,dǎo|隯,dǎo|壔,dǎo|嶹,dǎo|蹈,dǎo|禱,dǎo|菿,dǎo|島,dǎo|帱,dào|幬,dào|到,dào|悼,dào|盗,dào|椡,dào|盜,dào|道,dào|稲,dào|翢,dào|噵,dào|稻,dào|衜,dào|檤,dào|衟,dào|翿,dào|軇,dào|瓙,dào|纛,dào|箌,dào|的,de|嘚,dē|恴,dé|得,dé|淂,dé|悳,dé|惪,dé|锝,dé|徳,dé|德,dé|鍀,dé|棏,dé|揼,dem|扥,den|扽,den|灯,dēng|登,dēng|豋,dēng|噔,dēng|嬁,dēng|燈,dēng|璒,dēng|竳,dēng|簦,dēng|艠,dēng|覴,dēng|蹬,dēng|墱,dēng|戥,děng|等,děng|澂,dèng|邓,dèng|僜,dèng|凳,dèng|鄧,dèng|隥,dèng|嶝,dèng|瞪,dèng|磴,dèng|镫,dèng|櫈,dèng|鐙,dèng|仾,dī|低,dī|奃,dī|彽,dī|袛,dī|啲,dī|埞,dī|羝,dī|隄,dī|堤,dī|趆,dī|嘀,dī|滴,dī|磾,dī|鍉,dī|鞮,dī|氐,dī|牴,dī|碮,dī|踧,dí|镝,dí|廸,dí|狄,dí|籴,dí|苖,dí|迪,dí|唙,dí|敌,dí|涤,dí|荻,dí|梑,dí|笛,dí|觌,dí|靮,dí|滌,dí|髢,dí|嫡,dí|蔋,dí|蔐,dí|頔,dí|魡,dí|敵,dí|篴,dí|嚁,dí|藡,dí|豴,dí|糴,dí|覿,dí|鸐,dí|藋,dí|鬄,dí|樀,dí|蹢,dí|鏑,dí|泜,dǐ|诋,dǐ|邸,dǐ|阺,dǐ|呧,dǐ|坻,dǐ|底,dǐ|弤,dǐ|抵,dǐ|拞,dǐ|柢,dǐ|砥,dǐ|掋,dǐ|菧,dǐ|詆,dǐ|軧,dǐ|聜,dǐ|骶,dǐ|鯳,dǐ|坘,dǐ|厎,dǐ|赿,dì|地,dì|弚,dì|坔,dì|弟,dì|旳,dì|杕,dì|玓,dì|怟,dì|枤,dì|苐,dì|帝,dì|埊,dì|娣,dì|递,dì|逓,dì|偙,dì|啇,dì|梊,dì|焍,dì|眱,dì|祶,dì|第,dì|菂,dì|谛,dì|釱,dì|媂,dì|棣,dì|睇,dì|缔,dì|蒂,dì|僀,dì|禘,dì|腣,dì|遞,dì|鉪,dì|馰,dì|墑,dì|墬,dì|摕,dì|碲,dì|蝃,dì|遰,dì|慸,dì|甋,dì|締,dì|嶳,dì|諦,dì|踶,dì|弔,dì|嵽,dì|諟,dì|珶,dì|渧,dì|蹏,dì|揥,dì|墆,dì|疐,dì|俤,dì|蔕,dì|嗲,diǎ|敁,diān|掂,diān|傎,diān|厧,diān|嵮,diān|滇,diān|槙,diān|瘨,diān|颠,diān|蹎,diān|巅,diān|顚,diān|顛,diān|癫,diān|巓,diān|巔,diān|攧,diān|癲,diān|齻,diān|槇,diān|典,diǎn|点,diǎn|婰,diǎn|敟,diǎn|椣,diǎn|碘,diǎn|蒧,diǎn|蕇,diǎn|踮,diǎn|點,diǎn|痶,diǎn|丶,diǎn|奌,diǎn|电,diàn|佃,diàn|甸,diàn|坫,diàn|店,diàn|垫,diàn|扂,diàn|玷,diàn|钿,diàn|唸,diàn|婝,diàn|惦,diàn|淀,diàn|奠,diàn|琔,diàn|殿,diàn|蜔,diàn|鈿,diàn|電,diàn|墊,diàn|橂,diàn|澱,diàn|靛,diàn|磹,diàn|癜,diàn|簟,diàn|驔,diàn|腍,diàn|橝,diàn|壂,diàn|刁,diāo|叼,diāo|汈,diāo|刟,diāo|凋,diāo|奝,diāo|弴,diāo|彫,diāo|蛁,diāo|琱,diāo|貂,diāo|碉,diāo|鳭,diāo|殦,diāo|雕,diāo|鮉,diāo|鲷,diāo|簓,diāo|鼦,diāo|鯛,diāo|鵰,diāo|颩,diāo|矵,diāo|錭,diāo|淍,diāo|屌,diǎo|鸼,diǎo|鵃,diǎo|扚,diǎo|伄,diào|吊,diào|钓,diào|窎,diào|訋,diào|调,diào|掉,diào|釣,diào|铞,diào|鈟,diào|竨,diào|銱,diào|雿,diào|調,diào|瘹,diào|窵,diào|鋽,diào|鑃,diào|誂,diào|嬥,diào|絩,diào|爹,diē|跌,diē|褺,diē|跮,dié|苵,dié|迭,dié|垤,dié|峌,dié|恎,dié|绖,dié|胅,dié|瓞,dié|眣,dié|耊,dié|啑,dié|戜,dié|谍,dié|喋,dié|堞,dié|幉,dié|惵,dié|揲,dié|畳,dié|絰,dié|耋,dié|臷,dié|詄,dié|趃,dié|叠,dié|殜,dié|牃,dié|牒,dié|镻,dié|碟,dié|蜨,dié|褋,dié|艓,dié|蝶,dié|諜,dié|蹀,dié|鲽,dié|曡,dié|鰈,dié|疉,dié|疊,dié|氎,dié|渉,dié|崼,dié|鮙,dié|跕,dié|鐡,dié|怢,dié|槢,dié|挃,dié|柣,dié|螲,dié|疂,dié|眰,diè|嚸,dim|丁,dīng|仃,dīng|叮,dīng|帄,dīng|玎,dīng|甼,dīng|疔,dīng|盯,dīng|耵,dīng|靪,dīng|奵,dīng|町,dīng|虰,dīng|酊,dǐng|顶,dǐng|頂,dǐng|鼎,dǐng|鼑,dǐng|薡,dǐng|鐤,dǐng|顁,dǐng|艼,dǐng|濎,dǐng|嵿,dǐng|钉,dìng|釘,dìng|订,dìng|忊,dìng|饤,dìng|矴,dìng|定,dìng|訂,dìng|飣,dìng|啶,dìng|萣,dìng|椗,dìng|腚,dìng|碇,dìng|锭,dìng|碠,dìng|聢,dìng|錠,dìng|磸,dìng|铤,dìng|鋌,dìng|掟,dìng|丟,diū|丢,diū|铥,diū|銩,diū|东,dōng|冬,dōng|咚,dōng|東,dōng|苳,dōng|昸,dōng|氡,dōng|倲,dōng|鸫,dōng|埬,dōng|娻,dōng|崬,dōng|涷,dōng|笗,dōng|菄,dōng|氭,dōng|蝀,dōng|鮗,dōng|鼕,dōng|鯟,dōng|鶇,dōng|鶫,dōng|徚,dōng|夂,dōng|岽,dōng|揰,dǒng|董,dǒng|墥,dǒng|嬞,dǒng|懂,dǒng|箽,dǒng|蕫,dǒng|諌,dǒng|湩,dǒng|动,dòng|冻,dòng|侗,dòng|垌,dòng|峒,dòng|峝,dòng|恫,dòng|挏,dòng|栋,dòng|洞,dòng|胨,dòng|迵,dòng|凍,dòng|戙,dòng|胴,dòng|動,dòng|崠,dòng|硐,dòng|棟,dòng|腖,dòng|働,dòng|詷,dòng|駧,dòng|霘,dòng|狫,dòng|烔,dòng|絧,dòng|衕,dòng|勭,dòng|騆,dòng|姛,dòng|瞗,dōu|吺,dōu|剅,dōu|唗,dōu|都,dōu|兜,dōu|兠,dōu|蔸,dōu|橷,dōu|篼,dōu|侸,dōu|艔,dóu|乧,dǒu|阧,dǒu|抖,dǒu|枓,dǒu|陡,dǒu|蚪,dǒu|鈄,dǒu|斗,dòu|豆,dòu|郖,dòu|浢,dòu|荳,dòu|逗,dòu|饾,dòu|鬥,dòu|梪,dòu|毭,dòu|脰,dòu|酘,dòu|痘,dòu|閗,dòu|窦,dòu|鬦,dòu|鋀,dòu|餖,dòu|斣,dòu|闘,dòu|竇,dòu|鬪,dòu|鬭,dòu|凟,dòu|鬬,dòu|剢,dū|阇,dū|嘟,dū|督,dū|醏,dū|闍,dū|厾,dū|毒,dú|涜,dú|读,dú|渎,dú|椟,dú|牍,dú|犊,dú|裻,dú|読,dú|獨,dú|錖,dú|匵,dú|嬻,dú|瀆,dú|櫝,dú|殰,dú|牘,dú|犢,dú|瓄,dú|皾,dú|騳,dú|讀,dú|豄,dú|贕,dú|韣,dú|髑,dú|鑟,dú|韇,dú|韥,dú|黷,dú|讟,dú|独,dú|樚,dú|襡,dú|襩,dú|黩,dú|笃,dǔ|堵,dǔ|帾,dǔ|琽,dǔ|赌,dǔ|睹,dǔ|覩,dǔ|賭,dǔ|篤,dǔ|暏,dǔ|笁,dǔ|陼,dǔ|芏,dù|妒,dù|杜,dù|肚,dù|妬,dù|度,dù|荰,dù|秺,dù|渡,dù|镀,dù|螙,dù|殬,dù|鍍,dù|蠧,dù|蠹,dù|剫,dù|晵,dù|靯,dù|篅,duān|偳,duān|媏,duān|端,duān|褍,duān|鍴,duān|剬,duān|短,duǎn|段,duàn|断,duàn|塅,duàn|缎,duàn|葮,d[...string is too long...]";
				FmMain.PinYin.hashtable = new Hashtable();
				foreach (string text2 in text.Split(new char[] { '|' }))
				{
					string text3 = text2.Substring(0, text2.IndexOf(","));
					string text4 = text2.Substring(1 + text2.IndexOf(","));
					FmMain.PinYin.hashtable[text3] = text4;
				}
			}

			public static string ToPinYin(string hanzi)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string text = "";
				foreach (char c in hanzi)
				{
					string text2 = c.ToString();
					if (FmMain.contain_ch(c.ToString()))
					{
						text2 = FmMain.PinYin.hashtable[c.ToString()] as string;
					}
					if (FmMain.contain_en(c.ToString()))
					{
						stringBuilder.Append(text2);
					}
					else if (FmMain.contain_en(text))
					{
						stringBuilder.Append(" " + text2 + " ");
					}
					else
					{
						stringBuilder.Append(text2 + " ");
					}
					text = text2;
				}
				return stringBuilder.ToString().Replace("  ", " ").Replace("\n ", "\n")
					.Replace(" \n", "\n")
					.Trim();
			}

			private static Hashtable hashtable;
		}

		[Serializable]
		public class TransObj
		{
			public string From
			{
				get
				{
					return this.from;
				}
				set
				{
					this.from = value;
				}
			}

			public string To
			{
				get
				{
					return this.to;
				}
				set
				{
					this.to = value;
				}
			}

			public List<FmMain.TransResult> Data
			{
				get
				{
					return this.data;
				}
				set
				{
					this.data = value;
				}
			}

			public List<FmMain.TransResult> data;

			public string from;

			public string to;
		}

		[Serializable]
		public class TransResult
		{
			public string Src
			{
				get
				{
					return this.src;
				}
				set
				{
					this.src = value;
				}
			}

			public string Dst
			{
				get
				{
					return this.dst;
				}
				set
				{
					this.dst = value;
				}
			}

			public string dst;

			public string src;
		}

		private class HtmlToText
		{
			static HtmlToText()
			{
				FmMain.HtmlToText._tags.Add("address", "\n");
				FmMain.HtmlToText._tags.Add("blockquote", "\n");
				FmMain.HtmlToText._tags.Add("div", "\n");
				FmMain.HtmlToText._tags.Add("dl", "\n");
				FmMain.HtmlToText._tags.Add("fieldset", "\n");
				FmMain.HtmlToText._tags.Add("form", "\n");
				FmMain.HtmlToText._tags.Add("h1", "\n");
				FmMain.HtmlToText._tags.Add("/h1", "\n");
				FmMain.HtmlToText._tags.Add("h2", "\n");
				FmMain.HtmlToText._tags.Add("/h2", "\n");
				FmMain.HtmlToText._tags.Add("h3", "\n");
				FmMain.HtmlToText._tags.Add("/h3", "\n");
				FmMain.HtmlToText._tags.Add("h4", "\n");
				FmMain.HtmlToText._tags.Add("/h4", "\n");
				FmMain.HtmlToText._tags.Add("h5", "\n");
				FmMain.HtmlToText._tags.Add("/h5", "\n");
				FmMain.HtmlToText._tags.Add("h6", "\n");
				FmMain.HtmlToText._tags.Add("/h6", "\n");
				FmMain.HtmlToText._tags.Add("p", "\n");
				FmMain.HtmlToText._tags.Add("/p", "\n");
				FmMain.HtmlToText._tags.Add("table", "\n");
				FmMain.HtmlToText._tags.Add("/table", "\n");
				FmMain.HtmlToText._tags.Add("ul", "\n");
				FmMain.HtmlToText._tags.Add("/ul", "\n");
				FmMain.HtmlToText._tags.Add("ol", "\n");
				FmMain.HtmlToText._tags.Add("/ol", "\n");
				FmMain.HtmlToText._tags.Add("/li", "\n");
				FmMain.HtmlToText._tags.Add("br", "\n");
				FmMain.HtmlToText._tags.Add("/td", "\t");
				FmMain.HtmlToText._tags.Add("/tr", "\n");
				FmMain.HtmlToText._tags.Add("/pre", "\n");
				FmMain.HtmlToText._ignoreTags = new HashSet<string>();
				FmMain.HtmlToText._ignoreTags.Add("script");
				FmMain.HtmlToText._ignoreTags.Add("noscript");
				FmMain.HtmlToText._ignoreTags.Add("style");
				FmMain.HtmlToText._ignoreTags.Add("object");
			}

			public string Convert(string html)
			{
				this._text = new FmMain.HtmlToText.TextBuilder();
				this._html = html;
				this._pos = 0;
				while (!this.EndOfText)
				{
					if (this.Peek() == '<')
					{
						bool flag;
						string text = this.ParseTag(out flag);
						if (text == "body")
						{
							this._text.Clear();
						}
						else if (text == "/body")
						{
							this._pos = this._html.Length;
						}
						else if (text == "pre")
						{
							this._text.Preformatted = true;
							this.EatWhitespaceToNextLine();
						}
						else if (text == "/pre")
						{
							this._text.Preformatted = false;
						}
						string text2;
						if (FmMain.HtmlToText._tags.TryGetValue(text, out text2))
						{
							this._text.Write(text2);
						}
						if (FmMain.HtmlToText._ignoreTags.Contains(text))
						{
							this.EatInnerContent(text);
						}
					}
					else if (char.IsWhiteSpace(this.Peek()))
					{
						this._text.Write(this._text.Preformatted ? this.Peek() : ' ');
						this.MoveAhead();
					}
					else
					{
						this._text.Write(this.Peek());
						this.MoveAhead();
					}
				}
				return HttpUtility.HtmlDecode(this._text.ToString());
			}

			protected string ParseTag(out bool selfClosing)
			{
				string text = string.Empty;
				selfClosing = false;
				if (this.Peek() == '<')
				{
					this.MoveAhead();
					this.EatWhitespace();
					int pos = this._pos;
					if (this.Peek() == '/')
					{
						this.MoveAhead();
					}
					while (!this.EndOfText && !char.IsWhiteSpace(this.Peek()) && this.Peek() != '/' && this.Peek() != '>')
					{
						this.MoveAhead();
					}
					text = this._html.Substring(pos, this._pos - pos).ToLower();
					while (!this.EndOfText && this.Peek() != '>')
					{
						if (this.Peek() == '"' || this.Peek() == '\'')
						{
							this.EatQuotedValue();
						}
						else
						{
							if (this.Peek() == '/')
							{
								selfClosing = true;
							}
							this.MoveAhead();
						}
					}
					this.MoveAhead();
				}
				return text;
			}

			protected void EatInnerContent(string tag)
			{
				string text = "/" + tag;
				while (!this.EndOfText)
				{
					if (this.Peek() == '<')
					{
						bool flag;
						if (this.ParseTag(out flag) == text)
						{
							return;
						}
						if (!flag && !tag.StartsWith("/"))
						{
							this.EatInnerContent(tag);
						}
					}
					else
					{
						this.MoveAhead();
					}
				}
			}

			protected bool EndOfText
			{
				get
				{
					return this._pos >= this._html.Length;
				}
			}

			protected char Peek()
			{
				if (this._pos >= this._html.Length)
				{
					return '\0';
				}
				return this._html[this._pos];
			}

			protected void MoveAhead()
			{
				this._pos = Math.Min(this._pos + 1, this._html.Length);
			}

			protected void EatWhitespace()
			{
				while (char.IsWhiteSpace(this.Peek()))
				{
					this.MoveAhead();
				}
			}

			protected void EatWhitespaceToNextLine()
			{
				while (char.IsWhiteSpace(this.Peek()))
				{
					int num = (int)this.Peek();
					this.MoveAhead();
					if (num == 10)
					{
						break;
					}
				}
			}

			protected void EatQuotedValue()
			{
				char c = this.Peek();
				if (c == '"' || c == '\'')
				{
					this.MoveAhead();
					int pos = this._pos;
					this._pos = this._html.IndexOfAny(new char[] { c, '\r', '\n' }, this._pos);
					if (this._pos < 0)
					{
						this._pos = this._html.Length;
						return;
					}
					this.MoveAhead();
				}
			}

			protected static Dictionary<string, string> _tags = new Dictionary<string, string>();

			protected static HashSet<string> _ignoreTags;

			protected FmMain.HtmlToText.TextBuilder _text;

			protected string _html;

			protected int _pos;

			protected class TextBuilder
			{
				public TextBuilder()
				{
					this._text = new StringBuilder();
					this._currLine = new StringBuilder();
					this._emptyLines = 0;
					this._preformatted = false;
				}

				public bool Preformatted
				{
					get
					{
						return this._preformatted;
					}
					set
					{
						if (value)
						{
							if (this._currLine.Length > 0)
							{
								this.FlushCurrLine();
							}
							this._emptyLines = 0;
						}
						this._preformatted = value;
					}
				}

				public void Clear()
				{
					this._text.Length = 0;
					this._currLine.Length = 0;
					this._emptyLines = 0;
				}

				public void Write(string s)
				{
					foreach (char c in s)
					{
						this.Write(c);
					}
				}

				public void Write(char c)
				{
					if (this._preformatted)
					{
						this._text.Append(c);
						return;
					}
					if (c != '\r')
					{
						if (c == '\n')
						{
							this.FlushCurrLine();
							return;
						}
						if (char.IsWhiteSpace(c))
						{
							int length = this._currLine.Length;
							if (length == 0 || !char.IsWhiteSpace(this._currLine[length - 1]))
							{
								this._currLine.Append(' ');
								return;
							}
						}
						else
						{
							this._currLine.Append(c);
						}
					}
				}

				protected void FlushCurrLine()
				{
					string text = this._currLine.ToString().Trim();
					if (text.Replace("\u00a0", string.Empty).Length == 0)
					{
						this._emptyLines++;
						if (this._emptyLines < 2 && this._text.Length > 0)
						{
							this._text.AppendLine(text);
						}
					}
					else
					{
						this._emptyLines = 0;
						this._text.AppendLine(text);
					}
					this._currLine.Length = 0;
				}

				public override string ToString()
				{
					if (this._currLine.Length > 0)
					{
						this.FlushCurrLine();
					}
					return this._text.ToString();
				}

				private StringBuilder _text;

				private StringBuilder _currLine;

				private int _emptyLines;

				private bool _preformatted;
			}
		}

		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			internal void <GetTextFromClipboard>b__89_0()
			{
				SendKeys.SendWait("^c");
				SendKeys.Flush();
			}

			public static readonly FmMain.<>c <>9 = new FmMain.<>c();

			public static ThreadStart <>9__89_0;
		}
	}
}
