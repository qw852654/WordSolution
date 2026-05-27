using 题库核心.内容块模块.领域;

namespace 题库应用.引用关系模块
{
    public class 引用位置结果
    {
        public string 引用类型 { get; set; } = string.Empty;

        public int 引用对象ID { get; set; }

        public string 引用对象标题 { get; set; } = string.Empty;

        public int? 引用项ID { get; set; }

        public bool 是否直接引用 { get; set; }

        public string 引用链 { get; set; } = string.Empty;

        public 内容块引用版本模式? 引用版本模式 { get; set; }

        public int? 锁定内容块版本ID { get; set; }

        public int? 锁定版本号 { get; set; }

        public bool 是否锁定旧版本 { get; set; }

        public string 说明 { get; set; } = string.Empty;
    }
}
