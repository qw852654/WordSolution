namespace 题库核心.试卷导入模块.领域
{
    public class 知识点映射
    {
        private 知识点映射()
        {
        }

        private 知识点映射(int id, string 原始文本, string 归一化原始文本, int? 目标标签ID, bool 是否抛弃)
        {
            Id = id;
            this.原始文本 = 原始文本;
            this.归一化原始文本 = 归一化原始文本;
            this.目标标签ID = 目标标签ID;
            this.是否抛弃 = 是否抛弃;
        }

        public int Id { get; private set; }

        public string 原始文本 { get; private set; } = string.Empty;

        public string 归一化原始文本 { get; private set; } = string.Empty;

        public int? 目标标签ID { get; private set; }

        public bool 是否抛弃 { get; private set; }

        public static 知识点映射 创建(string 原始文本, string 归一化原始文本, int? 目标标签ID, bool 是否抛弃)
        {
            return new 知识点映射(0, 原始文本, 归一化原始文本, 目标标签ID, 是否抛弃);
        }

        public static 知识点映射 从持久化恢复(int id, string 原始文本, string 归一化原始文本, int? 目标标签ID, bool 是否抛弃)
        {
            return new 知识点映射(id, 原始文本, 归一化原始文本, 目标标签ID, 是否抛弃);
        }
    }
}
