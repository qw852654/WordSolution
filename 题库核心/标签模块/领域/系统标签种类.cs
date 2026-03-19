namespace 题库核心.标签模块.领域
{
    public static class 系统标签种类
    {
        public const int 章节 = 1;
        public const int 做题方法 = 2;
        public const int 难度 = 3;
        public const int 附加标签 = 4;
        public const int 待整理 = 5;

        public static bool 是否树形(int 标签种类ID)
        {
            return 标签种类ID == 章节 || 标签种类ID == 做题方法 || 标签种类ID == 待整理;
        }

        public static bool 是否允许多选(int 标签种类ID)
        {
            return 标签种类ID != 难度;
        }

        public static bool 是否正式工作流可见(int 标签种类ID)
        {
            return 标签种类ID != 待整理;
        }
    }
}
