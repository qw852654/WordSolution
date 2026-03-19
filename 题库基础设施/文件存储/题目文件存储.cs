using System;
using System.IO;
using 题库核心.题目模块.契约;

namespace 题库基础设施.文件存储
{
    public class 题目文件存储 : I题目文件存储
    {
        private readonly string _题库根目录;

        public 题目文件存储(string 题库根目录)
        {
            if (string.IsNullOrWhiteSpace(题库根目录))
            {
                throw new ArgumentException("题库根目录不能为空。", nameof(题库根目录));
            }

            _题库根目录 = 题库根目录;
        }

        public string 保存题目文件(int 题目ID, byte[] 文件内容, string 文件扩展名)
        {
            if (文件内容 == null || 文件内容.Length == 0)
            {
                throw new ArgumentException("文件内容不能为空。", nameof(文件内容));
            }

            var 题目文件路径 = 获取题目文件路径(题目ID, 文件扩展名);
            var 目录路径 = Path.GetDirectoryName(题目文件路径);

            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            File.WriteAllBytes(题目文件路径, 文件内容);
            return 题目文件路径;
        }

        public string 获取题目文件路径(int 题目ID, string 文件扩展名 = ".docx")
        {
            return Path.Combine(_题库根目录, "source", $"{题目ID}{规范化文件扩展名(文件扩展名)}");
        }

        public string 获取题目预览文件路径(int 题目ID)
        {
            return Path.Combine(_题库根目录, "html", $"{题目ID}.html");
        }

        public byte[]? 读取题目文件(int 题目ID, string 文件扩展名 = ".docx")
        {
            var 题目文件路径 = 获取题目文件路径(题目ID, 文件扩展名);
            if (!File.Exists(题目文件路径))
            {
                return null;
            }

            return File.ReadAllBytes(题目文件路径);
        }

        public string? 读取题目预览HTML(int 题目ID)
        {
            var HTML文件路径 = 获取题目预览文件路径(题目ID);
            if (!File.Exists(HTML文件路径))
            {
                return null;
            }

            return File.ReadAllText(HTML文件路径);
        }

        private string 规范化文件扩展名(string 文件扩展名)
        {
            if (string.IsNullOrWhiteSpace(文件扩展名))
            {
                return ".docx";
            }

            if (文件扩展名.StartsWith("."))
            {
                return 文件扩展名;
            }

            return "." + 文件扩展名;
        }
    }
}
