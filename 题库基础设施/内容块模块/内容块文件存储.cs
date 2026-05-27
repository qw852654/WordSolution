using System;
using System.IO;
using 题库基础设施.题库实例;
using 题库核心.内容块模块.契约;

namespace 题库基础设施.内容块模块
{
    public class 内容块文件存储 : I内容块文件存储
    {
        private readonly 题库路径提供器 _题库路径提供器;

        public 内容块文件存储(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        public string 获取内容块文件路径(int 内容块ID, int 版本号, string 文件扩展名 = ".docx")
        {
            var 当前题库键 = _题库路径提供器.获取当前请求题库键();
            return 获取内容块文件路径(当前题库键, 内容块ID, 版本号, 文件扩展名);
        }

        public string 获取内容块文件路径(string 题库键, int 内容块ID, int 版本号, string 文件扩展名 = ".docx")
        {
            return Path.Combine(
                获取内容块Source根目录(题库键),
                内容块ID.ToString(),
                $"v{版本号}{规范化文件扩展名(文件扩展名)}");
        }

        public string 获取内容块预览文件路径(int 内容块ID, int 版本号)
        {
            var 当前题库键 = _题库路径提供器.获取当前请求题库键();
            return 获取内容块预览文件路径(当前题库键, 内容块ID, 版本号);
        }

        public string 获取内容块预览文件路径(string 题库键, int 内容块ID, int 版本号)
        {
            return Path.Combine(
                获取内容块Html根目录(题库键),
                内容块ID.ToString(),
                $"v{版本号}.html");
        }

        public byte[]? 读取内容块文件(string 文件路径)
        {
            if (string.IsNullOrWhiteSpace(文件路径) || !File.Exists(文件路径))
            {
                return null;
            }

            return File.ReadAllBytes(文件路径);
        }

        public string? 读取内容块预览HTML(string HTML文件路径)
        {
            if (string.IsNullOrWhiteSpace(HTML文件路径) || !File.Exists(HTML文件路径))
            {
                return null;
            }

            return File.ReadAllText(HTML文件路径);
        }

        private string 获取内容块Source根目录(string 题库键)
        {
            return Path.Combine(_题库路径提供器.获取题库根目录(题库键), "content-blocks", "source");
        }

        private string 获取内容块Html根目录(string 题库键)
        {
            return Path.Combine(_题库路径提供器.获取题库根目录(题库键), "content-blocks", "html");
        }

        private string 规范化文件扩展名(string 文件扩展名)
        {
            if (string.IsNullOrWhiteSpace(文件扩展名))
            {
                return ".docx";
            }

            return 文件扩展名.StartsWith(".", StringComparison.Ordinal)
                ? 文件扩展名
                : "." + 文件扩展名;
        }
    }
}
