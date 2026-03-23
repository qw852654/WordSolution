using System.Collections.Generic;

namespace 题库应用.题目模块
{
    public class 获取下一道待识别题型题目结果
    {
        public int 题目ID { get; set; }

        public string? 描述 { get; set; }

        public string 预览Html { get; set; } = string.Empty;

        public int? 题型ID { get; set; }

        public int? 推荐题型ID { get; set; }

        public string? 推荐题型名称 { get; set; }

        public string 识别说明 { get; set; } = string.Empty;

        public double 置信度 { get; set; }

        public List<题型选项项> 可选题型列表 { get; set; } = new();

        public int 剩余未设置题型数量 { get; set; }
    }
}
