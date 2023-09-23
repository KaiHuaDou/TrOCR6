using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TrOCR.Controls;

public class HWColorPicker : FloatLayerBase, IDisposable
{
    public Color SelectedColor { get; private set; }

    [DefaultValue(true)]
    [Description("是否显示颜色提示")]
    public bool ShowTip { get; set; }

    [DefaultValue(typeof(Color), "255, 238, 194")]
    [Description("高亮背景色")]
    public Color HoverBKColor
    {
        get => hoverBKColor;
        set
        {
            if (hoverBKColor != value)
            {
                hoverBrush?.Dispose( );
                hoverBrush = new SolidBrush(value);
            }
            hoverBKColor = value;
        }
    }

    public List<ColorItemX> ColorTable { get; private set; }

    public HWColorPicker( )
    {
        Font = new Font(Font.Name, 9f / StaticValue.Dpifactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        hoverItem = -1;
        InitializeComponent( );
        InitColor( );
        CalcWindowSize( );
        stringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        HoverBKColor = Color.FromArgb(255, 238, 194);
        ShowTip = true;
    }

    public void CalcWindowSize( )
    {
        int num = 152;
        int num2 = 100;
        Size = new Size(num, num2);
    }

    public static Rectangle CalcItemBound(int index)
    {
        if (index is < 0 or > 40)
            throw new ArgumentOutOfRangeException(nameof(index));
        Rectangle rectangle = index == 40 ? Rectangle.FromLTRB(0, 0, 0, 0) : new Rectangle(index % 8 * 18 + 3, index / 8 * 18 + 3, 18, 18);
        return rectangle;
    }

    public int GetIndexFromPoint(Point point)
    {
        int num;
        if (point.X <= 3 || point.X >= Width - 3 || point.Y <= 3 || point.Y >= Height - 3)
        {
            num = -1;
        }
        else
        {
            int num2 = (point.Y - 3) / 18;
            if (num2 > 4)
            {
                num = 40;
            }
            else
            {
                int num3 = (point.X - 3) / 18;
                num = num2 * 8 + num3;
            }
        }
        return num;
    }

    public void SelectColor(Color color)
    {
        SelectedColor = color;
        SelectedColorChanged?.Invoke(ctl_T, EventArgs.Empty);
        Hide( );
    }

    public void DrawItem(DrawItemEventArgs e)
    {
        Rectangle bounds = e.Bounds;
        bounds.Inflate(-1, -1);
        e.DrawBackground( );
        if ((e.State & DrawItemState.HotLight) > DrawItemState.None)
        {
            e.Graphics.FillRectangle(hoverBrush, bounds);
            e.Graphics.DrawRectangle(Pens.Black, bounds);
        }
        if (e.Index != 40)
        {
            Rectangle bounds2 = e.Bounds;
            bounds2.Inflate(-3, -3);
            using (Brush brush = new SolidBrush(ColorTable[e.Index].ItemColor))
            {
                e.Graphics.FillRectangle(brush, bounds2);
            }
            e.Graphics.DrawRectangle(Pens.Gray, bounds2);
        }
    }

    private void HWColorPickerPaint(object o, PaintEventArgs e)
    {
        e.Graphics.DrawRectangle(Pens.LightGray, 0, 0, 150, 98);
        for (int i = 0; i < 40; i++)
        {
            DrawItemEventArgs drawItemEventArgs = new(e.Graphics, Font, CalcItemBound(i), i, DrawItemState.Default, ForeColor, BackColor);
            DrawItem(drawItemEventArgs);
        }
    }

    private void HWColorPickerMouseMove(object o, MouseEventArgs e)
    {
        int indexFromPoint = GetIndexFromPoint(e.Location);
        if (indexFromPoint != hoverItem)
        {
            Graphics graphics = CreateGraphics( );
            if (hoverItem != -1)
                DrawItem(new DrawItemEventArgs(graphics, Font, CalcItemBound(hoverItem), hoverItem, DrawItemState.Default));
            if (indexFromPoint <= 40)
            {
                if (indexFromPoint != -1)
                {
                    if (ShowTip)
                        ShowToolTip(indexFromPoint);
                    DrawItem(new DrawItemEventArgs(graphics, Font, CalcItemBound(indexFromPoint), indexFromPoint, DrawItemState.Default | DrawItemState.HotLight));
                }
                graphics.Dispose( );
                hoverItem = indexFromPoint;
            }
        }
    }

    private void HWColorPickerMouseClick(object o, MouseEventArgs e)
    {
        int indexFromPoint = GetIndexFromPoint(e.Location);
        if (indexFromPoint is not (-1) and not 40)
        {
            SelectedColored = ColorTable[indexFromPoint].ItemColor;
            DialogResult = DialogResult.OK;
        }
    }

    public void ShowToolTip(int index)
    {
        if (index != 40)
            tip.SetToolTip(this, ColorTable[index].Name);
    }

    public void InitColor( )
    {
        ColorTable = new List<ColorItemX>( );
        ColorTable.AddRange(new ColorItemX[]
        {
                    new ColorItemX("黑色", Color.FromArgb(0, 0, 0)),
                    new ColorItemX("褐色", Color.FromArgb(153, 51, 0)),
                    new ColorItemX("橄榄色", Color.FromArgb(51, 51, 0)),
                    new ColorItemX("深绿", Color.FromArgb(0, 51, 0)),
                    new ColorItemX("深青", Color.FromArgb(0, 51, 102)),
                    new ColorItemX("深蓝", Color.FromArgb(0, 0, 128)),
                    new ColorItemX("靛蓝", Color.FromArgb(51, 51, 153)),
                    new ColorItemX("灰色-80%", Color.FromArgb(51, 51, 51)),
                    new ColorItemX("深红", Color.FromArgb(128, 0, 0)),
                    new ColorItemX("橙色", Color.FromArgb(255, 102, 0)),
                    new ColorItemX("深黄", Color.FromArgb(128, 128, 0)),
                    new ColorItemX("绿色", Color.FromArgb(0, 128, 0)),
                    new ColorItemX("青色", Color.FromArgb(0, 128, 128)),
                    new ColorItemX("蓝色", Color.FromArgb(0, 0, 255)),
                    new ColorItemX("蓝灰", Color.FromArgb(102, 102, 153)),
                    new ColorItemX("灰色-50%", Color.FromArgb(128, 128, 128)),
                    new ColorItemX("红色", Color.FromArgb(255, 0, 0)),
                    new ColorItemX("浅橙", Color.FromArgb(255, 153, 0)),
                    new ColorItemX("酸橙", Color.FromArgb(153, 204, 0)),
                    new ColorItemX("海绿", Color.FromArgb(51, 153, 102)),
                    new ColorItemX("水绿", Color.FromArgb(51, 204, 204)),
                    new ColorItemX("浅蓝", Color.FromArgb(51, 102, 255)),
                    new ColorItemX("紫罗兰", Color.FromArgb(128, 0, 128)),
                    new ColorItemX("灰色-40%", Color.FromArgb(153, 153, 153)),
                    new ColorItemX("粉红", Color.FromArgb(255, 0, 255)),
                    new ColorItemX("金色", Color.FromArgb(255, 204, 0)),
                    new ColorItemX("黄色", Color.FromArgb(255, 255, 0)),
                    new ColorItemX("鲜绿", Color.FromArgb(0, 255, 0)),
                    new ColorItemX("青绿", Color.FromArgb(0, 255, 255)),
                    new ColorItemX("天蓝", Color.FromArgb(0, 204, 255)),
                    new ColorItemX("梅红", Color.FromArgb(153, 51, 102)),
                    new ColorItemX("灰色-25%", Color.FromArgb(192, 192, 192)),
                    new ColorItemX("玫瑰红", Color.FromArgb(255, 153, 204)),
                    new ColorItemX("茶色", Color.FromArgb(255, 204, 153)),
                    new ColorItemX("浅黄", Color.FromArgb(255, 255, 204)),
                    new ColorItemX("浅绿", Color.FromArgb(204, 255, 204)),
                    new ColorItemX("浅青绿", Color.FromArgb(204, 255, 255)),
                    new ColorItemX("淡蓝", Color.FromArgb(153, 204, 255)),
                    new ColorItemX("淡紫", Color.FromArgb(204, 153, 255)),
                    new ColorItemX("白色", Color.FromArgb(255, 255, 255))
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose( );
        base.Dispose(disposing);
    }

    public void InitializeComponent( )
    {
        components = new Container( );
        tip = new ToolTip(components);
        SuspendLayout( );
        BackColor = Color.White;
        AutoScaleMode = AutoScaleMode.None;
        Name = "HWColorPicker";
        Paint += HWColorPickerPaint;
        MouseClick += HWColorPickerMouseClick;
        MouseMove += HWColorPickerMouseMove;
        ClientSize = new Size(114, 115);
        SizeGripStyle = SizeGripStyle.Hide;
        ResumeLayout(false);
    }

    public Color SelectedColored { get; private set; }

    [CompilerGenerated]
    private readonly EventHandler SelectedColorChanged;
    private Color hoverBKColor;
    private int hoverItem;
    private Brush hoverBrush;
    private StringFormat stringFormat;
    private IContainer components;
    private ToolTip tip;
    private ToolStripButton ctl_T;
}
