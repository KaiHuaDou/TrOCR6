namespace TrOCR
{
	public partial class FmFlags : global::System.Windows.Forms.Form
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
			base.SuspendLayout();
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.None;
			this.ForeColor = global::System.Drawing.Color.Aqua;
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.None;
			base.ClientSize = new global::System.Drawing.Size(50, 50);
			base.Name = "Form1";
			this.Text = "Form1";
			base.TopMost = true;
			base.ShowInTaskbar = false;
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.Manual;
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;
	}
}
