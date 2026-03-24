using Aspose.Words;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 试卷段落信息
    {
        public int 索引 { get; set; }

        public Paragraph 段落 { get; set; } = null!;

        public string 文本 { get; set; } = string.Empty;

        public bool 是答案区 { get; set; }
    }
}
