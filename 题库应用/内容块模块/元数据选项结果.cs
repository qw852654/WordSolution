using System;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 元数据选项结果
    {
        public int Id { get; set; }

        public 元数据选项类别 Category { get; set; }

        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime UpdatedTime { get; set; }

        public static 元数据选项结果 从选项(元数据选项 选项)
        {
            return new 元数据选项结果
            {
                Id = 选项.Id,
                Category = 选项.Category,
                Name = 选项.Name,
                SortOrder = 选项.SortOrder,
                IsActive = 选项.IsActive,
                CreatedTime = 选项.CreatedTime,
                UpdatedTime = 选项.UpdatedTime,
            };
        }
    }
}
