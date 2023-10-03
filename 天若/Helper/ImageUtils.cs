using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Helper;
public static class ImageUtils
{
    public static Image BoundingBox(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        Image result;
        using (VectorOfVectorOfPoint vectorZ = new( ))
        {
            CvInvoke.FindContours(src, vectorZ, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
            Image image = draw.ToBitmap( );
            Graphics g = Graphics.FromImage(image);
            int size = vectorZ.Size;
            for (int i = 0; i < size; i++)
            {
                using VectorOfPoint vectorY = vectorZ[i];
                Rectangle rect = CvInvoke.BoundingRectangle(vectorY);
                int x = rect.Location.X;
                int y = rect.Location.Y;
                int width = rect.Size.Width;
                int height = rect.Size.Height;
                if (width > 5 || height > 5)
                    g.FillRectangle(Brushes.White, x, 0, width, image.Size.Height);
            }
            g.Dispose( );
            Bitmap bitmap = new(image.Width + 2, image.Height + 2);
            Graphics drawer = Graphics.FromImage(bitmap);
            drawer.DrawImage(image, 1, 1, image.Width, image.Height);
            drawer.Save( );
            drawer.Dispose( );
            result = bitmap;
        }
        return result;
    }

    public static Image BoundingBoxFences(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        Image result;
        using (VectorOfVectorOfPoint vectorZ = new( ))
        {
            CvInvoke.FindContours(src, vectorZ, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
            Image image = draw.ToBitmap( );
            Graphics graphics = Graphics.FromImage(image);
            int size = vectorZ.Size;
            for (int i = 0; i < size; i++)
            {
                using VectorOfPoint vectorY = vectorZ[i];
                Rectangle rect = CvInvoke.BoundingRectangle(vectorY);
                int x = rect.Location.X;
                int y = rect.Location.Y;
                int width = rect.Size.Width;
                int height = rect.Size.Height;
                graphics.FillRectangle(Brushes.White, x, 0, width, draw.Height);
            }
            graphics.Dispose( );
            Bitmap bitmap = new(image.Width + 2, image.Height + 2);
            Graphics drawer = Graphics.FromImage(bitmap);
            drawer.DrawImage(image, 1, 1, image.Width, image.Height);
            drawer.Save( );
            drawer.Dispose( );
            image.Dispose( );
            src.Dispose( );
            result = bitmap;
        }
        return result;
    }

    public static Rectangle[] BoundingBoxFencesUp(Image<Gray, byte> src)
    {
        using VectorOfVectorOfPoint vectorZ = new( );
        CvInvoke.FindContours(src, vectorZ, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, default);
        int size = vectorZ.Size;
        Rectangle[] array = new Rectangle[size];
        for (int i = 0; i < size; i++)
        {
            using VectorOfPoint vectorY = vectorZ[i];
            array[size - 1 - i] = CvInvoke.BoundingRectangle(vectorY);
        }
        return array;
    }

    public static Image FindBundingBox(Bitmap bitmap)
    {
        Image<Bgr, byte> bgr = new(bitmap);
        Image<Gray, byte> gray = new(bgr.Width, bgr.Height);
        CvInvoke.CvtColor(bgr, gray, ColorConversion.Bgra2Gray, 0);
        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(1, 1));
        CvInvoke.Erode(gray, gray, structuringElement, new Point(0, 2), 1, BorderType.Reflect101, default);
        CvInvoke.Threshold(gray, gray, 100.0, 255.0, (ThresholdType) 9);
        Image<Gray, byte> gray2 = new(gray.ToBitmap( ));
        Image<Bgr, byte> bgr2 = gray2.Convert<Bgr, byte>( );
        Image<Gray, byte> grayClone = gray2.Clone( );
        CvInvoke.Canny(gray2, grayClone, 255.0, 255.0, 5, true);
        return BoundingBox(grayClone, bgr2);
    }

    public static Bitmap GetRect(Image image, Rectangle rectangle)
    {
        Rectangle rect = new(0, 0, rectangle.Width, rectangle.Height);
        Bitmap b = new(rect.Width, rect.Height);
        Graphics g = Graphics.FromImage(b);
        g.Clear(Color.FromArgb(0, 0, 0, 0));
        g.DrawImage(image, rect, rectangle, GraphicsUnit.Pixel);
        g.Dispose( );
        return b;
    }

    public static Bitmap[] GetSubImage(Image image, Rectangle[] rects)
    {
        Bitmap[] array = new Bitmap[rects.Length];
        for (int i = 0; i < rects.Length; i++)
        {
            array[i] = GetRect(image, rects[i]);
            string text = Config.Get("配置", "截图位置") + "\\" + TextUtils.RenameFile(Config.Get("配置", "截图位置"), "图片.Png");
            array[i].Save(text, ImageFormat.Png);
        }
        return array;
    }

    public static byte[] ImageToByte(Image img)
    {
        byte[] result;
        try
        {
            MemoryStream stream = new( );
            img.Save(stream, ImageFormat.Jpeg);
            byte[] array = new byte[stream.Length];
            stream.Position = 0L;
            stream.Read(array, 0, (int) stream.Length);
            stream.Close( );
            result = array;
        }
        catch
        {
            result = null;
        }
        return result;
    }

    public static byte[] ImageToByteArray(Image img)
            => (byte[]) new ImageConverter( ).ConvertTo(img, typeof(byte[]));

    public static byte[] MergeBytes(byte[] a, byte[] b, byte[] c)
    {
        byte[] array = new byte[a.Length + b.Length + c.Length];
        a.CopyTo(array, 0);
        b.CopyTo(array, a.Length);
        c.CopyTo(array, a.Length + b.Length);
        return array;
    }

    public static void SetImage(Bitmap bitmap, int left, int top, IntPtr handle)
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
            Point point = new(left, top);
            Size size = new(bitmap.Width, bitmap.Height);
            BLENDFUNCTION blendfunction = default;
            Point point2 = new(0, 0);
            intPtr2 = bitmap.GetHbitmap(Color.FromArgb(0));
            intPtr = SelectObject(intPtr3, intPtr2);
            blendfunction.BlendOp = 0;
            blendfunction.SourceConstantAlpha = byte.MaxValue;
            blendfunction.AlphaFormat = 1;
            blendfunction.BlendFlags = 0;
            UpdateLayeredWindow(handle, dc, ref point, ref size, intPtr3, ref point2, 0, ref blendfunction, 2);
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

    public static Bitmap ToGray(Bitmap image)
    {
        Bitmap bitmap = new(image.Width, image.Height);
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                Color pixel = image.GetPixel(i, j);
                int num = (pixel.R + pixel.G + pixel.B) / 3;
                bitmap.SetPixel(i, j, Color.FromArgb(num, num, num));
            }
        }
        return bitmap;
    }

    public static Bitmap ZoomImage(Bitmap bitmap, int destHeight, int destWidth)
    {
        double _width = bitmap.Width;
        double _height = bitmap.Height;
        if (_width < destHeight)
        {
            while (_width < destHeight)
            {
                _height *= 1.1;
                _width *= 1.1;
            }
        }
        if (_height < destWidth)
        {
            while (_height < destWidth)
            {
                _height *= 1.1;
                _width *= 1.1;
            }
        }
        int width = (int) _width;
        int height = (int) _height;
        Bitmap image = new(width, height);
        Graphics g = Graphics.FromImage(image);
        g.DrawImage(bitmap, 0, 0, width, height);
        g.Save( );
        g.Dispose( );
        return new Bitmap(image);
    }
}
