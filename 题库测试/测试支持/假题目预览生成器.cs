using System.IO;
using 题库核心.题目模块.契约;

namespace 题库测试.测试支持
{
    public class 假题目预览生成器 : I题目预览生成器
    {
        public void 生成HTML预览(string 题目文件路径, string HTML文件路径)
        {
            var 目录路径 = Path.GetDirectoryName(HTML文件路径);
            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            File.WriteAllText(HTML文件路径, "<html><body>测试预览</body></html>");
        }
    }
}
