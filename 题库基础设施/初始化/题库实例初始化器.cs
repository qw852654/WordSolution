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
            初始化试卷题型种子数据(dbContext);
        }

        private void 初始化标签种类(数据访问.题库DbContext dbContext)
        {
            确保标签种类存在(dbContext, 系统标签种类.章节, "章节", true, true, true, true);
            确保标签种类存在(dbContext, 系统标签种类.做题方法, "做题方法", true, true, true, true);
            确保标签种类存在(dbContext, 系统标签种类.难度, "难度", false, false, true, true);
            确保标签种类存在(dbContext, 系统标签种类.附加标签, "附加标签", false, true, true, true);
            确保标签种类存在(dbContext, 系统标签种类.待整理, "待整理", true, true, true, false);
            确保标签种类存在(dbContext, 系统标签种类.试卷题型, "试卷题型", false, false, true, true);

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

        private void 初始化试卷题型种子数据(数据访问.题库DbContext dbContext)
        {
            确保平铺标签存在(dbContext, 系统标签种类.试卷题型, "选择题", 0);
            确保平铺标签存在(dbContext, 系统标签种类.试卷题型, "填空题", 1);
            确保平铺标签存在(dbContext, 系统标签种类.试卷题型, "实验题", 2);
            确保平铺标签存在(dbContext, 系统标签种类.试卷题型, "解答题", 3);

            dbContext.SaveChanges();
        }

        private static void 确保标签种类存在(
            数据访问.题库DbContext dbContext,
            int 标签种类ID,
            string 名称,
            bool 是否树形,
            bool 是否允许多选,
            bool 是否系统内置,
            bool 是否在正式工作流中可见)
        {
            if (dbContext.标签种类表.Any(标签种类 => 标签种类.Id == 标签种类ID))
            {
                return;
            }

            dbContext.标签种类表.Add(
                标签种类.创建(标签种类ID, 名称, 是否树形, 是否允许多选, 是否系统内置, 是否在正式工作流中可见)
            );
        }

        private static void 确保平铺标签存在(
            数据访问.题库DbContext dbContext,
            int 标签种类ID,
            string 名称,
            int 同级排序值)
        {
            if (dbContext.标签表.Any(标签 => 标签.标签种类ID == 标签种类ID && 标签.ParentId == null && 标签.名称 == 名称))
            {
                return;
            }

            dbContext.标签表.Add(标签.创建标签(标签种类ID, 名称, null, null, 同级排序值, null, true));
        }
    }
}
