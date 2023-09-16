using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR
{
	public partial class ReplaceForm : Form
	{
		public ReplaceForm(AdvRichTextBox mm)
		{
			this.InitializeComponent();
			this.Fmok = mm;
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FmMain));
			base.Icon = (Icon)componentResourceManager.GetObject("minico.Icon");
			base.StartPosition = FormStartPosition.Manual;
		}

		private void Form2_Load(object sender, EventArgs e)
		{
		}

		private void findbutton_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.Fmok.richTextBox1.Text != "")
				{
					this.p = this.Fmok.richTextBox1.Text.IndexOf(this.findtextbox.Text, this.p);
					if (this.p != -1)
					{
						this.Fmok.richTextBox1.Select(this.p, this.findtextbox.Text.Length);
						this.p++;
					}
					else
					{
						MessageBox.Show("已查找到文档尾！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						this.p = 0;
					}
				}
			}
			catch
			{
				this.p = 0;
				MessageBox.Show("已查找到文档尾！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}

		private void replacebutton_Click(object sender, EventArgs e)
		{
			if (this.Fmok.richTextBox1.Text != "")
			{
				this.p = 0;
				this.p = this.Fmok.richTextBox1.Text.IndexOf(this.findtextbox.Text, this.p);
				if (this.p != -1)
				{
					this.Fmok.richTextBox1.Select(this.p, this.findtextbox.Text.Length);
					this.Fmok.richTextBox1.SelectedText = this.replacetextBox.Text;
					this.p++;
					return;
				}
				MessageBox.Show("已替换完！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				this.p = 0;
			}
		}

		private void replaceallbutton_Click(object sender, EventArgs e)
		{
			if (this.Fmok.richTextBox1.Text != "" && this.findtextbox.Text != "")
			{
				this.p = 0;
				this.p = this.Fmok.richTextBox1.Text.IndexOf(this.findtextbox.Text, this.p);
				while (this.p != -1)
				{
					this.Fmok.richTextBox1.Select(this.p, this.findtextbox.Text.Length);
					this.Fmok.richTextBox1.SelectedText = this.replacetextBox.Text;
					this.p = this.Fmok.richTextBox1.Text.IndexOf(this.findtextbox.Text, this.p);
					this.flag = true;
				}
				if (this.flag)
				{
					MessageBox.Show("替换完毕！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return;
				}
				if (MessageBox.Show("替换内容不存在，请重新输入！", "提醒") == DialogResult.OK)
				{
					this.findtextbox.Text = "";
				}
			}
		}

		private void canclebutton_Click(object sender, EventArgs e)
		{
			base.Hide();
			this.Fmok.Focus();
		}

		private void ReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			base.Hide();
			this.Fmok.Focus();
		}

		public AdvRichTextBox Fmok;

		private int p;

		private bool flag;
	}
}
