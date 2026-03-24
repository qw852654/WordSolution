using System.Collections.Generic;
using 题库核心.题目模块.领域;

namespace 题库应用.试卷导入模块
{
    public class 当前导入题目结果
    {
        public int 试卷记录ID { get; set; }

        public int 试卷题目项ID { get; set; }

        public int 草稿题序号 { get; set; }

        public string 题号文本 { get; set; } = string.Empty;

        public string 题目摘要 { get; set; } = string.Empty;

        public string 题目预览Html { get; set; } = string.Empty;

        public int? 推荐题型ID { get; set; }

        public string? 推荐题型名称 { get; set; }

        public string 识别说明 { get; set; } = string.Empty;

        public double 置信度 { get; set; }

        public IReadOnlyList<题型定义> 可选题型列表 { get; set; } = new List<题型定义>();

        public string 原始难度文本 { get; set; } = string.Empty;

        public IReadOnlyList<知识点映射展示项> 知识点列表 { get; set; } = new List<知识点映射展示项>();

        public IReadOnlyList<int> 预填标签ID列表 { get; set; } = new List<int>();

        public int 剩余数量 { get; set; }
    }
}
