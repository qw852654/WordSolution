using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库基础设施.文件存储;
using 题库核心.标签模块.领域;

namespace 题库测试.测试支持
{
    public abstract class 集成测试基础类 : IDisposable
    {
        private readonly string _临时目录路径;
        private readonly string _数据库文件路径;

        protected 集成测试基础类()
        {
            _临时目录路径 = Path.Combine(Path.GetTempPath(), "题库测试", Guid.NewGuid().ToString("N"));
            _数据库文件路径 = Path.Combine(_临时目录路径, "题库测试.db");
            Directory.CreateDirectory(_临时目录路径);
        }

        protected string 临时目录路径 => _临时目录路径;

        protected 题库DbContext 创建数据库上下文()
        {
            var options = new DbContextOptionsBuilder<题库DbContext>()
                .UseSqlite($"Data Source={_数据库文件路径}")
                .Options;

            var 数据库 = new 题库DbContext(options);
            数据库.Database.EnsureCreated();
            return 数据库;
        }

        protected 题目文件存储 创建题目文件存储()
        {
            return new 题目文件存储(_临时目录路径);
        }

        protected void 保存标签(params 标签[] 标签列表)
        {
            using var 数据库 = 创建数据库上下文();
            数据库.标签表.AddRange(标签列表);
            数据库.SaveChanges();
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(_临时目录路径))
            {
                Directory.Delete(_临时目录路径, true);
            }
        }
    }
}
