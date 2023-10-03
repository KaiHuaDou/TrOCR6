using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;
using TrOCR.Helper;
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

        Graphics g = Graphics.FromImage(image);
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.Transparent);
        g.FillRectangle(new SolidBrush(Color.FromArgb(1, 255, 255, 255)), ClientRectangle);
        g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 1, 1, fmWidth - 2, 48);
        g.DrawRectangle(new Pen(Color.FromArgb(224, 224, 224)), 2, 2, fmWidth - 2 - 2, 46);
        g.DrawString(
            text,
            new Font("微软雅黑", 24f / Helper.System.DpiFactor),
            new SolidBrush(Color.FromArgb(255, Color.White)),
            new Rectangle(0, 3, fmWidth, 50),
            new StringFormat( ) { Alignment = StringAlignment.Center }
        );
        ImageUtils.SetImage(image, Left, Top, Handle);

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
}
