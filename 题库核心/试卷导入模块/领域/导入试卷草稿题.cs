using System.Collections.Generic;

namespace 题库核心.试卷导入模块.领域
{
    public class 导入试卷草稿题
    {
        public int 序号 { get; set; }

        public string 题号文本 { get; set; } = string.Empty;

        public string 题目摘要 { get; set; } = string.Empty;

        public string 完整Ooxml内容 { get; set; } = string.Empty;

        public string 题目正文Ooxml内容 { get; set; } = string.Empty;

        public string 原始难度文本 { get; set; } = string.Empty;

        public List<string> 原始知识点列表 { get; set; } = new();

        public int? 推荐题型ID { get; set; }

        public string? 推荐题型名称 { get; set; }

        public string 识别说明 { get; set; } = string.Empty;

        public double 置信度 { get; set; }

        public bool 已跳过 { get; set; }
    }
}
