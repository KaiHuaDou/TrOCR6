namespace TrOCR.Controls;
public enum EditorFont
{
    宋体, 楷体, 黑体, 微软雅黑, 新罗马
}

public enum TextAlign
{
    Left = 1,
    Right = 2,
    Center = 3,
    Justify = 4
}

public enum OcrType
{
    None, Tencent, Youdao
}

public enum TranslateType
{
    ZhEn, ZhJp, ZhKo
}
public enum WindowType
{
    Main, Second
}
public enum MsgFlag : int
{
    FmMain = 786,
    ShowNote = 520,
    TransClose = 511,
    TransOpen = 512,
    Voice = 518,
    OcrImage = 580,
    Topmost = 725
}
