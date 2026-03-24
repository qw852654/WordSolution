namespace 题库本地服务.试卷导入模块
{
    public class 知识点映射决策请求
    {
        public string 原始知识点文本 { get; set; } = string.Empty;

        public int? 目标标签ID { get; set; }

        public bool 是否抛弃 { get; set; }
    }
}
