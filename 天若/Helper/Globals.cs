using System;
using System.Drawing;
using TrOCR.Controls;

namespace TrOCR.Helper;

public static class Globals
{
    public static string BaiduApiId = "";
    public static string BaiduApiKey = "";
    public static bool CaptureRejection;
    public static float DpiFactor = 1f;
    public static string GoogleTransText;
    public static Image ImageOCR;
    public static IntPtr MainHandle;
    public static string Note = "";
    public static int NoteCount = 40;
    public static string[] Notes;
    public static string SplitedText;
    public static bool Topmost;
    public static TranslateType TransType = TranslateType.ZhEn;
    public static string UpdateText = "天若 OCR 更新";
    public static string UpdateTime = "2023-10-05";
    public static string UpdateVersion = "6.0.0";
}
