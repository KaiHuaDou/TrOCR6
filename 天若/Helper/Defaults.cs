using System;
using System.Drawing;
using TrOCR.Controls;

namespace TrOCR.Helper;

public static class Defaults
{
    public static string BaiduApiId = "";
    public static string BaiduApiKey = "";
    public static bool CaptureRejection;
    public static string CopyFormat = "无格式";
    public static string DateNow = "2023-10-01";
    public static float DpiFactor = 1f;
    public static string GoogleTransBack;
    public static string GoogleTransText;
    public static int ImageHeight;
    public static Image ImageOCR;
    public static int ImageWidth;
    public static IntPtr MainHandle;
    public static string Merge;
    public static string Note = "";
    public static int NoteCount = 40;
    public static string[] Notes;
    public static string Restore;
    public static Image ScreenImage;
    public static bool SetCapture;
    public static bool SetDefault = true;
    public static bool SetMerge;
    public static bool SetRecord;
    public static bool SetSpilt;
    public static bool SetTrans;
    public static string Split;
    public static bool Topmost;
    public static string UpdateText = "天若 OCR 更新";
    public static string Version = "6.0.0";
    public static TranslateType TransType = TranslateType.ZhEn;
}
