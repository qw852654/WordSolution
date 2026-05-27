using System;
using 题库核心.内容块模块.领域;
using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 讲义项结果
    {
        public int Id { get; set; }

        public int 讲义ID { get; set; }

        public 讲义项目标类型 目标类型 { get; set; }

        public int 目标ID { get; set; }

        public string 目标标题 { get; set; } = string.Empty;

        public string? 目标摘要 { get; set; }

        public 内容块引用版本模式 引用版本模式 { get; set; }

        public int? 锁定内容块版本ID { get; set; }

        public int? 引用版本号 { get; set; }

        public string? 角色 { get; set; }

        public int 排序 { get; set; }

        public DateTime 创建时间 { get; set; }

        public static 讲义项结果 从讲义项(
            讲义项 讲义项,
            string 目标标题,
            string? 目标摘要,
            内容块版本? 引用版本)
        {
            return new 讲义项结果
            {
                Id = 讲义项.Id,
                讲义ID = 讲义项.讲义ID,
                目标类型 = 讲义项.目标类型,
                目标ID = 讲义项.目标ID,
                目标标题 = 目标标题,
                目标摘要 = 目标摘要,
                引用版本模式 = 讲义项.引用版本模式,
                锁定内容块版本ID = 讲义项.锁定内容块版本ID,
                引用版本号 = 引用版本?.版本号,
                角色 = 讲义项.角色,
                排序 = 讲义项.排序,
                创建时间 = 讲义项.创建时间
            };
        }
    }
}
