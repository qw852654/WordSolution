using System;
using System.Collections.Generic;
using 题库核心.内容块模块.领域;
using 题库核心.小节模块.领域;

namespace 题库应用.小节模块
{
    public class 小节项结果
    {
        public int Id { get; set; }

        public int 小节ID { get; set; }

        public int 内容块ID { get; set; }

        public string 内容块标题 { get; set; } = string.Empty;

        public string? 内容块摘要 { get; set; }

        public 内容块类型 内容块类型 { get; set; }

        public 内容块状态 内容块状态 { get; set; }

        public 内容块结构类型 内容块结构类型 { get; set; }

        public int? RoleOptionId { get; set; }

        public string? RoleOptionName { get; set; }

        public int? DifficultyOptionId { get; set; }

        public string? DifficultyOptionName { get; set; }

        public int? UsageOptionId { get; set; }

        public string? UsageOptionName { get; set; }

        public int? QuestionTypeOptionId { get; set; }

        public string? QuestionTypeOptionName { get; set; }

        public bool DefaultIncluded { get; set; } = true;

        public string? Note { get; set; }

        public int? 内容块当前版本ID { get; set; }

        public int? 内容块版本ID { get; set; }

        public int? 引用版本ID { get; set; }

        public int? 引用版本号 { get; set; }

        public 内容块引用版本模式 引用版本模式 { get; set; }

        public string? 角色 { get; set; }

        public int 排序 { get; set; }

        public DateTime 创建时间 { get; set; }

        public static 小节项结果 从小节项(
            小节项 小节项,
            内容块 内容块,
            内容块版本? 引用版本,
            IReadOnlyDictionary<int, 元数据选项>? 元数据选项字典 = null)
        {
            return new 小节项结果
            {
                Id = 小节项.Id,
                小节ID = 小节项.小节ID,
                内容块ID = 小节项.内容块ID,
                内容块标题 = 内容块.标题,
                内容块摘要 = 内容块.摘要,
                内容块类型 = 内容块.类型,
                内容块状态 = 内容块.状态,
                内容块结构类型 = 内容块.结构类型,
                RoleOptionId = 内容块.RoleOptionId,
                RoleOptionName = 获取选项名称(内容块.RoleOptionId, 元数据选项字典),
                DifficultyOptionId = 内容块.DifficultyOptionId,
                DifficultyOptionName = 获取选项名称(内容块.DifficultyOptionId, 元数据选项字典),
                UsageOptionId = 内容块.UsageOptionId,
                UsageOptionName = 获取选项名称(内容块.UsageOptionId, 元数据选项字典),
                QuestionTypeOptionId = 内容块.QuestionTypeOptionId,
                QuestionTypeOptionName = 获取选项名称(内容块.QuestionTypeOptionId, 元数据选项字典),
                DefaultIncluded = 内容块.DefaultIncluded,
                Note = 内容块.Note,
                内容块当前版本ID = 内容块.当前版本ID,
                内容块版本ID = 小节项.内容块版本ID,
                引用版本ID = 引用版本?.Id,
                引用版本号 = 引用版本?.版本号,
                引用版本模式 = 小节项.引用版本模式,
                角色 = 小节项.角色,
                排序 = 小节项.排序,
                创建时间 = 小节项.创建时间,
            };
        }

        private static string? 获取选项名称(int? 选项ID, IReadOnlyDictionary<int, 元数据选项>? 元数据选项字典)
        {
            if (!选项ID.HasValue || 元数据选项字典 == null)
            {
                return null;
            }

            return 元数据选项字典.TryGetValue(选项ID.Value, out var 选项) ? 选项.Name : null;
        }
    }
}
