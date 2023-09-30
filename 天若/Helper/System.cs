using System;
using Microsoft.Win32;
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
}
