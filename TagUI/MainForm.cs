using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TagRunner;

namespace TagUI
{
    public partial class MainForm : Form
    {
        private string _rootDir;
        private 标签查询服务 _标签服务;
        private 题目查询服务 _题目服务;

        public MainForm()
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
                var questions = _题目服务.按标签查找(tag.Id,_标签服务);
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
    }
}
