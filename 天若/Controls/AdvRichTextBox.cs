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
    private bool isChecked;
    private bool isFence;
    private bool isIndent;
    private bool isMerge;
    private bool isParagraph;
    private bool isSplit;
    private bool isTopmost;
    private bool isTranslate;
    private ComponentResourceManager resourceManager = new(typeof(AdvRichTextBox));
    private UndoCommand undoCmd = new(50);
    private WindowType windowType;
    public AdvRichTextBox( )
    {
        InitializeComponent( );
        ButtonFont.Font = new Font("微软雅黑", 9f * Helper.System.DpiFactor, FontStyle.Regular);
        ButtonLang.Font = new Font("微软雅黑", 9f * Helper.System.DpiFactor, FontStyle.Regular);
        EditBox.Font = new Font("Times New Roman", 16f * Helper.System.DpiFactor, GraphicsUnit.Pixel);
        EditBox.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
        mode.Font = new Font("微软雅黑", 9f * Helper.System.DpiFactor, FontStyle.Regular);
        Font = new Font(Font.Name, 9f / Globals.DpiFactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);

        CheckForIllegalCrossThreadCalls = false;
        ReadConfig( );
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
        new Thread(TypoCheckingApi).Start( );
        SetForegroundWindow(Globals.MainHandle);
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
        }
        else
        {
            toolStripToolBar.Items.AddRange(new ToolStripItem[]
            {
                ButtonLang, ButtonFont, ButtonBold, ButtonColor, ButtonLeft, ButtonFull, ButtonSpace, ButtonVoice, ButtonFind, ButtonSend,
                ButtonTransClose
            });
        }
    }

    public void UpdateTransType( )
    {
        ZhEn.ForeColor = Globals.TransType == TranslateType.ZhEn ? Color.Red : Color.Black;
        ZhJp.ForeColor = Globals.TransType == TranslateType.ZhJp ? Color.Red : Color.Black;
        ZhKo.ForeColor = Globals.TransType == TranslateType.ZhKo ? Color.Red : Color.Black;
        SendMessage(Globals.MainHandle, MsgFlag.FmMain, MsgFlag.TransOpen);
    }

    private void ButtomTopmostClick(object o, MouseEventArgs e)
    {
        isTopmost = !isTopmost;
        ButtonTopmost.Image = isTopmost
            ? (Image) resourceManager.GetObject("mode.Image")
            : (Image) resourceManager.GetObject("main.Image");
        Globals.Topmost = isTopmost;
        Config.Set("工具栏", "顶置", isTopmost);
        SendMessage(Globals.MainHandle, MsgFlag.FmMain, MsgFlag.Topmost);
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
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonCheckClick(object o, EventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
        new Thread(TypoCheckingApi).Start( );
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonCheckRightClick(object o, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;
        isChecked = !isChecked;
        ButtonCheck.Image = isChecked
            ? (Image) resourceManager.GetObject("Buttoncheck2.Image")
            : (Image) resourceManager.GetObject("Buttoncheck.Image");
        Config.Set("工具栏", "检查", isChecked);
    }

    private void ButtonTransCloseClick(object o, EventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
        SendMessage(GetForegroundWindow( ), MsgFlag.FmMain, MsgFlag.TransClose);
    }

    private void ButtonColorClick(object o, EventArgs e)
    {
        if (EditBox.SelectionLength != 0)
        {
            EditBox.SelectionColor = ButtonColor.SelectedColor;
        }
        else
        {
            EditBox.SelectAll( );
            EditBox.SelectionColor = ButtonColor.SelectedColor;
            EditBox.Select(0, 0);
        }
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonFenceClick(object o, EventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
        isFence = !isFence;
        ButtonFence.Image = isFence
            ? (Image) resourceManager.GetObject("ButtonFence2.Image")
            : (Image) resourceManager.GetObject("ButtonFence.Image");
        Config.Set("工具栏", "分栏", isFence);
    }

    private void ButtonFindClick(object o, EventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
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
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonLeftClick(object o, EventArgs e)
    {
        EditBox.SelectAll( );
        EditBox.SelectionAlignment = TextAlign.Left;
        EditBox.Select(0, 0);
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonMergeClick(object o, EventArgs e)
    {
        EditBox.Text = TextUtils.MergeLines(EditBox.Text);
        Application.DoEvents( );
        SetForegroundWindow(Globals.MainHandle);
        SwitchSpiltMerge( );
    }

    private void ButtonNoteClick(object o, EventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
        SendMessage(Globals.MainHandle, MsgFlag.FmMain, MsgFlag.ShowNote);
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonParagraphClick(object o, EventArgs e)
    {
        isParagraph = !isParagraph;
        ButtonParagraph.Image = isParagraph
            ? (Image) resourceManager.GetObject("ButtonParagraph2.Image")
            : (Image) resourceManager.GetObject("ButtonParagraph.Image");
        Config.Set("工具栏", "分段", isParagraph);
        return;
    }

    private void ButtonSendClick(object o, EventArgs e)
    {
        Clipboard.SetText(EditBox.Text);
        FmFlags.Display("已复制");
    }

    private void ButtonSpaceClick(object o, EventArgs e)
    {
        EditBox.SelectAll( );
        isIndent = !isIndent;
        Indent(isIndent ? 1 : 0);
        EditBox.Select(0, 0);
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonSplitClick(object o, EventArgs e)
    {
        EditBox.Text = Globals.SplitedText;
        Application.DoEvents( );
        SetForegroundWindow(Globals.MainHandle);
        SwitchSpiltMerge( );
    }

    private void ButtonTransClick(object o, EventArgs e)
    {
        isTranslate = !isTranslate;
        ButtonTrans.Image = isTranslate
            ? (Image) resourceManager.GetObject("ButtonTrans2.Image")
            : (Image) resourceManager.GetObject("ButtonTrans.Image");
        Config.Set("工具栏", "翻译", isTranslate);
        SendMessage(Globals.MainHandle, MsgFlag.FmMain, isTranslate ? MsgFlag.TransOpen : MsgFlag.TransClose);
        SetForegroundWindow(Globals.MainHandle);
    }

    private void ButtonVoiceClick(object o, EventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
        SendMessage(Globals.MainHandle, MsgFlag.FmMain, MsgFlag.Voice);
        SetForegroundWindow(Globals.MainHandle);
    }

    private void FormDragDrop(object o, DragEventArgs e)
    {
        try
        {
            Globals.ImageOCR = Image.FromFile((e.Data.GetData(DataFormats.FileDrop, false) as string[])[0]);
            SendMessage(Globals.MainHandle, MsgFlag.FmMain, MsgFlag.OcrImage);
        }
        catch (Exception)
        {
            MessageBox.Show("文件格式不正确", "天若 OCR 6", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FormDragEnter(object o, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.All;
            return;
        }
        e.Effect = DragDropEffects.None;
    }

    private void Indent(int flag)
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
        Indent(1);
        SetFont(EditorFont.微软雅黑);
        isIndent = true;
        if (Config.Get("工具栏", "顶置") == "_ERROR_")
            Config.Set("工具栏", "顶置", "False");
        if (Config.Get("工具栏", "合并") == "_ERROR_")
            Config.Set("工具栏", "合并", "False");
        if (Config.Get("工具栏", "拆分") == "_ERROR_")
            Config.Set("工具栏", "拆分", "False");
        if (Config.Get("工具栏", "检查") == "_ERROR_")
            Config.Set("工具栏", "检查", "False");
        if (Config.Get("工具栏", "翻译") == "_ERROR_")
            Config.Set("工具栏", "翻译", "False");
        if (Config.Get("工具栏", "分段") == "_ERROR_")
            Config.Set("工具栏", "分段", "False");
        if (Config.Get("工具栏", "分栏") == "_ERROR_")
            Config.Set("工具栏", "分栏", "False");
        isChecked = bool.Parse(Config.Get("工具栏", "检查"));
        isFence = bool.Parse(Config.Get("工具栏", "分栏"));
        isMerge = bool.Parse(Config.Get("工具栏", "合并"));
        isParagraph = bool.Parse(Config.Get("工具栏", "分段"));
        isSplit = bool.Parse(Config.Get("工具栏", "拆分"));
        isTopmost = bool.Parse(Config.Get("工具栏", "顶置"));
        isTranslate = bool.Parse(Config.Get("工具栏", "翻译"));
        Globals.Topmost = isTopmost;
        if (isChecked)
            ButtonCheckClick(null, null);
        ButtonTopmost.Image = isTopmost
            ? (Image) resourceManager.GetObject("mode.Image")
            : (Image) resourceManager.GetObject("main.Image");
        ButtonFence.Image = isFence
            ? (Image) resourceManager.GetObject("ButtonFence2.Image")
            : (Image) resourceManager.GetObject("ButtonFence.Image");
        ButtonParagraph.Image = isParagraph
            ? (Image) resourceManager.GetObject("ButtonParagraph2.Image")
            : (Image) resourceManager.GetObject("ButtonParagraph.Image");
        ButtonCheck.Image = isChecked
            ? (Image) resourceManager.GetObject("Buttoncheck2.Image")
            : (Image) resourceManager.GetObject("Buttoncheck.Image");
        ButtonMerge.Image = isMerge
            ? (Image) resourceManager.GetObject("ButtonMerge2.Image")
            : (Image) resourceManager.GetObject("ButtonMerge.Image");
        ButtonSplit.Image = isSplit
            ? (Image) resourceManager.GetObject("ButtonSplit2.Image")
            : (Image) resourceManager.GetObject("ButtonSplit.Image");
        ButtonTrans.Image = isTranslate
            ? (Image) resourceManager.GetObject("ButtonTrans2.Image")
            : (Image) resourceManager.GetObject("ButtonTrans.Image");
    }

    private void RichBoxKeyDown(object o, KeyEventArgs e)
    {
        SetForegroundWindow(Globals.MainHandle);
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
        SetForegroundWindow(Globals.MainHandle);
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

    private void SwitchSpiltMerge( )
    {
        isMerge = !isMerge;
        isSplit = !isSplit;
        ButtonSplit.Image = !isMerge ? (Image) resourceManager.GetObject("ButtonSplit2.Image") : (Image) resourceManager.GetObject("ButtonSplit.Image");
        ButtonMerge.Image = isMerge ? (Image) resourceManager.GetObject("ButtonMerge2.Image") : (Image) resourceManager.GetObject("ButtonMerge.Image");
        Config.Set("工具栏", "拆分", isSplit);
        Config.Set("工具栏", "合并", isMerge);
    }

    private void TypoCheckingApi( )
    {
        EditBox.SelectAll( );
        EditBox.SelectionColor = Color.Black;
        EditBox.Select(0, 0);
        try
        {
            JArray jarray = JArray.Parse(
                (JsonConvert.DeserializeObject(
                    Web.PostHtml("http://www.cuobiezi.net/api/v1/zh_spellcheck/client/pos/json", $"{{\"check_mode\": \"value2\",\"content\": \"{EditBox.Text}\", \"content2\": \"value1\",  \"doc_type\": \"value2\",\"method\": \"value2\",\"return_format\": \"value2\",\"username\": \"tianruoyouxin\"}}"))
            as JObject)["Cases"].ToString( ));
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
        Globals.TransType = TranslateType.ZhEn;
        UpdateTransType( );
    }

    private void ZhJpClick(object o, EventArgs e)
    {
        Globals.TransType = TranslateType.ZhJp;
        UpdateTransType( );
    }

    private void ZhKoClick(object o, EventArgs e)
    {
        Globals.TransType = TranslateType.ZhKo;
        UpdateTransType( );
    }
}
