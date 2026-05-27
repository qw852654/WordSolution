using System.Collections.Generic;

namespace 题库应用.引用关系模块
{
    public class 内容块引用影响结果
    {
        public int 内容块ID { get; set; }

        public string 内容块标题 { get; set; } = string.Empty;

        public int? 当前版本ID { get; set; }

        public int? 当前版本号 { get; set; }

        public int 组合块引用数量 { get; set; }

        public int 小节引用数量 { get; set; }

        public int 讲义引用数量 { get; set; }

        public int 锁定旧版本数量 { get; set; }

        public IReadOnlyList<引用位置结果> 引用位置列表 { get; set; } = new List<引用位置结果>();

        public IReadOnlyList<旧版本引用结果> 旧版本引用列表 { get; set; } = new List<旧版本引用结果>();
    }
}
