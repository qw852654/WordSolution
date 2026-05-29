using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 新增元数据选项的请求
    {
        public 元数据选项类别 Category { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? SortOrder { get; set; }
    }
}
