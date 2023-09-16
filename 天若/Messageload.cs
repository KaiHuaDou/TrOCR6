using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR
{
	public partial class Messageload : Form
	{
		public Messageload()
		{
			this.InitializeComponent();
		}

		public void Form1_Load(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.OK;
		}

		public void InitializeComponent()
		{
			base.SuspendLayout();
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.FormBorderStyle = FormBorderStyle.None;
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = Color.White;
			base.ClientSize = new Size(0, 0);
			this.ForeColor = Color.Black;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "Form1";
			this.Text = "弹窗";
			base.Load += this.Form1_Load;
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
