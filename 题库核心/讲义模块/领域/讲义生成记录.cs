using System;

namespace 题库核心.讲义模块.领域
{
    public class 讲义生成记录
    {
        private 讲义生成记录()
        {
        }

        private 讲义生成记录(
            int id,
            int 讲义ID,
            string 文件路径,
            string? 版本清单Json,
            DateTime 生成时间)
        {
            Id = id;
            this.讲义ID = 讲义ID;
            this.文件路径 = 文件路径;
            this.版本清单Json = 版本清单Json;
            this.生成时间 = 生成时间;
        }

        public int Id { get; private set; }

        public int 讲义ID { get; private set; }

        public string 文件路径 { get; private set; } = string.Empty;

        public string? 版本清单Json { get; private set; }

        public DateTime 生成时间 { get; private set; }

        public static 讲义生成记录 创建(int 讲义ID, string 文件路径, string? 版本清单Json)
        {
            if (讲义ID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(讲义ID));
            }

            if (string.IsNullOrWhiteSpace(文件路径))
            {
                throw new ArgumentException("文件路径不能为空。", nameof(文件路径));
            }

            return new 讲义生成记录(0, 讲义ID, 文件路径, 版本清单Json, DateTime.Now);
        }

        public static 讲义生成记录 从持久化恢复(int id, int 讲义ID, string 文件路径, string? 版本清单Json, DateTime 生成时间)
        {
            return new 讲义生成记录(id, 讲义ID, 文件路径, 版本清单Json, 生成时间);
        }
    }
}
