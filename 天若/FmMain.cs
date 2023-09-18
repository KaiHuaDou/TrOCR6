using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
using TrOCR.Helper;
using TrOCR.Ocr;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class FmMain : Form
{
    public FmMain( )
    {
        set_merge = false;
        set_split = false;
        set_split = false;
        StaticValue.截图排斥 = false;
        pinyin_flag = false;
        tranclick = false;
        are = new AutoResetEvent(false);
        imagelist = new List<Image>( );
        StaticValue.v_notecount = Convert.ToInt32(IniHelp.GetValue("配置", "记录数目"));
        baidu_flags = "";
        esc = "";
        voice_count = 0;
        fmnote = new Fmnote( );
        fmflags = new FmFlags( );
        pubnote = new string[StaticValue.v_notecount];
        for (int i = 0; i < StaticValue.v_notecount; i++)
        {
            pubnote[i] = "";
        }
        StaticValue.v_note = pubnote;
        StaticValue.mainhandle = Handle;
        Font = new Font(Font.Name, 9f / StaticValue.Dpifactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        googleTranslate_txt = "";
        num_ok = 0;
        F_factor = Program.factor;
        components = null;
        InitializeComponent( );
        nextClipboardViewer = (IntPtr) SetClipboardViewer((int) Handle);
        InitMinimize( );
        ReadIniFile( );
        WindowState = FormWindowState.Minimized;
        Visible = false;
        SplitedText = "";
        MinimumSize = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
        speak_copy = false;
        OcrForeach("");
    }

    private void Load_Click(object o, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        Visible = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 953)
        {
            speaking = false;
        }
        if (m.Msg == 274 && (int) m.WParam == 61536)
        {
            WindowState = FormWindowState.Minimized;
            Visible = false;
            return;
        }
        if (m.Msg == 600 && (int) m.WParam == 725)
        {
            if (IniHelp.GetValue("工具栏", "顶置") == "True")
            {
                TopMost = true;
                return;
            }
            TopMost = false;
            return;
        }
        else
        {
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 530 && RichBoxBody.Text != null)
            {
                p_note(RichBoxBody.Text);
                StaticValue.v_note = pubnote;
                if (fmnote.Created)
                {
                    fmnote.TextNote = "";
                }
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 520)
            {
                fmnote.Show( );
                fmnote.Focus( );
                fmnote.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - fmnote.Width, Screen.PrimaryScreen.WorkingArea.Height - fmnote.Height);
                fmnote.WindowState = FormWindowState.Normal;
                return;
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 580)
            {
                UnregisterHotKey(Handle, 205);
                change_QQ_screenshot = false;
                FormBorderStyle = FormBorderStyle.None;
                Hide( );
                form_width = transtalate_fla == "开启" ? Width / 2 : Width;
                form_height = Height;
                minico.Visible = false;
                minico.Visible = true;
                menu.Close( );
                menu_copy.Close( );
                auto_fla = "开启";
                SplitedText = "";
                RichBoxBody.Text = "***该区域未发现文本***";
                RichBoxBody_T.Text = "";
                typeset_txt = "";
                transtalate_fla = "关闭";
                Trans_close.PerformClick( );
                Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
                FormBorderStyle = FormBorderStyle.Sizable;
                StaticValue.截图排斥 = true;
                image_screen = StaticValue.image_OCR;
                if (IniHelp.GetValue("工具栏", "分栏") == "True")
                {
                    minico.Visible = true;
                    thread = new Thread(new ThreadStart(ShowLoading));
                    thread.Start( );
                    ts = new TimeSpan(DateTime.Now.Ticks);
                    Image image = image_screen;
                    Bitmap bitmap = new(image.Width, image.Height);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                    graphics.Save( );
                    graphics.Dispose( );
                    image_ori = bitmap;
                    ((Bitmap) FindBundingBox_fences((Bitmap) image)).Save("Data\\分栏预览图.jpg");
                }
                else
                {
                    minico.Visible = true;
                    thread = new Thread(new ThreadStart(ShowLoading));
                    thread.Start( );
                    ts = new TimeSpan(DateTime.Now.Ticks);
                    Messageload messageload = new( );
                    messageload.ShowDialog( );
                    if (messageload.DialogResult == DialogResult.OK)
                    {
                        esc_thread = new Thread(new ThreadStart(Main_OCR_Thread));
                        esc_thread.Start( );
                    }
                }
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 590 && speak_copyb == "朗读")
            {
                TTS( );
                return;
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 511)
            {
                base.MinimumSize = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
                transtalate_fla = "关闭";
                RichBoxBody.Dock = DockStyle.Fill;
                RichBoxBody_T.Visible = false;
                PictureBox1.Visible = false;
                RichBoxBody_T.Text = "";
                if (WindowState == FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Normal;
                }
                Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 512)
            {
                transtalate_Click( );
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 518)
            {
                if (ActiveControl.Name == "htmlTextBoxBody")
                {
                    htmltxt = RichBoxBody.Text;
                }
                if (ActiveControl.Name == "rich_trans")
                {
                    htmltxt = RichBoxBody_T.Text;
                }
                if (string.IsNullOrEmpty(htmltxt))
                {
                    return;
                }
                TTS( );
            }
            if (m.Msg == 161)
            {
                SetForegroundWindow(Handle);
                base.WndProc(ref m);
                return;
            }
            if (m.Msg != 163)
            {
                if (m.Msg == 786 && m.WParam.ToInt32( ) == 222)
                {
                    try
                    {
                        StaticValue.截图排斥 = false;
                        esc = "退出";
                        fmloading.fml_close = "窗体已关闭";
                        esc_thread.Abort( );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    FormBorderStyle = FormBorderStyle.Sizable;
                    Visible = true;
                    Show( );
                    WindowState = FormWindowState.Normal;
                    if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value = IniHelp.GetValue("快捷键", "翻译文本");
                        string text = "None";
                        string text2 = "F9";
                        SetHotkey(text, text2, value, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                }
                if (m.Msg == 786 && m.WParam.ToInt32( ) == 200)
                {
                    UnregisterHotKey(Handle, 205);
                    menu.Hide( );
                    RichBoxBody.Hide = "";
                    RichBoxBody_T.Hide = "";
                    Main_OCR_Quickscreenshots( );
                }
                if (m.Msg == 786 && m.WParam.ToInt32( ) == 206)
                {
                    if (!fmnote.Visible || base.Focused)
                    {
                        fmnote.Show( );
                        fmnote.WindowState = FormWindowState.Normal;
                        fmnote.Visible = true;
                    }
                    else
                    {
                        fmnote.Hide( );
                        fmnote.WindowState = FormWindowState.Minimized;
                        fmnote.Visible = false;
                    }
                }
                if (m.Msg == 786 && m.WParam.ToInt32( ) == 235)
                {
                    if (!Visible)
                    {
                        TopMost = true;
                        Show( );
                        WindowState = FormWindowState.Normal;
                        Visible = true;
                        Thread.Sleep(100);
                        if (IniHelp.GetValue("工具栏", "顶置") == "False")
                        {
                            TopMost = false;
                            return;
                        }
                    }
                    else
                    {
                        Hide( );
                        Visible = false;
                    }
                }
                if (m.Msg == 786 && m.WParam.ToInt32( ) == 205)
                {
                    翻译文本( );
                }
                base.WndProc(ref m);
                return;
            }
            if (transtalate_fla == "开启")
            {
                WindowState = FormWindowState.Normal;
                Size = new Size((int) font_base.Width * 23 * 2, (int) font_base.Height * 24);
                Location = (Point) new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10 * 2, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
                return;
            }
            WindowState = FormWindowState.Normal;
            Location = (Point) new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
            Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
            return;
        }
    }

    private void Form1_FormClosing(object o, FormClosedEventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        Visible = false;
    }

    private void InitMinimize( )
    {
        try
        {
            MenuItem TrayShowMenu = new( ) { Text = "显示" };
            TrayShowMenu.Click += TrayShowClick;
            MenuItem TraySettingMenu = new( ) { Text = "设置" };
            TraySettingMenu.Click += TraySettingClick;
            MenuItem TrayUpdateMenu = new( ) { Text = "更新" };
            TrayUpdateMenu.Click += TrayUpdateClick;
            MenuItem TrayHelpMenu = new( ) { Text = "帮助" };
            TrayHelpMenu.Click += TrayHelpClick;
            MenuItem TrayExitMenu = new( ) { Text = "退出" };
            TrayExitMenu.Click += TrayExitClick;
            minico.ContextMenu = new ContextMenu(new MenuItem[]
            {
                TrayShowMenu, TraySettingMenu, TrayUpdateMenu, TrayHelpMenu, TrayExitMenu
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("InitMinimize()" + ex.Message);
        }
    }

    private void TrayShowClick(object o, EventArgs e)
    {
        Show( );
        Activate( );
        Visible = true;
        WindowState = FormWindowState.Normal;
        if (IniHelp.GetValue("工具栏", "顶置") == "True")
        {
            TopMost = true;
            return;
        }
        TopMost = false;
    }

    private void TrayExitClick(object o, EventArgs e)
    {
        minico.Dispose( );
        SaveIniFile( );
        Process.GetCurrentProcess( ).Kill( );
    }

    private void Main_copy_Click(object o, EventArgs e)
    {
        RichBoxBody.Focus( );
        RichBoxBody.richTextBox1.Copy( );
    }

    private static string PunctuationEnZh(string text)
    {
        char[] array = text.ToCharArray( );
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

    private void MainSelectAllClick(object o, EventArgs e)
    {
        RichBoxBody.Focus( );
        RichBoxBody.richTextBox1.SelectAll( );
    }

    private void MainPasteClick(object o, EventArgs e)
    {
        RichBoxBody.Focus( );
        RichBoxBody.richTextBox1.Paste( );
    }

    private void SplitClick(object o, EventArgs e)
        => RichBoxBody.Text = SplitedText;

    private static byte[] CopyBytes(byte[] a, byte[] b)
    {
        byte[] array = new byte[a.Length + b.Length];
        a.CopyTo(array, 0);
        b.CopyTo(array, a.Length);
        return array;
    }

    private void OcrSogou( )
    {
        try
        {
            SplitedText = "";
            Image image = image_screen;
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
            Bitmap bitmap = new(i, j);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(image, 0, 0, i, j);
            graphics.Save( );
            graphics.Dispose( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(OCR_sougou_SogouOCR(bitmap)))["result"].ToString( ));
            bitmap.Dispose( );
            checked_txt(jarray, 2, "content");
        }
        catch
        {
            RichBoxBody.Text = "***该区域未发现文本***";
            if (esc == "退出")
                esc = "";
        }
    }

    private static byte[] ImageToBytes(Image image)
    {
        MemoryStream stream = new( );
        image.Save(stream, ImageFormat.Jpeg);
        byte[] array = new byte[stream.Length];
        stream.Position = 0L;
        stream.Read(array, 0, array.Length);
        stream.Close( );
        return array;
    }

    private void OcrTencent( )
    {
        try
        {
            SplitedText = "";
            string text = "------WebKitFormBoundaryRDEqU0w702X9cWPJ";
            Image image = image_screen;
            if (image.Width > 90 && image.Height < 90)
            {
                Bitmap bitmap = new(image.Width, 300);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, 5, 0, image.Width, image.Height);
                graphics.Save( );
                graphics.Dispose( );
                image = new Bitmap(bitmap);
            }
            else if (image.Width <= 90 && image.Height >= 90)
            {
                Bitmap bitmap2 = new(300, image.Height);
                Graphics graphics2 = Graphics.FromImage(bitmap2);
                graphics2.DrawImage(image, 0, 5, image.Width, image.Height);
                graphics2.Save( );
                graphics2.Dispose( );
                image = new Bitmap(bitmap2);
            }
            else if (image.Width < 90 && image.Height < 90)
            {
                Bitmap bitmap3 = new(300, 300);
                Graphics graphics3 = Graphics.FromImage(bitmap3);
                graphics3.DrawImage(image, 5, 5, image.Width, image.Height);
                graphics3.Save( );
                graphics3.Dispose( );
                image = new Bitmap(bitmap3);
            }
            else
            {
                image = image_screen;
            }
            byte[] array = OCR_ImgToByte(image);
            string text2 = text + "\r\nContent-Disposition: form-data; name=\"image_file\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = "\r\n" + text + "--\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(text2);
            byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
            byte[] array2 = Mergebyte(bytes, array, bytes2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://ai.qq.com/cgi-bin/appdemo_generalocr");
            httpWebRequest.Method = "POST";
            httpWebRequest.Referer = "http://ai.qq.com/product/ocr.shtml";
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
            httpWebRequest.Timeout = 8000;
            httpWebRequest.ReadWriteTimeout = 2000;
            byte[] array3 = array2;
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(array3, 0, array2.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text4))["data"]["item_list"].ToString( ));
            checked_txt(jarray, 1, "itemstring");
        }
        catch
        {
            RichBoxBody.Text = "***该区域未发现文本***";
            if (esc == "退出")
                esc = "";
        }
    }

    private void _bak_OcrBaidu( )
    {
        SplitedText = "";
        try
        {
            string text = "CHN_ENG";
            SplitedText = "";
            Image image = image_screen;
            MemoryStream memoryStream = new( );
            image.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            switch (interface_flag)
            {
                case "中英": text = "CHN_ENG"; break;
                case "日语": text = "JAP"; break;
                case "韩语": text = "KOR"; break;
            }
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string text5 = "";
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                char[] array2 = jobject["words"].ToString( ).ToCharArray( );
                if (!char.IsPunctuation(array2[array2.Length - 1]))
                {
                    if (!TextUtils.ContainsZh(jobject["words"].ToString( )))
                    {
                        text5 = text5 + jobject["words"].ToString( ).Trim( ) + " ";
                    }
                    else
                    {
                        text5 += jobject["words"].ToString( );
                    }
                }
                else if (own_punctuation(array2[array2.Length - 1].ToString( )))
                {
                    if (!TextUtils.ContainsZh(jobject["words"].ToString( )))
                    {
                        text5 = text5 + jobject["words"].ToString( ).Trim( ) + " ";
                    }
                    else
                    {
                        text5 += jobject["words"].ToString( );
                    }
                }
                else
                {
                    text5 = text5 + jobject["words"] + "\r\n";
                }
                text4 = text4 + jobject["words"] + "\r\n";
            }
            SplitedText = text4;
            typeset_txt = text5;
        }
        catch
        {
            RichBoxBody.Text = "***该区域未发现文本***";
            if (esc == "退出")
            {
                esc = "";
            }
        }
    }

    private void OcrSogouClick(object o, EventArgs e)
        => OcrForeach("搜狗");

    private void OcrTencentClick(object o, EventArgs e)
        => OcrForeach("腾讯");

    private void OCR_baidu_Click(object o, EventArgs e)
    {
    }

    private void OcrYoudao( )
    {
        try
        {
            SplitedText = "";
            Image image = image_screen;
            if (image.Width > 90 && image.Height < 90)
            {
                Bitmap bitmap = new(image.Width, 200);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, 5, 0, image.Width, image.Height);
                graphics.Save( );
                graphics.Dispose( );
                image = new Bitmap(bitmap);
            }
            else if (image.Width <= 90 && image.Height >= 90)
            {
                Bitmap bitmap2 = new(200, image.Height);
                Graphics graphics2 = Graphics.FromImage(bitmap2);
                graphics2.DrawImage(image, 0, 5, image.Width, image.Height);
                graphics2.Save( );
                graphics2.Dispose( );
                image = new Bitmap(bitmap2);
            }
            else if (image.Width < 90 && image.Height < 90)
            {
                Bitmap bitmap3 = new(200, 200);
                Graphics graphics3 = Graphics.FromImage(bitmap3);
                graphics3.DrawImage(image, 5, 5, image.Width, image.Height);
                graphics3.Save( );
                graphics3.Dispose( );
                image = new Bitmap(bitmap3);
            }
            else
            {
                image = image_screen;
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
            Bitmap bitmap4 = new(i, j);
            Graphics graphics4 = Graphics.FromImage(bitmap4);
            graphics4.DrawImage(image, 0, 0, i, j);
            graphics4.Save( );
            graphics4.Dispose( );
            image = new Bitmap(bitmap4);
            byte[] array = OCR_ImgToByte(image);
            string text = "imgBase=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&lang=auto&company=";
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://aidemo.youdao.com/ocrapi1");
            httpWebRequest.Method = "POST";
            httpWebRequest.Referer = "http://aidemo.youdao.com/ocrdemo";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Timeout = 8000;
            httpWebRequest.ReadWriteTimeout = 2000;
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text2 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text2))["lines"].ToString( ));
            checked_txt(jarray, 1, "words");
            image.Dispose( );
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private void OCR_youdao_Click(object o, EventArgs e) => OcrForeach("有道");

    private void change_Chinese_Click(object o, EventArgs e)
    {
        language = "中文标点";
        if (!string.IsNullOrEmpty(typeset_txt))
        {
            RichBoxBody.Text = punctuation_en_ch_x(RichBoxBody.Text);
            RichBoxBody.Text = TextUtils.PunctuationQuotation(RichBoxBody.Text);
        }
    }

    private void ChangeEnglishClick(object o, EventArgs e)
    {
        language = "英文标点";
        if (!string.IsNullOrEmpty(typeset_txt))
        {
            RichBoxBody.Text = PunctuationChEn(RichBoxBody.Text);
        }
    }

    private static string PunctuationChEn(string text)
    {
        char[] array = text.ToCharArray( );
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

    private void SaveIniFile( )
        => IniHelp.SetValue("配置", "接口", interface_flag);

    private void ReadIniFile( )
    {
        Proxy_flag = IniHelp.GetValue("代理", "代理类型");
        Proxy_url = IniHelp.GetValue("代理", "服务器");
        Proxy_port = IniHelp.GetValue("代理", "端口");
        Proxy_name = IniHelp.GetValue("代理", "服务器账号");
        Proxy_password = IniHelp.GetValue("代理", "服务器密码");
        if (Proxy_flag == "不使用代理")
        {
            WebRequest.DefaultWebProxy = null;
        }
        if (Proxy_flag == "系统代理")
        {
            WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy( );
        }
        if (Proxy_flag == "自定义代理")
        {
            try
            {
                WebProxy webProxy = new(Proxy_url, Convert.ToInt32(Proxy_port));
                if (!string.IsNullOrEmpty(Proxy_name) && !string.IsNullOrEmpty(Proxy_password))
                {
                    ICredentials credentials = new NetworkCredential(Proxy_name, Proxy_password);
                    webProxy.Credentials = credentials;
                }
                WebRequest.DefaultWebProxy = webProxy;
            }
            catch
            {
                MessageBox.Show("请检查代理设置！");
            }
        }
        interface_flag = IniHelp.GetValue("配置", "接口");
        if (interface_flag == "发生错误")
        {
            IniHelp.SetValue("配置", "接口", "搜狗");
            OcrForeach("搜狗");
        }
        else
        {
            OcrForeach(interface_flag);
        }
        string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
        if (IniHelp.GetValue("快捷键", "文字识别") != "请按下快捷键")
        {
            string value = IniHelp.GetValue("快捷键", "文字识别");
            string text2 = "None";
            string text3 = "F4";
            SetHotkey(text2, text3, value, 200);
        }
        if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
        {
            string value2 = IniHelp.GetValue("快捷键", "翻译文本");
            string text4 = "None";
            string text5 = "F7";
            SetHotkey(text4, text5, value2, 205);
        }
        if (IniHelp.GetValue("快捷键", "记录界面") != "请按下快捷键")
        {
            string value3 = IniHelp.GetValue("快捷键", "记录界面");
            string text6 = "None";
            string text7 = "F8";
            SetHotkey(text6, text7, value3, 206);
        }
        if (IniHelp.GetValue("快捷键", "识别界面") != "请按下快捷键")
        {
            string value4 = IniHelp.GetValue("快捷键", "识别界面");
            string text8 = "None";
            string text9 = "F11";
            SetHotkey(text8, text9, value4, 235);
        }
        StaticValue.baiduAPI_ID = IniHelp.GetValue("密钥_百度", "secret_id", text);
        if (IniHelp.GetValue("密钥_百度", "secret_id", text) == "发生错误")
        {
            StaticValue.baiduAPI_ID = "请输入secret_id";
        }
        StaticValue.baiduAPI_key = IniHelp.GetValue("密钥_百度", "secret_key", text);
        if (IniHelp.GetValue("密钥_百度", "secret_key", text) == "发生错误")
        {
            StaticValue.baiduAPI_key = "请输入secret_key";
        }
    }

    private static string check_ch_en(string text)
    {
        char[] array = text.ToCharArray( );
        for (int i = 0; i < array.Length; i++)
        {
            int num = "：".IndexOf(array[i]);
            if (num != -1 && i - 1 >= 0 && i + 1 < array.Length && TextUtils.ContainEn(array[i - 1].ToString( )) && TextUtils.ContainEn(array[i + 1].ToString( )))
            {
                array[i] = ":"[num];
            }
            if (num != -1 && i - 1 >= 0 && i + 1 < array.Length && TextUtils.ContainEn(array[i - 1].ToString( )) && contain_punctuation(array[i + 1].ToString( )))
            {
                array[i] = ":"[num];
            }
        }
        return new string(array);
    }

    private void TraySettingClick(object o, EventArgs e)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        UnregisterHotKey(Handle, 200);
        UnregisterHotKey(Handle, 205);
        UnregisterHotKey(Handle, 206);
        UnregisterHotKey(Handle, 235);
        WindowState = FormWindowState.Minimized;
        FmSetting fmSetting = new( )
        {
            TopMost = true
        };
        fmSetting.ShowDialog( );
        if (fmSetting.DialogResult == DialogResult.OK)
        {
            StaticValue.v_notecount = Convert.ToInt32(IniHelp.GetValue("配置", "记录数目"));
            pubnote = new string[StaticValue.v_notecount];
            for (int i = 0; i < StaticValue.v_notecount; i++)
            {
                pubnote[i] = "";
            }
            StaticValue.v_note = pubnote;
            fmnote.TextNoteChange = "";
            fmnote.Location = new Point(Screen.AllScreens[0].WorkingArea.Width - fmnote.Width, Screen.AllScreens[0].WorkingArea.Height - fmnote.Height);
            if (IniHelp.GetValue("快捷键", "文字识别") != "请按下快捷键")
            {
                string value = IniHelp.GetValue("快捷键", "文字识别");
                string text2 = "None";
                string text3 = "F4";
                SetHotkey(text2, text3, value, 200);
            }
            if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
            {
                string value2 = IniHelp.GetValue("快捷键", "翻译文本");
                string text4 = "None";
                string text5 = "F9";
                SetHotkey(text4, text5, value2, 205);
            }
            if (IniHelp.GetValue("快捷键", "记录界面") != "请按下快捷键")
            {
                string value3 = IniHelp.GetValue("快捷键", "记录界面");
                string text6 = "None";
                string text7 = "F8";
                SetHotkey(text6, text7, value3, 206);
            }
            if (IniHelp.GetValue("快捷键", "识别界面") != "请按下快捷键")
            {
                string value4 = IniHelp.GetValue("快捷键", "识别界面");
                string text8 = "None";
                string text9 = "F11";
                SetHotkey(text8, text9, value4, 235);
            }
            Proxy_flag = IniHelp.GetValue("代理", "代理类型");
            Proxy_url = IniHelp.GetValue("代理", "服务器");
            Proxy_port = IniHelp.GetValue("代理", "端口");
            Proxy_name = IniHelp.GetValue("代理", "服务器账号");
            Proxy_password = IniHelp.GetValue("代理", "服务器密码");
            StaticValue.baiduAPI_ID = IniHelp.GetValue("密钥_百度", "secret_id");
            StaticValue.baiduAPI_key = IniHelp.GetValue("密钥_百度", "secret_key");
            if (Proxy_flag == "不使用代理")
            {
                WebRequest.DefaultWebProxy = null;
            }
            if (Proxy_flag == "系统代理")
            {
                WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy( );
            }
            if (Proxy_flag == "自定义代理")
            {
                try
                {
                    WebProxy webProxy = new(Proxy_url, Convert.ToInt32(Proxy_port));
                    if (!string.IsNullOrEmpty(Proxy_name) && !string.IsNullOrEmpty(Proxy_password))
                    {
                        ICredentials credentials = new NetworkCredential(Proxy_name, Proxy_password);
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
                Program.checkTimer.Interval = 3600000.0 * Convert.ToInt32(IniHelp.GetValue("更新", "间隔时间"));
                Program.checkTimer.Elapsed += Program.CheckTimer_Elapsed;
                Program.checkTimer.Start( );
            }
        }
    }

    private void tray_limit_Click(object o, EventArgs e) => new Thread(new ThreadStart(about)).Start( );

    private static bool IsNum(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] is < '0' or > '9')
            {
                return false;
            }
        }
        return true;
    }

    private static bool own_punctuation(string text) => ",;，、__《》()-（）.。".IndexOf(text) != -1;

    private static string punctuation_Del_space(string text)
    {
        string text2 = "(?<=.)([^\\*]+)(?=.)";
        string text3;
        if (Regex.Match(text, text2).ToString( ).IndexOf(" ") >= 0)
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

    private void transtalate_Click( )
    {
        typeset_txt = RichBoxBody.Text;
        RichBoxBody_T.Visible = true;
        WindowState = FormWindowState.Normal;
        transtalate_fla = "开启";
        RichBoxBody.Dock = DockStyle.None;
        RichBoxBody_T.Dock = DockStyle.None;
        RichBoxBody_T.BorderStyle = BorderStyle.Fixed3D;
        RichBoxBody_T.Text = "";
        RichBoxBody.Focus( );
        if (num_ok == 0)
        {
            RichBoxBody.Size = new Size(ClientRectangle.Width, ClientRectangle.Height);
            Size = new Size(RichBoxBody.Width * 2, RichBoxBody.Height);
            RichBoxBody_T.Size = new Size(RichBoxBody.Width, RichBoxBody.Height);
            RichBoxBody_T.Location = (Point) new Size(RichBoxBody.Width, 0);
            RichBoxBody_T.Name = "rich_trans";
            RichBoxBody_T.TabIndex = 1;
            RichBoxBody_T.Text_flag = "我是翻译文本框";
            RichBoxBody_T.ImeMode = ImeMode.On;
        }
        num_ok++;
        PictureBox1.Visible = true;
        PictureBox1.BringToFront( );
        MinimumSize = new Size((int) font_base.Width * 23 * 2, (int) font_base.Height * 24);
        Size = new Size((int) font_base.Width * 23 * 2, (int) font_base.Height * 24);
        CheckForIllegalCrossThreadCalls = false;
        new Thread(new ThreadStart(trans_Calculate)).Start( );
    }

    private void Form_Resize(object o, EventArgs e)
    {
        if (RichBoxBody.Dock != DockStyle.Fill)
        {
            RichBoxBody.Size = new Size(ClientRectangle.Width / 2, ClientRectangle.Height);
            RichBoxBody_T.Size = new Size(RichBoxBody.Width, ClientRectangle.Height);
            RichBoxBody_T.Location = (Point) new Size(RichBoxBody.Width, 0);
        }
    }

    private void Trans_copy_Click(object o, EventArgs e)
    {
        RichBoxBody_T.Focus( );
        RichBoxBody_T.richTextBox1.Copy( );
    }

    private void Trans_paste_Click(object o, EventArgs e)
    {
        RichBoxBody_T.Focus( );
        RichBoxBody_T.richTextBox1.Paste( );
    }

    private void Trans_SelectAll_Click(object o, EventArgs e)
    {
        RichBoxBody_T.Focus( );
        RichBoxBody_T.richTextBox1.SelectAll( );
    }

    private void trans_Calculate( )
    {
        if (pinyin_flag)
        {
            googleTranslate_txt = Pinyin.ToPinyin(typeset_txt);
        }
        else if (string.IsNullOrEmpty(typeset_txt))
        {
            googleTranslate_txt = "";
        }
        else
        {
            if (interface_flag == "韩语")
            {
                StaticValue.zh_en = false;
                StaticValue.zh_jp = false;
                StaticValue.zh_ko = true;
                RichBoxBody_T.set_language = "韩语";
            }
            if (interface_flag == "日语")
            {
                StaticValue.zh_en = false;
                StaticValue.zh_jp = true;
                StaticValue.zh_ko = false;
                RichBoxBody_T.set_language = "日语";
            }
            if (interface_flag == "中英")
            {
                StaticValue.zh_en = true;
                StaticValue.zh_jp = false;
                StaticValue.zh_ko = false;
                RichBoxBody_T.set_language = "中英";
            }
            if (IniHelp.GetValue("配置", "翻译接口") == "谷歌")
            {
                googleTranslate_txt = Translate_Google(typeset_txt);
            }
            if (IniHelp.GetValue("配置", "翻译接口") == "百度")
            {
                googleTranslate_txt = TranslateBaidu(typeset_txt);
            }
            if (IniHelp.GetValue("配置", "翻译接口") == "腾讯")
            {
                googleTranslate_txt = Translate_Tencent(typeset_txt);
            }
        }
        PictureBox1.Visible = false;
        PictureBox1.SendToBack( );
        Invoke(new translate(translate_child));
        pinyin_flag = false;
    }

    private void Trans_close_Click(object o, EventArgs e)
    {
        base.MinimumSize = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
        transtalate_fla = "关闭";
        RichBoxBody.Dock = DockStyle.Fill;
        RichBoxBody_T.Visible = false;
        PictureBox1.Visible = false;
        RichBoxBody_T.Text = "";
        if (WindowState == FormWindowState.Maximized)
        {
            WindowState = FormWindowState.Normal;
        }
        Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
    }

    private void ShowLoading( )
    {
        try
        {
            fmloading = new Fmloading( );
            Application.Run(fmloading);
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString( ));
        }
        finally
        {
            thread.Abort( );
        }
    }

    private void TTS( ) => new Thread(new ThreadStart(TTS_thread)).Start( );

    private void about( )
    {
        WindowState = FormWindowState.Minimized;
        CheckForIllegalCrossThreadCalls = false;
        new Thread(new ThreadStart(ThreadFun)).Start( );
    }

    private void ThreadFun( )
    {
    }

    private void translate_child( )
    {
        RichBoxBody_T.Text = googleTranslate_txt;
        googleTranslate_txt = "";
    }

    private void TTS_thread( )
    {
        try
        {
            Stream responseStream = ((HttpWebResponse) ((HttpWebRequest) WebRequest.Create(string.Format("{0}?{1}", "http://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=iQekhH39WqHoxur5ss59GpU4&client_secret=8bcee1cee76ed60cdfaed1f2c038584d"))).GetResponse( )).GetResponseStream( );
            string text = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            string text2 = !TextUtils.ContainsZh(htmltxt) ? "zh" : "zh";
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(string.Concat(new string[]
                {
                    "http://tsn.baidu.com/text2audio?lan=" + text2 + "&ctp=1&cuid=abcdxxx&tok=",
                    ((JObject)JsonConvert.DeserializeObject(text))["access_token"].ToString(),
                    "&tex=",
                    HttpUtility.UrlEncode(htmltxt.Replace("***", "")),
                    "&vol=9&per=0&spd=5&pit=5"
                }));
            httpWebRequest.Method = "POST";
            HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( );
            byte[] array = new byte[16384];
            byte[] array2;
            using (MemoryStream memoryStream = new( ))
            {
                int num;
                while ((num = httpWebResponse.GetResponseStream( ).Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, num);
                }
                array2 = memoryStream.ToArray( );
            }
            ttsData = array2;
            if (speak_copyb == "朗读" || voice_count == 0)
            {
                Invoke(new translate(Speak_child));
                speak_copyb = "";
            }
            else
            {
                Invoke(new translate(TTS_child));
            }
            voice_count++;
        }
        catch (Exception ex)
        {
            if (ex.ToString( ).IndexOf("Null") <= -1)
            {
                MessageBox.Show("文本过长，请使用右键菜单中的选中朗读！", "提醒");
            }
        }
    }

    private void TTS_child( )
    {
        if (RichBoxBody.Text != null || !string.IsNullOrEmpty(RichBoxBody_T.Text))
        {
            if (speaking)
            {
                mciSendString("close media", null, 0, IntPtr.Zero);
                speaking = false;
                return;
            }
            string tempPath = Path.GetTempPath( );
            string text = tempPath + "\\声音.mp3";
            try
            {
                File.WriteAllBytes(text, ttsData);
            }
            catch
            {
                text = tempPath + "\\声音1.mp3";
                File.WriteAllBytes(text, ttsData);
            }
            PlaySong(text);
            speaking = true;
        }
    }
    public void TrayUpdateClick(object o, EventArgs e) => Program.CheckUpdate( );

    public void PlaySong(string file)
    {
        mciSendString("close media", null, 0, IntPtr.Zero);
        mciSendString("open \"" + file + "\" type mpegvideo alias media", null, 0, IntPtr.Zero);
        mciSendString("play media notify", null, 0, Handle);
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

    private static string GetGoogletHtml(string url, CookieContainer cookie, string refer)
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
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                using StreamReader streamReader = new(httpWebResponse.GetResponseStream( ), Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            text2 = text;
        }
        catch
        {
            text2 = null;
        }
        return text2;
    }

    private static string Translate_Google(string text)
    {
        string text2 = "";
        try
        {
            string text3 = "zh-CN";
            string text4 = "en";
            if (StaticValue.zh_en)
            {
                if (ch_count(text.Trim( )) > en_count(text.Trim( )) || (en_count(text.Trim( )) == 1 && ch_count(text.Trim( )) == 1))
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
                if (TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(text.Trim( )))))
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
                if (TextUtils.ContainKor(text.Trim( )))
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
            HttpHelper httpHelper = new( );
            HttpItem httpItem = new( )
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
            JArray jarray = (JArray) JsonConvert.DeserializeObject(httpHelper.GetHtml(httpItem).Html);
            int count = ((JArray) jarray[0]).Count;
            for (int i = 0; i < count; i++)
            {
                text2 += jarray[0][i][0].ToString( );
            }
        }
        catch (Exception)
        {
            text2 = "[谷歌接口报错]：\r\n1.网络错误或者文本过长。\r\n2.谷歌接口可能对于某些网络不能用，具体不清楚。可以尝试挂VPN试试。\r\n3.这个问题我没办法修复，请右键菜单更换百度、腾讯翻译接口。";
        }
        return text2;
    }

    private static string CookieCollectionToStrCookie(CookieCollection cookie)
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
                Cookie cookie2 = (Cookie) obj;
                text2 += string.Format("{0}={1};", cookie2.Name, cookie2.Value);
            }
            text = text2;
        }
        return text;
    }

    private string ScanQRCode( )
    {
        string text = "";
        try
        {
            BinaryBitmap binaryBitmap = new(new HybridBinarizer(new BitmapLuminanceSource((Bitmap) image_screen)));
            Result result = new QRCodeReader( ).decode(binaryBitmap);
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

    private void Main_baidu_search(object o, EventArgs e)
    {
        if (string.IsNullOrEmpty(RichBoxBody.SelectText))
        {
            Process.Start("https://www.baidu.com/");
            return;
        }
        Process.Start("https://www.baidu.com/s?wd=" + RichBoxBody.SelectText);
    }

    private void Main_Voice_Click(object o, EventArgs e)
    {
        RichBoxBody.Focus( );
        speak_copyb = "朗读";
        htmltxt = RichBoxBody.SelectText;
        SendMessage(Handle, 786, 590);
    }

    private void Trans_Voice_Click(object o, EventArgs e)
    {
        RichBoxBody_T.Focus( );
        speak_copyb = "朗读";
        htmltxt = RichBoxBody_T.SelectText;
        SendMessage(Handle, 786, 590);
    }

    private void Speak_child( )
    {
        if (RichBoxBody.Text != null || !string.IsNullOrEmpty(RichBoxBody_T.Text))
        {
            string tempPath = Path.GetTempPath( );
            string text = tempPath + "\\声音.mp3";
            try
            {
                File.WriteAllBytes(text, ttsData);
            }
            catch
            {
                text = tempPath + "\\声音1.mp3";
                File.WriteAllBytes(text, ttsData);
            }
            PlaySong(text);
            speaking = true;
        }
    }

    private void change_zh_tra_Click(object o, EventArgs e)
    {
        if (RichBoxBody.Text != null)
        {
            RichBoxBody.Text = TextUtils.ToTraditional(RichBoxBody.Text);
        }
    }

    private void change_tra_zh_Click(object o, EventArgs e)
    {
        if (RichBoxBody.Text != null)
        {
            RichBoxBody.Text = TextUtils.ToSimplified(RichBoxBody.Text);
        }
    }

    private void change_str_Upper_Click(object o, EventArgs e)
    {
        if (RichBoxBody.Text != null)
        {
            RichBoxBody.Text = RichBoxBody.Text.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    private void change_Upper_str_Click(object o, EventArgs e)
    {
        if (RichBoxBody.Text != null)
        {
            RichBoxBody.Text = RichBoxBody.Text.ToLower(System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    private static string[] hotkey(string text, string text2, string value)
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

    private void SetHotkey(string text, string text2, string value, int flag)
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
        if (!RegisterHotKey(Handle, flag, (KeyModifiers) Enum.Parse(typeof(KeyModifiers), array2[0].Trim( )), (Keys) Enum.Parse(typeof(Keys), array2[1].Trim( ))))
        {
            fmflags.Show( );
            fmflags.DrawStr("快捷键冲突，请更换！");
        }
        RegisterHotKey(Handle, flag, (KeyModifiers) Enum.Parse(typeof(KeyModifiers), array2[0].Trim( )), (Keys) Enum.Parse(typeof(Keys), array2[1].Trim( )));
    }

    private void p_note(string a)
    {
        for (int i = 0; i < StaticValue.v_notecount; i++)
        {
            if (i == StaticValue.v_notecount - 1)
            {
                pubnote[StaticValue.v_notecount - 1] = a;
            }
            else
            {
                pubnote[i] = pubnote[i + 1];
            }
        }
    }

    private void tray_note_Click(object o, EventArgs e)
    {
        fmnote.Show( );
        fmnote.WindowState = FormWindowState.Normal;
        fmnote.Visible = true;
    }

    private string Google_Hotkey(string text)
    {
        string text2 = "";
        try
        {
            string text3;
            string text4;
            if (TextUtils.ContainsZh(trans_hotkey.Trim( )))
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
                    "https://translate.googleapis.com/translate_a/single?client=gtx&sl=",
                    text3,
                    "&tl=",
                    text4,
                    "&dt=t&q=",
                    HttpUtility.UrlEncode(text).Replace("+", "%20")
            });
            JArray jarray = (JArray) JsonConvert.DeserializeObject(Get_GoogletHtml(text5));
            int count = ((JArray) jarray[0]).Count;
            for (int i = 0; i < count; i++)
            {
                text2 += jarray[0][i][0].ToString( );
            }
        }
        catch (Exception ex)
        {
            text2 = "[Error]:" + ex.Message;
        }
        return text2;
    }

    private static string Get_html(string url)
    {
        string text = "";
        HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
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

    private static CookieContainer Post_Html_Getcookie(string url, string post_str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(post_str);
        HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.Timeout = 5000;
        httpWebRequest.Headers.Add("Accept-Language:zh-CN,zh;q=0.8");
        httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        httpWebRequest.CookieContainer = new CookieContainer( );
        try
        {
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            StreamReader streamReader = new(responseStream, Encoding.GetEncoding("utf-8"));
            streamReader.ReadToEnd( );
            responseStream.Close( );
            streamReader.Close( );
            httpWebRequest.Abort( );
        }
        catch
        {
        }
        return httpWebRequest.CookieContainer;
    }

    private static string Post_Html_Reccookie(string url, string post_str, CookieContainer CookieContainer)
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
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            StreamReader streamReader = new(responseStream, Encoding.GetEncoding("utf-8"));
            text = streamReader.ReadToEnd( );
            responseStream.Close( );
            streamReader.Close( );
            httpWebRequest.Abort( );
        }
        catch
        {
        }
        return text;
    }

    private static string Post_Html(string url, string post_str)
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
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            StreamReader streamReader = new(responseStream, Encoding.GetEncoding("utf-8"));
            text = streamReader.ReadToEnd( );
            responseStream.Close( );
            streamReader.Close( );
            httpWebRequest.Abort( );
        }
        catch
        {
        }
        return text;
    }

    private void Main_OCR_Quickscreenshots( )
    {
        if (!StaticValue.截图排斥)
        {
            try
            {
                change_QQ_screenshot = false;
                FormBorderStyle = FormBorderStyle.None;
                Visible = false;
                Thread.Sleep(100);
                form_width = transtalate_fla == "开启" ? Width / 2 : Width;
                shupai_Right_txt = "";
                shupai_Left_txt = "";
                form_height = Height;
                minico.Visible = false;
                minico.Visible = true;
                menu.Close( );
                menu_copy.Close( );
                auto_fla = "开启";
                SplitedText = "";
                RichBoxBody.Text = "***该区域未发现文本***";
                RichBoxBody_T.Text = "";
                typeset_txt = "";
                transtalate_fla = "关闭";
                if (IniHelp.GetValue("工具栏", "翻译") == "False")
                {
                    Trans_close.PerformClick( );
                }
                Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
                FormBorderStyle = FormBorderStyle.Sizable;
                StaticValue.截图排斥 = true;
                image_screen = RegionCaptureTasks.GetRegionImage_Mo(new RegionCaptureOptions
                {
                    ShowMagnifier = false,
                    UseSquareMagnifier = false,
                    MagnifierPixelCount = 15,
                    MagnifierPixelSize = 10
                }, out string mode_flag, out Point point, out Rectangle[] array);
                if (mode_flag == "高级截图")
                {
                    RegionCaptureMode regionCaptureMode = RegionCaptureMode.Annotation;
                    RegionCaptureOptions regionCaptureOptions = new( );
                    using RegionCaptureForm regionCaptureForm = new(regionCaptureMode, regionCaptureOptions);
                    regionCaptureForm.Image_get = false;
                    regionCaptureForm.Prepare(image_screen);
                    regionCaptureForm.ShowDialog( );
                    image_screen = null;
                    image_screen = regionCaptureForm.GetResultImage( );
                    mode_flag = regionCaptureForm.Mode_flag;
                }
                RegisterHotKey(Handle, 222, KeyModifiers.None, Keys.Escape);
                if (mode_flag == "贴图")
                {
                    Point point2 = new(point.X, point.Y);
                    new FmScreenPaste(image_screen, point2).Show( );
                    if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value = IniHelp.GetValue("快捷键", "翻译文本");
                        string text = "None";
                        string text2 = "F9";
                        SetHotkey(text, text2, value, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                    StaticValue.截图排斥 = false;
                }
                else if (mode_flag == "区域多选")
                {
                    if (image_screen == null)
                    {
                        if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                        {
                            string value2 = IniHelp.GetValue("快捷键", "翻译文本");
                            string text3 = "None";
                            string text4 = "F9";
                            SetHotkey(text3, text4, value2, 205);
                        }
                        UnregisterHotKey(Handle, 222);
                        StaticValue.截图排斥 = false;
                    }
                    else
                    {
                        minico.Visible = true;
                        thread = new Thread(new ThreadStart(ShowLoading));
                        thread.Start( );
                        ts = new TimeSpan(DateTime.Now.Ticks);
                        getSubPics_ocr(image_screen, array);
                    }
                }
                else if (mode_flag == "取色")
                {
                    if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value3 = IniHelp.GetValue("快捷键", "翻译文本");
                        string text5 = "None";
                        string text6 = "F9";
                        SetHotkey(text5, text6, value3, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                    StaticValue.截图排斥 = false;
                    fmflags.Show( );
                    fmflags.DrawStr("已复制颜色");
                }
                else if (image_screen == null)
                {
                    if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value4 = IniHelp.GetValue("快捷键", "翻译文本");
                        string text7 = "None";
                        string text8 = "F9";
                        SetHotkey(text7, text8, value4, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                    StaticValue.截图排斥 = false;
                }
                else
                {
                    if (mode_flag == "百度")
                    {
                        baidu_flags = "百度";
                    }
                    if (mode_flag == "拆分")
                    {
                        set_merge = false;
                        set_split = true;
                    }
                    if (mode_flag == "合并")
                    {
                        set_merge = true;
                        set_split = false;
                    }
                    if (mode_flag == "截图")
                    {
                        Clipboard.SetImage(image_screen);
                        if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                        {
                            string value5 = IniHelp.GetValue("快捷键", "翻译文本");
                            string text9 = "None";
                            string text10 = "F9";
                            SetHotkey(text9, text10, value5, 205);
                        }
                        UnregisterHotKey(Handle, 222);
                        StaticValue.截图排斥 = false;
                        if (IniHelp.GetValue("截图音效", "粘贴板") == "True")
                        {
                            PlaySong(IniHelp.GetValue("截图音效", "音效路径"));
                        }
                        fmflags.Show( );
                        fmflags.DrawStr("已复制截图");
                    }
                    else if (mode_flag == "自动保存" && IniHelp.GetValue("配置", "自动保存") == "True")
                    {
                        string text11 = IniHelp.GetValue("配置", "截图位置") + "\\" + TextUtils.RenameFile(IniHelp.GetValue("配置", "截图位置"), "图片.Png");
                        image_screen.Save(text11, ImageFormat.Png);
                        StaticValue.截图排斥 = false;
                        if (IniHelp.GetValue("截图音效", "自动保存") == "True")
                        {
                            PlaySong(IniHelp.GetValue("截图音效", "音效路径"));
                        }
                        fmflags.Show( );
                        fmflags.DrawStr("已保存图片");
                    }
                    else if (mode_flag == "多区域自动保存" && IniHelp.GetValue("配置", "自动保存") == "True")
                    {
                        getSubPics(image_screen, array);
                        StaticValue.截图排斥 = false;
                        if (IniHelp.GetValue("截图音效", "自动保存") == "True")
                        {
                            PlaySong(IniHelp.GetValue("截图音效", "音效路径"));
                        }
                        fmflags.Show( );
                        fmflags.DrawStr("已保存图片");
                    }
                    else if (mode_flag == "保存")
                    {
                        SaveFileDialog saveFileDialog = new( )
                        {
                            Filter = "png图片(*.png)|*.png|jpg图片(*.jpg)|*.jpg|bmp图片(*.bmp)|*.bmp",
                            AddExtension = false,
                            FileName = string.Concat(new string[]
                        {
                            "tianruo_",
                            DateTime.Now.Year.ToString(),
                            "-",
                            DateTime.Now.Month.ToString(),
                            "-",
                            DateTime.Now.Day.ToString(),
                            "-",
                            DateTime.Now.Ticks.ToString()
                        }),
                            Title = "保存图片",
                            FilterIndex = 1,
                            RestoreDirectory = true
                        };
                        if (saveFileDialog.ShowDialog( ) == DialogResult.OK)
                        {
                            string extension = Path.GetExtension(saveFileDialog.FileName);
                            if (extension.Equals(".jpg", StringComparison.Ordinal))
                            {
                                image_screen.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                            }
                            if (extension.Equals(".png", StringComparison.Ordinal))
                            {
                                image_screen.Save(saveFileDialog.FileName, ImageFormat.Png);
                            }
                            if (extension.Equals(".bmp", StringComparison.Ordinal))
                            {
                                image_screen.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                            }
                        }
                        if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
                        {
                            string value6 = IniHelp.GetValue("快捷键", "翻译文本");
                            string text12 = "None";
                            string text13 = "F9";
                            SetHotkey(text12, text13, value6, 205);
                        }
                        UnregisterHotKey(Handle, 222);
                        StaticValue.截图排斥 = false;
                    }
                    else if (image_screen != null)
                    {
                        if (IniHelp.GetValue("工具栏", "分栏") == "True")
                        {
                            minico.Visible = true;
                            thread = new Thread(new ThreadStart(ShowLoading));
                            thread.Start( );
                            ts = new TimeSpan(DateTime.Now.Ticks);
                            Image image = image_screen;
                            Graphics graphics = Graphics.FromImage(new Bitmap(image.Width, image.Height));
                            graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                            graphics.Save( );
                            graphics.Dispose( );
                            ((Bitmap) FindBundingBox_fences((Bitmap) image)).Save("Data\\分栏预览图.jpg");
                            image.Dispose( );
                            image_screen.Dispose( );
                        }
                        else
                        {
                            minico.Visible = true;
                            thread = new Thread(new ThreadStart(ShowLoading));
                            thread.Start( );
                            ts = new TimeSpan(DateTime.Now.Ticks);
                            Messageload messageload = new( );
                            messageload.ShowDialog( );
                            if (messageload.DialogResult == DialogResult.OK)
                            {
                                esc_thread = new Thread(new ThreadStart(Main_OCR_Thread));
                                esc_thread.Start( );
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

    private void Main_OCR_Thread( )
    {
        if (!string.IsNullOrEmpty(ScanQRCode( )))
        {
            typeset_txt = ScanQRCode( );
            RichBoxBody.Text = typeset_txt;
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_last));
            return;
        }
        if (interface_flag == "搜狗")
        {
            OCR_sougou2( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_last));
            return;
        }
        if (interface_flag == "腾讯")
        {
            OcrTencent( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_last));
            return;
        }
        if (interface_flag == "有道")
        {
            OcrYoudao( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_last));
            return;
        }
        if (interface_flag == "公式")
        {
            OCR_Math( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_last));
            return;
        }
        if (interface_flag == "百度表格")
        {
            OCR_baidu_table( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_table));
            return;
        }
        if (interface_flag == "阿里表格")
        {
            OCR_ali_table( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_table));
            return;
        }
        if (interface_flag is "日语" or "中英" or "韩语")
        {
            OCR_baidu( );
            fmloading.fml_close = "窗体已关闭";
            Invoke(new ocr_thread(Main_OCR_Thread_last));
        }
        if (interface_flag is "从左向右" or "从右向左")
        {
            shupai_Right_txt = "";
            Image image = image_screen;
            Bitmap bitmap = new(image.Width, image.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(image, 0, 0, image.Width, image.Height);
            graphics.Save( );
            graphics.Dispose( );
            image_ori = bitmap;
            Image<Gray, byte> image2 = new(bitmap);
            Image<Gray, byte> image3 = new((Bitmap) FindBundingBox(image2.ToBitmap( )));
            Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>( );
            Image<Gray, byte> image5 = image3.Clone( );
            CvInvoke.Canny(image3, image5, 0.0, 0.0, 5, true);
            select_image(image5, image4);
            bitmap.Dispose( );
            image2.Dispose( );
            image3.Dispose( );
        }
        image_screen.Dispose( );
        GC.Collect( );
    }

    private void Main_OCR_Thread_last( )
    {
        image_screen.Dispose( );
        GC.Collect( );
        StaticValue.截图排斥 = false;
        string text = typeset_txt;
        text = check_str(text);
        SplitedText = check_str(SplitedText);
        if (!TextUtils.HasPunctuation(text))
        {
            text = SplitedText;
        }
        if (TextUtils.ContainsZh(text.Trim( )))
        {
            text = TextUtils.RemoveSpace(text);
        }
        if (!string.IsNullOrEmpty(text))
        {
            RichBoxBody.Text = text;
        }
        StaticValue.v_Split = SplitedText;
        if (bool.Parse(IniHelp.GetValue("工具栏", "拆分")) || set_split)
        {
            set_split = false;
            RichBoxBody.Text = SplitedText;
        }
        if (bool.Parse(IniHelp.GetValue("工具栏", "合并")) || set_merge)
        {
            set_merge = false;
            RichBoxBody.Text = text.Replace("\n", "").Replace("\r", "");
        }
        TimeSpan timeSpan = new(DateTime.Now.Ticks);
        TimeSpan timeSpan2 = timeSpan.Subtract(ts).Duration( );
        string text2 = string.Concat(new string[]
        {
                timeSpan2.Seconds.ToString(),
                ".",
                Convert.ToInt32(timeSpan2.TotalMilliseconds).ToString(),
                "秒"
        });
        if (RichBoxBody.Text != null)
        {
            p_note(RichBoxBody.Text);
            StaticValue.v_note = pubnote;
            if (fmnote.Created)
            {
                fmnote.TextNote = "";
            }
        }
        TopMost = StaticValue.v_topmost;
        Text = "耗时：" + text2;
        minico.Visible = true;
        if (interface_flag == "从右向左")
        {
            RichBoxBody.Text = shupai_Right_txt;
        }
        if (interface_flag == "从左向右")
        {
            RichBoxBody.Text = shupai_Left_txt;
        }
        Clipboard.SetDataObject(RichBoxBody.Text);
        if (baidu_flags == "百度")
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
            Visible = false;
            WindowState = FormWindowState.Minimized;
            Show( );
            Process.Start("https://www.baidu.com/s?wd=" + RichBoxBody.Text);
            baidu_flags = "";
            if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
            {
                string value = IniHelp.GetValue("快捷键", "翻译文本");
                string text3 = "None";
                string text4 = "F9";
                SetHotkey(text3, text4, value, 205);
            }
            UnregisterHotKey(Handle, 222);
            return;
        }
        if (IniHelp.GetValue("配置", "识别弹窗") == "False")
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            Size = new Size((int) font_base.Width * 23, (int) font_base.Height * 24);
            Visible = false;
            fmflags.Show( );
            if (RichBoxBody.Text == "***该区域未发现文本***")
            {
                fmflags.DrawStr("无文本");
            }
            else
            {
                fmflags.DrawStr("已识别");
            }
            if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
            {
                string value2 = IniHelp.GetValue("快捷键", "翻译文本");
                string text5 = "None";
                string text6 = "F9";
                SetHotkey(text5, text6, value2, 205);
            }
            UnregisterHotKey(Handle, 222);
            return;
        }
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        Show( );
        WindowState = FormWindowState.Normal;
        Size = new Size(form_width, form_height);
        SetForegroundWindow(Handle);
        StaticValue.v_googleTranslate_txt = RichBoxBody.Text;
        if (bool.Parse(IniHelp.GetValue("工具栏", "翻译")))
        {
            try
            {
                auto_fla = "";
                Invoke(new translate(transtalate_Click));
            }
            catch
            {
            }
        }
        if (bool.Parse(IniHelp.GetValue("工具栏", "检查")))
        {
            try
            {
                RichBoxBody.Find = "";
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
            SetHotkey(text7, text8, value3, 205);
        }
        UnregisterHotKey(Handle, 222);
        RichBoxBody.Refresh( );
    }

    private void OCR_baidu_Ch_and_En_Click(object o, EventArgs e) => OcrForeach("中英");

    private void OCR_baidu_Jap_Click(object o, EventArgs e) => OcrForeach("日语");

    private void OCR_baidu_Kor_Click(object o, EventArgs e) => OcrForeach("韩语");

    private static string Get_GoogletHtml(string url)
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
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                using StreamReader streamReader = new(httpWebResponse.GetResponseStream( ), Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            text2 = text;
        }
        catch
        {
            text2 = null;
        }
        return text2;
    }

    private void OCR_baidu( )
    {
        SplitedText = "";
        try
        {
            baidu_vip = Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + StaticValue.baiduAPI_ID + "&client_secret=" + StaticValue.baiduAPI_key));
            if (string.IsNullOrEmpty(baidu_vip))
            {
                MessageBox.Show("请检查密钥输入是否正确！", "提醒");
            }
            else
            {
                string text = "CHN_ENG";
                SplitedText = "";
                Image image = image_screen;
                byte[] array = OCR_ImgToByte(image);
                if (interface_flag == "中英")
                {
                    text = "CHN_ENG";
                }
                if (interface_flag == "日语")
                {
                    text = "JAP";
                }
                if (interface_flag == "韩语")
                {
                    text = "KOR";
                }
                string text2 = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array)) + "&language_type=" + text;
                byte[] bytes = Encoding.UTF8.GetBytes(text2);
                HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + ((JObject) JsonConvert.DeserializeObject(baidu_vip))["access_token"]);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httpWebRequest.Timeout = 8000;
                httpWebRequest.ReadWriteTimeout = 5000;
                using (Stream requestStream = httpWebRequest.GetRequestStream( ))
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
                string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
                responseStream.Close( );
                JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["words_result"].ToString( ));
                checked_txt(jarray, 1, "words");
            }
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本或者密钥次数用尽***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private static string check_str(string text)
    {
        if (TextUtils.ContainsZh(text.Trim( )))
        {
            text = PunctuationEnZh(text.Trim( ));
            text = check_ch_en(text.Trim( ));
        }
        else
        {
            text = PunctuationChEn(text.Trim( ));
            if (TextUtils.Contains(text, ".")
                && (TextUtils.Contains(text, ",")
                || TextUtils.Contains(text, "!")
                || TextUtils.Contains(text, "(")
                || TextUtils.Contains(text, ")")
                || TextUtils.Contains(text, "'")))
            {
                text = punctuation_Del_space(text);
            }
        }
        return text;
    }

    private static string punctuation_en_ch_x(string text)
    {
        char[] array = text.ToCharArray( );
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

    private static string OCR_sougou_SogouPost(string url, CookieContainer cookie, byte[] content)
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
        httpWebRequest.ContentLength = content.Length;
        Stream requestStream = httpWebRequest.GetRequestStream( );
        requestStream.Write(content, 0, content.Length);
        requestStream.Close( );
        string text2;
        try
        {
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                Stream stream = httpWebResponse.GetResponseStream( );
                if (httpWebResponse.ContentEncoding.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains("gzip"))
                {
                    stream = new GZipStream(stream, CompressionMode.Decompress);
                }
                using StreamReader streamReader = new(stream, Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            text2 = text;
        }
        catch
        {
            text2 = null;
        }
        return text2;
    }

    private static string OCR_sougou_SogouGet(string url, CookieContainer cookie, string refer)
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
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                Stream stream = httpWebResponse.GetResponseStream( );
                if (httpWebResponse.ContentEncoding.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains("gzip"))
                {
                    stream = new GZipStream(stream, CompressionMode.Decompress);
                }
                using StreamReader streamReader = new(stream, Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            text2 = text;
        }
        catch
        {
            text2 = null;
        }
        return text2;
    }

    private static string OCR_sougou_SogouOCR(Image img)
    {
        CookieContainer cookieContainer = new( );
        string text = "http://pic.sogou.com/pic/upload_pic.jsp";
        string text2 = OCR_sougou_SogouPost(text, cookieContainer, OCR_sougou_Content_Length(img));
        string text3 = "http://pic.sogou.com/pic/ocr/ocrOnline.jsp?query=" + text2;
        string text4 = "http://pic.sogou.com/resource/pic/shitu_intro/word_1.html?keyword=" + text2;
        return OCR_sougou_SogouGet(text3, cookieContainer, text4);
    }

    private static byte[] OCR_ImgToByte(Image img)
    {
        byte[] array2;
        try
        {
            MemoryStream memoryStream = new( );
            img.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            array2 = array;
        }
        catch
        {
            array2 = null;
        }
        return array2;
    }

    private static byte[] OCR_sougou_Content_Length(Image img)
    {
        byte[] bytes = Encoding.UTF8.GetBytes("------WebKitFormBoundary1ZZDB9E4sro7pf0g\r\nContent-Disposition: form-data; name=\"pic_path\"; filename=\"test2018.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n");
        byte[] array = OCR_ImgToByte(img);
        byte[] bytes2 = Encoding.UTF8.GetBytes("\r\n------WebKitFormBoundary1ZZDB9E4sro7pf0g--\r\n");
        byte[] array2 = new byte[bytes.Length + array.Length + bytes2.Length];
        bytes.CopyTo(array2, 0);
        array.CopyTo(array2, bytes.Length);
        bytes2.CopyTo(array2, bytes.Length + array.Length);
        return array2;
    }

    private void OCR_sougou2( )
    {
        try
        {
            SplitedText = "";
            string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
            Image image = ZoomImage((Bitmap) image_screen, 120, 120);
            byte[] array = OCR_ImgToByte(image);
            string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = "\r\n" + text + "--\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(text2);
            byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
            byte[] array2 = Mergebyte(bytes, array, bytes2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ocr.shouji.sogou.com/v2/ocr/json");
            httpWebRequest.Timeout = 8000;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(array2, 0, array2.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text4))["result"].ToString( ));
            if (IniHelp.GetValue("工具栏", "分段") == "True")
            {
                checked_location_sougou(jarray, 2, "content", "frame");
            }
            else
            {
                checked_txt(jarray, 2, "content");
            }
            image.Dispose( );
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private static byte[] Mergebyte(byte[] a, byte[] b, byte[] c)
    {
        byte[] array = new byte[a.Length + b.Length + c.Length];
        a.CopyTo(array, 0);
        b.CopyTo(array, a.Length);
        c.CopyTo(array, a.Length + b.Length);
        return array;
    }

    private static bool contain_punctuation(string str) => Regex.IsMatch(str, "\\p{P}");

    private void TrayHelpClick(object o, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        new FmHelp( ).Show( );
    }

    private static bool Is_punctuation(string text) => ",;:，（）、；".IndexOf(text) != -1;

    private static bool has_punctuation(string text) => ",;，；、<>《》()-（）".IndexOf(text) != -1;

    private void checked_txt(JArray jarray, int lastlength, string words)
    {
        int num = 0;
        for (int i = 0; i < jarray.Count; i++)
        {
            int length = JObject.Parse(jarray[i].ToString( ))[words].ToString( ).Length;
            if (length > num)
            {
                num = length;
            }
        }
        string text = "";
        string text2 = "";
        for (int j = 0; j < jarray.Count - 1; j++)
        {
            JObject jobject = JObject.Parse(jarray[j].ToString( ));
            char[] array = jobject[words].ToString( ).ToCharArray( );
            JObject jobject2 = JObject.Parse(jarray[j + 1].ToString( ));
            char[] array2 = jobject2[words].ToString( ).ToCharArray( );
            int length2 = jobject[words].ToString( ).Length;
            int length3 = jobject2[words].ToString( ).Length;
            if (Math.Abs(length2 - length3) <= 0)
            {
                if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && Is_punctuation(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else
                {
                    text2 += jobject[words].ToString( ).Trim( );
                }
            }
            else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && Math.Abs(length2 - length3) <= 1)
            {
                if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && Is_punctuation(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else
                {
                    text2 += jobject[words].ToString( ).Trim( );
                }
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && length2 <= num / 2)
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )) && length3 - length2 < 4 && array2[1].ToString( ) == ".")
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (TextUtils.ContainEn(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + " ";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (TextUtils.ContainEn(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && Is_punctuation(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (Is_punctuation(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (Is_punctuation(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + " ";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (IsNum(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (IsNum(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            if (has_punctuation(jobject[words].ToString( )))
            {
                text2 += "\r\n";
            }
            text = text + jobject[words].ToString( ).Trim( ) + "\r\n";
        }
        SplitedText = text + JObject.Parse(jarray[jarray.Count - 1].ToString( ))[words];
        typeset_txt = text2.Replace("\r\n\r\n", "\r\n") + JObject.Parse(jarray[jarray.Count - 1].ToString( ))[words];
    }

    private void OcrForeach(string name)
    {
        if (name == "韩语")
        {
            interface_flag = "韩语";
            Refresh( );
            baidu.Text = "百度√";
            kor.Text = "韩语√";
        }
        if (name == "日语")
        {
            interface_flag = "日语";
            Refresh( );
            baidu.Text = "百度√";
            jap.Text = "日语√";
        }
        if (name == "中英")
        {
            interface_flag = "中英";
            Refresh( );
            baidu.Text = "百度√";
            ch_en.Text = "中英√";
        }
        if (name == "搜狗")
        {
            interface_flag = "搜狗";
            Refresh( );
            sougou.Text = "搜狗√";
        }
        if (name == "腾讯")
        {
            interface_flag = "腾讯";
            Refresh( );
            tencent.Text = "腾讯√";
        }
        if (name == "有道")
        {
            interface_flag = "有道";
            Refresh( );
            youdao.Text = "有道√";
        }
        if (name == "公式")
        {
            interface_flag = "公式";
            Refresh( );
            Mathfuntion.Text = "公式√";
        }
        if (name == "百度表格")
        {
            interface_flag = "百度表格";
            Refresh( );
            ocr_table.Text = "表格√";
            baidu_table.Text = "百度√";
        }
        if (name == "阿里表格")
        {
            interface_flag = "阿里表格";
            Refresh( );
            ocr_table.Text = "表格√";
            ali_table.Text = "阿里√";
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
                interface_flag = "从左向右";
                Refresh( );
                shupai.Text = "竖排√";
                left_right.Text = "从左向右√";
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
            interface_flag = "从右向左";
            Refresh( );
            shupai.Text = "竖排√";
            righ_left.Text = "从右向左√";
        }
        IniHelp.SetValue("配置", "接口", interface_flag);
    }

    private void OCR_shupai_Click(object o, EventArgs e)
    {
    }

    private void OCR_write_Click(object o, EventArgs e) => OcrForeach("手写");

    private void OCR_lefttoright_Click(object o, EventArgs e) => OcrForeach("从左向右");

    private void OCR_righttoleft_Click(object o, EventArgs e) => OcrForeach("从右向左");

    private void OCR_baidu_acc( )
    {
        SplitedText = "";
        string text = "";
        try
        {
            baidu_vip = Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + StaticValue.baiduAPI_ID + "&client_secret=" + StaticValue.baiduAPI_key));
            if (string.IsNullOrEmpty(baidu_vip))
            {
                MessageBox.Show("请检查密钥输入是否正确！", "提醒");
            }
            else
            {
                SplitedText = "";
                Image image = image_screen;
                byte[] array = OCR_ImgToByte(image);
                string text2 = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array));
                byte[] bytes = Encoding.UTF8.GetBytes(text2);
                HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + ((JObject) JsonConvert.DeserializeObject(baidu_vip))["access_token"]);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httpWebRequest.Timeout = 8000;
                httpWebRequest.ReadWriteTimeout = 5000;
                ServicePointManager.DefaultConnectionLimit = 512;
                using (Stream requestStream = httpWebRequest.GetRequestStream( ))
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
                string text3;
                text = text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
                responseStream.Close( );
                JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["words_result"].ToString( ));
                string text4 = "";
                for (int i = 0; i < jarray.Count; i++)
                {
                    JObject jobject = JObject.Parse(jarray[i].ToString( ));
                    text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                }
                shupai_Right_txt = shupai_Right_txt + text4 + "\r\n";
                Thread.Sleep(600);
            }
        }
        catch
        {
            MessageBox.Show(text, "提醒");
            StaticValue.截图排斥 = false;
            esc = "退出";
            fmloading.fml_close = "窗体已关闭";
            esc_thread.Abort( );
        }
    }

    private void OCR_Tencent_handwriting( )
    {
        try
        {
            SplitedText = "";
            string text = "------WebKitFormBoundaryRDEqU0w702X9cWPJ";
            Image image = image_screen;
            if (image.Width > 90 && image.Height < 90)
            {
                Bitmap bitmap = new(image.Width, 300);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, 5, 0, image.Width, image.Height);
                graphics.Save( );
                graphics.Dispose( );
                image = new Bitmap(bitmap);
            }
            else if (image.Width <= 90 && image.Height >= 90)
            {
                Bitmap bitmap2 = new(300, image.Height);
                Graphics graphics2 = Graphics.FromImage(bitmap2);
                graphics2.DrawImage(image, 0, 5, image.Width, image.Height);
                graphics2.Save( );
                graphics2.Dispose( );
                image = new Bitmap(bitmap2);
            }
            else if (image.Width < 90 && image.Height < 90)
            {
                Bitmap bitmap3 = new(300, 300);
                Graphics graphics3 = Graphics.FromImage(bitmap3);
                graphics3.DrawImage(image, 5, 5, image.Width, image.Height);
                graphics3.Save( );
                graphics3.Dispose( );
                image = new Bitmap(bitmap3);
            }
            else
            {
                image = image_screen;
            }
            byte[] array = OCR_ImgToByte(image);
            string text2 = text + "\r\nContent-Disposition: form-data; name=\"image_file\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = "\r\n" + text + "--\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(text2);
            byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
            byte[] array2 = Mergebyte(bytes, array, bytes2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://ai.qq.com/cgi-bin/appdemo_handwritingocr");
            httpWebRequest.Method = "POST";
            httpWebRequest.Referer = "http://ai.qq.com/product/ocr.shtml";
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
            httpWebRequest.Timeout = 8000;
            httpWebRequest.ReadWriteTimeout = 2000;
            byte[] array3 = array2;
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(array3, 0, array2.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text4))["data"]["item_list"].ToString( ));
            checked_txt(jarray, 1, "itemstring");
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private static Image BoundingBox(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        Image image2;
        using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new( ))
        {
            CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
            Image image = draw.ToBitmap( );
            Graphics graphics = Graphics.FromImage(image);
            int size = vectorOfVectorOfPoint.Size;
            for (int i = 0; i < size; i++)
            {
                using VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i];
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
            graphics.Dispose( );
            Bitmap bitmap = new(image.Width + 2, image.Height + 2);
            Graphics graphics2 = Graphics.FromImage(bitmap);
            graphics2.DrawImage(image, 1, 1, image.Width, image.Height);
            graphics2.Save( );
            graphics2.Dispose( );
            image2 = bitmap;
        }
        return image2;
    }

    private void select_image(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        try
        {
            using VectorOfVectorOfPoint vectorOfVectorOfPoint = new( );
            CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
            int num = vectorOfVectorOfPoint.Size / 2;
            imagelist_lenght = num;
            bool_image_count(num);
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data\\image_temp"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Data\\image_temp");
            }
            OCR_baidu_a = "";
            OCR_baidu_b = "";
            OCR_baidu_c = "";
            OCR_baidu_d = "";
            OCR_baidu_e = "";
            for (int i = 0; i < num; i++)
            {
                using VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i];
                Rectangle rectangle = CvInvoke.BoundingRectangle(vectorOfPoint);
                if (rectangle.Size.Width > 1 && rectangle.Size.Height > 1)
                {
                    int x = rectangle.Location.X;
                    int y = rectangle.Location.Y;
                    int width = rectangle.Size.Width;
                    int height = rectangle.Size.Height;
                    new Point(x, 0);
                    new Point(x, image_ori.Size.Height);
                    Rectangle rectangle2 = new(x, 0, width, image_ori.Size.Height);
                    Bitmap bitmap = new(width + 70, rectangle2.Size.Height);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Size.Width, bitmap.Size.Height);
                    graphics.DrawImage(image_ori, 30, 0, rectangle2, GraphicsUnit.Pixel);
                    Bitmap bitmap2 = Image.FromHbitmap(bitmap.GetHbitmap( ));
                    bitmap2.Save("Data\\image_temp\\" + i + ".jpg", ImageFormat.Jpeg);
                    bitmap2.Dispose( );
                    bitmap.Dispose( );
                    graphics.Dispose( );
                }
            }
            Messageload messageload = new( );
            messageload.ShowDialog( );
            if (messageload.DialogResult == DialogResult.OK)
            {
                ManualResetEvent[] array = new ManualResetEvent[]
                {
                        new ManualResetEvent(false)
                };
                ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), array[0]);
            }
        }
        catch
        {
            exit_thread( );
        }
    }

    private static Image FindBundingBox(Bitmap bitmap)
    {
        Image<Bgr, byte> image = new(bitmap);
        Image<Gray, byte> image2 = new(image.Width, image.Height);
        CvInvoke.CvtColor(image, image2, ColorConversion.Bgra2Gray, 0);
        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(1, 1));
        CvInvoke.Erode(image2, image2, structuringElement, new Point(0, 2), 1, BorderType.Reflect101, default);
        CvInvoke.Threshold(image2, image2, 100.0, 255.0, (ThresholdType) 9);
        Image<Gray, byte> image3 = new(image2.ToBitmap( ));
        Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>( );
        Image<Gray, byte> image5 = image3.Clone( );
        CvInvoke.Canny(image3, image5, 255.0, 255.0, 5, true);
        return BoundingBox(image5, image4);
    }

    private void Captureimage(int width, Image g_image, string saveFilePath, Rectangle rect)
    {
        Bitmap bitmap = new(width + 70, g_image.Size.Height);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Size.Width, bitmap.Size.Height);
        graphics.DrawImage(g_image, 30, 0, rect, GraphicsUnit.Pixel);
        Bitmap bitmap2 = Image.FromHbitmap(bitmap.GetHbitmap( ));
        bitmap2.Save(saveFilePath, ImageFormat.Jpeg);
        image_screen = bitmap2;
        OCR_baidu_use( );
        bitmap2.Dispose( );
        bitmap.Dispose( );
        graphics.Dispose( );
    }

    private void OCR_baidu_use( )
    {
        SplitedText = "";
        try
        {
            string text = "CHN_ENG";
            SplitedText = "";
            Image image = image_screen;
            MemoryStream memoryStream = new( );
            image.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            string text5 = "";
            for (int j = 0; j < array2.Length; j++)
            {
                text5 += array2[j];
            }
            shupai_Right_txt = (shupai_Right_txt + text4 + "\r\n").Replace("\r\n\r\n", "");
            shupai_Left_txt = text5.Replace("\r\n\r\n", "");
            MessageBox.Show(shupai_Left_txt);
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private void OCR_sougou_use( )
    {
        try
        {
            SplitedText = "";
            string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
            Image image = image_screen;
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
            Bitmap bitmap = new(i, j);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(image, 0, 0, i, j);
            graphics.Save( );
            graphics.Dispose( );
            image = new Bitmap(bitmap);
            byte[] array = OCR_ImgToByte(image);
            string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = "\r\n" + text + "--\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(text2);
            byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
            byte[] array2 = Mergebyte(bytes, array, bytes2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ocr.shouji.sogou.com/v2/ocr/json");
            httpWebRequest.Timeout = 8000;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(array2, 0, array2.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text4))["result"].ToString( ));
            string text5 = "";
            for (int k = 0; k < jarray.Count; k++)
            {
                JObject jobject = JObject.Parse(jarray[k].ToString( ));
                text5 += jobject["content"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            shupai_Right_txt = shupai_Right_txt + text5 + "\r\n";
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private void baidu_image_a(object objEvent)
    {
        try
        {
            for (int i = 0; i < image_num[0]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OCR_baidu_use_A(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            exit_thread( );
        }
    }

    private void baidu_image_b(object objEvent)
    {
        try
        {
            for (int i = image_num[0]; i < image_num[1]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OCR_baidu_use_B(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            exit_thread( );
        }
    }

    private void DoWork(object state)
    {
        ManualResetEvent[] array = new ManualResetEvent[5];
        array[0] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(baidu_image_a), array[0]);
        array[1] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(baidu_image_b), array[1]);
        array[2] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(baidu_image_c), array[2]);
        array[3] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(baidu_image_d), array[3]);
        array[4] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(baidu_image_e), array[4]);
        WaitHandle[] array2 = array;
        WaitHandle.WaitAll(array2);
        shupai_Right_txt = string.Concat(new string[] { OCR_baidu_a, OCR_baidu_b, OCR_baidu_c, OCR_baidu_d, OCR_baidu_e }).Replace("\r\n\r\n", "");
        string text = shupai_Right_txt.TrimEnd(new char[] { '\n' }).TrimEnd(new char[] { '\r' }).TrimEnd(new char[] { '\n' });
        if (text.Split(Environment.NewLine.ToCharArray( )).Length > 1)
        {
            string[] array3 = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string text2 = "";
            for (int i = 0; i < array3.Length; i++)
            {
                text2 = text2 + array3[array3.Length - i - 1].Replace("\r", "").Replace("\n", "") + "\r\n";
            }
            shupai_Left_txt = text2;
        }
        fmloading.fml_close = "窗体已关闭";
        Invoke(new ocr_thread(Main_OCR_Thread_last));
        try
        {
            DeleteFile("Data\\image_temp");
        }
        catch
        {
            exit_thread( );
        }
        image_ori.Dispose( );
    }

    private void OCR_baidu_use_B(Image imagearr)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream memoryStream = new( );
            imagearr.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            string text5 = "";
            for (int j = 0; j < array2.Length; j++)
            {
                text5 += array2[j];
            }
            OCR_baidu_b = (OCR_baidu_b + text4 + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private void OCR_baidu_use_A(Image imagearr)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream memoryStream = new( );
            imagearr.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            string text5 = "";
            for (int j = 0; j < array2.Length; j++)
            {
                text5 += array2[j];
            }
            OCR_baidu_a = (OCR_baidu_a + text4 + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private static void DeleteFile(string path)
    {
        if (File.GetAttributes(path) == FileAttributes.Directory)
        {
            Directory.Delete(path, true);
            return;
        }
        File.Delete(path);
    }

    private static void OCR_baidu_image(Image imagearr, string str_image)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream memoryStream = new( );
            imagearr.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
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

    private void OCR_baidu_use_E(Image imagearr)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream memoryStream = new( );
            imagearr.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            string text5 = "";
            for (int j = 0; j < array2.Length; j++)
            {
                text5 += array2[j];
            }
            OCR_baidu_e = (OCR_baidu_e + text4 + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private void OCR_baidu_use_D(Image imagearr)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream memoryStream = new( );
            imagearr.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            string text5 = "";
            for (int j = 0; j < array2.Length; j++)
            {
                text5 += array2[j];
            }
            OCR_baidu_d = (OCR_baidu_d + text4 + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private void OCR_baidu_use_C(Image imagearr)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream memoryStream = new( );
            imagearr.Save(memoryStream, ImageFormat.Jpeg);
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Position = 0L;
            memoryStream.Read(array, 0, (int) memoryStream.Length);
            memoryStream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            httpWebRequest.CookieContainer = new CookieContainer( );
            httpWebRequest.GetResponse( ).Close( );
            HttpWebRequest httpWebRequest2 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            httpWebRequest2.Method = "POST";
            httpWebRequest2.Referer = "http://ai.baidu.com/tech/ocr/general";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest2.Timeout = 8000;
            httpWebRequest2.ReadWriteTimeout = 5000;
            httpWebRequest2.Headers.Add("Cookie:" + CookieCollectionToStrCookie(((HttpWebResponse) httpWebRequest.GetResponse( )).Cookies));
            using (Stream requestStream = httpWebRequest2.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest2.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string text4 = "";
            string[] array2 = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text4 += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                array2[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            string text5 = "";
            for (int j = 0; j < array2.Length; j++)
            {
                text5 += array2[j];
            }
            OCR_baidu_c = (OCR_baidu_c + text4 + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private void baidu_image_c(object objEvent)
    {
        try
        {
            for (int i = image_num[1]; i < image_num[2]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OCR_baidu_use_C(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            exit_thread( );
        }
    }

    private void baidu_image_d(object objEvent)
    {
        try
        {
            for (int i = image_num[2]; i < image_num[3]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OCR_baidu_use_D(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            exit_thread( );
        }
    }

    private void baidu_image_e(object objEvent)
    {
        try
        {
            for (int i = image_num[3]; i < image_num[4]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OCR_baidu_use_E(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            exit_thread( );
        }
    }

    private void bool_image_count(int num)
    {
        if (num >= 5)
        {
            image_num = new int[num];
            if (num - num / 5 * 5 == 0)
            {
                image_num[0] = num / 5;
                image_num[1] = num / 5 * 2;
                image_num[2] = num / 5 * 3;
                image_num[3] = num / 5 * 4;
                image_num[4] = num;
            }
            if (num - num / 5 * 5 == 1)
            {
                image_num[0] = num / 5 + 1;
                image_num[1] = num / 5 * 2;
                image_num[2] = num / 5 * 3;
                image_num[3] = num / 5 * 4;
                image_num[4] = num;
            }
            if (num - num / 5 * 5 == 2)
            {
                image_num[0] = num / 5 + 1;
                image_num[1] = num / 5 * 2 + 1;
                image_num[2] = num / 5 * 3;
                image_num[3] = num / 5 * 4;
                image_num[4] = num;
            }
            if (num - num / 5 * 5 == 3)
            {
                image_num[0] = num / 5 + 1;
                image_num[1] = num / 5 * 2 + 1;
                image_num[2] = num / 5 * 3 + 1;
                image_num[3] = num / 5 * 4;
                image_num[4] = num;
            }
            if (num - num / 5 * 5 == 4)
            {
                image_num[0] = num / 5 + 1;
                image_num[1] = num / 5 * 2 + 1;
                image_num[2] = num / 5 * 3 + 1;
                image_num[3] = num / 5 * 4 + 1;
                image_num[4] = num;
            }
        }
        if (num == 4)
        {
            image_num = new int[5];
            image_num[0] = 1;
            image_num[1] = 2;
            image_num[2] = 3;
            image_num[3] = 4;
            image_num[4] = 0;
        }
        if (num == 3)
        {
            image_num = new int[5];
            image_num[0] = 1;
            image_num[1] = 2;
            image_num[2] = 3;
            image_num[3] = 0;
            image_num[4] = 0;
        }
        if (num == 2)
        {
            image_num = new int[5];
            image_num[0] = 1;
            image_num[1] = 2;
            image_num[2] = 0;
            image_num[3] = 0;
            image_num[4] = 0;
        }
        if (num == 1)
        {
            image_num = new int[5];
            image_num[0] = 1;
            image_num[1] = 0;
            image_num[2] = 0;
            image_num[3] = 0;
            image_num[4] = 0;
        }
        if (num == 0)
        {
            image_num = new int[5];
            image_num[0] = 0;
            image_num[1] = 0;
            image_num[2] = 0;
            image_num[3] = 0;
            image_num[4] = 0;
        }
    }

    private void exit_thread( )
    {
        try
        {
            StaticValue.截图排斥 = false;
            esc = "退出";
            fmloading.fml_close = "窗体已关闭";
            esc_thread.Abort( );
        }
        catch
        {
        }
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        Show( );
        WindowState = FormWindowState.Normal;
        if (IniHelp.GetValue("快捷键", "翻译文本") != "请按下快捷键")
        {
            string value = IniHelp.GetValue("快捷键", "翻译文本");
            string text = "None";
            string text2 = "F9";
            SetHotkey(text, text2, value, 205);
        }
        UnregisterHotKey(Handle, 222);
    }

    private static void SetGlobalProxy( ) => WebRequest.DefaultWebProxy = null;

    private void tray_null_Proxy_Click(object o, EventArgs e)
    {
        null_Proxy.Text = "不使用代理√";
        customize_Proxy.Text = "自定义代理";
        system_Proxy.Text = "系统代理";
        Proxy_flag = "关闭";
        WebRequest.DefaultWebProxy = null;
    }

    private void tray_system_Proxy_Click(object o, EventArgs e)
    {
        null_Proxy.Text = "不使用代理";
        customize_Proxy.Text = "自定义代理";
        system_Proxy.Text = "系统代理√";
        Proxy_flag = "系统";
        WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy( );
    }

    private void change_pinyin_Click(object o, EventArgs e)
    {
        pinyin_flag = true;
        transtalate_Click( );
    }

    private static string Post_Html_pinyin(string url, string post_str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(post_str);
        string text = "";
        HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.Timeout = 6000;
        httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        try
        {
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            StreamReader streamReader = new(responseStream, Encoding.GetEncoding("utf-8"));
            text = streamReader.ReadToEnd( );
            responseStream.Close( );
            streamReader.Close( );
            httpWebRequest.Abort( );
        }
        catch
        {
        }
        return text;
    }

    private static Bitmap ZoomImage(Bitmap bitmap1, int destHeight, int destWidth)
    {
        double num = bitmap1.Width;
        double num2 = bitmap1.Height;
        if (num < destHeight)
        {
            while (num < destHeight)
            {
                num2 *= 1.1;
                num *= 1.1;
            }
        }
        if (num2 < destWidth)
        {
            while (num2 < destWidth)
            {
                num2 *= 1.1;
                num *= 1.1;
            }
        }
        int num3 = (int) num;
        int num4 = (int) num2;
        Bitmap bitmap2 = new(num3, num4);
        Graphics graphics = Graphics.FromImage(bitmap2);
        graphics.DrawImage(bitmap1, 0, 0, num3, num4);
        graphics.Save( );
        graphics.Dispose( );
        return new Bitmap(bitmap2);
    }

    private void 翻译文本( )
    {
        if (IniHelp.GetValue("配置", "快速翻译") == "True")
        {
            string text = "";
            try
            {
                trans_hotkey = TextUtils.GetTextFromClipboard( );
                if (IniHelp.GetValue("配置", "翻译接口") == "谷歌")
                {
                    text = Translate_Google(trans_hotkey);
                }
                if (IniHelp.GetValue("配置", "翻译接口") == "百度")
                {
                    text = TranslateBaidu(trans_hotkey);
                }
                if (IniHelp.GetValue("配置", "翻译接口") == "腾讯")
                {
                    text = Translate_Tencent(trans_hotkey);
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
        SendKeys.Flush( );
        RichBoxBody.Text = Clipboard.GetText( );
        transtalate_Click( );
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        SetForegroundWindow(StaticValue.mainhandle);
        Show( );
        WindowState = FormWindowState.Normal;
        if (IniHelp.GetValue("工具栏", "顶置") == "True")
        {
            TopMost = true;
            return;
        }
        TopMost = false;
    }

    private static Rectangle[] GetRects(Bitmap pic)
    {
        List<Rectangle> list = new( );
        bool[][] colors = getColors(pic);
        for (int i = 0; i < pic.Height; i++)
        {
            for (int j = 0; j < pic.Width; j++)
            {
                if (Exist(colors, i, j))
                {
                    Rectangle rect = GetRect(colors, i, j);
                    if (rect.Width > 10 && rect.Height > 10)
                    {
                        list.Add(rect);
                    }
                }
            }
        }
        return list.ToArray( );
    }

    private static Bitmap GetRect(Image pic, Rectangle Rect)
    {
        Rectangle rectangle = new(0, 0, Rect.Width, Rect.Height);
        Bitmap bitmap = new(rectangle.Width, rectangle.Height);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(0, 0, 0, 0));
        graphics.DrawImage(pic, rectangle, Rect, GraphicsUnit.Pixel);
        graphics.Dispose( );
        return bitmap;
    }

    private static Bitmap[] getSubPics(Image buildPic, Rectangle[] buildRects)
    {
        Bitmap[] array = new Bitmap[buildRects.Length];
        for (int i = 0; i < buildRects.Length; i++)
        {
            array[i] = GetRect(buildPic, buildRects[i]);
            string text = IniHelp.GetValue("配置", "截图位置") + "\\" + TextUtils.RenameFile(IniHelp.GetValue("配置", "截图位置"), "图片.Png");
            array[i].Save(text, ImageFormat.Png);
        }
        return array;
    }

    private static bool[][] getColors(Bitmap pic)
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
                array[i][j] = pixel.A >= 3 && (num < 2 || pixel.A >= 30);
            }
        }
        return array;
    }

    private static bool Exist(bool[][] Colors, int x, int y) => x >= 0 && y >= 0 && x < Colors.Length && y < Colors[0].Length && Colors[x][y];

    private static bool R_Exist(bool[][] Colors, Rectangle Rect)
    {
        if (Rect.Right >= Colors[0].Length || Rect.Left < 0)
        {
            return false;
        }
        for (int i = 0; i < Rect.Height; i++)
        {
            if (Exist(Colors, Rect.Top + i, Rect.Right + 1))
            {
                return true;
            }
        }
        return false;
    }

    private static bool D_Exist(bool[][] Colors, Rectangle Rect)
    {
        if (Rect.Bottom >= Colors.Length || Rect.Top < 0)
        {
            return false;
        }
        for (int i = 0; i < Rect.Width; i++)
        {
            if (Exist(Colors, Rect.Bottom + 1, Rect.Left + i))
            {
                return true;
            }
        }
        return false;
    }

    private static bool L_Exist(bool[][] Colors, Rectangle Rect)
    {
        if (Rect.Right >= Colors[0].Length || Rect.Left < 0)
        {
            return false;
        }
        for (int i = 0; i < Rect.Height; i++)
        {
            if (Exist(Colors, Rect.Top + i, Rect.Left - 1))
            {
                return true;
            }
        }
        return false;
    }

    private static bool U_Exist(bool[][] Colors, Rectangle Rect)
    {
        if (Rect.Bottom >= Colors.Length || Rect.Top < 0)
        {
            return false;
        }
        for (int i = 0; i < Rect.Width; i++)
        {
            if (Exist(Colors, Rect.Top - 1, Rect.Left + i))
            {
                return true;
            }
        }
        return false;
    }

    private static Rectangle GetRect(bool[][] Colors, int x, int y)
    {
        Rectangle rectangle = new(new Point(y, x), new Size(1, 1));
        bool flag;
        int num;
        do
        {
            flag = false;
            while (R_Exist(Colors, rectangle))
            {
                num = rectangle.Width;
                rectangle.Width = num + 1;
                flag = true;
            }
            while (D_Exist(Colors, rectangle))
            {
                num = rectangle.Height;
                rectangle.Height = num + 1;
                flag = true;
            }
            while (L_Exist(Colors, rectangle))
            {
                num = rectangle.Width;
                rectangle.Width = num + 1;
                num = rectangle.X;
                rectangle.X = num - 1;
                flag = true;
            }
            while (U_Exist(Colors, rectangle))
            {
                num = rectangle.Height;
                rectangle.Height = num + 1;
                num = rectangle.Y;
                rectangle.Y = num - 1;
                flag = true;
            }
        }
        while (flag);
        clearRect(Colors, rectangle);
        num = rectangle.Width;
        rectangle.Width = num + 1;
        num = rectangle.Height;
        rectangle.Height = num + 1;
        return rectangle;
    }

    private static void clearRect(bool[][] Colors, Rectangle Rect)
    {
        for (int i = Rect.Top; i <= Rect.Bottom; i++)
        {
            for (int j = Rect.Left; j <= Rect.Right; j++)
            {
                Colors[i][j] = false;
            }
        }
    }

    private static string ReFileNamekey(string strFilePath)
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
            array[i] = GetRect(buildPic, buildRects[i]);
            image_screen = array[i];
            Messageload messageload = new( );
            messageload.ShowDialog( );
            if (messageload.DialogResult == DialogResult.OK)
            {
                if (interface_flag == "搜狗")
                {
                    OCR_sougou2( );
                }
                if (interface_flag == "腾讯")
                {
                    OcrTencent( );
                }
                if (interface_flag == "有道")
                {
                    OcrYoudao( );
                }
                if (interface_flag is "日语" or "中英" or "韩语")
                {
                    OCR_baidu( );
                }
                messageload.Dispose( );
            }
            if (IniHelp.GetValue("工具栏", "分栏") == "True")
            {
                if (paragraph)
                {
                    text = text + "\r\n" + typeset_txt.Trim( );
                    text2 = text2 + "\r\n" + SplitedText.Trim( ) + "\r\n";
                }
                else
                {
                    text += typeset_txt.Trim( );
                    text2 = text2 + "\r\n" + SplitedText.Trim( ) + "\r\n";
                }
            }
            else if (paragraph)
            {
                text = text + "\r\n" + typeset_txt.Trim( ) + "\r\n";
                text2 = text2 + "\r\n" + SplitedText.Trim( ) + "\r\n";
            }
            else
            {
                text = text + typeset_txt.Trim( ) + "\r\n";
                text2 = text2 + "\r\n" + SplitedText.Trim( ) + "\r\n";
            }
        }
        typeset_txt = text.Replace("\r\n\r\n", "\r\n");
        SplitedText = text2.Replace("\r\n\r\n", "\r\n");
        fmloading.fml_close = "窗体已关闭";
        Invoke(new ocr_thread(Main_OCR_Thread_last));
        return array;
    }

    private void OCR_sougou_bat(Bitmap image_screen)
    {
        try
        {
            SplitedText = "";
            string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
            Image image = ZoomImage(image_screen, 120, 120);
            byte[] array = OCR_ImgToByte(image);
            string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = "\r\n" + text + "--\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(text2);
            byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
            byte[] array2 = Mergebyte(bytes, array, bytes2);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("http://ocr.shouji.sogou.com/v2/ocr/json");
            httpWebRequest.Timeout = 8000;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + text.Substring(2);
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(array2, 0, array2.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text4 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text4))["result"].ToString( ));
            checked_txt(jarray, 2, "content");
            image.Dispose( );
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private static Image BoundingBox_fences(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        Image image2;
        using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new( ))
        {
            CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
            Image image = draw.ToBitmap( );
            Graphics graphics = Graphics.FromImage(image);
            int size = vectorOfVectorOfPoint.Size;
            for (int i = 0; i < size; i++)
            {
                using VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i];
                Rectangle rectangle = CvInvoke.BoundingRectangle(vectorOfPoint);
                int x = rectangle.Location.X;
                int y = rectangle.Location.Y;
                int width = rectangle.Size.Width;
                int height = rectangle.Size.Height;
                graphics.FillRectangle(Brushes.White, x, 0, width, draw.Height);
            }
            graphics.Dispose( );
            Bitmap bitmap = new(image.Width + 2, image.Height + 2);
            Graphics graphics2 = Graphics.FromImage(bitmap);
            graphics2.DrawImage(image, 1, 1, image.Width, image.Height);
            graphics2.Save( );
            graphics2.Dispose( );
            image.Dispose( );
            src.Dispose( );
            image2 = bitmap;
        }
        return image2;
    }

    private Image FindBundingBox_fences(Bitmap bitmap)
    {
        Image<Bgr, byte> image = new(bitmap);
        Image<Gray, byte> image2 = new(image.Width, image.Height);
        CvInvoke.CvtColor(image, image2, ColorConversion.Bgra2Gray, 0);
        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(6, 20), new Point(1, 1));
        CvInvoke.Erode(image2, image2, structuringElement, new Point(0, 2), 1, BorderType.Reflect101, default);
        CvInvoke.Threshold(image2, image2, 100.0, 255.0, (ThresholdType) 9);
        Image<Gray, byte> image3 = new(image2.ToBitmap( ));
        Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>( );
        Image<Gray, byte> image5 = image3.Clone( );
        CvInvoke.Canny(image3, image5, 255.0, 255.0, 5, true);
        Image image6 = BoundingBox_fences(image5, image4);
        Image<Gray, byte> image7 = new((Bitmap) image6);
        BoundingBoxFencesUp(image7);
        image.Dispose( );
        image2.Dispose( );
        image3.Dispose( );
        image7.Dispose( );
        return image6;
    }

    private void BoundingBoxFencesUp(Image<Gray, byte> src)
    {
        using VectorOfVectorOfPoint vectorOfVectorOfPoint = new( );
        CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
        int size = vectorOfVectorOfPoint.Size;
        Rectangle[] array = new Rectangle[size];
        for (int i = 0; i < size; i++)
        {
            using VectorOfPoint vectorOfPoint = vectorOfVectorOfPoint[i];
            array[size - 1 - i] = CvInvoke.BoundingRectangle(vectorOfPoint);
        }
        getSubPics_ocr(image_screen, array);
    }

    private void CheckedLocationTxt(JArray jarray, int lastlength, string words)
    {
        int num = 0;
        for (int i = 0; i < jarray.Count; i++)
        {
            int length = JObject.Parse(jarray[i].ToString( ))[words].ToString( ).Length;
            if (length > num)
            {
                num = length;
            }
        }
        string text = "";
        string text2 = "";
        for (int j = 0; j < jarray.Count - 1; j++)
        {
            JObject jobject = JObject.Parse(jarray[j].ToString( ));
            char[] array = jobject[words].ToString( ).ToCharArray( );
            JObject jobject2 = JObject.Parse(jarray[j + 1].ToString( ));
            char[] array2 = jobject2[words].ToString( ).ToCharArray( );
            int length2 = jobject[words].ToString( ).Length;
            int length3 = jobject2[words].ToString( ).Length;
            if (Math.Abs(length2 - length3) <= 0)
            {
                if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && Is_punctuation(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else
                {
                    text2 += jobject[words].ToString( ).Trim( );
                }
            }
            else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && Math.Abs(length2 - length3) <= 1)
            {
                if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else if (TextUtils.IsSplited(array[array.Length - lastlength].ToString( )) && Is_punctuation(array2[0].ToString( )))
                {
                    text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
                }
                else
                {
                    text2 += jobject[words].ToString( ).Trim( );
                }
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && length2 <= num / 2)
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )) && length3 - length2 < 4 && array2[1].ToString( ) == ".")
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (TextUtils.ContainEn(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + " ";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (TextUtils.ContainEn(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && Is_punctuation(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (Is_punctuation(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (Is_punctuation(array[array.Length - lastlength].ToString( )) && TextUtils.ContainEn(array2[0].ToString( )))
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + " ";
            }
            else if (TextUtils.ContainsZh(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (IsNum(array[array.Length - lastlength].ToString( )) && TextUtils.ContainsZh(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else if (IsNum(array[array.Length - lastlength].ToString( )) && IsNum(array2[0].ToString( )))
            {
                text2 += jobject[words].ToString( ).Trim( );
            }
            else
            {
                text2 = text2 + jobject[words].ToString( ).Trim( ) + "\r\n";
            }
            if (has_punctuation(jobject[words].ToString( )))
            {
                text2 += "\r\n";
            }
            text = text + jobject[words].ToString( ).Trim( ) + "\r\n";
        }
        SplitedText = text + JObject.Parse(jarray[jarray.Count - 1].ToString( ))[words];
        typeset_txt = text2.Replace("\r\n\r\n", "\r\n") + JObject.Parse(jarray[jarray.Count - 1].ToString( ))[words];
    }

    private void checked_location_sougou(JArray jarray, int lastlength, string words, string location)
    {
        paragraph = false;
        int num = 20000;
        int num2 = 0;
        for (int i = 0; i < jarray.Count; i++)
        {
            JObject jobject = JObject.Parse(jarray[i].ToString( ));
            int num3 = split_char_x(jobject[location][1].ToString( )) - split_char_x(jobject[location][0].ToString( ));
            if (num3 > num2)
            {
                num2 = num3;
            }
            int num4 = split_char_x(jobject[location][0].ToString( ));
            if (num4 < num)
            {
                num = num4;
            }
        }
        JObject jobject2 = JObject.Parse(jarray[0].ToString( ));
        if (Math.Abs(split_char_x(jobject2[location][0].ToString( )) - num) > 10)
        {
            paragraph = true;
        }
        string text = "";
        string text2 = "";
        for (int j = 0; j < jarray.Count; j++)
        {
            JObject jobject3 = JObject.Parse(jarray[j].ToString( ));
            char[] array = jobject3[words].ToString( ).ToCharArray( );
            JObject jobject4 = JObject.Parse(jarray[j].ToString( ));
            bool flag = Math.Abs(split_char_x(jobject4[location][1].ToString( )) - split_char_x(jobject4[location][0].ToString( )) - num2) > 20;
            bool flag2 = Math.Abs(split_char_x(jobject4[location][0].ToString( )) - num) > 10;
            if (flag && flag2)
            {
                text = text.Trim( ) + "\r\n" + jobject4[words].ToString( ).Trim( );
            }
            else if (IsNum(array[0].ToString( )) && !TextUtils.ContainsZh(array[1].ToString( )) && flag)
            {
                text = text.Trim( ) + "\r\n" + jobject4[words].ToString( ).Trim( ) + "\r\n";
            }
            else
            {
                text += jobject4[words].ToString( ).Trim( );
            }
            if (TextUtils.ContainEn(array[array.Length - lastlength].ToString( )))
            {
                text = text + jobject3[words].ToString( ).Trim( ) + " ";
            }
            text2 = text2 + jobject4[words].ToString( ).Trim( ) + "\r\n";
        }
        SplitedText = text2.Replace("\r\n\r\n", "\r\n");
        typeset_txt = text;
    }

    private static int split_char_x(string split_char) => Convert.ToInt32(split_char.Split(new char[] { ',' })[0]);

    private void tray_double_Click(object o, EventArgs e)
    {
        UnregisterHotKey(Handle, 205);
        menu.Hide( );
        RichBoxBody.Hide = "";
        RichBoxBody_T.Hide = "";
        Main_OCR_Quickscreenshots( );
    }

    private static int en_count(string text) => Regex.Matches(text, "\\s+").Count + 1;

    private static int ch_count(string str)
    {
        int num = 0;
        Regex regex = new("^[\\u4E00-\\u9FA5]{0,}$");
        for (int i = 0; i < str.Length; i++)
        {
            if (regex.IsMatch(str[i].ToString( )))
            {
                num++;
            }
        }
        return num;
    }

    private void checked_location_youdao(JArray jarray, int lastlength, string words, string location)
    {
        paragraph = false;
        int num = 20000;
        int num2 = 0;
        for (int i = 0; i < jarray.Count; i++)
        {
            JObject jobject = JObject.Parse(jarray[i].ToString( ));
            int num3 = split_char_youdao(jobject[location].ToString( ), 3) - split_char_youdao(jobject[location].ToString( ), 1);
            if (num3 > num2)
            {
                num2 = num3;
            }
            int num4 = split_char_youdao(jobject[location].ToString( ), 1);
            if (num4 < num)
            {
                num = num4;
            }
        }
        JObject jobject2 = JObject.Parse(jarray[0].ToString( ));
        if (Math.Abs(split_char_youdao(jobject2[location].ToString( ), 1) - num) > 10)
        {
            paragraph = true;
        }
        string text = "";
        string text2 = "";
        for (int j = 0; j < jarray.Count; j++)
        {
            JObject jobject3 = JObject.Parse(jarray[j].ToString( ));
            char[] array = jobject3[words].ToString( ).ToCharArray( );
            JObject jobject4 = JObject.Parse(jarray[j].ToString( ));
            bool flag = Math.Abs(split_char_youdao(jobject4[location].ToString( ), 3) - split_char_youdao(jobject4[location].ToString( ), 1) - num2) > 20;
            bool flag2 = Math.Abs(split_char_youdao(jobject4[location].ToString( ), 1) - num) > 10;
            if (flag && flag2)
            {
                text = text.Trim( ) + "\r\n" + jobject4[words].ToString( ).Trim( );
            }
            else if (IsNum(array[0].ToString( )) && !TextUtils.ContainsZh(array[1].ToString( )) && flag)
            {
                text = text.Trim( ) + "\r\n" + jobject4[words].ToString( ).Trim( ) + "\r\n";
            }
            else
            {
                text += jobject4[words].ToString( ).Trim( );
            }
            if (TextUtils.ContainEn(array[array.Length - lastlength].ToString( )))
            {
                text = text + jobject3[words].ToString( ).Trim( ) + " ";
            }
            text2 = text2 + jobject4[words].ToString( ).Trim( ) + "\r\n";
        }
        SplitedText = text2.Replace("\r\n\r\n", "\r\n");
        typeset_txt = text;
    }

    private static int split_char_youdao(string split_char, int i) => Convert.ToInt32(split_char.Split(new char[] { ',' })[i - 1]);

    private void Trans_google_Click(object o, EventArgs e) => Trans_foreach("谷歌");

    private void Trans_baidu_Click(object o, EventArgs e) => Trans_foreach("百度");

    private void Trans_foreach(string name)
    {
        if (name == "百度")
        {
            trans_baidu.Text = "百度√";
            trans_google.Text = "谷歌";
            trans_tencent.Text = "腾讯";
            IniHelp.SetValue("配置", "翻译接口", "百度");
        }
        if (name == "谷歌")
        {
            trans_baidu.Text = "百度";
            trans_google.Text = "谷歌√";
            trans_tencent.Text = "腾讯";
            IniHelp.SetValue("配置", "翻译接口", "谷歌");
        }
        if (name == "腾讯")
        {
            trans_google.Text = "谷歌";
            trans_baidu.Text = "百度";
            trans_tencent.Text = "腾讯√";
            IniHelp.SetValue("配置", "翻译接口", "腾讯");
        }
    }

    private string GetBaiduHtml(string url, CookieContainer cookie, string refer, string content_length)
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
            Stream requestStream = httpWebRequest.GetRequestStream( );
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close( );
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                using StreamReader streamReader = new(httpWebResponse.GetResponseStream( ), Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            text2 = text;
        }
        catch
        {
            text2 = GetBaiduHtml(url, cookie, refer, content_length);
        }
        return text2;
    }

    private static string TranslateBaidu(string Text)
    {
        string text = "";
        try
        {
            new CookieContainer( );
            string text2 = "zh";
            string text3 = "en";
            if (StaticValue.zh_en)
            {
                if (ch_count(Text.Trim( )) > en_count(Text.Trim( )) || (en_count(text.Trim( )) == 1 && ch_count(text.Trim( )) == 1))
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
                if (TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(Text.Trim( )))))
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
                if (TextUtils.ContainKor(Text.Trim( )))
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
            HttpHelper httpHelper = new( );
            HttpItem httpItem = new( )
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
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(httpHelper.GetHtml(httpItem).Html))["trans"].ToString( ));
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text = text + jobject["dst"] + "\r\n";
            }
        }
        catch (Exception)
        {
            text = "[百度接口报错]：\r\n1.接口请求出现问题等待修复。";
        }
        return text;
    }

    private void Trans_tencent_Click(object o, EventArgs e) => Trans_foreach("腾讯");

    private static string Content_Length(string text, string fromlang, string tolang) => string.Concat(new string[]
        {
            "&source=",
            fromlang,
            "&target=",
            tolang,
            "&sourceText=",
            HttpUtility.UrlEncode(text).Replace("+", "%20")
        });

    private string TencentPOST(string url, string content)
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
            httpWebRequest.Headers.Add("cookie:" + GetCookies("http://fanyi.qq.com"));
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            httpWebRequest.ContentLength = bytes.Length;
            Stream requestStream = httpWebRequest.GetRequestStream( );
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close( );
            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse( ))
            {
                using StreamReader streamReader = new(httpWebResponse.GetResponseStream( ), Encoding.UTF8);
                text = streamReader.ReadToEnd( );
                streamReader.Close( );
                httpWebResponse.Close( );
            }
            text2 = text;
            if (text.Contains("\"records\":[]"))
            {
                Thread.Sleep(8);
                return TencentPOST(url, content);
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
                if (ch_count(strtrans.Trim( )) > en_count(strtrans.Trim( )) || (en_count(text.Trim( )) == 1 && ch_count(text.Trim( )) == 1))
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
                if (TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(strtrans.Trim( )))))
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
                if (TextUtils.ContainKor(strtrans.Trim( )))
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
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(TencentPOST("https://fanyi.qq.com/api/translate", Content_Length(strtrans, text2, text3))))["translate"]["records"].ToString( ));
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                text += jobject["targetText"].ToString( );
            }
        }
        catch (Exception)
        {
            text = "[腾讯接口报错]：\r\n1.接口请求出现问题等待修复。";
        }
        return text;
    }
    private static string GetCookies(string url)
    {
        uint num = 1024U;
        StringBuilder stringBuilder = new((int) num);
        if (!InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, IntPtr.Zero))
        {
            if (num < 0U)
            {
                return null;
            }
            stringBuilder = new StringBuilder((int) num);
            if (!InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, IntPtr.Zero))
            {
                return null;
            }
        }
        return stringBuilder.ToString( );
    }

    private static string Post_GoogletHtml(string post_str)
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
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            StreamReader streamReader = new(responseStream, Encoding.GetEncoding("utf-8"));
            text = streamReader.ReadToEnd( );
            responseStream.Close( );
            streamReader.Close( );
            httpWebRequest.Abort( );
        }
        catch
        {
        }
        return text;
    }

    private static void httpDownload(string URL, string filename)
    {
        try
        {
            HttpWebResponse httpWebResponse = (HttpWebResponse) ((HttpWebRequest) WebRequest.Create(URL)).GetResponse( );
            long contentLength = httpWebResponse.ContentLength;
            Stream responseStream = httpWebResponse.GetResponseStream( );
            Stream stream = new FileStream(filename, FileMode.Create);
            long num = 0L;
            byte[] array = new byte[2048];
            for (int i = responseStream.Read(array, 0, array.Length); i > 0; i = responseStream.Read(array, 0, array.Length))
            {
                num = i + num;
                stream.Write(array, 0, i);
            }
            stream.Close( );
            responseStream.Close( );
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void OCR_baidu_table( )
    {
        typeset_txt = "[消息]：表格已下载！";
        SplitedText = "";
        try
        {
            baidu_vip = Get_html(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + StaticValue.baiduAPI_ID + "&client_secret=" + StaticValue.baiduAPI_key));
            if (string.IsNullOrEmpty(baidu_vip))
            {
                MessageBox.Show("请检查密钥输入是否正确！", "提醒");
            }
            else
            {
                SplitedText = "";
                Image image = image_screen;
                MemoryStream memoryStream = new( );
                image.Save(memoryStream, ImageFormat.Jpeg);
                byte[] array = new byte[memoryStream.Length];
                memoryStream.Position = 0L;
                memoryStream.Read(array, 0, (int) memoryStream.Length);
                memoryStream.Close( );
                string text = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array));
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://aip.baidubce.com/rest/2.0/solution/v1/form_ocr/request?access_token=" + ((JObject) JsonConvert.DeserializeObject(baidu_vip))["access_token"]);
                httpWebRequest.Proxy = null;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Timeout = 8000;
                httpWebRequest.ReadWriteTimeout = 5000;
                using (Stream requestStream = httpWebRequest.GetRequestStream( ))
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
                string text2 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
                responseStream.Close( );
                string text3 = "request_id=" + JObject.Parse(JArray.Parse(((JObject) JsonConvert.DeserializeObject(text2))["result"].ToString( ))[0].ToString( ))["request_id"].ToString( ).Trim( ) + "&result_type=json";
                string text4 = "";
                while (!text4.Contains("已完成"))
                {
                    if (text4.Contains("image recognize error"))
                    {
                        RichBoxBody.Text = "[消息]：未发现表格！";
                        break;
                    }
                    Thread.Sleep(120);
                    text4 = Post_Html("https://aip.baidubce.com/rest/2.0/solution/v1/form_ocr/get_request_result?access_token=" + ((JObject) JsonConvert.DeserializeObject(baidu_vip))["access_token"], text3);
                }
                if (!text4.Contains("image recognize error"))
                {
                    get_table(text4);
                }
            }
        }
        catch
        {
            RichBoxBody.Text = "[消息]：免费百度密钥50次已经耗完！请更换自己的密钥继续使用！";
        }
    }

    private void OCR_table_Click(object o, EventArgs e) => OcrForeach("表格");

    private void get_table(string str)
    {
        JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(((JObject) JsonConvert.DeserializeObject(str))["result"]["result_data"].ToString( ).Replace("\\", "")))["forms"][0]["body"].ToString( ));
        int[] array = new int[jarray.Count];
        int[] array2 = new int[jarray.Count];
        for (int i = 0; i < jarray.Count; i++)
        {
            JObject jobject = JObject.Parse(jarray[i].ToString( ));
            string text = jobject["column"].ToString( ).Replace("[", "").Replace("]", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim( );
            string text2 = jobject["row"].ToString( ).Replace("[", "").Replace("]", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim( );
            array[i] = Convert.ToInt32(text);
            array2[i] = Convert.ToInt32(text2);
        }
        string[,] array3 = new string[array2.Max( ) + 1, array.Max( ) + 1];
        for (int j = 0; j < jarray.Count; j++)
        {
            JObject jobject2 = JObject.Parse(jarray[j].ToString( ));
            string text3 = jobject2["column"].ToString( ).Replace("[", "").Replace("]", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim( );
            string text4 = jobject2["row"].ToString( ).Replace("[", "").Replace("]", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim( );
            array[j] = Convert.ToInt32(text3);
            array2[j] = Convert.ToInt32(text4);
            string text5 = jobject2["word"].ToString( ).Replace("[", "").Replace("]", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim( );
            array3[Convert.ToInt32(text4), Convert.ToInt32(text3)] = text5;
        }
        Graphics graphics = CreateGraphics( );
        int[] array4 = new int[array.Max( ) + 1];
        int num = 0;
        new SizeF(10f, 10f);
        int num2 = Screen.PrimaryScreen.Bounds.Width / 4;
        for (int k = 0; k < array3.GetLength(1); k++)
        {
            for (int l = 0; l < array3.GetLength(0); l++)
            {
                SizeF sizeF = graphics.MeasureString(array3[l, k], new Font("宋体", 12f));
                if (num < (int) sizeF.Width)
                {
                    num = (int) sizeF.Width;
                }
                if (num > num2)
                {
                    num = num2;
                }
            }
            array4[k] = num;
            num = 0;
        }
        graphics.Dispose( );
        setClipboard_Table(array3, array4);
    }

    private void Main_OCR_Thread_table( )
    {
        ailibaba = new AliTable( );
        TimeSpan timeSpan = new(DateTime.Now.Ticks);
        TimeSpan timeSpan2 = timeSpan.Subtract(ts).Duration( );
        string text = string.Concat(new string[]
        {
                timeSpan2.Seconds.ToString(),
                ".",
                Convert.ToInt32(timeSpan2.TotalMilliseconds).ToString(),
                "秒"
        });
        TopMost = StaticValue.v_topmost;
        Text = "耗时：" + text;
        if (interface_flag == "百度表格")
        {
            DataObject dataObject = new( );
            dataObject.SetData(DataFormats.Rtf, RichBoxBody.rtf);
            dataObject.SetData(DataFormats.UnicodeText, RichBoxBody.Text);
            RichBoxBody.Text = "[消息]：表格已复制到粘贴板！";
            Clipboard.SetDataObject(dataObject);
        }
        image_screen.Dispose( );
        GC.Collect( );
        StaticValue.截图排斥 = false;
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        Show( );
        WindowState = FormWindowState.Normal;
        Size = new Size(form_width, form_height);
        SetForegroundWindow(Handle);
        if (interface_flag == "阿里表格")
        {
            if (SplitedText == "弹出cookie")
            {
                SplitedText = "";
                ailibaba.TopMost = true;
                ailibaba.GetCookie = "";
                IniHelp.SetValue("特殊", "ali_cookie", ailibaba.GetCookie);
                ailibaba.ShowDialog( );
                SetForegroundWindow(ailibaba.Handle);
                return;
            }
            Clipboard.SetDataObject(typeset_txt);
            CopyHtmlToClipBoard(typeset_txt);
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
                text2 = k == 0 ? text2 + "\\fs24 " + wordo[j, k] : text2 + "\\cell " + wordo[j, k];
            }
            if (j != wordo.GetLength(0) - 1)
            {
                text2 += "\\row\\intbl";
            }
        }
        RichBoxBody.rtf = text + text3 + text2 + text4;
    }

    private string Translate_Googlekey(string text)
    {
        string text2 = "";
        try
        {
            string text3 = "zh-CN";
            string text4 = "en";
            if (StaticValue.zh_en)
            {
                if (ch_count(typeset_txt.Trim( )) > en_count(typeset_txt.Trim( )))
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
                if (TextUtils.ContainJap(TextUtils.RepalceStr(TextUtils.RemoveZh(typeset_txt.Trim( )))))
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
                if (TextUtils.ContainKor(typeset_txt.Trim( )))
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
            JArray jarray = (JArray) JsonConvert.DeserializeObject(Post_GoogletHtml(text5));
            int count = ((JArray) jarray[0]).Count;
            for (int i = 0; i < count; i++)
            {
                text2 += jarray[0][i][0].ToString( );
            }
        }
        catch (Exception)
        {
            text2 = "[谷歌接口报错]：\r\n出现这个提示文字，表示您当前的网络不适合使用谷歌接口，使用方法开启设置中的系统代理，看是否可行，仍不可行的话，请自行挂VPN，多的不再说，这个问题不要再和我反馈了，个人能力有限解决不了。\r\n请放弃使用谷歌接口，腾讯，百度接口都可以正常使用。";
        }
        return text2;
    }

    private void OCR_baidutable_Click(object o, EventArgs e) => OcrForeach("百度表格");

    private void OCR_ailitable_Click(object o, EventArgs e) => OcrForeach("阿里表格");

    private new void Refresh( )
    {
        sougou.Text = "搜狗";
        tencent.Text = "腾讯";
        baidu.Text = "百度";
        youdao.Text = "有道";
        shupai.Text = "竖排";
        ocr_table.Text = "表格";
        ch_en.Text = "中英";
        jap.Text = "日语";
        kor.Text = "韩语";
        left_right.Text = "从左向右";
        righ_left.Text = "从右向左";
        baidu_table.Text = "百度";
        ali_table.Text = "阿里";
        Mathfuntion.Text = "公式";
    }

    private static byte[] ImageToByteArray(Image img) => (byte[]) new ImageConverter( ).ConvertTo(img, typeof(byte[]));

    private static Stream BytesToStream(byte[] bytes) => new MemoryStream(bytes);

    private void OCR_ali_table( )
    {
        string text = "";
        SplitedText = "";
        try
        {
            string value = IniHelp.GetValue("特殊", "ali_cookie");
            Stream stream = BytesToStream(ImageToByteArray(BWPic((Bitmap) image_screen)));
            string text2 = Convert.ToBase64String(new BinaryReader(stream).ReadBytes(Convert.ToInt32(stream.Length)));
            stream.Close( );
            string text3 = "{\n\t\"image\": \"" + text2 + "\",\n\t\"configure\": \"{\\\"format\\\":\\\"html\\\", \\\"finance\\\":false}\"\n}";
            string text4 = "https://predict-pai.data.aliyun.com/dp_experience_mall/ocr/ocr_table_parse";
            text = Post_Html_final(text4, text3, value);
            typeset_txt = ((JObject) JsonConvert.DeserializeObject(Post_Html_final(text4, text3, value)))["tables"].ToString( ).Replace("table tr td { border: 1px solid blue }", "table tr td {border: 0.5px black solid }").Replace("table { border: 1px solid blue }", "table { border: 0.5px black solid; border-collapse : collapse}\r\n");
            RichBoxBody.Text = "[消息]：表格已复制到粘贴板！";
        }
        catch
        {
            RichBoxBody.Text = "[消息]：阿里表格识别出错！";
            if (text.Contains("NEED_LOGIN"))
            {
                SplitedText = "弹出cookie";
            }
        }
    }

    private static Bitmap BWPic(Bitmap mybm)
    {
        Bitmap bitmap = new(mybm.Width, mybm.Height);
        for (int i = 0; i < mybm.Width; i++)
        {
            for (int j = 0; j < mybm.Height; j++)
            {
                Color pixel = mybm.GetPixel(i, j);
                int num = (pixel.R + pixel.G + pixel.B) / 3;
                bitmap.SetPixel(i, j, Color.FromArgb(num, num, num));
            }
        }
        return bitmap;
    }

    private static string Post_Html_final(string url, string post_str, string CookieContainer)
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
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            StreamReader streamReader = new(responseStream, Encoding.GetEncoding("utf-8"));
            text = streamReader.ReadToEnd( );
            responseStream.Close( );
            streamReader.Close( );
            httpWebRequest.Abort( );
        }
        catch
        {
        }
        return text;
    }

    private static void CopyHtmlToClipBoard(string html)
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
        DataObject dataObject = new( );
        dataObject.SetData(DataFormats.Html, new MemoryStream(utf.GetBytes(text5)));
        string text6 = new HtmlToText( ).Convert(html);
        dataObject.SetData(DataFormats.Text, text6);
        Clipboard.SetDataObject(dataObject);
    }

    private static string Encript(string functionName, object[] pams)
    {
        string text = File.ReadAllText("sign.js");
        ScriptControlClass scriptControlClass = new( );
        ((IScriptControl) scriptControlClass).Language = "javascript";
        ((IScriptControl) scriptControlClass).AddCode(text);
        return ((IScriptControl) scriptControlClass).Run(functionName, ref pams).ToString( );
    }

    private void OCR_Mathfuntion_Click(object o, EventArgs e) => OcrForeach("公式");

    private void OCR_Math( )
    {
        SplitedText = "";
        try
        {
            Image image = image_screen;
            byte[] array = OCR_ImgToByte(image);
            string text = "{\t\"formats\": [\"latex_styled\", \"text\"],\t\"metadata\": {\t\t\"count\": 1,\t\t\"platform\": \"windows 10\",\t\t\"skip_recrop\": true,\t\t\"user_id\": \"123ab2a82ea246a0b011a37183c87bab\",\t\t\"version\": \"snip.windows@00.00.0083\"\t},\t\"ocr\": [\"text\", \"math\"],\t\"src\": \"data:image/jpeg;base64," + Convert.ToBase64String(array) + "\"}";
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://api.mathpix.com/v3/latex");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Timeout = 8000;
            httpWebRequest.ReadWriteTimeout = 5000;
            httpWebRequest.Headers.Add("app_id: mathpix_chrome");
            httpWebRequest.Headers.Add("app_key: 85948264c5d443573286752fbe8df361");
            using (Stream requestStream = httpWebRequest.GetRequestStream( ))
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            Stream responseStream = ((HttpWebResponse) httpWebRequest.GetResponse( )).GetResponseStream( );
            string text2 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            responseStream.Close( );
            string text3 = "$" + ((JObject) JsonConvert.DeserializeObject(text2))["latex_styled"] + "$";
            SplitedText = text3;
            typeset_txt = text3;
        }
        catch
        {
            if (esc != "退出")
            {
                RichBoxBody.Text = "***该区域未发现文本或者密钥次数用尽***";
            }
            else
            {
                RichBoxBody.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private string interface_flag;

    private string language;

    private string SplitedText;
    private string transtalate_fla;

    private Fmloading fmloading;

    private Thread thread;
    private string googleTranslate_txt;

    private int num_ok;
    private string auto_fla;

    private string baidu_vip;

    private string htmltxt;
    private bool speaking;

    private static bool speak_copy;

    private string speak_copyb;
    private byte[] ttsData;

    private string[] pubnote;

    private Fmnote fmnote;

    private Image image_screen;

    private int voice_count;

    private int form_width;

    private int form_height;

    private bool change_QQ_screenshot;

    private readonly FmFlags fmflags;

    private string trans_hotkey;

    private TimeSpan ts;
    private Thread esc_thread;

    private string esc;
    private string typeset_txt;

    private string baidu_flags;
    private Image image_ori;

    private string shupai_Right_txt;

    private readonly AutoResetEvent are;
    private string shupai_Left_txt;
    private string OCR_baidu_a;

    private string OCR_baidu_b;
    private List<Image> imagelist;

    private int imagelist_lenght;

    private string OCR_baidu_d;

    private string OCR_baidu_c;

    private string OCR_baidu_e;

    private int[] image_num;

    private string Proxy_flag;

    private string Proxy_url;

    private string Proxy_port;

    private string Proxy_name;

    private string Proxy_password;

    private bool pinyin_flag;

    private bool set_split;

    private bool set_merge;

    private bool tranclick;
    private bool paragraph;
    private AliTable ailibaba;

    private delegate void translate( );

    private delegate void ocr_thread( );

    private delegate int Dllinput(string command);
}
