using System.Collections.Generic;

namespace 题库应用.小节模块
{
    public class 调整小节项排序的请求
    {
        public IList<小节项排序项> 项目排序列表 { get; set; } = new List<小节项排序项>();
    }

    public class 小节项排序项
    {
        public int 小节项ID { get; set; }

        public int 排序 { get; set; }
    }
}
