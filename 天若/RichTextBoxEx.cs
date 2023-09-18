using System.ComponentModel;

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

    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue(false)]
    [RefreshProperties(RefreshProperties.All)]
    [SettingsBindable(true)]
    public string Rtf2
    {
        get => Rtf;
        set => Rtf = value;
    }

    private IContainer components;
}
