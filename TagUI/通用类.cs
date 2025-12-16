using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagRunner;

namespace TagUI
{
    internal class 通用类
    {
        public static void 加载标签树(TreeView treeView, 标签 根标签)
        {
            if (treeView == null || 根标签 == null)
                throw new ArgumentNullException("参数不能为空");

            var rootNode = new TreeNode(根标签.Name);
            rootNode.Tag = 根标签;

            treeView.BeginUpdate();

            treeView.Nodes.Add(rootNode);


            AddChildrenNodes(rootNode, 根标签.Children);

            treeView.EndUpdate();
        }


        private static void AddChildrenNodes(TreeNode parent, IEnumerable<标签> children)
        {
            if(children == null)
                return;

            foreach (var child in children)
            {
                var node= new TreeNode(child.Name);
                parent.Nodes.Add(node);
                node.Tag= child;
                AddChildrenNodes(node, child.Children);
            }


                

        }
    }
    

}
    

