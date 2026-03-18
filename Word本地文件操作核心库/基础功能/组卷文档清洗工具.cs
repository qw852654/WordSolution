using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Word;

namespace Word本地文件操作核心库.工具
{
    public static class 组卷文档清洗工具
    {
        public static void 清洗(Document 文档)
        {
            删除前三段(文档);
            合并空段落(文档);
            删除题型标题(文档);
            删除最后一段(文档);
        }

        public static void 仅处理插入范围(Range 插入范围)
        {
            if (插入范围 == null) return;

            foreach (Paragraph 段落 in 插入范围.Paragraphs)
            {
                if (段落.Shading.BackgroundPatternColor != WdColor.wdColorAutomatic)
                {
                    段落.set_Style("答案");
                }
            }

            Regex 编号正则 = new Regex(@"^\d{1,2}[．.]");
            foreach (Paragraph 段落 in 插入范围.Paragraphs)
            {
                if (段落.Range.get_Information(WdInformation.wdWithInTable)) continue;
                if (段落.get_Style() is Style 样式 && 样式.NameLocal != "正文") continue;

                string 文本 = 段落.Range.Text;
                if (文本.EndsWith("\r") || 文本.EndsWith("\n"))
                {
                    文本 = 文本.Substring(0, 文本.Length - 1);
                }

                Match 匹配结果 = 编号正则.Match(文本);
                if (匹配结果.Success)
                {
                    Range 目标 = 段落.Range.Duplicate;
                    目标.End = 目标.Start + 匹配结果.Length;
                    目标.Delete();
                    段落.set_Style("例题");
                }
            }
        }

        private static void 删除前三段(Document 文档)
        {
            if (文档.Paragraphs.Count < 3) return;
            文档.Range(文档.Paragraphs[1].Range.Start, 文档.Paragraphs[3].Range.End).Delete();
        }

        private static void 合并空段落(Document 文档)
        {
            Range 范围 = 文档.Content;
            Find 查找 = 范围.Find;
            object 全部替换 = WdReplace.wdReplaceAll;

            查找.ClearFormatting();
            查找.Text = "^p^p";
            查找.Replacement.ClearFormatting();
            查找.Replacement.Text = "^p";
            查找.Forward = true;
            查找.Wrap = WdFindWrap.wdFindStop;
            查找.Format = false;
            查找.MatchCase = false;
            查找.MatchWholeWord = false;
            查找.MatchWildcards = false;
            查找.MatchSoundsLike = false;
            查找.MatchAllWordForms = false;
            查找.Execute(Replace: ref 全部替换);
        }

        private static void 删除题型标题(Document 文档)
        {
            Range 范围 = 文档.Content;
            Find 查找 = 范围.Find;
            object 全部替换 = WdReplace.wdReplaceAll;

            查找.ClearFormatting();
            查找.Font.Bold = 1;
            查找.Text = "[一二三四五]*^13";
            查找.Replacement.ClearFormatting();
            查找.Replacement.Text = "";
            查找.Forward = true;
            查找.Wrap = WdFindWrap.wdFindStop;
            查找.Format = true;
            查找.MatchWildcards = true;
            查找.Execute(Replace: ref 全部替换);
        }

        private static void 删除最后一段(Document 文档)
        {
            if (文档.Paragraphs.Count == 0) return;
            文档.Paragraphs[文档.Paragraphs.Count].Range.Delete();
        }

    }
}
