using System;

namespace 题库核心.试卷导入模块.领域
{
    public class 试卷源文件记录
    {
        private 试卷源文件记录()
        {
        }

        private 试卷源文件记录(int id, int 试卷记录ID, string 原始文件名, string 存储相对路径, DateTime 导入时间)
        {
            Id = id;
            this.试卷记录ID = 试卷记录ID;
            this.原始文件名 = 原始文件名;
            this.存储相对路径 = 存储相对路径;
            this.导入时间 = 导入时间;
        }

        public int Id { get; private set; }

        public int 试卷记录ID { get; private set; }

        public string 原始文件名 { get; private set; } = string.Empty;

        public string 存储相对路径 { get; private set; } = string.Empty;

        public DateTime 导入时间 { get; private set; }

        public static 试卷源文件记录 创建(int 试卷记录ID, string 原始文件名, string 存储相对路径, DateTime 导入时间)
        {
            return new 试卷源文件记录(0, 试卷记录ID, 原始文件名, 存储相对路径, 导入时间);
        }

        public static 试卷源文件记录 从持久化恢复(int id, int 试卷记录ID, string 原始文件名, string 存储相对路径, DateTime 导入时间)
        {
            return new 试卷源文件记录(id, 试卷记录ID, 原始文件名, 存储相对路径, 导入时间);
        }
    }
}
