using 题库核心.内容块模块.领域;

namespace 题库应用.小节模块
{
    public class 添加小节项的请求
    {
        public int 内容块ID { get; set; }

        public int? 内容块版本ID { get; set; }

        public 内容块引用版本模式? 引用版本模式 { get; set; }

        public string? 角色 { get; set; }

        public int? 排序 { get; set; }
    }
}
