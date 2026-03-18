using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Word;

namespace Word本地文件操作核心库.工具
{
    public static class 文档路径工具
    {
        public static string 获取文档完整本地路径(Document 文档)
        {
            if (文档 == null) throw new ArgumentNullException(nameof(文档));
            if (string.IsNullOrWhiteSpace(文档.FullName))
                throw new InvalidOperationException("当前文档没有有效路径。");

            return 转换成本地路径(文档.FullName);
        }

        public static string 获取文档所在目录(Document 文档)
        {
            string 完整路径 = 获取文档完整本地路径(文档);
            string 目录 = Path.GetDirectoryName(完整路径);
            if (string.IsNullOrWhiteSpace(目录))
                throw new InvalidOperationException("无法获取文档所在目录。");

            return 目录;
        }

        public static string 生成输出文件路径(string 输出目录, string 文件名不带扩展名, string 后缀)
        {
            if (string.IsNullOrWhiteSpace(输出目录))
                throw new ArgumentException("输出目录不能为空。", nameof(输出目录));
            if (string.IsNullOrWhiteSpace(文件名不带扩展名))
                throw new ArgumentException("文件名不能为空。", nameof(文件名不带扩展名));
            if (string.IsNullOrWhiteSpace(后缀))
                throw new ArgumentException("后缀不能为空。", nameof(后缀));

            return Path.Combine(输出目录, 文件名不带扩展名 + 后缀);
        }

        private static string 转换成本地路径(string 路径)
        {
            if (路径.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string onedrive本地目录 = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\OneDrive\";
                string 小写路径 = 路径.ToLowerInvariant();
                小写路径 = Regex.Replace(小写路径, @"^https://d\.docs\.live\.net/[0-9a-f]+/", onedrive本地目录.ToLowerInvariant());
                return 小写路径.Replace("/", "\\");
            }

            return 路径;
        }
    }
}

