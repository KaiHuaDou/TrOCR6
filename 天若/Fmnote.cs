using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR
{
	public partial class Fmnote : Form
	{
		public Fmnote()
		{
			this.InitializeComponent();
			base.Focus();
			base.TopMost = true;
			base.ShowInTaskbar = false;
			base.Location = new Point(Screen.AllScreens[0].WorkingArea.Width - base.Width, Screen.AllScreens[0].WorkingArea.Height - base.Height);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FmMain));
			base.Icon = (Icon)componentResourceManager.GetObject("minico.Icon");
			this.dataGridView1.ColumnCount = 1;
			this.dataGridView1.RowCount = StaticValue.v_notecount;
			this.dataGridView1.Columns[0].Width = Convert.ToInt32(400f * Program.factor);
			this.dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.AllowUserToResizeColumns = false;
			for (int i = 0; i < StaticValue.v_notecount; i++)
			{
				if (i < 9)
				{
					this.dataGridView1.Rows[i].Cells[0].Value = string.Concat(new object[]
					{
						"0",
						i + 1,
						".",
						StaticValue.v_note[i]
					});
				}
				else
				{
					this.dataGridView1.Rows[i].Cells[0].Value = i + 1 + "." + StaticValue.v_note[i];
				}
			}
			this.dataGridView1.Columns[0].DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
			this.dataGridView1.Size = new Size(Convert.ToInt32(402f * Program.factor), StaticValue.v_notecount * this.dataGridView1.Rows[0].Cells[0].Size.Height + 2);
			base.ClientSize = this.dataGridView1.Size;
			base.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3);
			this.dataGridView1.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3 - 5);
		}

		private void copy_click(object sender, EventArgs e)
		{
			string text = "";
			int[] array = new int[this.dataGridView1.SelectedRows.Count];
			int num = 0;
			foreach (object obj in this.dataGridView1.SelectedRows)
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				array[num] = Convert.ToInt32(dataGridViewRow.Cells[0].Value.ToString().Substring(0, 2));
				num++;
			}
			int[] array2 = array;
			for (int i = 0; i < array2.Length - 1; i++)
			{
				for (int j = 0; j < array2.Length - 1 - i; j++)
				{
					if (array2[j] > array2[j + 1])
					{
						int num2 = array2[j];
						array2[j] = array2[j + 1];
						array2[j + 1] = num2;
					}
				}
			}
			for (int k = 0; k < array2.Length; k++)
			{
				if (k == array2.Length - 1)
				{
					text += this.dataGridView1.Rows[array2[k] - 1].Cells[0].Value.ToString().Remove(0, 3);
				}
				else
				{
					text = text + this.dataGridView1.Rows[array2[k] - 1].Cells[0].Value.ToString().Remove(0, 3) + "\r\n";
				}
			}
			Clipboard.SetDataObject(text);
		}

		private void CopyItem_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public string Text_note
		{
			get
			{
				return null;
			}
			set
			{
				for (int i = 0; i < StaticValue.v_notecount; i++)
				{
					if (i < 9)
					{
						this.dataGridView1.Rows[i].Cells[0].Value = string.Concat(new object[]
						{
							"0",
							i + 1,
							".",
							StaticValue.v_note[i]
						});
					}
					else
					{
						this.dataGridView1.Rows[i].Cells[0].Value = i + 1 + "." + StaticValue.v_note[i];
					}
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 274 && (int)m.WParam == 61536)
			{
				base.Visible = false;
				return;
			}
			if (m.Msg == 163)
			{
				base.Location = new Point(Screen.AllScreens[0].WorkingArea.Width - base.Width, Screen.AllScreens[0].WorkingArea.Height - base.Height);
			}
			base.WndProc(ref m);
		}

		private void doub_click(object sender, EventArgs e)
		{
			if (this.dataGridView1.SelectedCells[0].Value.ToString().Remove(0, 3) != "")
			{
				Clipboard.SetDataObject(this.dataGridView1.SelectedCells[0].Value.ToString().Remove(0, 3));
				FmFlags fmFlags = new FmFlags();
				fmFlags.Show();
				fmFlags.DrawStr("已复制");
			}
		}

		public string Text_note_change
		{
			get
			{
				return null;
			}
			set
			{
				this.dataGridView1.Rows.Clear();
				this.dataGridView1.ColumnCount = 1;
				this.dataGridView1.RowCount = StaticValue.v_notecount;
				this.dataGridView1.Columns[0].Width = Convert.ToInt32(400f * Program.factor);
				this.dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
				this.dataGridView1.AllowUserToResizeRows = false;
				this.dataGridView1.AllowUserToResizeColumns = false;
				for (int i = 0; i < StaticValue.v_notecount; i++)
				{
					if (i < 9)
					{
						this.dataGridView1.Rows[i].Cells[0].Value = "0" + (i + 1) + ".";
					}
					else
					{
						this.dataGridView1.Rows[i].Cells[0].Value = i + 1 + ".";
					}
				}
				this.dataGridView1.Columns[0].DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
				this.dataGridView1.Size = new Size(Convert.ToInt32(402f * Program.factor), StaticValue.v_notecount * this.dataGridView1.Rows[0].Cells[0].Size.Height + 2);
				base.ClientSize = this.dataGridView1.Size;
				base.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3);
				this.dataGridView1.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3 - 5);
			}
		}
	}
}
