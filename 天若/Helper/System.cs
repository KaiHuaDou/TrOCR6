using System;
using System.IO;
using Microsoft.Win32;
using static TrOCR.External.NativeMethods;
namespace TrOCR.Helper;
public static class System
{
    static System( )
    {
        try
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\WindowMetrics", true);
            string dpi = registryKey.GetValue("AppliedDPI").ToString( );
            registryKey.Close( );
            DpiFactor = Convert.ToSingle(dpi) / 96f;
        }
        catch
        {
            DpiFactor = 1f;
        }
    }

    public static float DpiFactor { get; set; }

    public static void DeleteFile(string path)
    {
        if (File.GetAttributes(path) == FileAttributes.Directory)
        {
            Directory.Delete(path, true);
            return;
        }
        File.Delete(path);
    }

    public static void PlaySong(string file, IntPtr handle)
    {
        mciSendString("close media", null, 0, IntPtr.Zero);
        mciSendString("open \"" + file + "\" type mpegvideo alias media", null, 0, IntPtr.Zero);
        mciSendString("play media notify", null, 0, handle);
    }
}
