using System;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块详情结果
    {
        public int Id { get; set; }

        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public 内容块类型 类型 { get; set; }

        public 内容块状态 状态 { get; set; }

        public 内容块结构类型 结构类型 { get; set; }

        public bool 是否允许子块 { get; set; }

        public int? 当前版本ID { get; set; }

        public int? 当前版本号 { get; set; }

        public DateTime 创建时间 { get; set; }

        public DateTime 更新时间 { get; set; }

        public static 内容块详情结果 从内容块(内容块 内容块, 内容块版本? 当前版本)
        {
            return new 内容块详情结果
            {
                Id = 内容块.Id,
                标题 = 内容块.标题,
                摘要 = 内容块.摘要,
                类型 = 内容块.类型,
                状态 = 内容块.状态,
                结构类型 = 内容块.结构类型,
                是否允许子块 = 内容块.是否允许子块,
                当前版本ID = 内容块.当前版本ID,
                当前版本号 = 当前版本?.版本号,
                创建时间 = 内容块.创建时间,
                更新时间 = 内容块.更新时间,
            };
        }
    }
}
