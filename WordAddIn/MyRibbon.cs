using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WordLibrary;
using Microsoft.VisualBasic;
using UI;
using TagRunner;
using TagRunner.Models;
using TagRunner.业务;
using System.Xml.Linq;

namespace WordAddIn
{
    public partial class MyRibbon
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            //删除答案
            try
            {
                // 创建 WordLibrary 的文档对象
                文档 文档对象 = new 文档(Globals.ThisAddIn.Application.ActiveDocument);
                // 定义待删除的样式列表
                string[] 待删除的样式列表 = 删除样式.Text.Split('|');
                // 调用删除答案方法
                文档对象.删除答案(待删除的样式列表);
            }
            catch (Exception ex)
            {
                // 捕获并显示错误信息
                System.Windows.Forms.MessageBox.Show($"删除答案操作失败: {ex.Message}");
            }
        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            //导出无答案pdf
            try
            {
                // 创建 WordLibrary 的文档对象
                文档 文档对象 = new 文档(Globals.ThisAddIn.Application.ActiveDocument);
                // 定义待删除的样式列表
                string[] 待删除的样式列表 = 删除样式.Text.Split('|');
                //导出原始文档
                文档对象.导出原始文档();
                // 调用导出无答案pdf方法
                文档对象.导出无答案pdf(待删除的样式列表);
            }
            catch (Exception ex)
            {
                // 捕获并显示错误信息
                System.Windows.Forms.MessageBox.Show($"导出无答案pdf操作失败: {ex.Message}");
            }
        }

        private void editBox1_TextChanged(object sender, RibbonControlEventArgs e)
        {

        }

        private void button3_Click(object sender, RibbonControlEventArgs e)
        {
            //用提示框显示所选部分的段落底纹的填充色,具体写出获取颜色的逻辑
            try
            {
                // 获取当前文档
                var doc = Globals.ThisAddIn.Application.ActiveDocument;
                // 获取所选范围
                var selection = doc.Application.Selection;
                // 获取所选范围的段落底纹颜色
                var color = selection.Range.Shading.BackgroundPatternColor;
                // 显示颜色值
                System.Windows.Forms.MessageBox.Show($"所选部分的段落底纹颜色值: {color}");
            }
            catch (Exception ex)
            {
                // 捕获并显示错误信息
                System.Windows.Forms.MessageBox.Show($"获取段落底纹颜色操作失败: {ex.Message}");
            }
        }

        private void button4_Click(object sender, RibbonControlEventArgs e)
        {
            //根据底纹设置答案
            try
            {
                // 创建 WordLibrary 的文档对象
                文档 文档对象 = new 文档(Globals.ThisAddIn.Application.ActiveDocument);
                // 调用根据底纹设置答案方法
                文档对象.根据底纹设置答案();
            }
            catch (Exception ex)
            {
                // 捕获并显示错误信息
                System.Windows.Forms.MessageBox.Show($"根据底纹设置答案操作失败: {ex.Message}.(是不是忘导入模版了)");
            }
        }

        private void button5_Click(object sender, RibbonControlEventArgs e)
        {
            //导入模版并更新
            try
            {
                // 创建 WordLibrary 的文档对象
                文档 文档对象 = new 文档(Globals.ThisAddIn.Application.ActiveDocument);
                // 调用导入模版并更新方法
                文档对象.导入模板样式并更新文档(true);
            }
            catch (Exception ex)
            {
                // 捕获并显示错误信息
                System.Windows.Forms.MessageBox.Show($"导入模版并更新操作失败: {ex.Message}");
            }
        }

        private void 光标处插入文档_Click(object sender, RibbonControlEventArgs e)
        {
            // 弹窗让用户选择文档，然后再光标出插入分节符（下一页），将用户选择的文档打开，将所有内容插入，然后将光标定位到插入的内容最后。插入的时候，如果样式冲突，不要将插入的内容重命名
            var app = Globals.ThisAddIn.Application;
            var selection = app.Selection;

            // 1. 弹出文件选择窗口
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Word 文档 (*.doc;*.docx)|*.doc;*.docx";
            dlg.Title = "选择要插入的 Word 文件";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string filePath = dlg.FileName;

            // 2. 在光标处插入回车和分节符（下一页）
            selection.TypeParagraph();
            selection.InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdSectionBreakNextPage);

            // 记录插入前的位置
            int insertStart = selection.Range.End;

            // 插入内容
            selection.Range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);
            selection.InsertFile(
                filePath,
                Type.Missing,
                false, // ConfirmConversions
                false, // Link
                false  // Attachment
            );

            selection.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);






        }

        private void 根据字体颜色设置答案_Click(object sender, RibbonControlEventArgs e)
        {
            //遍历所有样式为正文的段落，如果字体颜色是红色，则将该段落的样式设置为答案样式
            try
            {
                // 创建 WordLibrary 的文档对象
                文档 文档对象 = new 文档(Globals.ThisAddIn.Application.ActiveDocument);
                // 调用根据字体颜色设置答案方法
                文档对象.根据字体颜色设置答案();
            }
            catch (Exception ex)
            {
                // 捕获并显示错误信息
                System.Windows.Forms.MessageBox.Show($"根据字体颜色设置答案操作失败: {ex.Message}");
            }
        }

        private void button6_Click(object sender, RibbonControlEventArgs e)
        {
            //弹出对话框询问用户文件名然后存储到变量“文件名”中
            string 文件名 = "";
            using (Form inputForm = new Form())
            {
                inputForm.Width = 300;
                inputForm.Height = 150;
                inputForm.Text = "保存文件";
                Label label = new Label() { Left = 10, Top = 20, Text = "请输入文件名", AutoSize = true };
                TextBox textBox = new TextBox() { Left = 10, Top = 50, Width = 260, Text = "无答案文档" };
                Button confirmation = new Button() { Text = "确定", Left = 200, Width = 70, Top = 80, DialogResult = DialogResult.OK };
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(confirmation);
                inputForm.AcceptButton = confirmation;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    文件名 = textBox.Text;
                }
            }
            文档 文档对象 = new 文档(Globals.ThisAddIn.Application.ActiveDocument);
            //导出选择部分的内容
            Range rng= Globals.ThisAddIn.Application.Selection.Range;
            文档对象.导出部分内容到桌面 (rng, 文件名);
        }

        private void button7_Click(object sender, RibbonControlEventArgs e)
        {
            //将活动文档选中的内容复制到新文档中，在新文档中，将每个带下划线的字符都替换成中文空格
            try
            {
                var app = Globals.ThisAddIn.Application;
                var selection = app.Selection;
                var rng = selection.Range;
                var doc = app.ActiveDocument; // 修复：定义 doc 变量

                string docDir = System.IO.Path.GetDirectoryName(doc.FullName);

                // 获取 selection 第一个段落的内容，并去除首尾空白及非法字符
                string paraText = selection.Range.Paragraphs[1].Range.Text.Trim();
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    paraText = paraText.Replace(c, '_');
                }

                // 拼接完整路径
                string pdfPath = System.IO.Path.Combine(docDir, paraText + ".pdf");

                // 新建文档
                var newDoc = app.Documents.Add();

                // 复制选中内容到新文档
                newDoc.Content.Delete(); // 清空新文档内容
                newDoc.Content.FormattedText = rng.FormattedText;

                // 替换新文档中所有带下划线的字符为中文空格
                var newRange = newDoc.Content;
                for (int i = 1; i <= newRange.Characters.Count; i++)
                {
                    var ch = newRange.Characters[i];
                    if (ch.Font.Underline != Microsoft.Office.Interop.Word.WdUnderline.wdUnderlineNone)
                    {
                        ch.Text = "　"; // 中文全角空格
                    }
                }

                // 导出文档
                文档 新文档对象 = new 文档(newDoc);
                新文档对象.导出当前文档为pdf(保存的文件路径: pdfPath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"操作失败: {ex.Message}");
            }
        }

        private void 插入选中题目(object sender, RibbonControlEventArgs e)
        {

        }

        private void 录入高中题目(object sender, RibbonControlEventArgs e)
        {
            string 高中题库路径 = @"E:\Desktop\GZ_realLibrary";
            Bootstrapper.Initialize(new 题库配置(高中题库路径), 覆盖数据库: false);
            录入选中部分进题库();
            
        }

        private static bool 录入选中部分进题库()
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                var selection = app.Selection;

                if (selection == null || selection.Range == null)
                {
                    MessageBox.Show("没有可用的选区。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (selection.Range.Start == selection.Range.End)
                {
                    MessageBox.Show("请先选中要作为题目的内容。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                // 1) 复制选中内容到新文档
                var newDoc = app.Documents.Add();
                newDoc.Content.Delete();
                newDoc.Content.FormattedText = selection.Range.FormattedText;

                // 2) 保存到临时目录
                var tempDir = Path.Combine(Path.GetTempPath(), "TagRunner");
                Directory.CreateDirectory(tempDir);
                var tempDocxPath = Path.Combine(tempDir, "题目_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".docx");

                newDoc.SaveAs2(tempDocxPath, WdSaveFormat.wdFormatDocumentDefault);
                newDoc.Close(WdSaveOptions.wdDoNotSaveChanges);

                // 3) 确保业务层已初始化
                var services = EnsureServicesInitialized();
                if (services == null)
                    return false;

                // 4) 打开新增题目窗口，并预填临时文件路径
                using (var dlg = new UI.窗体.新增题目窗口(services))
                {
                    dlg.设置源文件路径(tempDocxPath);
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("插入题目失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        private static 题库应用服务集 EnsureServicesInitialized()
        {
            // 如果已经初始化过，直接拿服务集
            if (Bootstrapper.配置 != null && Bootstrapper.标签服务 != null && Bootstrapper.题目服务 != null)
                return Bootstrapper.获取应用服务集();

            // 让用户选择题库根目录
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择题库根目录（包含 source/html/index/tagrunner.db）";
                if (fbd.ShowDialog() != DialogResult.OK)
                    return null;
                
                var root = fbd.SelectedPath;
                var config = new 题库配置(root);
                Bootstrapper.Initialize(config, 覆盖数据库: false);
                return Bootstrapper.获取应用服务集();
            }
        }

        private void 插入题目but_Click(object sender, RibbonControlEventArgs e)
        {
            插入题目(InsertPosition.AtSelection);
            
        }

        private static bool 插入题目(InsertPosition insP)
        {
            // 1) 确保题库服务已初始化
            var services = EnsureServicesInitialized();
            if (services == null)
                return false;

            // 2) 打开选题窗口，让用户筛选题目并返回题目 Id 列表
            using (var dlg = new UI.窗体.选题窗口(services))
            {
                var dr = dlg.ShowDialog();
                if (dr != DialogResult.OK)
                    return false;

                var questions = dlg.选中的题目;
                if (questions == null) questions = new List<题目>();

                //遍历题目列表，将题目插入到光标处
                foreach (var q in questions)
                {
                    var handler = new WordHandler(services, Globals.ThisAddIn.Application);
                    handler.插入题目(q, insP);

                }

            }

            return true;
        }

        private void 之前插入_Click(object sender, RibbonControlEventArgs e)
        {
            插入题目(InsertPosition.Before);
        }

        private void 之后插入_Click(object sender, RibbonControlEventArgs e)
        {
            插入题目(InsertPosition.After);
        }

        private void 测试加入题库(object sender, RibbonControlEventArgs e)
        {
            string 高中题库路径 = @"E:\Desktop\test_gaozhogn";
            Bootstrapper.Initialize(new 题库配置(高中题库路径), 覆盖数据库: false);
            录入选中部分进题库();
        }
    }
}
