using System.Collections.Generic;

namespace 题库应用.内容块模块
{
    public class 更新内容块标签的请求
    {
        public IReadOnlyList<int> 标签ID列表 { get; set; } = new List<int>();
    }
}
