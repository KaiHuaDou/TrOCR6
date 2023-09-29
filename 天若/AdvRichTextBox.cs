using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrOCR.Controls;
using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

[Description("Provides a user control that allows the user to edit HTML page.")]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
public class AdvRichTextBox : UserControl
{
    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose( );
        }
        base.Dispose(disposing);
    }

    public void InitializeComponent( )
    {
        CheckForIllegalCrossThreadCalls = false;
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        font_宋体 = new ToolStripMenuItem( );
        font_楷体 = new ToolStripMenuItem( );
        font_黑体 = new ToolStripMenuItem( );
        font_微软雅黑 = new ToolStripMenuItem( );
        font_新罗马 = new ToolStripMenuItem( );
        zh_jp = new ToolStripMenuItem( );
        zh_ko = new ToolStripMenuItem( );
        zh_en = new ToolStripMenuItem( );
        mode_顶置 = new ToolStripMenuItem( );
        mode_正常 = new ToolStripMenuItem( );
        mode_合并 = new ToolStripMenuItem( );
        topmost = new ToolStripButton( );
        languagle = new ToolStripDropDownButton( );
        mode = new ToolStripDropDownButton( );
        Fontstyle = new ToolStripDropDownButton( );
        toolStripToolBar = new ToolStripEx( );
        toolStripButtonclose = new ToolStripButton( );
        toolStripButtonBold = new ToolStripButton( );
        toolStripButtonParagraph = new ToolStripButton( );
        toolStripButtonFind = new ToolStripButton( );
        toolStripButtonColor = new ColorPicker( );
        toolStripSeparatorFont = new ToolStripSeparator( );
        toolStripButtonFence = new ToolStripButton( );
        toolStripButtonSplit = new ToolStripButton( );
        toolStripButtoncheck = new ToolStripButton( );
        toolStripButtonIndent = new ToolStripButton( );
        toolStripSeparatorFormat = new ToolStripSeparator( );
        toolStripButtonLeft = new ToolStripButton( );
        toolStripButtonMerge = new ToolStripButton( );
        toolStripButtonVoice = new ToolStripButton( );
        toolStripButtonFull = new ToolStripButton( );
        toolStripSeparatorAlign = new ToolStripSeparator( );
        toolStripButtonspace = new ToolStripButton( );
        toolStripButtonR_arow = new ToolStripButton( );
        toolStripButtonSend = new ToolStripButton( );
        toolStripButtonTrans = new ToolStripButton( );
        toolStripButtonNote = new ToolStripButton( );
        EditBox = new RichTextBoxEx( );
        toolStripToolBar.SuspendLayout( );
        SuspendLayout( );
        toolStripSeparatorFont.ForeColor = Color.White;
        toolStripToolBar.GripStyle = ToolStripGripStyle.Hidden;
        toolStripToolBar.Location = new Point(0, 0);
        toolStripToolBar.Name = "toolStripToolBar";
        toolStripToolBar.RenderMode = ToolStripRenderMode.System;
        toolStripToolBar.Size = new Size(600, 25);
        toolStripToolBar.TabIndex = 1;
        toolStripToolBar.Click += toolStripToolBar_Click;
        toolStripToolBar.Text = "Tool Bar";
        toolStripToolBar.BackColor = Color.White;
        toolStripToolBar.Renderer = new MenuItemRenderer( );
        toolStripButtonBold.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonBold.Image = (Image) componentResourceManager.GetObject("toolStripButtonBold.Image");
        toolStripButtonBold.ImageTransparentColor = Color.Magenta;
        toolStripButtonBold.Name = "toolStripButtonBold";
        toolStripButtonBold.Size = new Size(23, 22);
        toolStripButtonBold.Text = "加粗";
        toolStripButtonBold.Click += toolStripButtonBold_Click;
        toolStripButtonParagraph.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
        toolStripButtonParagraph.ImageTransparentColor = Color.Magenta;
        toolStripButtonParagraph.Name = "toolStripButtonParagraph";
        toolStripButtonParagraph.Size = new Size(23, 22);
        toolStripButtonParagraph.Text = "依据位置自动分段\r\n仅支持搜狗接口\r\n适合段落识别\r\n图片越清晰越准确\r\n准确度98%以上";
        toolStripButtonParagraph.Click += toolStripButtonParagraph_Click;
        toolStripButtonParagraph.MouseDown += toolStripButtonParagraph_keydown;
        toolStripButtonFind.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonFind.Image = (Image) componentResourceManager.GetObject("toolStripButtonFind.Image");
        toolStripButtonFind.ImageTransparentColor = Color.Magenta;
        toolStripButtonFind.Name = "toolStripButtonFind";
        toolStripButtonFind.Size = new Size(23, 22);
        toolStripButtonFind.Text = "查找\\替换";
        toolStripButtonFind.Click += toolStripButtonFind_Click;
        toolStripButtonColor.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonColor.Image = (Image) componentResourceManager.GetObject("toolStripButtonColor.Image");
        toolStripButtonColor.ImageTransparentColor = Color.Magenta;
        toolStripButtonColor.Name = "toolStripButtonColor";
        toolStripButtonColor.Size = new Size(23, 22);
        toolStripButtonColor.Text = "字体颜色";
        toolStripButtonColor.Click += toolStripButtonColor_Click;
        toolStripButtonLeft.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonLeft.Image = (Image) componentResourceManager.GetObject("toolStripButtonLeft.Image");
        toolStripButtonLeft.ImageTransparentColor = Color.Magenta;
        toolStripButtonLeft.Name = "toolStripButtonLeft";
        toolStripButtonLeft.Size = new Size(23, 22);
        toolStripButtonLeft.Text = "左对齐";
        toolStripButtonLeft.Click += toolStripButtonLeft_Click;
        toolStripButtonMerge.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
        toolStripButtonMerge.ImageTransparentColor = Color.Magenta;
        toolStripButtonMerge.Name = "toolStripButtonMerge";
        toolStripButtonMerge.Size = new Size(23, 22);
        toolStripButtonMerge.Text = "将文本合并成一段";
        toolStripButtonMerge.Click += toolStripButtonMerge_Click;
        toolStripButtonMerge.MouseDown += toolStripButtonMerge_keydown;
        toolStripButtonVoice.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonVoice.Image = (Image) componentResourceManager.GetObject("toolStripButtonVoice.Image");
        toolStripButtonVoice.ImageTransparentColor = Color.Magenta;
        toolStripButtonVoice.Name = "toolStripButtonVoice";
        toolStripButtonVoice.Size = new Size(23, 22);
        toolStripButtonVoice.Text = "朗读";
        toolStripButtonVoice.Click += toolStripButtonVoice_Click;
        toolStripButtonFull.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonFull.Image = (Image) componentResourceManager.GetObject("toolStripButtonFull.Image");
        toolStripButtonFull.ImageTransparentColor = Color.Magenta;
        toolStripButtonFull.Name = "toolStripButtonFull";
        toolStripButtonFull.Size = new Size(23, 22);
        toolStripButtonFull.Text = "两端对齐";
        toolStripButtonFull.Click += toolStripButtonFull_Click;
        toolStripButtonspace.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonspace.Image = (Image) componentResourceManager.GetObject("toolStripButtonspace.Image");
        toolStripButtonspace.ImageTransparentColor = Color.Magenta;
        toolStripButtonspace.Name = "toolStripButtonLine";
        toolStripButtonspace.Size = new Size(23, 22);
        toolStripButtonspace.Text = "首行缩进";
        toolStripButtonspace.Click += toolStripButtonspace_Click;
        toolStripButtonFence.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
        toolStripButtonFence.ImageTransparentColor = Color.Magenta;
        toolStripButtonFence.Name = "toolStripButtonformat";
        toolStripButtonFence.Size = new Size(23, 22);
        toolStripButtonFence.Text = "截图时自动分栏\r\n多选区时无效\r\n单击显示分栏示意图";
        toolStripButtonFence.Click += toolStripButtonFence_Click;
        toolStripButtonFence.MouseDown += toolStripButtonFence_keydown;
        toolStripButtonSend.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonSend.Image = (Image) componentResourceManager.GetObject("toolStripButtonSend.Image");
        toolStripButtonSend.ImageTransparentColor = Color.Magenta;
        toolStripButtonSend.Name = "toolStripButtonSend";
        toolStripButtonSend.Size = new Size(23, 22);
        toolStripButtonSend.Text = "复制/发送";
        toolStripButtonSend.Click += toolStripButtonSend_Click;
        toolStripButtonSplit.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
        toolStripButtonSplit.ImageTransparentColor = Color.Magenta;
        toolStripButtonSplit.Name = "toolStripButtonSplit";
        toolStripButtonSplit.Size = new Size(23, 22);
        toolStripButtonSplit.Text = "按图片中的行进行拆分";
        toolStripButtonSplit.Click += toolStripButtonSplit_Click;
        toolStripButtonSplit.MouseDown += toolStripButtonSplit_keydown;
        toolStripButtoncheck.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtoncheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
        toolStripButtoncheck.ImageTransparentColor = Color.Magenta;
        toolStripButtoncheck.Name = "toolStripButtoncheck";
        toolStripButtoncheck.Size = new Size(23, 22);
        toolStripButtoncheck.Text = "检查文本是否有错别字";
        toolStripButtoncheck.Click += toolStripButtoncheck_Click;
        toolStripButtoncheck.MouseDown += toolStripButtoncheck_keydown;
        toolStripButtonTrans.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
        toolStripButtonTrans.ImageTransparentColor = Color.Magenta;
        toolStripButtonTrans.Name = "toolStripButtonTrans";
        toolStripButtonTrans.Size = new Size(23, 22);
        toolStripButtonTrans.Text = "翻译";
        toolStripButtonTrans.Click += toolStripButtonTrans_Click;
        toolStripButtonTrans.MouseDown += toolStripButtontrans_keydown;
        toolStripButtonNote.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonNote.Image = (Image) componentResourceManager.GetObject("toolStripButtonNote.Image");
        toolStripButtonNote.ImageTransparentColor = Color.Magenta;
        toolStripButtonNote.Name = "toolStripButtonTrans";
        toolStripButtonNote.Size = new Size(23, 22);
        toolStripButtonNote.Text = "记录窗体";
        toolStripButtonNote.Click += toolStripButtonNote_Click;
        toolStripButtonclose.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolStripButtonclose.Image = (Image) componentResourceManager.GetObject("toolStripButtonclose.Image");
        toolStripButtonclose.ImageTransparentColor = Color.Magenta;
        toolStripButtonclose.Name = "toolStripButtonclose";
        toolStripButtonclose.Size = new Size(23, 22);
        toolStripButtonclose.Text = "关闭";
        toolStripButtonclose.Click += toolStripButtonclose_Click;
        languagle.DisplayStyle = ToolStripItemDisplayStyle.Image;
        languagle.Image = (Image) componentResourceManager.GetObject("languagle.Image");
        languagle.ImageTransparentColor = Color.Magenta;
        languagle.Name = "toolStripButtonclose";
        languagle.Size = new Size(23, 22);
        languagle.Text = "选择翻译语言\r\n支持自动检测\r\n可以双向翻译";
        zh_en.Text = "中⇆英";
        zh_en.ForeColor = Color.Red;
        zh_en.Click += zh_en_Click;
        zh_jp.Text = "中⇆日";
        zh_jp.ForeColor = Color.Black;
        zh_jp.Click += zh_jp_Click;
        zh_ko.Text = "中⇆韩";
        zh_ko.ForeColor = Color.Black;
        zh_ko.Click += zh_ko_Click;
        languagle.DropDownItems.Add(zh_en);
        languagle.DropDownItems.Add(zh_jp);
        languagle.DropDownItems.Add(zh_ko);
        languagle.AutoSize = false;
        ((ToolStripDropDownMenu) languagle.DropDown).ShowImageMargin = false;
        languagle.DropDown.BackColor = Color.White;
        languagle.DropDown.AutoSize = false;
        languagle.DropDown.AutoSize = Program.DpiFactor != 1f;
        languagle.DropDown.Width = Convert.ToInt32(55f);
        languagle.DropDown.Height = Convert.ToInt32(70f);
        languagle.ShowDropDownArrow = false;
        topmost.DisplayStyle = ToolStripItemDisplayStyle.Image;
        topmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
        topmost.ImageTransparentColor = Color.Magenta;
        topmost.Name = "toolStripButtonclose";
        topmost.Size = new Size(23, 22);
        topmost.Text = "顶置";
        topmost.MouseDown += topmost_keydown;
        Fontstyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
        Fontstyle.Image = (Image) componentResourceManager.GetObject("Fontstyle.Image");
        Fontstyle.ImageTransparentColor = Color.Magenta;
        Fontstyle.Name = "toolStripButtonclose";
        Fontstyle.Size = new Size(23, 22);
        Fontstyle.Text = "字体";
        Fontstyle.AutoSize = false;
        ((ToolStripDropDownMenu) Fontstyle.DropDown).ShowImageMargin = false;
        Fontstyle.DropDown.BackColor = Color.White;
        Fontstyle.DropDown.AutoSize = false;
        Fontstyle.DropDown.AutoSize = Program.DpiFactor != 1f;
        Fontstyle.DropDown.Width = Convert.ToInt32(123f);
        Fontstyle.DropDown.Height = Convert.ToInt32(115f);
        Fontstyle.ShowDropDownArrow = false;
        font_宋体.Text = "宋体";
        font_宋体.ForeColor = Color.Black;
        font_宋体.Click += font_宋体c;
        font_黑体.Text = "黑体";
        font_黑体.ForeColor = Color.Black;
        font_黑体.Click += font_黑体c;
        font_楷体.Text = "楷体";
        font_楷体.ForeColor = Color.Black;
        font_楷体.Click += font_楷体c;
        font_微软雅黑.Text = "微软雅黑";
        font_微软雅黑.ForeColor = Color.Black;
        font_微软雅黑.Click += font_微软雅黑c;
        font_新罗马.Text = "Time New Roman";
        font_新罗马.ForeColor = Color.Red;
        font_新罗马.Click += font_新罗马c;
        Fontstyle.DropDownItems.Add(font_宋体);
        Fontstyle.DropDownItems.Add(font_黑体);
        Fontstyle.DropDownItems.Add(font_楷体);
        Fontstyle.DropDownItems.Add(font_微软雅黑);
        Fontstyle.DropDownItems.Add(font_新罗马);
        EditBox.Location = new Point(32, 13);
        EditBox.Name = "richTextBox1";
        EditBox.Size = new Size(603, 457);
        EditBox.TabIndex = 0;
        EditBox.DetectUrls = true;
        EditBox.HideSelection = false;
        EditBox.Text = "";
        EditBox.BorderStyle = BorderStyle.None;
        EditBox.Dock = DockStyle.Fill;
        EditBox.Multiline = true;
        EditBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        EditBox.KeyDown += richTextBox1_KeyDown;
        EditBox.LinkClicked += richTextBox1_LinkClicked;
        EditBox.MouseDown += richtextbox1_MouseDown;
        EditBox.AllowDrop = true;
        EditBox.MouseEnter += Form1_MouseEnter;
        EditBox.DragEnter += Form1_DragEnter;
        EditBox.DragDrop += Form1_DragDrop;
        EditBox.SelectionAlignment = TextAlign.Justify;
        EditBox.Font = new Font("Times New Roman", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        EditBox.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
        EditBox.TextChanged += richeditbox_TextChanged;
        EditBox.Cursor = Cursors.IBeam;
        indent_two(1);
        mode.Font = new Font("微软雅黑", 9f * Program.DpiFactor, FontStyle.Regular);
        languagle.Font = new Font("微软雅黑", 9f * Program.DpiFactor, FontStyle.Regular);
        Fontstyle.Font = new Font("微软雅黑", 9f * Program.DpiFactor, FontStyle.Regular);
        AutoScaleMode = AutoScaleMode.None;
        Controls.Add(EditBox);
        Controls.Add(toolStripToolBar);
        Name = "richTextBox";
        base.Text = "richTextBox";
        Size = new Size(600, 300);
        toolStripToolBar.ResumeLayout(false);
        toolStripToolBar.PerformLayout( );
        ResumeLayout(false);
        PerformLayout( );
    }

    public AdvRichTextBox( )
    {
        toolspace = true;
        toolFull = true;
        c = new cmd(50);
        Font = new Font(Font.Name, 9f / StaticValue.Dpifactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        InitializeComponent( );
        readIniFile( );
        EditBox.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
    }

    public override string Text
    {
        get => EditBox.Text;
        set
        {
            EditBox.Font = new Font("Times New Roman", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
            EditBox.Text = value;
            EditBox.Font = new Font("Times New Roman", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        }
    }

    public void toolStripButtonBold_Click(object sender, EventArgs e)
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

    public void toolStripButtonParagraph_Click(object sender, EventArgs e)
    {
    }

    public void toolStripButtonFind_Click(object sender, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        ReplaceForm replaceForm = new(this);
        if (txt_flag == "天若幽心")
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

    public void toolStripButtonColor_Click(object sender, EventArgs e)
    {
        EditBox.SelectionColor = toolStripButtonColor.SelectedColor;
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtonFence_Click(object sender, EventArgs e)
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

    public void toolStripButtonSplit_Click(object sender, EventArgs e)
    {
        EditBox.Text = StaticValue.Split;
        Application.DoEvents( );
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtoncheck_Click(object sender, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        new Thread(new ThreadStart(错别字检查API)).Start( );
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtonIndent_Click(object sender, EventArgs e) => SetForegroundWindow(StaticValue.mainhandle);

    public void toolStripButtonLeft_Click(object sender, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Left;
        EditBox.Select(0, 0);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtonMerge_Click(object sender, EventArgs e)
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
                if (contain_en(text3) && contain_en(text4))
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
            if (contain_en(text5) && contain_en(text6))
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

    public void toolStripButtonVoice_Click(object sender, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        SendMessage(StaticValue.mainhandle, 786, 518);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtonFull_Click(object sender, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Justify;
        EditBox.Select(0, 0);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtonspace_Click(object sender, EventArgs e)
    {
        if (toolspace)
        {
            EditBox.SelectAll( );
            indent_two(0);
            EditBox.Select(0, 0);
            toolspace = false;
        }
        else
        {
            EditBox.SelectAll( );
            indent_two(1);
            EditBox.Select(0, 0);
            toolspace = true;
        }
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripButtonSend_Click(object sender, EventArgs e)
    {
        Clipboard.SetDataObject(EditBox.Text);
        SendMessage(GetForegroundWindow( ), 786, 530);
        keybd_event(Keys.ControlKey, 0, 0U, 0U);
        keybd_event(Keys.V, 0, 0U, 0U);
        keybd_event(Keys.V, 0, 2U, 0U);
        keybd_event(Keys.ControlKey, 0, 2U, 0U);
        FmFlags fmFlags = new( );
        fmFlags.Show( );
        fmFlags.DrawStr("已复制");
    }

    public ContextMenuStrip ContextMenuStrip1
    {
        get => EditBox.ContextMenuStrip;
        set => EditBox.ContextMenuStrip = value;
    }

    public string Text_flag
    {
        set
        {
            txt_flag = value;
            if (txt_flag == "天若幽心")
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
    }

    public void toolStripButtonclose_Click(object sender, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        SendMessage(GetForegroundWindow( ), 786, 511);
    }

    public void toolStripButtonTrans_Click(object sender, EventArgs e)
    {
        SendMessage(StaticValue.mainhandle, 786, 512);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    public void toolStripToolBar_Click(object sender, EventArgs e)
    {
    }

    private void zh_en_Click(object sender, EventArgs e)
    {
        zh_en.ForeColor = Color.Red;
        zh_jp.ForeColor = Color.Black;
        zh_ko.ForeColor = Color.Black;
        StaticValue.Zh2En = true;
        StaticValue.Zh2Jp = false;
        StaticValue.Zh2Ko = false;
        SendMessage(StaticValue.mainhandle, 786, 512);
    }

    private void zh_jp_Click(object sender, EventArgs e)
    {
        zh_en.ForeColor = Color.Black;
        zh_jp.ForeColor = Color.Red;
        zh_ko.ForeColor = Color.Black;
        StaticValue.Zh2En = false;
        StaticValue.Zh2Jp = true;
        StaticValue.Zh2Ko = false;
        SendMessage(StaticValue.mainhandle, 786, 512);
    }

    private void zh_ko_Click(object sender, EventArgs e)
    {
        zh_en.ForeColor = Color.Black;
        zh_jp.ForeColor = Color.Black;
        zh_ko.ForeColor = Color.Red;
        StaticValue.Zh2En = false;
        StaticValue.Zh2Jp = false;
        StaticValue.Zh2Ko = true;
        SendMessage(StaticValue.mainhandle, 786, 512);
    }

    public void font_宋体c(object sender, EventArgs e)
    {
        font_宋体.ForeColor = Color.Red;
        font_黑体.ForeColor = Color.Black;
        font_楷体.ForeColor = Color.Black;
        font_微软雅黑.ForeColor = Color.Black;
        font_新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("宋体", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    public void font_黑体c(object sender, EventArgs e)
    {
        font_宋体.ForeColor = Color.Black;
        font_黑体.ForeColor = Color.Red;
        font_楷体.ForeColor = Color.Black;
        font_微软雅黑.ForeColor = Color.Black;
        font_新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("黑体", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    public void font_楷体c(object sender, EventArgs e)
    {
        font_宋体.ForeColor = Color.Black;
        font_黑体.ForeColor = Color.Black;
        font_楷体.ForeColor = Color.Red;
        font_微软雅黑.ForeColor = Color.Black;
        font_新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("STKaiti", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    public void font_微软雅黑c(object sender, EventArgs e)
    {
        font_宋体.ForeColor = Color.Black;
        font_黑体.ForeColor = Color.Black;
        font_楷体.ForeColor = Color.Black;
        font_微软雅黑.ForeColor = Color.Red;
        font_新罗马.ForeColor = Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("微软雅黑", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    public void font_新罗马c(object sender, EventArgs e)
    {
        font_宋体.ForeColor = Color.Black;
        font_黑体.ForeColor = Color.Black;
        font_楷体.ForeColor = Color.Black;
        font_微软雅黑.ForeColor = Color.Black;
        font_新罗马.ForeColor = Color.Red;
        string text = EditBox.Text;
        EditBox.Text = "";
        Font font = new("Times New Roman", 16f * Program.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Font = font;
        EditBox.Text = text;
    }

    public void indent_two(int fla)
    {
        Font font = new(Font.Name, 9f * Program.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        Graphics graphics = CreateGraphics( );
        SizeF sizeF = graphics.MeasureString("中", font);
        EditBox.SelectionIndent = (int) sizeF.Width * 2 * fla;
        EditBox.SelectionHangingIndent = -(int) sizeF.Width * 2 * fla;
        graphics.Dispose( );
    }

    private void richeditbox_TextChanged(object sender, EventArgs e) => c.execute(EditBox.Text);

    private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e) => Process.Start(e.LinkText);

    public string SelectText => EditBox.SelectedText;

    private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        if (e.Control && e.KeyCode == Keys.V)
        {
            e.SuppressKeyPress = true;
            EditBox.Paste(DataFormats.GetFormat(DataFormats.Text));
        }
        if (e.Control && e.KeyCode == Keys.Z)
        {
            c.undo( );
            EditBox.Text = c.Record;
        }
        if (e.Control && e.KeyCode == Keys.Y)
        {
            c.redo( );
            EditBox.Text = c.Record;
        }
        if (e.Control && e.KeyCode == Keys.F)
        {
            ReplaceForm replaceForm = new(this);
            if (txt_flag == "天若幽心")
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

    public string Find
    {
        set
        {
            new Thread(new ThreadStart(错别字检查API)).Start( );
            SetForegroundWindow(StaticValue.mainhandle);
        }
    }

    public void 错别字检查API( )
    {
        EditBox.SelectAll( );
        EditBox.SelectionColor = Color.Black;
        EditBox.Select(0, 0);
        try
        {
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(Post_Html("http://www.cuobiezi.net/api/v1/zh_spellcheck/client/pos/json", "{\"check_mode\": \"value2\",\"content\": \"" + EditBox.Text + "\", \"content2\": \"value1\",  \"doc_type\": \"value2\",\"method\": \"value2\",\"return_format\": \"value2\",\"username\": \"tianruoyouxin\"}")))["Cases"].ToString( ));
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
            EditBox.Select(0, 0);
        }
        catch
        {
            EditBox.Select(0, 0);
        }
    }

    public string Post_Html(string url, string post_str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(post_str);
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
        catch
        {
        }
        return text;
    }

    public void richtextbox1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            SetForegroundWindow(StaticValue.mainhandle);
        }
    }

    public void toolStripButtonNote_Click(object sender, EventArgs e)
    {
        SetForegroundWindow(StaticValue.mainhandle);
        SendMessage(StaticValue.mainhandle, 786, 520);
        SetForegroundWindow(StaticValue.mainhandle);
    }

    private void Form1_MouseEnter(object sender, EventArgs e)
    {
    }

    private void Form1_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.All;
            return;
        }
        e.Effect = DragDropEffects.None;
    }

    private void Form1_DragDrop(object sender, DragEventArgs e)
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

    public static bool contain_en(string str) => Regex.IsMatch(str, "[a-zA-Z]");

    public void TextBox1TextChanged(object sender, EventArgs e) => c.execute(EditBox.Text);

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

    public void toolStripButtonSplit_keydown(object sender, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!splitcolor)
            {
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit_2.Image");
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
                splitcolor = true;
                mergecolor = false;
                StaticValue.SetSpilt = true;
                StaticValue.SetMerge = false;
                Config.Set("工具栏", "拆分", "True");
                Config.Set("工具栏", "合并", "False");
                return;
            }
            if (splitcolor)
            {
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
                splitcolor = false;
                mergecolor = false;
                StaticValue.SetSpilt = false;
                StaticValue.SetMerge = false;
                Config.Set("工具栏", "合并", "False");
                Config.Set("工具栏", "拆分", "False");
            }
        }
    }

    public void toolStripButtonMerge_keydown(object sender, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!mergecolor)
            {
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge_2.Image");
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
                splitcolor = false;
                mergecolor = true;
                StaticValue.SetSpilt = false;
                StaticValue.SetMerge = true;
                Config.Set("工具栏", "合并", "True");
                Config.Set("工具栏", "拆分", "False");
                return;
            }
            if (mergecolor)
            {
                toolStripButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
                toolStripButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
                splitcolor = false;
                mergecolor = false;
                StaticValue.SetSpilt = false;
                StaticValue.SetMerge = false;
                Config.Set("工具栏", "合并", "False");
                Config.Set("工具栏", "拆分", "False");
            }
        }
    }

    public void topmost_keydown(object sender, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Left)
        {
            if (!topmost_flag)
            {
                topmost.Image = (Image) componentResourceManager.GetObject("main.Image");
                StaticValue.Topmost = true;
                topmost_flag = true;
                Config.Set("工具栏", "顶置", "True");
                SendMessage(StaticValue.mainhandle, 600, 725);
                return;
            }
            topmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
            StaticValue.Topmost = false;
            topmost_flag = false;
            Config.Set("工具栏", "顶置", "False");
            SendMessage(StaticValue.mainhandle, 600, 725);
        }
    }

    public void readIniFile( )
    {
        string value = Config.Get("工具栏", "顶置");
        if (Config.Get("工具栏", "顶置") == "__ERROR__")
        {
            Config.Set("工具栏", "顶置", "False");
        }
        try
        {
            topmost_flag = bool.Parse(value);
        }
        catch
        {
            Config.Set("工具栏", "顶置", "True");
            topmost_flag = true;
        }
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (topmost_flag)
        {
            topmost.Image = (Image) componentResourceManager.GetObject("main.Image");
            StaticValue.Topmost = true;
        }
        if (!topmost_flag)
        {
            topmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
            StaticValue.Topmost = false;
        }
        if (Config.Get("工具栏", "合并") == "__ERROR__")
        {
            Config.Set("工具栏", "合并", "False");
        }
        mergecolor = bool.Parse(Config.Get("工具栏", "合并"));
        if (Config.Get("工具栏", "拆分") == "__ERROR__")
        {
            Config.Set("工具栏", "拆分", "False");
        }
        splitcolor = bool.Parse(Config.Get("工具栏", "拆分"));
        if (Config.Get("工具栏", "检查") == "__ERROR__")
        {
            Config.Set("工具栏", "检查", "False");
        }
        checkcolor = bool.Parse(Config.Get("工具栏", "检查"));
        if (Config.Get("工具栏", "翻译") == "__ERROR__")
        {
            Config.Set("工具栏", "翻译", "False");
        }
        transcolor = bool.Parse(Config.Get("工具栏", "翻译"));
        if (Config.Get("工具栏", "分段") == "__ERROR__")
        {
            Config.Set("工具栏", "分段", "False");
        }
        Paragraphcolor = bool.Parse(Config.Get("工具栏", "分段"));
        if (Config.Get("工具栏", "分栏") == "__ERROR__")
        {
            Config.Set("工具栏", "分栏", "False");
        }
        Fencecolor = bool.Parse(Config.Get("工具栏", "分栏"));
        toolStripButtonFence.Image = Fencecolor
            ? (Image) componentResourceManager.GetObject("toolStripButtonFence2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
        toolStripButtonParagraph.Image = Paragraphcolor
            ? (Image) componentResourceManager.GetObject("toolStripButtonParagraph2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
        toolStripButtoncheck.Image = checkcolor
            ? (Image) componentResourceManager.GetObject("toolStripButtoncheck2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
        toolStripButtonMerge.Image = mergecolor
            ? (Image) componentResourceManager.GetObject("toolStripButtonMerge_2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
        toolStripButtonSplit.Image = splitcolor
            ? (Image) componentResourceManager.GetObject("toolStripButtonSplit_2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
        if (transcolor)
        {
            toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans2.Image");
            return;
        }
        toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
    }

    public void saveIniFile( ) => Config.Set("工具栏", "顶置", topmost_flag.ToString( ));

    public void toolStripButtoncheck_keydown(object sender, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!checkcolor)
            {
                toolStripButtoncheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck2.Image");
                checkcolor = true;
                Config.Set("工具栏", "检查", "True");
                return;
            }
            if (checkcolor)
            {
                toolStripButtoncheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
                checkcolor = false;
                Config.Set("工具栏", "检查", "False");
            }
        }
    }

    public void toolStripButtontrans_keydown(object sender, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!transcolor)
            {
                toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans2.Image");
                transcolor = true;
                Config.Set("工具栏", "翻译", "True");
                return;
            }
            if (transcolor)
            {
                toolStripButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
                transcolor = false;
                Config.Set("工具栏", "翻译", "False");
            }
        }
    }

    public void toolStripButtonParagraph_keydown(object sender, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!Paragraphcolor)
            {
                toolStripButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph2.Image");
                Paragraphcolor = true;
                Config.Set("工具栏", "分段", "True");
                return;
            }
            if (Paragraphcolor)
            {
                toolStripButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
                Paragraphcolor = false;
                Config.Set("工具栏", "分段", "False");
            }
        }
    }

    public void toolStripButtonFence_keydown(object sender, MouseEventArgs e)
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
            if (!Fencecolor)
            {
                toolStripButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence2.Image");
                Fencecolor = true;
                Config.Set("工具栏", "分栏", "True");
                return;
            }
            if (Fencecolor)
            {
                toolStripButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
                Fencecolor = false;
                Config.Set("工具栏", "分栏", "False");
            }
        }
    }

    public string Rtf
    {
        set => EditBox.Rtf = value;
    }

    public string rtf
    {
        get => EditBox.Rtf;
        set => EditBox.Rtf = value;
    }

    public string set_language
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

    public IContainer components;

    public ToolStripButton toolStripButtonclose;

    public ToolStripButton toolStripButtonBold;

    public ToolStripButton toolStripButtonParagraph;

    public ToolStripButton toolStripButtonFind;

    public ToolStripSeparator toolStripSeparatorFont;

    public ToolStripButton toolStripButtonFence;

    public ToolStripButton toolStripButtonSplit;

    public ToolStripButton toolStripButtoncheck;

    public ToolStripButton toolStripButtonIndent;

    public ToolStripSeparator toolStripSeparatorFormat;

    public ToolStripButton toolStripButtonLeft;

    public ToolStripButton toolStripButtonMerge;

    public ToolStripButton toolStripButtonVoice;

    public ToolStripButton toolStripButtonFull;

    public ToolStripSeparator toolStripSeparatorAlign;

    public ToolStripButton toolStripButtonspace;

    public ToolStripButton toolStripButtonR_arow;

    public ToolStripButton toolStripButtonSend;

    public int dataUpdate;

    public bool toolspace;

    public string txt_flag;

    public ToolStripButton toolStripButtonTrans;

    public bool toolFull;

    public ToolStripDropDownButton languagle;

    public ToolStripMenuItem zh_jp;

    public ToolStripMenuItem zh_ko;

    public ToolStripMenuItem zh_en;

    public ToolStripDropDownButton mode;

    private ToolStripMenuItem mode_顶置;

    private ToolStripMenuItem mode_正常;

    private ToolStripMenuItem mode_合并;

    public ToolStripDropDownButton Fontstyle;

    private ToolStripMenuItem font_宋体;

    private ToolStripMenuItem font_楷体;

    private ToolStripMenuItem font_黑体;

    private ToolStripMenuItem font_微软雅黑;

    private ToolStripMenuItem font_新罗马;

    public ColorPicker toolStripButtonColor;

    public RichTextBoxEx EditBox;

    public ToolStripEx toolStripToolBar;

    private ToolStripButton toolStripButtonNote;

    private cmd c;

    public bool splitcolor;

    public bool mergecolor;

    public ToolStripButton topmost;

    public bool topmost_flag;

    public bool checkcolor;

    public bool transcolor;

    public bool Paragraphcolor;

    public bool Fencecolor;

    public class cmd
    {
        public cmd(int _undoCount)
        {
            undoCount = _undoCount + 1;
            undoList.Add("");
        }

        public void execute(string command)
        {
            Record = command;
            if (!und)
            {
                undoList.Add(command);
                if (undoCount != -1 && undoList.Count > undoCount)
                {
                    undoList.RemoveAt(0);
                    return;
                }
            }
            else
            {
                und = false;
            }
        }

        public void undo( )
        {
            if (undoList.Count > 1)
            {
                und = true;
                redoList.Add(undoList[undoList.Count - 1]);
                undoList.RemoveAt(undoList.Count - 1);
                Record = undoList[undoList.Count - 1];
            }
        }

        public void redo( )
        {
            if (redoList.Count > 0)
            {
                Record = redoList[redoList.Count - 1];
                redoList.RemoveAt(redoList.Count - 1);
            }
        }

        public string Record { get; private set; }

        private List<string> undoList = new( );

        private List<string> redoList = new( );

        private int undoCount = -1;

        private bool und;
    }
}
