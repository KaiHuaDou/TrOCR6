using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR
{
	public partial class Fmloading : Form
	{
		public Fmloading()
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
			if (Image.IsCanonicalPixelFormat(bitmap.PixelFormat) && Image.IsAlphaPixelFormat(bitmap.PixelFormat))
			{
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
					return;
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
			throw new ApplicationException("图片必须是32位带Alhpa通道的图片。");
		}

		public void InitializeComponent()
		{
			this.timer = new Timer();
			this.fm_close = "窗体已开启";
			base.SuspendLayout();
			base.ShowInTaskbar = false;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.ForeColor = Color.Aqua;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Name = "Form1";
			this.Text = "Form1";
			base.TopMost = true;
			base.ClientSize = new Size(120, 120);
			base.Location = (Point)new Size(500, 500);
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Set_png();
			base.ResumeLayout(false);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			try
			{
				if (this.fm_close != "窗体已开启")
				{
					base.Close();
				}
				if (this.i_c >= this.fla_1)
				{
					this.i_c = 0;
				}
				this.bgImg = (Image)new ComponentResourceManager(typeof(Fmloading)).GetObject(this.i_c + this.fla_2 + ".png");
				this.SetBits((Bitmap)this.bgImg);
				this.i_c++;
			}
			catch
			{
				MessageBox.Show("加载窗体关闭报错");
			}
		}

		public void Set_png()
		{
			string text;
			try
			{
				text = IniHelp.GetValue("配置", "窗体动画");
			}
			catch
			{
				text = "窗体";
			}
			if (text == "少女")
			{
				this.timer.Interval = 50;
				this.fla_1 = 27;
				this.fla_2 = "";
			}
			else if (text == "罗小黑")
			{
				this.timer.Interval = 18;
				this.fla_1 = 46;
				this.fla_2 = "_luo";
			}
			else
			{
				this.timer.Interval = 80;
				this.fla_1 = 4;
				this.fla_2 = "_load";
			}
			this.bgImg = null;
			this.i_c = 0;
			this.timer.Tick += this.timer1_Tick;
			this.timer.Start();
		}

		public string fml_close
		{
			get
			{
				return this.fm_close;
			}
			set
			{
				this.fm_close = value;
			}
		}

		public int i_c;

		private Image bgImg;

		public Timer timer;

		public int fla_1;

		public string fla_2;

		public string str;

		public string fm_close;
	}
}
