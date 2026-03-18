using System;
using Microsoft.Office.Interop.Word;
using Word本地文件操作核心库.工具;

namespace Word本地文件操作核心库.自动处理
{
    public class 将新下载的组卷试卷插入文档
    {
        public void 执行(Application 应用, string 下载文档路径)
        {
            if (应用 == null) throw new ArgumentNullException(nameof(应用));
            if (string.IsNullOrWhiteSpace(下载文档路径)) throw new ArgumentException("下载文档路径不能为空。", nameof(下载文档路径));

            Document 当前文档 = 应用.ActiveDocument;
            Range 当前光标范围 = 应用.Selection?.Range;

            if (当前文档 == null || 当前光标范围 == null)
            {
                return;
            }

            Document 下载文档 = null;
            try
            {
                下载文档 = 应用.Documents.Open(下载文档路径);
                组卷文档清洗工具.清洗(下载文档);
                Range 插入范围 = 文档插入工具.插入到当前光标(当前光标范围, 下载文档);
                组卷文档清洗工具.仅处理插入范围(插入范围);
            }
            finally
            {
                if (下载文档 != null)
                {
                    下载文档.Close(false);
                }
            }
        }
    }
}
