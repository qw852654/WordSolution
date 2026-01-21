using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TagRunner.Models;

namespace UI.控件
{
    // 封装 TagRunner 标签树控件（UI 占位）
    public class 标签树控件 : UserControl
    {
        private TreeView treeView1;

        public 标签树控件(List<标签> 根标签列表=null)
        {
            InitializeComponent();
            if (根标签列表 != null)
                加载标签列表(根标签列表);
        }

        private void 加载标签列表(List<标签> 根标签列表)
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
        }

        private void InitializeComponent()
        {
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(232, 497);
            this.treeView1.TabIndex = 0;
            // 
            // 标签树控件
            // 
            this.Controls.Add(this.treeView1);
            this.Name = "标签树控件";
            this.Size = new System.Drawing.Size(232, 497);
            this.ResumeLayout(false);

        }

        internal List<标签> 获取选中标签()
        {
            //返回选中的标签
            var 选中标签列表 = new List<标签>();
            选中标签列表.Add((标签)treeView1.SelectedNode.Tag);
            return 选中标签列表;
        }
    }
}
