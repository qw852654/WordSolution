using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using Word本地文件操作核心库.用例;

namespace VSTO
{
    public partial class Ribbon1
    {
        private const int 答案底纹颜色值 = 0x00F2F2F2;

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void 导出pdf_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var 当前文档 = Globals.ThisAddIn.Application.ActiveDocument;
                if (当前文档 == null)
                {
                    MessageBox.Show("当前没有可导出的文档。");
                    return;
                }

                var 参数 = new 导出双版本pdf参数
                {
                    文档 = 当前文档,
                    待删除样式 = new List<string> { "答案", "教学讲解内容" }
                };

                var 用例 = new 导出双版本pdf();
                var 结果 = 用例.执行(参数);

                string 无答案版提示 = string.IsNullOrWhiteSpace(结果.无答案版Pdf路径)
                    ? "无答案版：本次未生成，请先确认文档内容或稍后重试"
                    : "无答案版：" + 结果.无答案版Pdf路径;

                MessageBox.Show(
                    "导出完成。\n" +
                    "原始版：" + 结果.原始版Pdf路径 + "\n" +
                    无答案版提示);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败：" + ex.Message);
            }
        }

        private void 源目录导出pdf_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var 当前文档 = Globals.ThisAddIn.Application.ActiveDocument;
                if (当前文档 == null)
                {
                    MessageBox.Show("当前没有可导出的文档。");
                    return;
                }

                var 参数 = new 导出源目录pdf参数
                {
                    文档 = 当前文档
                };

                var 用例 = new 导出源目录pdf();
                var 结果 = 用例.执行(参数);

                MessageBox.Show(
                    "导出完成。\n" +
                    "PDF：" + 结果.Pdf路径);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败：" + ex.Message);
            }
        }

        private void 根据底纹设置答案_Click(object sender, RibbonControlEventArgs e)
        {
            var 当前文档 = Globals.ThisAddIn.Application.ActiveDocument;
            if (当前文档 == null)
            {
                MessageBox.Show("当前没有打开的文档。");
                return;
            }

            if (!文档包含答案样式(当前文档))
            {
                MessageBox.Show("当前文档中不存在“答案”样式，请先创建该样式。");
                return;
            }

            int 已设置段落数 = 0;
            bool 原始屏幕更新状态 = Globals.ThisAddIn.Application.ScreenUpdating;

            try
            {
                Globals.ThisAddIn.Application.ScreenUpdating = false;

                foreach (Word.Paragraph 段落 in 当前文档.Paragraphs)
                {
                    if (!是答案底纹段落(段落))
                    {
                        continue;
                    }

                    段落.set_Style("答案");
                    已设置段落数++;
                }

                MessageBox.Show($"处理完成，共设置 {已设置段落数} 个答案段落。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("根据底纹设置答案失败：" + ex.Message);
            }
            finally
            {
                Globals.ThisAddIn.Application.ScreenUpdating = 原始屏幕更新状态;
            }
        }

        private static bool 文档包含答案样式(Word.Document 文档)
        {
            foreach (Word.Style 样式 in 文档.Styles)
            {
                if (string.Equals(样式.NameLocal, "答案", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool 是答案底纹段落(Word.Paragraph 段落)
        {
            return (int)段落.Shading.BackgroundPatternColor == 答案底纹颜色值;
        }

        private void 将选择内容装入cc_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var 当前文档 = Globals.ThisAddIn.Application.ActiveDocument;
                if (当前文档 == null)
                {
                    MessageBox.Show("当前没有打开的文档。");
                    return;
                }

                var 当前选区 = Globals.ThisAddIn.Application.Selection;
                if (当前选区 == null)
                {
                    MessageBox.Show("当前没有可读取的选择内容。");
                    return;
                }

                var 选择文本 = (当前选区.Range?.Text ?? string.Empty)
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .Trim();

                选择内容文本框.Text = 选择文本;
            }
            catch (Exception ex)
            {
                MessageBox.Show("装入选择内容失败：" + ex.Message);
            }
        }
    }
}
