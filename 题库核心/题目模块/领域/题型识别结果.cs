namespace 题库核心.题目模块.领域
{
    public class 题型识别结果
    {
        public string 推荐题型名称 { get; set; } = string.Empty;

        public double 置信度 { get; set; }

        public string 说明 { get; set; } = string.Empty;
    }
}
