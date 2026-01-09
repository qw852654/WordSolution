using System;
using System.IO;

namespace TagRunner.Models
{
    /// <summary>
    /// 题库配置：表示一个题库实例的根目录与子目录约定。
    /// 通过构造函数创建实例，Bootstrapper 负责使用该实例进行初始化（建表、创建目录等）。
    /// </summary>
    public class 题库配置
    {
        // 根目录（绝对路径）
        public string 根目录 { get; }

        // 子目录名称（相对于 根目录）
        public string 源目录名 { get; }
        public string Html目录名 { get; }
        public string Pdf目录名 { get; }
        public string 索引目录名 { get; }

        // 数据库文件名（相对于 根目录）
        public string 数据库文件名 { get; }

        // 计算属性：各目录的绝对路径与数据库文件的绝对路径
        public string 源目录路径 => Path.Combine(根目录, 源目录名);
        public string Html目录路径 => Path.Combine(根目录, Html目录名);
        public string Pdf目录路径 => Path.Combine(根目录, Pdf目录名);
        public string 索引目录路径 => Path.Combine(根目录, 索引目录名);
        public string 数据库文件路径 => Path.Combine(根目录, 数据库文件名);

        /// <summary>
        /// 构造函数：指定根目录，支持可选的子目录与数据库文件名。
        /// 构造时会规范化根目录为绝对路径，但不会自动创建目录；由 Bootstrapper 调用 初始化目录 方法来决定是否创建。
        /// </summary>
        public 题库配置(string 根目录,
                      string 数据库文件名 = "tagrunner.db",
                      string 源目录名 = "source",
                      string Html目录名 = "html",
                      string Pdf目录名 = "pdf",
                      string 索引目录名 = "index")
        {
            if (string.IsNullOrWhiteSpace(根目录))
                throw new ArgumentException("题库根目录不能为空", nameof(根目录));

            this.根目录 = Path.GetFullPath(根目录);
            this.数据库文件名 = string.IsNullOrWhiteSpace(数据库文件名) ? "tagrunner.db" : 数据库文件名;
            this.源目录名 = string.IsNullOrWhiteSpace(源目录名) ? "source" : 源目录名;
            this.Html目录名 = string.IsNullOrWhiteSpace(Html目录名) ? "html" : Html目录名;
            this.Pdf目录名 = string.IsNullOrWhiteSpace(Pdf目录名) ? "pdf" : Pdf目录名;
            this.索引目录名 = string.IsNullOrWhiteSpace(索引目录名) ? "index" : 索引目录名;
        }

        /// <summary>
        /// 验证并（可选）创建所需的目录（根目录、source/html/pdf/index）。
        /// 返回已确保存在的根目录路径。
        /// </summary>
        public string 初始化目录(bool 创建如果不存在 = true)
        {
            if (!Directory.Exists(根目录))
            {
                if (创建如果不存在)
                    Directory.CreateDirectory(根目录);
                else
                    throw new DirectoryNotFoundException($"题库根目录不存在: {根目录}");
            }

            if (创建如果不存在)
            {
                Directory.CreateDirectory(源目录路径);
                Directory.CreateDirectory(Html目录路径);
                Directory.CreateDirectory(Pdf目录路径);
                Directory.CreateDirectory(索引目录路径);
            }
            else
            {
                if (!Directory.Exists(源目录路径) || !Directory.Exists(Html目录路径) || !Directory.Exists(Pdf目录路径))
                    throw new DirectoryNotFoundException("题库的子目录缺失，请先初始化或设置 创建如果不存在=true。");
            }

            return 根目录;
        }

        /// <summary>
        /// 返回题目对应的 Docx 文件路径（绝对路径）。
        /// </summary>
        public string 获取Docx路径(int 题目Id)
        {
            if (题目Id <= 0) throw new ArgumentException("题目Id必须为正整数", nameof(题目Id));
            return Path.Combine(源目录路径, $"{题目Id}.docx");
        }

        /// <summary>
        /// 返回题目对应的 Html 文件路径（绝对路径）。
        /// </summary>
        public string 获取Html路径(int 题目Id)
        {
            if (题目Id <= 0) throw new ArgumentException("题目Id必须为正整数", nameof(题目Id));
            return Path.Combine(Html目录路径, $"{题目Id}.html");
        }

        /// <summary>
        /// 返回题目对应的 Pdf 文件路径（绝对路径）。
        /// </summary>
        public string 获取Pdf路径(int 题目Id)
        {
            if (题目Id <= 0) throw new ArgumentException("题目Id必须为正整数", nameof(题目Id));
            return Path.Combine(Pdf目录路径, $"{题目Id}.pdf");
        }
    }
}
