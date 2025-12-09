using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using TagRunner;

namespace TestProject
{
    [TestClass]
    public class 标签维护器Test
    {
        private string _dataDir;
        private string _tagsPath;

        [TestInitialize]
        public void Setup()
        {
            // 测试运行目录（bin\Debug 或 bin\Release）
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 使用输出目录的 TestData 文件夹，便于运行后直接查看
            _dataDir = Path.Combine(baseDir, "TestData");
            Directory.CreateDirectory(_dataDir);

            _tagsPath = Path.Combine(_dataDir, "tags.json");

            // 若文件不存在则初始化为空；存在则保留（便于观察历史）
            if (!File.Exists(_tagsPath))
                File.WriteAllText(_tagsPath, "[]");
        }

        // 注意：运行时请在 Test Explorer 中单独运行该测试，并在弹窗出现时点击“是”以创建根标签。
        [TestMethod]
        public void AddTag_创建题型根与子标签_文件保留()
        {
            var query = new 标签查询服务(_tagsPath);
            query.LoadTagsTree();

            var maint = new 标签维护器(query);

            // 弹窗：确认将“题型”作为根标签创建，点击“是”
            var rootId = maint.AddTag(name: "题型", parentId: null, category: "题型");

            // 添加子标签（不弹窗，因为父存在）
            var childId = maint.AddTag(name: "填空题", parentId: rootId, category: "题型");

            // 基本结构断言
            var root = query.TagsTree.Single(t => t.Id == rootId);
            Assert.AreEqual("题型", root.Name);
            Assert.AreEqual(null, root.ParentId);

            var child = root.Children.Single(t => t.Id == childId);
            Assert.AreEqual("填空题", child.Name);
            Assert.AreEqual(rootId, child.ParentId);

            // 文件存在且有内容
            Assert.IsTrue(File.Exists(_tagsPath));
            var json = File.ReadAllText(_tagsPath);
            Assert.IsTrue(json.Length > 0);
        }

        // 注意：运行时请在弹窗出现时点击“是”以创建“难度”根标签。
        [TestMethod]
        public void AddTag_难度叶子带数值_文件保留()
        {
            var query = new 标签查询服务(_tagsPath);
            query.LoadTagsTree();

            var maint = new 标签维护器(query);

            // 弹窗：确认将“难度”作为根标签创建，点击“是”
            var diffRootId = maint.AddTag(name: "难度", parentId: null, category: "难度");

            var s1 = maint.AddTag(name: "一星", parentId: diffRootId, category: "难度", numericValue: 1);
            var s3 = maint.AddTag(name: "三星", parentId: diffRootId, category: "难度", numericValue: 3);

            var root = query.TagsTree.Single(t => t.Id == diffRootId);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(1, root.Children.Single(t => t.Id == s1).NumericValue);
            Assert.AreEqual(3, root.Children.Single(t => t.Id == s3).NumericValue);

            Assert.IsTrue(File.Exists(_tagsPath));
        }

        // 注意：运行时请在弹窗出现时点击“是”以创建“知识点”根标签。
        [TestMethod]
        public void Reload_再次加载结构一致_文件保留()
        {
            var query = new 标签查询服务(_tagsPath);
            query.LoadTagsTree();

            var maint = new 标签维护器(query);

            // 弹窗：确认将“知识点”作为根标签创建，点击“是”
            var rootId = maint.AddTag(name: "知识点", parentId: null, category: "知识点");
            var childId = maint.AddTag(name: "数学", parentId: rootId, category: "知识点");

            // 新建查询服务重新加载，验证持久化一致
            var query2 = new 标签查询服务(_tagsPath);
            query2.LoadTagsTree();

            var root2 = query2.TagsTree.Single(t => t.Id == rootId);
            Assert.AreEqual(1, root2.Children.Count);
            Assert.AreEqual(childId, root2.Children[0].Id);

            Assert.IsTrue(File.Exists(_tagsPath));
        }
    }
}
