using System.Collections.Generic;

namespace 题库应用.内容块模块
{
    public class 内容块结构树结果
    {
        public 内容块详情结果 内容块 { get; set; } = new 内容块详情结果();

        public 内容块子项结果? 来源子项 { get; set; }

        public int 深度 { get; set; }

        public bool 已达到最大深度 { get; set; }

        public IList<内容块结构树结果> 子块列表 { get; set; } = new List<内容块结构树结果>();
    }
}
