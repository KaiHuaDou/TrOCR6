using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrOCR
{
	public class HelpRepaint
	{
		public class ColorItemx
		{
			public ColorItemx(string name, Color clr)
			{
				this.Name = name;
				this.ItemColor = clr;
			}

			public Color ItemColor
			{
				get
				{
					return this.color;
				}
				set
				{
					this.color = value;
				}
			}

			public string Name
			{
				get
				{
					return this.name;
				}
				set
				{
					this.name = value;
				}
			}

			public string name;

			public Color color;
		}

		public class HWColorPicker : HelpRepaint.FloatLayerBase
		{
			public Color SelectedColor
			{
				get
				{
					return this.selectedColor;
				}
			}

			[DefaultValue(true)]
			[Description("是否显示颜色提示")]
			public bool ShowTip
			{
				get
				{
					return this.showTip;
				}
				set
				{
					this.showTip = value;
				}
			}

			[DefaultValue(typeof(Color), "255, 238, 194")]
			[Description("高亮背景色")]
			public Color HoverBKColor
			{
				get
				{
					return this.hoverBKColor;
				}
				set
				{
					if (this.hoverBKColor != value)
					{
						if (this.hoverBrush != null)
						{
							this.hoverBrush.Dispose();
						}
						this.hoverBrush = new SolidBrush(value);
					}
					this.hoverBKColor = value;
				}
			}

			public List<HelpRepaint.ColorItemx> ColorTable
			{
				get
				{
					return this.colorTable;
				}
			}

			public HWColorPicker()
			{
				this.Font = new Font(this.Font.Name, 9f / StaticValue.Dpifactor, this.Font.Style, this.Font.Unit, this.Font.GdiCharSet, this.Font.GdiVerticalFont);
				this.hoverItem = -1;
				this.InitializeComponent();
				this.InitColor();
				this.CalcWindowSize();
				this.sf = new StringFormat();
				this.sf.Alignment = StringAlignment.Center;
				this.sf.LineAlignment = StringAlignment.Center;
				this.HoverBKColor = Color.FromArgb(255, 238, 194);
				this.ShowTip = true;
			}

			public void CalcWindowSize()
			{
				int num = 152;
				int num2 = 100;
				base.Size = new Size(num, num2);
			}

			public Rectangle CalcItemBound(int index)
			{
				if (index < 0 || index > 40)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				Rectangle rectangle;
				if (index == 40)
				{
					rectangle = Rectangle.FromLTRB(0, 0, 0, 0);
				}
				else
				{
					rectangle = new Rectangle(index % 8 * 18 + 3, index / 8 * 18 + 3, 18, 18);
				}
				return rectangle;
			}

			public int GetIndexFromPoint(Point point)
			{
				int num;
				if (point.X <= 3 || point.X >= base.Width - 3 || point.Y <= 3 || point.Y >= base.Height - 3)
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
				this.selectedColor = clr;
				if (this.SelectedColorChanged != null)
				{
					this.SelectedColorChanged(this.ctl_T, EventArgs.Empty);
				}
				base.Hide();
			}

			public void DrawItem(DrawItemEventArgs e)
			{
				Rectangle bounds = e.Bounds;
				bounds.Inflate(-1, -1);
				e.DrawBackground();
				if ((e.State & DrawItemState.HotLight) > DrawItemState.None)
				{
					e.Graphics.FillRectangle(this.hoverBrush, bounds);
					e.Graphics.DrawRectangle(Pens.Black, bounds);
				}
				if (e.Index != 40)
				{
					Rectangle bounds2 = e.Bounds;
					bounds2.Inflate(-3, -3);
					using (Brush brush = new SolidBrush(this.ColorTable[e.Index].ItemColor))
					{
						e.Graphics.FillRectangle(brush, bounds2);
					}
					e.Graphics.DrawRectangle(Pens.Gray, bounds2);
				}
			}

			public void HWColorPicker_Paint(object sender, PaintEventArgs e)
			{
				e.Graphics.DrawRectangle(Pens.LightGray, 0, 0, 150, 98);
				for (int i = 0; i < 40; i++)
				{
					DrawItemEventArgs drawItemEventArgs = new DrawItemEventArgs(e.Graphics, this.Font, this.CalcItemBound(i), i, DrawItemState.Default, this.ForeColor, this.BackColor);
					this.DrawItem(drawItemEventArgs);
				}
			}

			public void HWColorPicker_MouseMove(object sender, MouseEventArgs e)
			{
				int indexFromPoint = this.GetIndexFromPoint(e.Location);
				if (indexFromPoint != this.hoverItem)
				{
					Graphics graphics = base.CreateGraphics();
					if (this.hoverItem != -1)
					{
						this.DrawItem(new DrawItemEventArgs(graphics, this.Font, this.CalcItemBound(this.hoverItem), this.hoverItem, DrawItemState.Default));
					}
					if (indexFromPoint <= 40)
					{
						if (indexFromPoint != -1)
						{
							if (this.ShowTip)
							{
								this.ShowToolTip(indexFromPoint);
							}
							this.DrawItem(new DrawItemEventArgs(graphics, this.Font, this.CalcItemBound(indexFromPoint), indexFromPoint, DrawItemState.Default | DrawItemState.HotLight));
						}
						graphics.Dispose();
						this.hoverItem = indexFromPoint;
					}
				}
			}

			public void HWColorPicker_MouseClick(object sender, MouseEventArgs e)
			{
				int indexFromPoint = this.GetIndexFromPoint(e.Location);
				if (indexFromPoint != -1 && indexFromPoint != 40)
				{
					this.SelectedColored = this.colorTable[indexFromPoint].ItemColor;
					base.DialogResult = DialogResult.OK;
				}
			}

			public void ShowToolTip(int index)
			{
				if (index != 40)
				{
					this.tip.SetToolTip(this, this.ColorTable[index].Name);
				}
			}

			public void InitColor()
			{
				this.colorTable = new List<HelpRepaint.ColorItemx>();
				this.colorTable.AddRange(new HelpRepaint.ColorItemx[]
				{
					new HelpRepaint.ColorItemx("黑色", Color.FromArgb(0, 0, 0)),
					new HelpRepaint.ColorItemx("褐色", Color.FromArgb(153, 51, 0)),
					new HelpRepaint.ColorItemx("橄榄色", Color.FromArgb(51, 51, 0)),
					new HelpRepaint.ColorItemx("深绿", Color.FromArgb(0, 51, 0)),
					new HelpRepaint.ColorItemx("深青", Color.FromArgb(0, 51, 102)),
					new HelpRepaint.ColorItemx("深蓝", Color.FromArgb(0, 0, 128)),
					new HelpRepaint.ColorItemx("靛蓝", Color.FromArgb(51, 51, 153)),
					new HelpRepaint.ColorItemx("灰色-80%", Color.FromArgb(51, 51, 51)),
					new HelpRepaint.ColorItemx("深红", Color.FromArgb(128, 0, 0)),
					new HelpRepaint.ColorItemx("橙色", Color.FromArgb(255, 102, 0)),
					new HelpRepaint.ColorItemx("深黄", Color.FromArgb(128, 128, 0)),
					new HelpRepaint.ColorItemx("绿色", Color.FromArgb(0, 128, 0)),
					new HelpRepaint.ColorItemx("青色", Color.FromArgb(0, 128, 128)),
					new HelpRepaint.ColorItemx("蓝色", Color.FromArgb(0, 0, 255)),
					new HelpRepaint.ColorItemx("蓝灰", Color.FromArgb(102, 102, 153)),
					new HelpRepaint.ColorItemx("灰色-50%", Color.FromArgb(128, 128, 128)),
					new HelpRepaint.ColorItemx("红色", Color.FromArgb(255, 0, 0)),
					new HelpRepaint.ColorItemx("浅橙", Color.FromArgb(255, 153, 0)),
					new HelpRepaint.ColorItemx("酸橙", Color.FromArgb(153, 204, 0)),
					new HelpRepaint.ColorItemx("海绿", Color.FromArgb(51, 153, 102)),
					new HelpRepaint.ColorItemx("水绿", Color.FromArgb(51, 204, 204)),
					new HelpRepaint.ColorItemx("浅蓝", Color.FromArgb(51, 102, 255)),
					new HelpRepaint.ColorItemx("紫罗兰", Color.FromArgb(128, 0, 128)),
					new HelpRepaint.ColorItemx("灰色-40%", Color.FromArgb(153, 153, 153)),
					new HelpRepaint.ColorItemx("粉红", Color.FromArgb(255, 0, 255)),
					new HelpRepaint.ColorItemx("金色", Color.FromArgb(255, 204, 0)),
					new HelpRepaint.ColorItemx("黄色", Color.FromArgb(255, 255, 0)),
					new HelpRepaint.ColorItemx("鲜绿", Color.FromArgb(0, 255, 0)),
					new HelpRepaint.ColorItemx("青绿", Color.FromArgb(0, 255, 255)),
					new HelpRepaint.ColorItemx("天蓝", Color.FromArgb(0, 204, 255)),
					new HelpRepaint.ColorItemx("梅红", Color.FromArgb(153, 51, 102)),
					new HelpRepaint.ColorItemx("灰色-25%", Color.FromArgb(192, 192, 192)),
					new HelpRepaint.ColorItemx("玫瑰红", Color.FromArgb(255, 153, 204)),
					new HelpRepaint.ColorItemx("茶色", Color.FromArgb(255, 204, 153)),
					new HelpRepaint.ColorItemx("浅黄", Color.FromArgb(255, 255, 204)),
					new HelpRepaint.ColorItemx("浅绿", Color.FromArgb(204, 255, 204)),
					new HelpRepaint.ColorItemx("浅青绿", Color.FromArgb(204, 255, 255)),
					new HelpRepaint.ColorItemx("淡蓝", Color.FromArgb(153, 204, 255)),
					new HelpRepaint.ColorItemx("淡紫", Color.FromArgb(204, 153, 255)),
					new HelpRepaint.ColorItemx("白色", Color.FromArgb(255, 255, 255))
				});
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && this.components != null)
				{
					this.components.Dispose();
				}
				base.Dispose(disposing);
			}

			public void InitializeComponent()
			{
				this.components = new Container();
				this.tip = new ToolTip(this.components);
				base.SuspendLayout();
				this.BackColor = Color.White;
				base.AutoScaleMode = AutoScaleMode.None;
				base.Name = "HWColorPicker";
				base.Paint += this.HWColorPicker_Paint;
				base.MouseClick += this.HWColorPicker_MouseClick;
				base.MouseMove += this.HWColorPicker_MouseMove;
				base.ClientSize = new Size(114, 115);
				base.SizeGripStyle = SizeGripStyle.Hide;
				base.ResumeLayout(false);
			}

			public Color SelectedColored { get; private set; }

			[CompilerGenerated]
			private EventHandler SelectedColorChanged;

			public bool showTip;

			public Color selectedColor;

			public Color hoverBKColor;

			public List<HelpRepaint.ColorItemx> colorTable;

			public StringFormat sf;

			public int hoverItem;

			public Control ctl;

			public Brush hoverBrush;

			public IContainer components;

			public ToolTip tip;

			public ToolStripButton ctl_T;
		}

		public class MenuItemRenderer : ToolStripProfessionalRenderer
		{
			public MenuItemRenderer()
			{
				this.textFont = new Font("微软雅黑", 9f / StaticValue.Dpifactor, FontStyle.Regular, GraphicsUnit.Point, 0);
				this.menuItemSelectedColor = Color.White;
				this.menuItemBorderColor = Color.White;
				this.menuItemSelectedColor = Color.White;
				this.menuItemBorderColor = Color.LightSlateGray;
			}

			protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
			{
				if (e.Item.IsOnDropDown)
				{
					if (e.Item.Selected && e.Item.Enabled)
					{
						this.DrawMenuDropDownItemHighlight(e);
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
				Rectangle rectangle = default(Rectangle);
				rectangle = new Rectangle(2, 0, (int)e.Graphics.VisibleClipBounds.Width - 36, (int)e.Graphics.VisibleClipBounds.Height - 1);
				using (Pen pen = new Pen(this.menuItemBorderColor))
				{
					e.Graphics.DrawRectangle(pen, rectangle);
				}
			}

			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
				ToolStrip toolStrip = e.ToolStrip;
				e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
				if (e.ToolStrip is MenuStrip || e.ToolStrip is ToolStripDropDown)
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
					Pen pen = new Pen(Color.LightSlateGray);
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
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(e.Item.Width, 0), Color.LightGray, Color.FromArgb(0, Color.LightGray));
					graphics.FillRectangle(linearGradientBrush, new Rectangle(33, e.Item.Height / 2, e.Item.Width / 4 * 3, 1));
				}
			}

			protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
			{
				ToolStripItem item = e.Item;
				Graphics graphics = e.Graphics;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				ToolStripItem item2 = e.Item;
				if (item.Selected)
				{
					Pen pen = new Pen(Color.LightSlateGray);
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
		}

		public class MenuItemRendererT : ToolStripProfessionalRenderer
		{
			public MenuItemRendererT()
			{
				this.textFont = new Font("微软雅黑", 9f / StaticValue.Dpifactor, FontStyle.Regular, GraphicsUnit.Point, 0);
				this.menuItemSelectedColor = Color.White;
				this.menuItemBorderColor = Color.White;
				this.menuItemSelectedColor = Color.White;
				this.menuItemBorderColor = Color.LightSlateGray;
			}

			protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
			{
				if (e.Item.IsOnDropDown)
				{
					if (e.Item.Selected && e.Item.Enabled)
					{
						this.DrawMenuDropDownItemHighlight(e);
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
				Rectangle rectangle = default(Rectangle);
				rectangle = new Rectangle(2, 0, (int)e.Graphics.VisibleClipBounds.Width - 4, (int)e.Graphics.VisibleClipBounds.Height - 1);
				using (Pen pen = new Pen(this.menuItemBorderColor))
				{
					e.Graphics.DrawRectangle(pen, rectangle);
				}
			}

			public Font textFont;

			public Color menuItemSelectedColor;

			public Color menuItemBorderColor;
		}

		public class myWebBroswer : WebBrowser
		{
			public override bool PreProcessMessage(ref Message msg)
			{
				if (msg.Msg == 256 && (int)msg.WParam == 86 && Control.ModifierKeys == Keys.Control)
				{
					Clipboard.SetDataObject((string)Clipboard.GetDataObject().GetData(DataFormats.UnicodeText));
					HelpWin32.keybd_event(Keys.ControlKey, 0, 0U, 0U);
					HelpWin32.keybd_event(Keys.D9, 0, 0U, 0U);
					HelpWin32.keybd_event(Keys.D9, 0, 2U, 0U);
					HelpWin32.keybd_event(Keys.ControlKey, 0, 2U, 0U);
				}
				if (msg.Msg == 256 && (int)msg.WParam == 67 && Control.ModifierKeys == Keys.Control)
				{
					HelpWin32.keybd_event(Keys.ControlKey, 0, 0U, 0U);
					HelpWin32.keybd_event(Keys.D8, 0, 0U, 0U);
					HelpWin32.keybd_event(Keys.D8, 0, 2U, 0U);
					HelpWin32.keybd_event(Keys.ControlKey, 0, 2U, 0U);
				}
				return base.PreProcessMessage(ref msg);
			}
		}

		[DefaultEvent("SelectedColorChanged")]
		[DefaultProperty("Color")]
		[Description("ToolStripItem that allows selecting a color from a color picker control.")]
		[ToolboxItem(false)]
		[ToolboxBitmap(typeof(HelpRepaint.ToolStripColorPicker), "ToolStripColorPicker")]
		public class ToolStripColorPicker : ToolStripButton
		{
			public Point GetOpenPoint()
			{
				Point point;
				if (base.Owner == null)
				{
					point = new Point(5, 5);
				}
				else
				{
					int num = 0;
					foreach (object obj in base.Parent.Items)
					{
						ToolStripItem toolStripItem = (ToolStripItem)obj;
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
				get
				{
					return this.GetOpenPoint();
				}
				set
				{
				}
			}
		}

		public class ToolStripEx : ToolStrip
		{
			protected override void WndProc(ref Message m)
			{
				if (m.Msg == 33 && base.CanFocus && !this.Focused)
				{
					base.Focus();
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
				get
				{
					return this._borderType;
				}
				set
				{
					if (this._borderType != value)
					{
						this._borderType = value;
						base.Invalidate();
					}
				}
			}

			[DefaultValue(Border3DStyle.RaisedInner)]
			[Description("获取或设置三维边框样式。")]
			public Border3DStyle Border3DStyle
			{
				get
				{
					return this._border3DStyle;
				}
				set
				{
					if (this._border3DStyle != value)
					{
						this._border3DStyle = value;
						base.Invalidate();
					}
				}
			}

			[DefaultValue(ButtonBorderStyle.Solid)]
			[Description("获取或设置线型边框样式。")]
			public ButtonBorderStyle BorderSingleStyle
			{
				get
				{
					return this._borderSingleStyle;
				}
				set
				{
					if (this._borderSingleStyle != value)
					{
						this._borderSingleStyle = value;
						base.Invalidate();
					}
				}
			}

			[DefaultValue(typeof(Color), "DarkGray")]
			[Description("获取或设置边框颜色（仅当边框类型为线型时有效）。")]
			public Color BorderColor
			{
				get
				{
					return this._borderColor;
				}
				set
				{
					if (!(this._borderColor == value))
					{
						this._borderColor = value;
						base.Invalidate();
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

			public FloatLayerBase()
			{
				this._mouseMsgFilter = new HelpRepaint.FloatLayerBase.AppMouseMessageHandler(this);
				this.InitBaseProperties();
				this._borderType = BorderStyle.Fixed3D;
				this._border3DStyle = Border3DStyle.RaisedInner;
				this._borderSingleStyle = ButtonBorderStyle.Solid;
				this._borderColor = Color.DarkGray;
			}

			protected override void OnLoad(EventArgs e)
			{
				if (!this._isShowDialogAgain)
				{
					if (!base.DesignMode)
					{
						Size frameBorderSize = SystemInformation.FrameBorderSize;
						base.Size -= frameBorderSize + frameBorderSize;
					}
					base.OnLoad(e);
				}
			}

			protected override void OnShown(EventArgs e)
			{
				if (!this._isShowDialogAgain)
				{
					if (base.Modal)
					{
						this._isShowDialogAgain = true;
					}
					Control control = null;
					if (!base.DesignMode && (control = base.GetNextControl(this, true)) != null)
					{
						control.Focus();
					}
					base.OnShown(e);
				}
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == 24 && m.WParam != IntPtr.Zero && m.LParam == IntPtr.Zero && base.Modal && base.Owner != null && !base.Owner.IsDisposed)
				{
					if (base.Owner.IsMdiChild)
					{
						HelpRepaint.FloatLayerBase.NativeMethods.EnableWindow(base.Owner.MdiParent.Handle, true);
						HelpRepaint.FloatLayerBase.NativeMethods.SetParent(base.Handle, base.Owner.Handle);
					}
					else
					{
						HelpRepaint.FloatLayerBase.NativeMethods.EnableWindow(base.Owner.Handle, true);
					}
				}
				base.WndProc(ref m);
			}

			protected override void OnPaintBackground(PaintEventArgs e)
			{
				base.OnPaintBackground(e);
				if (this._borderType == BorderStyle.Fixed3D)
				{
					ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, this.Border3DStyle);
					return;
				}
				if (this._borderType == BorderStyle.FixedSingle)
				{
					ControlPaint.DrawBorder(e.Graphics, base.ClientRectangle, this.BorderColor, this.BorderSingleStyle);
				}
			}

			protected override void OnVisibleChanged(EventArgs e)
			{
				if (!base.DesignMode)
				{
					if (base.Visible)
					{
						Application.AddMessageFilter(this._mouseMsgFilter);
					}
					else
					{
						Application.RemoveMessageFilter(this._mouseMsgFilter);
					}
				}
				base.OnVisibleChanged(e);
			}

			public DialogResult ShowDialog(Control control)
			{
				return this.ShowDialog(control, 0, control.Height);
			}

			public DialogResult ShowDialog(Control control, int offsetX, int offsetY)
			{
				return this.ShowDialog(control, new Point(offsetX, offsetY));
			}

			public DialogResult ShowDialog(Control control, Point offset)
			{
				return this.ShowDialogInternal(control, offset);
			}

			public DialogResult ShowDialog(ToolStripItem item)
			{
				return this.ShowDialog(item, 0, item.Height + 4);
			}

			public DialogResult ShowDialog(ToolStripItem item, int offsetX, int offsetY)
			{
				return this.ShowDialog(item, new Point(offsetX, offsetY));
			}

			public DialogResult ShowDialog(ToolStripItem item, Point offset)
			{
				return this.ShowDialogInternal(item, offset);
			}

			public void Show(Control control)
			{
				this.Show(control, 0, control.Height);
			}

			public void Show(Control control, int offsetX, int offsetY)
			{
				this.Show(control, new Point(offsetX, offsetY));
			}

			public void Show(Control control, Point offset)
			{
				this.ShowInternal(control, offset);
			}

			public void Show(ToolStripItem item)
			{
				this.Show(item, 0, item.Height);
			}

			public void Show(ToolStripItem item, int offsetX, int offsetY)
			{
				this.Show(item, new Point(offsetX, offsetY));
			}

			public void Show(ToolStripItem item, Point offset)
			{
				this.ShowInternal(item, offset);
			}

			public DialogResult ShowDialogInternal(Component controlOrItem, Point offset)
			{
				DialogResult dialogResult;
				if (base.Visible)
				{
					dialogResult = DialogResult.None;
				}
				else
				{
					this.SetLocationAndOwner(controlOrItem, offset);
					dialogResult = base.ShowDialog();
				}
				return dialogResult;
			}

			public void ShowInternal(Component controlOrItem, Point offset)
			{
				if (!base.Visible)
				{
					this.SetLocationAndOwner(controlOrItem, offset);
					base.Show();
				}
			}

			public void SetLocationAndOwner(Component controlOrItem, Point offset)
			{
				Point empty = Point.Empty;
				if (controlOrItem is ToolStripItem)
				{
					ToolStripItem toolStripItem = (ToolStripItem)controlOrItem;
					empty.Offset(toolStripItem.Bounds.Location);
					controlOrItem = toolStripItem.Owner;
				}
				Control control = (Control)controlOrItem;
				empty.Offset(HelpRepaint.FloatLayerBase.GetControlLocationInForm(control));
				empty.Offset(offset);
				base.Location = empty;
				base.Owner = control.FindForm();
			}

			public static Point GetControlLocationInForm(Control c)
			{
				Point location = c.Location;
				while (!((c = c.Parent) is Form))
				{
					location.Offset(c.Location);
				}
				return location;
			}

			public void InitBaseProperties()
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
			public new DialogResult ShowDialog()
			{
				throw new NotImplementedException();
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("请使用别的重载！", true)]
			public new DialogResult ShowDialog(IWin32Window owner)
			{
				throw new NotImplementedException();
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("请使用别的重载！", true)]
			public new void Show()
			{
				throw new NotImplementedException();
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("请使用别的重载！", true)]
			public new void Show(IWin32Window owner)
			{
				throw new NotImplementedException();
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool ControlBox
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("设置边框请使用Border相关属性！", true)]
			public new FormBorderStyle FormBorderStyle
			{
				get
				{
					return FormBorderStyle.SizableToolWindow;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public sealed override string Text
			{
				get
				{
					return string.Empty;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool HelpButton
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new Image Icon
			{
				get
				{
					return null;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool IsMdiContainer
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool MaximizeBox
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool MinimizeBox
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool ShowIcon
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool ShowInTaskbar
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new FormStartPosition StartPosition
			{
				get
				{
					return FormStartPosition.Manual;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new bool TopMost
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("禁用该属性！", true)]
			public new FormWindowState WindowState
			{
				get
				{
					return FormWindowState.Normal;
				}
				set
				{
				}
			}

			public readonly HelpRepaint.FloatLayerBase.AppMouseMessageHandler _mouseMsgFilter;

			public bool _isShowDialogAgain;

			public BorderStyle _borderType;

			public Border3DStyle _border3DStyle;

			public ButtonBorderStyle _borderSingleStyle;

			public Color _borderColor;

			public class AppMouseMessageHandler : IMessageFilter
			{
				public AppMouseMessageHandler(HelpRepaint.FloatLayerBase layerForm)
				{
					this._layerForm = layerForm;
				}

				public bool PreFilterMessage(ref Message m)
				{
					if (m.Msg == 513 && this._layerForm.Visible && !HelpRepaint.FloatLayerBase.NativeMethods.GetWindowRect(this._layerForm.Handle).Contains(Control.MousePosition))
					{
						this._layerForm.Hide();
					}
					return false;
				}

				public readonly HelpRepaint.FloatLayerBase _layerForm;
			}

			public static class NativeMethods
			{
				[DllImport("user32.dll")]
				[return: MarshalAs(UnmanagedType.Bool)]
				public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

				[DllImport("user32.dll", CharSet = CharSet.Auto)]
				public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

				[DllImport("user32.dll")]
				public static extern bool ReleaseCapture();

				[DllImport("user32.dll", SetLastError = true)]
				public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

				[DllImport("user32.dll", SetLastError = true)]
				public static extern bool GetWindowRect(IntPtr hwnd, out HelpRepaint.FloatLayerBase.NativeMethods.RECT lpRect);

				public static Rectangle GetWindowRect(IntPtr hwnd)
				{
					HelpRepaint.FloatLayerBase.NativeMethods.RECT rect;
					HelpRepaint.FloatLayerBase.NativeMethods.GetWindowRect(hwnd, out rect);
					return (Rectangle)rect;
				}

				public struct RECT
				{
					public static explicit operator Rectangle(HelpRepaint.FloatLayerBase.NativeMethods.RECT rect)
					{
						return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
					}

					public int left;

					public int top;

					public int right;

					public int bottom;
				}
			}
		}

		public class ColorPicker : ToolStripButton
		{
			public ColorPicker()
			{
				this.cp = new HelpRepaint.HWColorPicker
				{
					BorderType = BorderStyle.FixedSingle
				};
			}

			protected override void OnClick(EventArgs e)
			{
				if (this.cp.ShowDialog(this) == DialogResult.OK)
				{
					this.select_color = this.cp.SelectedColored;
					base.OnClick(e);
				}
			}

			public Color SelectedColor
			{
				get
				{
					return this.select_color;
				}
			}

			private readonly HelpRepaint.HWColorPicker cp;

			public Color select_color;
		}

		public class AdvRichTextBox : RichTextBox
		{
			public void BeginUpdate()
			{
				this.updating++;
				if (this.updating <= 1)
				{
					this.oldEventMask = HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 1073, 0, 0);
					HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 11, 0, 0);
				}
			}

			public void EndUpdate()
			{
				this.updating--;
				if (this.updating <= 0)
				{
					HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 11, 1, 0);
					HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 1073, 0, this.oldEventMask);
				}
			}

			public new HelpRepaint.TextAlign SelectionAlignment
			{
				get
				{
					HelpRepaint.AdvRichTextBox.PARAFORMAT paraformat = default(HelpRepaint.AdvRichTextBox.PARAFORMAT);
					paraformat.cbSize = Marshal.SizeOf(paraformat);
					HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 1085, 1, ref paraformat);
					HelpRepaint.TextAlign textAlign;
					if ((paraformat.dwMask & 8U) == 0U)
					{
						textAlign = HelpRepaint.TextAlign.Left;
					}
					else
					{
						textAlign = (HelpRepaint.TextAlign)paraformat.wAlignment;
					}
					return textAlign;
				}
				set
				{
					HelpRepaint.AdvRichTextBox.PARAFORMAT paraformat = default(HelpRepaint.AdvRichTextBox.PARAFORMAT);
					paraformat.cbSize = Marshal.SizeOf(paraformat);
					paraformat.dwMask = 8U;
					paraformat.wAlignment = (short)value;
					HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 1095, 1, ref paraformat);
				}
			}

			protected override void OnHandleCreated(EventArgs e)
			{
				base.OnHandleCreated(e);
				if (!base.AutoWordSelection)
				{
					base.AutoWordSelection = true;
					base.AutoWordSelection = false;
				}
				HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 1226, 1, 1);
			}

			[DllImport("user32", CharSet = CharSet.Auto)]
			private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

			[DllImport("user32", CharSet = CharSet.Auto)]
			private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref HelpRepaint.AdvRichTextBox.PARAFORMAT lp);

			public void SetLineSpace()
			{
				HelpRepaint.AdvRichTextBox.PARAFORMAT paraformat = default(HelpRepaint.AdvRichTextBox.PARAFORMAT);
				paraformat.cbSize = Marshal.SizeOf(paraformat);
				paraformat.bLineSpacingRule = 4;
				paraformat.dyLineSpacing = 400;
				paraformat.dwMask = 256U;
				HelpRepaint.AdvRichTextBox.SendMessage(new HandleRef(this, base.Handle), 1095, 0, ref paraformat);
			}

			public string SetLine
			{
				set
				{
					this.SetLineSpace();
				}
			}

			private const int EM_SETEVENTMASK = 1073;

			private const int EM_GETPARAFORMAT = 1085;

			private const int EM_SETPARAFORMAT = 1095;

			private const int EM_SETTYPOGRAPHYOPTIONS = 1226;

			private const int WM_SETREDRAW = 11;

			private const int TO_ADVANCEDTYPOGRAPHY = 1;

			private const int PFM_ALIGNMENT = 8;

			private const int SCF_SELECTION = 1;

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
}
