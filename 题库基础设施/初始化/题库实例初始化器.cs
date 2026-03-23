using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库基础设施.题库实例;
using 题库核心.标签模块.领域;
using 题库核心.题目模块.领域;

namespace 题库基础设施.初始化
{
    public class 题库实例初始化器
    {
        private const int 旧试卷题型标签种类ID = 6;
        private const string 旧试卷题型标签种类名称 = "试卷题型";

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

            确保题型表存在(dbContext);
            确保题目表包含题型列(dbContext);
            初始化标签种类(dbContext);
            初始化难度种子数据(dbContext);
            初始化题型定义(dbContext, 题库键);
            迁移旧试卷题型标签(dbContext, 题库键);
        }

        private void 初始化标签种类(题库DbContext dbContext)
        {
            确保标签种类存在(dbContext, 系统标签种类.章节, "章节", true, true, true, true);
            确保标签种类存在(dbContext, 系统标签种类.做题方法, "做题方法", true, true, true, true);
            确保标签种类存在(dbContext, 系统标签种类.难度, "难度", false, false, true, true);
            确保标签种类存在(dbContext, 系统标签种类.附加标签, "附加标签", false, true, true, true);
            确保标签种类存在(dbContext, 系统标签种类.年份, "年份", false, false, true, true);
            确保标签种类存在(dbContext, 系统标签种类.来源, "来源", false, false, true, true);
            确保标签种类存在(dbContext, 系统标签种类.待整理, "待整理", true, true, true, false);
            dbContext.SaveChanges();
        }

        private void 初始化难度种子数据(题库DbContext dbContext)
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

        private void 初始化题型定义(题库DbContext dbContext, string 题库键)
        {
            var 已有题型定义 = dbContext.题型定义表
                .AsNoTracking()
                .ToDictionary(题型 => 题型.名称, StringComparer.OrdinalIgnoreCase);

            foreach (var 预设 in 获取题库预设题型(题库键))
            {
                if (已有题型定义.ContainsKey(预设.名称))
                {
                    continue;
                }

                dbContext.题型定义表.Add(题型定义.创建(预设.名称, 预设.描述, 预设.排序值));
            }

            dbContext.SaveChanges();
        }

        private void 迁移旧试卷题型标签(题库DbContext dbContext, string 题库键)
        {
            var 旧标签种类 = dbContext.标签种类表
                .SingleOrDefault(标签种类 => 标签种类.Id == 旧试卷题型标签种类ID || 标签种类.名称 == 旧试卷题型标签种类名称);
            if (旧标签种类 == null)
            {
                return;
            }

            var 旧题型标签列表 = dbContext.标签表
                .Where(标签 => 标签.标签种类ID == 旧标签种类.Id)
                .ToList();
            if (旧题型标签列表.Count == 0)
            {
                dbContext.标签种类表.Remove(旧标签种类);
                dbContext.SaveChanges();
                return;
            }

            var 名称到新题型ID映射 = dbContext.题型定义表
                .AsNoTracking()
                .ToDictionary(题型 => 题型.名称, 题型 => 题型.Id, StringComparer.OrdinalIgnoreCase);
            var 旧标签字典 = 旧题型标签列表.ToDictionary(标签 => 标签.Id);
            var 旧标签ID列表 = 旧题型标签列表.Select(标签 => 标签.Id).ToList();
            var 旧关系列表 = dbContext.题目标签关系表
                .Where(关系 => 旧标签ID列表.Contains(关系.标签ID))
                .ToList();

            var 已迁移数量 = 0;
            var 冲突数量 = 0;
            var 无匹配数量 = 0;

            foreach (var 分组 in 旧关系列表.GroupBy(关系 => 关系.题目ID))
            {
                var 当前题目 = dbContext.题目表.SingleOrDefault(题目 => 题目.Id == 分组.Key);
                if (当前题目 == null || 当前题目.题型ID.HasValue)
                {
                    continue;
                }

                var 旧题型名称列表 = 分组
                    .Select(关系 => 旧标签字典[关系.标签ID].名称)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (旧题型名称列表.Count != 1)
                {
                    冲突数量++;
                    continue;
                }

                var 旧题型名称 = 旧题型名称列表[0];
                if (!名称到新题型ID映射.TryGetValue(旧题型名称, out var 新题型ID))
                {
                    无匹配数量++;
                    continue;
                }

                dbContext.Database.ExecuteSqlInterpolated($"UPDATE Questions SET TypeId = {新题型ID} WHERE Id = {分组.Key} AND TypeId IS NULL");
                已迁移数量++;
            }

            if (旧关系列表.Count > 0)
            {
                dbContext.题目标签关系表.RemoveRange(旧关系列表);
            }

            dbContext.标签表.RemoveRange(旧题型标签列表);
            dbContext.标签种类表.Remove(旧标签种类);
            dbContext.SaveChanges();

            Console.WriteLine($"题库 {题库键} 题型迁移完成：已迁移 {已迁移数量}，冲突 {冲突数量}，无匹配 {无匹配数量}。");
        }

        private void 确保题型表存在(题库DbContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS \"QuestionTypes\" (" +
                "\"Id\" INTEGER NOT NULL CONSTRAINT \"PK_QuestionTypes\" PRIMARY KEY AUTOINCREMENT," +
                "\"Name\" TEXT NOT NULL," +
                "\"Description\" TEXT NULL," +
                "\"SortOrder\" INTEGER NOT NULL);");
        }

        private void 确保题目表包含题型列(题库DbContext dbContext)
        {
            if (表列已存在(dbContext, "Questions", "TypeId"))
            {
                return;
            }

            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Questions\" ADD COLUMN \"TypeId\" INTEGER NULL;");
        }

        private bool 表列已存在(题库DbContext dbContext, string 表名, string 列名)
        {
            var connection = dbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
            {
                connection.Open();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info('{表名.Replace("'", "''")}')";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var 当前列名 = reader[1]?.ToString();
                    if (string.Equals(当前列名, 列名, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                if (shouldClose)
                {
                    connection.Close();
                }
            }
        }

        private IEnumerable<(string 名称, string? 描述, int 排序值)> 获取题库预设题型(string 题库键)
        {
            var 规范题库键 = _题库路径提供器.规范化题库键(题库键);
            if (string.Equals(规范题库键, "GZ", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    ("选择题", "关键词：正确的是,错误的是,不正确的是,符合题意,说法正确", 0),
                    ("实验题", "关键词：实验,探究,器材,步骤,误差,现象,分析,记录,量筒,天平,电流表,电压表,滑动变阻器,压强计", 1),
                    ("解答题", null, 2),
                    ("作图题", "关键词：画出,做出,作出", 3),
                };
            }

            return new[]
            {
                ("选择题", "关键词：正确的是,错误的是,不正确的是,符合题意,说法正确", 0),
                ("填空题", null, 1),
                ("实验题", "关键词：实验,探究,器材,步骤,误差,现象,分析,记录,量筒,天平,电流表,电压表,滑动变阻器,压强计", 2),
                ("解答题", null, 3),
                ("作图题", "关键词：画出,做出,作出", 4),
            };
        }

        private static void 确保标签种类存在(
            题库DbContext dbContext,
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
    }
}


