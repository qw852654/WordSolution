using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TagRunner;
using TagRunner.Models;
using TagRunner.业务;

namespace UI.控件
{
    // 标签筛选框控件（占位）
    public class 标签筛选框控件 : UserControl
    {
        private SplitContainer splitContainer1;
        private TextBox textBox1;
        private ListBox listBox1;
        private I标签服务 _标签服务;
        private Button button1;

        // 记录已选中的标签，便于从联想结果中过滤
        private List<标签> _已选标签 = new List<标签>();



        public 标签筛选框控件(I标签服务 service)
        {
            InitializeComponent();
            this.splitContainer1.SplitterDistance = this.splitContainer1.Width / 2;
            _标签服务 = service;
            this.listBox1.Height=this.splitContainer1.Panel2.Height - this.textBox1.Height;
        }


        private void 增加选中标签(标签 待加入标签)
        {
            var 待加入标签控件 = new 基础控件.标签控件_右键点击删除(待加入标签);
            待加入标签控件.Dock = DockStyle.Top;

            // 订阅内部标签控件的删除请求事件
            待加入标签控件.DeleteRequested += 删除选中的标签;

            // 维护已选标签列表
            if (待加入标签 != null && !_已选标签.Exists(t => t.Id == 待加入标签.Id))
                _已选标签.Add(待加入标签);

            this.splitContainer1.Panel1.Controls.Add(待加入标签控件);
        }

        // 当某个标签控件触发删除请求时执行：从界面移除控件并通知外部
        private void 删除选中的标签(object sender, 标签 e)
        {
            if (sender is Control c)
            {
                if (this.splitContainer1.Panel1.Controls.Contains(c))
                {
                    this.splitContainer1.Panel1.Controls.Remove(c);
                    c.Dispose();
                }
            }

            // 从已选列表中移除（防御性检查）
            if (e != null)
            {
                _已选标签.RemoveAll(t => t.Id == e.Id);
            }
        }

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listBox1);
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Size = new System.Drawing.Size(490, 150);
            this.splitContainer1.SplitterDistance = 121;
            this.splitContainer1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(44, -1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "清空";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(0, 25);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(365, 124);
            this.listBox1.TabIndex = 1;
            this.listBox1.Visible = false;
            this.listBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseClick);
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(365, 25);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.联想标签);
            // 
            // 标签筛选框控件
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "标签筛选框控件";
            this.Size = new System.Drawing.Size(490, 150);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void 联想标签(object sender, EventArgs e)
        {
            string 输入文本 = this.textBox1.Text;
            List<标签> 联想结果=_标签服务.联想标签(输入文本);
            
            // 从联想结果中剔除已被选择的标签
            var 过滤后 = (联想结果 ?? new List<标签>()).FindAll(t => !_已选标签.Exists(s => s.Id == t.Id));

            更新联想列表(过滤后);
        }

        private void 更新联想列表(List<标签> 联想结果)
        {
            if (联想结果 == null || 联想结果.Count == 0)
            {
                this.listBox1.Visible = false;
                return;
            }
            else
            {
                this.listBox1.Visible = true;
                this.listBox1.Items.Clear();
                foreach (var tag in 联想结果)
                {
                    this.listBox1.Items.Add(tag);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // 在列表中单击某项时将其加入已选（满足“点击选中即新增”要求）
        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.listBox1.SelectedItem is 标签 selected)
            {
                增加选中标签(selected);
                // 清除输入并隐藏联想列表
                this.textBox1.Clear();
                更新联想列表(null);
                this.textBox1.Focus();
            }
        }

        // 按回车键也加入已选
        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && this.listBox1.SelectedItem is 标签 selected)
            {
                e.Handled = true;
                增加选中标签(selected);
                this.textBox1.Clear();
                更新联想列表(null);
                this.textBox1.Focus();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 收集要移除的标签控件（避免在遍历时直接修改集合）
            var toRemove = new List<Control>();
            foreach (Control c in this.splitContainer1.Panel1.Controls)
            {
                if (c is 基础控件.标签控件_右键点击删除)
                    toRemove.Add(c);
            }

            // 移除并释放控件
            foreach (var c in toRemove)
            {
                this.splitContainer1.Panel1.Controls.Remove(c);
                c.Dispose();
            }

            // 清空已选标签记录
            _已选标签.Clear();
        }

        internal List<标签> 获取选中标签()
        {
            // 返回已选标签的副本，避免外部修改内部状态
            return new List<标签>(_已选标签);
        }
    }
}
