namespace 题库应用.题目模块
{
    public class 预览Ooxml题目的结果
    {
        public string 预览Html { get; set; } = string.Empty;

        public int? 推荐题型ID { get; set; }

        public string? 推荐题型名称 { get; set; }

        public string 识别说明 { get; set; } = string.Empty;

        public double 置信度 { get; set; }
    }
}
