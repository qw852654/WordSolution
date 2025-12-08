using System.Collections.Generic;

namespace TagRunner
{
    public class 标签
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }       // null 表示根
        public List<标签> Children { get; set; } = new List<标签>();

        // 类别：如 "题型"、"难度"、"知识点" 等
        public string Category { get; set; } 

        // 数值语义（用于类别=难度 等场景），其它类别保持为 null
        public int? NumericValue { get; set; }
    }
}
