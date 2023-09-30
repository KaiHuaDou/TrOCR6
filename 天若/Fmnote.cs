using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR;

public partial class FmNote : Form
{
    public FmNote( )
    {
        InitializeComponent( );
        base.Focus( );
        base.TopMost = true;
        base.ShowInTaskbar = false;
        base.Location = new Point(Screen.AllScreens[0].WorkingArea.Width - base.Width, Screen.AllScreens[0].WorkingArea.Height - base.Height);
    }

    private void Form1_Load(object o, EventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(FmMain));
        base.Icon = (Icon) componentResourceManager.GetObject("minico.Icon");
        mainDataGrid.ColumnCount = 1;
        mainDataGrid.RowCount = StaticValue.NoteCount;
        mainDataGrid.Columns[0].Width = Convert.ToInt32(400f * Helper.System.DpiFactor);
        mainDataGrid.CellBorderStyle = DataGridViewCellBorderStyle.None;
        mainDataGrid.AllowUserToResizeRows = false;
        mainDataGrid.AllowUserToResizeColumns = false;
        for (int i = 0; i < StaticValue.NoteCount; i++)
        {
            mainDataGrid.Rows[i].Cells[0].Value = i < 9
            ? string.Concat(new object[]
            {
                    "0",
                    i + 1,
                    ".",
                    StaticValue.Notes[i]
            })
            : (object) (i + 1 + "." + StaticValue.Notes[i]);
        }
        mainDataGrid.Columns[0].DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
        mainDataGrid.Size = new Size(Convert.ToInt32(402f * Helper.System.DpiFactor), StaticValue.NoteCount * mainDataGrid.Rows[0].Cells[0].Size.Height + 2);
        base.ClientSize = mainDataGrid.Size;
        base.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3);
        mainDataGrid.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3 - 5);
    }

    private void CopyClicked(object o, EventArgs e)
    {
        string text = "";
        int[] array = new int[mainDataGrid.SelectedRows.Count];
        int num = 0;
        foreach (object obj in mainDataGrid.SelectedRows)
        {
            DataGridViewRow dataGridViewRow = (DataGridViewRow) obj;
            array[num] = Convert.ToInt32(dataGridViewRow.Cells[0].Value.ToString( ).Substring(0, 2));
            num++;
        }
        int[] array2 = array;
        for (int i = 0; i < array2.Length - 1; i++)
        {
            for (int j = 0; j < array2.Length - 1 - i; j++)
            {
                if (array2[j] > array2[j + 1])
                {
                    (array2[j + 1], array2[j]) = (array2[j], array2[j + 1]);
                }
            }
        }
        for (int k = 0; k < array2.Length; k++)
        {
            if (k == array2.Length - 1)
            {
                text += mainDataGrid.Rows[array2[k] - 1].Cells[0].Value.ToString( ).Remove(0, 3);
            }
            else
            {
                text = text + mainDataGrid.Rows[array2[k] - 1].Cells[0].Value.ToString( ).Remove(0, 3) + "\r\n";
            }
        }
        Clipboard.SetDataObject(text);
    }

    public string TextNote
    {
        get => null;
        set
        {
            for (int i = 0; i < StaticValue.NoteCount; i++)
            {
                mainDataGrid.Rows[i].Cells[0].Value = i < 9
                    ? $"0{i + 1}." + StaticValue.Notes[i]
                    : $"{i + 1}.{StaticValue.Notes[i]}";
            }
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 274 && (int) m.WParam == 61536)
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

    private void DoubleClicked(object o, EventArgs e)
    {
        if (mainDataGrid.SelectedCells[0].Value.ToString( ).Remove(0, 3) != "")
        {
            Clipboard.SetDataObject(mainDataGrid.SelectedCells[0].Value.ToString( ).Remove(0, 3));
            FmFlags.Display("已复制");
        }
    }

    public string TextNoteChange
    {
        get => null;
        set
        {
            mainDataGrid.Rows.Clear( );
            mainDataGrid.ColumnCount = 1;
            mainDataGrid.RowCount = StaticValue.NoteCount;
            mainDataGrid.Columns[0].Width = Convert.ToInt32(400f * Helper.System.DpiFactor);
            mainDataGrid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            mainDataGrid.AllowUserToResizeRows = false;
            mainDataGrid.AllowUserToResizeColumns = false;
            for (int i = 0; i < StaticValue.NoteCount; i++)
            {
                mainDataGrid.Rows[i].Cells[0].Value = i < 9 ? "0" + (i + 1) + "." : (object) (i + 1 + ".");
            }
            mainDataGrid.Columns[0].DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            mainDataGrid.Size = new Size(Convert.ToInt32(402f * Helper.System.DpiFactor), StaticValue.NoteCount * mainDataGrid.Rows[0].Cells[0].Size.Height + 2);
            base.ClientSize = mainDataGrid.Size;
            base.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3);
            mainDataGrid.MaximumSize = new Size(base.Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3 - 5);
        }
    }
}
