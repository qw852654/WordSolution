namespace 题库基础设施.题库实例
{
    public class 题库实例信息
    {
        public string 题库键 { get; set; } = string.Empty;

        public string 显示名称 { get; set; } = string.Empty;

        public bool 是否已初始化 { get; set; }

        public int 题目数量 { get; set; }

        public int 标签数量 { get; set; }
    }
}
