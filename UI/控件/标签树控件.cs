using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Core.QuestionBank.Domain;

namespace UI.控件
{
    // 封装 TagRunner 标签树控件（UI 占位）
    public class 标签树控件 : UserControl
    {
        private ContextMenuStrip contextMenuStrip1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem 新增标签ToolStripMenuItem;
        private ToolStripMenuItem 删除标签ToolStripMenuItem;
        private TreeView treeView1;

        public event EventHandler<标签> TagSelected;

        public 标签树控件(List<标签> 根标签列表 = null)
        {
            InitializeComponent();

            treeView1.NodeMouseClick += treeView1_NodeMouseClick;

            if (根标签列表 != null)
                加载标签树(根标签列表);
        }

        internal void 加载标签树(List<标签> 根标签列表)
        {
            //递归加载标签节点
            void 添加标签节点(TreeNodeCollection 节点集合, 标签 当前标签)
            {
                var node = new TreeNode(当前标签.Name) { Tag = 当前标签 };
                节点集合.Add(node);
                if (当前标签.Children != null)
                {
                    foreach (var child in 当前标签.Children)
                    {
                        添加标签节点(node.Nodes, child);
                    }
                }
            }

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if (根标签列表 != null)
            {
                foreach (var 标签 in 根标签列表)
                {
                    添加标签节点(treeView1.Nodes, 标签);
                }
            }
            treeView1.EndUpdate();
            treeView1.ExpandAll();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e == null) return;

           
            // 右键时也选中节点，便于菜单操作
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }

            //左键点击调用事件通知外部
            if (e.Button == MouseButtons.Left)
            {
                treeView1.SelectedNode = e.Node;

                var handler = TagSelected;
                if (handler != null)
                    handler(this, (标签)e.Node.Tag);
            }
        }


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新增标签ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除标签ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(232, 497);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect_1);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新增标签ToolStripMenuItem,
            this.删除标签ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(139, 52);
            // 
            // 新增标签ToolStripMenuItem
            // 
            this.新增标签ToolStripMenuItem.Name = "新增标签ToolStripMenuItem";
            this.新增标签ToolStripMenuItem.Size = new System.Drawing.Size(138, 24);
            this.新增标签ToolStripMenuItem.Text = "新增标签";
            this.新增标签ToolStripMenuItem.Click += new System.EventHandler(this.新增标签ToolStripMenuItem_Click);
            // 
            // 删除标签ToolStripMenuItem
            // 
            this.删除标签ToolStripMenuItem.Name = "删除标签ToolStripMenuItem";
            this.删除标签ToolStripMenuItem.Size = new System.Drawing.Size(138, 24);
            this.删除标签ToolStripMenuItem.Text = "删除标签";
            // 
            // 标签树控件
            // 
            this.Controls.Add(this.treeView1);
            this.Name = "标签树控件";
            this.Size = new System.Drawing.Size(232, 497);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        public List<标签> 获取选中标签()
        {
            var 选中标签列表 = new List<标签>();
            if (treeView1.SelectedNode == null)
                return 选中标签列表;

            var tag = treeView1.SelectedNode.Tag as 标签;
            if (tag != null)
                选中标签列表.Add(tag);

            return 选中标签列表;
        }

        private void 新增标签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selected = treeView1.SelectedNode != null ? treeView1.SelectedNode.Tag as 标签 : null;
            if (selected == null)
            {
                MessageBox.Show(this, "请先选择一个父标签节点。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 使用简单输入框获取名称
            var name = Prompt.ShowDialog("请输入子标签名称：", "新增标签");
            if (string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                var newTag = new 标签
                {
                    Name = name.Trim(),
                    ParentId = selected.Id,
                    Description = null,
                    Children = new List<标签>(),
                    PrevSiblingId = null,
                    NextSiblingId = null
                };

                var newId = TagRunner.业务.Bootstrapper.标签服务.新增标签(newTag);

                // 重新加载树
                var roots = TagRunner.业务.Bootstrapper.标签服务.获取标签树();
                加载标签树(roots);

                // 定位并选中新节点
                SelectNodeByTagId(treeView1.Nodes, newId);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "新增失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "新增失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool SelectNodeByTagId(TreeNodeCollection nodes, int id)
        {
            if (nodes == null) return false;

            foreach (TreeNode n in nodes)
            {
                var tag = n.Tag as 标签;
                if (tag != null && tag.Id == id)
                {
                    treeView1.SelectedNode = n;
                    n.EnsureVisible();
                    return true;
                }

                if (SelectNodeByTagId(n.Nodes, id))
                    return true;
            }

            return false;
        }

        // 简易输入框（避免外部依赖）
        private static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                using (var form = new Form())
                using (var label = new Label())
                using (var textBox = new TextBox())
                using (var buttonOk = new Button())
                using (var buttonCancel = new Button())
                {
                    form.Text = caption;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.ShowInTaskbar = false;
                    form.ClientSize = new System.Drawing.Size(360, 120);

                    label.AutoSize = true;
                    label.Text = text;
                    label.Left = 12;
                    label.Top = 12;

                    textBox.Left = 12;
                    textBox.Top = 40;
                    textBox.Width = 336;

                    buttonOk.Text = "确定";
                    buttonOk.DialogResult = DialogResult.OK;
                    buttonOk.Left = 192;
                    buttonOk.Top = 76;
                    buttonOk.Width = 75;

                    buttonCancel.Text = "取消";
                    buttonCancel.DialogResult = DialogResult.Cancel;
                    buttonCancel.Left = 273;
                    buttonCancel.Top = 76;
                    buttonCancel.Width = 75;

                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                    form.AcceptButton = buttonOk;
                    form.CancelButton = buttonCancel;

                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                        return textBox.Text;

                    return null;
                }
            }
        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {

        }
    }
}
