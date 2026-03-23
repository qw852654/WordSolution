namespace 题库核心.题目模块.领域
{
    public class 题型定义
    {
        private 题型定义()
        {
        }

        private 题型定义(int id, string 名称, string? 描述, int 排序值)
        {
            Id = id;
            this.名称 = 名称;
            描述 = string.IsNullOrWhiteSpace(描述) ? null : 描述;
            this.排序值 = 排序值;
        }

        public int Id { get; private set; }

        public string 名称 { get; private set; } = string.Empty;

        public string? 描述 { get; private set; }

        public int 排序值 { get; private set; }

        public static 题型定义 创建(string 名称, string? 描述, int 排序值)
        {
            return new 题型定义(0, 名称, 描述, 排序值);
        }

        public static 题型定义 从持久化恢复(int id, string 名称, string? 描述, int 排序值)
        {
            return new 题型定义(id, 名称, 描述, 排序值);
        }
    }
}
