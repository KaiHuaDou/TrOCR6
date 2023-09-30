using System;

namespace TrOCR.Helper;
public static class Update
{
    public static void CheckUpdate( )
    {
        return; //TODO: CheckUpdate
    }

    public static bool CheckVersion(string version, string current)
    {
        string[] versionArr = version.Split(new char[] { '.' });
        string[] currArr = current.Split(new char[] { '.' });
        for (int i = 0; i < versionArr.Length; i++)
        {
            if (Convert.ToInt32(versionArr[i]) > Convert.ToInt32(currArr[i]))
                return true;
        }
        return false;
    }
}
