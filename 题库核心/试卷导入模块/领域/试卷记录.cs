namespace 题库核心.试卷导入模块.领域
{
    public class 试卷记录
    {
        private 试卷记录()
        {
        }

        private 试卷记录(
            int id,
            int 年份标签ID,
            int 来源标签ID,
            string 显示名称,
            int 总题数,
            int 已确认数,
            int 已跳过数,
            试卷导入状态 状态)
        {
            Id = id;
            this.年份标签ID = 年份标签ID;
            this.来源标签ID = 来源标签ID;
            this.显示名称 = 显示名称;
            this.总题数 = 总题数;
            this.已确认数 = 已确认数;
            this.已跳过数 = 已跳过数;
            this.状态 = 状态;
        }

        public int Id { get; private set; }

        public int 年份标签ID { get; private set; }

        public int 来源标签ID { get; private set; }

        public string 显示名称 { get; private set; } = string.Empty;

        public int 总题数 { get; private set; }

        public int 已确认数 { get; private set; }

        public int 已跳过数 { get; private set; }

        public 试卷导入状态 状态 { get; private set; } = 试卷导入状态.导入中;

        public static 试卷记录 创建(int 年份标签ID, int 来源标签ID, string 显示名称)
        {
            return new 试卷记录(0, 年份标签ID, 来源标签ID, 显示名称, 0, 0, 0, 试卷导入状态.导入中);
        }

        public static 试卷记录 从持久化恢复(
            int id,
            int 年份标签ID,
            int 来源标签ID,
            string 显示名称,
            int 总题数,
            int 已确认数,
            int 已跳过数,
            试卷导入状态 状态)
        {
            return new 试卷记录(id, 年份标签ID, 来源标签ID, 显示名称, 总题数, 已确认数, 已跳过数, 状态);
        }

        public void 更新显示名称(string 显示名称)
        {
            if (!string.IsNullOrWhiteSpace(显示名称))
            {
                this.显示名称 = 显示名称.Trim();
            }
        }

        public void 设置总题数(int 总题数)
        {
            if (总题数 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(总题数));
            }

            this.总题数 = 总题数;
            刷新状态();
        }

        public void 标记已确认一题()
        {
            已确认数++;
            刷新状态();
        }

        public void 标记已跳过一题()
        {
            已跳过数++;
            刷新状态();
        }

        private void 刷新状态()
        {
            状态 = 总题数 > 0 && 已确认数 + 已跳过数 >= 总题数
                ? 试卷导入状态.已完成
                : 试卷导入状态.导入中;
        }
    }
}
