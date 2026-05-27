using System;
using System.IO;
using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 讲义生成记录结果
    {
        public int Id { get; set; }

        public int 讲义ID { get; set; }

        public string 文件名 { get; set; } = string.Empty;

        public DateTime 生成时间 { get; set; }

        public static 讲义生成记录结果 从生成记录(讲义生成记录 记录)
        {
            return new 讲义生成记录结果
            {
                Id = 记录.Id,
                讲义ID = 记录.讲义ID,
                文件名 = Path.GetFileName(记录.文件路径),
                生成时间 = 记录.生成时间
            };
        }
    }
}
