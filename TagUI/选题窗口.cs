using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TagRunner;
using Microsoft.VisualBasic;

namespace TagUI
{
    public partial class 选题窗口 : Form
    {
        private string _rootDir;
        private 标签查询服务 _标签服务;
        private 题目查询服务 _题目服务;

        public 选题窗口()
        {
            InitializeComponent();

            // 窗体加载事件：自动加载标签树
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            初始化环境.初始化(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData_题目查询服务"));
            // 根目录按需调整为你的数据根目录（与题目查询服务一致）
            _rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData_题目查询服务");

            // 确保标签文件存在
            var tagsPath = Path.Combine(_rootDir, "tags.json");
            Directory.CreateDirectory(_rootDir);
            if (!File.Exists(tagsPath)) 
                File.WriteAllText(tagsPath, "[]");

            // 加载标签树
            _标签服务 = new 标签查询服务(tagsPath);
            _标签服务.LoadTagsTree();

            // 初始化题目服务
            _题目服务 = new 题目查询服务(_rootDir);

            LoadTagsTree();
        }

        // 将标签树加载到 TreeView（TagsTreeView）
        private void LoadTagsTree()
        {
            TagsTreeView.BeginUpdate();
            try
            {
                TagsTreeView.Nodes.Clear();

                // 顶层“全部标签”节点
                var allNode = new TreeNode("全部标签") { Tag = null };
                TagsTreeView.Nodes.Add(allNode);

                // 加载根标签（ParentId == null）
                foreach (var root in _标签服务.TagsTree.Where(t => t.ParentId == null))
                {
                    allNode.Nodes.Add(CreateTagNodeRecursive(root));
                }

                TagsTreeView.ExpandAll();
                TagsTreeView.SelectedNode = allNode;
            }
            finally
            {
                TagsTreeView.EndUpdate();
            }
        }

        // 递归创建树节点
        private TreeNode CreateTagNodeRecursive(标签 tag)
        {
            var node = new TreeNode(tag.Name) { Tag = tag };
            if (tag.Children != null && tag.Children.Count > 0)
            {
                foreach (var child in tag.Children)
                {
                    node.Nodes.Add(CreateTagNodeRecursive(child));
                }
            }
            return node;
        }

        private void 选中标签后加载题目(object sender, TreeViewEventArgs e)
        {
            var tag =e.Node.Tag as 标签;
            if (tag != null) {
                this.flowLayoutPanel1.Controls.Clear();
                var questions = _题目服务.标签ID找题(tag.Id);
                foreach (var q in questions)
                {
                    var htmlPath = Path.Combine(_rootDir, "html", $"{q.Id}.html");
                    var card = new QuestionCard(htmlPath);
                    card.Width = this.flowLayoutPanel1.Width - 10;
                    card.Height = 300;
                    this.flowLayoutPanel1.Controls.Add(card);
                }
            }
            
        }

        private void TagsTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TagsTreeView.SelectedNode = e.Node;
            }
        }

        private void addChildTag_Click(object sender, EventArgs e)
        {
            var selectedNode = TagsTreeView.SelectedNode;
            int? parentTagId;
            if(selectedNode.Tag==null)
            {
                parentTagId = null;
            }
            else
            {
                var parentTag = (标签)selectedNode.Tag;
                parentTagId = parentTag.Id;
            }




            var inputTagName = Interaction.InputBox("请输入新标签名称：", "添加子标签");
            
            if(string.IsNullOrWhiteSpace(inputTagName))
                return;

            var maintainer = new 标签维护器(_标签服务);
            int newTagId;
            try
            {
                newTagId=maintainer.新增标签(inputTagName.Trim(), parentTagId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加标签失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            LoadTagsTree();
        }

        private void 多标签筛选_Click(object sender, EventArgs e)
        {

        }

        private void 新增题目(object sender, EventArgs e)
        {

        }

        private void removeTag_Click(object sender, EventArgs e)
        {
            var selectedNode = TagsTreeView.SelectedNode;
            if (selectedNode.Tag == null)
            {
                MessageBox.Show("无法删除根节点“全部标签”。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(selectedNode.Tag is 标签 selectedTag)
            {
                if (selectedTag.ParentId == null)
                {
                    MessageBox.Show("无法删除根标签。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    var maintainer = new 标签维护器(_标签服务);
                    try
                    {
                        maintainer.RemoveTagById(selectedTag.Id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除标签失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    LoadTagsTree();
                }
            }
        }
    }
}
