using System;
using System.Drawing;
using System.Windows.Forms;
using TrOCR.Ocr;

namespace TrOCR;

public class ColorPicker : ToolStripButton
{
    public ColorPicker( ) => cp = new HWColorPicker
    {
        BorderType = BorderStyle.FixedSingle
    };

    protected override void OnClick(EventArgs e)
    {
        if (cp.ShowDialog(this) == DialogResult.OK)
        {
            select_color = cp.SelectedColored;
            base.OnClick(e);
        }
    }

    public Color SelectedColor => select_color;

    private readonly HWColorPicker cp;

    public Color select_color;
}
