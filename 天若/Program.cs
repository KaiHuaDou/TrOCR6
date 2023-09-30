using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

internal static class Program
{
    public static bool createNew;
    public static EventWaitHandle ProgramStarted;
    public static System.Timers.Timer updateTimer;

    public static void CheckTimerElapsed(object o, ElapsedEventArgs e)
        => Update.CheckUpdate( );

    [STAThread]
    public static void Main(string[] args)
    {
        Config.Check( );
        updateTimer = new System.Timers.Timer( );
        Application.EnableVisualStyles( );
        Application.SetCompatibleTextRenderingDefault(false);
        Version OsVersion = Environment.OSVersion.Version;
        if (OsVersion.CompareTo(new Version("6.1")) >= 0)
        {
            SetProcessDPIAware( );
        }
        ProgramStart( );
        if (!createNew)
        {
            ProgramStarted.Set( );
            FmFlags.Display("软件已经运行");
            return;
        }
        if (args.Length != 0 && args[0] == "更新")
        {
            new FmSetting { SelectedTab = FmSettingTab.更新 }.ShowDialog( );
        }
        if (Config.Get("更新", "检测更新") is "True" or "__ERROR__")
        {
            new Thread(Update.CheckUpdate).Start( );
            if (Config.Get("更新", "更新间隔") == "True")
            {
                updateTimer.Enabled = true;
                updateTimer.Interval = 3600000.0 * Convert.ToInt32(Config.Get("更新", "间隔时间"));
                updateTimer.Elapsed += CheckTimerElapsed;
                updateTimer.Start( );
            }
        }
        else
        {
            FmFlags.Display("天若");
        }
        Application.Run(new FmMain( ));
    }

    public static void ProgramStart( )
                => ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "天若", out createNew);
}
