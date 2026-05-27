using System.Collections.Generic;

namespace 题库应用.讲义模块
{
    public class 调整讲义项排序的请求
    {
        public IReadOnlyList<int> 讲义项ID列表 { get; set; } = new List<int>();
    }
}
