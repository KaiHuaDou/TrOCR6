namespace TrOCR
{
	public partial class AliTable : global::System.Windows.Forms.Form
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
			this.components = new global::System.ComponentModel.Container();
			this.webBrowser1 = new global::System.Windows.Forms.WebBrowser();
			this.CookiesBox = new global::System.Windows.Forms.TextBox();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			base.SuspendLayout();
			this.webBrowser1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.webBrowser1.Location = new global::System.Drawing.Point(0, 0);
			this.webBrowser1.MinimumSize = new global::System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new global::System.Drawing.Size(399, 422);
			this.webBrowser1.TabIndex = 0;
			this.webBrowser1.ScriptErrorsSuppressed = true;
			this.webBrowser1.DocumentCompleted += new global::System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.BrowserLoaded);
			this.CookiesBox.Location = new global::System.Drawing.Point(121, 87);
			this.CookiesBox.Name = "textBox1";
			this.CookiesBox.Size = new global::System.Drawing.Size(100, 21);
			this.CookiesBox.TabIndex = 1;
			this.CookiesBox.TextChanged += new global::System.EventHandler(this.CookiesBoxTextChange);
			this.timer1.Tick += new global::System.EventHandler(this.TimerTick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(399, 422);
			base.Controls.Add(this.webBrowser1);
			base.Controls.Add(this.CookiesBox);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.None;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterScreen;
			base.Name = "AliTable";
			this.Text = "阿里表格";
			base.Load += new global::System.EventHandler(this.FormLoad);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.WebBrowser webBrowser1;

		private global::System.Windows.Forms.TextBox CookiesBox;

		private global::System.Windows.Forms.Timer timer1;
	}
}
