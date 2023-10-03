using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
    private bool toolSpace;
    private bool transColor;
    private UndoCommand undoCmd;
    private WindowType windowType;
    public AdvRichTextBox( )
    {
        toolSpace = true;
        undoCmd = new UndoCommand(50);
        Font = new Font(Font.Name, 9f / Defaults.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        InitializeComponent( );
        ReadConfig( );
        EditBox.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
        SetFont(EditorFont.微软雅黑);
    }

    public new string Hide
    {
        set
        {
            EditBox.Focus( );
            mode.HideDropDown( );
            ButtonFont.HideDropDown( );
            ButtonLang.HideDropDown( );
        }
    }

    public ContextMenuStrip InnerContextMenuStrip
    {
        get => EditBox.ContextMenuStrip;
        set => EditBox.ContextMenuStrip = value;
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
        set => EditBox.Text = value;
    }

    public void CheckTyping( )
    {
        new Thread(typoCheckingApi).Start( );
        SetForegroundWindow(Defaults.MainHandle);
    }

    public void SetToolBar(WindowType type)
    {
        windowType = type;
        if (windowType == WindowType.Main)
        {
            toolStripToolBar.Items.AddRange(new ToolStripItem[]
            {
                ButtonTopmost, ButtonFont, ButtonBold, ButtonColor, ButtonLeft, ButtonFull, ButtonSpace, ButtonVoice, ButtonFind, ButtonSend,
                ButtonNote, ButtonParagraph, ButtonFence, ButtonSplit, ButtonMerge, ButtonCheck, ButtonTrans
            });
            return;
        }
        toolStripToolBar.Items.AddRange(new ToolStripItem[]
        {
            ButtonLang, ButtonFont, ButtonBold, ButtonColor, ButtonLeft, ButtonFull, ButtonSpace, ButtonVoice, ButtonFind, ButtonSend,
            ButtonClose
        });
    }

    public void UpdateTransType( )
    {
        ZhEn.ForeColor = Defaults.TransType == TranslateType.ZhEn ? Color.Red : Color.Black;
        ZhJp.ForeColor = Defaults.TransType == TranslateType.ZhJp ? Color.Red : Color.Black;
        ZhKo.ForeColor = Defaults.TransType == TranslateType.ZhKo ? Color.Red : Color.Black;
        SendMessage(Defaults.MainHandle, 786, 512);
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
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonCheckClick(object o, EventArgs e)
    {
        SetForegroundWindow(Defaults.MainHandle);
        new Thread(new ThreadStart(typoCheckingApi)).Start( );
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonCheckKeyDown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!checkColor)
            {
                ButtonCheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck2.Image");
                checkColor = true;
                Config.Set("工具栏", "检查", "True");
                return;
            }
            if (checkColor)
            {
                ButtonCheck.Image = (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
                checkColor = false;
                Config.Set("工具栏", "检查", "False");
            }
        }
    }

    private void ButtonCloseClick(object o, EventArgs e)
    {
        SetForegroundWindow(Defaults.MainHandle);
        SendMessage(GetForegroundWindow( ), 786, 511);
    }

    private void ButtonColorClick(object o, EventArgs e)
    {
        EditBox.SelectionColor = ButtonColor.SelectedColor;
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonFenceClick(object o, EventArgs e)
    {
        if (!File.Exists("cvextern.dll"))
        {
            MessageBox.Show("请从蓝奏网盘中下载cvextern.dll大小约25m，点击确定自动弹出网页。\r\n将下载后的文件与 天若.exe 这个文件放在一起。");
            Process.Start("https://www.lanzous.com/i1ab3vg");
            return;
        }
        SetForegroundWindow(Defaults.MainHandle);
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
                ButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence2.Image");
                fenceColor = true;
                Config.Set("工具栏", "分栏", "True");
                return;
            }
            else if (fenceColor)
            {
                ButtonFence.Image = (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
                fenceColor = false;
                Config.Set("工具栏", "分栏", "False");
            }
        }
    }

    private void ButtonFindClick(object o, EventArgs e)
    {
        SetForegroundWindow(Defaults.MainHandle);
        ReplaceForm replaceForm = new(this)
        {
            Text = windowType == WindowType.Main ? "识别替换" : "翻译替换"
        };
        replaceForm.Location = PointToScreen(new Point((Width - replaceForm.Width) / 2, (Height - replaceForm.Height) / 2));
        replaceForm.Show(this);
    }

    private void ButtonFullClick(object o, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Justify;
        EditBox.Select(0, 0);
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonLeftClick(object o, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Left;
        EditBox.Select(0, 0);
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonMergeClick(object o, EventArgs e)
    {
        EditBox.Text = TextUtils.MergeLines(EditBox.Text);
        Application.DoEvents( );
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonMergeKeyDown(object o, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;
        SwitchSpiltMerge(true);
    }

    private void ButtonNoteClick(object o, EventArgs e)
    {
        SetForegroundWindow(Defaults.MainHandle);
        SendMessage(Defaults.MainHandle, 786, 520);
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void ButtonSplitKeyDown(object o, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;
        SwitchSpiltMerge(false);
    }

    private void Form1_DragDrop(object o, DragEventArgs e)
    {
        try
        {
            Defaults.ImageOCR = Image.FromFile((e.Data.GetData(DataFormats.FileDrop, false) as string[])[0]);
            SendMessage(Defaults.MainHandle, 786, 580);
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

    private void IndentTwo(int flag)
    {
        Font font = new(Font.Name, 9f * Helper.System.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        Graphics graphics = CreateGraphics( );
        SizeF sizeF = graphics.MeasureString("中", font);
        EditBox.SelectionIndent = (int) sizeF.Width * 2 * flag;
        EditBox.SelectionHangingIndent = -(int) sizeF.Width * 2 * flag;
        graphics.Dispose( );
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
            ButtonTopmost.Image = (Image) componentResourceManager.GetObject("main.Image");
            Defaults.Topmost = true;
        }
        else
        {
            ButtonTopmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
            Defaults.Topmost = false;
        }
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
        mergeColor = bool.Parse(Config.Get("工具栏", "合并"));
        splitColor = bool.Parse(Config.Get("工具栏", "拆分"));
        checkColor = bool.Parse(Config.Get("工具栏", "检查"));
        transColor = bool.Parse(Config.Get("工具栏", "翻译"));
        paragraphColor = bool.Parse(Config.Get("工具栏", "分段"));
        fenceColor = bool.Parse(Config.Get("工具栏", "分栏"));
        ButtonFence.Image = fenceColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonFence2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonFence.Image");
        ButtonParagraph.Image = paragraphColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonParagraph2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
        ButtonCheck.Image = checkColor
            ? (Image) componentResourceManager.GetObject("toolStripButtoncheck2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtoncheck.Image");
        ButtonMerge.Image = mergeColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonMerge_2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
        ButtonSplit.Image = splitColor
            ? (Image) componentResourceManager.GetObject("toolStripButtonSplit_2.Image")
            : (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
        if (transColor)
        {
            ButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans2.Image");
            return;
        }
        ButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
    }

    private void RichBoxKeyDown(object o, KeyEventArgs e)
    {
        SetForegroundWindow(Defaults.MainHandle);
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
            ButtonFindClick(o, e);
        }
    }

    private void RichBoxMouseDown(object o, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void RichBoxTextChanged(object o, EventArgs e)
        => undoCmd.Execute(EditBox.Text);
    private void RichBoxTextClicked(object o, LinkClickedEventArgs e)
        => Process.Start(e.LinkText);

    private void SetFont(EditorFont font)
    {
        font宋体.ForeColor = font == EditorFont.宋体 ? Color.Red : Color.Black;
        font黑体.ForeColor = font == EditorFont.黑体 ? Color.Red : Color.Black;
        font楷体.ForeColor = font == EditorFont.楷体 ? Color.Red : Color.Black;
        font微软雅黑.ForeColor = font == EditorFont.微软雅黑 ? Color.Red : Color.Black;
        font新罗马.ForeColor = font == EditorFont.新罗马 ? Color.Red : Color.Black;
        string text = EditBox.Text;
        EditBox.Text = "";
        EditBox.Font = new(font.ToString( ), 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.Text = text;
    }

    private void SetFont黑体(object o, EventArgs e)
        => SetFont(EditorFont.黑体);

    private void SetFont楷体(object o, EventArgs e)
        => SetFont(EditorFont.楷体);

    private void SetFont宋体(object o, EventArgs e)
        => SetFont(EditorFont.宋体);

    private void SetFont微软雅黑(object o, EventArgs e)
        => SetFont(EditorFont.微软雅黑);

    private void SetFont新罗马(object o, EventArgs e)
        => SetFont(EditorFont.新罗马);
    private void SwitchSpiltMerge(bool isMerge)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (isMerge ? !mergeColor : !splitColor)
        {
            ButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit_2.Image");
            ButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
            splitColor = true;
            mergeColor = false;
            Defaults.SetSpilt = true;
            Defaults.SetMerge = false;
            Config.Set("工具栏", "拆分", "True");
            Config.Set("工具栏", "合并", "False");
            return;
        }
        else
        {
            ButtonMerge.Image = (Image) componentResourceManager.GetObject("toolStripButtonMerge.Image");
            ButtonSplit.Image = (Image) componentResourceManager.GetObject("toolStripButtonSplit.Image");
            splitColor = false;
            mergeColor = false;
            Defaults.SetSpilt = false;
            Defaults.SetMerge = false;
            Config.Set("工具栏", "合并", "False");
            Config.Set("工具栏", "拆分", "False");
        }
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
                ButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph2.Image");
                paragraphColor = true;
                Config.Set("工具栏", "分段", "True");
                return;
            }
            else
            {
                ButtonParagraph.Image = (Image) componentResourceManager.GetObject("toolStripButtonParagraph.Image");
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
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void toolStripButtonSplit_Click(object o, EventArgs e)
    {
        EditBox.Text = Defaults.Split;
        Application.DoEvents( );
        SetForegroundWindow(Defaults.MainHandle);
    }
    private void toolStripButtonTrans_Click(object o, EventArgs e)
    {
        SendMessage(Defaults.MainHandle, 786, 512);
        SetForegroundWindow(Defaults.MainHandle);
    }

    private void toolStripButtontrans_keydown(object o, MouseEventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(AdvRichTextBox));
        if (e.Button == MouseButtons.Right)
        {
            if (!transColor)
            {
                ButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans2.Image");
                transColor = true;
                Config.Set("工具栏", "翻译", "True");
                return;
            }
            if (transColor)
            {
                ButtonTrans.Image = (Image) componentResourceManager.GetObject("toolStripButtonTrans.Image");
                transColor = false;
                Config.Set("工具栏", "翻译", "False");
            }
        }
    }

    private void toolStripButtonVoice_Click(object o, EventArgs e)
    {
        SetForegroundWindow(Defaults.MainHandle);
        SendMessage(Defaults.MainHandle, 786, 518);
        SetForegroundWindow(Defaults.MainHandle);
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
                ButtonTopmost.Image = (Image) componentResourceManager.GetObject("main.Image");
                Defaults.Topmost = true;
                isTopmost = true;
                Config.Set("工具栏", "顶置", "True");
                SendMessage(Defaults.MainHandle, 600, 725);
                return;
            }
            ButtonTopmost.Image = (Image) componentResourceManager.GetObject("mode.Image");
            Defaults.Topmost = false;
            isTopmost = false;
            Config.Set("工具栏", "顶置", "False");
            SendMessage(Defaults.MainHandle, 600, 725);
        }
    }

    private void typoCheckingApi( )
    {
        EditBox.SelectAll( );
        EditBox.SelectionColor = Color.Black;
        EditBox.Select(0, 0);
        try
        {
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(Web.PostHtml("http://www.cuobiezi.net/api/v1/zh_spellcheck/client/pos/json", "{\"check_mode\": \"value2\",\"content\": \"" + EditBox.Text + "\", \"content2\": \"value1\",  \"doc_type\": \"value2\",\"method\": \"value2\",\"return_format\": \"value2\",\"username\": \"tianruoyouxin\"}")))["Cases"].ToString( ));
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
    private void ZhEnClick(object o, EventArgs e)
    {
        Defaults.TransType = TranslateType.ZhEn;
        UpdateTransType( );
    }

    private void ZhJpClick(object o, EventArgs e)
    {
        Defaults.TransType = TranslateType.ZhJp;
        UpdateTransType( );
    }

    private void ZhKoClick(object o, EventArgs e)
    {
        Defaults.TransType = TranslateType.ZhKo;
        UpdateTransType( );
    }
}
