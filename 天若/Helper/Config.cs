﻿using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR.Helper;

public static class Config
{
    private static readonly string config = AppDomain.CurrentDomain.BaseDirectory + "Data\\config.ini";
    public static string Get(string sectionName, string keyName)
    {
        if (!File.Exists(config))
        {
            File.Create(config);
            return "_ERROR_";
        }
        StringBuilder buffer = new(2048);
        GetPrivateProfileString(sectionName, keyName, "_ERROR_", buffer, 255, config);
        return buffer.ToString( );
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

    public static bool Set(string sectionName, string key, bool value)
        => Set(sectionName, key, value ? "True" : "False");


    public static void Check( )
    {
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Data"))
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Data");
        if (!File.Exists(config))
        {
            File.Create(config);
            Set("配置", "接口", "搜狗");
            Set("配置", "开机自启", "True");
            Set("配置", "快速翻译", "True");
            Set("配置", "识别弹窗", "True");
            Set("配置", "窗体动画", "窗体");
            Set("配置", "记录数目", "20");
            Set("配置", "自动保存", "True");
            Set("配置", "截图位置", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            Set("配置", "翻译接口", "谷歌");
            Set("快捷键", "文字识别", "F4");
            Set("快捷键", "翻译文本", "F9");
            Set("快捷键", "记录界面", "请按下快捷键");
            Set("快捷键", "识别界面", "请按下快捷键");
            Set("密钥_百度", "secret_id", "YsZKG1wha34PlDOPYaIrIIKO");
            Set("密钥_百度", "secret_key", "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD");
            Set("代理", "代理类型", "系统代理");
            Set("代理", "服务器", "");
            Set("代理", "端口", "");
            Set("代理", "需要密码", "False");
            Set("代理", "服务器账号", "");
            Set("代理", "服务器密码", "");
            Set("更新", "检测更新", "True");
            Set("更新", "更新间隔", "True");
            Set("更新", "间隔时间", "24");
            Set("截图音效", "自动保存", "True");
            Set("截图音效", "音效路径", "Data\\screenshot.wav");
            Set("截图音效", "剪贴板", "False");
            Set("工具栏", "合并", "False");
            Set("工具栏", "分段", "False");
            Set("工具栏", "分栏", "False");
            Set("工具栏", "拆分", "False");
            Set("工具栏", "检查", "False");
            Set("工具栏", "翻译", "False");
            Set("工具栏", "顶置", "True");
            Set("取色器", "类型", "RGB");
        }
        if (Get("配置", "接口") == "_ERROR_")
            Set("配置", "接口", "搜狗");
        if (Get("配置", "开机自启") == "_ERROR_")
            Set("配置", "开机自启", "True");
        if (Get("配置", "快速翻译") == "_ERROR_")
            Set("配置", "快速翻译", "True");
        if (Get("配置", "识别弹窗") == "_ERROR_")
            Set("配置", "识别弹窗", "True");
        if (Get("配置", "窗体动画") == "_ERROR_")
            Set("配置", "窗体动画", "窗体");
        if (Get("配置", "记录数目") == "_ERROR_")
            Set("配置", "记录数目", "20");
        if (Get("配置", "自动保存") == "_ERROR_")
            Set("配置", "自动保存", "True");
        if (Get("配置", "翻译接口") == "_ERROR_")
            Set("配置", "翻译接口", "谷歌");
        if (Get("配置", "截图位置") == "_ERROR_")
            Set("配置", "截图位置", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        if (Get("快捷键", "文字识别") == "_ERROR_")
            Set("快捷键", "文字识别", "F4");
        if (Get("快捷键", "翻译文本") == "_ERROR_")
            Set("快捷键", "翻译文本", "F9");
        if (Get("快捷键", "记录界面") == "_ERROR_")
            Set("快捷键", "记录界面", "请按下快捷键");
        if (Get("快捷键", "识别界面") == "_ERROR_")
            Set("快捷键", "识别界面", "请按下快捷键");
        if (Get("密钥_百度", "secret_id") == "_ERROR_")
            Set("密钥_百度", "secret_id", "YsZKG1wha34PlDOPYaIrIIKO");
        if (Get("密钥_百度", "secret_key") == "_ERROR_")
            Set("密钥_百度", "secret_key", "HPRZtdOHrdnnETVsZM2Nx7vbDkMfxrkD");
        if (Get("代理", "代理类型") == "_ERROR_")
            Set("代理", "代理类型", "系统代理");
        if (Get("代理", "服务器") == "_ERROR_")
            Set("代理", "服务器", "");
        if (Get("代理", "端口") == "_ERROR_")
            Set("代理", "端口", "");
        if (Get("代理", "需要密码") == "_ERROR_")
            Set("代理", "需要密码", "False");
        if (Get("代理", "服务器账号") == "_ERROR_")
            Set("代理", "服务器账号", "");
        if (Get("代理", "服务器密码") == "_ERROR_")
            Set("代理", "服务器密码", "");
        if (Get("更新", "检测更新") == "_ERROR_")
            Set("更新", "检测更新", "True");
        if (Get("更新", "更新间隔") == "_ERROR_")
            Set("更新", "更新间隔", "True");
        if (Get("更新", "间隔时间") == "_ERROR_")
            Set("更新", "间隔时间", "24");
        if (Get("截图音效", "自动保存") == "_ERROR_")
            Set("截图音效", "自动保存", "True");
        if (Get("截图音效", "音效路径") == "_ERROR_")
            Set("截图音效", "音效路径", "Data\\screenshot.wav");
        if (Get("截图音效", "剪贴板") == "_ERROR_")
            Set("截图音效", "剪贴板", "False");
        if (Get("工具栏", "合并") == "_ERROR_")
            Set("工具栏", "合并", "False");
        if (Get("工具栏", "拆分") == "_ERROR_")
            Set("工具栏", "拆分", "False");
        if (Get("工具栏", "检查") == "_ERROR_")
            Set("工具栏", "检查", "False");
        if (Get("工具栏", "翻译") == "_ERROR_")
            Set("工具栏", "翻译", "False");
        if (Get("工具栏", "分段") == "_ERROR_")
            Set("工具栏", "分段", "False");
        if (Get("工具栏", "分栏") == "_ERROR_")
            Set("工具栏", "分栏", "False");
        if (Get("工具栏", "顶置") == "_ERROR_")
            Set("工具栏", "顶置", "True");
        if (Get("取色器", "类型") == "_ERROR_")
            Set("取色器", "类型", "RGB");
        if (Get("特殊", "ali_cookie") == "_ERROR_")
            Set("特殊", "ali_cookie", "cna=noXhE38FHGkCAXDve7YaZ8Tn; cnz=noXhE4/VhB8CAbZ773ApeV14; isg=BGJi2c2YTeeP6FG7S_Re8kveu-jEs2bNwToQnKz7jlWAfwL5lEO23eh9q3km9N5l; ");
        if (Get("特殊", "ali_account") == "_ERROR_")
            Set("特殊", "ali_account", "");
        if (Get("特殊", "ali_password") == "_ERROR_")
            Set("特殊", "ali_password", "");
    }

    public static void SetHotkey(string control, string key, string original, int flag)
    {
        string[] hotkeys = (original + "+").Split('+');
        if (hotkeys.Length == 3)
        {
            control = hotkeys[0];
            key = hotkeys[1];
        }
        else if (hotkeys.Length == 2)
        {
            control = "None";
            key = original;
        }
        hotkeys = new string[] { control, key };
        if (!RegisterHotKey(Globals.MainHandle, flag, (KeyModifiers) Enum.Parse(typeof(KeyModifiers), hotkeys[0].Trim( )), (Keys) Enum.Parse(typeof(Keys), hotkeys[1].Trim( ))))
            FmFlags.Display("快捷键冲突，请更换！");
        RegisterHotKey(Globals.MainHandle, flag, (KeyModifiers) Enum.Parse(typeof(KeyModifiers), hotkeys[0].Trim( )), (Keys) Enum.Parse(typeof(Keys), hotkeys[1].Trim( )));
    }
}
