using System.Collections.Generic;

namespace TagRunner
{
    public class 标签
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }       // null 表示根
        public List<标签> Children { get; set; } = new List<标签>();

        // 新增：题目类型与难度归属到标签
        public string Type { get; set; }         // 如 "选择题"/"计算题"；可为空意味着未指定
        public int? Difficulty { get; set; }     // 1..5；null 表示未指定
    }
}
