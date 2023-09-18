namespace TrOCR
{
	public partial class FmScreenPaste : global::System.Windows.Forms.Form
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
			this.RightMenu = new global::System.Windows.Forms.ContextMenuStrip();
			this.关闭ToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new global::System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem6 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new global::System.Windows.Forms.ToolStripSeparator();
			this.TopmostMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.RightMenu.SuspendLayout();
			base.SuspendLayout();
			this.RightMenu.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.关闭ToolStripMenuItem, this.toolStripSeparator1, this.toolStripMenuItem6, this.toolStripMenuItem8, this.toolStripSeparator2, this.TopmostMenuItem });
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
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.Manual;
			base.TopMost = true;
			base.ShowInTaskbar = false;
			this.RightMenu.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.ContextMenuStrip RightMenu;

		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;

		private global::System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;

		private global::System.Windows.Forms.ToolStripMenuItem 关闭ToolStripMenuItem;

		private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

		private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator2;

		private global::System.Windows.Forms.ToolStripMenuItem TopmostMenuItem;
	}
}
