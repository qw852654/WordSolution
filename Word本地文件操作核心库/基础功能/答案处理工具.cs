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

            var 样式集合 = 待删除样式列表
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            if (样式集合.Count == 0) return;

            for (int i = 文档.Paragraphs.Count; i >= 1; i--)
            {
                Paragraph 段落 = 文档.Paragraphs[i];
                string 样式名 = 段落.get_Style().NameLocal;
                if (样式集合.Contains(样式名))
                {
                    段落.Range.Delete();
                }
            }
        }
    }
}

