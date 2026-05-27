using System;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块子项结果
    {
        public int Id { get; set; }

        public int 父内容块ID { get; set; }

        public int 子内容块ID { get; set; }

        public string 子内容块标题 { get; set; } = string.Empty;

        public 内容块类型 子内容块类型 { get; set; }

        public 内容块状态 子内容块状态 { get; set; }

        public 内容块结构类型 子内容块结构类型 { get; set; }

        public bool 子内容块是否允许子块 { get; set; }

        public int? 子内容块当前版本ID { get; set; }

        public int? 子内容块版本ID { get; set; }

        public int? 引用版本ID { get; set; }

        public int? 引用版本号 { get; set; }

        public 内容块引用版本模式 引用版本模式 { get; set; }

        public string? 角色 { get; set; }

        public int 排序 { get; set; }

        public DateTime 创建时间 { get; set; }

        public static 内容块子项结果 从子项(内容块子项 子项, 内容块 子内容块, 内容块版本? 引用版本)
        {
            return new 内容块子项结果
            {
                Id = 子项.Id,
                父内容块ID = 子项.父内容块ID,
                子内容块ID = 子项.子内容块ID,
                子内容块标题 = 子内容块.标题,
                子内容块类型 = 子内容块.类型,
                子内容块状态 = 子内容块.状态,
                子内容块结构类型 = 子内容块.结构类型,
                子内容块是否允许子块 = 子内容块.是否允许子块,
                子内容块当前版本ID = 子内容块.当前版本ID,
                子内容块版本ID = 子项.子内容块版本ID,
                引用版本ID = 引用版本?.Id,
                引用版本号 = 引用版本?.版本号,
                引用版本模式 = 子项.引用版本模式,
                角色 = 子项.角色,
                排序 = 子项.排序,
                创建时间 = 子项.创建时间,
            };
        }
    }
}
