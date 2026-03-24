using System;
using System.IO;
using System.Linq;
using 题库基础设施.题库实例;
using 题库核心.试卷导入模块.契约;

namespace 题库基础设施.文件存储
{
    public class 试卷源文件存储 : I试卷源文件存储
    {
        private readonly 题库路径提供器 _题库路径提供器;

        public 试卷源文件存储(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        public (string 相对路径, string 绝对路径) 保存源文件(int 试卷记录ID, string 原始文件名, byte[] 文件内容)
        {
            if (文件内容 == null || 文件内容.Length == 0)
            {
                throw new ArgumentException("试卷文件不能为空。", nameof(文件内容));
            }

            var 当前题库键 = _题库路径提供器.获取当前请求题库键();
            var 题库根目录 = _题库路径提供器.获取题库根目录(当前题库键);
            var 相对目录 = Path.Combine("papers", 试卷记录ID.ToString(), "sources");
            var 绝对目录 = Path.Combine(题库根目录, 相对目录);
            Directory.CreateDirectory(绝对目录);

            var 安全文件名 = 规范化文件名(原始文件名);
            var 最终文件名 = $"{DateTime.Now:yyyyMMddHHmmssfff}_{安全文件名}";
            var 绝对路径 = Path.Combine(绝对目录, 最终文件名);
            File.WriteAllBytes(绝对路径, 文件内容);

            return (Path.Combine(相对目录, 最终文件名), 绝对路径);
        }

        private static string 规范化文件名(string 文件名)
        {
            var 原始文件名 = string.IsNullOrWhiteSpace(文件名) ? "import.docx" : 文件名.Trim();
            var 无效字符集合 = Path.GetInvalidFileNameChars();
            var 安全字符数组 = 原始文件名
                .Select(字符 => 无效字符集合.Contains(字符) ? '_' : 字符)
                .ToArray();
            var 安全文件名 = new string(安全字符数组);
            return string.IsNullOrWhiteSpace(安全文件名) ? "import.docx" : 安全文件名;
        }
    }
}
