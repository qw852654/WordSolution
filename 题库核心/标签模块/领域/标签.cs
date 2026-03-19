using System;
using System.Collections.Generic;

namespace 题库核心.标签模块.领域
{
    public class 标签
    {
        private readonly List<标签> _子标签列表 = new();

        private 标签()
        {
        }

        private 标签(
            int id,
            int 标签种类ID,
            string 名称,
            string? description,
            int? parentId,
            int 同级排序值,
            decimal? numericValue,
            bool isEnabled)
        {
            Id = id;
            更新标签(标签种类ID, 名称, description, parentId, 同级排序值, numericValue, isEnabled);
        }

        public int Id { get; private set; }

        public int 标签种类ID { get; private set; }

        public string 名称 { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public int? ParentId { get; private set; }

        public int 同级排序值 { get; private set; }

        public decimal? NumericValue { get; private set; }

        public bool IsEnabled { get; private set; }

        public IReadOnlyList<标签> 子标签列表 => _子标签列表;

        public static 标签 创建标签(
            int 标签种类ID,
            string 名称,
            string? description,
            int? parentId,
            int 同级排序值,
            decimal? numericValue,
            bool isEnabled)
        {
            return new 标签(0, 标签种类ID, 名称, description, parentId, 同级排序值, numericValue, isEnabled);
        }

        public static 标签 从持久化恢复标签(
            int id,
            int 标签种类ID,
            string 名称,
            string? description,
            int? parentId,
            int 同级排序值,
            decimal? numericValue,
            bool isEnabled)
        {
            return new 标签(id, 标签种类ID, 名称, description, parentId, 同级排序值, numericValue, isEnabled);
        }

        public void 更新标签(
            int 标签种类ID,
            string 名称,
            string? description,
            int? parentId,
            int 同级排序值,
            decimal? numericValue,
            bool isEnabled)
        {
            if (string.IsNullOrWhiteSpace(名称))
            {
                throw new ArgumentException("名称不能为空。", nameof(名称));
            }

            this.标签种类ID = 标签种类ID;
            this.名称 = 名称.Trim();
            Description = description;
            ParentId = parentId;
            this.同级排序值 = 同级排序值;
            NumericValue = numericValue;
            IsEnabled = isEnabled;
        }

        public void 清空子标签()
        {
            _子标签列表.Clear();
        }

        public void 添加子标签(标签 子标签)
        {
            _子标签列表.Add(子标签);
        }
    }
}
