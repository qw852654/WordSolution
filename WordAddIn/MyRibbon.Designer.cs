namespace WordAddIn
{
    partial class MyRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public MyRibbon()
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
            this.处理当前文档 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.button1 = this.Factory.CreateRibbonButton();
            this.button2 = this.Factory.CreateRibbonButton();
            this.删除样式 = this.Factory.CreateRibbonEditBox();
            this.tab2 = this.Factory.CreateRibbonTab();
            this.group3 = this.Factory.CreateRibbonGroup();
            this.button4 = this.Factory.CreateRibbonButton();
            this.根据字体颜色设置答案 = this.Factory.CreateRibbonButton();
            this.group2 = this.Factory.CreateRibbonGroup();
            this.button3 = this.Factory.CreateRibbonButton();
            this.button5 = this.Factory.CreateRibbonButton();
            this.导出选中内容 = this.Factory.CreateRibbonGroup();
            this.button6 = this.Factory.CreateRibbonButton();
            this.button7 = this.Factory.CreateRibbonButton();
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group5 = this.Factory.CreateRibbonGroup();
            this.光标处插入文档 = this.Factory.CreateRibbonButton();
            this.tab3 = this.Factory.CreateRibbonTab();
            this.group4 = this.Factory.CreateRibbonGroup();
            this.button8 = this.Factory.CreateRibbonButton();
            this.button9 = this.Factory.CreateRibbonButton();
            this.group6 = this.Factory.CreateRibbonGroup();
            this.插入题目but = this.Factory.CreateRibbonButton();
            this.之前插入 = this.Factory.CreateRibbonButton();
            this.之后插入 = this.Factory.CreateRibbonButton();
            this.group7 = this.Factory.CreateRibbonGroup();
            this.button10 = this.Factory.CreateRibbonButton();
            this.处理当前文档.SuspendLayout();
            this.group1.SuspendLayout();
            this.tab2.SuspendLayout();
            this.group3.SuspendLayout();
            this.group2.SuspendLayout();
            this.导出选中内容.SuspendLayout();
            this.tab1.SuspendLayout();
            this.group5.SuspendLayout();
            this.tab3.SuspendLayout();
            this.group4.SuspendLayout();
            this.group6.SuspendLayout();
            this.group7.SuspendLayout();
            this.SuspendLayout();
            // 
            // 处理当前文档
            // 
            this.处理当前文档.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.处理当前文档.Label = "处理当前文档";
            this.处理当前文档.Name = "处理当前文档";
            // 
            // group1
            // 
            this.group1.Items.Add(this.button1);
            this.group1.Items.Add(this.button2);
            this.group1.Items.Add(this.删除样式);
            this.group1.Label = "删除";
            this.group1.Name = "group1";
            // 
            // button1
            // 
            this.button1.Label = "删除答案";
            this.button1.Name = "button1";
            this.button1.ShowImage = true;
            this.button1.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Label = "导出pdf";
            this.button2.Name = "button2";
            this.button2.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button2_Click);
            // 
            // 删除样式
            // 
            this.删除样式.Label = "删除样式";
            this.删除样式.Name = "删除样式";
            this.删除样式.Text = "答案|教学讲解内容";
            this.删除样式.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.editBox1_TextChanged);
            // 
            // tab2
            // 
            this.tab2.Groups.Add(this.group1);
            this.tab2.Groups.Add(this.group3);
            this.tab2.Groups.Add(this.group2);
            this.tab2.Groups.Add(this.导出选中内容);
            this.tab2.Label = "处理当前文档";
            this.tab2.Name = "tab2";
            // 
            // group3
            // 
            this.group3.Items.Add(this.button4);
            this.group3.Items.Add(this.根据字体颜色设置答案);
            this.group3.Label = "设置答案";
            this.group3.Name = "group3";
            // 
            // button4
            // 
            this.button4.Label = "根据底纹设置答案";
            this.button4.Name = "button4";
            this.button4.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button4_Click);
            // 
            // 根据字体颜色设置答案
            // 
            this.根据字体颜色设置答案.Label = "根据字体颜色设置答案";
            this.根据字体颜色设置答案.Name = "根据字体颜色设置答案";
            this.根据字体颜色设置答案.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.根据字体颜色设置答案_Click);
            // 
            // group2
            // 
            this.group2.Items.Add(this.button3);
            this.group2.Items.Add(this.button5);
            this.group2.Label = "group2";
            this.group2.Name = "group2";
            // 
            // button3
            // 
            this.button3.Label = "显示选中底纹颜色代码";
            this.button3.Name = "button3";
            this.button3.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button3_Click);
            // 
            // button5
            // 
            this.button5.Label = "导入模版";
            this.button5.Name = "button5";
            this.button5.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button5_Click);
            // 
            // 导出选中内容
            // 
            this.导出选中内容.Items.Add(this.button6);
            this.导出选中内容.Items.Add(this.button7);
            this.导出选中内容.Label = "导出选中内容";
            this.导出选中内容.Name = "导出选中内容";
            // 
            // button6
            // 
            this.button6.Label = "导出选中内容到桌面";
            this.button6.Name = "button6";
            this.button6.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Label = "去下划线";
            this.button7.Name = "button7";
            this.button7.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button7_Click);
            // 
            // tab1
            // 
            this.tab1.Groups.Add(this.group5);
            this.tab1.Label = "处理新文档";
            this.tab1.Name = "tab1";
            // 
            // group5
            // 
            this.group5.Items.Add(this.光标处插入文档);
            this.group5.Label = "group5";
            this.group5.Name = "group5";
            // 
            // 光标处插入文档
            // 
            this.光标处插入文档.Label = "光标处插入文档";
            this.光标处插入文档.Name = "光标处插入文档";
            this.光标处插入文档.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.光标处插入文档_Click);
            // 
            // tab3
            // 
            this.tab3.Groups.Add(this.group4);
            this.tab3.Groups.Add(this.group6);
            this.tab3.Groups.Add(this.group7);
            this.tab3.Label = "题库";
            this.tab3.Name = "tab3";
            // 
            // group4
            // 
            this.group4.Items.Add(this.button8);
            this.group4.Items.Add(this.button9);
            this.group4.Label = "录入题目";
            this.group4.Name = "group4";
            // 
            // button8
            // 
            this.button8.Label = "加入初中题库";
            this.button8.Name = "button8";
            this.button8.ShowImage = true;
            this.button8.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.录入初中题目);
            // 
            // button9
            // 
            this.button9.Label = "加入高中题库";
            this.button9.Name = "button9";
            this.button9.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.录入高中题目);
            // 
            // group6
            // 
            this.group6.Items.Add(this.插入题目but);
            this.group6.Items.Add(this.之前插入);
            this.group6.Items.Add(this.之后插入);
            this.group6.Label = "插入题目";
            this.group6.Name = "group6";
            // 
            // 插入题目but
            // 
            this.插入题目but.Label = "插入题目";
            this.插入题目but.Name = "插入题目but";
            this.插入题目but.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.插入题目but_Click);
            // 
            // 之前插入
            // 
            this.之前插入.Label = "之前插入";
            this.之前插入.Name = "之前插入";
            this.之前插入.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.之前插入_Click);
            // 
            // 之后插入
            // 
            this.之后插入.Label = "之后插入";
            this.之后插入.Name = "之后插入";
            this.之后插入.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.之后插入_Click);
            // 
            // group7
            // 
            this.group7.Items.Add(this.button10);
            this.group7.Label = "测试";
            this.group7.Name = "group7";
            // 
            // button10
            // 
            this.button10.Label = "加入题库";
            this.button10.Name = "button10";
            this.button10.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.测试加入题库);
            // 
            // MyRibbon
            // 
            this.Name = "MyRibbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.处理当前文档);
            this.Tabs.Add(this.tab2);
            this.Tabs.Add(this.tab1);
            this.Tabs.Add(this.tab3);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.处理当前文档.ResumeLayout(false);
            this.处理当前文档.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.tab2.ResumeLayout(false);
            this.tab2.PerformLayout();
            this.group3.ResumeLayout(false);
            this.group3.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();
            this.导出选中内容.ResumeLayout(false);
            this.导出选中内容.PerformLayout();
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group5.ResumeLayout(false);
            this.group5.PerformLayout();
            this.tab3.ResumeLayout(false);
            this.tab3.PerformLayout();
            this.group4.ResumeLayout(false);
            this.group4.PerformLayout();
            this.group6.ResumeLayout(false);
            this.group6.PerformLayout();
            this.group7.ResumeLayout(false);
            this.group7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab 处理当前文档;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button2;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox 删除样式;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button3;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group3;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button4;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button5;
        private Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group5;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 光标处插入文档;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 根据字体颜色设置答案;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup 导出选中内容;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button6;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button7;
        private Microsoft.Office.Tools.Ribbon.RibbonTab tab3;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group4;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button8;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button9;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group6;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 插入题目but;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 之前插入;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton 之后插入;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group7;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button10;
    }

    partial class ThisRibbonCollection
    {
        internal MyRibbon Ribbon1
        {
            get { return this.GetRibbon<MyRibbon>(); }
        }
    }
}
