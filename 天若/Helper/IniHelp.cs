using System;
using System.IO;
using System.Text;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Helper;

public static class IniHelp
{
    private static readonly string config = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";

    public static string GetValue(string sectionName, string key)
    {
        if (!File.Exists(config))
            File.Create(config);
        byte[] array = new byte[2048];
        int privateProfileString = GetPrivateProfileString(sectionName, key, "发生错误", array, 999, config);
        return Encoding.Default.GetString(array, 0, privateProfileString);
    }

    public static bool SetValue(string sectionName, string key, string value)
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
