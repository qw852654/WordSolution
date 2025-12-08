using System;
using System.IO;

namespace TagRunner
{
    /// <summary>
    /// 文件库助手：构造时提供题库根目录，按约定生成不同类型文件的规范路径。
    /// 目录结构：
    ///   root/source/{id}.docx
    ///   root/html/{id}.html
    ///   root/pdf/{id}.pdf
    /// </summary>
    public class 文件库助手
    {
        public string 根目录 { get; }

        public string 源目录 => Path.Combine(根目录, "source");
        public string Html目录 => Path.Combine(根目录, "html");
        public string Pdf目录 => Path.Combine(根目录, "pdf");

        /// <summary>
        /// 构造函数，提供题库根目录。不会自动创建目录，除非调用 EnsureDirectories。
        /// </summary>
        public 文件库助手(string rootDir)
        {
            if (string.IsNullOrWhiteSpace(rootDir))
                throw new ArgumentException("根目录不能为空", nameof(rootDir));
            根目录 = rootDir;
        }

        /// <summary>
        /// 可选：确保 source/html/pdf 目录存在。
        /// </summary>
        public void EnsureDirectories()
        {
            Directory.CreateDirectory(源目录);
            Directory.CreateDirectory(Html目录);
            Directory.CreateDirectory(Pdf目录);
        }

        /// <summary>
        /// 获取源 DOCX 文件路径（存在性校验）。
        /// </summary>
        public string GetDocxPath(string id)
        {
            var path = Path.Combine(源目录, 规范化id格式(id) + ".docx");
            if (!File.Exists(path))
                throw new FileNotFoundException("源 DOCX 文件不存在", path);
            return path;
        }

        /// <summary>
        /// 获取 HTML 文件路径（存在性校验）。
        /// </summary>
        public string GetHtmlPath(string id)
        {
            var path = Path.Combine(Html目录, 规范化id格式(id) + ".html");
            if (!File.Exists(path))
                throw new FileNotFoundException("HTML 文件不存在", path);
            return path;
        }

        /// <summary>
        /// 获取 PDF 文件路径（存在性校验）。
        /// </summary>
        public string GetPdfPath(string id)
        {
            var path = Path.Combine(Pdf目录, 规范化id格式(id) + ".pdf");
            if (!File.Exists(path))
                throw new FileNotFoundException("PDF 文件不存在", path);
            return path;
        }

        /// <summary>
        /// 将输入 Id 进行规范化（去空格）。
        /// </summary>
        private static string 规范化id格式(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("文件 Id 不能为空", nameof(id));
            return id.Trim();
        }
    }
}
