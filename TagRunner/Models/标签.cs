using System;
using System.Collections.Generic;

namespace TagRunner.Models
{
    /// <summary>
    /// 领域模型：标签（移动到 Models 目录，保留英文字段以便与数据库列映射）。
    /// </summary>
    public class 标签 : IEquatable<标签>
    {
        // 原始（与数据库列名保持一致，便于 Dapper 映射）
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; } //null表示无父节点
        public List<标签> Children { get; set; } = new List<标签>();

        // 描述信息
        public string Description { get; set; } 

        // 在父节点下的顺序位置（可用于快速排序或显示）；如果需要链式首尾相接，也可以使用 PrevSiblingId/NextSiblingId
        public int? PrevSiblingId { get; set; }
        public int? NextSiblingId { get; set; }


        public override bool Equals(object obj)
        {
            return Equals(obj as 标签);
        }

        public bool Equals(标签 other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            // 对于未持久化（Id == 0）的对象，按引用区分，避免不同未保存对象被认为相等
            if (Id == 0 || other.Id == 0) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            // 对于未持久化对象使用基类哈希，已持久化使用 Id 的哈希
            return Id == 0 ? base.GetHashCode() : Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? $"标签:{Id}" : $"{Name}({Id})";
        }
    }
}
