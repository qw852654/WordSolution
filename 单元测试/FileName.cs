using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.Interop.Word;
using WordLibrary;

namespace WordLibrary.Tests
{
    [TestClass]
    public class 文档Tests
    {
        private Application _wordApp;
        private Document _testDocument;
        private 文档 _文档对象;

        [TestInitialize]
        public void TestInitialize()
        {
            // 初始化Word应用程序
            _wordApp = new Application();
            _wordApp.Visible = true;

            // 创建测试文档
            _testDocument = _wordApp.Documents.Add();
            _文档对象 = new 文档(_testDocument);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // 关闭测试文档和Word应用程序
            _testDocument.Close(false);
            _wordApp.Quit();
        }

        [TestMethod]
        public void 删除答案_删除指定样式段落()
        {
            // 添加段落并设置样式
            Paragraph para1 = _testDocument.Content.Paragraphs.Add();
            para1.Range.Text = "这是一个答案段落";
            para1.set_Style("答案");

            Paragraph para2 = _testDocument.Content.Paragraphs.Add();
            para2.Range.Text = "这是一个普通段落";
            para2.set_Style("普通");

            // 调用删除答案方法
            _文档对象.删除答案(new string[] { "答案" });

            // 验证段落是否被删除
            bool hasAnswerParagraph = false;
            foreach (Paragraph para in _testDocument.Paragraphs)
            {
                if (para.get_Style().NameLocal == "答案")
                {
                    hasAnswerParagraph = true;
                    break;
                }
            }

            Assert.IsFalse(hasAnswerParagraph, "答案样式的段落未被删除");
        }

        [TestMethod]
        public void 导出pdf到文档目录_生成PDF文件()
        {
            // 设置保存路径
            string tempDirectory = System.IO.Path.GetTempPath();
            string pdfPath = System.IO.Path.Combine(tempDirectory, "测试文档.pdf");

            // 调用导出PDF方法
            _文档对象.导出pdf到文档目录(pdfPath);

            // 验证PDF文件是否生成
            Assert.IsTrue(System.IO.File.Exists(pdfPath), "PDF文件未生成");

            // 删除生成的PDF文件
            if (System.IO.File.Exists(pdfPath))
            {
                System.IO.File.Delete(pdfPath);
            }
        }

        [TestMethod]
        public void 导出无答案的pdf_生成无答案PDF文件()
        {
            // 添加段落并设置样式
            Paragraph para1 = _testDocument.Content.Paragraphs.Add();
            para1.Range.Text = "这是一个答案段落";
            para1.set_Style("答案");

            Paragraph para2 = _testDocument.Content.Paragraphs.Add();
            para2.Range.Text = "这是一个普通段落";
            para2.set_Style("普通");

            // 设置保存路径
            string tempDirectory = System.IO.Path.GetTempPath();
            string pdfPath = System.IO.Path.Combine(tempDirectory, "测试文档_无答案.pdf");

            // 调用导出无答案PDF方法
            _文档对象.导出无答案的pdf(new string[] { "答案" });

            // 验证PDF文件是否生成
            Assert.IsTrue(System.IO.File.Exists(pdfPath), "无答案PDF文件未生成");

            // 删除生成的PDF文件
            if (System.IO.File.Exists(pdfPath))
            {
                System.IO.File.Delete(pdfPath);
            }
        }
    }
}
