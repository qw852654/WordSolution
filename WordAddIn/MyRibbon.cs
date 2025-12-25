using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WordLibrary;
using Microsoft.VisualBasic;
using TagUI;
using TagRunner;
using System.Xml.Linq;

namespace WordAddIn
{
    public partial class MyRibbon
    {
        private string 题库路径= @"E:\Desktop\个人题库";

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

            TagUI.静态参数.题库目录 = @"E:\Desktop\个人题库";
            将选中题目加入题库();


        }

        private void button9_Click(object sender, RibbonControlEventArgs e)
        {
            TagUI.静态参数.题库目录 = @"E:\Desktop\高中题库";
            将选中题目加入题库();
        }

        private void 将选中题目加入题库()
        {
            TagUI.通用类.初始化三大类();


            var app = Globals.ThisAddIn.Application;
            if (app.Selection == null || app.Selection.Range.StoryLength == 0) return;
            var quesRange = app.Selection.Range;

            if (quesRange.StoryLength == 0)
            {
                MessageBox.Show("请先选择题目内容");
                return;
            }


            var quesDoc = app.Documents.Add();

            quesDoc.Content.Delete();

            quesDoc.Content.FormattedText = quesRange.FormattedText;

            //激活标签窗口筛选标签
            var 标签查询器 = TagRunner.标签查询服务.Instance;
            List<标签> quesTags = null;

            using (var dlg = new TagUI.标签选择窗口(标签查询器))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    quesTags = (List<标签>)dlg.标签列表;
            }

            if (quesTags == null || quesTags.Count == 0)
            {
                quesDoc.Close(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
                return;
            }

            var 题目管理器 = TagRunner.题目服务.Instance;
            var quesTempPath = System.IO.Path.GetTempFileName() + $"题目_{DateTime.Now:yyyyMMdd_HHmmssfff}.docx";
            quesDoc.SaveAs2(quesTempPath, WdSaveFormat.wdFormatXMLDocument);


            if (题目管理器.新增题目(quesTags, quesTempPath))
                MessageBox.Show("题目添加成功");

            quesDoc.Close(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
        }

        
    }
}
