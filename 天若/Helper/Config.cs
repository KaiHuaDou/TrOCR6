using System;
using System.IO;
using System.Text;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Helper;

public static class Config
{
    private static readonly string config = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";

    public static string Get(string sectionName, string key)
    {
        if (!File.Exists(config))
            File.Create(config);
        byte[] array = new byte[2048];
        int dataLength = GetPrivateProfileString(sectionName, key, "__ERROR__", array, 999, config);
        return Encoding.Default.GetString(array, 0, dataLength);
    }

    public static bool Set(string sectionName, string key, string value)
    {
        if (!File.Exists(config))
            File.Create(config);
        try
        {
            return (int) WritePrivateProfileString(sectionName, key, value, config) > 0;
        }
        catch { throw; }
    }
}
