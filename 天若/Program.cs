using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using TrOCR.Helper;

namespace TrOCR
{
	internal static class Program
	{
		public static void ProgramStart()
		{
			Program.ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "天若", out Program.createNew);
		}

		[STAThread]
		public static void Main(string[] args)
		{
			Program.SetConfig();
			Program.bool_error();
			Program.checkTimer = new System.Timers.Timer();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Version version = Environment.OSVersion.Version;
			Version version2 = new Version("6.1");
			Program.factor = Program.GetDpi_factor();
			if (version.CompareTo(version2) >= 0)
			{
				Program.SetProcessDPIAware();
			}
			Program.ProgramStart();
			if (!Program.createNew)
			{
				Program.ProgramStarted.Set();
				FmFlags fmFlags = new FmFlags();
				fmFlags.Show();
				fmFlags.DrawStr("软件已经运行");
				return;
			}
			if (args.Length != 0 && args[0] == "更新")
			{
				new FmSetting
				{
					Start_set = ""
				}.ShowDialog();
			}
			if (IniHelp.GetValue("更新", "检测更新") == "True" || IniHelp.GetValue("更新", "检测更新") == "发生错误")
			{
				new Thread(new ThreadStart(Program.CheckUpdate)).Start();
				if (IniHelp.GetValue("更新", "更新间隔") == "True")
				{
					Program.checkTimer.Enabled = true;
					Program.checkTimer.Interval = 3600000.0 * (double)Convert.ToInt32(IniHelp.GetValue("更新", "间隔时间"));
					Program.checkTimer.Elapsed += Program.CheckTimer_Elapsed;
					Program.checkTimer.Start();
				}
			}
			else
			{
				FmFlags fmFlags2 = new FmFlags();
				fmFlags2.Show();
				fmFlags2.DrawStr("天若");
			}
			Application.Run(new FmMain());
		}

		public static void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
		}

		[DllImport("wininet")]
		private static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

		public static bool IsConnectedInternet()
		{
			int num = 0;
			return Program.InternetGetConnectedState(out num, 0);
		}

		public static int GetPidByProcessName(string processName)
		{
			Process[] processesByName = Process.GetProcessesByName(processName);
			int num = 0;
			int num2;
			if (num >= processesByName.Length)
			{
				num2 = 0;
			}
			else
			{
				num2 = processesByName[num].Id;
			}
			return num2;
		}

		[DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();

		public static float GetDpi_factor()
		{
			float num;
			try
			{
				string text = "AppliedDPI";
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\WindowMetrics", true);
				string text2 = registryKey.GetValue(text).ToString();
				registryKey.Close();
				num = Convert.ToSingle(text2) / 96f;
			}
			catch
			{
				num = 1f;
			}
			return num;
		}

		public static void CheckUpdate()
		{
			string htmlContent = WebHelper.GetHtmlContent("https://www.jianshu.com/p/3afe79471cb9");
			if (string.IsNullOrEmpty(htmlContent))
			{
				return;
			}
			Match match = Regex.Match(htmlContent, "(?<=<pre><code>)[\\s\\S]+?(?=</code>)");
			if (match.Success)
			{
				JObject jobject = JObject.Parse(match.Value.Trim());
				string text = jobject["version"].Value<string>();
				string productVersion = Application.ProductVersion;
				if (!Program.CheckVersion(text, productVersion))
				{
					FmFlags fmFlags = new FmFlags();
					fmFlags.Show();
					fmFlags.DrawStr("当前已是最新版本");
					return;
				}
				FmFlags fmFlags2 = new FmFlags();
				fmFlags2.Show();
				fmFlags2.DrawStr("有新版本：" + text);
				if (jobject["full_update"].Value<bool>())
				{
					MessageBox.Show("发现新版本：" + text + "，请到百度网盘下载！", "提醒");
					Process.Start(jobject["pan_url"].Value<string>());
					return;
				}
				Process.Start("Data\\update.exe", string.Concat(new string[]
				{
					" ",
					jobject["main_url"].Value<string>(),
					" ",
					jobject["pan_url"].Value<string>(),
					" ",
					Path.Combine(Application.ExecutablePath, "天若.exe")
				}));
				Environment.Exit(0);
			}
		}

		private static bool CheckVersion(string newVersion, string curVersion)
		{
			string[] array = newVersion.Split(new char[] { '.' });
			string[] array2 = curVersion.Split(new char[] { '.' });
			for (int i = 0; i < array.Length; i++)
			{
				if (Convert.ToInt32(array[i]) > Convert.ToInt32(array2[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static void SetConfig()
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
			if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data"))
			{
				Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Data");
			}
			if (!File.Exists(text))
			{
				using (File.Create(text))
				{
				}
				IniHelp.SetValue("配置", "接口", "搜狗");
				IniHelp.SetValue("配置", "开机自启", "True");
				IniHelp.SetValue("配置", "快速翻译", "True");
				IniHelp.SetValue("配置", "识别弹窗", "True");
				IniHelp.SetValue("配置", "窗体动画", "窗体");
				IniHelp.SetValue("配置", "记录数目", "20");
				IniHelp.SetValue("配置", "自动保存", "True");
				IniHelp.SetValue("配置", "截图位置", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
				IniHelp.SetValue("配置", "翻译接口", "谷歌");
				IniHelp.SetValue("快捷键", "文字识别", "F4");
				IniHelp.SetValue("快捷键", "翻译文本", "F9");
				IniHelp.SetValue("快捷键", "记录界面", "请按下快捷键");
				IniHelp.SetValue("快捷键", "识别界面", "请按下快捷键");
				IniHelp.SetValue("密钥_百度", "secret_id", "YsZKG1wha34PlDOPYaIrIIKO");
				IniHelp.SetValue("密钥_百度", "secret_key", "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD");
				IniHelp.SetValue("代理", "代理类型", "系统代理");
				IniHelp.SetValue("代理", "服务器", "");
				IniHelp.SetValue("代理", "端口", "");
				IniHelp.SetValue("代理", "需要密码", "False");
				IniHelp.SetValue("代理", "服务器账号", "");
				IniHelp.SetValue("代理", "服务器密码", "");
				IniHelp.SetValue("更新", "检测更新", "True");
				IniHelp.SetValue("更新", "更新间隔", "True");
				IniHelp.SetValue("更新", "间隔时间", "24");
				IniHelp.SetValue("截图音效", "自动保存", "True");
				IniHelp.SetValue("截图音效", "音效路径", "Data\\screenshot.wav");
				IniHelp.SetValue("截图音效", "粘贴板", "False");
				IniHelp.SetValue("工具栏", "合并", "False");
				IniHelp.SetValue("工具栏", "分段", "False");
				IniHelp.SetValue("工具栏", "分栏", "False");
				IniHelp.SetValue("工具栏", "拆分", "False");
				IniHelp.SetValue("工具栏", "检查", "False");
				IniHelp.SetValue("工具栏", "翻译", "False");
				IniHelp.SetValue("工具栏", "顶置", "True");
				IniHelp.SetValue("取色器", "类型", "RGB");
			}
		}

		public static void bool_error()
		{
			if (IniHelp.GetValue("配置", "接口") == "发生错误")
			{
				IniHelp.SetValue("配置", "接口", "搜狗");
			}
			if (IniHelp.GetValue("配置", "开机自启") == "发生错误")
			{
				IniHelp.SetValue("配置", "开机自启", "True");
			}
			if (IniHelp.GetValue("配置", "快速翻译") == "发生错误")
			{
				IniHelp.SetValue("配置", "快速翻译", "True");
			}
			if (IniHelp.GetValue("配置", "识别弹窗") == "发生错误")
			{
				IniHelp.SetValue("配置", "识别弹窗", "True");
			}
			if (IniHelp.GetValue("配置", "窗体动画") == "发生错误")
			{
				IniHelp.SetValue("配置", "窗体动画", "窗体");
			}
			if (IniHelp.GetValue("配置", "记录数目") == "发生错误")
			{
				IniHelp.SetValue("配置", "记录数目", "20");
			}
			if (IniHelp.GetValue("配置", "自动保存") == "发生错误")
			{
				IniHelp.SetValue("配置", "自动保存", "True");
			}
			if (IniHelp.GetValue("配置", "翻译接口") == "发生错误")
			{
				IniHelp.SetValue("配置", "翻译接口", "谷歌");
			}
			if (IniHelp.GetValue("配置", "截图位置") == "发生错误")
			{
				IniHelp.SetValue("配置", "截图位置", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
			}
			if (IniHelp.GetValue("快捷键", "文字识别") == "发生错误")
			{
				IniHelp.SetValue("快捷键", "文字识别", "F4");
			}
			if (IniHelp.GetValue("快捷键", "翻译文本") == "发生错误")
			{
				IniHelp.SetValue("快捷键", "翻译文本", "F9");
			}
			if (IniHelp.GetValue("快捷键", "记录界面") == "发生错误")
			{
				IniHelp.SetValue("快捷键", "记录界面", "请按下快捷键");
			}
			if (IniHelp.GetValue("快捷键", "识别界面") == "发生错误")
			{
				IniHelp.SetValue("快捷键", "识别界面", "请按下快捷键");
			}
			if (IniHelp.GetValue("密钥_百度", "secret_id") == "发生错误")
			{
				IniHelp.SetValue("密钥_百度", "secret_id", "YsZKG1wha34PlDOPYaIrIIKO");
			}
			if (IniHelp.GetValue("密钥_百度", "secret_key") == "发生错误")
			{
				IniHelp.SetValue("密钥_百度", "secret_key", "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD");
			}
			if (IniHelp.GetValue("代理", "代理类型") == "发生错误")
			{
				IniHelp.SetValue("代理", "代理类型", "系统代理");
			}
			if (IniHelp.GetValue("代理", "服务器") == "发生错误")
			{
				IniHelp.SetValue("代理", "服务器", "");
			}
			if (IniHelp.GetValue("代理", "端口") == "发生错误")
			{
				IniHelp.SetValue("代理", "端口", "");
			}
			if (IniHelp.GetValue("代理", "需要密码") == "发生错误")
			{
				IniHelp.SetValue("代理", "需要密码", "False");
			}
			if (IniHelp.GetValue("代理", "服务器账号") == "发生错误")
			{
				IniHelp.SetValue("代理", "服务器账号", "");
			}
			if (IniHelp.GetValue("代理", "服务器密码") == "发生错误")
			{
				IniHelp.SetValue("代理", "服务器密码", "");
			}
			if (IniHelp.GetValue("更新", "检测更新") == "发生错误")
			{
				IniHelp.SetValue("更新", "检测更新", "True");
			}
			if (IniHelp.GetValue("更新", "更新间隔") == "发生错误")
			{
				IniHelp.SetValue("更新", "更新间隔", "True");
			}
			if (IniHelp.GetValue("更新", "间隔时间") == "发生错误")
			{
				IniHelp.SetValue("更新", "间隔时间", "24");
			}
			if (IniHelp.GetValue("截图音效", "自动保存") == "发生错误")
			{
				IniHelp.SetValue("截图音效", "自动保存", "True");
			}
			if (IniHelp.GetValue("截图音效", "音效路径") == "发生错误")
			{
				IniHelp.SetValue("截图音效", "音效路径", "Data\\screenshot.wav");
			}
			if (IniHelp.GetValue("截图音效", "粘贴板") == "发生错误")
			{
				IniHelp.SetValue("截图音效", "粘贴板", "False");
			}
			if (IniHelp.GetValue("工具栏", "合并") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "合并", "False");
			}
			if (IniHelp.GetValue("工具栏", "拆分") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "拆分", "False");
			}
			if (IniHelp.GetValue("工具栏", "检查") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "检查", "False");
			}
			if (IniHelp.GetValue("工具栏", "翻译") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "翻译", "False");
			}
			if (IniHelp.GetValue("工具栏", "分段") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "分段", "False");
			}
			if (IniHelp.GetValue("工具栏", "分栏") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "分栏", "False");
			}
			if (IniHelp.GetValue("工具栏", "顶置") == "发生错误")
			{
				IniHelp.SetValue("工具栏", "顶置", "True");
			}
			if (IniHelp.GetValue("取色器", "类型") == "发生错误")
			{
				IniHelp.SetValue("取色器", "类型", "RGB");
			}
			if (IniHelp.GetValue("特殊", "ali_cookie") == "发生错误")
			{
				IniHelp.SetValue("特殊", "ali_cookie", "cna=noXhE38FHGkCAXDve7YaZ8Tn; cnz=noXhE4/VhB8CAbZ773ApeV14; isg=BGJi2c2YTeeP6FG7S_Re8kveu-jEs2bNwToQnKz7jlWAfwL5lEO23eh9q3km9N5l; ");
			}
			if (IniHelp.GetValue("特殊", "ali_account") == "发生错误")
			{
				IniHelp.SetValue("特殊", "ali_account", "");
			}
			if (IniHelp.GetValue("特殊", "ali_password") == "发生错误")
			{
				IniHelp.SetValue("特殊", "ali_password", "");
			}
		}

		public static EventWaitHandle ProgramStarted;

		public static float factor;

		public static bool createNew;

		public static System.Timers.Timer checkTimer;
	}
}
