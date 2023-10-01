# Deprecate Code

## FmMain.cs

### OcrBaiduB

Deprecate Reason: Same as method `OcrBaiduA`

```csharp
    private string ocrBaiduB;

    private void DoWork(object state)
    {
        ...
        verticalRightText = string.Concat(new string[] { ocrBaiduA, ocrBaiduB, ocrBaiduC, ocrBaiduD, ocrBaiduE }).Replace("\r\n\r\n", "");
        ...
    }

    private void OcrBaiduB(Image images)
    {
        try
        {
            string text = "CHN_ENG";
            MemoryStream stream = new( );
            images.Save(stream, ImageFormat.Jpeg);
            byte[] array = new byte[stream.Length];
            stream.Position = 0L;
            stream.Read(array, 0, (int) stream.Length);
            stream.Close( );
            string text2 = "type=general_location&image=data" + HttpUtility.UrlEncode(":image/jpeg;base64," + Convert.ToBase64String(array)) + "&language_type=" + text;
            byte[] bytes = Encoding.UTF8.GetBytes(text2);
            HttpWebRequest req0 = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/tech/ocr/general");
            req0.CookieContainer = new CookieContainer( );
            req0.GetResponse( ).Close( );
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create("http://ai.baidu.com/aidemo");
            req.Method = "POST";
            req.Referer = "http://ai.baidu.com/tech/ocr/general";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Timeout = 8000;
            req.ReadWriteTimeout = 5000;
            req.Headers.Add("Cookie:" + Web.CookieToStr(((HttpWebResponse) req0.GetResponse( )).Cookies));
            using (Stream reqSteam = req.GetRequestStream( ))
            {
                reqSteam.Write(bytes, 0, bytes.Length);
            }
            Stream resStream = ((HttpWebResponse) req.GetResponse( )).GetResponseStream( );
            string text3 = new StreamReader(resStream, Encoding.GetEncoding("utf-8")).ReadToEnd( );
            resStream.Close( );
            JArray jarray = JArray.Parse(((JObject) JsonConvert.DeserializeObject(text3))["data"]["words_result"].ToString( ));
            string result = "";
            string[] results = new string[jarray.Count];
            for (int i = 0; i < jarray.Count; i++)
            {
                JObject jobject = JObject.Parse(jarray[i].ToString( ));
                result += jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
                results[jarray.Count - 1 - i] = jobject["words"].ToString( ).Replace("\r", "").Replace("\n", "");
            }
            //string text5 = "";
            //for (int j = 0; j < array2.Length; j++)
            //{
            //    text5 += array2[j];
            //}
            ocrBaiduB = (ocrBaiduB + result + "\r\n").Replace("\r\n\r\n", "");
            Thread.Sleep(10);
        }
        catch
        {
        }
    }

    private void SelectImage(Image<Gray, byte> src, Image<Bgr, byte> draw)
    {
        ...
        {ocrBaiduA = "";}
        ocrBaiduB = "";
        {ocrBaiduC = "";}
        ...
    }
```
