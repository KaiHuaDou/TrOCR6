using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TrOCR;

public class RichTextBoxEx : HelpRepaint.AdvRichTextBox
{
    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose( );
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent( ) => components = new Container( );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string path);

    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue(false)]
    [RefreshProperties(RefreshProperties.All)]
    [SettingsBindable(true)]
    public string Rtf2
    {
        get => base.Rtf;
        set => base.Rtf = value;
    }

    private IContainer components;

    private static readonly IntPtr moduleHandle;
}
