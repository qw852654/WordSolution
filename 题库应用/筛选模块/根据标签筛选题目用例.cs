using System.Collections.Generic;
using System.Linq;
using 题库核心.筛选模块.领域;
using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库应用.筛选模块
{
    public class 根据标签筛选题目用例
    {
        private readonly I题目仓储 _题目仓储;

        public 根据标签筛选题目用例(I题目仓储 题目仓储)
        {
            _题目仓储 = 题目仓储;
        }

        public IReadOnlyList<题目> 执行(IReadOnlyList<筛选步骤> 筛选步骤列表)
        {
            if (筛选步骤列表 == null || 筛选步骤列表.Count == 0)
            {
                return new List<题目>();
            }

            var 当前结果 = new List<题目>();
            var 是否第一步 = true;

            foreach (var 步骤 in 筛选步骤列表)
            {
                if (步骤.标签ID列表.Count == 0)
                {
                    continue;
                }

                var 本步结果 = _题目仓储
                    .根据标签查找(步骤.标签ID列表, 步骤.本步标签组合方式)
                    .ToList();

                if (是否第一步)
                {
                    当前结果 = 本步结果;
                    是否第一步 = false;
                    continue;
                }

                当前结果 = 合并结果(当前结果, 本步结果, 步骤.与前一步结果组合方式);
            }

            return 当前结果;
        }

        private List<题目> 合并结果(
            List<题目> 当前结果,
            List<题目> 本步结果,
            组合方式 组合方式)
        {
            if (组合方式 == 组合方式.并集)
            {
                return 当前结果
                    .Concat(本步结果)
                    .GroupBy(题目 => 题目.Id)
                    .Select(分组 => 分组.First())
                    .ToList();
            }

            var 本步题目ID集合 = 本步结果
                .Select(题目 => 题目.Id)
                .ToHashSet();

            return 当前结果
                .Where(题目 => 本步题目ID集合.Contains(题目.Id))
                .ToList();
        }
    }
}
