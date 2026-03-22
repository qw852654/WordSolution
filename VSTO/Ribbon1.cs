using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Word本地文件操作核心库.用例;

namespace VSTO
{
    public partial class Ribbon1
    {
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
    }
}

