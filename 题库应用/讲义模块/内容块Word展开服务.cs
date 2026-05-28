using System;
using System.Collections.Generic;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.讲义模块.契约;

namespace 题库应用.讲义模块
{
    public class 内容块Word展开服务
    {
        private readonly I内容块仓储 _内容块仓储;

        public 内容块Word展开服务(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public void 展开内容块(
            int 内容块ID,
            内容块引用版本模式 引用版本模式,
            int? 内容块版本ID,
            string? 角色,
            List<讲义生成源文件> 源文件列表,
            List<Word生成版本清单项> 版本清单)
        {
            展开内容块(
                内容块ID,
                引用版本模式,
                内容块版本ID,
                角色,
                源文件列表,
                版本清单,
                new HashSet<int>());
        }

        private void 展开内容块(
            int 内容块ID,
            内容块引用版本模式 引用版本模式,
            int? 内容块版本ID,
            string? 角色,
            List<讲义生成源文件> 源文件列表,
            List<Word生成版本清单项> 版本清单,
            HashSet<int> 路径内容块ID集合)
        {
            if (!路径内容块ID集合.Add(内容块ID))
            {
                throw new InvalidOperationException("Word 生成时检测到内容块循环引用。");
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
                版本清单.Add(new Word生成版本清单项
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
    }

    public class Word生成版本清单项
    {
        public int 内容块ID { get; set; }

        public string 内容块标题 { get; set; } = string.Empty;

        public int 内容块版本ID { get; set; }

        public int 版本号 { get; set; }

        public string 引用版本模式 { get; set; } = string.Empty;

        public string 文件路径 { get; set; } = string.Empty;
    }
}
