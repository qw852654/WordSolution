using Aspose.Words;

namespace 题库基础设施.Aspose
{
    public static class 文档清理帮助类
    {
        public static void 清理页眉页脚(Document 文档)
        {
            foreach (Section 节 in 文档.Sections)
            {
                节.HeadersFooters.Clear();
            }
        }
    }
}
