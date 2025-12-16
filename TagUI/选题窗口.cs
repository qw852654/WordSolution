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
        private 标签查询服务 _标签查询器;
        private 题目服务 _题目服务;

        public 选题窗口()
        {
            InitializeComponent();

            // 窗体加载事件：自动加载标签树
            this.Load += MainForm_Load;



        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData_题目查询服务");
            重建环境并刷新界面(rootDir);
        }

        private void 重建环境并刷新界面(string rootDir)
        {
            _rootDir = rootDir;

            var tagsPath = Path.Combine(_rootDir, "tags.json");

            if (!File.Exists(tagsPath))
            {
                var result = MessageBox.Show($"指定的题库目录不存在，是否在该位置创建题库", "错误", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    初始化环境.初始化题库目录(_rootDir);
                }
                else
                {
                    return;
                }
            }
            

            // 确保标签文件存在
            
            Directory.CreateDirectory(_rootDir);
            if (!File.Exists(tagsPath)) 
                File.WriteAllText(tagsPath, "[]");

            // 加载标签树
            _标签查询器 = new 标签查询服务(rootDir);
            _标签查询器.加载标签树();

            // 初始化题目服务
            _题目服务 = new 题目服务(_rootDir);

            加载treeView();

            
        }

        // 将标签树加载到 TreeView（TagsTreeView）
        private void 加载treeView()
        {
            TagsTreeView.BeginUpdate();
            try
            {
                TagsTreeView.Nodes.Clear();

                // 顶层“全部标签”节点
                var allNode = new TreeNode("全部标签") { Tag = null };
                TagsTreeView.Nodes.Add(allNode);

                // 加载根标签（ParentId == null）
                foreach (var root in _标签查询器.标签树根.Where(t => t.ParentId == null))
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
            string category = null;
            if (selectedNode.Tag==null)
            {
                parentTagId = null;
                category=Interaction.InputBox("请输入新增的类别名：题型、难度、章节、解题思维方法、来源、其他" ,"添加根标签");
            }
            else
            {
                var parentTag = (标签)selectedNode.Tag;
                parentTagId = parentTag.Id;
                category = parentTag.Category;
            }


            string inputTagName;
            if (parentTagId==null)
                inputTagName=category;
            else
                inputTagName = Interaction.InputBox("请输入新标签名称：", "添加子标签");
            
            if(string.IsNullOrWhiteSpace(inputTagName))
                return;

            var maintainer = new 标签维护器(_标签查询器);
            int newTagId;
            try
            {
                newTagId=maintainer.新增标签(name:inputTagName.Trim(),category:category, parentId:parentTagId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加标签失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            加载treeView();
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
                    var maintainer = new 标签维护器(_标签查询器);
                    try
                    {
                        maintainer.ID删除标签(selectedTag.Id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除标签失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    加载treeView();
                }
            }
        }

        private void 切换个人题库(object sender, EventArgs e)
        {
            this._rootDir = @"E:\Desktop\个人题库";
            重建环境并刷新界面(_rootDir);
        }
    }
}
