namespace 题库应用.试卷导入模块
{
    public class 试卷记录列表项结果
    {
        public int 试卷记录ID { get; set; }

        public string 显示名称 { get; set; } = string.Empty;

        public int 年份标签ID { get; set; }

        public string 年份标签名称 { get; set; } = string.Empty;

        public int 来源标签ID { get; set; }

        public string 来源标签名称 { get; set; } = string.Empty;

        public int 总题数 { get; set; }

        public int 已确认数 { get; set; }

        public int 已跳过数 { get; set; }

        public string 状态 { get; set; } = string.Empty;
    }
}
