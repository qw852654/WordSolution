using System.Collections.Generic;

namespace 题库应用.内容块模块
{
    public class 调整内容块子块排序的请求
    {
        public IList<内容块子项排序项> 子项排序列表 { get; set; } = new List<内容块子项排序项>();
    }

    public class 内容块子项排序项
    {
        public int 子项ID { get; set; }

        public int 排序 { get; set; }
    }
}
