using System;
using System.IO;
using System.Text;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Helper;

public static class IniHelp
{
    public static string GetValue(string sectionName, string key, string filePath = "Data\\config.ini")
    {
        string file = AppDomain.CurrentDomain.BaseDirectory + filePath;
        if (!File.Exists(file))
        {
            File.Create(file);
        }
        byte[] array = new byte[2048];
        int privateProfileString = GetPrivateProfileString(sectionName, key, "发生错误", array, 999, file);
        return Encoding.Default.GetString(array, 0, privateProfileString);
    }

    public static bool SetValue(string sectionName, string key, string value)
    {
        string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
        if (!File.Exists(text))
        {
            File.Create(text);
        }
        bool flag;
        try
        {
            flag = (int) WritePrivateProfileString(sectionName, key, value, text) > 0;
        }
        catch { throw; }
        return flag;
    }
}
