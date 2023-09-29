using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class FmFlags : Form
{
    public FmFlags( ) => InitializeComponent( );

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams createParams = base.CreateParams;
            createParams.Style |= 131072;
            if (!DesignMode)
            {
                createParams.ExStyle |= 524288;
            }
            return createParams;
        }
    }

    public void SetBits(Bitmap bitmap)
    {
        if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
        {
            throw new ApplicationException("图片必须是32位带Alhpa通道的图片。");
        }
        IntPtr intPtr = IntPtr.Zero;
        IntPtr dc = GetDC(IntPtr.Zero);
        IntPtr intPtr2 = IntPtr.Zero;
        IntPtr intPtr3 = CreateCompatibleDC(dc);
        try
        {
            Point point = new(Left, Top);
            Size size = new(bitmap.Width, bitmap.Height);
            BLENDFUNCTION blendfunction = default;
            Point point2 = new(0, 0);
            intPtr2 = bitmap.GetHbitmap(Color.FromArgb(0));
            intPtr = SelectObject(intPtr3, intPtr2);
            blendfunction.BlendOp = 0;
            blendfunction.SourceConstantAlpha = byte.MaxValue;
            blendfunction.AlphaFormat = 1;
            blendfunction.BlendFlags = 0;
            UpdateLayeredWindow(Handle, dc, ref point, ref size, intPtr3, ref point2, 0, ref blendfunction, 2);
        }
        finally
        {
            if (intPtr2 != IntPtr.Zero)
            {
                SelectObject(intPtr3, intPtr);
                DeleteObject(intPtr2);
            }
            ReleaseDC(IntPtr.Zero, dc);
            DeleteDC(intPtr3);
        }
    }

    public void DrawStr(string str)
    {
        FmWidth = 50 * str.Length;
        ClientSize = new Size(FmWidth, 50);
        Location = new Point((Screen.PrimaryScreen.Bounds.Width - Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2 / 3 * 5);
        image = new Bitmap(FmWidth, 50);
        g = Graphics.FromImage(image);
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.Transparent);
        g.FillRectangle(new SolidBrush(Color.FromArgb(1, 255, 255, 255)), ClientRectangle);
        StringFormat stringFormat = new( )
        {
            Alignment = StringAlignment.Center
        };
        Rectangle rectangle = new(0, 3, FmWidth, 50);
        g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 1, 1, FmWidth - 2, 48);
        g.DrawRectangle(new Pen(Color.FromArgb(224, 224, 224)), 2, 2, FmWidth - 2 - 2, 46);
        g.DrawString(str, new Font("微软雅黑", 24f / Program.DpiFactor), new SolidBrush(Color.FromArgb(255, Color.White)), rectangle, stringFormat);
        SetBits(image);
        g.Dispose( );
        image.Dispose( );
        Delay(600U);
        Hide( );
    }

    private static void Delay(uint ms)
    {
        uint tickCount = GetTickCount( );
        while (GetTickCount( ) - tickCount < ms)
        {
            Thread.Sleep(1);
            Application.DoEvents( );
        }
    }

    public void DrawStrUpdate(string str)
    {
        FmWidth = 28 * str.Length;
        ClientSize = new Size(FmWidth, 50);
        image = new Bitmap(FmWidth, 50);
        g = Graphics.FromImage(image);
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.Transparent);
        g.FillRectangle(new SolidBrush(Color.FromArgb(1, 255, 255, 255)), ClientRectangle);
        StringFormat stringFormat = new( )
        {
            Alignment = StringAlignment.Center
        };
        Rectangle rectangle = new(0, 10, FmWidth, 48);
        g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 1, 1, FmWidth - 2, 48);
        g.DrawRectangle(new Pen(Color.FromArgb(224, 224, 224)), 1, 1, FmWidth - 2, 48);
        g.DrawString(str, new Font("微软雅黑", 18f), new SolidBrush(Color.FromArgb(255, Color.White)), rectangle, stringFormat);
        SetBitsUpdate(image);
        Delay(2000U);
        Hide( );
    }

    public void SetBitsUpdate(Bitmap bitmap)
    {
        if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
        {
            throw new ApplicationException("图片必须是32位带Alhpa通道的图片。");
        }
        IntPtr intPtr = IntPtr.Zero;
        IntPtr dc = GetDC(IntPtr.Zero);
        IntPtr intPtr2 = IntPtr.Zero;
        IntPtr intPtr3 = CreateCompatibleDC(dc);
        try
        {
            Point point = new(Screen.PrimaryScreen.Bounds.Width - Width, Screen.PrimaryScreen.WorkingArea.Height - 50);
            Size size = new(bitmap.Width, bitmap.Height);
            BLENDFUNCTION blendfunction = default;
            Point point2 = new(0, 0);
            intPtr2 = bitmap.GetHbitmap(Color.FromArgb(0));
            intPtr = SelectObject(intPtr3, intPtr2);
            blendfunction.BlendOp = 0;
            blendfunction.SourceConstantAlpha = byte.MaxValue;
            blendfunction.AlphaFormat = 1;
            blendfunction.BlendFlags = 0;
            UpdateLayeredWindow(Handle, dc, ref point, ref size, intPtr3, ref point2, 0, ref blendfunction, 2);
        }
        finally
        {
            if (intPtr2 != IntPtr.Zero)
            {
                SelectObject(intPtr3, intPtr);
                DeleteObject(intPtr2);
            }
            ReleaseDC(IntPtr.Zero, dc);
            DeleteDC(intPtr3);
        }
    }

    private Bitmap image;

    private Graphics g;

    public int FmWidth;
}
