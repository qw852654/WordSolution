using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using TagRunner;

namespace TagUI
{
    public static class 静态参数
    {
        private static string _题库目录;
        public static string 题库目录
        {
            get
            {
                if (string.IsNullOrEmpty(_题库目录))
                {
                    // 1. 尝试从配置文件读取
                    _题库目录 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData_题目查询服务");
                    // 2. 如果没有配置，弹窗让用户选择
                    if (string.IsNullOrEmpty(_题库目录) || !Directory.Exists(_题库目录))
                    {
                        using (var dlg = new FolderBrowserDialog())
                        {
                            dlg.Description = "请选择题库目录";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                _题库目录 = dlg.SelectedPath;
                            }
                            else
                            {
                                throw new InvalidOperationException("未设置题库目录，无法继续。");
                            }
                        }
                    }
                }
                return _题库目录;
            }
            set
            {
                _题库目录 = value;
            }
        }
    }


    public static class 通用类
    {
        public static void 初始化三大类()
        {
            try
            {
                标签查询服务.Initialize(静态参数.题库目录);
                题目服务.Initialize(静态参数.题库目录);
                标签维护器.Initialize(标签查询服务.Instance);
            }
            catch(InvalidOperationException ex)
            {
                if (MessageBox.Show("不存在题库，是否初始化题库？", "错误", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
                TagRunner.初始化题库.初始化题库目录(静态参数.题库目录);
            }
        }

        internal static bool 删除选中标签(TreeView TagsTreeView)
        {
            标签 rootTag = TagsTreeView.Nodes[0].Tag as 标签;
            var selectedNode = TagsTreeView.SelectedNode;
            if (selectedNode.Tag == null)
            {
                MessageBox.Show("错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (selectedNode.Tag is 标签 selectedTag)
            {
                if (selectedTag.ParentId == null)
                {
                    MessageBox.Show("无法删除根标签。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false ;
                }
                else
                {
                    var maintainer = 标签维护器.Instance;
                    try
                    {
                        maintainer.删除标签(selectedTag);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除标签失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    加载标签树(TagsTreeView, rootTag);
                    return true;
                }
            }
            return false;
        }

        public static bool 新增题目(List<标签> tags, string quesDocx=null)
        {
            if (tags == null || tags.Count == 0)
            {
                MessageBox.Show("请至少选择一个标签");
                return false;
            }

            if (quesDocx==null)
            {
                MessageBox.Show("没有选择正确的题目");
                return false;
            }
            题目服务.Instance.新增题目(tags, quesDocx);
            return true;

        }
        public static void 加载标签树(TreeView treeView, 标签 根标签)
        {
            if (treeView == null || 根标签 == null)
                throw new ArgumentNullException("参数不能为空");

            var rootNode = new TreeNode(根标签.Name) { Tag = 根标签 };
            treeView.BeginUpdate();
            treeView.Nodes.Add(rootNode);
            AddChildrenNodes(rootNode, 根标签.Children);
            treeView.EndUpdate();
            treeView.ExpandAll();
        }

        public static bool 新增子标签(标签维护器 maintainer, string tagName, 标签 parentTag)
        {
            if (string.IsNullOrWhiteSpace(tagName) || parentTag == null )
            {
                MessageBox.Show("输入参数不对。（标签名字或者父标签）");
                return false;
            }
            maintainer.新增标签(tagName, parentTag.Category, parentId: parentTag.Id);
            return true;
        }

        private static void AddChildrenNodes(TreeNode parent, IEnumerable<标签> children)
        {
            if (children == null) return;
            foreach (var child in children)
            {
                var node = new TreeNode(child.Name) { Tag = child };
                parent.Nodes.Add(node);
                AddChildrenNodes(node, child.Children);
            }
        }
    }
    

}

