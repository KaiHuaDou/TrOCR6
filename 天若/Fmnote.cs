using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TrOCR.Helper;

namespace TrOCR;

public partial class FmNote : Form
{
    public FmNote( )
    {
        InitializeComponent( );
        Focus( );
        TopMost = true;
        ShowInTaskbar = false;
        Location = new Point(Screen.AllScreens[0].WorkingArea.Width - Width, Screen.AllScreens[0].WorkingArea.Height - Height);
    }

    public void SetTextNote( )
    {
        for (int i = 0; i < Globals.NoteCount; i++)
        {
            mainDataGrid.Rows[i].Cells[0].Value = i < 9
                ? $"0{i + 1}." + Globals.Notes[i]
                : $"{i + 1}.{Globals.Notes[i]}";
        }
    }

    public void TextNoteChange( )
    {
        mainDataGrid.Rows.Clear( );
        mainDataGrid.ColumnCount = 1;
        mainDataGrid.RowCount = Globals.NoteCount;
        mainDataGrid.Columns[0].Width = Convert.ToInt32(400f * Helper.System.DpiFactor);
        mainDataGrid.CellBorderStyle = DataGridViewCellBorderStyle.None;
        mainDataGrid.AllowUserToResizeRows = false;
        mainDataGrid.AllowUserToResizeColumns = false;
        for (int i = 0; i < Globals.NoteCount; i++)
        {
            mainDataGrid.Rows[i].Cells[0].Value = i < 9 ? $"0{i + 1}." : $"{i + 1}.";
        }
        mainDataGrid.Columns[0].DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
        mainDataGrid.Size = new Size(Convert.ToInt32(402f * Helper.System.DpiFactor), Globals.NoteCount * mainDataGrid.Rows[0].Cells[0].Size.Height + 2);
        ClientSize = mainDataGrid.Size;
        base.MaximumSize = new Size(Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3);
        mainDataGrid.MaximumSize = new Size(Size.Width, Screen.GetWorkingArea(this).Height / 4 * 3 - 5);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 274 && (int) m.WParam == 61536)
        {
            Visible = false;
            return;
        }
        if (m.Msg == 163)
        {
            Location = new Point(Screen.AllScreens[0].WorkingArea.Width - Width, Screen.AllScreens[0].WorkingArea.Height - Height);
        }
        base.WndProc(ref m);
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

    private void DoubleClicked(object o, EventArgs e)
    {
        if (string.IsNullOrEmpty(mainDataGrid.SelectedCells[0].Value.ToString( ).Remove(0, 3)))
            return;
        Clipboard.SetDataObject(mainDataGrid.SelectedCells[0].Value.ToString( ).Remove(0, 3));
        FmFlags.Display("已复制");
    }

    private void FormLoaded(object o, EventArgs e)
    {
        ComponentResourceManager componentResourceManager = new(typeof(FmMain));
        Icon = (Icon) componentResourceManager.GetObject("minico.Icon");
        TextNoteChange( );
    }
}
