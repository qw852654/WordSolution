using System.IO;
using System.Linq;
using 题库基础设施.题库实例;
using 题库核心.标签模块.领域;

namespace 题库基础设施.初始化
{
    public class 题库实例初始化器
    {
        private readonly 题库路径提供器 _题库路径提供器;
        private readonly 题库DbContext工厂 _题库DbContext工厂;

        public 题库实例初始化器(
            题库路径提供器 题库路径提供器,
            题库DbContext工厂 题库DbContext工厂)
        {
            _题库路径提供器 = 题库路径提供器;
            _题库DbContext工厂 = 题库DbContext工厂;
        }

        public void 确保题库已初始化(string 题库键)
        {
            var 题库根目录 = _题库路径提供器.获取题库根目录(题库键);
            Directory.CreateDirectory(题库根目录);
            Directory.CreateDirectory(_题库路径提供器.获取Source目录(题库键));
            Directory.CreateDirectory(_题库路径提供器.获取Html目录(题库键));
            Directory.CreateDirectory(_题库路径提供器.获取Index目录(题库键));

            using var dbContext = _题库DbContext工厂.创建(题库键);
            dbContext.Database.EnsureCreated();

            初始化标签种类(dbContext);
            初始化难度种子数据(dbContext);
        }

        private void 初始化标签种类(数据访问.题库DbContext dbContext)
        {
            if (dbContext.标签种类表.Any())
            {
                return;
            }

            dbContext.标签种类表.AddRange(
                标签种类.创建(系统标签种类.章节, "章节", true, true, true, true),
                标签种类.创建(系统标签种类.做题方法, "做题方法", true, true, true, true),
                标签种类.创建(系统标签种类.难度, "难度", false, false, true, true),
                标签种类.创建(系统标签种类.附加标签, "附加标签", false, true, true, true),
                标签种类.创建(系统标签种类.待整理, "待整理", true, true, true, false));

            dbContext.SaveChanges();
        }

        private void 初始化难度种子数据(数据访问.题库DbContext dbContext)
        {
            var 难度标签已存在 = dbContext.标签表.Any(标签 => 标签.标签种类ID == 系统标签种类.难度);
            if (难度标签已存在)
            {
                return;
            }

            dbContext.标签表.AddRange(
                标签.创建标签(系统标签种类.难度, "典型", null, null, 0, 0, true),
                标签.创建标签(系统标签种类.难度, "基础", null, null, 1, 1, true),
                标签.创建标签(系统标签种类.难度, "提高", null, null, 2, 2, true),
                标签.创建标签(系统标签种类.难度, "拔尖", null, null, 3, 3, true));

            dbContext.SaveChanges();
        }
    }
}
