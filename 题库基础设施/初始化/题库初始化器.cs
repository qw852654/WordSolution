using System.IO;
using 题库基础设施.数据访问;

namespace 题库基础设施.初始化
{
    public class 题库初始化器
    {
        private readonly 题库DbContext _题库DbContext;
        private readonly string _题库根目录;
        private readonly string _文件存储根目录;

        public 题库初始化器(
            题库DbContext 题库DbContext,
            string 题库根目录,
            string 文件存储根目录)
        {
            _题库DbContext = 题库DbContext;
            _题库根目录 = 题库根目录;
            _文件存储根目录 = 文件存储根目录;
        }

        public bool 题库是否存在()
        {
            return File.Exists(获取数据库文件路径());
        }

        public void 初始化题库()
        {
            if (题库是否存在())
            {
                return;
            }

            Directory.CreateDirectory(_题库根目录);
            Directory.CreateDirectory(_文件存储根目录);
            Directory.CreateDirectory(Path.Combine(_文件存储根目录, "source"));
            Directory.CreateDirectory(Path.Combine(_文件存储根目录, "html"));

            _题库DbContext.Database.EnsureCreated();
        }

        private string 获取数据库文件路径()
        {
            return Path.Combine(_题库根目录, "question-bank.db");
        }
    }
}
