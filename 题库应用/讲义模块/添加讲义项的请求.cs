using 题库核心.内容块模块.领域;
using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 添加讲义项的请求
    {
        public 讲义项目标类型 目标类型 { get; set; }

        public int 目标ID { get; set; }

        public 内容块引用版本模式? 引用版本模式 { get; set; }

        public int? 锁定内容块版本ID { get; set; }

        public string? 角色 { get; set; }

        public int? 排序 { get; set; }
    }
}
