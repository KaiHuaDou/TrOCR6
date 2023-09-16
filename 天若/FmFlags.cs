using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;

namespace TrOCR
{
	public partial class FmFlags : Form
	{
		public FmFlags()
		{
			this.InitializeComponent();
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style |= 131072;
				if (!base.DesignMode)
				{
					createParams.ExStyle |= 524288;
				}
				return createParams;
			}
		}

		public void SetBits(Bitmap bitmap)
		{
			if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
			{
				throw new ApplicationException("图片必须是32位带Alhpa通道的图片。");
			}
			IntPtr intPtr = IntPtr.Zero;
			IntPtr dc = HelpWin32.GetDC(IntPtr.Zero);
			IntPtr intPtr2 = IntPtr.Zero;
			IntPtr intPtr3 = HelpWin32.CreateCompatibleDC(dc);
			try
			{
				HelpWin32.Point point = new HelpWin32.Point(base.Left, base.Top);
				HelpWin32.Size size = new HelpWin32.Size(bitmap.Width, bitmap.Height);
				HelpWin32.BLENDFUNCTION blendfunction = default(HelpWin32.BLENDFUNCTION);
				HelpWin32.Point point2 = new HelpWin32.Point(0, 0);
				intPtr2 = bitmap.GetHbitmap(Color.FromArgb(0));
				intPtr = HelpWin32.SelectObject(intPtr3, intPtr2);
				blendfunction.BlendOp = 0;
				blendfunction.SourceConstantAlpha = byte.MaxValue;
				blendfunction.AlphaFormat = 1;
				blendfunction.BlendFlags = 0;
				HelpWin32.UpdateLayeredWindow(base.Handle, dc, ref point, ref size, intPtr3, ref point2, 0, ref blendfunction, 2);
			}
			finally
			{
				if (intPtr2 != IntPtr.Zero)
				{
					HelpWin32.SelectObject(intPtr3, intPtr);
					HelpWin32.DeleteObject(intPtr2);
				}
				HelpWin32.ReleaseDC(IntPtr.Zero, dc);
				HelpWin32.DeleteDC(intPtr3);
			}
		}

		public void DrawStr(string str)
		{
			this.宽度 = 50 * str.Length;
			base.ClientSize = new Size(this.宽度, 50);
			base.Location = new Point((Screen.PrimaryScreen.Bounds.Width - base.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - base.Height) / 2 / 3 * 5);
			this.bmp = new Bitmap(this.宽度, 50);
			this.g = Graphics.FromImage(this.bmp);
			this.g.InterpolationMode = InterpolationMode.Bilinear;
			this.g.SmoothingMode = SmoothingMode.HighQuality;
			this.g.TextRenderingHint = TextRenderingHint.AntiAlias;
			this.g.Clear(Color.Transparent);
			this.g.FillRectangle(new SolidBrush(Color.FromArgb(1, 255, 255, 255)), base.ClientRectangle);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			Rectangle rectangle = new Rectangle(0, 3, this.宽度, 50);
			this.g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 1, 1, this.宽度 - 2, 48);
			this.g.DrawRectangle(new Pen(Color.FromArgb(224, 224, 224)), 2, 2, this.宽度 - 2 - 2, 46);
			this.g.DrawString(str, new Font("微软雅黑", 24f / Program.factor), new SolidBrush(Color.FromArgb(255, Color.White)), rectangle, stringFormat);
			this.SetBits(this.bmp);
			this.g.Dispose();
			this.bmp.Dispose();
			this.Delay(600U);
			base.Hide();
		}

		private void Delay(uint ms)
		{
			uint tickCount = HelpWin32.GetTickCount();
			while (HelpWin32.GetTickCount() - tickCount < ms)
			{
				Thread.Sleep(1);
				Application.DoEvents();
			}
		}

		public void DrawStr_update(string str)
		{
			this.宽度 = 28 * str.Length;
			base.ClientSize = new Size(this.宽度, 50);
			this.bmp = new Bitmap(this.宽度, 50);
			this.g = Graphics.FromImage(this.bmp);
			this.g.InterpolationMode = InterpolationMode.Bilinear;
			this.g.SmoothingMode = SmoothingMode.HighQuality;
			this.g.TextRenderingHint = TextRenderingHint.AntiAlias;
			this.g.Clear(Color.Transparent);
			this.g.FillRectangle(new SolidBrush(Color.FromArgb(1, 255, 255, 255)), base.ClientRectangle);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			Rectangle rectangle = new Rectangle(0, 10, this.宽度, 48);
			this.g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 1, 1, this.宽度 - 2, 48);
			this.g.DrawRectangle(new Pen(Color.FromArgb(224, 224, 224)), 1, 1, this.宽度 - 2, 48);
			this.g.DrawString(str, new Font("微软雅黑", 18f), new SolidBrush(Color.FromArgb(255, Color.White)), rectangle, stringFormat);
			this.SetBits_update(this.bmp);
			this.Delay(2000U);
			base.Hide();
		}

		public void SetBits_update(Bitmap bitmap)
		{
			if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
			{
				throw new ApplicationException("图片必须是32位带Alhpa通道的图片。");
			}
			IntPtr intPtr = IntPtr.Zero;
			IntPtr dc = HelpWin32.GetDC(IntPtr.Zero);
			IntPtr intPtr2 = IntPtr.Zero;
			IntPtr intPtr3 = HelpWin32.CreateCompatibleDC(dc);
			try
			{
				HelpWin32.Point point = new HelpWin32.Point(Screen.PrimaryScreen.Bounds.Width - base.Width, Screen.PrimaryScreen.WorkingArea.Height - 50);
				HelpWin32.Size size = new HelpWin32.Size(bitmap.Width, bitmap.Height);
				HelpWin32.BLENDFUNCTION blendfunction = default(HelpWin32.BLENDFUNCTION);
				HelpWin32.Point point2 = new HelpWin32.Point(0, 0);
				intPtr2 = bitmap.GetHbitmap(Color.FromArgb(0));
				intPtr = HelpWin32.SelectObject(intPtr3, intPtr2);
				blendfunction.BlendOp = 0;
				blendfunction.SourceConstantAlpha = byte.MaxValue;
				blendfunction.AlphaFormat = 1;
				blendfunction.BlendFlags = 0;
				HelpWin32.UpdateLayeredWindow(base.Handle, dc, ref point, ref size, intPtr3, ref point2, 0, ref blendfunction, 2);
			}
			finally
			{
				if (intPtr2 != IntPtr.Zero)
				{
					HelpWin32.SelectObject(intPtr3, intPtr);
					HelpWin32.DeleteObject(intPtr2);
				}
				HelpWin32.ReleaseDC(IntPtr.Zero, dc);
				HelpWin32.DeleteDC(intPtr3);
			}
		}

		private Bitmap bmp;

		private Graphics g;

		public int 宽度;
	}
}
