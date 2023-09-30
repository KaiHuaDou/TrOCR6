using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Controls;

[Description("Provides a user control that allows the user to edit HTML page.")]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
public partial class AdvRichTextBox : UserControl
{
    private bool checkColor;
    private bool fenceColor;
    private bool isTopmost;
    private bool mergeColor;
    private bool paragraphColor;
    private bool splitColor;
    private string textFlag;
    private bool toolFull;
    private bool toolSpace;
    private bool transColor;
    private UndoCommand undoCmd;

    public AdvRichTextBox( )
    {
        toolSpace = true;
        toolFull = true;
        undoCmd = new UndoCommand(50);
        Font = new Font(Font.Name, 9f / StaticValue.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        InitializeComponent( );
        ReadConfig( );
        EditBox.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
    }

    public string Find
    {
        set
        {
            new Thread(new ThreadStart(typoCheckingApi)).Start( );
            SetForegroundWindow(StaticValue.mainhandle);
        }
    }
    public new string Hide
    {
        set
        {
            EditBox.Focus( );
            mode.HideDropDown( );
            Fontstyle.HideDropDown( );
            languagle.HideDropDown( );
        }
    }
    public ContextMenuStrip InnerContextMenuStrip
    {
        get => EditBox.ContextMenuStrip;
        set => EditBox.ContextMenuStrip = value;
    }
    public string Language
    {
        set
        {
            if (value == "中英")
            {
                zh_en.ForeColor = Color.Red;
                zh_jp.ForeColor = Color.Black;
                zh_ko.ForeColor = Color.Black;
            }
            if (value == "日语")
            {
                zh_en.ForeColor = Color.Black;
                zh_jp.ForeColor = Color.Red;
                zh_ko.ForeColor = Color.Black;
            }
            if (value == "韩语")
            {
                zh_en.ForeColor = Color.Black;
                zh_jp.ForeColor = Color.Black;
                zh_ko.ForeColor = Color.Red;
            }
        }
    }
    public string Rtf
    {
        get => EditBox.Rtf;
        set => EditBox.Rtf = value;
    }

    public string SelectText => EditBox.SelectedText;
    public override string Text
    {
        get => EditBox.Text;
        set
        {
            EditBox.Font = new Font("Times New Roman", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
            EditBox.Text = value;
            EditBox.Font = new Font("Times New Roman", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        }
    }

    public void SetTextFlag(string value)
    {
        textFlag = value;
        if (textFlag == "天若幽心")
        {
            toolStripToolBar.Items.AddRange(new ToolStripItem[]
            {
                    topmost, Fontstyle, toolStripButtonBold, toolStripButtonColor, toolStripButtonLeft, toolStripButtonFull, toolStripButtonspace, toolStripButtonVoice, toolStripButtonFind, toolStripButtonSend,
                    toolStripButtonNote, toolStripButtonParagraph, toolStripButtonFence, toolStripButtonSplit, toolStripButtonMerge, toolStripButtoncheck, toolStripButtonTrans
            });
            return;
        }
        toolStripToolBar.Items.AddRange(new ToolStripItem[]
        {
                languagle, Fontstyle, toolStripButtonBold, toolStripButtonColor, toolStripButtonLeft, toolStripButtonFull, toolStripButtonspace, toolStripButtonVoice, toolStripButtonFind, toolStripButtonSend,
                toolStripButtonclose
        });
    }

    private void ButtonBoldClick(object o, EventArgs e)
    {
        Font selectionFont = EditBox.SelectionFont;
        if (selectionFont.Bold)
        {
            Font font = new(selectionFont, selectionFont.Style & ~FontStyle.Bold);
            EditBox.SelectionFont = font;
        }
        else
        {
            Font font2 = new(selectionFont, selectionFont.Style | FontStyle.Bold);
            EditBox.SelectionFont = font2;
        }
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void ButtonCheckClick(object o, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        new Thread(new ThreadStart(typoCheckingApi)).Start( );
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void ButtonCheckKeyDown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!checkColor)
            {
                toolStripButtoncheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck2.Image");
                checkColor = true;
                Config.Set("工具栏", "检查", "True");
                return;
            }
            if (checkColor)
            {
                toolStripButtoncheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
                checkColor = false;
                Config.Set("工具栏", "检查", "False");
            }
        }
    }

    private void ButtonCloseClick(object o, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        SendMessage(GetForegroundWindow( ), 786, 511);
    }

    private void ButtonColorClick(object o, EventArgs e)
    {
        EditBox.SelectionColor = toolStripButtonColor.SelectedColor;
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void ButtonFenceClick(object o, EventArgs e)
    {
        if (!File.Exists("cvextern.dll"))
        {
            MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\r\n将下载后的文件与 天若.exe 这个文件放在一起。");
            Process.Start("https://www.lanzous.com/i1ab3vg");
            return;
        }
        SetForegroundWindow(StaticValue.mainhandle);
        if (File.Exists("Data\\分栏预览图.jpg"))
        {
            Process process = new( );
            process.StartInfo.FileName = "Data\\分栏预览图.jpg";
            process.StartInfo.Arguments = "rundl132.exe C://WINDOWS//system32//shimgvw.dll,ImageView";
            process.Start( );
            process.Close( );
        }
    }

    private void ButtonFenceKeydown(object o, MouseEventArgs e)
    {
        if (!File.Exists("cvextern.dll"))
        {
            MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\r\n将下载后的文件与 天若.exe 这个文件放在一起。");
            Process.Start("https://www.lanzous.com/i1ab3vg");
            return;
        }
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!fenceColor)
            {
                toolStripButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence2.Image");
                fenceColor = true;
                Config.Set("工具栏", "分栏", "True");
                return;
            }
            if (fenceColor)
            {
                toolStripButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
                fenceColor = false;
                Config.Set("工具栏", "分栏", "False");
            }
        }
    }

    private void ButtonFullClick(object o, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Justify;
        EditBox.Select(0, 0);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void Form1_DragDrop(object o, DragEventArgs e)
    {
        try
        {
            StaticValue.ImageOCR = Image.FromFile((e.Data.GetData(DataFormats.FileDrop, false) as string[])[0]);
            SendMessage(StaticValue.mainhandle, 786, 580);
        }
        catch (Exception)
        {
            MessageBox.Show("文件格式不正确！", "提醒");
        }
    }

    private void Form1_DragEnter(object o, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.All;
            return;
        }
        e.Effect = DragDropEffects.None;
    }

    private void Form1_MouseEnter(object o, EventArgs e)
    {
    }

    private void IndentTwo(int flag)
    {
        Font font = new(Font.Name, 9f * Helper.System.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        Graphics graphics = CreateGraphics( );
        SizeF sizeF = graphics.MeasureString("中", font);
        EditBox.SelectionIndent = (int) sizeF.Width * 2 * flag;
        EditBox.SelectionHangingIndent = -(int) sizeF.Width * 2 * flag;
        graphics.Dispose( );
    }

    private string PostHTML(string url, string content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        string text = "";
        HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        httpWebRequest.Method = "POST";
        httpWebRequest.Timeout = 3000;
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");
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
        catch { }
        return text;
    }

    private void ReadConfig( )
    {
        string value = Config.Get("工具栏", "顶置");
        if (Config.Get("工具栏", "顶置") == "__ERROR__")
        {
            Config.Set("工具栏", "顶置", "False");
        }
        try
        {
            isTopmost = bool.Parse(value);
        }
        catch
        {
            Config.Set("工具栏", "顶置", "True");
            isTopmost = true;
        }
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (isTopmost)
        {
            topmost.Image = (Image) componentResourceManager.GetObject("main.Image");
            StaticValue.Topmost = true;
        }
        if (!isTopmost)
        {
            topmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
            StaticValue.Topmost = false;
        }
        if (Config.Get("工具栏", "合并") == "__ERROR__")
        {
            Config.Set("工具栏", "合并", "False");
        }
        mergeColor = bool.Parse(Config.Get("工具栏", "合并"));
        if (Config.Get("工具栏", "拆分") == "__ERROR__")
        {
            Config.Set("工具栏", "拆分", "False");
        }
        splitColor = bool.Parse(Config.Get("工具栏", "拆分"));
        if (Config.Get("工具栏", "检查") == "__ERROR__")
        {
            Config.Set("工具栏", "检查", "False");
        }
        checkColor = bool.Parse(Config.Get("工具栏", "检查"));
        if (Config.Get("工具栏", "翻译") == "__ERROR__")
        {
            Config.Set("工具栏", "翻译", "False");
        }
        transColor = bool.Parse(Config.Get("工具栏", "翻译"));
        if (Config.Get("工具栏", "分段") == "__ERROR__")
        {
            Config.Set("工具栏", "分段", "False");
        }
        paragraphColor = bool.Parse(Config.Get("工具栏", "分段"));
        if (Config.Get("工具栏", "分栏") == "__ERROR__")
        {
            Config.Set("工具栏", "分栏", "False");
        }
        fenceColor = bool.Parse(Config.Get("工具栏", "分栏"));
        toolStripButtonFence.Image = fenceColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonFence2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
        toolStripButtonParagraph.Image = paragraphColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonParagraph2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
        toolStripButtoncheck.Image = checkColor
            ? (Image) componentResourceManager.GetObject("toolStripButtoncheck2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
        toolStripButtonMerge.Image = mergeColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonMerge_2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
        toolStripButtonSplit.Image = splitColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonSplit_2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
        if (transColor)
        {
            toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans2.Image");
            return;
        }
        toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
    }

    private void RichBoxMouseDown(object o, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            SetForegroundWindow(StaticValue.mainhandle);
        }
    }

    private void richeditbox_TextChanged(object o, EventArgs e) => undoCmd.Execute(EditBox.Text);

    private void richTextBox1_KeyDown(object o, KeyEventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        if (e.Control && e.KeyCode == Keys.V)
        {
            e.SuppressKeyPress = true;
            EditBox.Paste(DataFormats.GetFormat(DataFormats.Text));
        }
        if (e.Control && e.KeyCode == Keys.Z)
        {
            undoCmd.Undo( );
            EditBox.Text = undoCmd.Record;
        }
        if (e.Control && e.KeyCode == Keys.Y)
        {
            undoCmd.Redo( );
            EditBox.Text = undoCmd.Record;
        }
        if (e.Control && e.KeyCode == Keys.F)
        {
            ReplaceForm replaceForm = new(this);
            if (textFlag == "天若幽心")
            {
                replaceForm.Text = "识别替换";
                replaceForm.Location = PointToScreen(new Point((Width - replaceForm.Width) / 2, (Height - replaceForm.Height) / 2));
            }
            else
            {
                replaceForm.Text = "翻译替换";
                replaceForm.Location = PointToScreen(new Point((Width - replaceForm.Width) / 2, (Height - replaceForm.Height) / 2));
            }
            replaceForm.Show(this);
        }
    }

    private void richTextBox1_LinkClicked(object o, LinkClickedEventArgs e) => Process.Start(e.LinkText);

    private void SetFont黑体(object o, EventArgs e)
    {
        font宋体.ForeColor = Color.Black;
        font黑体.ForeColor = Color.Red;
        font楷体.ForeColor = Color.Black;
        font微软雅黑.ForeColor = Color.Black;
        font新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("黑体", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    private void SetFont楷体(object o, EventArgs e)
    {
        font宋体.ForeColor = Color.Black;
        font黑体.ForeColor = Color.Black;
        font楷体.ForeColor = Color.Red;
        font微软雅黑.ForeColor = Color.Black;
        font新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("STKaiti", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    private void SetFont宋体(object o, EventArgs e)
    {
        font宋体.ForeColor = Color.Red;
        font黑体.ForeColor = Color.Black;
        font楷体.ForeColor = Color.Black;
        font微软雅黑.ForeColor = Color.Black;
        font新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("宋体", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    private void SetFont微软雅黑(object o, EventArgs e)
    {
        font宋体.ForeColor = Color.Black;
        font黑体.ForeColor = Color.Black;
        font楷体.ForeColor = Color.Black;
        font微软雅黑.ForeColor = Color.Red;
        font新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("微软雅黑", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    private void SetFont新罗马(object o, EventArgs e)
    {
        font宋体.ForeColor = Color.Black;
        font黑体.ForeColor = Color.Black;
        font楷体.ForeColor = Color.Black;
        font微软雅黑.ForeColor = Color.Black;
        font新罗马.ForeColor = Color.Red;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("Times New Roman", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }
    private void TextBox1TextChanged(object o, EventArgs e)
        => undoCmd.Execute(EditBox.Text);
    private void toolStripButtonFind_Click(object o, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        ReplaceForm replaceForm = new(this);
        if (textFlag == "天若幽心")
        {
            replaceForm.Text = "识别替换";
            replaceForm.Location = PointToScreen(new Point((Width - replaceForm.Width) / 2, (Height - replaceForm.Height) / 2));
        }
        else
        {
            replaceForm.Text = "翻译替换";
            replaceForm.Location = PointToScreen(new Point((Width - replaceForm.Width) / 2, (Height - replaceForm.Height) / 2));
        }
        replaceForm.Show(this);
    }
    private void toolStripButtonIndent_Click(object o, EventArgs e) => SetForegroundWindow(StaticValue.mainhandle);

    private void toolStripButtonLeft_Click(object o, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Left;
        EditBox.Select(0, 0);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void toolStripButtonMerge_Click(object o, EventArgs e)
    {
        string text = EditBox.Text.TrimEnd(new char[] { '\n' }).TrimEnd(new char[] { '\r' }).TrimEnd(new char[] { '\n' });
        if (text.Split(Environment.NewLine.ToCharArray( )).Length > 1)
        {
            string[] array = text.Split(Environment.NewLine.ToCharArray( ));
            string text2 = "";
            for (int i = 0; i < array.Length - 1; i++)
            {
                string text3 = array[i].Substring(array[i].Length - 1, 1);
                string text4 = array[i + 1].Substring(0, 1);
                if (TextUtils.ContainEn(text3) && TextUtils.ContainEn(text4))
                {
                    text2 = text2 + array[i] + " ";
                }
                else
                {
                    text2 += array[i];
                }
            }
            string text5 = text2.Substring(text2.Length - 1, 1);
            string text6 = array[array.Length - 1].Substring(0, 1);
            if (TextUtils.ContainEn(text5) && TextUtils.ContainEn(text6))
            {
                text2 = text2 + array[array.Length - 1] + " ";
            }
            else
            {
                text2 += array[array.Length - 1];
            }
            EditBox.Text = text2;
        }
        Application.DoEvents( );
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void toolStripButtonMerge_keydown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!mergeColor)
            {
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge_2.Image");
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
                splitColor = false;
                mergeColor = true;
                StaticValue.SetSpilt = false;
                StaticValue.SetMerge = true;
                Config.Set("工具栏", "合并", "True");
                Config.Set("工具栏", "拆分", "False");
                return;
            }
            if (mergeColor)
            {
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
                splitColor = false;
                mergeColor = false;
                StaticValue.SetSpilt = false;
                StaticValue.SetMerge = false;
                Config.Set("工具栏", "合并", "False");
                Config.Set("工具栏", "拆分", "False");
            }
        }
    }

    private void toolStripButtonNote_Click(object o, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        SendMessage(StaticValue.mainhandle, 786, 520);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void toolStripButtonParagraph_Click(object o, EventArgs e)
    {
    }
    private void toolStripButtonParagraph_keydown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!paragraphColor)
            {
                toolStripButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph2.Image");
                paragraphColor = true;
                Config.Set("工具栏", "分段", "True");
                return;
            }
            if (paragraphColor)
            {
                toolStripButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
                paragraphColor = false;
                Config.Set("工具栏", "分段", "False");
            }
        }
    }

    private void toolStripButtonSend_Click(object o, EventArgs e)
    {
        Clipboard.SetDataObject(EditBox.Text);
        SendMessage(GetForegroundWindow( ), 786, 530);
        keybd_event(Keys.ControlKey, 0, 0U, 0U);
        keybd_event(Keys.V, 0, 0U, 0U);
        keybd_event(Keys.V, 0, 2U, 0U);
        keybd_event(Keys.ControlKey, 0, 2U, 0U);
        FmFlags.Display("已复制");
    }

    private void toolStripButtonspace_Click(object o, EventArgs e)
    {
        if (toolSpace)
        {
            EditBox.SelectAll( );
            IndentTwo(0);
            EditBox.Select(0, 0);
            toolSpace = false;
        }
        else
        {
            EditBox.SelectAll( );
            IndentTwo(1);
            EditBox.Select(0, 0);
            toolSpace = true;
        }
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void toolStripButtonSplit_Click(object o, EventArgs e)
    {
        EditBox.Text = StaticValue.Split;
        Application.DoEvents( );
        SetForegroundWindow(StaticValue.mainhandle);
    }
    private void toolStripButtonSplit_keydown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!splitColor)
            {
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit_2.Image");
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
                splitColor = true;
                mergeColor = false;
                StaticValue.SetSpilt = true;
                StaticValue.SetMerge = false;
                Config.Set("工具栏", "拆分", "True");
                Config.Set("工具栏", "合并", "False");
                return;
            }
            if (splitColor)
            {
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
                splitColor = false;
                mergeColor = false;
                StaticValue.SetSpilt = false;
                StaticValue.SetMerge = false;
                Config.Set("工具栏", "合并", "False");
                Config.Set("工具栏", "拆分", "False");
            }
        }
    }

    private void toolStripButtonTrans_Click(object o, EventArgs e)
    {
        SendMessage(StaticValue.mainhandle, 786, 512);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void toolStripButtontrans_keydown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!transColor)
            {
                toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans2.Image");
                transColor = true;
                Config.Set("工具栏", "翻译", "True");
                return;
            }
            if (transColor)
            {
                toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
                transColor = false;
                Config.Set("工具栏", "翻译", "False");
            }
        }
    }

    private void toolStripButtonVoice_Click(object o, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        SendMessage(StaticValue.mainhandle, 786, 518);
        SetForegroundWindow(StaticValue.mainhandle);
    }
    private void toolStripToolBar_Click(object o, EventArgs e)
    {
    }

    private void topmost_keydown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Left)
        {
            if (!isTopmost)
            {
                topmost.Image = (Image) componentResourceManager.GetObject("main.Image");
                StaticValue.Topmost = true;
                isTopmost = true;
                Config.Set("工具栏", "顶置", "True");
                SendMessage(StaticValue.mainhandle, 600, 725);
                return;
            }
            topmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
            StaticValue.Topmost = false;
            isTopmost = false;
            Config.Set("工具栏", "顶置", "False");
            SendMessage(StaticValue.mainhandle, 600, 725);
        }
    }

    private void typoCheckingApi( )
    {
        EditBox.SelectAll( );
        EditBox.SelectionColor = Color.Black;
        EditBox.Select(0, 0);
        try
        {
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(PostHTML("http://www.cuobiezi.net/api/v1/zh_spellcheck/client/pos/json", "{\"check_mode\": \"value2\",\"content\": \"" + EditBox.Text + "\", \"content2\": \"value1\",  \"doc_type\": \"value2\",\"method\": \"value2\",\"return_format\": \"value2\",\"username\": \"tianruoyouxin\"}")))["Cases"].ToString( ));
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                int num = 0;
                int length = EditBox.Text.Length;
                for (int num2 = EditBox.Find(jobject["Error"].ToString( ), num, length, RichTextBoxFinds.None); num2 != -1; num2 = EditBox.Find(jobject["Error"].ToString( ), num, length, RichTextBoxFinds.None))
                {
                    EditBox.SelectionColor = Color.Red;
                    num = num2 + jobject["Error"].ToString( ).Length;
                }
            }
        }
        catch { }
        EditBox.Select(0, 0);
    }
    private void zh_en_Click(object o, EventArgs e)
    {
        zh_en.ForeColor = Color.Red;
        zh_jp.ForeColor = Color.Black;
        zh_ko.ForeColor = Color.Black;
        StaticValue.Zh2En = true;
        StaticValue.Zh2Jp = false;
        StaticValue.Zh2Ko = false;
        SendMessage(StaticValue.mainhandle, 786, 512);
    }

    private void zh_jp_Click(object o, EventArgs e)
    {
        zh_en.ForeColor = Color.Black;
        zh_jp.ForeColor = Color.Red;
        zh_ko.ForeColor = Color.Black;
        StaticValue.Zh2En = false;
        StaticValue.Zh2Jp = true;
        StaticValue.Zh2Ko = false;
        SendMessage(StaticValue.mainhandle, 786, 512);
    }

    private void zh_ko_Click(object o, EventArgs e)
    {
        zh_en.ForeColor = Color.Black;
        zh_jp.ForeColor = Color.Black;
        zh_ko.ForeColor = Color.Red;
        StaticValue.Zh2En = false;
        StaticValue.Zh2Jp = false;
        StaticValue.Zh2Ko = true;
        SendMessage(StaticValue.mainhandle, 786, 512);
    }
}
