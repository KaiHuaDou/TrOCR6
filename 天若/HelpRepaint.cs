using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public static class HelpRepaint
{
    public class ColorItemx
    {
        public ColorItemx(string name, Color clr)
        {
            Name = name;
            ItemColor = clr;
        }

        public Color ItemColor
        {
            get => color;
            set => color = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string name;

        public Color color;
    }

    public class HWColorPicker : FloatLayerBase
    {
        public Color SelectedColor => selectedColor;

        [DefaultValue(true)]
        [Description("是否显示颜色提示")]
        public bool ShowTip
        {
            get => showTip;
            set => showTip = value;
        }

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

        public List<ColorItemx> ColorTable => colorTable;

        public HWColorPicker( )
        {
            Font = new Font(Font.Name, 9f / StaticValue.Dpifactor, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
            hoverItem = -1;
            InitializeComponent( );
            InitColor( );
            CalcWindowSize( );
            sf = new StringFormat
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
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
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

        public void SelectColor(Color clr)
        {
            selectedColor = clr;
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

        public void HWColorPickerPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.LightGray, 0, 0, 150, 98);
            for (int i = 0; i < 40; i++)
            {
                DrawItemEventArgs drawItemEventArgs = new(e.Graphics, Font, CalcItemBound(i), i, DrawItemState.Default, ForeColor, BackColor);
                DrawItem(drawItemEventArgs);
            }
        }

        public void HWColorPickerMouseMove(object sender, MouseEventArgs e)
        {
            int indexFromPoint = GetIndexFromPoint(e.Location);
            if (indexFromPoint != hoverItem)
            {
                Graphics graphics = CreateGraphics( );
                if (hoverItem != -1)
                {
                    DrawItem(new DrawItemEventArgs(graphics, Font, CalcItemBound(hoverItem), hoverItem, DrawItemState.Default));
                }
                if (indexFromPoint <= 40)
                {
                    if (indexFromPoint != -1)
                    {
                        if (ShowTip)
                        {
                            ShowToolTip(indexFromPoint);
                        }
                        DrawItem(new DrawItemEventArgs(graphics, Font, CalcItemBound(indexFromPoint), indexFromPoint, DrawItemState.Default | DrawItemState.HotLight));
                    }
                    graphics.Dispose( );
                    hoverItem = indexFromPoint;
                }
            }
        }

        public void HWColorPickerMouseClick(object sender, MouseEventArgs e)
        {
            int indexFromPoint = GetIndexFromPoint(e.Location);
            if (indexFromPoint is not (-1) and not 40)
            {
                SelectedColored = colorTable[indexFromPoint].ItemColor;
                DialogResult = DialogResult.OK;
            }
        }

        public void ShowToolTip(int index)
        {
            if (index != 40)
            {
                tip.SetToolTip(this, ColorTable[index].Name);
            }
        }

        public void InitColor( )
        {
            colorTable = new List<ColorItemx>( );
            colorTable.AddRange(new ColorItemx[]
            {
                    new ColorItemx("黑色", Color.FromArgb(0, 0, 0)),
                    new ColorItemx("褐色", Color.FromArgb(153, 51, 0)),
                    new ColorItemx("橄榄色", Color.FromArgb(51, 51, 0)),
                    new ColorItemx("深绿", Color.FromArgb(0, 51, 0)),
                    new ColorItemx("深青", Color.FromArgb(0, 51, 102)),
                    new ColorItemx("深蓝", Color.FromArgb(0, 0, 128)),
                    new ColorItemx("靛蓝", Color.FromArgb(51, 51, 153)),
                    new ColorItemx("灰色-80%", Color.FromArgb(51, 51, 51)),
                    new ColorItemx("深红", Color.FromArgb(128, 0, 0)),
                    new ColorItemx("橙色", Color.FromArgb(255, 102, 0)),
                    new ColorItemx("深黄", Color.FromArgb(128, 128, 0)),
                    new ColorItemx("绿色", Color.FromArgb(0, 128, 0)),
                    new ColorItemx("青色", Color.FromArgb(0, 128, 128)),
                    new ColorItemx("蓝色", Color.FromArgb(0, 0, 255)),
                    new ColorItemx("蓝灰", Color.FromArgb(102, 102, 153)),
                    new ColorItemx("灰色-50%", Color.FromArgb(128, 128, 128)),
                    new ColorItemx("红色", Color.FromArgb(255, 0, 0)),
                    new ColorItemx("浅橙", Color.FromArgb(255, 153, 0)),
                    new ColorItemx("酸橙", Color.FromArgb(153, 204, 0)),
                    new ColorItemx("海绿", Color.FromArgb(51, 153, 102)),
                    new ColorItemx("水绿", Color.FromArgb(51, 204, 204)),
                    new ColorItemx("浅蓝", Color.FromArgb(51, 102, 255)),
                    new ColorItemx("紫罗兰", Color.FromArgb(128, 0, 128)),
                    new ColorItemx("灰色-40%", Color.FromArgb(153, 153, 153)),
                    new ColorItemx("粉红", Color.FromArgb(255, 0, 255)),
                    new ColorItemx("金色", Color.FromArgb(255, 204, 0)),
                    new ColorItemx("黄色", Color.FromArgb(255, 255, 0)),
                    new ColorItemx("鲜绿", Color.FromArgb(0, 255, 0)),
                    new ColorItemx("青绿", Color.FromArgb(0, 255, 255)),
                    new ColorItemx("天蓝", Color.FromArgb(0, 204, 255)),
                    new ColorItemx("梅红", Color.FromArgb(153, 51, 102)),
                    new ColorItemx("灰色-25%", Color.FromArgb(192, 192, 192)),
                    new ColorItemx("玫瑰红", Color.FromArgb(255, 153, 204)),
                    new ColorItemx("茶色", Color.FromArgb(255, 204, 153)),
                    new ColorItemx("浅黄", Color.FromArgb(255, 255, 204)),
                    new ColorItemx("浅绿", Color.FromArgb(204, 255, 204)),
                    new ColorItemx("浅青绿", Color.FromArgb(204, 255, 255)),
                    new ColorItemx("淡蓝", Color.FromArgb(153, 204, 255)),
                    new ColorItemx("淡紫", Color.FromArgb(204, 153, 255)),
                    new ColorItemx("白色", Color.FromArgb(255, 255, 255))
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose( );
            }
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

        public bool showTip;

        public Color selectedColor;

        public Color hoverBKColor;

        public List<ColorItemx> colorTable;

        public StringFormat sf;

        public int hoverItem;

        public Control ctl;

        public Brush hoverBrush;

        public IContainer components;

        public ToolTip tip;

        public ToolStripButton ctl_T;
    }

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
            _ = e.Item;
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

        public void Dispose( ) => throw new NotImplementedException( );
    }

    public class MenuItemRendererT : ToolStripProfessionalRenderer, IDisposable
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

        public void Dispose( ) => throw new NotImplementedException( );
    }

    public class myWebBroswer : WebBrowser
    {
        public override bool PreProcessMessage(ref Message msg)
        {
            if (msg.Msg == 256 && (int) msg.WParam == 86 && ModifierKeys == Keys.Control)
            {
                Clipboard.SetDataObject((string) Clipboard.GetDataObject( ).GetData(DataFormats.UnicodeText));
                keybd_event(Keys.ControlKey, 0, 0U, 0U);
                keybd_event(Keys.D9, 0, 0U, 0U);
                keybd_event(Keys.D9, 0, 2U, 0U);
                keybd_event(Keys.ControlKey, 0, 2U, 0U);
            }
            if (msg.Msg == 256 && (int) msg.WParam == 67 && ModifierKeys == Keys.Control)
            {
                keybd_event(Keys.ControlKey, 0, 0U, 0U);
                keybd_event(Keys.D8, 0, 0U, 0U);
                keybd_event(Keys.D8, 0, 2U, 0U);
                keybd_event(Keys.ControlKey, 0, 2U, 0U);
            }
            return base.PreProcessMessage(ref msg);
        }
    }

    [DefaultEvent("SelectedColorChanged")]
    [DefaultProperty("Color")]
    [Description("ToolStripItem that allows selecting a color from a color picker control.")]
    [ToolboxItem(false)]
    [ToolboxBitmap(typeof(ToolStripColorPicker), "ToolStripColorPicker")]
    public class ToolStripColorPicker : ToolStripButton
    {
        public Point GetOpenPoint( )
        {
            Point point;
            if (Owner == null)
            {
                point = new Point(5, 5);
            }
            else
            {
                int num = 0;
                foreach (object obj in Parent.Items)
                {
                    ToolStripItem toolStripItem = (ToolStripItem) obj;
                    if (toolStripItem == this)
                    {
                        break;
                    }
                    num += toolStripItem.Width;
                }
                point = new Point(num, -4);
            }
            return point;
        }

        public Point GetPoint
        {
            get => GetOpenPoint( );
            set
            {
            }
        }
    }

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

    public class FloatLayerBase : Form
    {
        [DefaultValue(BorderStyle.Fixed3D)]
        [Description("获取或设置边框类型。")]
        public BorderStyle BorderType
        {
            get => _borderType;
            set
            {
                if (_borderType != value)
                {
                    _borderType = value;
                    Invalidate( );
                }
            }
        }

        [DefaultValue(Border3DStyle.RaisedInner)]
        [Description("获取或设置三维边框样式。")]
        public Border3DStyle Border3DStyle
        {
            get => _border3DStyle;
            set
            {
                if (_border3DStyle != value)
                {
                    _border3DStyle = value;
                    Invalidate( );
                }
            }
        }

        [DefaultValue(ButtonBorderStyle.Solid)]
        [Description("获取或设置线型边框样式。")]
        public ButtonBorderStyle BorderSingleStyle
        {
            get => _borderSingleStyle;
            set
            {
                if (_borderSingleStyle != value)
                {
                    _borderSingleStyle = value;
                    Invalidate( );
                }
            }
        }

        [DefaultValue(typeof(Color), "DarkGray")]
        [Description("获取或设置边框颜色（仅当边框类型为线型时有效）。")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (!(_borderColor == value))
                {
                    _borderColor = value;
                    Invalidate( );
                }
            }
        }

        protected sealed override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style |= 1073741824;
                createParams.Style |= 67108864;
                createParams.Style |= 65536;
                createParams.Style &= -262145;
                createParams.Style &= -8388609;
                createParams.Style &= -4194305;
                createParams.ExStyle = 0;
                createParams.ExStyle |= 65536;
                return createParams;
            }
        }

        public FloatLayerBase( )
        {
            _mouseMsgFilter = new AppMouseMessageHandler(this);
            InitBaseProperties( );
            _borderType = BorderStyle.Fixed3D;
            _border3DStyle = Border3DStyle.RaisedInner;
            _borderSingleStyle = ButtonBorderStyle.Solid;
            _borderColor = Color.DarkGray;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!_isShowDialogAgain)
            {
                if (!DesignMode)
                {
                    Size frameBorderSize = SystemInformation.FrameBorderSize;
                    Size -= frameBorderSize + frameBorderSize;
                }
                base.OnLoad(e);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (!_isShowDialogAgain)
            {
                if (Modal)
                {
                    _isShowDialogAgain = true;
                }
                Control control;
                if (!DesignMode && (control = GetNextControl(this, true)) != null)
                {
                    control.Focus( );
                }
                base.OnShown(e);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 24 && m.WParam != IntPtr.Zero && m.LParam == IntPtr.Zero && Modal && Owner != null && !Owner.IsDisposed)
            {
                if (Owner.IsMdiChild)
                {
                    EnableWindow(Owner.MdiParent.Handle, true);
                    SetParent(Handle, Owner.Handle);
                }
                else
                {
                    EnableWindow(Owner.Handle, true);
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (_borderType == BorderStyle.Fixed3D)
            {
                ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle);
                return;
            }
            if (_borderType == BorderStyle.FixedSingle)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle, BorderColor, BorderSingleStyle);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!DesignMode)
            {
                if (Visible)
                {
                    Application.AddMessageFilter(_mouseMsgFilter);
                }
                else
                {
                    Application.RemoveMessageFilter(_mouseMsgFilter);
                }
            }
            base.OnVisibleChanged(e);
        }

        public DialogResult ShowDialog(Control control) => ShowDialog(control, 0, control.Height);

        public DialogResult ShowDialog(Control control, int offsetX, int offsetY) => ShowDialog(control, new Point(offsetX, offsetY));

        public DialogResult ShowDialog(Control control, Point offset) => ShowDialogInternal(control, offset);

        public DialogResult ShowDialog(ToolStripItem item) => ShowDialog(item, 0, item.Height + 4);

        public DialogResult ShowDialog(ToolStripItem item, int offsetX, int offsetY) => ShowDialog(item, new Point(offsetX, offsetY));

        public DialogResult ShowDialog(ToolStripItem item, Point offset) => ShowDialogInternal(item, offset);

        public void Show(Control control) => Show(control, 0, control.Height);

        public void Show(Control control, int offsetX, int offsetY) => Show(control, new Point(offsetX, offsetY));

        public void Show(Control control, Point offset) => ShowInternal(control, offset);

        public void Show(ToolStripItem item) => Show(item, 0, item.Height);

        public void Show(ToolStripItem item, int offsetX, int offsetY) => Show(item, new Point(offsetX, offsetY));

        public void Show(ToolStripItem item, Point offset) => ShowInternal(item, offset);

        public DialogResult ShowDialogInternal(Component controlOrItem, Point offset)
        {
            DialogResult dialogResult;
            if (Visible)
            {
                dialogResult = DialogResult.None;
            }
            else
            {
                SetLocationAndOwner(controlOrItem, offset);
                dialogResult = base.ShowDialog( );
            }
            return dialogResult;
        }

        public void ShowInternal(Component controlOrItem, Point offset)
        {
            if (!Visible)
            {
                SetLocationAndOwner(controlOrItem, offset);
                base.Show( );
            }
        }

        public void SetLocationAndOwner(Component controlOrItem, Point offset)
        {
            Point empty = Point.Empty;
            if (controlOrItem is ToolStripItem toolStripItem)
            {
                empty.Offset(toolStripItem.Bounds.Location);
                controlOrItem = toolStripItem.Owner;
            }
            Control control = (Control) controlOrItem;
            empty.Offset(GetControlLocationInForm(control));
            empty.Offset(offset);
            Location = empty;
            Owner = control.FindForm( );
        }

        public static Point GetControlLocationInForm(Control c)
        {
            Point location = c.Location;
            while ((c = c.Parent) is not Form)
            {
                location.Offset(c.Location);
            }
            return location;
        }

        public void InitBaseProperties( )
        {
            base.ControlBox = false;
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.Text = string.Empty;
            base.HelpButton = false;
            base.Icon = null;
            base.IsMdiContainer = false;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            base.TopMost = false;
            base.WindowState = FormWindowState.Normal;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("请使用别的重载！", true)]
        public new DialogResult ShowDialog( ) => throw new NotImplementedException( );

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("请使用别的重载！", true)]
        public new DialogResult ShowDialog(IWin32Window owner) => throw new NotImplementedException( );

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("请使用别的重载！", true)]
        public new void Show( ) => throw new NotImplementedException( );

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("请使用别的重载！", true)]
        public new void Show(IWin32Window owner) => throw new NotImplementedException( );

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool ControlBox
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("设置边框请使用Border相关属性！", true)]
        public new FormBorderStyle FormBorderStyle
        {
            get => FormBorderStyle.SizableToolWindow;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public sealed override string Text
        {
            get => string.Empty;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool HelpButton
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new Image Icon
        {
            get => null;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool IsMdiContainer
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool MaximizeBox
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool MinimizeBox
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool ShowIcon
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool ShowInTaskbar
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new FormStartPosition StartPosition
        {
            get => FormStartPosition.Manual;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new bool TopMost
        {
            get => false;
            set
            {
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("禁用该属性！", true)]
        public new FormWindowState WindowState
        {
            get => FormWindowState.Normal;
            set
            {
            }
        }

        public readonly AppMouseMessageHandler _mouseMsgFilter;

        public bool _isShowDialogAgain;

        public BorderStyle _borderType;

        public Border3DStyle _border3DStyle;

        public ButtonBorderStyle _borderSingleStyle;

        public Color _borderColor;

        public class AppMouseMessageHandler : IMessageFilter
        {
            public AppMouseMessageHandler(FloatLayerBase layerForm) => _layerForm = layerForm;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 513 && _layerForm.Visible && !GetWindowRect(_layerForm.Handle).Contains(MousePosition))
                {
                    _layerForm.Hide( );
                }
                return false;
            }

            public readonly FloatLayerBase _layerForm;
        }
    }

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

    public class AdvRichTextBox : RichTextBox
    {
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

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT lp);

        public void SetLineSpace( )
        {
            PARAFORMAT paraformat = default;
            paraformat.cbSize = Marshal.SizeOf(paraformat);
            paraformat.bLineSpacingRule = 4;
            paraformat.dyLineSpacing = 400;
            paraformat.dwMask = 256U;
            SendMessage(new HandleRef(this, Handle), 1095, 0, ref paraformat);
        }

        public string SetLine
        {
            set => SetLineSpace( );
        }

        private int updating;

        private int oldEventMask;

        private struct PARAFORMAT
        {
            public int cbSize;

            public uint dwMask;

            public short wNumbering;

            public short wReserved;

            public int dxStartIndent;

            public int dxRightIndent;

            public int dxOffset;

            public short wAlignment;

            public short cTabCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;

            public int dySpaceBefore;

            public int dySpaceAfter;

            public int dyLineSpacing;

            public short sStyle;

            public byte bLineSpacingRule;

            public byte bOutlineLevel;

            public short wShadingWeight;

            public short wShadingStyle;

            public short wNumberingStart;

            public short wNumberingStyle;

            public short wNumberingTab;

            public short wBorderSpace;

            public short wBorderWidth;

            public short wBorders;
        }
    }

    public enum TextAlign
    {
        Left = 1,
        Right,
        Center,
        Justify
    }
}
