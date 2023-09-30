using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR;

public partial class MessageLoad : Form
{
    public MessageLoad( ) => InitializeComponent( );

    public void Form1Load(object sender, EventArgs e) => base.DialogResult = DialogResult.OK;

    public void InitializeComponent( )
    {
        base.SuspendLayout( );
        base.AutoScaleDimensions = new SizeF(6f, 12f);
        base.FormBorderStyle = FormBorderStyle.None;
        base.AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        base.ClientSize = new Size(0, 0);
        ForeColor = Color.Black;
        base.MaximizeBox = false;
        base.MinimizeBox = false;
        base.Name = "Form1";
        Text = "弹窗";
        base.Load += Form1Load;
        base.ResumeLayout(false);
        base.PerformLayout( );
    }
}
