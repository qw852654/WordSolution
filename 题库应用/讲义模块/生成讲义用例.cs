using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;
using 题库核心.小节模块.契约;

namespace 题库应用.讲义模块
{
    public class 生成讲义用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly I小节仓储 _小节仓储;
        private readonly I内容块仓储 _内容块仓储;
        private readonly I讲义文件存储 _讲义文件存储;
        private readonly I讲义Word生成器 _讲义Word生成器;

        public 生成讲义用例(
            I讲义仓储 讲义仓储,
            I小节仓储 小节仓储,
            I内容块仓储 内容块仓储,
            I讲义文件存储 讲义文件存储,
            I讲义Word生成器 讲义Word生成器)
        {
            _讲义仓储 = 讲义仓储;
            _小节仓储 = 小节仓储;
            _内容块仓储 = 内容块仓储;
            _讲义文件存储 = 讲义文件存储;
            _讲义Word生成器 = 讲义Word生成器;
        }

        public 讲义生成记录结果? 执行(int 讲义ID)
        {
            var 讲义 = _讲义仓储.GetById(讲义ID);
            if (讲义 == null)
            {
                return null;
            }

            var 源文件列表 = new List<讲义生成源文件>();
            var 版本清单 = new List<讲义版本清单项>();
            var 讲义项列表 = _讲义仓储.获取讲义项列表(讲义ID);
            foreach (var 讲义项 in 讲义项列表)
            {
                if (讲义项.目标类型 == 讲义项目标类型.小节)
                {
                    展开小节(讲义项, 源文件列表, 版本清单);
                }
                else
                {
                    展开内容块(
                        讲义项.目标ID,
                        讲义项.引用版本模式,
                        讲义项.锁定内容块版本ID,
                        讲义项.角色,
                        源文件列表,
                        版本清单,
                        new HashSet<int>());
                }
            }

            var 文件名 = $"{清理文件名(讲义.标题)}-{DateTime.Now:yyyyMMddHHmmss}.docx";
            var 输出路径 = _讲义文件存储.获取讲义生成文件路径(讲义ID, 文件名);
            _讲义Word生成器.生成(讲义.标题, 源文件列表, 输出路径);

            var 版本清单Json = JsonSerializer.Serialize(版本清单, new JsonSerializerOptions { WriteIndented = true });
            var 生成记录 = 讲义生成记录.创建(讲义ID, 输出路径, 版本清单Json);
            _讲义仓储.增加生成记录(生成记录);
            return 讲义生成记录结果.从生成记录(生成记录);
        }

        private void 展开小节(
            讲义项 讲义项,
            List<讲义生成源文件> 源文件列表,
            List<讲义版本清单项> 版本清单)
        {
            var 小节 = _小节仓储.GetById(讲义项.目标ID);
            if (小节 == null)
            {
                throw new InvalidOperationException($"讲义中的小节 {讲义项.目标ID} 不存在。");
            }

            var 小节项列表 = _小节仓储.获取小节项列表(小节.Id);
            foreach (var 小节项 in 小节项列表)
            {
                展开内容块(
                    小节项.内容块ID,
                    小节项.引用版本模式,
                    小节项.内容块版本ID,
                    $"{讲义项.角色 ?? "小节"} / {小节项.角色 ?? "内容块"}",
                    源文件列表,
                    版本清单,
                    new HashSet<int>());
            }
        }

        private void 展开内容块(
            int 内容块ID,
            内容块引用版本模式 引用版本模式,
            int? 内容块版本ID,
            string? 角色,
            List<讲义生成源文件> 源文件列表,
            List<讲义版本清单项> 版本清单,
            HashSet<int> 路径内容块ID集合)
        {
            if (!路径内容块ID集合.Add(内容块ID))
            {
                throw new InvalidOperationException("讲义生成时检测到内容块循环引用。");
            }

            var 内容块 = _内容块仓储.GetById(内容块ID);
            if (内容块 == null)
            {
                throw new InvalidOperationException($"内容块 {内容块ID} 不存在。");
            }

            var 版本 = 获取引用版本(内容块, 引用版本模式, 内容块版本ID);
            if (版本 != null)
            {
                源文件列表.Add(new 讲义生成源文件
                {
                    标题 = 内容块.标题,
                    角色 = string.IsNullOrWhiteSpace(角色) ? 内容块.类型.ToString() : 角色!,
                    文件路径 = 版本.Docx路径
                });
                版本清单.Add(new 讲义版本清单项
                {
                    内容块ID = 内容块.Id,
                    内容块标题 = 内容块.标题,
                    内容块版本ID = 版本.Id,
                    版本号 = 版本.版本号,
                    引用版本模式 = 引用版本模式.ToString(),
                    文件路径 = 版本.Docx路径
                });
            }
            else if (!内容块.是否允许子块)
            {
                throw new InvalidOperationException($"内容块“{内容块.标题}”还没有可生成的 Word 版本。");
            }

            if (内容块.是否允许子块)
            {
                foreach (var 子项 in _内容块仓储.获取子项列表(内容块.Id))
                {
                    展开内容块(
                        子项.子内容块ID,
                        子项.引用版本模式,
                        子项.子内容块版本ID,
                        子项.角色 ?? 角色,
                        源文件列表,
                        版本清单,
                        路径内容块ID集合);
                }
            }

            路径内容块ID集合.Remove(内容块ID);
        }

        private 内容块版本? 获取引用版本(内容块 内容块, 内容块引用版本模式 引用版本模式, int? 内容块版本ID)
        {
            if (引用版本模式 == 内容块引用版本模式.跟随最新)
            {
                return _内容块仓储.获取当前版本(内容块.Id);
            }

            if (!内容块版本ID.HasValue)
            {
                throw new InvalidOperationException($"内容块“{内容块.标题}”使用锁定版本，但没有记录版本ID。");
            }

            var 版本 = _内容块仓储.获取版本(内容块版本ID.Value);
            if (版本 == null || 版本.内容块ID != 内容块.Id)
            {
                throw new InvalidOperationException($"内容块“{内容块.标题}”的锁定版本不存在。");
            }

            return 版本;
        }

        private static string 清理文件名(string 文件名)
        {
            var 清理后 = Path.GetInvalidFileNameChars()
                .Aggregate(文件名, (当前, 非法字符) => 当前.Replace(非法字符, '-'))
                .Trim();

            return string.IsNullOrWhiteSpace(清理后) ? "讲义" : 清理后;
        }

        private class 讲义版本清单项
        {
            public int 内容块ID { get; set; }

            public string 内容块标题 { get; set; } = string.Empty;

            public int 内容块版本ID { get; set; }

            public int 版本号 { get; set; }

            public string 引用版本模式 { get; set; } = string.Empty;

            public string 文件路径 { get; set; } = string.Empty;
        }
    }
}
