namespace 题库应用.标签模块
{
    public class 更新标签的请求
    {
        public int 标签种类ID { get; set; }

        public string 名称 { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? NumericValue { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
