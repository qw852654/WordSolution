using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using TagRunner;


namespace TestProject
{
    [TestClass]
    public class 题目查询服务Test
    {
        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            Aspose许可授权.Authorize();
        }

        private string _testDir;
        private string _jsonPath;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData_题目查询服务");
            Directory.CreateDirectory(_testDir);
            _jsonPath = Path.Combine(_testDir, "Questions.json");
            // 初始化空题目文件
            File.WriteAllText(_jsonPath, "[]");
        }

        
        

        [TestMethod]
        public void 新增题目含文档上传_成功()
        {
            var service = new 题目查询服务(_testDir);

            // 请将此路径修改为你本机的有效 DOCX 路径
            string 用户提供的Docx路径 = @"E:\Desktop\test.docx";
            Assert.IsTrue(File.Exists(用户提供的Docx路径), "请提供有效的 DOCX 文件路径到 用户提供的Docx路径 变量。");

            var newQ = new 题目
            {
                Id = 1001,
                TagIds = new List<int> { 201, 202 },
                Status = 题目状态.待审核
            };

            // 调用包含文档路径的新增方法
            var ok = service.新增题目(newQ, 用户提供的Docx路径);
            Assert.IsTrue(ok, "新增题目或上传文档失败。");

            // 验证 JSON 写入
            Assert.IsTrue(File.Exists(_jsonPath));
            var json = File.ReadAllText(_jsonPath);
            Assert.IsTrue(json.Contains("\"Id\": 1001"));

            var savedDocPath = Path.Combine(_testDir, "source", $"{newQ.Id}.docx");
            Assert.IsTrue(File.Exists(savedDocPath), "目标题库的 DOCX 未找到，上传可能失败。");

            var byTag201 = service.按标签查找(201);
            Assert.IsTrue(byTag201.Exists(q => q.Id == 1001));
        }

        [TestMethod]
        public void 删除题目_同步删除索引与文件_成功()
        {
            var service = new 题目查询服务(_testDir);

            // 先准备：新增一题并上传文档（确保有可删对象）
            string 用户提供的Docx路径 = @"E:\Desktop\test.docx";
            Assert.IsTrue(File.Exists(用户提供的Docx路径), "请提供有效的 DOCX 文件路径到 用户提供的Docx路径 变量。");

            var q = new 题目
            {
                Id = 2002,
                TagIds = new List<int> { 301, 302 },
                Status = 题目状态.待审核
            };
            Assert.IsTrue(service.新增题目(q, 用户提供的Docx路径));

            // 模拟已存在的转换文件（HTML/PDF），以验证删除文件逻辑
            var htmlPath = Path.Combine(_testDir, "html", $"{q.Id}.html");
            var pdfPath  = Path.Combine(_testDir, "pdf",  $"{q.Id}.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(htmlPath));
            Directory.CreateDirectory(Path.GetDirectoryName(pdfPath));
            File.WriteAllText(htmlPath, "<html>test</html>");
            File.WriteAllText(pdfPath, "%PDF-1.4");

            // 验证新增后索引与文件存在
            var docxPath = Path.Combine(_testDir, "source", $"{q.Id}.docx");
            Assert.IsTrue(File.Exists(docxPath));
            Assert.IsTrue(File.Exists(htmlPath));
            Assert.IsTrue(File.Exists(pdfPath));
            Assert.AreEqual(1, service.按标签查找(301).Count);

            // 执行删除
            var deleteResult = typeof(题目查询服务)
                .GetMethod("删除题目")
                .Invoke(service, new object[] { q.Id });
            // 方法签名为 void，无返回；若你改为 bool 可直接断言结果
            // Assert.IsTrue((bool)deleteResult);

            // 断言：索引更新（不应再包含该题）
            var list301 = service.按标签查找(301);
            Assert.AreEqual(0, list301.Count, "标签索引仍包含已删除题目。");

            // 断言：JSON 内容已不含该题
            var json = File.ReadAllText(_jsonPath);
            Assert.IsFalse(json.Contains("\"Id\": 2002"), "JSON 仍包含已删除题目。");

            // 断言：文件被删除
            Assert.IsFalse(File.Exists(docxPath), "DOCX 文件未删除。");
            Assert.IsFalse(File.Exists(htmlPath), "HTML 文件未删除。");
            Assert.IsFalse(File.Exists(pdfPath), "PDF 文件未删除。");
        }
    }
}
