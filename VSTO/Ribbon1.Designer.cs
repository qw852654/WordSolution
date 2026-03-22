namespace VSTO
{
    partial class Ribbon1 : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon1()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.处理当前文档 = this.Factory.CreateRibbonTab();
            this.导出功能组 = this.Factory.CreateRibbonGroup();
            this.导出为pdf = this.Factory.CreateRibbonButton();
            this.源目录导出pdf = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.处理当前文档.SuspendLayout();
            this.导出功能组.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Label = "group1";
            this.group1.Name = "group1";
            // 
            // 处理当前文档
            // 
            this.处理当前文档.Groups.Add(this.导出功能组);
            this.处理当前文档.Label = "处理当前文档";
            this.处理当前文档.Name = "处理当前文档";
            // 
            // 导出功能组
            // 
            this.导出功能组.Items.Add(this.导出为pdf);
            this.导出功能组.Items.Add(this.源目录导出pdf);
            this.导出功能组.Label = "导出功能";
            this.导出功能组.Name = "导出功能组";
            // 
            // 导出为pdf
            // 
            this.导出为pdf.Label = "导出为pdf";
            this.导出为pdf.Name = "导出为pdf";
            this.导出为pdf.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.导出pdf_Click);
            // 
            // 源目录导出pdf
            // 
            this.源目录导出pdf.Label = "源目录导出pdf";
            this.源目录导出pdf.Name = "源目录导出pdf";
            this.源目录导出pdf.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.源目录导出pdf_Click);
            // 
            // Ribbon1
            // 
            this.Name = "Ribbon1";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.tab1);
            this.Tabs.Add(this.处理当前文档);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.处理当前文档.ResumeLayout(false);
            this.处理当前文档.PerformLayout();
            this.导出功能组.ResumeLayout(false);
            this.导出功能组.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        private Microsoft.Office.Tools.Ribbon.RibbonTab 处理当前文档;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup 导出功能组;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 导出为pdf;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 源目录导出pdf;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon1 Ribbon1
        {
            get { return this.GetRibbon<Ribbon1>(); }
        }
    }
}
