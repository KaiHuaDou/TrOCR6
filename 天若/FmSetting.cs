using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Microsoft.Win32;
using TrOCR.Properties;

namespace TrOCR
{
	public sealed partial class FmSetting : Form
	{
		public FmSetting()
		{
			this.Font = new Font(this.Font.Name, 9f / StaticValue.Dpifactor, this.Font.Style, this.Font.Unit, this.Font.GdiCharSet, this.Font.GdiVerticalFont);
			this.InitializeComponent();
		}

		public void readIniFile()
		{
			string value = IniHelp.GetValue("配置", "开机自启");
			if (value == "发生错误")
			{
				this.cbBox_开机.Checked = true;
			}
			try
			{
				this.cbBox_开机.Checked = Convert.ToBoolean(value);
			}
			catch
			{
				this.cbBox_开机.Checked = true;
			}
			string value2 = IniHelp.GetValue("配置", "快速翻译");
			if (value2 == "发生错误")
			{
				this.cbBox_翻译.Checked = true;
			}
			try
			{
				this.cbBox_翻译.Checked = Convert.ToBoolean(value2);
			}
			catch
			{
				this.cbBox_翻译.Checked = true;
			}
			string value3 = IniHelp.GetValue("配置", "识别弹窗");
			if (value3 == "发生错误")
			{
				this.cbBox_弹窗.Checked = true;
			}
			try
			{
				this.cbBox_弹窗.Checked = Convert.ToBoolean(value3);
			}
			catch
			{
				this.cbBox_弹窗.Checked = true;
			}
			string value4 = IniHelp.GetValue("配置", "窗体动画");
			this.cobBox_动画.Text = value4;
			if (value4 == "发生错误")
			{
				this.cobBox_动画.Text = "窗体";
			}
			string value5 = IniHelp.GetValue("配置", "记录数目");
			this.numbox_记录.Value = Convert.ToInt32(value5);
			if (value5 == "发生错误")
			{
				this.numbox_记录.Value = 20m;
			}
			string value6 = IniHelp.GetValue("配置", "自动保存");
			if (value6 == "发生错误")
			{
				this.cbBox_保存.Checked = false;
			}
			try
			{
				this.cbBox_保存.Checked = Convert.ToBoolean(value6);
			}
			catch
			{
				this.cbBox_保存.Checked = false;
			}
			if (this.cbBox_保存.Checked)
			{
				this.textBox_path.Enabled = true;
				this.btn_浏览.Enabled = true;
			}
			if (!this.cbBox_保存.Checked)
			{
				this.textBox_path.Enabled = false;
				this.btn_浏览.Enabled = false;
			}
			string value7 = IniHelp.GetValue("配置", "截图位置");
			this.textBox_path.Text = value7;
			if (value7 == "发生错误")
			{
				this.textBox_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			}
			string value8 = IniHelp.GetValue("快捷键", "文字识别");
			this.txtBox_文字识别.Text = value8;
			if (value8 == "发生错误")
			{
				this.txtBox_文字识别.Text = "F4";
			}
			string value9 = IniHelp.GetValue("快捷键", "翻译文本");
			this.txtBox_翻译文本.Text = value9;
			if (value9 == "发生错误")
			{
				this.txtBox_翻译文本.Text = "F9";
			}
			string value10 = IniHelp.GetValue("快捷键", "记录界面");
			this.txtBox_记录界面.Text = value10;
			if (value10 == "发生错误")
			{
				this.txtBox_记录界面.Text = "请按下快捷键";
			}
			string value11 = IniHelp.GetValue("快捷键", "识别界面");
			this.txtBox_识别界面.Text = value11;
			if (value11 == "发生错误")
			{
				this.txtBox_识别界面.Text = "请按下快捷键";
			}
			this.pictureBox_文字识别.Image = ((this.txtBox_文字识别.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1);
			this.pictureBox_翻译文本.Image = ((this.txtBox_翻译文本.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1);
			this.pictureBox_记录界面.Image = ((this.txtBox_记录界面.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1);
			this.pictureBox_识别界面.Image = ((this.txtBox_识别界面.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1);
			string value12 = IniHelp.GetValue("密钥_百度", "secret_id");
			this.text_baiduaccount.Text = value12;
			if (value12 == "发生错误")
			{
				this.text_baiduaccount.Text = "YsZKG1wha34PlDOPYaIrIIKO";
			}
			string value13 = IniHelp.GetValue("密钥_百度", "secret_key");
			this.text_baidupassword.Text = value13;
			if (value13 == "发生错误")
			{
				this.text_baidupassword.Text = "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD";
			}
			string value14 = IniHelp.GetValue("代理", "代理类型");
			this.combox_代理.Text = value14;
			if (value14 == "发生错误")
			{
				this.combox_代理.Text = "系统代理";
			}
			if (this.combox_代理.Text == "不使用代理" || this.combox_代理.Text == "系统代理")
			{
				this.text_账号.Enabled = false;
				this.text_密码.Enabled = false;
				this.chbox_代理服务器.Enabled = false;
				this.text_端口.Enabled = false;
				this.text_服务器.Enabled = false;
			}
			if (this.combox_代理.Text == "自定义代理")
			{
				this.text_端口.Enabled = true;
				this.text_服务器.Enabled = true;
			}
			string value15 = IniHelp.GetValue("代理", "服务器");
			this.text_服务器.Text = value15;
			if (value15 == "发生错误")
			{
				this.text_服务器.Text = "127.0.0.1";
			}
			string value16 = IniHelp.GetValue("代理", "端口");
			this.text_端口.Text = value16;
			if (value16 == "发生错误")
			{
				this.text_端口.Text = "1080";
			}
			string value17 = IniHelp.GetValue("代理", "需要密码");
			if (value17 == "发生错误")
			{
				this.chbox_代理服务器.Checked = false;
			}
			try
			{
				this.chbox_代理服务器.Checked = Convert.ToBoolean(value17);
			}
			catch
			{
				this.chbox_代理服务器.Checked = false;
			}
			string value18 = IniHelp.GetValue("代理", "服务器账号");
			this.text_账号.Text = value18;
			if (value18 == "发生错误")
			{
				this.text_账号.Text = "";
			}
			string value19 = IniHelp.GetValue("代理", "服务器密码");
			this.text_密码.Text = value19;
			if (value19 == "发生错误")
			{
				this.text_密码.Text = "";
			}
			if (this.chbox_代理服务器.Checked)
			{
				this.text_账号.Enabled = true;
				this.text_密码.Enabled = true;
			}
			if (!this.chbox_代理服务器.Checked)
			{
				this.text_账号.Enabled = false;
				this.text_密码.Enabled = false;
			}
			string value20 = IniHelp.GetValue("更新", "检测更新");
			if (value20 == "发生错误")
			{
				this.check_检查更新.Checked = false;
			}
			try
			{
				this.check_检查更新.Checked = Convert.ToBoolean(value20);
			}
			catch
			{
				this.check_检查更新.Checked = false;
			}
			if (this.check_检查更新.Checked)
			{
				this.checkBox_更新间隔.Enabled = true;
			}
			if (!this.check_检查更新.Checked)
			{
				this.checkBox_更新间隔.Enabled = false;
				this.numbox_间隔时间.Enabled = false;
			}
			string value21 = IniHelp.GetValue("更新", "更新间隔");
			if (value21 == "发生错误")
			{
				this.checkBox_更新间隔.Checked = false;
			}
			try
			{
				this.checkBox_更新间隔.Checked = Convert.ToBoolean(value21);
			}
			catch
			{
				this.checkBox_更新间隔.Checked = false;
			}
			if (this.checkBox_更新间隔.Checked)
			{
				this.numbox_间隔时间.Enabled = true;
			}
			if (!this.checkBox_更新间隔.Checked)
			{
				this.numbox_间隔时间.Enabled = false;
			}
			string value22 = IniHelp.GetValue("更新", "间隔时间");
			this.numbox_间隔时间.Value = Convert.ToInt32(value22);
			if (value5 == "发生错误")
			{
				this.numbox_间隔时间.Value = 24m;
			}
			string value23 = IniHelp.GetValue("截图音效", "粘贴板");
			if (value23 == "发生错误")
			{
				this.chbox_copy.Checked = false;
			}
			try
			{
				this.chbox_copy.Checked = Convert.ToBoolean(value23);
			}
			catch
			{
				this.chbox_copy.Checked = false;
			}
			string value24 = IniHelp.GetValue("截图音效", "自动保存");
			if (value24 == "发生错误")
			{
				this.chbox_save.Checked = true;
			}
			try
			{
				this.chbox_save.Checked = Convert.ToBoolean(value24);
			}
			catch
			{
				this.chbox_save.Checked = true;
			}
			string value25 = IniHelp.GetValue("截图音效", "音效路径");
			this.text_音效path.Text = value25;
			if (value25 == "发生错误")
			{
				this.text_音效path.Text = "Data\\screenshot.wav";
			}
			string value26 = IniHelp.GetValue("取色器", "类型");
			if (value26 == "发生错误")
			{
				this.chbox_取色.Checked = false;
			}
			if (value26 == "RGB")
			{
				this.chbox_取色.Checked = false;
			}
			if (value26 == "HEX")
			{
				this.chbox_取色.Checked = true;
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FmMain));
			base.Icon = (Icon)componentResourceManager.GetObject("minico.Icon");
			NumericUpDown numericUpDown = this.numbox_记录;
			int[] array = new int[4];
			array[0] = 99;
			numericUpDown.Maximum = new decimal(array);
			NumericUpDown numericUpDown2 = this.numbox_记录;
			int[] array2 = new int[4];
			array2[0] = 1;
			numericUpDown2.Minimum = new decimal(array2);
			NumericUpDown numericUpDown3 = this.numbox_记录;
			int[] array3 = new int[4];
			array3[0] = 1;
			numericUpDown3.Value = new decimal(array3);
			NumericUpDown numericUpDown4 = this.numbox_间隔时间;
			int[] array4 = new int[4];
			array4[0] = 24;
			numericUpDown4.Maximum = new decimal(array4);
			NumericUpDown numericUpDown5 = this.numbox_间隔时间;
			int[] array5 = new int[4];
			array5[0] = 1;
			numericUpDown5.Minimum = new decimal(array5);
			NumericUpDown numericUpDown6 = this.numbox_间隔时间;
			int[] array6 = new int[4];
			array6[0] = 1;
			numericUpDown6.Value = new decimal(array6);
			this.tab_标签.Height = (int)(350.0 * (double)Program.factor);
			base.Height = this.tab_标签.Height + 50;
			this.readIniFile();
			this.chbox_代理服务器.CheckedChanged += this.chbox_代理服务器_CheckedChanged;
			this.更新Button_check.Click += this.更新Button_check_Click;
			this.label_更新日期.Text = "更新时间：" + StaticValue.v_date;
			this.label_版本号.Text = "当前版本：" + StaticValue.current_v;
			this.txt_更新说明.Text = (string)componentResourceManager.GetObject("更新说明");
			this.txt_更新说明.ReadOnly = true;
			this.txt_更新说明.WordWrap = true;
			this.txt_更新说明.ScrollBars = ScrollBars.Vertical;
		}

		private void 百度申请_Click(object sender, EventArgs e)
		{
			Process.Start("https://console.bce.baidu.com/ai/");
		}

		public static string Get_html(string url)
		{
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			string text;
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

		private void tab_标签_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.tab_标签.SelectedTab == this.page_常规)
			{
				this.tab_标签.Height = (int)(350.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_快捷键)
			{
				this.tab_标签.Height = (int)(225.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_密钥)
			{
				this.tab_标签.Height = (int)(190.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_代理)
			{
				this.tab_标签.Height = (int)(245.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_更新)
			{
				this.tab_标签.Height = (int)(135.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_关于)
			{
				this.tab_标签.Height = (int)(340.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_赞助)
			{
				this.tab_标签.Height = (int)(225.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
			if (this.tab_标签.SelectedTab == this.Page_反馈)
			{
				this.tab_标签.Height = (int)(200.0 * (double)Program.factor);
				base.Height = this.tab_标签.Height + 50;
			}
		}

		private void pic_help_Click(object sender, EventArgs e)
		{
			new FmHelp().Show();
		}

		private void cbBox_开机_CheckedChanged(object sender, EventArgs e)
		{
			FmSetting.AutoStart(this.cbBox_开机.Checked);
		}

		private void cbBox_翻译_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void cbBox_弹窗_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void cobBox_动画_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void numbox_记录_ValueChanged(object sender, EventArgs e)
		{
		}

		private void cbBox_保存_CheckedChanged(object sender, EventArgs e)
		{
			if (this.cbBox_保存.Checked)
			{
				this.textBox_path.Enabled = true;
				this.btn_浏览.Enabled = true;
			}
			if (!this.cbBox_保存.Checked)
			{
				this.textBox_path.Enabled = false;
				this.btn_浏览.Enabled = false;
			}
		}

		private void btn_浏览_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				this.textBox_path.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void 密钥Button_Click(object sender, EventArgs e)
		{
			this.text_baiduaccount.Text = "YsZKG1wha34PlDOPYaIrIIKO";
			this.text_baidupassword.Text = "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD";
		}

		private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
		{
		}

		private void 常规Button_Click(object sender, EventArgs e)
		{
			this.cbBox_开机.Checked = true;
			this.cbBox_翻译.Checked = true;
			this.cbBox_弹窗.Checked = true;
			this.cobBox_动画.SelectedIndex = 0;
			this.numbox_记录.Value = 20m;
			this.cbBox_保存.Checked = true;
			this.textBox_path.Enabled = true;
			this.textBox_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			this.btn_浏览.Enabled = true;
			this.chbox_save.Checked = true;
			this.text_音效path.Text = "Data\\screenshot.wav";
			this.chbox_copy.Checked = false;
			this.chbox_取色.Checked = false;
		}

		private void txtBox_KeyUp(object sender, KeyEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			Regex regex = new Regex("[一-龥]+");
			string text = "";
			foreach (object obj in regex.Matches(textBox.Name))
			{
				text = ((Match)obj).ToString();
			}
			string text2 = "pictureBox_" + text;
			PictureBox pictureBox = (PictureBox)base.Controls.Find(text2, true)[0];
			new ComponentResourceManager(typeof(FmSetting));
			if (e.KeyData == Keys.Back)
			{
				textBox.Text = "请按下快捷键";
				pictureBox.Image = Resources.快捷键_0;
				if (textBox.Name.Contains("文字识别"))
				{
					IniHelp.SetValue("快捷键", "文字识别", this.txtBox_文字识别.Text);
				}
				if (textBox.Name.Contains("翻译文本"))
				{
					IniHelp.SetValue("快捷键", "翻译文本", this.txtBox_翻译文本.Text);
				}
				if (textBox.Name.Contains("记录界面"))
				{
					IniHelp.SetValue("快捷键", "记录界面", this.txtBox_记录界面.Text);
				}
				if (textBox.Name.Contains("识别界面"))
				{
					IniHelp.SetValue("快捷键", "识别界面", this.txtBox_识别界面.Text);
					return;
				}
			}
			else if (e.KeyValue != 16 && e.KeyValue != 17 && e.KeyValue != 18)
			{
				string[] array = e.KeyData.ToString().Replace(" ", "").Replace("Control", "Ctrl")
					.Split(new char[] { ',' });
				pictureBox.Image = Resources.快捷键_1;
				if (array.Length == 1)
				{
					textBox.Text = array[0];
				}
				if (array.Length == 2)
				{
					textBox.Text = array[1] + "+" + array[0];
				}
				if (array.Length <= 2)
				{
					if (textBox.Name.Contains("文字识别"))
					{
						IniHelp.SetValue("快捷键", "文字识别", this.txtBox_文字识别.Text);
					}
					if (textBox.Name.Contains("翻译文本"))
					{
						IniHelp.SetValue("快捷键", "翻译文本", this.txtBox_翻译文本.Text);
					}
					if (textBox.Name.Contains("记录界面"))
					{
						IniHelp.SetValue("快捷键", "记录界面", this.txtBox_记录界面.Text);
					}
					if (textBox.Name.Contains("识别界面"))
					{
						IniHelp.SetValue("快捷键", "识别界面", this.txtBox_识别界面.Text);
					}
				}
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return (keyData == Keys.Tab && this.txtBox_文字识别.Focused) || (keyData == Keys.Tab && this.txtBox_翻译文本.Focused) || (keyData == Keys.Tab && this.txtBox_记录界面.Focused) || (keyData == Keys.Tab && this.txtBox_识别界面.Focused);
		}

		private void txtBox_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
		}

		private void 快捷键Button_Click(object sender, EventArgs e)
		{
			new ComponentResourceManager(typeof(FmSetting));
			this.txtBox_文字识别.Text = "F4";
			this.pictureBox_文字识别.Image = Resources.快捷键_1;
			this.txtBox_翻译文本.Text = "F9";
			this.pictureBox_翻译文本.Image = Resources.快捷键_1;
			this.txtBox_记录界面.Text = "请按下快捷键";
			this.pictureBox_记录界面.Image = Resources.快捷键_0;
			this.txtBox_识别界面.Text = "请按下快捷键";
			this.pictureBox_识别界面.Image = Resources.快捷键_0;
		}

		private void 百度_btn_Click(object sender, EventArgs e)
		{
			if (FmSetting.Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + this.text_baiduaccount.Text + "&client_secret=" + this.text_baidupassword.Text)) != "")
			{
				MessageBox.Show("密钥正确!", "提醒");
				return;
			}
			MessageBox.Show("请确保密钥正确!", "提醒");
		}

		private void combox_代理_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.combox_代理.Text == "不使用代理" || this.combox_代理.Text == "系统代理")
			{
				this.text_账号.Enabled = false;
				this.text_密码.Enabled = false;
				this.chbox_代理服务器.Enabled = false;
				this.text_端口.Enabled = false;
				this.chbox_代理服务器.Checked = false;
				this.text_服务器.Enabled = false;
				this.text_服务器.Text = "";
				this.text_端口.Text = "";
				this.text_服务器.Text = "";
				this.text_账号.Text = "";
				this.text_密码.Text = "";
			}
			if (this.combox_代理.Text == "自定义代理")
			{
				this.text_端口.Enabled = true;
				this.text_服务器.Enabled = true;
				this.chbox_代理服务器.Enabled = true;
			}
		}

		private void text_端口_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
		{
		}

		private void text_baiduaccount_TextChanged(object sender, EventArgs e)
		{
		}

		private void text_baidupassword_TextChanged(object sender, EventArgs e)
		{
		}

		private void text_服务器_TextChanged(object sender, EventArgs e)
		{
		}

		private void text_端口_TextChanged(object sender, EventArgs e)
		{
		}

		private void chbox_代理服务器_CheckedChanged(object sender, EventArgs e)
		{
			if (this.chbox_代理服务器.Checked)
			{
				this.text_账号.Enabled = true;
				this.text_密码.Enabled = true;
			}
			if (!this.chbox_代理服务器.Checked)
			{
				this.text_账号.Enabled = false;
				this.text_密码.Enabled = false;
			}
		}

		private void text_账号_TextChanged(object sender, EventArgs e)
		{
		}

		private void text_密码_TextChanged(object sender, EventArgs e)
		{
		}

		private void 代理Button_Click(object sender, EventArgs e)
		{
			this.combox_代理.Text = "系统代理";
			this.text_账号.Enabled = false;
			this.text_密码.Enabled = false;
			this.chbox_代理服务器.Enabled = false;
			this.text_端口.Enabled = false;
			this.text_服务器.Enabled = false;
		}

		private void check_检查更新_CheckedChanged(object sender, EventArgs e)
		{
			if (this.check_检查更新.Checked)
			{
				this.checkBox_更新间隔.Enabled = true;
				this.checkBox_更新间隔.Checked = true;
				this.numbox_间隔时间.Enabled = true;
			}
			if (!this.check_检查更新.Checked)
			{
				this.checkBox_更新间隔.Checked = false;
				this.checkBox_更新间隔.Enabled = false;
				this.numbox_间隔时间.Enabled = false;
			}
		}

		private void checkBox_更新间隔_CheckedChanged(object sender, EventArgs e)
		{
			if (this.checkBox_更新间隔.Checked)
			{
				this.numbox_间隔时间.Enabled = true;
			}
			if (!this.checkBox_更新间隔.Checked)
			{
				this.numbox_间隔时间.Enabled = false;
			}
		}

		private void numbox_间隔时间_ValueChanged(object sender, EventArgs e)
		{
		}

		private void 更新Button_Click(object sender, EventArgs e)
		{
			this.numbox_间隔时间.Value = 24m;
			this.check_检查更新.Checked = true;
			this.checkBox_更新间隔.Checked = true;
		}

		private void 更新Button_check_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(Program.CheckUpdate)).Start();
		}

		private void 反馈Button_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(this.反馈send)).Start();
		}

		public string Post_Html(string url, string post_str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(post_str);
			string text = "";
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			httpWebRequest.Timeout = 6000;
			httpWebRequest.Proxy = null;
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
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

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			IniHelp.SetValue("配置", "开机自启", this.cbBox_开机.Checked.ToString());
			IniHelp.SetValue("配置", "快速翻译", this.cbBox_翻译.Checked.ToString());
			IniHelp.SetValue("配置", "识别弹窗", this.cbBox_弹窗.Checked.ToString());
			IniHelp.SetValue("配置", "窗体动画", this.cobBox_动画.Text);
			IniHelp.SetValue("配置", "记录数目", this.numbox_记录.Text);
			IniHelp.SetValue("配置", "自动保存", this.cbBox_保存.Checked.ToString());
			IniHelp.SetValue("配置", "截图位置", this.textBox_path.Text);
			IniHelp.SetValue("快捷键", "文字识别", this.txtBox_文字识别.Text);
			IniHelp.SetValue("快捷键", "翻译文本", this.txtBox_翻译文本.Text);
			IniHelp.SetValue("快捷键", "记录界面", this.txtBox_记录界面.Text);
			IniHelp.SetValue("快捷键", "识别界面", this.txtBox_识别界面.Text);
			IniHelp.SetValue("密钥_百度", "secret_id", this.text_baiduaccount.Text);
			IniHelp.SetValue("密钥_百度", "secret_key", this.text_baidupassword.Text);
			IniHelp.SetValue("代理", "代理类型", this.combox_代理.Text);
			IniHelp.SetValue("代理", "服务器", this.text_服务器.Text);
			IniHelp.SetValue("代理", "端口", this.text_端口.Text);
			IniHelp.SetValue("代理", "需要密码", this.chbox_代理服务器.Checked.ToString());
			IniHelp.SetValue("代理", "服务器账号", this.text_账号.Text);
			IniHelp.SetValue("代理", "服务器密码", this.text_密码.Text);
			IniHelp.SetValue("更新", "检测更新", this.check_检查更新.Checked.ToString());
			IniHelp.SetValue("更新", "更新间隔", this.checkBox_更新间隔.Checked.ToString());
			IniHelp.SetValue("更新", "间隔时间", this.numbox_间隔时间.Value.ToString());
			IniHelp.SetValue("截图音效", "自动保存", this.chbox_save.Checked.ToString());
			IniHelp.SetValue("截图音效", "音效路径", this.text_音效path.Text);
			IniHelp.SetValue("截图音效", "粘贴板", this.chbox_copy.Checked.ToString());
			if (!this.chbox_取色.Checked)
			{
				IniHelp.SetValue("取色器", "类型", "RGB");
			}
			if (this.chbox_取色.Checked)
			{
				IniHelp.SetValue("取色器", "类型", "HEX");
			}
			base.DialogResult = DialogResult.OK;
		}

		public static void AutoStart(bool isAuto)
		{
			try
			{
				string text = Application.ExecutablePath.Replace("/", "\\");
				if (isAuto)
				{
					RegistryKey currentUser = Registry.CurrentUser;
					RegistryKey registryKey = currentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
					registryKey.SetValue("tianruoOCR", text);
					registryKey.Close();
					currentUser.Close();
				}
				else
				{
					RegistryKey currentUser2 = Registry.CurrentUser;
					RegistryKey registryKey2 = currentUser2.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
					registryKey2.DeleteValue("tianruoOCR", false);
					registryKey2.Close();
					currentUser2.Close();
				}
			}
			catch (Exception)
			{
				MessageBox.Show("您需要管理员权限修改", "提示");
			}
		}

		private void 反馈send()
		{
			if (this.txt_问题反馈.Text != "")
			{
				string text = "sm=%E5%A4%A9%E8%8B%A5OCR%E6%96%87%E5%AD%97%E8%AF%86%E5%88%AB" + StaticValue.current_v + "&nr=";
				this.Post_Html("http://cd.ys168.com/f_ht/ajcx/lyd.aspx?cz=lytj&pdgk=1&pdgly=0&pdzd=0&tou=1&yzm=undefined&_dlmc=tianruoyouxin&_dlmm=", text + HttpUtility.UrlEncode(this.txt_问题反馈.Text));
				this.txt_问题反馈.Text = "";
				FmFlags fmFlags = new FmFlags();
				fmFlags.Show();
				fmFlags.DrawStr("感谢您的反馈！");
				return;
			}
			FmFlags fmFlags2 = new FmFlags();
			fmFlags2.Show();
			fmFlags2.DrawStr("反馈文本不能为空");
		}

		public void PlaySong(string file)
		{
			HelpWin32.mciSendString("close media", null, 0, IntPtr.Zero);
			HelpWin32.mciSendString("open \"" + file + "\" type mpegvideo alias media", null, 0, IntPtr.Zero);
			HelpWin32.mciSendString("play media notify", null, 0, base.Handle);
		}

		private void btn_音效_Click(object sender, EventArgs e)
		{
			this.PlaySong(this.text_音效path.Text);
		}

		private void btn_音效路径_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "请选择音效文件";
			openFileDialog.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";
			openFileDialog.RestoreDirectory = true;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.text_音效path.Text = Path.GetFullPath(openFileDialog.FileName);
			}
		}

		private void chbox_copy_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void chbox_save_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void chbox_取色_CheckedChanged(object sender, EventArgs e)
		{
		}

		public string Start_set
		{
			set
			{
				this.tab_标签.SelectedIndex = 5;
			}
		}
	}
}
