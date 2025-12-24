namespace TagUI.用户控件
{
    partial class 标签树控件
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.多选框 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新增标签ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除标签ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.修改标签ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.多选框);
            this.splitContainer1.Panel1.Controls.Add(this.textBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.treeView1);
            this.splitContainer1.Size = new System.Drawing.Size(299, 686);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 0;
            // 
            // 多选框
            // 
            this.多选框.AutoSize = true;
            this.多选框.Checked = true;
            this.多选框.CheckState = System.Windows.Forms.CheckState.Checked;
            this.多选框.Dock = System.Windows.Forms.DockStyle.Right;
            this.多选框.Location = new System.Drawing.Point(240, 0);
            this.多选框.Name = "多选框";
            this.多选框.Size = new System.Drawing.Size(59, 25);
            this.多选框.TabIndex = 1;
            this.多选框.Text = "多选";
            this.多选框.UseVisualStyleBackColor = true;
            this.多选框.CheckedChanged += new System.EventHandler(this.多选框状态切换);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(299, 25);
            this.textBox1.TabIndex = 0;
            // 
            // treeView1
            // 
            this.treeView1.CheckBoxes = true;
            this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(299, 657);
            this.treeView1.TabIndex = 0;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新增标签ToolStripMenuItem,
            this.删除标签ToolStripMenuItem,
            this.修改标签ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(211, 104);
            // 
            // 新增标签ToolStripMenuItem
            // 
            this.新增标签ToolStripMenuItem.Name = "新增标签ToolStripMenuItem";
            this.新增标签ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.新增标签ToolStripMenuItem.Text = "新增子标签";
            this.新增标签ToolStripMenuItem.Click += new System.EventHandler(this.新增子标签ToolStripMenuItem_Click);
            // 
            // 删除标签ToolStripMenuItem
            // 
            this.删除标签ToolStripMenuItem.Name = "删除标签ToolStripMenuItem";
            this.删除标签ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.删除标签ToolStripMenuItem.Text = "删除标签";
            // 
            // 修改标签ToolStripMenuItem
            // 
            this.修改标签ToolStripMenuItem.Name = "修改标签ToolStripMenuItem";
            this.修改标签ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.修改标签ToolStripMenuItem.Text = "修改标签";
            // 
            // 标签树控件
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "标签树控件";
            this.Size = new System.Drawing.Size(299, 686);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.CheckBox 多选框;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 新增标签ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除标签ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 修改标签ToolStripMenuItem;
    }
}
