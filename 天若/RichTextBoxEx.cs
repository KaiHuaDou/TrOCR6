using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public class RichTextBoxEx : RichTextBox
{
    public RichTextBoxEx( )
        => SetLineSpace( );

    public void BeginUpdate( )
    {
        updating++;
        if (updating <= 1)
        {
            oldEventMask = SendMessage(new HandleRef(this, Handle), 1073, 0, 0);
            SendMessage(new HandleRef(this, Handle), 11, 0, 0);
        }
    }

    public void EndUpdate( )
    {
        updating--;
        if (updating <= 0)
        {
            SendMessage(new HandleRef(this, Handle), 11, 1, 0);
            SendMessage(new HandleRef(this, Handle), 1073, 0, oldEventMask);
        }
    }

    public new TextAlign SelectionAlignment
    {
        get
        {
            PARAFORMAT paraformat = default;
            paraformat.cbSize = Marshal.SizeOf(paraformat);
            SendMessage(new HandleRef(this, Handle), 1085, 1, ref paraformat);
            TextAlign textAlign = (paraformat.dwMask & 8U) == 0U ? TextAlign.Left : (TextAlign) paraformat.wAlignment;
            return textAlign;
        }
        set
        {
            PARAFORMAT paraformat = default;
            paraformat.cbSize = Marshal.SizeOf(paraformat);
            paraformat.dwMask = 8U;
            paraformat.wAlignment = (short) value;
            SendMessage(new HandleRef(this, Handle), 1095, 1, ref paraformat);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!AutoWordSelection)
        {
            AutoWordSelection = true;
            AutoWordSelection = false;
        }
        SendMessage(new HandleRef(this, Handle), 1226, 1, 1);
    }

    public void SetLineSpace( )
    {
        PARAFORMAT paraformat = default;
        paraformat.cbSize = Marshal.SizeOf(paraformat);
        paraformat.bLineSpacingRule = 4;
        paraformat.dyLineSpacing = 400;
        paraformat.dwMask = 256U;
        SendMessage(new HandleRef(this, Handle), 1095, 0, ref paraformat);
    }

    private int updating;

    private int oldEventMask;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose( );
        }
        base.Dispose(disposing);
    }

    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue(false)]
    [RefreshProperties(RefreshProperties.All)]
    [SettingsBindable(true)]
    public string RtfEx
    {
        get => Rtf;
        set => Rtf = value;
    }

    private IContainer components;
}
