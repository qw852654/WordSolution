namespace 题库应用.题目模块.题型识别
{
    public class 根据Ooxml识别题型结果
    {
        public int? 推荐题型ID { get; set; }

        public string? 推荐题型名称 { get; set; }

        public double 置信度 { get; set; }

        public string 说明 { get; set; } = string.Empty;
    }
}
