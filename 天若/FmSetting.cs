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
using TrOCR.Helper;
using TrOCR.Properties;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public enum FmSettingTab
{
    常规, 快捷键, 密钥, 代理, 更新, 关于, 赞助, 反馈,
}

public sealed partial class FmSetting : Form
{
    public FmSetting( )
    {
        Font = new Font(Font.Name, 9f / StaticValue.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        InitializeComponent( );
    }

    public FmSettingTab SelectedTab
    {
        get => (FmSettingTab) tabControl.SelectedIndex;
        set => tabControl.SelectedIndex = (int) value;
    }

    public static string GetHtml(string url)
    {
        HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        string text;
        try
        {
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                using StreamReader streamReader = new(httpWebResponse.GetResponseStream( ), Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            httpWebRequest.Abort( );
        }
        catch
        {
            text = "";
        }
        return text;
    }

    public static string PostHtml(string url, string postStr)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(postStr);
        string text = "";
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.Timeout = 6000;
        request.Proxy = null;
        request.ContentType = "application/x-www-form-urlencoded";
        try
        {
            using (Stream reqStream = request.GetRequestStream( ))
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }
            Stream resStream = ((HttpWebResponse) request.GetResponse( )).GetResponseStream( );
            StreamReader reader = new(resStream, Encoding.GetEncoding("utf-8"));
            text = reader.ReadToEnd( );
            resStream.Close( );
            reader.Close( );
            request.Abort( );
        }
        catch { }
        return text;
    }

    public static void SetAutoStart(bool isAuto)
    {
        try
        {
            string text = Application.ExecutablePath.Replace("/", "\\");
            if (isAuto)
            {
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey registryKey = currentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                registryKey.SetValue("tianruoOCR", text);
                registryKey.Close( );
                currentUser.Close( );
            }
            else
            {
                RegistryKey currentUser2 = Registry.CurrentUser;
                RegistryKey registryKey2 = currentUser2.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                registryKey2.DeleteValue("tianruoOCR", false);
                registryKey2.Close( );
                currentUser2.Close( );
            }
        }
        catch (Exception)
        {
            MessageBox.Show("您需要管理员权限修改", "提示");
        }
    }
    public void PlaySong(string file)
    {
        mciSendString("close media", null, 0, IntPtr.Zero);
        mciSendString("open \"" + file + "\" type mpegvideo alias media", null, 0, IntPtr.Zero);
        mciSendString("play media notify", null, 0, Handle);
    }

    public void ReadConfig( )
    {
        string value = Config.Get("配置", "开机自启");
        if (value == "__ERROR__")
        {
            Box开机.Checked = true;
        }
        try
        {
            Box开机.Checked = Convert.ToBoolean(value);
        }
        catch
        {
            Box开机.Checked = true;
        }
        string value2 = Config.Get("配置", "快速翻译");
        if (value2 == "__ERROR__")
        {
            Box翻译.Checked = true;
        }
        try
        {
            Box翻译.Checked = Convert.ToBoolean(value2);
        }
        catch
        {
            Box翻译.Checked = true;
        }
        string value3 = Config.Get("配置", "识别弹窗");
        if (value3 == "__ERROR__")
        {
            Box弹窗.Checked = true;
        }
        try
        {
            Box弹窗.Checked = Convert.ToBoolean(value3);
        }
        catch
        {
            Box弹窗.Checked = true;
        }
        string value4 = Config.Get("配置", "窗体动画");
        Box动画.Text = value4;
        if (value4 == "__ERROR__")
        {
            Box动画.Text = "窗体";
        }
        string value5 = Config.Get("配置", "记录数目");
        Box记录.Value = Convert.ToInt32(value5);
        if (value5 == "__ERROR__")
        {
            Box记录.Value = 20m;
        }
        string value6 = Config.Get("配置", "自动保存");
        if (value6 == "__ERROR__")
        {
            Box保存.Checked = false;
        }
        try
        {
            Box保存.Checked = Convert.ToBoolean(value6);
        }
        catch
        {
            Box保存.Checked = false;
        }
        if (Box保存.Checked)
        {
            Box截图位置.Enabled = true;
            btn_浏览.Enabled = true;
        }
        if (!Box保存.Checked)
        {
            Box截图位置.Enabled = false;
            btn_浏览.Enabled = false;
        }
        string value7 = Config.Get("配置", "截图位置");
        Box截图位置.Text = value7;
        if (value7 == "__ERROR__")
        {
            Box截图位置.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        string value8 = Config.Get("快捷键", "文字识别");
        Box文字识别.Text = value8;
        if (value8 == "__ERROR__")
        {
            Box文字识别.Text = "F4";
        }
        string value9 = Config.Get("快捷键", "翻译文本");
        Box翻译文本.Text = value9;
        if (value9 == "__ERROR__")
        {
            Box翻译文本.Text = "F9";
        }
        string value10 = Config.Get("快捷键", "记录界面");
        Box记录界面.Text = value10;
        if (value10 == "__ERROR__")
        {
            Box记录界面.Text = "请按下快捷键";
        }
        string value11 = Config.Get("快捷键", "识别界面");
        Box识别界面.Text = value11;
        if (value11 == "__ERROR__")
        {
            Box识别界面.Text = "请按下快捷键";
        }
        pictureBox_文字识别.Image = (Box文字识别.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1;
        pictureBox_翻译文本.Image = (Box翻译文本.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1;
        pictureBox_记录界面.Image = (Box记录界面.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1;
        pictureBox_识别界面.Image = (Box识别界面.Text == "请按下快捷键") ? Resources.快捷键_0 : Resources.快捷键_1;
        string value12 = Config.Get("密钥_百度", "secret_id");
        BoxBaiduId.Text = value12;
        if (value12 == "__ERROR__")
        {
            BoxBaiduId.Text = "YsZKG1wha34PlDOPYaIrIIKO";
        }
        string value13 = Config.Get("密钥_百度", "secret_key");
        BoxBaiduKey.Text = value13;
        if (value13 == "__ERROR__")
        {
            BoxBaiduKey.Text = "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD";
        }
        string value14 = Config.Get("代理", "代理类型");
        Box代理.Text = value14;
        if (value14 == "__ERROR__")
        {
            Box代理.Text = "系统代理";
        }
        if (Box代理.Text is "不使用代理" or "系统代理")
        {
            Box代理账号.Enabled = false;
            Box代理密码.Enabled = false;
            Box代理服务器.Enabled = false;
            Box端口.Enabled = false;
            Box服务器.Enabled = false;
        }
        if (Box代理.Text == "自定义代理")
        {
            Box端口.Enabled = true;
            Box服务器.Enabled = true;
        }
        string value15 = Config.Get("代理", "服务器");
        Box服务器.Text = value15;
        if (value15 == "__ERROR__")
        {
            Box服务器.Text = "127.0.0.1";
        }
        string value16 = Config.Get("代理", "端口");
        Box端口.Text = value16;
        if (value16 == "__ERROR__")
        {
            Box端口.Text = "1080";
        }
        string value17 = Config.Get("代理", "需要密码");
        if (value17 == "__ERROR__")
        {
            Box代理服务器.Checked = false;
        }
        try
        {
            Box代理服务器.Checked = Convert.ToBoolean(value17);
        }
        catch
        {
            Box代理服务器.Checked = false;
        }
        string value18 = Config.Get("代理", "服务器账号");
        Box代理账号.Text = value18;
        if (value18 == "__ERROR__")
        {
            Box代理账号.Text = "";
        }
        string value19 = Config.Get("代理", "服务器密码");
        Box代理密码.Text = value19;
        if (value19 == "__ERROR__")
        {
            Box代理密码.Text = "";
        }
        if (Box代理服务器.Checked)
        {
            Box代理账号.Enabled = true;
            Box代理密码.Enabled = true;
        }
        if (!Box代理服务器.Checked)
        {
            Box代理账号.Enabled = false;
            Box代理密码.Enabled = false;
        }
        string value20 = Config.Get("更新", "检测更新");
        if (value20 == "__ERROR__")
        {
            Box检查更新.Checked = false;
        }
        try
        {
            Box检查更新.Checked = Convert.ToBoolean(value20);
        }
        catch
        {
            Box检查更新.Checked = false;
        }
        if (Box检查更新.Checked)
        {
            Box更新间隔.Enabled = true;
        }
        if (!Box检查更新.Checked)
        {
            Box更新间隔.Enabled = false;
            Box间隔时间.Enabled = false;
        }
        string value21 = Config.Get("更新", "更新间隔");
        if (value21 == "__ERROR__")
        {
            Box更新间隔.Checked = false;
        }
        try
        {
            Box更新间隔.Checked = Convert.ToBoolean(value21);
        }
        catch
        {
            Box更新间隔.Checked = false;
        }
        if (Box更新间隔.Checked)
        {
            Box间隔时间.Enabled = true;
        }
        if (!Box更新间隔.Checked)
        {
            Box间隔时间.Enabled = false;
        }
        string value22 = Config.Get("更新", "间隔时间");
        Box间隔时间.Value = Convert.ToInt32(value22);
        if (value5 == "__ERROR__")
        {
            Box间隔时间.Value = 24m;
        }
        string value23 = Config.Get("截图音效", "剪贴板");
        if (value23 == "__ERROR__")
        {
            Box截图剪贴板.Checked = false;
        }
        try
        {
            Box截图剪贴板.Checked = Convert.ToBoolean(value23);
        }
        catch
        {
            Box截图剪贴板.Checked = false;
        }
        string value24 = Config.Get("截图音效", "自动保存");
        if (value24 == "__ERROR__")
        {
            Box截图保存.Checked = true;
        }
        try
        {
            Box截图保存.Checked = Convert.ToBoolean(value24);
        }
        catch
        {
            Box截图保存.Checked = true;
        }
        string value25 = Config.Get("截图音效", "音效路径");
        Box音效路径.Text = value25;
        if (value25 == "__ERROR__")
        {
            Box音效路径.Text = "Data\\screenshot.wav";
        }
        string value26 = Config.Get("取色器", "类型");
        if (value26 == "__ERROR__")
        {
            Box取色.Checked = false;
        }
        if (value26 == "RGB")
        {
            Box取色.Checked = false;
        }
        if (value26 == "HEX")
        {
            Box取色.Checked = true;
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        => (keyData == Keys.Tab && Box文字识别.Focused) || (keyData == Keys.Tab && Box翻译文本.Focused) || (keyData == Keys.Tab && Box记录界面.Focused) || (keyData == Keys.Tab && Box识别界面.Focused);

    private void btn_浏览_Click(object o, EventArgs e)
    {
        FolderBrowserDialog folderBrowserDialog = new( );
        if (folderBrowserDialog.ShowDialog( ) == DialogResult.OK)
        {
            Box截图位置.Text = folderBrowserDialog.SelectedPath;
        }
    }

    private void btn_音效_Click(object o, EventArgs e)
        => PlaySong(Box音效路径.Text);

    private void btn_音效路径_Click(object o, EventArgs e)
    {
        OpenFileDialog openFileDialog = new( )
        {
            Title = "请选择音效文件",
            Filter = "All files（*.*）|*.*|All files(*.*)|*.* ",
            RestoreDirectory = true
        };
        if (openFileDialog.ShowDialog( ) == DialogResult.OK)
        {
            Box音效路径.Text = Path.GetFullPath(openFileDialog.FileName);
        }
    }

    private void cbBox_保存_CheckedChanged(object o, EventArgs e)
    {
        if (Box保存.Checked)
        {
            Box截图位置.Enabled = true;
            btn_浏览.Enabled = true;
        }
        if (!Box保存.Checked)
        {
            Box截图位置.Enabled = false;
            btn_浏览.Enabled = false;
        }
    }

    private void cbBox_弹窗_CheckedChanged(object o, EventArgs e)
    {
    }

    private void cbBox_翻译_CheckedChanged(object o, EventArgs e)
    {
    }

    private void cbBox_开机_CheckedChanged(object o, EventArgs e)
        => SetAutoStart(Box开机.Checked);

    private void chbox_copy_CheckedChanged(object o, EventArgs e)
    {
    }

    private void chbox_save_CheckedChanged(object o, EventArgs e)
    {
    }

    private void chbox_代理服务器_CheckedChanged(object o, EventArgs e)
    {
        if (Box代理服务器.Checked)
        {
            Box代理账号.Enabled = true;
            Box代理密码.Enabled = true;
        }
        if (!Box代理服务器.Checked)
        {
            Box代理账号.Enabled = false;
            Box代理密码.Enabled = false;
        }
    }

    private void chbox_取色_CheckedChanged(object o, EventArgs e)
    {
    }

    private void check_检查更新_CheckedChanged(object o, EventArgs e)
    {
        if (Box检查更新.Checked)
        {
            Box更新间隔.Enabled = true;
            Box更新间隔.Checked = true;
            Box间隔时间.Enabled = true;
        }
        if (!Box检查更新.Checked)
        {
            Box更新间隔.Checked = false;
            Box更新间隔.Enabled = false;
            Box间隔时间.Enabled = false;
        }
    }

    private void checkBox_更新间隔_CheckedChanged(object o, EventArgs e)
    {
        if (Box更新间隔.Checked)
        {
            Box间隔时间.Enabled = true;
        }
        if (!Box更新间隔.Checked)
        {
            Box间隔时间.Enabled = false;
        }
    }

    private void combox_代理_SelectedIndexChanged(object o, EventArgs e)
    {
        if (Box代理.Text is "不使用代理" or "系统代理")
        {
            Box代理账号.Enabled = false;
            Box代理密码.Enabled = false;
            Box代理服务器.Enabled = false;
            Box端口.Enabled = false;
            Box代理服务器.Checked = false;
            Box服务器.Enabled = false;
            Box服务器.Text = "";
            Box端口.Text = "";
            Box服务器.Text = "";
            Box代理账号.Text = "";
            Box代理密码.Text = "";
        }
        if (Box代理.Text == "自定义代理")
        {
            Box端口.Enabled = true;
            Box服务器.Enabled = true;
            Box代理服务器.Enabled = true;
        }
    }

    private void FormClosed(object o, FormClosedEventArgs e)
    {
        Config.Set("配置", "开机自启", Box开机.Checked.ToString( ));
        Config.Set("配置", "快速翻译", Box翻译.Checked.ToString( ));
        Config.Set("配置", "识别弹窗", Box弹窗.Checked.ToString( ));
        Config.Set("配置", "窗体动画", Box动画.Text);
        Config.Set("配置", "记录数目", Box记录.Text);
        Config.Set("配置", "自动保存", Box保存.Checked.ToString( ));
        Config.Set("配置", "截图位置", Box截图位置.Text);
        Config.Set("快捷键", "文字识别", Box文字识别.Text);
        Config.Set("快捷键", "翻译文本", Box翻译文本.Text);
        Config.Set("快捷键", "记录界面", Box记录界面.Text);
        Config.Set("快捷键", "识别界面", Box识别界面.Text);
        Config.Set("密钥_百度", "secret_id", BoxBaiduId.Text);
        Config.Set("密钥_百度", "secret_key", BoxBaiduKey.Text);
        Config.Set("代理", "代理类型", Box代理.Text);
        Config.Set("代理", "服务器", Box服务器.Text);
        Config.Set("代理", "端口", Box端口.Text);
        Config.Set("代理", "需要密码", Box代理服务器.Checked.ToString( ));
        Config.Set("代理", "服务器账号", Box代理账号.Text);
        Config.Set("代理", "服务器密码", Box代理密码.Text);
        Config.Set("更新", "检测更新", Box检查更新.Checked.ToString( ));
        Config.Set("更新", "更新间隔", Box更新间隔.Checked.ToString( ));
        Config.Set("更新", "间隔时间", Box间隔时间.Value.ToString( ));
        Config.Set("截图音效", "自动保存", Box截图保存.Checked.ToString( ));
        Config.Set("截图音效", "音效路径", Box音效路径.Text);
        Config.Set("截图音效", "剪贴板", Box截图剪贴板.Checked.ToString( ));
        if (!Box取色.Checked)
        {
            Config.Set("取色器", "类型", "RGB");
        }
        if (Box取色.Checked)
        {
            Config.Set("取色器", "类型", "HEX");
        }
        DialogResult = DialogResult.OK;
    }

    private void FormLoaded(object o, EventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(FmMain));
        Icon = (Icon) componentResourceManager.GetObject("minico.Icon");
        NumericUpDown numericUpDown = Box记录;
        int[] array = new int[4];
        array[0] = 99;
        numericUpDown.Maximum = new decimal(array);
        NumericUpDown numericUpDown2 = Box记录;
        int[] array2 = new int[4];
        array2[0] = 1;
        numericUpDown2.Minimum = new decimal(array2);
        NumericUpDown numericUpDown3 = Box记录;
        int[] array3 = new int[4];
        array3[0] = 1;
        numericUpDown3.Value = new decimal(array3);
        NumericUpDown numericUpDown4 = Box间隔时间;
        int[] array4 = new int[4];
        array4[0] = 24;
        numericUpDown4.Maximum = new decimal(array4);
        NumericUpDown numericUpDown5 = Box间隔时间;
        int[] array5 = new int[4];
        array5[0] = 1;
        numericUpDown5.Minimum = new decimal(array5);
        NumericUpDown numericUpDown6 = Box间隔时间;
        int[] array6 = new int[4];
        array6[0] = 1;
        numericUpDown6.Value = new decimal(array6);
        tabControl.Height = (int) (400.0 * Helper.System.DpiFactor);
        Height = tabControl.Height + 50;
        ReadConfig( );
        Box代理服务器.CheckedChanged += chbox_代理服务器_CheckedChanged;
        更新Button_check.Click += 更新Check;
        Text更新日期.Text = "更新时间：" + StaticValue.DateNow;
        Text版本号.Text = "当前版本：" + StaticValue.Version;
        Box更新说明.Text = (string) componentResourceManager.GetObject("更新说明");
        Box更新说明.ReadOnly = true;
        Box更新说明.WordWrap = true;
        Box更新说明.ScrollBars = ScrollBars.Vertical;
    }

    private void PictureHelpClick(object o, EventArgs e)
        => new FmHelp( ).Show( );

    private void TabControlSelectionChanged(object o, EventArgs e)
    {
        double height = 0;
        switch (SelectedTab)
        {
            case FmSettingTab.常规: height = 400.0; break;
            case FmSettingTab.快捷键: height = 280.0; break;
            case FmSettingTab.密钥: height = 240.0; break;
            case FmSettingTab.代理: height = 300.0; break;
            case FmSettingTab.更新: height = 185.0; break;
            case FmSettingTab.关于: height = 400.0; break;
            case FmSettingTab.赞助: height = 275.0; break;
            case FmSettingTab.反馈: height = 250.0; break;
        }
        tabControl.Height = (int) (height * Helper.System.DpiFactor);
        Height = tabControl.Height + 50;
    }

    private void TextBoxKeyDown(object o, KeyEventArgs e)
        => e.SuppressKeyPress = true;

    private void TextBoxKeyUp(object o, KeyEventArgs e)
    {
        TextBox textBox = o as TextBox;
        Regex regex = new("[一-龥]+");
        string text = "";
        foreach (object obj in regex.Matches(textBox.Name))
        {
            text = ((Match) obj).ToString( );
        }
        string text2 = "pictureBox_" + text;
        PictureBox pictureBox = (PictureBox) Controls.Find(text2, true)[0];
        new ComponentResourceManager(typeof(FmSetting));
        if (e.KeyData == Keys.Back)
        {
            textBox.Text = "请按下快捷键";
            pictureBox.Image = Resources.快捷键_0;
            if (textBox.Name.Contains("文字识别"))
            {
                Config.Set("快捷键", "文字识别", Box文字识别.Text);
            }
            if (textBox.Name.Contains("翻译文本"))
            {
                Config.Set("快捷键", "翻译文本", Box翻译文本.Text);
            }
            if (textBox.Name.Contains("记录界面"))
            {
                Config.Set("快捷键", "记录界面", Box记录界面.Text);
            }
            if (textBox.Name.Contains("识别界面"))
            {
                Config.Set("快捷键", "识别界面", Box识别界面.Text);
                return;
            }
        }
        else if (e.KeyValue is not 16 and not 17 and not 18)
        {
            string[] array = e.KeyData.ToString( ).Replace(" ", "").Replace("Control", "Ctrl")
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
                    Config.Set("快捷键", "文字识别", Box文字识别.Text);
                }
                if (textBox.Name.Contains("翻译文本"))
                {
                    Config.Set("快捷键", "翻译文本", Box翻译文本.Text);
                }
                if (textBox.Name.Contains("记录界面"))
                {
                    Config.Set("快捷键", "记录界面", Box记录界面.Text);
                }
                if (textBox.Name.Contains("识别界面"))
                {
                    Config.Set("快捷键", "识别界面", Box识别界面.Text);
                }
            }
        }
    }

    private void 百度_btn_Click(object o, EventArgs e)
    {
        if (!string.IsNullOrEmpty(GetHtml(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + BoxBaiduId.Text + "&client_secret=" + BoxBaiduKey.Text))))
        {
            MessageBox.Show("密钥正确!", "提醒");
            return;
        }
        MessageBox.Show("请确保密钥正确!", "提醒");
    }

    private void 百度申请Click(object o, EventArgs e)
        => Process.Start("https://console.bce.baidu.com/ai/");
    private void 常规Click(object o, EventArgs e)
    {
        Box开机.Checked = true;
        Box翻译.Checked = true;
        Box弹窗.Checked = true;
        Box动画.SelectedIndex = 0;
        Box记录.Value = 20m;
        Box保存.Checked = true;
        Box截图位置.Enabled = true;
        Box截图位置.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        btn_浏览.Enabled = true;
        Box截图保存.Checked = true;
        Box音效路径.Text = "Data\\screenshot.wav";
        Box截图剪贴板.Checked = false;
        Box取色.Checked = false;
    }

    private void 代理Click(object o, EventArgs e)
    {
        Box代理.Text = "系统代理";
        Box代理账号.Enabled = false;
        Box代理密码.Enabled = false;
        Box代理服务器.Enabled = false;
        Box端口.Enabled = false;
        Box服务器.Enabled = false;
    }

    private void 反馈Click(object o, EventArgs e)
        => new Thread(new ThreadStart(反馈Send)).Start( );

    private void 反馈Send( )
    {
        if (!string.IsNullOrEmpty(Box问题反馈.Text))
        {
            string text = "sm=%E5%A4%A9%E8%8B%A5OCR%E6%96%87%E5%AD%97%E8%AF%86%E5%88%AB" + StaticValue.Version + "&nr=";
            PostHtml("http://cd.ys168.com/f_ht/ajcx/lyd.aspx?cz=lytj&pdgk=1&pdgly=0&pdzd=0&tou=1&yzm=undefined&_dlmc=tianruoyouxin&_dlmm=", text + HttpUtility.UrlEncode(Box问题反馈.Text));
            Box问题反馈.Text = "";
            FmFlags.Display("感谢您的反馈！");
            return;
        }
        FmFlags.Display("反馈文本不能为空");
    }

    private void 更新Check(object o, EventArgs e)
        => new Thread(new ThreadStart(Helper.Update.CheckUpdate)).Start( );

    private void 更新Click(object o, EventArgs e)
    {
        Box间隔时间.Value = 24m;
        Box检查更新.Checked = true;
        Box更新间隔.Checked = true;
    }

    private void 快捷键Click(object o, EventArgs e)
    {
        new ComponentResourceManager(typeof(FmSetting));
        Box文字识别.Text = "F4";
        pictureBox_文字识别.Image = Resources.快捷键_1;
        Box翻译文本.Text = "F9";
        pictureBox_翻译文本.Image = Resources.快捷键_1;
        Box记录界面.Text = "请按下快捷键";
        pictureBox_记录界面.Image = Resources.快捷键_0;
        Box识别界面.Text = "请按下快捷键";
        pictureBox_识别界面.Image = Resources.快捷键_0;
    }

    private void 密钥Click(object o, EventArgs e)
    {
        BoxBaiduId.Text = "YsZKG1wha34PlDOPYaIrIIKO";
        BoxBaiduKey.Text = "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD";
    }
}
