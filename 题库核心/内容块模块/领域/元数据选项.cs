using System;

namespace 题库核心.内容块模块.领域
{
    public class 元数据选项
    {
        private 元数据选项()
        {
        }

        private 元数据选项(
            int id,
            元数据选项类别 category,
            string name,
            int sortOrder,
            bool isActive,
            DateTime createdTime,
            DateTime updatedTime)
        {
            Id = id;
            Category = category;
            Name = name;
            SortOrder = sortOrder;
            IsActive = isActive;
            CreatedTime = createdTime;
            UpdatedTime = updatedTime;
        }

        public int Id { get; private set; }

        public 元数据选项类别 Category { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public int SortOrder { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public DateTime UpdatedTime { get; private set; }

        public static 元数据选项 创建(元数据选项类别 category, string name, int sortOrder, bool isActive = true)
        {
            校验类别(category);
            校验名称(name);
            var now = DateTime.Now;
            return new 元数据选项(0, category, name.Trim(), sortOrder, isActive, now, now);
        }

        public static 元数据选项 从持久化恢复(
            int id,
            元数据选项类别 category,
            string name,
            int sortOrder,
            bool isActive,
            DateTime createdTime,
            DateTime updatedTime)
        {
            return new 元数据选项(id, category, name, sortOrder, isActive, createdTime, updatedTime);
        }

        public void 修改(string name, int sortOrder)
        {
            校验名称(name);
            Name = name.Trim();
            SortOrder = sortOrder;
            UpdatedTime = DateTime.Now;
        }

        public void 启用()
        {
            IsActive = true;
            UpdatedTime = DateTime.Now;
        }

        public void 停用()
        {
            IsActive = false;
            UpdatedTime = DateTime.Now;
        }

        private static void 校验类别(元数据选项类别 category)
        {
            if (!Enum.IsDefined(typeof(元数据选项类别), category))
            {
                throw new ArgumentOutOfRangeException(nameof(category));
            }
        }

        private static void 校验名称(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("选项名称不能为空。", nameof(name));
            }
        }
    }
}
