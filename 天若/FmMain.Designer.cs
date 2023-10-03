using System;
using System.Drawing;
using System.Windows.Forms;
using TrOCR.Controls;
using TrOCR.Helper;
using static TrOCR.External.NativeMethods;

namespace TrOCR
{
    public partial class FmMain : Form
    {
        protected override void Dispose(bool disposing)
        {
            ChangeClipboardChain(base.Handle, this.nextClipboardViewer);
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new global::System.ComponentModel.Container();
            global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(FmMain));
            this.notifyIcon = new NotifyIcon(this.components);
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.toolStrip = new ToolStripMenuItem();
            this.transInput = new ToolStripMenuItem();
            this.transGoogle = new ToolStripMenuItem();
            this.transBaidu = new ToolStripMenuItem();
            this.transTencent = new ToolStripMenuItem();
            this.tableBaidu = new ToolStripMenuItem();
            this.tableAli = new ToolStripMenuItem();
            this.tableOcr = new ToolStripMenuItem();
            this.menu = new ContextMenuStrip();
            this.menu.Renderer = new MenuItemRendererT();
            this.ch2en = new ToolStripMenuItem();
            this.jap = new ToolStripMenuItem();
            this.kor = new ToolStripMenuItem();
            this.pinyin = new ToolStripMenuItem();
            this.customizeProxy = new ToolStripMenuItem();
            this.nullProxy = new ToolStripMenuItem();
            this.systemProxy = new ToolStripMenuItem();
            this.Proxy = new ToolStripMenuItem();
            this.left2right = new ToolStripMenuItem();
            this.right2left = new ToolStripMenuItem();
            this.mainCopy = new ToolStripMenuItem();
            this.mainPaste = new ToolStripMenuItem();
            this.mainSelectAll = new ToolStripMenuItem();
            this.mainInterface = new ToolStripMenuItem();
            this.mainExit = new ToolStripMenuItem();
            this.mainChange = new ToolStripMenuItem();
            this.ZhTrans = new ToolStripMenuItem();
            this.TransZh = new ToolStripMenuItem();
            this.StrUpper = new ToolStripMenuItem();
            this.UpperStr = new ToolStripMenuItem();
            this.speak = new ToolStripMenuItem();
            this.transCopy = new ToolStripMenuItem();
            this.transPaste = new ToolStripMenuItem();
            this.transSelectAll = new ToolStripMenuItem();
            this.transClose = new ToolStripMenuItem();
            this.transVoice = new ToolStripMenuItem();
            this.sougou = new ToolStripMenuItem();
            this.mathFunction = new ToolStripMenuItem();
            this.tencent = new ToolStripMenuItem();
            this.baidu = new ToolStripMenuItem();
            this.verticalScan = new ToolStripMenuItem();
            this.write = new ToolStripMenuItem();
            this.tencentV = new ToolStripMenuItem();
            this.baiduS = new ToolStripMenuItem();
            this.baiduV = new ToolStripMenuItem();
            this.tencent = new ToolStripMenuItem();
            this.baidu = new ToolStripMenuItem();
            this.youdao = new ToolStripMenuItem();
            this.zh = new ToolStripMenuItem();
            this.en = new ToolStripMenuItem();
            this.split = new ToolStripMenuItem();
            this.restore = new ToolStripMenuItem();
            this.menuCopy = new ContextMenuStrip();
            this.menuCopy.Renderer = new MenuItemRendererT();
            this.image1 = new PictureBox();
            this.richBox = new AdvRichTextBox();
            this.richBoxTrans = new AdvRichTextBox();
            this.notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "最小化到任务栏";
            this.notifyIcon.BalloonTipTitle = "提示";
            this.notifyIcon.Icon = (Icon)componentResourceManager.GetObject("minico.Icon");
            this.notifyIcon.Text = "双击开始截图识别";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new MouseEventHandler(this.TrayDoubleClick);
            this.fontBase.Width = 18f * this.dpiFactor;
            this.fontBase.Height = 17f * this.dpiFactor;
            this.richBoxTrans.Visible = false;
            this.richBox.Dock = DockStyle.Fill;
            this.richBox.BorderStyle = BorderStyle.Fixed3D;
            this.richBox.Location = new Point(0, 0);
            this.richBox.Name = "htmlTextBoxBody";
            this.richBox.ImeMode = ImeMode.HangulFull;
            this.richBox.TabIndex = 200;
            this.richBox.SetToolBar(WindowType.Main);
            this.richBoxTrans.ImeMode = ImeMode.HangulFull;
            this.transCopy.Text = "复制";
            this.transCopy.Click += new EventHandler(this.TransCopyClick);
            this.transPaste.Text = "粘贴";
            this.transPaste.Click += new EventHandler(this.TransPasteClick);
            this.transSelectAll.Text = "全选";
            this.transSelectAll.Click += new EventHandler(this.TransSelectAllClick);
            this.transClose.Text = "关闭";
            this.transClose.Click += new EventHandler(this.TransCloseClick);
            this.transVoice.Text = "朗读";
            this.transVoice.Click += new EventHandler(this.TranslateVoiceClick);
            this.transInput.Text = "接口";
            this.transInput.Click += new EventHandler(this.TransSelectAllClick);
            this.transGoogle.Text = "谷歌√";
            this.transGoogle.Click += new EventHandler(this.TransGoogleClick);
            this.transBaidu.Text = "百度";
            this.transBaidu.Click += new EventHandler(this.TransBaiduClick);
            this.transTencent.Text = "腾讯";
            this.transTencent.Click += new EventHandler(this.TransTencentClick);
            this.menuCopy.Items.AddRange(new ToolStripItem[] { this.transCopy, this.transPaste, this.transSelectAll, this.transVoice, this.transInput, this.transClose });
            this.transInput.DropDownItems.AddRange(new ToolStripItem[] { this.transGoogle, this.transBaidu, this.transTencent });
            this.menuCopy.Font = new Font("微软雅黑", 9f / Defaults.DpiFactor, FontStyle.Regular);
            this.mainCopy.Text = "复制";
            this.mainCopy.Click += new EventHandler(this.MainCopyClick);
            this.mainPaste.Text = "粘贴";
            this.mainPaste.Click += new EventHandler(this.MainPasteClick);
            this.mainSelectAll.Text = "全选";
            this.mainSelectAll.Click += new EventHandler(this.MainSelectAllClick);
            this.speak.Text = "朗读";
            this.speak.Click += new EventHandler(this.MainVoiceClick);
            this.baiduS.Text = "搜索";
            this.baiduS.Click += new EventHandler(this.MainSearchClick);
            this.mainChange.Text = "转换";
            this.mainInterface.Text = "接口";
            this.mainExit.Text = "退出";
            this.mainExit.Click += new EventHandler(this.TrayExitClick);
            this.menu.Items.AddRange(new ToolStripItem[] { this.mainCopy, this.mainPaste, this.mainSelectAll, this.speak, this.baiduS, this.mainChange, this.mainInterface, this.mainExit });
            this.menu.Font = new Font("微软雅黑", 9f / Defaults.DpiFactor, FontStyle.Regular);
            this.sougou.Text = "搜狗√";
            this.sougou.Click += new EventHandler(this.OcrSogouClick);
            this.mathFunction.Text = "公式";
            this.mathFunction.Click += new EventHandler(this.OcrMathClick);
            this.tencent.Text = "腾讯";
            this.tencent.Click += new EventHandler(this.OcrTencentClick);
            this.baidu.Text = "百度";
            this.youdao.Text = "有道";
            this.youdao.Click += new EventHandler(this.OcrYoudaoClick);
            this.tableOcr.Text = "表格";
            this.tableBaidu.Text = "百度";
            this.tableBaidu.Click += new EventHandler(this.OcrTableBaiduClick);
            this.tableAli.Text = "阿里";
            this.tableAli.Click += new EventHandler(this.OcrTableAliClick);
            this.tableOcr.DropDownItems.AddRange(new ToolStripItem[] { this.tableBaidu, this.tableAli });
            this.verticalScan.Text = "竖排";
            this.write.Text = "手写";
            this.write.Click += new EventHandler(this.OcrWriteClick);
            this.zh.Text = "中文标点";
            this.zh.Click += new EventHandler(this.Switch2ZhClick);
            this.en.Text = "英文标点";
            this.en.Click += new EventHandler(this.Switch2EnClick);
            this.ZhTrans.Text = "中文繁体";
            this.ZhTrans.Click += new EventHandler(this.Switch2ZhTransClick);
            this.TransZh.Text = "中文简体";
            this.TransZh.Click += new EventHandler(this.Switch2TransZhClick);
            this.StrUpper.Text = "英文大写";
            this.StrUpper.Click += new EventHandler(this.Switch2StrUpperClick);
            this.UpperStr.Text = "英文小写";
            this.UpperStr.Click += new EventHandler(this.Switch2UpperStrClick);
            this.pinyin.Text = "汉语拼音";
            this.pinyin.Click += new EventHandler(this.Switch2PinyinClick);
            this.change_button = this.mainChange;
            this.change_button.DropDownItems.AddRange(new ToolStripItem[] { this.zh, this.en, this.ZhTrans, this.TransZh, this.StrUpper, this.UpperStr, this.pinyin });
            this.interface_button = this.mainInterface;
            this.interface_button.DropDownItems.AddRange(new ToolStripItem[] { this.sougou, this.tencent, this.youdao, this.baidu, this.toolStripSeparator1, this.mathFunction, this.tableOcr, this.verticalScan });
            if (Helper.Config.Get("配置", "接口") == "百度")
            {
                this.ch2en.Text = "中英√";
            }
            else
            {
                this.ch2en.Text = "中英";
            }
            this.ch2en.Click += new EventHandler(this.OcrBaiduZhEnClick);
            this.jap.Text = "日语";
            this.jap.Click += new EventHandler(this.OCR_baidu_Jap_Click);
            this.kor.Text = "韩语";
            this.kor.Click += new EventHandler(this.OCR_baidu_Kor_Click);
            ((ToolStripDropDownItem)this.baidu).DropDownItems.AddRange(new ToolStripItem[] { this.ch2en, this.jap, this.kor });
            this.left2right.Text = "从左向右";
            this.left2right.Click += new EventHandler(this.OcrLtrClick);
            this.right2left.Text = "从右向左";
            this.right2left.Click += new EventHandler(this.OcrRtlClick);
            ((ToolStripDropDownItem)this.verticalScan).DropDownItems.AddRange(new ToolStripItem[] { this.left2right, this.right2left });
            this.richBox.InnerContextMenuStrip = this.menu;
            this.richBoxTrans.InnerContextMenuStrip = this.menuCopy;
            this.image1.Image = (Image)new global::System.ComponentModel.ComponentResourceManager(typeof(FmMain)).GetObject("loadcat.gif");
            this.image1.Size = new Size(85, 85);
            this.image1.Location = (Point)new Size((int)this.fontBase.Width * 34 - this.image1.Size.Width / 2, (int)(110f * this.dpiFactor));
            this.image1.BackColor = Color.White;
            this.image1.Visible = false;
            base.SuspendLayout();
            base.StartPosition = FormStartPosition.Manual;
            base.Location = (Point)new Size(Screen.PrimaryScreen.Bounds.Width / 2 - Screen.PrimaryScreen.Bounds.Width / 10, Screen.PrimaryScreen.Bounds.Height / 2 - Screen.PrimaryScreen.Bounds.Height / 6);
            base.Size = new Size((int)this.fontBase.Width * 23, (int)this.fontBase.Height * 24);
            base.Controls.Add(this.richBoxTrans);
            base.Controls.Add(this.image1);
            base.Controls.Add(this.richBox);
            base.Load += new EventHandler(this.Load_Click);
            base.Resize += new EventHandler(this.FormResizing);
            base.Name = "Form1";
            this.Text = "耗时：";
            if (Helper.Config.Get("工具栏", "顶置") == "True")
            {
                base.TopMost = true;
            }
            else
            {
                base.TopMost = false;
            }
            base.Icon = (Icon)componentResourceManager.GetObject("minico.Icon");
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private global::System.ComponentModel.IContainer components;

        public NotifyIcon notifyIcon;

        public ContextMenuStrip menu;

        private ToolStripMenuItem toolStrip;

        public ToolStripMenuItem mainCopy;

        public ToolStripMenuItem mainPaste;

        public ToolStripMenuItem mainSelectAll;

        public ToolStripMenuItem mainExit;

        public ToolStripMenuItem mainInterface;

        public ToolStripItem sougou;

        public ToolStripItem tencent;

        public ToolStripItem baidu;

        public ToolStripItem youdao;

        public ToolStripDropDownItem interface_button;

        public ToolStripMenuItem mainChange;

        public ToolStripDropDownItem change_button;

        public ToolStripMenuItem zh;

        public ToolStripMenuItem en;

        public AdvRichTextBox richBoxTrans;

        public ContextMenuStrip menuCopy;

        public ToolStripMenuItem transCopy;

        public ToolStripMenuItem transPaste;

        public ToolStripMenuItem transSelectAll;

        public ToolStripMenuItem transClose;

        public SizeF fontBase;

        public PictureBox image1;

        public ToolStripMenuItem split;

        public ToolStripMenuItem restore;


        private AdvRichTextBox richBox;

        private global::System.IntPtr nextClipboardViewer;

        public ToolStripMenuItem baiduV;

        public ToolStripMenuItem tencentV;

        public ToolStripMenuItem baiduS;

        private ToolStripMenuItem speak;

        private ToolStripMenuItem transVoice;

        private ToolStripMenuItem ZhTrans;

        private ToolStripMenuItem TransZh;

        private ToolStripMenuItem StrUpper;

        private ToolStripMenuItem UpperStr;

        private ToolStripMenuItem ch2en;

        private ToolStripMenuItem jap;

        private ToolStripMenuItem kor;

        public ToolStripItem verticalScan;

        public ToolStripItem write;

        private ToolStripMenuItem left2right;

        private ToolStripMenuItem right2left;

        private ToolStripMenuItem customizeProxy;

        private ToolStripMenuItem nullProxy;

        private ToolStripMenuItem systemProxy;

        private ToolStripMenuItem Proxy;

        private ToolStripMenuItem pinyin;

        private ToolStripMenuItem transInput;

        private ToolStripMenuItem transGoogle;

        private ToolStripMenuItem transBaidu;

        private ToolStripMenuItem transTencent;

        private ToolStripMenuItem tableOcr;

        private ToolStripSeparator toolStripSeparator1;

        private ToolStripMenuItem tableBaidu;

        private ToolStripMenuItem tableAli;

        public ToolStripItem mathFunction;
    }
}
