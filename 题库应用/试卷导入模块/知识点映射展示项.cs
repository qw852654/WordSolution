namespace 题库应用.试卷导入模块
{
    public class 知识点映射展示项
    {
        public string 原始知识点文本 { get; set; } = string.Empty;

        public bool 是否已解决 { get; set; }

        public int? 目标标签ID { get; set; }

        public string? 目标标签名称 { get; set; }

        public bool 是否抛弃 { get; set; }
    }
}
