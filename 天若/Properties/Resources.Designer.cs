using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TrOCR.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		internal Resources()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("TrOCR.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static Bitmap 二维码
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("二维码", Resources.resourceCulture);
			}
		}

		internal static Bitmap 头像
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("头像", Resources.resourceCulture);
			}
		}

		internal static Bitmap 帮助
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("帮助", Resources.resourceCulture);
			}
		}

		internal static Bitmap 快捷键_0
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("快捷键_0", Resources.resourceCulture);
			}
		}

		internal static Bitmap 快捷键_1
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("快捷键_1", Resources.resourceCulture);
			}
		}

		internal static Bitmap 语音按钮
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("语音按钮", Resources.resourceCulture);
			}
		}

		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;
	}
}
