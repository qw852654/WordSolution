using System;
using System.Collections.Generic;

namespace 题库应用.题目模块
{
    public class 录入题目的请求
    {
        public string? Description { get; set; }

        public List<int> 标签ID列表 { get; set; } = new();

        public string 文件扩展名 { get; set; } = ".docx";

        public byte[] 题目文件内容 { get; set; } = Array.Empty<byte>();
    }
}
