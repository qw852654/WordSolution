using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;

namespace 题库基础设施.题库实例
{
    public class 题库DbContext工厂
    {
        private readonly 题库路径提供器 _题库路径提供器;

        public 题库DbContext工厂(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        public 题库DbContext 创建(string 题库键)
        {
            var 数据库文件路径 = _题库路径提供器.获取数据库文件路径(题库键);
            var optionsBuilder = new DbContextOptionsBuilder<题库DbContext>();
            optionsBuilder.UseSqlite($"Data Source={数据库文件路径}");
            return new 题库DbContext(optionsBuilder.Options);
        }
    }
}
