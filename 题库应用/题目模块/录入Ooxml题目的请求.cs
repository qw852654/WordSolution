using System.Collections.Generic;

namespace 题库应用.题目模块
{
    public class 录入Ooxml题目的请求
    {
        public string? Description { get; set; }

        public int 题型ID { get; set; }

        public List<int> 标签ID列表 { get; set; } = new();

        public string Ooxml内容 { get; set; } = string.Empty;
    }
}
