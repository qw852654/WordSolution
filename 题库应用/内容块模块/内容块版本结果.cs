using System;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块版本结果
    {
        public int Id { get; set; }

        public int 内容块ID { get; set; }

        public int 版本号 { get; set; }

        public DateTime 创建时间 { get; set; }

        public bool 是否当前版本 { get; set; }

        public static 内容块版本结果 从版本(内容块版本 版本)
        {
            return new 内容块版本结果
            {
                Id = 版本.Id,
                内容块ID = 版本.内容块ID,
                版本号 = 版本.版本号,
                创建时间 = 版本.创建时间,
                是否当前版本 = 版本.是否当前版本,
            };
        }
    }
}
