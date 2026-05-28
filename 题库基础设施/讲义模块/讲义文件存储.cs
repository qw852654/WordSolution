using System.IO;
using 题库基础设施.题库实例;
using 题库核心.讲义模块.契约;

namespace 题库基础设施.讲义模块
{
    public class 讲义文件存储 : I讲义文件存储
    {
        private readonly 题库路径提供器 _题库路径提供器;

        public 讲义文件存储(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        public string 获取讲义生成文件路径(int 讲义ID, string 文件名)
        {
            var 当前题库键 = _题库路径提供器.获取当前请求题库键();
            var 目录 = Path.Combine(_题库路径提供器.获取题库根目录(当前题库键), "handouts", "generated", 讲义ID.ToString());
            Directory.CreateDirectory(目录);
            return Path.Combine(目录, 文件名);
        }

        public string 获取小节导出文件路径(int 小节ID, string 文件名)
        {
            var 当前题库键 = _题库路径提供器.获取当前请求题库键();
            var 目录 = Path.Combine(_题库路径提供器.获取题库根目录(当前题库键), "sections", "exported", 小节ID.ToString());
            Directory.CreateDirectory(目录);
            return Path.Combine(目录, 文件名);
        }

        public byte[]? 读取生成文件(string 文件路径)
        {
            if (string.IsNullOrWhiteSpace(文件路径) || !File.Exists(文件路径))
            {
                return null;
            }

            return File.ReadAllBytes(文件路径);
        }
    }
}
