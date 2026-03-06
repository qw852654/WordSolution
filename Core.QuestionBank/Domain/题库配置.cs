using System;
using System.IO;

namespace Core.QuestionBank.Domain
{
    /// <summary>
    /// 题库配置：表示一个题库实例的根目录与子目录约定。
    /// 当前约定：
    /// - source 目录：存放题目的源 docx
    /// - html 目录：存放转换后的 html
    /// - index 目录：存放索引文件（若以后实现索引）
    /// - 数据库文件：题库的元数据（若以后启用 SQLite）
    /// </summary>
    public class 题库配置
    {
        public string 根目录 { get; }

        public string 源目录名 { get; }
        public string Html目录名 { get; }
        public string 索引目录名 { get; }

        public string 数据库文件名 { get; }

        public string 源目录路径 => Path.Combine(根目录, 源目录名);
        public string Html目录路径 => Path.Combine(根目录, Html目录名);
        public string 索引目录路径 => Path.Combine(根目录, 索引目录名);
        public string 数据库文件路径 => Path.Combine(根目录, 数据库文件名);

        public 题库配置(string 根目录,
                      string 数据库文件名 = "tagrunner.db",
                      string 源目录名 = "source",
                      string Html目录名 = "html",
                      string 索引目录名 = "index")
        {
            if (string.IsNullOrWhiteSpace(根目录))
                throw new ArgumentException("题库根目录不能为空", nameof(根目录));

            this.根目录 = Path.GetFullPath(根目录);
            this.数据库文件名 = string.IsNullOrWhiteSpace(数据库文件名) ? "tagrunner.db" : 数据库文件名;
            this.源目录名 = string.IsNullOrWhiteSpace(源目录名) ? "source" : 源目录名;
            this.Html目录名 = string.IsNullOrWhiteSpace(Html目录名) ? "html" : Html目录名;
            this.索引目录名 = string.IsNullOrWhiteSpace(索引目录名) ? "index" : 索引目录名;
        }

        /// <summary>
        /// 验证并（可选）创建所需的目录（根目录、source/html/index）。
        /// </summary>
        public string 初始化目录(bool 如果不存在就创建 = true)
        {
            if (!Directory.Exists(根目录))
            {
                if (如果不存在就创建)
                    Directory.CreateDirectory(根目录);
                else
                    throw new DirectoryNotFoundException($"题库根目录不存在: {根目录}");
            }

            if (如果不存在就创建)
            {
                Directory.CreateDirectory(源目录路径);
                Directory.CreateDirectory(Html目录路径);
                Directory.CreateDirectory(索引目录路径);
            }
            else
            {
                if (!Directory.Exists(源目录路径) || !Directory.Exists(Html目录路径))
                    throw new DirectoryNotFoundException("题库的子目录缺失，请先初始化或设置 创建如果不存在=true。");
            }

            return 根目录;
        }

        public string 获取Docx目录(int 题目Id)
        {
            if (题目Id <= 0) throw new ArgumentException("题目Id必须为正整数", nameof(题目Id));
            return Path.Combine(源目录路径, $"{题目Id}.docx");
        }

        public string 获取Html目录(int 题目Id)
        {
            if (题目Id <= 0) throw new ArgumentException("题目Id必须为正整数", nameof(题目Id));
            return Path.Combine(Html目录路径, $"{题目Id}.html");
        }
    }
}
