using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 更新讲义的请求
    {
        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public 讲义状态? 状态 { get; set; }
    }
}
