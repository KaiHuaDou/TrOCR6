using System;
using System.Collections.Generic;

namespace TrOCR.Ocr;

[Serializable]
public class TransObj
{
    public string From { get; set; }
    public string To { get; set; }
    public List<TransResult> Data { get; set; }
}
