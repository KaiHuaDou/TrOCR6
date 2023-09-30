using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class FmScreenPaste : Form
{
    private bool isAeroEnable;
    private bool leftFlag;
    private Point mouseOff;

    public FmScreenPaste(Image image, Point location)
    {
        isAeroEnable = false;
        InitializeComponent( );
        BackgroundImage = image;
        Location = location;
        FormBorderStyle = FormBorderStyle.None;
        MouseDown += FormMouseDownNew;
        MouseMove += FormMouseMove;
        MouseUp += FormMouseUp;
        Size size = image.Size;
        MaximumSize = MinimumSize = size;
        Size = size;
        MouseDoubleClick += FormDoubleClick;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            isAeroEnable = CheckAeroEnabled( );
            CreateParams createParams = base.CreateParams;
            if (!isAeroEnable)
            {
                createParams.ClassStyle |= 131072;
            }
            return createParams;
        }
    }

    public void AdjustSize( )
    {
        Size size = new(10, 25);
        MaximumSize = MinimumSize = size;
        Size = size;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        DoubleBuffered = true;
        if (BackgroundImage != null)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.DrawImage(BackgroundImage, new Rectangle(0, 0, Width, Height), 0, 0, BackgroundImage.Width, BackgroundImage.Height, GraphicsUnit.Pixel);
        }
        base.OnPaint(e);
    }

    protected override void WndProc(ref Message m)
    {
        int msg = m.Msg;
        if (m.Msg == 132 && (int) m.Result == 1)
        {
            m.Result = (IntPtr) 2;
        }
        if (msg == 133 && isAeroEnable)
        {
            int num = 2;
            DwmSetWindowAttribute(Handle, 2, ref num, 4);
            MARGINS margins = new( )
            {
                bottomHeight = 1,
                leftWidth = 1,
                rightWidth = 1,
                topHeight = 1
            };
            DwmExtendFrameIntoClientArea(Handle, ref margins);
        }
        base.WndProc(ref m);
    }

    private static bool CheckAeroEnabled( )
    {
        bool flag;
        if (Environment.OSVersion.Version.Major >= 6)
        {
            int num = 0;
            DwmIsCompositionEnabled(ref num);
            flag = num == 1;
        }
        else
        {
            flag = false;
        }
        return flag;
    }

    private void CloseMenuClick(object o, EventArgs e)
    {
        BackgroundImage.Dispose( );
        GC.Collect( );
        Close( );
    }

    private void CopyMenuClick(object o, EventArgs e)
        => Clipboard.SetImage(BackgroundImage);

    private void FormDoubleClick(object o, MouseEventArgs e)
    {
        BackgroundImage.Dispose( );
        GC.Collect( );
        Close( );
    }

    private void FormMouseDown(object o, MouseEventArgs e)
    {
        int num = 274;
        int num2 = 61456;
        int num3 = 2;
        ReleaseCapture( );
        SendMessage(Handle, num, num2 + num3, 0);
    }

    private void FormMouseDownNew(object o, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;
        mouseOff = new Point(-e.X, -e.Y);
        leftFlag = true;
    }

    private void FormMouseMove(object o, MouseEventArgs e)
    {
        if (!leftFlag)
            return;
        Point mousePosition = MousePosition;
        mousePosition.Offset(mouseOff.X, mouseOff.Y);
        Location = mousePosition;
    }

    private void FormMouseUp(object o, MouseEventArgs e)
    {
        leftFlag = false;
    }

    private void RightCMSOpening(object o, CancelEventArgs e)
    {
        if (TopMost)
        {
            TopmostMenuItem.Text = "取消置顶";
            return;
        }
        TopmostMenuItem.Text = "置顶窗体";
    }

    private void SaveMenuClick(object o, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new( )
        {
            Filter = "JPEG图像(*.jpg)|*.jpg|PNG图像(*.png)|*.jpg|位图图像(*.bmp)|*.bmp",
            AddExtension = false,
            FileName = string.Concat(new string[]
            {
                "tianruo_",
                DateTime.Now.Year.ToString(),
                "-",
                DateTime.Now.Month.ToString(),
                "-",
                DateTime.Now.Day.ToString(),
                "-",
                DateTime.Now.Ticks.ToString()
            }),
            Title = "保存图片",
            FilterIndex = 1,
            RestoreDirectory = true
        };
        if (saveFileDialog.ShowDialog( ) == DialogResult.OK)
        {
            string extension = Path.GetExtension(saveFileDialog.FileName);
            if (extension.Equals(".jpg", StringComparison.Ordinal))
            {
                BackgroundImage.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
            }
            else if (extension.Equals(".png", StringComparison.Ordinal))
            {
                BackgroundImage.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
            else if (extension.Equals(".bmp", StringComparison.Ordinal))
            {
                BackgroundImage.Save(saveFileDialog.FileName, ImageFormat.Bmp);
            }
        }
    }

    private void TopmostMenuClick(object o, EventArgs e)
        => TopMost = !TopMost;
}
