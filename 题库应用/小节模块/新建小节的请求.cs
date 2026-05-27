using 题库核心.小节模块.领域;

namespace 题库应用.小节模块
{
    public class 新建小节的请求
    {
        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public int? 章节标签ID { get; set; }

        public 小节状态 状态 { get; set; } = 小节状态.草稿;
    }
}
