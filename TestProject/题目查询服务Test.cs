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
        public void 删除题目_同步删除索引与文件_成功()
        {
            var service = new 题目服务(_testDir);

            // 先准备：新增一题并上传文档（确保有可删对象）
            string 用户提供的Docx路径 = @"E:\Desktop\test.docx";


            var q = new 题目
            {
                Id = 2002,
                TagIds = new List<int> { 301, 302 },
                Status = 题目状态.待审核
            };

            var tags= new List<标签>
            {
                new 标签 { Id = 301, Name = "标签A" },
                new 标签 { Id = 302, Name = "标签B" }
            };

            service.新增题目(tags, 用户提供的Docx路径);


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


            // 执行删除
            var deleteResult = typeof(题目服务)
                .GetMethod("删除题目")
                .Invoke(service, new object[] { q.Id });
            // 方法签名为 void，无返回；若你改为 bool 可直接断言结果
            // Assert.IsTrue((bool)deleteResult);



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
