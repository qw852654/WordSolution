using System.Collections.Generic;

namespace 题库本地服务.试卷导入模块
{
    public class 确认导入题目的请求
    {
        public int 试卷题目项ID { get; set; }

        public int 题型ID { get; set; }

        public int 难度标签ID { get; set; }

        public List<int> 最终标签ID列表 { get; set; } = new();

        public List<知识点映射决策请求> 新建知识点映射列表 { get; set; } = new();
    }
}
