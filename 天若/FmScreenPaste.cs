using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrOCR;

public partial class FmScreenPaste : Form
{
    public FmScreenPaste(Image img, Point LocationPoint)
    {
        m_aeroEnabled = false;
        InitializeComponent( );
        BackgroundImage = img;
        base.Location = LocationPoint;
        base.FormBorderStyle = FormBorderStyle.None;
        base.MouseDown += Form1_MouseDown;
        base.MouseMove += Form1_MouseMove;
        base.MouseUp += Form1_MouseUp;
        Size size = img.Size;
        MaximumSize = MinimumSize = size;
        base.Size = size;
        base.MouseDoubleClick += 双击_MouseDoubleClick;
    }

    private void Form_MouseWheel(object sender, MouseEventArgs e)
    {
    }

    private void RightCMS_Opening(object sender, CancelEventArgs e)
    {
        if (base.TopMost)
        {
            置顶ToolStripMenuItem.Text = "取消置顶";
            return;
        }
        置顶ToolStripMenuItem.Text = "置顶窗体";
    }

    private void 置顶ToolStripMenuItem_Click(object sender, EventArgs e) => base.TopMost = !base.TopMost;

    private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
    {
        BackgroundImage.Dispose( );
        GC.Collect( );
        base.Close( );
    }

    private void 复制toolStripMenuItem_Click(object sender, EventArgs e) => Clipboard.SetImage(BackgroundImage);

    private void 保存toolStripMenuItem_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new( )
        {
            Filter = "jpg图片(*.jpg)|*.jpg|png图片(*.png)|*.jpg|bmp图片(*.bmp)|*.bmp",
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
            if (extension.Equals(".png", StringComparison.Ordinal))
            {
                BackgroundImage.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
            if (extension.Equals(".bmp", StringComparison.Ordinal))
            {
                BackgroundImage.Save(saveFileDialog.FileName, ImageFormat.Bmp);
            }
        }
    }

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture( );

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    private void Form_MouseDown(object sender, MouseEventArgs e)
    {
        int num = 274;
        int num2 = 61456;
        int num3 = 2;
        FmScreenPaste.ReleaseCapture( );
        FmScreenPaste.SendMessage(base.Handle, num, num2 + num3, 0);
    }

    [DllImport("Gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("dwmapi.dll")]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref FmScreenPaste.MARGINS pMarInset);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll")]
    public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

    private static bool CheckAeroEnabled( )
    {
        bool flag;
        if (Environment.OSVersion.Version.Major >= 6)
        {
            int num = 0;
            FmScreenPaste.DwmIsCompositionEnabled(ref num);
            flag = num == 1;
        }
        else
        {
            flag = false;
        }
        return flag;
    }

    protected override void WndProc(ref Message m)
    {
        int msg = m.Msg;
        if (m.Msg == 132 && (int) m.Result == 1)
        {
            m.Result = (IntPtr) 2;
        }
        if (msg == 133 && m_aeroEnabled)
        {
            int num = 2;
            FmScreenPaste.DwmSetWindowAttribute(base.Handle, 2, ref num, 4);
            FmScreenPaste.MARGINS margins = new( )
            {
                bottomHeight = 1,
                leftWidth = 1,
                rightWidth = 1,
                topHeight = 1
            };
            FmScreenPaste.DwmExtendFrameIntoClientArea(base.Handle, ref margins);
        }
        base.WndProc(ref m);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            m_aeroEnabled = CheckAeroEnabled( );
            CreateParams createParams = base.CreateParams;
            if (!m_aeroEnabled)
            {
                createParams.ClassStyle |= 131072;
            }
            return createParams;
        }
    }

    private void 双击_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        BackgroundImage.Dispose( );
        GC.Collect( );
        base.Close( );
    }

    public void AdjustSize( )
    {
        Size size = new(10, 25);
        MaximumSize = MinimumSize = size;
        base.Size = size;
    }

    private void Form1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            mouseOff = new Point(-e.X, -e.Y);
            leftFlag = true;
        }
    }

    private void Form1_MouseMove(object sender, MouseEventArgs e)
    {
        if (leftFlag)
        {
            Point mousePosition = Control.MousePosition;
            mousePosition.Offset(mouseOff.X, mouseOff.Y);
            base.Location = mousePosition;
        }
    }

    private void Form1_MouseUp(object sender, MouseEventArgs e)
    {
        if (leftFlag)
        {
            leftFlag = false;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        DoubleBuffered = true;
        if (BackgroundImage != null)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.DrawImage(BackgroundImage, new Rectangle(0, 0, base.Width, base.Height), 0, 0, BackgroundImage.Width, BackgroundImage.Height, GraphicsUnit.Pixel);
        }
        base.OnPaint(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    private readonly int zoomLevel;

    private readonly string ScreenshotLastSavePath;

    private bool m_aeroEnabled;

    private const int CS_DROPSHADOW = 131072;

    private const int WM_NCPAINT = 133;

    private const int WM_ACTIVATEAPP = 28;

    private const int WM_NCHITTEST = 132;

    private const int HTCLIENT = 1;

    private const int HTCAPTION = 2;

    private Point mouseOff;

    private bool leftFlag;

    public struct MARGINS
    {
        public int leftWidth;

        public int rightWidth;

        public int topHeight;

        public int bottomHeight;
    }
}
