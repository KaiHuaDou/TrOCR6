using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class Fmloading : Form
{
    public Fmloading( ) => InitializeComponent( );

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
        if (Image.IsCanonicalPixelFormat(bitmap.PixelFormat) && Image.IsAlphaPixelFormat(bitmap.PixelFormat))
        {
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
                return;
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
        throw new ApplicationException("图片必须是32位带Alhpa通道的图片。");
    }

    public void InitializeComponent( )
    {
        timer = new Timer( );
        fm_close = "窗体已开启";
        SuspendLayout( );
        ShowInTaskbar = false;
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ForeColor = Color.Aqua;
        FormBorderStyle = FormBorderStyle.None;
        Name = "Form1";
        Text = "Form1";
        TopMost = true;
        ClientSize = new Size(120, 120);
        Location = (Point) new Size(500, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Set_png( );
        ResumeLayout(false);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        try
        {
            if (fm_close != "窗体已开启")
            {
                Close( );
            }
            if (i_c >= fla_1)
            {
                i_c = 0;
            }
            bgImg = (Image) new ComponentResourceManager(typeof(Fmloading)).GetObject(i_c + fla_2 + ".png");
            SetBits((Bitmap) bgImg);
            i_c++;
        }
        catch
        {
            MessageBox.Show("加载窗体关闭报错");
        }
    }

    public void Set_png( )
    {
        string text;
        try
        {
            text = IniHelp.GetValue("配置", "窗体动画");
        }
        catch
        {
            text = "窗体";
        }
        if (text == "少女")
        {
            timer.Interval = 50;
            fla_1 = 27;
            fla_2 = "";
        }
        else if (text == "罗小黑")
        {
            timer.Interval = 18;
            fla_1 = 46;
            fla_2 = "_luo";
        }
        else
        {
            timer.Interval = 80;
            fla_1 = 4;
            fla_2 = "_load";
        }
        bgImg = null;
        i_c = 0;
        timer.Tick += timer1_Tick;
        timer.Start( );
    }

    public string fml_close
    {
        get => fm_close;
        set => fm_close = value;
    }

    public int i_c;

    private Image bgImg;

    public Timer timer;

    public int fla_1;

    public string fla_2;

    public string str;

    public string fm_close;
}
