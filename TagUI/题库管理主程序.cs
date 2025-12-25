using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TagRunner;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace TagUI
{
    public partial class 题库管理主程序 : Form
    {
        private 标签查询服务 _标签查询器;
        private 题目服务 _题目服务;

        public 题库管理主程序()
        {
            InitializeComponent();



            // 窗体加载事件：自动加载标签树
            this.Load += MainForm_Load;



        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            重建环境并刷新界面();
        }

        private void 重建环境并刷新界面()
        {
            //初始化服务
            通用类.初始化三大类();

            var tagsPath = Path.Combine(题库参数.题库目录, "tags.json");

            if (!File.Exists(tagsPath))
            {
                var result = MessageBox.Show($"指定的题库目录不存在，是否在该位置创建题库", "错误", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    初始化题库.初始化题库目录(题库参数.题库目录);
                }
                else
                {
                    return;
                }
            }
            

            // 确保标签文件存在
            
            Directory.CreateDirectory(题库参数.题库目录);
            if (!File.Exists(tagsPath)) 
                File.WriteAllText(tagsPath, "[]");

            // 加载标签树
            _标签查询器 = 标签查询服务.Instance;
            _标签查询器.加载标签树();

            // 初始化题目服务
            _题目服务 = 题目服务.Instance;

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

        private void 加载结点题目(object sender, TreeViewEventArgs e)
        {
            var tag =e.Node.Tag as 标签;
            if (tag != null) {
                this.flowLayoutPanel1.Controls.Clear();
                var questions = _题目服务.标签找题(tag);
                foreach (var q in questions)
                {
                    var card = new QuestionCard(q);
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

            var maintainer = 标签维护器.Instance;
            int newTagId;
            try
            {
                newTagId=maintainer.新增标签(tagName:inputTagName.Trim(),category:category, parentId:parentTagId);
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

       

        private void 删除标签_Click(object sender, EventArgs e)
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
                    var maintainer = 标签维护器.Instance;
                    try
                    {
                        maintainer.删除标签(selectedTag);
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
            题库参数.题库目录 = @"E:\Desktop\个人题库";
            重建环境并刷新界面();
        }

        private void 增加题目ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            List<标签> selectedTags = new List<标签>();
            var selectedNode = TagsTreeView.SelectedNode;
            string quesPath = null;


            if (selectedNode.Tag != null)
            {
                selectedTags.Add((标签)selectedNode.Tag);
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "选择题目文件",
                    Filter = "Word 文档 (*.docx)|*.docx",

                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    quesPath = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            bool success = 通用类.新增题目(selectedTags, quesPath);
            if (success)
            {
                // 刷新题目列表
                加载结点题目(this, new TreeViewEventArgs(selectedNode));

            }
        }

        private void 选择题库(object sender, EventArgs e)
        {
            
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                Description = "选择题库根目录",
            };

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK&&File.Exists(Path.Combine(folderBrowserDialog.SelectedPath,"tags.json")))
            {
                题库参数.题库目录 = folderBrowserDialog.SelectedPath;
                重建环境并刷新界面();
            }

        }
    }
}
