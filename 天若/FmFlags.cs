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
    private string text;

    public FmFlags( ) => InitializeComponent( );

    // Override this to change the style of the form.
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams createParams = base.CreateParams;
            createParams.Style |= 131072;
            if (!DesignMode)
                createParams.ExStyle |= 524288;
            return createParams;
        }
    }

    public static void Display(string text)
        => new FmFlags( ) { text = text }.Show( );

    public new void Show( )
    {
        base.Show( );

        int fmWidth = 50 * text.Length;
        ClientSize = new Size(fmWidth, 50);
        Location = new Point((Screen.PrimaryScreen.Bounds.Width - Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2 / 3 * 5);
        Bitmap image = new(fmWidth, 50);

        Graphics graphics = Graphics.FromImage(image);
        graphics.InterpolationMode = InterpolationMode.Bilinear;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        graphics.Clear(Color.Transparent);
        graphics.FillRectangle(new SolidBrush(Color.FromArgb(1, 255, 255, 255)), ClientRectangle);
        graphics.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 1, 1, fmWidth - 2, 48);
        graphics.DrawRectangle(new Pen(Color.FromArgb(224, 224, 224)), 2, 2, fmWidth - 2 - 2, 46);
        graphics.DrawString(
            text,
            new Font("微软雅黑", 24f / Helper.System.DpiFactor),
            new SolidBrush(Color.FromArgb(255, Color.White)),
            new Rectangle(0, 3, fmWidth, 50),
            new StringFormat( ) { Alignment = StringAlignment.Center }
        );
        SetImage(image);

        graphics.Dispose( );
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

    private void SetImage(Bitmap bitmap)
    {
        if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
        {
            throw new BadImageFormatException("图片必须是32位色彩且带 Alpha 通道的图片");
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
}
