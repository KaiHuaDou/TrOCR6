using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public partial class FmLoading : Form
{
    private int count;
    private Image backgrond;
    private int flag1;
    private Timer timer;
    private string windowType;

    public FmLoading( ) => InitializeComponent( );

    public string FormClose { get; set; }
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

    public void InitializeComponent( )
    {
        timer = new Timer( );
        FormClose = "窗体已开启";
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
        SetPng( );
        ResumeLayout(false);
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
        throw new ApplicationException("图片必须是32位带Alhpa通道的图片");
    }
    public void SetPng( )
    {
        string text;
        try
        {
            text = Config.Get("配置", "窗体动画");
        }
        catch
        {
            text = "窗体";
        }
        if (text == "少女")
        {
            timer.Interval = 50;
            flag1 = 27;
            windowType = "";
        }
        else if (text == "罗小黑")
        {
            timer.Interval = 18;
            flag1 = 46;
            windowType = "_luo";
        }
        else
        {
            timer.Interval = 80;
            flag1 = 4;
            windowType = "_load";
        }
        backgrond = null;
        count = 0;
        timer.Tick += TimerTick;
        timer.Start( );
    }

    private void TimerTick(object o, EventArgs e)
    {
        try
        {
            if (FormClose != "窗体已开启")
            {
                Close( );
            }
            if (count >= flag1)
            {
                count = 0;
            }
            backgrond = (Image) new ComponentResourceManager(typeof(FmLoading)).GetObject(count + windowType + ".png");
            SetBits((Bitmap) backgrond);
            count++;
        }
        catch
        {
            MessageBox.Show("加载窗体关闭报错");
        }
    }
}
