using 题库核心.内容块模块.领域;

namespace 题库应用.引用关系模块
{
    public class 旧版本引用结果
    {
        public int 内容块ID { get; set; }

        public string 内容块标题 { get; set; } = string.Empty;

        public int? 当前版本ID { get; set; }

        public int? 当前版本号 { get; set; }

        public string 引用类型 { get; set; } = string.Empty;

        public int 引用对象ID { get; set; }

        public string 引用对象标题 { get; set; } = string.Empty;

        public int? 引用项ID { get; set; }

        public 内容块引用版本模式 引用版本模式 { get; set; }

        public int? 锁定内容块版本ID { get; set; }

        public int? 锁定版本号 { get; set; }

        public string 引用链 { get; set; } = string.Empty;
    }
}
