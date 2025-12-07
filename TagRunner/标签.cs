using System.Collections.Generic;

namespace TagRunner
{
    public class 标签
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }       // null 表示根
        public List<标签> Children { get; set; } = new List<标签>();
    }
}
