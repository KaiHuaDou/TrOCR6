using System.Windows.Forms;

namespace TrOCR
{
	public partial class FmScreenPaste : Form
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
			this.RightMenu = new ContextMenuStrip();
			this.关闭ToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.toolStripMenuItem6 = new ToolStripMenuItem();
			this.toolStripMenuItem8 = new ToolStripMenuItem();
			this.toolStripSeparator2 = new ToolStripSeparator();
			this.TopmostMenuItem = new ToolStripMenuItem();
			this.RightMenu.SuspendLayout();
			base.SuspendLayout();
			this.RightMenu.Items.AddRange(new ToolStripItem[] { this.关闭ToolStripMenuItem, this.toolStripSeparator1, this.toolStripMenuItem6, this.toolStripMenuItem8, this.toolStripSeparator2, this.TopmostMenuItem });
			this.RightMenu.Name = "dSkinContextMenuStrip2";
			this.RightMenu.ShowImageMargin = false;
			this.RightMenu.Size = new global::System.Drawing.Size(124, 126);
			this.RightMenu.Opening += new global::System.ComponentModel.CancelEventHandler(this.RightCMSOpening);
			this.关闭ToolStripMenuItem.Name = "关闭ToolStripMenuItem";
			this.关闭ToolStripMenuItem.Size = new global::System.Drawing.Size(123, 22);
			this.关闭ToolStripMenuItem.Text = "关闭";
			this.关闭ToolStripMenuItem.Click += new global::System.EventHandler(this.CloseMenuClick);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new global::System.Drawing.Size(120, 6);
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new global::System.Drawing.Size(123, 22);
			this.toolStripMenuItem6.Text = "复制图像";
			this.toolStripMenuItem6.Click += new global::System.EventHandler(this.CopyMenuClick);
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			this.toolStripMenuItem8.Size = new global::System.Drawing.Size(123, 22);
			this.toolStripMenuItem8.Text = "图像另存为";
			this.toolStripMenuItem8.Click += new global::System.EventHandler(this.SaveMenuClick);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new global::System.Drawing.Size(120, 6);
			this.TopmostMenuItem.Name = "置顶窗体ToolStripMenuItem";
			this.TopmostMenuItem.Size = new global::System.Drawing.Size(123, 22);
			this.TopmostMenuItem.Text = "置顶窗体";
			this.TopmostMenuItem.Click += new global::System.EventHandler(this.TopmostMenuClick);
			this.ContextMenuStrip = this.RightMenu;
			base.StartPosition = FormStartPosition.Manual;
			base.TopMost = true;
			base.ShowInTaskbar = false;
			this.RightMenu.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private ContextMenuStrip RightMenu;

		private ToolStripMenuItem toolStripMenuItem6;

		private ToolStripMenuItem toolStripMenuItem8;

		private ToolStripMenuItem 关闭ToolStripMenuItem;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripSeparator toolStripSeparator2;

		private ToolStripMenuItem TopmostMenuItem;
	}
}
