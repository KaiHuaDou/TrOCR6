using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TrOCR
{
	public class RichTextBoxEx : HelpRepaint.AdvRichTextBox
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new Container();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string path);

		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		[SettingsBindable(true)]
		public string Rtf2
		{
			get
			{
				return base.Rtf;
			}
			set
			{
				base.Rtf = value;
			}
		}

		private IContainer components;

		private static IntPtr moduleHandle;
	}
}
