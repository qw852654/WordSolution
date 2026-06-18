using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Word;

namespace Word本地文件操作核心库.工具
{
    public static class 答案处理工具
    {
        public static void 删除指定样式段落(Document 文档, IEnumerable<string> 待删除样式列表)
        {
            if (文档 == null) throw new ArgumentNullException(nameof(文档));
            if (待删除样式列表 == null) return;

            var 样式集合 = 获取有效样式集合(待删除样式列表);

            if (样式集合.Count == 0) return;

            foreach (string 样式名 in 样式集合)
            {
                Range 范围 = 文档.Content;
                Find 查找 = 范围.Find;

                初始化查找替换(查找);
                查找.Text = "";
                查找.Replacement.Text = "";
                设置查找样式(查找, 样式名);

                object 全部替换 = WdReplace.wdReplaceAll;
                查找.Execute(Replace: ref 全部替换);
            }
        }

        public static void 将指定样式下划线字符替换为中文空格(Document 文档, IEnumerable<string> 样式列表)
        {
            if (文档 == null) throw new ArgumentNullException(nameof(文档));
            if (样式列表 == null) return;

            var 样式集合 = 获取有效样式集合(样式列表);

            foreach (string 样式名 in 样式集合)
            {
                Range 范围 = 文档.Content;
                Find 查找 = 范围.Find;

                初始化查找替换(查找);
                查找.Text = "?";
                查找.Replacement.Text = "\u3000\u3000";
                查找.MatchWildcards = true;
                设置查找样式(查找, 样式名);
                查找.Font.Underline = WdUnderline.wdUnderlineSingle;
                查找.Replacement.Font.Underline = WdUnderline.wdUnderlineSingle;

                object 全部替换 = WdReplace.wdReplaceAll;
                查找.Execute(Replace: ref 全部替换);
            }
        }

        private static List<string> 获取有效样式集合(IEnumerable<string> 样式列表)
        {
            return 样式列表
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct()
                .ToList();
        }

        private static void 初始化查找替换(Find 查找)
        {
            查找.ClearFormatting();
            查找.Replacement.ClearFormatting();

            查找.Forward = true;
            查找.Wrap = WdFindWrap.wdFindStop;
            查找.Format = true;
            查找.MatchCase = false;
            查找.MatchWholeWord = false;
            查找.MatchWildcards = false;
            查找.MatchSoundsLike = false;
            查找.MatchAllWordForms = false;
        }

        private static void 设置查找样式(Find 查找, string 样式名)
        {
            object 样式对象 = 样式名;
            查找.set_Style(ref 样式对象);
        }
    }
}

