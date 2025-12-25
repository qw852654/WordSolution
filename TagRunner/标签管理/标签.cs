using System;
using System.Collections.Generic;

namespace TagRunner
{
    public class 标签 : IEquatable<标签>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }       // null 表示根
        public List<标签> Children { get; set; } = new List<标签>();

        // 类别：如 "题型"、"难度"、"知识点" 等
        public string Category { get; set; }

        // 数值语义（用于类别=难度 等场景），其它类别保持为 null
        public int? NumericValue { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as 标签);
        }

        public bool Equals(标签 other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public static class 标签类别
    {
        public static readonly IReadOnlyCollection<string> 允许的根标签类别 = new[] {
            "题型",
            "难度",
            "章节",
            "思维方法",
            "特定问题类型",
            "来源",
            "其他",
            "无标签题目"
        };

        public static bool 是允许的类别(string category)
        {
            if(category==null) throw new System.ArgumentNullException("没有提供类别");
            foreach(var c in 允许的根标签类别)
            {
                if(c==category) return true;
            }
            return false;
        }
    }

}