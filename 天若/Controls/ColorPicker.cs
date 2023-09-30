using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR.Controls;

public class ColorPicker : ToolStripButton
{
    private readonly HWColorPicker picker;

    public ColorPicker( ) => picker = new HWColorPicker { BorderType = BorderStyle.FixedSingle };

    public Color SelectedColor { get; set; }

    protected override void OnClick(EventArgs e)
    {
        if (picker.ShowDialog(this) == DialogResult.OK)
        {
            SelectedColor = picker.SelectedColored;
            base.OnClick(e);
        }
    }
}
