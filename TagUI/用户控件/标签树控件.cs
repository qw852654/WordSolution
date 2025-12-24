using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagRunner;
using TagUI;

namespace TagUI.用户控件
{
    public partial class 标签树控件 : UserControl
    {
        private readonly 标签 _rootTag;

        public 标签树控件(标签 rootTag)
        {
            InitializeComponent();
            _rootTag = rootTag ?? throw new ArgumentNullException(nameof(rootTag));
            通用类.加载标签树(this.treeView1, _rootTag);
        }

        public List<标签> 获取选中的标签()
        {
            var selectedTags = new List<标签>();
            if (treeView1.CheckBoxes == false)
            {
                var node= treeView1.SelectedNode;
                if(node!=null&&node.Tag is 标签 tag)
                {
                    selectedTags.Add(tag);
                }
                return selectedTags;
            }
            else
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if (node.Checked)
                    {
                        if (node.Tag is 标签 tag)
                        {
                            selectedTags.Add(tag);
                        }
                    }
                    colloctNode(node, selectedTags);
                }
            }


            return selectedTags;
        }

        private void colloctNode(TreeNode node,List<标签> col)
        {
            if (node == null)
                return;

            if (node.Checked && node?.Tag is 标签 tag)
                col.Add(tag);

            foreach (TreeNode child in node.Nodes)
                colloctNode(child,col);
        }

        private void 多选框状态切换(object sender, EventArgs e)
        {
            treeView1.CheckBoxes = 多选框.Checked == true;
        }

        

        private void 新增子标签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeView1.SelectedNode;
            var parentTag = selectedNode?.Tag as 标签;

            string tagName = Interaction.InputBox("请输入新标签名称：", "新增标签", "");
                       
            if(通用类.新增子标签(标签维护器.Instance, tagName, parentTag))
            {
                // 刷新树视图
                treeView1.Nodes.Clear();
                通用类.加载标签树(this.treeView1, _rootTag);
            }

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }
    }
}
