using Microsoft.Office.Interop.Word;

namespace Word本地文件操作核心库.工具
{
    public static class 文档插入工具
    {
        public static Range 插入到当前光标(Range 当前光标范围, Document 来源文档)
        {
            if (当前光标范围 == null) return null;
            if (来源文档 == null) return null;

            Document 目标文档 = 当前光标范围.Document;
            int 插入开始位置 = 当前光标范围.Start;

            当前光标范围.FormattedText = 来源文档.Content.FormattedText;

            int 插入结束位置 = 当前光标范围.End;
            if (插入结束位置 < 插入开始位置)
            {
                int 临时值 = 插入开始位置;
                插入开始位置 = 插入结束位置;
                插入结束位置 = 临时值;
            }

            Range 插入范围 = 目标文档.Range(插入开始位置, 插入结束位置);
            当前光标范围.Collapse(WdCollapseDirection.wdCollapseStart);
            return 插入范围;
        }
    }
}
