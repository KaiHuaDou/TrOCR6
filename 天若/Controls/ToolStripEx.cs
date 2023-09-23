using System.Windows.Forms;

namespace TrOCR;

public class ToolStripEx : ToolStrip
{
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 33 && CanFocus && !Focused)
        {
            Focus( );
        }
        base.WndProc(ref m);
    }
}
