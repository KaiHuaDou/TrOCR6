using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR;

public class MenuItemRendererT : ToolStripProfessionalRenderer
{
    public MenuItemRendererT( )
    {
        textFont = new Font("微软雅黑", 9f / StaticValue.Dpifactor, FontStyle.Regular, GraphicsUnit.Point, 0);
        menuItemSelectedColor = Color.White;
        menuItemBorderColor = Color.White;
        menuItemSelectedColor = Color.White;
        menuItemBorderColor = Color.LightSlateGray;
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.IsOnDropDown)
        {
            if (e.Item.Selected && e.Item.Enabled)
            {
                DrawMenuDropDownItemHighlight(e);
                return;
            }
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }

    public void DrawMenuDropDownItemHighlight(ToolStripItemRenderEventArgs e)
    {
        Rectangle rectangle = new(2, 0, (int) e.Graphics.VisibleClipBounds.Width - 4, (int) e.Graphics.VisibleClipBounds.Height - 1);
        using Pen pen = new(menuItemBorderColor);
        e.Graphics.DrawRectangle(pen, rectangle);
    }

    public Font textFont;

    public Color menuItemSelectedColor;

    public Color menuItemBorderColor;
}
