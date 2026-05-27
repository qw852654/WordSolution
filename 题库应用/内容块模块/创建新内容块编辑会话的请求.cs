using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 创建新内容块编辑会话的请求
    {
        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public 内容块类型? 内容块类型 { get; set; }

        public 内容块状态? 内容块状态 { get; set; }

        public 内容块结构类型? 内容块结构类型 { get; set; }

        public bool? 是否允许子块 { get; set; }

        public bool? 是否打开Word { get; set; }
    }
}
