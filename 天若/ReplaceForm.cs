using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TrOCR.Controls;

namespace TrOCR;

public partial class ReplaceForm : Form
{
    public AdvRichTextBox FmOK;
    private bool flag;
    private int p;

    public ReplaceForm(AdvRichTextBox mm)
    {
        InitializeComponent( );
        FmOK = mm;
        ComponentResourceManager componentResourceManager = new(typeof(FmMain));
        Icon = (Icon) componentResourceManager.GetObject("minico.Icon");
        StartPosition = FormStartPosition.Manual;
    }

    private void CancelClick(object sender, EventArgs e)
    {
        Hide( );
        FmOK.Focus( );
    }

    private void FindButtonClick(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(FmOK.EditBox.Text))
            {
                p = FmOK.EditBox.Text.IndexOf(FindTextBox.Text, p);
                if (p != -1)
                {
                    FmOK.EditBox.Select(p, FindTextBox.Text.Length);
                    p++;
                }
                else
                {
                    MessageBox.Show("已查找到文档尾！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    p = 0;
                }
            }
        }
        catch
        {
            p = 0;
            MessageBox.Show("已查找到文档尾！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }

    private void Form2_Load(object sender, EventArgs e)
    {
    }
    private void ReplaceAllClick(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(FmOK.EditBox.Text) && !string.IsNullOrEmpty(FindTextBox.Text))
        {
            p = 0;
            p = FmOK.EditBox.Text.IndexOf(FindTextBox.Text, p);
            while (p != -1)
            {
                FmOK.EditBox.Select(p, FindTextBox.Text.Length);
                FmOK.EditBox.SelectedText = replacetextBox.Text;
                p = FmOK.EditBox.Text.IndexOf(FindTextBox.Text, p);
                flag = true;
            }
            if (flag)
            {
                MessageBox.Show("替换完毕！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (MessageBox.Show("替换内容不存在，请重新输入！", "提醒") == DialogResult.OK)
            {
                FindTextBox.Text = "";
            }
        }
    }

    private void ReplaceButtonClick(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(FmOK.EditBox.Text))
        {
            p = 0;
            p = FmOK.EditBox.Text.IndexOf(FindTextBox.Text, p);
            if (p != -1)
            {
                FmOK.EditBox.Select(p, FindTextBox.Text.Length);
                FmOK.EditBox.SelectedText = replacetextBox.Text;
                p++;
                return;
            }
            MessageBox.Show("已替换完！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            p = 0;
        }
    }
    private void ReplaceFormClosing(object sender, FormClosingEventArgs e)
    {
        Hide( );
        FmOK.Focus( );
    }
}
