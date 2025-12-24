namespace TagUI
{
    partial class 选题窗口
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(选题窗口));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.多标签筛选 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TagsTreeView = new System.Windows.Forms.TreeView();
            this.增删标签右键弹窗 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addChildTag = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTag = new System.Windows.Forms.ToolStripMenuItem();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.QuestionCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.增加题目ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.增删标签右键弹窗.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton2,
            this.toolStripButton1,
            this.多标签筛选,
            this.toolStripButton4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1076, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(73, 24);
            this.toolStripButton2.Text = "删除题目";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(43, 24);
            this.toolStripButton1.Text = "刷新";
            // 
            // 多标签筛选
            // 
            this.多标签筛选.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.多标签筛选.Image = ((System.Drawing.Image)(resources.GetObject("多标签筛选.Image")));
            this.多标签筛选.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.多标签筛选.Name = "多标签筛选";
            this.多标签筛选.Size = new System.Drawing.Size(88, 24);
            this.多标签筛选.Text = "多标签选题";
            this.多标签筛选.Click += new System.EventHandler(this.多标签筛选_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(118, 24);
            this.toolStripButton4.Text = "切换到个人题库";
            this.toolStripButton4.Click += new System.EventHandler(this.切换个人题库);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.TagsTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip2);
            this.splitContainer1.Size = new System.Drawing.Size(1076, 838);
            this.splitContainer1.SplitterDistance = 196;
            this.splitContainer1.TabIndex = 1;
            // 
            // TagsTreeView
            // 
            this.TagsTreeView.CheckBoxes = true;
            this.TagsTreeView.ContextMenuStrip = this.增删标签右键弹窗;
            this.TagsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TagsTreeView.Location = new System.Drawing.Point(0, 0);
            this.TagsTreeView.Name = "TagsTreeView";
            this.TagsTreeView.Size = new System.Drawing.Size(196, 838);
            this.TagsTreeView.TabIndex = 0;
            this.TagsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.加载结点题目);
            this.TagsTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TagsTreeView_NodeMouseClick);
            // 
            // 增删标签右键弹窗
            // 
            this.增删标签右键弹窗.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.增删标签右键弹窗.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addChildTag,
            this.removeTag,
            this.增加题目ToolStripMenuItem});
            this.增删标签右键弹窗.Name = "contextMenuStrip1";
            this.增删标签右键弹窗.Size = new System.Drawing.Size(154, 76);
            // 
            // addChildTag
            // 
            this.addChildTag.Name = "addChildTag";
            this.addChildTag.Size = new System.Drawing.Size(153, 24);
            this.addChildTag.Text = "新增子标签";
            this.addChildTag.Click += new System.EventHandler(this.addChildTag_Click);
            // 
            // removeTag
            // 
            this.removeTag.Name = "removeTag";
            this.removeTag.Size = new System.Drawing.Size(153, 24);
            this.removeTag.Text = "删除标签";
            this.removeTag.Click += new System.EventHandler(this.removeTag_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(876, 813);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // toolStrip2
            // 
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuestionCount,
            this.toolStripLabel1});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(876, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // QuestionCount
            // 
            this.QuestionCount.Name = "QuestionCount";
            this.QuestionCount.Size = new System.Drawing.Size(69, 22);
            this.QuestionCount.Text = "题目总数";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(122, 22);
            this.toolStripLabel1.Text = "toolStripLabel1";
            // 
            // 增加题目ToolStripMenuItem
            // 
            this.增加题目ToolStripMenuItem.Name = "增加题目ToolStripMenuItem";
            this.增加题目ToolStripMenuItem.Size = new System.Drawing.Size(153, 24);
            this.增加题目ToolStripMenuItem.Text = "增加题目";
            this.增加题目ToolStripMenuItem.Click += new System.EventHandler(this.增加题目ToolStripMenuItem_Click);
            // 
            // 选题窗口
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 865);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "选题窗口";
            this.Text = "选题窗口";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.增删标签右键弹窗.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView TagsTreeView;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel QuestionCount;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ContextMenuStrip 增删标签右键弹窗;
        private System.Windows.Forms.ToolStripMenuItem addChildTag;
        private System.Windows.Forms.ToolStripMenuItem removeTag;
        private System.Windows.Forms.ToolStripButton 多标签筛选;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripMenuItem 增加题目ToolStripMenuItem;
    }
}