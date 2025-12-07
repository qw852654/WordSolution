using System.IO;

namespace TagRunner
{
    public static class 文件布局
    {
        public static string SourcePath(string rootDir, string id) => Path.Combine(rootDir, "source", id + ".docx");
        public static string HtmlPath(string rootDir, string id) => Path.Combine(rootDir, "html", id + ".html");
        public static string PdfPath(string rootDir, string id) => Path.Combine(rootDir, "pdf", id + ".pdf");
    }
}