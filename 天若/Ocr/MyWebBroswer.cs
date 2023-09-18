using System.Windows.Forms;
using static TrOCR.External.NativeMethods;

namespace TrOCR;

public class MyWebBroswer : WebBrowser
{
    public override bool PreProcessMessage(ref Message msg)
    {
        if (msg.Msg == 256 && (int) msg.WParam == 86 && ModifierKeys == Keys.Control)
        {
            Clipboard.SetDataObject((string) Clipboard.GetDataObject( ).GetData(DataFormats.UnicodeText));
            keybd_event(Keys.ControlKey, 0, 0U, 0U);
            keybd_event(Keys.D9, 0, 0U, 0U);
            keybd_event(Keys.D9, 0, 2U, 0U);
            keybd_event(Keys.ControlKey, 0, 2U, 0U);
        }
        if (msg.Msg == 256 && (int) msg.WParam == 67 && ModifierKeys == Keys.Control)
        {
            keybd_event(Keys.ControlKey, 0, 0U, 0U);
            keybd_event(Keys.D8, 0, 0U, 0U);
            keybd_event(Keys.D8, 0, 2U, 0U);
            keybd_event(Keys.ControlKey, 0, 2U, 0U);
        }
        return base.PreProcessMessage(ref msg);
    }
}
