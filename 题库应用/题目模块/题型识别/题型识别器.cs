using System;
using System.Collections.Generic;
using System.Linq;
using 题库应用.题目模块.题型识别.题型特征;
using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库应用.题目模块.题型识别
{
    public class 题型识别器 : I题型识别器
    {
        private static readonly string[] 内置实验关键词 =
        {
            "实验", "探究", "器材", "步骤", "误差", "现象", "分析", "记录", "量筒", "天平", "电流表", "电压表", "滑动变阻器", "压强计"
        };

        private static readonly string[] 内置作图关键词 = { "画出", "做出", "作出" };

        private readonly Ooxml题型特征提取器 _特征提取器;
        private readonly 选择题规则 _选择题规则;
        private readonly 填空题规则 _填空题规则;
        private readonly 实验题规则 _实验题规则;
        private readonly 解答题规则 _解答题规则;
        private readonly 作图题规则 _作图题规则;

        public 题型识别器(
            Ooxml题型特征提取器 特征提取器,
            选择题规则 选择题规则,
            填空题规则 填空题规则,
            实验题规则 实验题规则,
            解答题规则 解答题规则,
            作图题规则 作图题规则)
        {
            _特征提取器 = 特征提取器;
            _选择题规则 = 选择题规则;
            _填空题规则 = 填空题规则;
            _实验题规则 = 实验题规则;
            _解答题规则 = 解答题规则;
            _作图题规则 = 作图题规则;
        }

        public 题型识别结果 识别(string ooxml内容, IReadOnlyList<题型定义> 当前题型定义列表)
        {
            var 特征 = _特征提取器.提取(ooxml内容);
            var 实验关键词 = 获取关键词列表(当前题型定义列表, "实验题", 内置实验关键词);
            var 作图关键词 = 获取关键词列表(当前题型定义列表, "作图题", 内置作图关键词);
            var 选择题关键词 = 获取关键词列表(当前题型定义列表, "选择题", Array.Empty<string>());

            var 实验关键词命中数量 = 统计关键词命中数量(特征.归一化文本, 实验关键词);
            var 作图关键词命中数量 = 统计关键词命中数量(特征.归一化文本, 作图关键词);

            var 作图结果 = _作图题规则.判断(特征, 作图关键词命中数量);
            var 选择题结果 = _选择题规则.判断(特征, 选择题关键词);
            var 填空题结果 = _填空题规则.判断(特征, 实验关键词命中数量);
            var 实验题结果 = _实验题规则.判断(特征, 实验关键词命中数量);
            var 解答题结果 = _解答题规则.判断();

            if (实验题结果.命中 && 特征.完整ABCD结构组数 >= 2)
            {
                return new 题型识别结果
                {
                    推荐题型名称 = "实验题",
                    置信度 = 实验题结果.置信度,
                    说明 = 实验题结果.说明,
                };
            }

            var 候选结果 = new List<(string 名称, bool 命中, double 置信度, string 说明)>
            {
                ("作图题", 作图结果.命中, 作图结果.置信度, 作图结果.说明),
                ("选择题", 选择题结果.命中, 选择题结果.置信度, 选择题结果.说明),
                ("填空题", 填空题结果.命中, 填空题结果.置信度, 填空题结果.说明),
                ("实验题", 实验题结果.命中, 实验题结果.置信度, 实验题结果.说明),
                ("解答题", 解答题结果.命中, 解答题结果.置信度, 解答题结果.说明),
            };

            var 最佳结果 = 候选结果
                .Where(候选 => 候选.命中)
                .OrderByDescending(候选 => 候选.置信度)
                .First();

            return new 题型识别结果
            {
                推荐题型名称 = 最佳结果.名称,
                置信度 = 最佳结果.置信度,
                说明 = 最佳结果.说明,
            };
        }

        private static IReadOnlyList<string> 获取关键词列表(IReadOnlyList<题型定义> 当前题型定义列表, string 题型名称, IReadOnlyList<string> 内置关键词)
        {
            var 题型定义 = 当前题型定义列表.FirstOrDefault(题型 => string.Equals(题型.名称, 题型名称, StringComparison.OrdinalIgnoreCase));
            var 描述关键词 = 解析描述关键词(题型定义?.描述);
            return 内置关键词.Concat(描述关键词).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static IReadOnlyList<string> 解析描述关键词(string? 描述)
        {
            if (string.IsNullOrWhiteSpace(描述))
            {
                return Array.Empty<string>();
            }

            const string 前缀 = "关键词：";
            var 起始索引 = 描述.IndexOf(前缀, StringComparison.OrdinalIgnoreCase);
            if (起始索引 < 0)
            {
                return Array.Empty<string>();
            }

            var 关键词文本 = 描述[(起始索引 + 前缀.Length)..].Trim();
            if (关键词文本.Length == 0)
            {
                return Array.Empty<string>();
            }

            return 关键词文本
                .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(关键词 => 关键词.Trim())
                .Where(关键词 => 关键词.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int 统计关键词命中数量(string 文本, IReadOnlyList<string> 关键词列表)
        {
            return 关键词列表.Count(关键词 => 文本.Contains(关键词, StringComparison.OrdinalIgnoreCase));
        }
    }
}
