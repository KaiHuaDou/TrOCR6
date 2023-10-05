using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareX.ScreenCaptureLib;
using TrOCR.Controls;
using TrOCR.Helper;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class FmMain : Form
{
    private static bool speakCopy;
    private readonly AutoResetEvent autoResetEvent;
    private string autoFlag;
    private string baiduVip;
    private string baiduFlags;
    private bool changeQQScreenshot;
    private float dpiFactor;
    private string esc;
    private Thread escThread;
    private FmLoading fmLoading;
    private FmNote fmNote;
    private int formHeight;
    private int formWidth;
    private string googleTransText;
    private string htmltxt;
    private List<Image> imagelist;
    private int imageListLength;
    private int[] imageNum;
    private Image ImageOri;
    private Image imageScreen;
    private string interfaceFlag;
    private bool isMerged;
    private int isNumOk;
    private bool isSplited;
    private bool isTranslated;
    private string language;
    private string ocrBaidu;
    private bool paragraph;
    private bool pinyinFlag;
    private string proxyFlag;
    private string proxyName;
    private string proxyPassword;
    private string proxyPort;
    private string proxyUrl;
    private string[] pubnote;
    private string speakCopyB;
    private bool speaking;
    private string splitedText;
    private Thread thread;
    private TimeSpan timeUsed;
    private string isTransOpen;
    private string transHotkey;
    private byte[] ttsData;
    private string typeSetText;
    private string verticalLeftText;
    private string verticalRightText;
    private int voiceCount;

    public FmMain( )
    {
        isMerged = false;
        isSplited = false;
        isSplited = false;
        Globals.CaptureRejection = false;
        pinyinFlag = false;
        isTranslated = false;
        autoResetEvent = new AutoResetEvent(false);
        imagelist = new List<Image>( );
        Globals.NoteCount = Convert.ToInt32(Config.Get("配置", "记录数目"));
        baiduFlags = "";
        esc = "";
        voiceCount = 0;
        fmNote = new FmNote( );
        pubnote = new string[Globals.NoteCount];
        for (int i = 0; i < Globals.NoteCount; i++)
            pubnote[i] = "";
        Globals.Notes = pubnote;
        Globals.MainHandle = Handle;
        Font = new Font(Font.Name, 9f / Globals.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        googleTransText = "";
        isNumOk = 0;
        dpiFactor = Helper.System.DpiFactor;
        components = null;
        InitializeComponent( );
        nextClipboardViewer = (IntPtr) SetClipboardViewer((int) Handle);
        InitMinimize( );
        ReadIniFile( );
        WindowState = FormWindowState.Minimized;
        Visible = false;
        splitedText = "";
        MinimumSize = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
        speakCopy = false;
        OcrForeach("");
    }

    private delegate int DllInput(string command);

    private delegate void OcrThread( );

    private delegate void Translate( );

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams createParams = base.CreateParams;
            createParams.ExStyle |= 134217728;
            return createParams;
        }
    }

    public Bitmap[] GetSubPicOcr(Image image, Rectangle[] rects)
    {
        string text = "";
        Bitmap[] array = new Bitmap[rects.Length];
        string text2 = "";
        for (int i = 0; i < rects.Length; i++)
        {
            array[i] = ImageUtils.GetRect(image, rects[i]);
            imageScreen = array[i];
            MessageLoad messageload = new( );
            messageload.ShowDialog( );
            if (messageload.DialogResult == DialogResult.OK)
            {
                switch (interfaceFlag)
                {
                    case "搜狗": OcrSogou2( ); break;
                    case "腾讯": OcrTencent( ); break;
                    case "有道": OcrYoudao( ); break;
                    case "日语" or "中英" or "韩语": OcrBaidu( ); break;
                }
                messageload.Dispose( );
            }
            if (Config.Get("工具栏", "分栏") == "True")
            {
                if (paragraph)
                {
                    text = text + "\r\n" + typeSetText.Trim( );
                    text2 = text2 + "\r\n" + splitedText.Trim( ) + "\r\n";
                }
                else
                {
                    text += typeSetText.Trim( );
                    text2 = text2 + "\r\n" + splitedText.Trim( ) + "\r\n";
                }
            }
            else if (paragraph)
            {
                text = text + "\r\n" + typeSetText.Trim( ) + "\r\n";
                text2 = text2 + "\r\n" + splitedText.Trim( ) + "\r\n";
            }
            else
            {
                text = text + typeSetText.Trim( ) + "\r\n";
                text2 = text2 + "\r\n" + splitedText.Trim( ) + "\r\n";
            }
        }
        typeSetText = text.Replace("\r\n\r\n", "\r\n");
        splitedText = text2.Replace("\r\n\r\n", "\r\n");
        fmLoading.FormClose = "窗体已关闭";
        Invoke(new OcrThread(MainOcrThreadNormal));
        return array;
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
        if (m.Msg == 786 && (int) m.WParam == 725)
        {
            TopMost = Config.Get("工具栏", "顶置") == "True";
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 530 && richBox.Text != null)
        {
            p_note(richBox.Text);
            Globals.Notes = pubnote;
            if (fmNote.Created)
            {
                fmNote.SetTextNote( );
            }
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 520)
        {
            fmNote.Show( );
            fmNote.Focus( );
            fmNote.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - fmNote.Width, Screen.PrimaryScreen.WorkingArea.Height - fmNote.Height);
            fmNote.WindowState = FormWindowState.Normal;
            return;
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 580)
        {
            UnregisterHotKey(Handle, 205);
            changeQQScreenshot = false;
            FormBorderStyle = FormBorderStyle.None;
            Hide( );
            formWidth = isTransOpen == "开启" ? Width / 2 : Width;
            formHeight = Height;
            notifyIcon.Visible = false;
            notifyIcon.Visible = true;
            menu.Close( );
            menuCopy.Close( );
            autoFlag = "开启";
            splitedText = "";
            richBox.Text = "***该区域未发现文本***";
            richBoxTrans.Text = "";
            typeSetText = "";
            isTransOpen = "关闭";
            transClose.PerformClick( );
            Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
            FormBorderStyle = FormBorderStyle.Sizable;
            Globals.CaptureRejection = true;
            imageScreen = Globals.ImageOCR;
            if (Config.Get("工具栏", "分栏") == "True")
            {
                notifyIcon.Visible = true;
                thread = new Thread(new ThreadStart(ShowLoading));
                thread.Start( );
                timeUsed = new TimeSpan(DateTime.Now.Ticks);
                Image image = imageScreen;
                Bitmap bitmap = new(image.Width, image.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                graphics.Save( );
                graphics.Dispose( );
                ImageOri = bitmap;
                ((Bitmap) FindBundingBoxFences((Bitmap) image)).Save("Data\\分栏预览图.jpg");
            }
            else
            {
                notifyIcon.Visible = true;
                thread = new Thread(new ThreadStart(ShowLoading));
                thread.Start( );
                timeUsed = new TimeSpan(DateTime.Now.Ticks);
                MessageLoad messageload = new( );
                messageload.ShowDialog( );
                if (messageload.DialogResult == DialogResult.OK)
                {
                    escThread = new Thread(new ThreadStart(MainOcrThread));
                    escThread.Start( );
                }
            }
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 590 && speakCopyB == "朗读")
        {
            Tts( );
            return;
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 511)
        {
            base.MinimumSize = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
            isTransOpen = "关闭";
            richBox.Dock = DockStyle.Fill;
            richBoxTrans.Visible = false;
            image1.Visible = false;
            richBoxTrans.Text = "";
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 512)
        {
            TranslateClick( );
        }
        if (m.Msg == 786 && m.WParam.ToInt32( ) == 518)
        {
            if (ActiveControl.Name == "htmlTextBoxBody")
            {
                htmltxt = richBox.Text;
            }
            if (ActiveControl.Name == "rich_trans")
            {
                htmltxt = richBoxTrans.Text;
            }
            if (string.IsNullOrEmpty(htmltxt))
            {
                return;
            }
            Tts( );
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
                ExitThread( );
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 200)
            {
                UnregisterHotKey(Handle, 205);
                menu.Hide( );
                richBox.Hide = "";
                richBoxTrans.Hide = "";
                MainOcrQuickCapture( );
            }
            if (m.Msg == 786 && m.WParam.ToInt32( ) == 206)
            {
                if (!fmNote.Visible || base.Focused)
                {
                    fmNote.Show( );
                    fmNote.WindowState = FormWindowState.Normal;
                    fmNote.Visible = true;
                }
                else
                {
                    fmNote.Hide( );
                    fmNote.WindowState = FormWindowState.Minimized;
                    fmNote.Visible = false;
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
                    if (Config.Get("工具栏", "顶置") == "False")
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
                translateText( );
            }
            base.WndProc(ref m);
            return;
        }
        if (isTransOpen == "开启")
        {
            WindowState = FormWindowState.Normal;
            Size = new Size((int) fontBase.Width * 23 * 2, (int) fontBase.Height * 24);
            Location = (Point) new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10 * 2, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
            return;
        }
        WindowState = FormWindowState.Normal;
        Location = (Point) new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
        Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
        return;
    }

    private void TextFinalize(JArray jarray, string words, string text, string text2)
    {
        splitedText = text + JObject.Parse(jarray[jarray.Count - 1].ToString( ))[words];
        typeSetText = text2.Replace("\r\n\r\n", "\r\n") + JObject.Parse(jarray[jarray.Count - 1].ToString( ))[words];
    }

    private void BaiduImageA(object objEvent)
    {
        try
        {
            for (int i = 0; i < imageNum[0]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OcrBaiduA(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            ExitThread( );
        }
    }

    private void BaiduImageB(object objEvent)
    {
        try
        {
            for (int i = imageNum[0]; i < imageNum[1]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OcrBaiduA(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            ExitThread( );
        }
    }

    private void BaiduImageC(object objEvent)
    {
        try
        {
            for (int i = imageNum[1]; i < imageNum[2]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OcrBaiduA(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            ExitThread( );
        }
    }

    private void BaiduImageD(object objEvent)
    {
        try
        {
            for (int i = imageNum[2]; i < imageNum[3]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OcrBaiduA(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            ExitThread( );
        }
    }

    private void BaiduImageE(object objEvent)
    {
        try
        {
            for (int i = imageNum[3]; i < imageNum[4]; i++)
            {
                Stream stream = File.Open("Data\\image_temp\\" + i + ".jpg", FileMode.Open);
                OcrBaiduA(Image.FromStream(stream));
                stream.Close( );
            }
            ((ManualResetEvent) objEvent).Set( );
        }
        catch
        {
            ExitThread( );
        }
    }

    private void CountBoolImage(int num)
    {
        switch (num)
        {
            case >= 5:
                imageNum = new int[num];
                imageNum[4] = num;
                switch (num - num / 5 * 5)
                {
                    case 0:
                        imageNum[0] = num / 5;
                        imageNum[1] = num / 5 * 2;
                        imageNum[2] = num / 5 * 3;
                        imageNum[3] = num / 5 * 4;
                        break;
                    case 1:
                        imageNum[0] = num / 5 + 1;
                        imageNum[1] = num / 5 * 2;
                        imageNum[2] = num / 5 * 3;
                        imageNum[3] = num / 5 * 4;
                        break;
                    case 2:
                        imageNum[0] = num / 5 + 1;
                        imageNum[1] = num / 5 * 2 + 1;
                        imageNum[2] = num / 5 * 3;
                        imageNum[3] = num / 5 * 4;
                        break;
                    case 3:
                        imageNum[0] = num / 5 + 1;
                        imageNum[1] = num / 5 * 2 + 1;
                        imageNum[2] = num / 5 * 3 + 1;
                        imageNum[3] = num / 5 * 4;
                        break;
                    case 4:
                        imageNum[0] = num / 5 + 1;
                        imageNum[1] = num / 5 * 2 + 1;
                        imageNum[2] = num / 5 * 3 + 1;
                        imageNum[3] = num / 5 * 4 + 1;
                        break;
                }
                break;
            case 4:
                imageNum = new int[5];
                imageNum[0] = 1;
                imageNum[1] = 2;
                imageNum[2] = 3;
                imageNum[3] = 4;
                imageNum[4] = 0;
                break;
            case 3:
                imageNum = new int[5];
                imageNum[0] = 1;
                imageNum[1] = 2;
                imageNum[2] = 3;
                imageNum[3] = 0;
                imageNum[4] = 0;
                break;
            case 2:
                imageNum = new int[5];
                imageNum[0] = 1;
                imageNum[1] = 2;
                imageNum[2] = 0;
                imageNum[3] = 0;
                imageNum[4] = 0;
                break;
            case 1:
                imageNum = new int[5];
                imageNum[0] = 1;
                imageNum[1] = 0;
                imageNum[2] = 0;
                imageNum[3] = 0;
                imageNum[4] = 0;
                break;
            case 0:
                imageNum = new int[5];
                imageNum[0] = 0;
                imageNum[1] = 0;
                imageNum[2] = 0;
                imageNum[3] = 0;
                imageNum[4] = 0;
                break;
        }
    }

    private void DoWork(object state)
    {
        ManualResetEvent[] array = new ManualResetEvent[5];
        array[0] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(BaiduImageA), array[0]);
        array[1] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(BaiduImageB), array[1]);
        array[2] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(BaiduImageC), array[2]);
        array[3] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(BaiduImageD), array[3]);
        array[4] = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(new WaitCallback(BaiduImageE), array[4]);
        WaitHandle[] array2 = array;
        WaitHandle.WaitAll(array2);
        verticalRightText = string.Concat(new string[] { ocrBaidu, ocrBaidu, ocrBaidu, ocrBaidu, ocrBaidu }).Replace("\r\n\r\n", "");
        string text = verticalRightText.TrimEnd(new char[] { '\n' }).TrimEnd(new char[] { '\r' }).TrimEnd(new char[] { '\n' });
        if (text.Split(Environment.NewLine.ToCharArray( )).Length > 1)
        {
            string[] array3 = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string text2 = "";
            for (int i = 0; i < array3.Length; i++)
            {
                text2 = text2 + array3[array3.Length - i - 1].Replace("\r", "").Replace("\n", "") + "\r\n";
            }
            verticalLeftText = text2;
        }
        fmLoading.FormClose = "窗体已关闭";
        Invoke(new OcrThread(MainOcrThreadNormal));
        try
        {
            Helper.System.DeleteFile("Data\\image_temp");
        }
        catch
        {
            ExitThread( );
        }
        ImageOri.Dispose( );
    }

    private void ExitThread( )
    {
        try
        {
            Globals.CaptureRejection = false;
            esc = "退出";
            fmLoading.FormClose = "窗体已关闭";
            escThread.Abort( );
        }
        catch { }
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        Show( );
        WindowState = FormWindowState.Normal;
        if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
        {
            string value = Config.Get("快捷键", "翻译文本");
            string text = "None";
            string text2 = "F9";
            Config.SetHotkey(text, text2, value, 205);
        }
        UnregisterHotKey(Handle, 222);
    }

    private Image FindBundingBoxFences(Bitmap bitmap)
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
        Image image6 = ImageUtils.BoundingBoxFences(image5, image4);
        Image<Gray, byte> image7 = new((Bitmap) image6);
        GetSubPicOcr(imageScreen, ImageUtils.BoundingBoxFencesUp(image7));
        image.Dispose( );
        image2.Dispose( );
        image3.Dispose( );
        image7.Dispose( );
        return image6;
    }

    private new void FormClosing(object o, FormClosedEventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        Visible = false;
    }

    private void FormResizing(object o, EventArgs e)
    {
        if (richBox.Dock == DockStyle.Fill)
            return;
        richBox.Size = new Size(ClientRectangle.Width / 2, ClientRectangle.Height);
        richBoxTrans.Size = new Size(richBox.Width, ClientRectangle.Height);
        richBoxTrans.Location = (Point) new Size(richBox.Width, 0);
    }

    private void GetTable(string str)
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

    private void GoAbout( )
    {
        WindowState = FormWindowState.Minimized;
        CheckForIllegalCrossThreadCalls = false;
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
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                TrayShowMenu, TraySettingMenu, TrayUpdateMenu, TrayHelpMenu, TrayExitMenu
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("InitMinimize()" + ex.Message);
        }
    }

    private void Load_Click(object o, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        Visible = false;
    }

    private void MainCopyClick(object o, EventArgs e)
    {
        richBox.Focus( );
        richBox.EditBox.Copy( );
    }

    private void MainOcrQuickCapture( )
    {
        if (!Globals.CaptureRejection)
        {
            try
            {
                changeQQScreenshot = false;
                FormBorderStyle = FormBorderStyle.None;
                Visible = false;
                Thread.Sleep(100);
                formWidth = isTransOpen == "开启" ? Width / 2 : Width;
                verticalRightText = "";
                verticalLeftText = "";
                formHeight = Height;
                notifyIcon.Visible = false;
                notifyIcon.Visible = true;
                menu.Close( );
                menuCopy.Close( );
                autoFlag = "开启";
                splitedText = "";
                richBox.Text = "***该区域未发现文本***";
                richBoxTrans.Text = "";
                typeSetText = "";
                isTransOpen = "关闭";
                if (Config.Get("工具栏", "翻译") == "False")
                {
                    transClose.PerformClick( );
                }
                Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
                FormBorderStyle = FormBorderStyle.Sizable;
                Globals.CaptureRejection = true;
                imageScreen = RegionCaptureTasks.GetRegionImage_Mo(new RegionCaptureOptions
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
                    regionCaptureForm.Prepare(imageScreen);
                    regionCaptureForm.ShowDialog( );
                    imageScreen = null;
                    imageScreen = regionCaptureForm.GetResultImage( );
                    mode_flag = regionCaptureForm.Mode_flag;
                }
                RegisterHotKey(Handle, 222, KeyModifiers.None, Keys.Escape);
                if (mode_flag == "贴图")
                {
                    Point point2 = new(point.X, point.Y);
                    new FmScreenPaste(imageScreen, point2).Show( );
                    if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value = Config.Get("快捷键", "翻译文本");
                        string text = "None";
                        string text2 = "F9";
                        Config.SetHotkey(text, text2, value, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                    Globals.CaptureRejection = false;
                }
                else if (mode_flag == "区域多选")
                {
                    if (imageScreen == null)
                    {
                        if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
                        {
                            string value2 = Config.Get("快捷键", "翻译文本");
                            string text3 = "None";
                            string text4 = "F9";
                            Config.SetHotkey(text3, text4, value2, 205);
                        }
                        UnregisterHotKey(Handle, 222);
                        Globals.CaptureRejection = false;
                    }
                    else
                    {
                        notifyIcon.Visible = true;
                        thread = new Thread(new ThreadStart(ShowLoading));
                        thread.Start( );
                        timeUsed = new TimeSpan(DateTime.Now.Ticks);
                        GetSubPicOcr(imageScreen, array);
                    }
                }
                else if (mode_flag == "取色")
                {
                    if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value3 = Config.Get("快捷键", "翻译文本");
                        string text5 = "None";
                        string text6 = "F9";
                        Config.SetHotkey(text5, text6, value3, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                    Globals.CaptureRejection = false;
                    FmFlags.Display("已复制颜色");
                }
                else if (imageScreen == null)
                {
                    if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
                    {
                        string value4 = Config.Get("快捷键", "翻译文本");
                        string text7 = "None";
                        string text8 = "F9";
                        Config.SetHotkey(text7, text8, value4, 205);
                    }
                    UnregisterHotKey(Handle, 222);
                    Globals.CaptureRejection = false;
                }
                else
                {
                    if (mode_flag == "百度")
                    {
                        baiduFlags = "百度";
                    }
                    if (mode_flag == "拆分")
                    {
                        isMerged = false;
                        isSplited = true;
                    }
                    if (mode_flag == "合并")
                    {
                        isMerged = true;
                        isSplited = false;
                    }
                    if (mode_flag == "截图")
                    {
                        Clipboard.SetImage(imageScreen);
                        if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
                        {
                            string value5 = Config.Get("快捷键", "翻译文本");
                            string text9 = "None";
                            string text10 = "F9";
                            Config.SetHotkey(text9, text10, value5, 205);
                        }
                        UnregisterHotKey(Handle, 222);
                        Globals.CaptureRejection = false;
                        if (Config.Get("截图音效", "剪贴板") == "True")
                        {
                            Helper.System.PlaySong(Config.Get("截图音效", "音效路径"), Handle);
                        }
                        FmFlags.Display("已复制截图");
                    }
                    else if (mode_flag == "自动保存" && Config.Get("配置", "自动保存") == "True")
                    {
                        string text11 = Config.Get("配置", "截图位置") + "\\" + TextUtils.RenameFile(Config.Get("配置", "截图位置"), "图片.Png");
                        imageScreen.Save(text11, ImageFormat.Png);
                        Globals.CaptureRejection = false;
                        if (Config.Get("截图音效", "自动保存") == "True")
                        {
                            Helper.System.PlaySong(Config.Get("截图音效", "音效路径"), Handle);
                        }
                        FmFlags.Display("已保存图片");
                    }
                    else if (mode_flag == "多区域自动保存" && Config.Get("配置", "自动保存") == "True")
                    {
                        ImageUtils.GetSubImage(imageScreen, array);
                        Globals.CaptureRejection = false;
                        if (Config.Get("截图音效", "自动保存") == "True")
                        {
                            Helper.System.PlaySong(Config.Get("截图音效", "音效路径"), Handle);
                        }
                        FmFlags.Display("已保存图片");
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
                                imageScreen.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                            }
                            if (extension.Equals(".png", StringComparison.Ordinal))
                            {
                                imageScreen.Save(saveFileDialog.FileName, ImageFormat.Png);
                            }
                            if (extension.Equals(".bmp", StringComparison.Ordinal))
                            {
                                imageScreen.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                            }
                        }
                        if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
                        {
                            string value6 = Config.Get("快捷键", "翻译文本");
                            string text12 = "None";
                            string text13 = "F9";
                            Config.SetHotkey(text12, text13, value6, 205);
                        }
                        UnregisterHotKey(Handle, 222);
                        Globals.CaptureRejection = false;
                    }
                    else if (imageScreen != null)
                    {
                        if (Config.Get("工具栏", "分栏") == "True")
                        {
                            notifyIcon.Visible = true;
                            thread = new Thread(new ThreadStart(ShowLoading));
                            thread.Start( );
                            timeUsed = new TimeSpan(DateTime.Now.Ticks);
                            Image image = imageScreen;
                            Graphics graphics = Graphics.FromImage(new Bitmap(image.Width, image.Height));
                            graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                            graphics.Save( );
                            graphics.Dispose( );
                            ((Bitmap) FindBundingBoxFences((Bitmap) image)).Save("Data\\分栏预览图.jpg");
                            image.Dispose( );
                            imageScreen.Dispose( );
                        }
                        else
                        {
                            notifyIcon.Visible = true;
                            thread = new Thread(new ThreadStart(ShowLoading));
                            thread.Start( );
                            timeUsed = new TimeSpan(DateTime.Now.Ticks);
                            MessageLoad messageload = new( );
                            messageload.ShowDialog( );
                            if (messageload.DialogResult == DialogResult.OK)
                            {
                                escThread = new Thread(new ThreadStart(MainOcrThread));
                                escThread.Start( );
                            }
                        }
                    }
                }
            }
            catch
            {
                Globals.CaptureRejection = false;
            }
        }
    }

    private void MainOcrThread( )
    {
        if (!string.IsNullOrEmpty(ScanQRCode( )))
        {
            typeSetText = ScanQRCode( );
            richBox.Text = typeSetText;
            fmLoading.FormClose = "窗体已关闭";
            Invoke(new OcrThread(MainOcrThreadNormal));
            return;
        }
        switch (interfaceFlag)
        {
            case "日语" or "中英" or "韩语":
                OcrBaidu( );
                fmLoading.FormClose = "窗体已关闭";
                Invoke(new OcrThread(MainOcrThreadNormal));
                imageScreen.Dispose( );
                GC.Collect( );
                return;
            case "从左向右" or "从右向左":
            {
                verticalRightText = "";
                Image image = imageScreen;
                Bitmap bitmap = new(image.Width, image.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                graphics.Save( );
                graphics.Dispose( );
                ImageOri = bitmap;
                Image<Gray, byte> image2 = new(bitmap);
                Image<Gray, byte> image3 = new((Bitmap) ImageUtils.FindBundingBox(image2.ToBitmap( )));
                Image<Bgr, byte> image4 = image3.Convert<Bgr, byte>( );
                Image<Gray, byte> image5 = image3.Clone( );
                CvInvoke.Canny(image3, image5, 0.0, 0.0, 5, true);
                SelectImage(image5, image4);
                bitmap.Dispose( );
                image2.Dispose( );
                image3.Dispose( );
                imageScreen.Dispose( );
                GC.Collect( );
                return;
            }
            case "搜狗": OcrSogou2( ); break;
            case "腾讯": OcrTencent( ); break;
            case "有道": OcrYoudao( ); break;
            case "公式": OcrMath( ); break;
            case "百度表格": OcrTableBaidu( ); break;
            case "阿里表格": OcrTableAli( ); break;
        }
        fmLoading.FormClose = "窗体已关闭";
        Invoke(new OcrThread(MainOcrThreadNormal));
    }

    private void MainOcrThreadNormal( )
    {
        imageScreen.Dispose( );
        GC.Collect( );
        Globals.CaptureRejection = false;
        string text = typeSetText;
        text = TextUtils.CheckStr(text);
        splitedText = TextUtils.CheckStr(splitedText);
        if (!TextUtils.HasPunctuation(text))
        {
            text = splitedText;
        }
        if (TextUtils.ContainsZh(text.Trim( )))
        {
            text = TextUtils.RemoveSpace(text);
        }
        if (!string.IsNullOrEmpty(text))
        {
            richBox.Text = text;
        }
        Globals.SplitedText = splitedText;
        if (bool.Parse(Config.Get("工具栏", "拆分")) || isSplited)
        {
            isSplited = false;
            richBox.Text = splitedText;
        }
        if (bool.Parse(Config.Get("工具栏", "合并")) || isMerged)
        {
            isMerged = false;
            richBox.Text = text.Replace("\n", "").Replace("\r", "");
        }
        TimeSpan timeSpan = new(DateTime.Now.Ticks);
        TimeSpan timeSpan2 = timeSpan.Subtract(timeUsed).Duration( );
        string text2 = string.Concat(new string[]
        {
                timeSpan2.Seconds.ToString(),
                ".",
                Convert.ToInt32(timeSpan2.TotalMilliseconds).ToString(),
                "秒"
        });
        if (richBox.Text != null)
        {
            p_note(richBox.Text);
            Globals.Notes = pubnote;
            if (fmNote.Created)
            {
                fmNote.SetTextNote( );
            }
        }
        TopMost = Globals.Topmost;
        Text = "耗时：" + text2;
        notifyIcon.Visible = true;
        if (interfaceFlag == "从右向左")
        {
            richBox.Text = verticalRightText;
        }
        if (interfaceFlag == "从左向右")
        {
            richBox.Text = verticalLeftText;
        }
        Clipboard.SetDataObject(richBox.Text);
        if (baiduFlags == "百度")
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
            Visible = false;
            WindowState = FormWindowState.Minimized;
            Show( );
            Process.Start("https://www.baidu.com/s?wd=" + richBox.Text);
            baiduFlags = "";
            if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
            {
                string value = Config.Get("快捷键", "翻译文本");
                string text3 = "None";
                string text4 = "F9";
                Config.SetHotkey(text3, text4, value, 205);
            }
            UnregisterHotKey(Handle, 222);
            return;
        }
        if (Config.Get("配置", "识别弹窗") == "False")
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
            Visible = false;
            if (richBox.Text == "***该区域未发现文本***")
            {
                FmFlags.Display("无文本");
            }
            else
            {
                FmFlags.Display("已识别");
            }
            if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
            {
                string value2 = Config.Get("快捷键", "翻译文本");
                string text5 = "None";
                string text6 = "F9";
                Config.SetHotkey(text5, text6, value2, 205);
            }
            UnregisterHotKey(Handle, 222);
            return;
        }
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        Show( );
        WindowState = FormWindowState.Normal;
        Size = new Size(formWidth, formHeight);
        SetForegroundWindow(Handle);
        Globals.GoogleTransText = richBox.Text;
        if (bool.Parse(Config.Get("工具栏", "翻译")))
        {
            try
            {
                autoFlag = "";
                Invoke(new Translate(TranslateClick));
            }
            catch
            {
            }
        }
        if (bool.Parse(Config.Get("工具栏", "检查")))
        {
            try { richBox.CheckTyping( ); } catch { }
        }
        if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
        {
            string value3 = Config.Get("快捷键", "翻译文本");
            string text7 = "None";
            string text8 = "F9";
            Config.SetHotkey(text7, text8, value3, 205);
        }
        UnregisterHotKey(Handle, 222);
        richBox.Refresh( );
    }

    private void MainPasteClick(object o, EventArgs e)
    {
        richBox.Focus( );
        richBox.EditBox.Paste( );
    }

    private void MainSelectAllClick(object o, EventArgs e)
    {
        richBox.Focus( );
        richBox.EditBox.SelectAll( );
    }

    private void MainVoiceClick(object o, EventArgs e)
    {
        richBox.Focus( );
        speakCopyB = "朗读";
        htmltxt = richBox.SelectText;
        SendMessage(Handle, 786, 590);
    }

    private void MainSearchClick(object o, EventArgs e)
    {
        if (string.IsNullOrEmpty(richBox.SelectText))
        {
            Process.Start("https://www.baidu.com/");
            return;
        }
        Process.Start("https://www.baidu.com/s?wd=" + richBox.SelectText);
    }

    private void OCR_baidu_Jap_Click(object o, EventArgs e)
        => OcrForeach("日语");

    private void OCR_baidu_Kor_Click(object o, EventArgs e)
        => OcrForeach("韩语");

    private void OcrBaidu( )
    {
        splitedText = "";
        try
        {
            baiduVip = Web.GetHtml(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + Globals.BaiduApiId + "&client_secret=" + Globals.BaiduApiKey));
            if (string.IsNullOrEmpty(baiduVip))
            {
                MessageBox.Show("请检查密钥输入是否正确！", "提醒");
            }
            else
            {
                string text = "CHN_ENG";
                splitedText = "";
                Image image = imageScreen;
                byte[] array = ImageUtils.ImageToByte(image);
                if (interfaceFlag == "中英")
                    text = "CHN_ENG";
                if (interfaceFlag == "日语")
                    text = "JAP";
                if (interfaceFlag == "韩语")
                    text = "KOR";
                string param = $"image={HttpUtility.UrlEncode(Convert.ToBase64String(array))}&language_type={text}";
                byte[] bytes = Encoding.UTF8.GetBytes(param);
                HttpWebRequest req = (HttpWebRequest) WebRequest.Create("https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + ((JObject) JsonConvert.DeserializeObject(baiduVip))["access_token"]);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                req.Timeout = 8000;
                req.ReadWriteTimeout = 5000;
                using (Stream requestStream = req.GetRequestStream( ))
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                Stream responseStream = ((HttpWebResponse) req.GetResponse( )).GetResponseStream( );
                string text3 = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
                responseStream.Close( );
                JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["words_result"].ToString( ));
                TextUtils.TextCheck(jarray, 1, "words", TextFinalize);
            }
        }
        catch
        {
            if (esc != "退出")
            {
                richBox.Text = "***该区域未发现文本或者密钥次数用尽***";
            }
            else
            {
                richBox.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private void OcrBaiduA(Image images)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream stream = new( );
            images.Save(stream, ImageFormat.Jpeg);
            byte[] array = new byte[stream.Length];
            stream.Position = 0L;
            stream.Read(array, 0, (int) stream.Length);
            stream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest req0 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            req0.CookieContainer = new CookieContainer( );
            req0.GetResponse( ).Close( );
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            req.Method = "POST";
            req.Referer = "http://ai.baidu.com/tech/ocr/general";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Timeout = 8000;
            req.ReadWriteTimeout = 5000;
            req.Headers.Add("Cookie:" + Web.CookieToStr(((HttpWebResponse) req0.GetResponse( )).Cookies));
            using (Stream reqStream = req.GetRequestStream( ))
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }
            Stream res = ((HttpWebResponse) req.GetResponse( )).GetResponseStream( );
            string content = new StreamReader(res, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            res.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(content))["data"]["words_result"].ToString( ));
            string result = "";
            string[] results = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                result += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                results[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            // No use var "text5" at all
            //string text5 = "";
            //for (int j = 0; j < array2.Length; j++)
            //{
            //    text5 += array2[j];
            //}
            ocrBaidu = (ocrBaidu + result + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch { }
    }

    private void OcrBaiduZhEnClick(object o, EventArgs e)
        => OcrForeach("中英");

    private void OcrForeach(string name)
    {
        switch (name)
        {
            case "韩语":
                interfaceFlag = "韩语";
                Refresh( );
                baidu.Text = "百度√";
                kor.Text = "韩语√";
                break;
            case "日语":
                interfaceFlag = "日语";
                Refresh( );
                baidu.Text = "百度√";
                jap.Text = "日语√";
                break;
            case "中英":
                interfaceFlag = "中英";
                Refresh( );
                baidu.Text = "百度√";
                ch2en.Text = "中英√";
                break;
            case "搜狗":
                interfaceFlag = "搜狗";
                Refresh( );
                sougou.Text = "搜狗√";
                break;
            case "腾讯":
                interfaceFlag = "腾讯";
                Refresh( );
                tencent.Text = "腾讯√";
                break;
            case "有道":
                interfaceFlag = "有道";
                Refresh( );
                youdao.Text = "有道√";
                break;
            case "公式":
                interfaceFlag = "公式";
                Refresh( );
                mathFunction.Text = "公式√";
                break;
            case "百度表格":
                interfaceFlag = "百度表格";
                Refresh( );
                tableOcr.Text = "表格√";
                tableBaidu.Text = "百度√";
                break;
            case "阿里表格":
                interfaceFlag = "阿里表格";
                Refresh( );
                tableOcr.Text = "表格√";
                tableAli.Text = "阿里√";
                break;
            case "从左向右":
                if (!File.Exists("cvextern.dll"))
                {
                    MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\r\n将下载后的文件与 天若.exe 这个文件放在一起。");
                    Process.Start("https://www.lanzous.com/i1ab3vg");
                }
                else
                {
                    interfaceFlag = "从左向右";
                    Refresh( );
                    verticalScan.Text = "竖排√";
                    left2right.Text = "从左向右√";
                }
                break;
            case "从右向左":
                if (!File.Exists("cvextern.dll"))
                {
                    MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\n将下载后的文件与 天若.exe 这个文件放在一起。");
                    Process.Start("https://www.lanzous.com/i1ab3vg");
                    return;
                }
                interfaceFlag = "从右向左";
                Refresh( );
                verticalScan.Text = "竖排√";
                right2left.Text = "从右向左√";
                break;
        }
        Config.Set("配置", "接口", interfaceFlag);
    }

    private void OcrLtrClick(object o, EventArgs e)
        => OcrForeach("从左向右");

    private void OcrMath( )
    {
        splitedText = "";
        try
        {
            Image image = imageScreen;
            byte[] array = ImageUtils.ImageToByte(image);
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
            splitedText = text3;
            typeSetText = text3;
        }
        catch
        {
            if (esc != "退出")
            {
                richBox.Text = "***该区域未发现文本或者密钥次数用尽***";
            }
            else
            {
                richBox.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private void OcrMathClick(object o, EventArgs e) => OcrForeach("公式");

    private void OcrRtlClick(object o, EventArgs e)
        => OcrForeach("从右向左");

    private void OcrSogou2( )
    {
        try
        {
            splitedText = "";
            string text = "------WebKitFormBoundary8orYTmcj8BHvQpVU";
            Image image = ImageUtils.ZoomImage((Bitmap) imageScreen, 120, 120);
            byte[] array = ImageUtils.ImageToByte(image);
            string text2 = text + "\r\nContent-Disposition: form-data; name=\"pic\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = "\r\n" + text + "--\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(text2);
            byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
            byte[] array2 = Web.MergeBytes(bytes, array, bytes2);
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
            if (Config.Get("工具栏", "分段") == "True")
            {
                SogouLocationCheck(jarray, 2, "content", "frame");
            }
            else
            {
                TextUtils.TextCheck(jarray, 2, "content", TextFinalize);
            }
            image.Dispose( );
        }
        catch
        {
            if (esc != "退出")
            {
                richBox.Text = "***该区域未发现文本***";
            }
            else
            {
                richBox.Text = "***该区域未发现文本***";
                esc = "";
            }
        }
    }

    private void OcrSogouClick(object o, EventArgs e)
        => OcrForeach("搜狗");

    private void OcrTableAli( )
    {
        string text = "";
        splitedText = "";
        try
        {
            string value = Config.Get("特殊", "ali_cookie");
            Stream stream = new MemoryStream(ImageUtils.ImageToByteArray(ImageUtils.ToGray((Bitmap) imageScreen)));
            string text2 = Convert.ToBase64String(new BinaryReader(stream).ReadBytes(Convert.ToInt32(stream.Length)));
            stream.Close( );
            string text3 = "{\n\t\"image\": \"" + text2 + "\",\n\t\"configure\": \"{\\\"format\\\":\\\"html\\\", \\\"finance\\\":false}\"\n}";
            string text4 = "https://predict-pai.data.aliyun.com/dp_experience_mall/ocr/ocr_table_parse";
            text = Web.PostHtmlFinal(text4, text3, value);
            typeSetText = ((JObject) JsonConvert.DeserializeObject(Web.PostHtmlFinal(text4, text3, value)))["tables"].ToString( ).Replace("table tr td { border: 1px solid blue }", "table tr td {border: 0.5px black solid }").Replace("table { border: 1px solid blue }", "table { border: 0.5px black solid; border-collapse : collapse}\r\n");
            richBox.Text = "[消息]：表格已复制到剪贴板！";
        }
        catch
        {
            richBox.Text = "[消息]：阿里表格识别出错！";
            if (text.Contains("NEED_LOGIN"))
            {
                splitedText = "弹出cookie";
            }
        }
    }

    private void OcrTableAliClick(object o, EventArgs e)
        => OcrForeach("阿里表格");

    private void OcrTableBaidu( )
    {
        typeSetText = "[消息]：表格已下载！";
        splitedText = "";
        try
        {
            baiduVip = Web.GetHtml(string.Format("{0}?{1}", "https://aip.baidubce.com/oauth/2.0/token", "grant_type=client_credentials&client_id=" + Globals.BaiduApiId + "&client_secret=" + Globals.BaiduApiKey));
            if (string.IsNullOrEmpty(baiduVip))
            {
                MessageBox.Show("请检查密钥输入是否正确！", "提醒");
            }
            else
            {
                splitedText = "";
                Image image = imageScreen;
                MemoryStream memoryStream = new( );
                image.Save(memoryStream, ImageFormat.Jpeg);
                byte[] array = new byte[memoryStream.Length];
                memoryStream.Position = 0L;
                memoryStream.Read(array, 0, (int) memoryStream.Length);
                memoryStream.Close( );
                string text = "image=" + HttpUtility.UrlEncode(Convert.ToBase64String(array));
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://aip.baidubce.com/rest/2.0/solution/v1/form_ocr/request?access_token=" + ((JObject) JsonConvert.DeserializeObject(baiduVip))["access_token"]);
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
                        richBox.Text = "[消息]：未发现表格！";
                        break;
                    }
                    Thread.Sleep(120);
                    text4 = Web.PostHtml("https://aip.baidubce.com/rest/2.0/solution/v1/form_ocr/get_request_result?access_token=" + ((JObject) JsonConvert.DeserializeObject(baiduVip))["access_token"], text3);
                }
                if (!text4.Contains("image recognize error"))
                {
                    GetTable(text4);
                }
            }
        }
        catch
        {
            richBox.Text = "[消息]：免费百度密钥50次已经耗完！请更换自己的密钥继续使用！";
        }
    }

    private void OcrTableBaiduClick(object o, EventArgs e)
        => OcrForeach("百度表格");

    private void OcrTableClick(object o, EventArgs e)
        => OcrForeach("表格");

    private void OcrTencent( )
    {
        try
        {
            splitedText = "";
            string text = "------WebKitFormBoundaryRDEqU0w702X9cWPJ";
            Image image = imageScreen;
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
                image = imageScreen;
            }
            byte[] imageBytes = ImageUtils.ImageToByte(image);
            string text2 = $"{text}\r\nContent-Disposition: form-data; name=\"image_file\"; filename=\"pic.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string text3 = $"\r\n{text}--\r\n";
            byte[] reqBytes = Web.MergeBytes(Encoding.ASCII.GetBytes(text2), imageBytes, Encoding.ASCII.GetBytes(text3));
            string result = Web.PostCompressContent("https://ai.qq.com/cgi-bin/appdemo_generalocr", reqBytes, "http://ai.qq.com/product/ocr.shtml", OcrType.Tencent);
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(result))["data"]["item_list"].ToString( ));
            TextUtils.TextCheck(jarray, 1, "itemstring", TextFinalize);
        }
        catch
        {
            richBox.Text = "***该区域未发现文本***";
            if (esc == "退出")
                esc = "";
        }
    }

    private void OcrTencentClick(object o, EventArgs e)
        => OcrForeach("腾讯");

    private void OcrWriteClick(object o, EventArgs e)
        => OcrForeach("手写");

    private void OcrYoudao( )
    {
        try
        {
            splitedText = "";
            Image image = imageScreen;
            switch (image.Width)
            {
                case > 90 when image.Height < 90:
                {
                    Bitmap b1 = new(image.Width, 200);
                    Graphics g2 = Graphics.FromImage(b1);
                    g2.DrawImage(image, 5, 0, image.Width, image.Height);
                    g2.Save( );
                    g2.Dispose( );
                    image = new Bitmap(b1);
                    break;
                }

                case <= 90 when image.Height >= 90:
                {
                    Bitmap b2 = new(200, image.Height);
                    Graphics g2 = Graphics.FromImage(b2);
                    g2.DrawImage(image, 0, 5, image.Width, image.Height);
                    g2.Save( );
                    g2.Dispose( );
                    image = new Bitmap(b2);
                    break;
                }

                case < 90 when image.Height < 90:
                {
                    Bitmap b3 = new(200, 200);
                    Graphics g3 = Graphics.FromImage(b3);
                    g3.DrawImage(image, 5, 5, image.Width, image.Height);
                    g3.Save( );
                    g3.Dispose( );
                    image = new Bitmap(b3);
                    break;
                }

                default:
                    image = imageScreen;
                    break;
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
            Bitmap b = new(i, j);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(image, 0, 0, i, j);
            g.Save( );
            g.Dispose( );
            image = new Bitmap(b);
            byte[] array = ImageUtils.ImageToByte(image);
            byte[] bytes = Encoding.UTF8.GetBytes($"imgBase=data{HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array))}&lang=auto&company=");
            string result = Web.PostCompressContent("http://aidemo.youdao.com/ocrapi1", bytes, "http://aidemo.youdao.com/ocrdemo", OcrType.Youdao);
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(result))["lines"].ToString( ));
            TextUtils.TextCheck(jarray, 1, "words", TextFinalize);
            image.Dispose( );
        }
        catch
        {
            richBox.Text = "***该区域未发现文本***";
            if (esc == "退出")
                esc = "";
        }
    }

    private void OcrYoudaoClick(object o, EventArgs e) => OcrForeach("有道");

    private void p_note(string a)
    {
        for (int i = 0; i < Globals.NoteCount; i++)
        {
            if (i == Globals.NoteCount - 1)
            {
                pubnote[Globals.NoteCount - 1] = a;
            }
            else
            {
                pubnote[i] = pubnote[i + 1];
            }
        }
    }

    private void ReadIniFile( )
    {
        proxyFlag = Config.Get("代理", "代理类型");
        proxyUrl = Config.Get("代理", "服务器");
        proxyPort = Config.Get("代理", "端口");
        proxyName = Config.Get("代理", "服务器账号");
        proxyPassword = Config.Get("代理", "服务器密码");
        if (proxyFlag == "不使用代理")
        {
            WebRequest.DefaultWebProxy = null;
        }
        if (proxyFlag == "系统代理")
        {
            WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy( );
        }
        if (proxyFlag == "自定义代理")
        {
            try
            {
                WebProxy webProxy = new(proxyUrl, Convert.ToInt32(proxyPort));
                if (!string.IsNullOrEmpty(proxyName) && !string.IsNullOrEmpty(proxyPassword))
                {
                    ICredentials credentials = new NetworkCredential(proxyName, proxyPassword);
                    webProxy.Credentials = credentials;
                }
                WebRequest.DefaultWebProxy = webProxy;
            }
            catch
            {
                MessageBox.Show("请检查代理设置！");
            }
        }
        interfaceFlag = Config.Get("配置", "接口");
        if (interfaceFlag == "_ERROR_")
        {
            Config.Set("配置", "接口", "搜狗");
            OcrForeach("搜狗");
        }
        else
        {
            OcrForeach(interfaceFlag);
        }
        if (Config.Get("快捷键", "文字识别") != "请按下快捷键")
        {
            string value = Config.Get("快捷键", "文字识别");
            string text2 = "None";
            string text3 = "F4";
            Config.SetHotkey(text2, text3, value, 200);
        }
        if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
        {
            string value2 = Config.Get("快捷键", "翻译文本");
            string text4 = "None";
            string text5 = "F7";
            Config.SetHotkey(text4, text5, value2, 205);
        }
        if (Config.Get("快捷键", "记录界面") != "请按下快捷键")
        {
            string value3 = Config.Get("快捷键", "记录界面");
            string text6 = "None";
            string text7 = "F8";
            Config.SetHotkey(text6, text7, value3, 206);
        }
        if (Config.Get("快捷键", "识别界面") != "请按下快捷键")
        {
            string value4 = Config.Get("快捷键", "识别界面");
            string text8 = "None";
            string text9 = "F11";
            Config.SetHotkey(text8, text9, value4, 235);
        }
        Globals.BaiduApiId = Config.Get("密钥_百度", "secret_id");
        if (Config.Get("密钥_百度", "secret_id") == "_ERROR_")
        {
            Globals.BaiduApiId = "请输入secret_id";
        }
        Globals.BaiduApiKey = Config.Get("密钥_百度", "secret_key");
        if (Config.Get("密钥_百度", "secret_key") == "_ERROR_")
        {
            Globals.BaiduApiKey = "请输入secret_key";
        }
    }

    private new void Refresh( )
    {
        sougou.Text = "搜狗";
        tencent.Text = "腾讯";
        baidu.Text = "百度";
        youdao.Text = "有道";
        verticalScan.Text = "竖排";
        tableOcr.Text = "表格";
        ch2en.Text = "中英";
        jap.Text = "日语";
        kor.Text = "韩语";
        left2right.Text = "从左向右";
        right2left.Text = "从右向左";
        tableBaidu.Text = "百度";
        tableAli.Text = "阿里";
        mathFunction.Text = "公式";
    }

    private void SaveIniFile( )
        => Config.Set("配置", "接口", interfaceFlag);

    private string ScanQRCode( )
    {
        string text = "";
        try
        {
            BinaryBitmap binaryBitmap = new(new HybridBinarizer(new BitmapLuminanceSource((Bitmap) imageScreen)));
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

    private void SelectImage(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        try
        {
            using VectorOfVectorOfPoint vectorOfVectorOfPoint = new( );
            CvInvoke.FindContours(src, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
            int num = vectorOfVectorOfPoint.Size / 2;
            imageListLength = num;
            CountBoolImage(num);
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data\\image_temp"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Data\\image_temp");
            }
            ocrBaidu = "";
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
                    new Point(x, ImageOri.Size.Height);
                    Rectangle rectangle2 = new(x, 0, width, ImageOri.Size.Height);
                    Bitmap bitmap = new(width + 70, rectangle2.Size.Height);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Size.Width, bitmap.Size.Height);
                    graphics.DrawImage(ImageOri, 30, 0, rectangle2, GraphicsUnit.Pixel);
                    Bitmap bitmap2 = Image.FromHbitmap(bitmap.GetHbitmap( ));
                    bitmap2.Save("Data\\image_temp\\" + i + ".jpg", ImageFormat.Jpeg);
                    bitmap2.Dispose( );
                    bitmap.Dispose( );
                    graphics.Dispose( );
                }
            }
            MessageLoad messageload = new( );
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
            ExitThread( );
        }
    }

    private void setClipboard_Table(string[,] word, int[] cc)
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
        for (int j = 0; j < word.GetLength(0); j++)
        {
            for (int k = 0; k < word.GetLength(1); k++)
            {
                text2 = k == 0 ? text2 + "\\fs24 " + word[j, k] : text2 + "\\cell " + word[j, k];
            }
            if (j != word.GetLength(0) - 1)
            {
                text2 += "\\row\\intbl";
            }
        }
        richBox.Rtf = text + text3 + text2 + text4;
    }

    private void ShowLoading( )
    {
        try
        {
            fmLoading = new FmLoading( );
            Application.Run(fmLoading);
        }
        catch (ThreadAbortException) { }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString( ));
        }
        finally
        {
            thread.Abort( );
        }
    }

    private void SogouLocationCheck(JArray jarray, int lastlength, string words, string location)
    {
        paragraph = false;
        int num = 20000;
        int num2 = 0;
        for (int i = 0; i < jarray.Count; i++)
        {
            JObject jobject = JObject.Parse(jarray[i].ToString( ));
            int num3 = TextUtils.GetFirstNum(jobject[location][1].ToString( )) - TextUtils.GetFirstNum(jobject[location][0].ToString( ));
            if (num3 > num2)
            {
                num2 = num3;
            }
            int num4 = TextUtils.GetFirstNum(jobject[location][0].ToString( ));
            if (num4 < num)
            {
                num = num4;
            }
        }
        JObject jobject2 = JObject.Parse(jarray[0].ToString( ));
        if (Math.Abs(TextUtils.GetFirstNum(jobject2[location][0].ToString( )) - num) > 10)
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
            bool flag = Math.Abs(TextUtils.GetFirstNum(jobject4[location][1].ToString( )) - TextUtils.GetFirstNum(jobject4[location][0].ToString( )) - num2) > 20;
            bool flag2 = Math.Abs(TextUtils.GetFirstNum(jobject4[location][0].ToString( )) - num) > 10;
            if (flag && flag2)
            {
                text = text.Trim( ) + "\r\n" + jobject4[words].ToString( ).Trim( );
            }
            else if (TextUtils.IsNum(array[0].ToString( )) && !TextUtils.ContainsZh(array[1].ToString( )) && flag)
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
        splitedText = text2.Replace("\r\n\r\n", "\r\n");
        typeSetText = text;
    }

    private void SplitClick(object o, EventArgs e)
        => richBox.Text = splitedText;

    private void Switch2EnClick(object o, EventArgs e)
    {
        language = "英文标点";
        if (!string.IsNullOrEmpty(typeSetText))
        {
            richBox.Text = TextUtils.PunctuationChEn(richBox.Text);
        }
    }

    private void Switch2PinyinClick(object o, EventArgs e)
    {
        pinyinFlag = true;
        TranslateClick( );
    }

    private void Switch2StrUpperClick(object o, EventArgs e)
    {
        if (richBox.Text != null)
        {
            richBox.Text = richBox.Text.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    private void Switch2TransZhClick(object o, EventArgs e)
    {
        if (richBox.Text != null)
        {
            richBox.Text = TextUtils.ToSimplified(richBox.Text);
        }
    }

    private void Switch2UpperStrClick(object o, EventArgs e)
    {
        if (richBox.Text != null)
        {
            richBox.Text = richBox.Text.ToLower(System.Globalization.CultureInfo.CurrentCulture);
        }
    }

    private void Switch2ZhClick(object o, EventArgs e)
    {
        language = "中文标点";
        if (!string.IsNullOrEmpty(typeSetText))
        {
            richBox.Text = TextUtils.PunctuationEnZhX(richBox.Text);
            richBox.Text = TextUtils.PunctuationQuotation(richBox.Text);
        }
    }

    private void Switch2ZhTransClick(object o, EventArgs e)
    {
        if (richBox.Text != null)
        {
            richBox.Text = TextUtils.ToTraditional(richBox.Text);
        }
    }

    public static string TencentPOST(string url, string content)
    {
        string result;
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
            httpWebRequest.Headers.Add("cookie:" + Web.GetCookies("http://fanyi.qq.com"));
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
            result = text;
            if (text.Contains("\"records\":[]"))
            {
                Thread.Sleep(8);
                return TencentPOST(url, content);
            }
        }
        catch
        {
            result = "[腾讯接口报错]：\r\n请切换其它接口或再次尝试。";
        }
        return result;
    }

    private void TransBaiduClick(object o, EventArgs e) => TranslateForeach("百度");

    private void TransCloseClick(object o, EventArgs e)
    {
        base.MinimumSize = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
        isTransOpen = "关闭";
        richBox.Dock = DockStyle.Fill;
        richBoxTrans.Visible = false;
        image1.Visible = false;
        richBoxTrans.Text = "";
        if (WindowState == FormWindowState.Maximized)
        {
            WindowState = FormWindowState.Normal;
        }
        Size = new Size((int) fontBase.Width * 23, (int) fontBase.Height * 24);
    }

    private void TransCopyClick(object o, EventArgs e)
    {
        richBoxTrans.Focus( );
        richBoxTrans.EditBox.Copy( );
    }

    private void TransGoogleClick(object o, EventArgs e)
        => TranslateForeach("谷歌");

    private void TranslateChild( )
    {
        richBoxTrans.Text = googleTransText;
        googleTransText = "";
    }

    private void TranslateClick( )
    {
        typeSetText = richBox.Text;
        richBoxTrans.Visible = true;
        WindowState = FormWindowState.Normal;
        isTransOpen = "开启";
        richBox.Dock = DockStyle.None;
        richBoxTrans.Dock = DockStyle.None;
        richBoxTrans.BorderStyle = BorderStyle.Fixed3D;
        richBoxTrans.Text = "";
        richBox.Focus( );
        if (isNumOk == 0)
        {
            richBox.Size = new Size(ClientRectangle.Width, ClientRectangle.Height);
            Size = new Size(richBox.Width * 2, richBox.Height);
            richBoxTrans.Size = new Size(richBox.Width, richBox.Height);
            richBoxTrans.Location = (Point) new Size(richBox.Width, 0);
            richBoxTrans.Name = "rich_trans";
            richBoxTrans.TabIndex = 1;
            richBoxTrans.SetToolBar(WindowType.Second);
            richBoxTrans.ImeMode = ImeMode.On;
        }
        isNumOk++;
        image1.Visible = true;
        image1.BringToFront( );
        MinimumSize = new Size((int) fontBase.Width * 23 * 2, (int) fontBase.Height * 24);
        Size = new Size((int) fontBase.Width * 23 * 2, (int) fontBase.Height * 24);
        CheckForIllegalCrossThreadCalls = false;
        new Thread(new ThreadStart(TransParse)).Start( );
    }

    private void TranslateForeach(string name)
    {
        if (name == "百度")
        {
            transBaidu.Text = "百度√";
            transGoogle.Text = "谷歌";
            transTencent.Text = "腾讯";
            Config.Set("配置", "翻译接口", "百度");
        }
        if (name == "谷歌")
        {
            transBaidu.Text = "百度";
            transGoogle.Text = "谷歌√";
            transTencent.Text = "腾讯";
            Config.Set("配置", "翻译接口", "谷歌");
        }
        if (name == "腾讯")
        {
            transGoogle.Text = "谷歌";
            transBaidu.Text = "百度";
            transTencent.Text = "腾讯√";
            Config.Set("配置", "翻译接口", "腾讯");
        }
    }

    private void translateText( )
    {
        if (Config.Get("配置", "快速翻译") == "True")
        {
            string text = "";
            try
            {
                transHotkey = TextUtils.GetTextFromClipboard( );
                text = Helper.Translate.TranslateAsConfig(transHotkey);
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
        richBox.Text = Clipboard.GetText( );
        TranslateClick( );
        FormBorderStyle = FormBorderStyle.Sizable;
        Visible = true;
        SetForegroundWindow(Globals.MainHandle);
        Show( );
        WindowState = FormWindowState.Normal;
        if (Config.Get("工具栏", "顶置") == "True")
        {
            TopMost = true;
            return;
        }
        TopMost = false;
    }

    private void TranslateVoiceClick(object o, EventArgs e)
    {
        richBoxTrans.Focus( );
        speakCopyB = "朗读";
        htmltxt = richBoxTrans.SelectText;
        SendMessage(Handle, 786, 590);
    }

    private void TransParse( )
    {
        if (pinyinFlag)
        {
            googleTransText = Pinyin.Convert(typeSetText);
        }
        else if (string.IsNullOrEmpty(typeSetText))
        {
            googleTransText = "";
        }
        else
        {
            if (interfaceFlag == "韩语")
            {
                Globals.TransType = TranslateType.ZhEn;
                richBox.UpdateTransType( );
            }
            else if (interfaceFlag == "日语")
            {
                Globals.TransType = TranslateType.ZhEn;
                richBox.UpdateTransType( );
            }
            else if (interfaceFlag == "中英")
            {
                Globals.TransType = TranslateType.ZhEn;
                richBox.UpdateTransType( );
            }
            googleTransText = Helper.Translate.TranslateAsConfig(typeSetText);
        }
        image1.Visible = false;
        image1.SendToBack( );
        Invoke(new Translate(TranslateChild));
        pinyinFlag = false;
    }

    private void TransPasteClick(object o, EventArgs e)
    {
        richBoxTrans.Focus( );
        richBoxTrans.EditBox.Paste( );
    }

    private void TransSelectAllClick(object o, EventArgs e)
    {
        richBoxTrans.Focus( );
        richBoxTrans.EditBox.SelectAll( );
    }

    private void TransTencentClick(object o, EventArgs e)
        => TranslateForeach("腾讯");

    private void TrayDoubleClick(object o, EventArgs e)
    {
        UnregisterHotKey(Handle, 205);
        menu.Hide( );
        richBox.Hide = "";
        richBoxTrans.Hide = "";
        MainOcrQuickCapture( );
    }

    private void TrayExitClick(object o, EventArgs e)
    {
        notifyIcon.Dispose( );
        SaveIniFile( );
        Process.GetCurrentProcess( ).Kill( );
    }

    private void TrayHelpClick(object o, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        new FmHelp( ).Show( );
    }

    private void TrayLimitClick(object o, EventArgs e)
        => new Thread(new ThreadStart(GoAbout)).Start( );

    private void TrayNoteClick(object o, EventArgs e)
    {
        fmNote.Show( );
        fmNote.WindowState = FormWindowState.Normal;
        fmNote.Visible = true;
    }

    private void TrayNullProxyClick(object o, EventArgs e)
    {
        nullProxy.Text = "不使用代理√";
        customizeProxy.Text = "自定义代理";
        systemProxy.Text = "系统代理";
        proxyFlag = "关闭";
        WebRequest.DefaultWebProxy = null;
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
            Globals.NoteCount = Convert.ToInt32(Config.Get("配置", "记录数目"));
            pubnote = new string[Globals.NoteCount];
            for (int i = 0; i < Globals.NoteCount; i++)
            {
                pubnote[i] = "";
            }
            Globals.Notes = pubnote;
            fmNote.TextNoteChange( );
            fmNote.Location = new Point(Screen.AllScreens[0].WorkingArea.Width - fmNote.Width, Screen.AllScreens[0].WorkingArea.Height - fmNote.Height);
            if (Config.Get("快捷键", "文字识别") != "请按下快捷键")
            {
                string value = Config.Get("快捷键", "文字识别");
                string text2 = "None";
                string text3 = "F4";
                Config.SetHotkey(text2, text3, value, 200);
            }
            if (Config.Get("快捷键", "翻译文本") != "请按下快捷键")
            {
                string value2 = Config.Get("快捷键", "翻译文本");
                string text4 = "None";
                string text5 = "F9";
                Config.SetHotkey(text4, text5, value2, 205);
            }
            if (Config.Get("快捷键", "记录界面") != "请按下快捷键")
            {
                string value3 = Config.Get("快捷键", "记录界面");
                string text6 = "None";
                string text7 = "F8";
                Config.SetHotkey(text6, text7, value3, 206);
            }
            if (Config.Get("快捷键", "识别界面") != "请按下快捷键")
            {
                string value4 = Config.Get("快捷键", "识别界面");
                string text8 = "None";
                string text9 = "F11";
                Config.SetHotkey(text8, text9, value4, 235);
            }
            proxyFlag = Config.Get("代理", "代理类型");
            proxyUrl = Config.Get("代理", "服务器");
            proxyPort = Config.Get("代理", "端口");
            proxyName = Config.Get("代理", "服务器账号");
            proxyPassword = Config.Get("代理", "服务器密码");
            Globals.BaiduApiId = Config.Get("密钥_百度", "secret_id");
            Globals.BaiduApiKey = Config.Get("密钥_百度", "secret_key");
            if (proxyFlag == "不使用代理")
            {
                WebRequest.DefaultWebProxy = null;
            }
            if (proxyFlag == "系统代理")
            {
                WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy( );
            }
            if (proxyFlag == "自定义代理")
            {
                try
                {
                    WebProxy webProxy = new(proxyUrl, Convert.ToInt32(proxyPort));
                    if (!string.IsNullOrEmpty(proxyName) && !string.IsNullOrEmpty(proxyPassword))
                    {
                        ICredentials credentials = new NetworkCredential(proxyName, proxyPassword);
                        webProxy.Credentials = credentials;
                    }
                    WebRequest.DefaultWebProxy = webProxy;
                }
                catch
                {
                    MessageBox.Show("请检查代理设置！");
                }
            }
            if (Config.Get("更新", "更新间隔") == "True")
            {
                Program.updateTimer.Enabled = true;
                Program.updateTimer.Interval = 3600000.0 * Convert.ToInt32(Config.Get("更新", "间隔时间"));
                Program.updateTimer.Elapsed += Program.CheckTimerElapsed;
                Program.updateTimer.Start( );
            }
        }
    }

    private void TrayShowClick(object o, EventArgs e)
    {
        Show( );
        Activate( );
        Visible = true;
        WindowState = FormWindowState.Normal;
        if (Config.Get("工具栏", "顶置") == "True")
        {
            TopMost = true;
            return;
        }
        TopMost = false;
    }

    private void TraySystemProxyClick(object o, EventArgs e)
    {
        nullProxy.Text = "不使用代理";
        customizeProxy.Text = "自定义代理";
        systemProxy.Text = "系统代理√";
        proxyFlag = "系统";
        WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy( );
    }

    private void TrayUpdateClick(object o, EventArgs e)
        => new FmSetting( ) { SelectedTab = FmSettingTab.更新 }.Show( );

    private void Tts( )
        => new Thread(new ThreadStart(TtsThread)).Start( );

    private void TtsChild( )
    {
        if (richBox.Text == null && string.IsNullOrEmpty(richBoxTrans.Text))
            return;
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
        Helper.System.PlaySong(text, Handle);
        speaking = true;
    }

    private void TtsThread( )
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
            if (speakCopyB == "朗读" || voiceCount == 0)
            {
                Invoke(new Translate(TtsChild));
                speakCopyB = "";
            }
            else
            {
                Invoke(new Translate(TtsChild));
            }
            voiceCount++;
        }
        catch (Exception ex)
        {
            if (ex.ToString( ).IndexOf("Null") <= -1)
            {
                MessageBox.Show("文本过长，请使用右键菜单中的选中朗读！", "提醒");
            }
        }
    }
}
