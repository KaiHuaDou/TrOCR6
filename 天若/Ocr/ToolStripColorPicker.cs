using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TrOCR;

public static partial class HelpRepaint
{
    [DefaultEvent("SelectedColorChanged")]
    [DefaultProperty("Color")]
    [Description("ToolStripItem that allows selecting a color from a color picker control.")]
    [ToolboxItem(false)]
    [ToolboxBitmap(typeof(ToolStripColorPicker), "ToolStripColorPicker")]
    public class ToolStripColorPicker : ToolStripButton
    {
        public Point GetOpenPoint( )
        {
            Point point;
            if (Owner == null)
            {
                point = new Point(5, 5);
            }
            else
            {
                int num = 0;
                foreach (object obj in Parent.Items)
                {
                    ToolStripItem toolStripItem = (ToolStripItem) obj;
                    if (toolStripItem == this)
                    {
                        break;
                    }
                    num += toolStripItem.Width;
                }
                point = new Point(num, -4);
            }
            return point;
        }

        public Point GetPoint
        {
            get => GetOpenPoint( );
            set
            {
            }
        }
    }
}
