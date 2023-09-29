using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

internal static class Program
{
    public static EventWaitHandle ProgramStarted;
    public static float DpiFactor;
    public static bool createNew;
    public static System.Timers.Timer updateTimer;

    public static void ProgramStart( )
        => ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "天若", out createNew);

    [STAThread]
    public static void Main(string[] args)
    {
        InitConfig( );
        CheckConfig( );
        updateTimer = new System.Timers.Timer( );
        Application.EnableVisualStyles( );
        Application.SetCompatibleTextRenderingDefault(false);
        Version OsVersion = Environment.OSVersion.Version;
        DpiFactor = GetDpiFactor( );
        if (OsVersion.CompareTo(new Version("6.1")) >= 0)
        {
            SetProcessDPIAware( );
        }
        ProgramStart( );
        if (!createNew)
        {
            ProgramStarted.Set( );
            FmFlags fmFlags = new( );
            fmFlags.Show( );
            fmFlags.DrawStr("软件已经运行");
            return;
        }
        if (args.Length != 0 && args[0] == "更新")
        {
            new FmSetting { Start_set = "" }.ShowDialog( );
        }
        if (Config.Get("更新", "检测更新") is "True" or "__ERROR__")
        {
            new Thread(CheckUpdate).Start( );
            if (Config.Get("更新", "更新间隔") == "True")
            {
                updateTimer.Enabled = true;
                updateTimer.Interval = 3600000.0 * Convert.ToInt32(Config.Get("更新", "间隔时间"));
                updateTimer.Elapsed += CheckTimer_Elapsed;
                updateTimer.Start( );
            }
        }
        else
        {
            FmFlags fmFlags = new( );
            fmFlags.Show( );
            fmFlags.DrawStr("天若");
        }
        Application.Run(new FmMain( ));
    }

    public static void CheckTimer_Elapsed(object o, ElapsedEventArgs e)
        => CheckUpdate( );

    public static float GetDpiFactor( )
    {
        float factor;
        try
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\WindowMetrics", true);
            string dpi = registryKey.GetValue("AppliedDPI").ToString( );
            registryKey.Close( );
            factor = Convert.ToSingle(dpi) / 96f;
        }
        catch
        {
            factor = 1f;
        }
        return factor;
    }

    public static void CheckUpdate( )
    {
#pragma warning disable
        return; //TODO: CheckUpdate
        string htmlContent = WebHelper.GetHtmlContent("https://www.jianshu.com/p/3afe79471cb9");
        if (string.IsNullOrEmpty(htmlContent))
        {
            return;
        }
        Match match = Regex.Match(htmlContent, "(?<=<pre><code>)[\\s\\S]+?(?=</code>)");
        if (match.Success)
        {
            JObject jobject = JObject.Parse(match.Value.Trim( ));
            string text = jobject["version"].Value<string>( );
            string productVersion = Application.ProductVersion;
            if (!CheckVersion(text, productVersion))
            {
                FmFlags fmFlags = new( );
                fmFlags.Show( );
                fmFlags.DrawStr("当前已是最新版本");
                return;
            }
            FmFlags fmFlags2 = new( );
            fmFlags2.Show( );
            fmFlags2.DrawStr("有新版本：" + text);
            if (jobject["full_update"].Value<bool>( ))
            {
                MessageBox.Show("发现新版本：" + text + "，请到百度网盘下载！", "提醒");
                Process.Start(jobject["pan_url"].Value<string>( ));
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
#pragma warning restore
    }

    private static bool CheckVersion(string version, string current)
    {
        string[] array = version.Split(new char[] { '.' });
        string[] array2 = current.Split(new char[] { '.' });
        for (int i = 0; i < array.Length; i++)
        {
            if (Convert.ToInt32(array[i]) > Convert.ToInt32(array2[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static void InitConfig( )
    {
        string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data"))
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Data");
        }
        if (File.Exists(text))
            return;
        File.Create(text);
        Config.Set("配置", "接口", "搜狗");
        Config.Set("配置", "开机自启", "True");
        Config.Set("配置", "快速翻译", "True");
        Config.Set("配置", "识别弹窗", "True");
        Config.Set("配置", "窗体动画", "窗体");
        Config.Set("配置", "记录数目", "20");
        Config.Set("配置", "自动保存", "True");
        Config.Set("配置", "截图位置", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        Config.Set("配置", "翻译接口", "谷歌");
        Config.Set("快捷键", "文字识别", "F4");
        Config.Set("快捷键", "翻译文本", "F9");
        Config.Set("快捷键", "记录界面", "请按下快捷键");
        Config.Set("快捷键", "识别界面", "请按下快捷键");
        Config.Set("密钥_百度", "secret_id", "YsZKG1wha34PlDOPYaIrIIKO");
        Config.Set("密钥_百度", "secret_key", "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD");
        Config.Set("代理", "代理类型", "系统代理");
        Config.Set("代理", "服务器", "");
        Config.Set("代理", "端口", "");
        Config.Set("代理", "需要密码", "False");
        Config.Set("代理", "服务器账号", "");
        Config.Set("代理", "服务器密码", "");
        Config.Set("更新", "检测更新", "True");
        Config.Set("更新", "更新间隔", "True");
        Config.Set("更新", "间隔时间", "24");
        Config.Set("截图音效", "自动保存", "True");
        Config.Set("截图音效", "音效路径", "Data\\screenshot.wav");
        Config.Set("截图音效", "粘贴板", "False");
        Config.Set("工具栏", "合并", "False");
        Config.Set("工具栏", "分段", "False");
        Config.Set("工具栏", "分栏", "False");
        Config.Set("工具栏", "拆分", "False");
        Config.Set("工具栏", "检查", "False");
        Config.Set("工具栏", "翻译", "False");
        Config.Set("工具栏", "顶置", "True");
        Config.Set("取色器", "类型", "RGB");
    }

    public static void CheckConfig( )
    {
        if (Config.Get("配置", "接口") == "__ERROR__")
            Config.Set("配置", "接口", "搜狗");
        if (Config.Get("配置", "开机自启") == "__ERROR__")
            Config.Set("配置", "开机自启", "True");
        if (Config.Get("配置", "快速翻译") == "__ERROR__")
            Config.Set("配置", "快速翻译", "True");
        if (Config.Get("配置", "识别弹窗") == "__ERROR__")
            Config.Set("配置", "识别弹窗", "True");
        if (Config.Get("配置", "窗体动画") == "__ERROR__")
            Config.Set("配置", "窗体动画", "窗体");
        if (Config.Get("配置", "记录数目") == "__ERROR__")
            Config.Set("配置", "记录数目", "20");
        if (Config.Get("配置", "自动保存") == "__ERROR__")
            Config.Set("配置", "自动保存", "True");
        if (Config.Get("配置", "翻译接口") == "__ERROR__")
            Config.Set("配置", "翻译接口", "谷歌");
        if (Config.Get("配置", "截图位置") == "__ERROR__")
            Config.Set("配置", "截图位置", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        if (Config.Get("快捷键", "文字识别") == "__ERROR__")
            Config.Set("快捷键", "文字识别", "F4");
        if (Config.Get("快捷键", "翻译文本") == "__ERROR__")
            Config.Set("快捷键", "翻译文本", "F9");
        if (Config.Get("快捷键", "记录界面") == "__ERROR__")
            Config.Set("快捷键", "记录界面", "请按下快捷键");
        if (Config.Get("快捷键", "识别界面") == "__ERROR__")
            Config.Set("快捷键", "识别界面", "请按下快捷键");
        if (Config.Get("密钥_百度", "secret_id") == "__ERROR__")
            Config.Set("密钥_百度", "secret_id", "YsZKG1wha34PlDOPYaIrIIKO");
        if (Config.Get("密钥_百度", "secret_key") == "__ERROR__")
            Config.Set("密钥_百度", "secret_key", "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD");
        if (Config.Get("代理", "代理类型") == "__ERROR__")
            Config.Set("代理", "代理类型", "系统代理");
        if (Config.Get("代理", "服务器") == "__ERROR__")
            Config.Set("代理", "服务器", "");
        if (Config.Get("代理", "端口") == "__ERROR__")
            Config.Set("代理", "端口", "");
        if (Config.Get("代理", "需要密码") == "__ERROR__")
            Config.Set("代理", "需要密码", "False");
        if (Config.Get("代理", "服务器账号") == "__ERROR__")
            Config.Set("代理", "服务器账号", "");
        if (Config.Get("代理", "服务器密码") == "__ERROR__")
            Config.Set("代理", "服务器密码", "");
        if (Config.Get("更新", "检测更新") == "__ERROR__")
            Config.Set("更新", "检测更新", "True");
        if (Config.Get("更新", "更新间隔") == "__ERROR__")
            Config.Set("更新", "更新间隔", "True");
        if (Config.Get("更新", "间隔时间") == "__ERROR__")
            Config.Set("更新", "间隔时间", "24");
        if (Config.Get("截图音效", "自动保存") == "__ERROR__")
            Config.Set("截图音效", "自动保存", "True");
        if (Config.Get("截图音效", "音效路径") == "__ERROR__")
            Config.Set("截图音效", "音效路径", "Data\\screenshot.wav");
        if (Config.Get("截图音效", "粘贴板") == "__ERROR__")
            Config.Set("截图音效", "粘贴板", "False");
        if (Config.Get("工具栏", "合并") == "__ERROR__")
            Config.Set("工具栏", "合并", "False");
        if (Config.Get("工具栏", "拆分") == "__ERROR__")
            Config.Set("工具栏", "拆分", "False");
        if (Config.Get("工具栏", "检查") == "__ERROR__")
            Config.Set("工具栏", "检查", "False");
        if (Config.Get("工具栏", "翻译") == "__ERROR__")
            Config.Set("工具栏", "翻译", "False");
        if (Config.Get("工具栏", "分段") == "__ERROR__")
            Config.Set("工具栏", "分段", "False");
        if (Config.Get("工具栏", "分栏") == "__ERROR__")
            Config.Set("工具栏", "分栏", "False");
        if (Config.Get("工具栏", "顶置") == "__ERROR__")
            Config.Set("工具栏", "顶置", "True");
        if (Config.Get("取色器", "类型") == "__ERROR__")
            Config.Set("取色器", "类型", "RGB");
        if (Config.Get("特殊", "ali_cookie") == "__ERROR__")
            Config.Set("特殊", "ali_cookie", "cna=noXhE38FHGkCAXDve7YaZ8Tn; cnz=noXhE4/VhB8CAbZ773ApeV14; isg=BGJi2c2YTeeP6FG7S_Re8kveu-jEs2bNwToQnKz7jlWAfwL5lEO23eh9q3km9N5l; ");
        if (Config.Get("特殊", "ali_account") == "__ERROR__")
            Config.Set("特殊", "ali_account", "");
        if (Config.Get("特殊", "ali_password") == "__ERROR__")
            Config.Set("特殊", "ali_password", "");
    }
}
