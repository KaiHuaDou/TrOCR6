namespace TrOCR
{
	public partial class Fmnote : global::System.Windows.Forms.Form
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
			base.Location = new global::System.Drawing.Point(global::System.Windows.Forms.Screen.AllScreens[0].WorkingArea.Width - base.Width, global::System.Windows.Forms.Screen.AllScreens[0].WorkingArea.Height - base.Height);
			base.Hide();
			this.Font = new global::System.Drawing.Font(this.Font.Name, 9f / global::TrOCR.StaticValue.Dpifactor, this.Font.Style, this.Font.Unit, this.Font.GdiCharSet, this.Font.GdiVerticalFont);
			this.copyItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.components = new global::System.ComponentModel.Container();
			this.mainDataGrid = new global::System.Windows.Forms.DataGridView();
			this.contextMenuStrip1 = new global::System.Windows.Forms.ContextMenuStrip(this.components);
			this.contextMenuStrip1.Renderer = new global::TrOCR.MenuItemRendererT();
			((global::System.ComponentModel.ISupportInitialize)this.mainDataGrid).BeginInit();
			base.SuspendLayout();
			this.mainDataGrid.BackgroundColor = global::System.Drawing.Color.White;
			this.mainDataGrid.ColumnHeadersVisible = false;
			this.mainDataGrid.GridColor = global::System.Drawing.Color.White;
			this.mainDataGrid.Location = new global::System.Drawing.Point(0, 0);
			this.mainDataGrid.Name = "dataGridView1";
			this.mainDataGrid.RowHeadersVisible = false;
			this.mainDataGrid.TabIndex = 0;
			this.mainDataGrid.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.mainDataGrid.ContextMenuStrip = this.contextMenuStrip1;
			this.mainDataGrid.EditMode = global::System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.mainDataGrid.DoubleClick += new global::System.EventHandler(this.DoubleClicked);
			this.copyItem.Text = "复制";
			this.copyItem.Click += new global::System.EventHandler(this.CopyClicked);
			this.contextMenuStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[] { this.copyItem });
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new global::System.Drawing.Size(61, 4);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.mainDataGrid);
			base.Name = "Form1";
			base.MinimizeBox = false;
			base.MaximizeBox = false;
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Text = "记录";
			base.Load += new global::System.EventHandler(this.Form1_Load);
			((global::System.ComponentModel.ISupportInitialize)this.mainDataGrid).EndInit();
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.ToolStripMenuItem copyItem;

		private global::System.Windows.Forms.DataGridView mainDataGrid;

		private global::System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
	}
}
