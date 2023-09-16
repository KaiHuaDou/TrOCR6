using System;
using System.Drawing;

namespace TrOCR
{
	public static class StaticValue
	{
		public static string v_Split;

		public static string v_Restore;

		public static string v_Merge;

		public static string v_googleTranslate_txt;

		public static string v_googleTranslate_back;

		public static int image_h;

		public static int image_w;

		public static string v_single;

		public static Image image_OCR;

		public static string current_v = "5.0.0";

		public static string copy_f = "无格式";

		public static string content = "天若OCR更新";

		public static bool zh_en = true;

		public static bool zh_jp = false;

		public static bool zh_ko = false;

		public static bool set_默认 = true;

		public static bool set_拆分 = false;

		public static bool set_合并 = false;

		public static bool set_翻译 = false;

		public static bool set_记录 = false;

		public static bool set_截图 = false;

		public static float Dpifactor = 1f;

		public static IntPtr mainhandle;

		public static string note = "";

		public static string[] v_note;

		public static int v_notecount = 40;

		public static string baiduAPI_ID = "";

		public static string baiduAPI_key = "";

		public static bool 截图排斥;

		public static bool v_topmost;

		public static Image v_screenimage;

		public static string v_date = "2019-02-22";
	}
}
