using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TrOCR;

public class MenuItemRenderer : ToolStripProfessionalRenderer, IDisposable
{
    public MenuItemRenderer( )
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
        Rectangle rectangle = new(2, 0, (int) e.Graphics.VisibleClipBounds.Width - 36, (int) e.Graphics.VisibleClipBounds.Height - 1);
        using Pen pen = new(menuItemBorderColor);
        e.Graphics.DrawRectangle(pen, rectangle);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        ToolStrip toolStrip = e.ToolStrip;
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        if (e.ToolStrip is MenuStrip or ToolStripDropDown)
        {
            e.Graphics.DrawRectangle(new Pen(Color.LightSlateGray), new Rectangle(0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
            return;
        }
        if (toolStrip != null)
        {
            e.Graphics.DrawRectangle(new Pen(Color.White), new Rectangle(0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
        }
        base.OnRenderToolStripBorder(e);
    }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        ToolStripItem item = e.Item;
        if (item.Selected)
        {
            Pen pen = new(Color.LightSlateGray);
            Point[] array = new Point[]
            {
                        new Point(0, 0),
                        new Point(item.Size.Width - 1, 0),
                        new Point(item.Size.Width - 1, item.Size.Height - 3),
                        new Point(0, item.Size.Height - 3),
                        new Point(0, 0)
            };
            graphics.DrawLines(pen, array);
            return;
        }
        base.OnRenderMenuItemBackground(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        Graphics graphics = e.Graphics;
        if (e.ToolStrip is ToolStripDropDown)
        {
            LinearGradientBrush linearGradientBrush = new(new Point(0, 0), new Point(e.Item.Width, 0), Color.LightGray, Color.FromArgb(0, Color.LightGray));
            graphics.FillRectangle(linearGradientBrush, new Rectangle(33, e.Item.Height / 2, e.Item.Width / 4 * 3, 1));
        }
    }

    protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
    {
        ToolStripItem item = e.Item;
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        if (item.Selected)
        {
            Pen pen = new(Color.LightSlateGray);
            Point[] array = new Point[]
            {
                new Point(0, 0),
                new Point(item.Size.Width - 1, 0),
                new Point(item.Size.Width - 1, item.Size.Height - 3),
                new Point(0, item.Size.Height - 3),
                new Point(0, 0)
            };
            graphics.DrawLines(pen, array);
            return;
        }
        base.OnRenderMenuItemBackground(e);
    }

    public Font textFont;

    public Color menuItemSelectedColor;

    public Color menuItemBorderColor;

    public void Dispose( )
    {
        throw new NotImplementedException( );
    }
}
