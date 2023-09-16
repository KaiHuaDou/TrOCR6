using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TrOCR
{
	public static class IniHelp
	{
		[DllImport("kernel32")]
		public static extern int GetPrivateProfileString(string sectionName, string key, string defaultValue, byte[] returnBuffer, int size, string filePath);

		[DllImport("kernel32")]
		public static extern long WritePrivateProfileString(string sectionName, string key, string value, string filePath);

		public static string GetValue(string sectionName, string key)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
			if (!File.Exists(text))
			{
				using (File.Create(text))
				{
				}
			}
			byte[] array = new byte[2048];
			int privateProfileString = IniHelp.GetPrivateProfileString(sectionName, key, "发生错误", array, 999, text);
			return Encoding.Default.GetString(array, 0, privateProfileString);
		}

		public static bool SetValue(string sectionName, string key, string value)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
			if (!File.Exists(text))
			{
				using (File.Create(text))
				{
				}
			}
			bool flag;
			try
			{
				flag = (int)IniHelp.WritePrivateProfileString(sectionName, key, value, text) > 0;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return flag;
		}

		public static bool RemoveSection(string sectionName, string filePath)
		{
			bool flag;
			try
			{
				flag = (int)IniHelp.WritePrivateProfileString(sectionName, null, "", filePath) > 0;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return flag;
		}

		public static bool Removekey(string sectionName, string key, string filePath)
		{
			bool flag;
			try
			{
				flag = (int)IniHelp.WritePrivateProfileString(sectionName, key, null, filePath) > 0;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return flag;
		}
	}
}
