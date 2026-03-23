using System;
using System.Collections.Generic;
using System.Linq;

namespace 题库应用.题目模块.题型识别.题型特征
{
    public class 选择题规则
    {
        private static readonly string[] 内置弱关键词 = { "正确的是", "错误的是", "不正确的是", "符合题意", "说法正确", "（ ）", "( )", "（）" };

        public (bool 命中, double 置信度, string 说明) 判断(题型特征集 特征, IReadOnlyList<string> 关键词)
        {
            if (特征.完整ABCD结构组数 != 1)
            {
                return (false, 0, "未检测到且仅检测到一组完整ABCD结构。");
            }

            var 命中弱关键词 = 获取全部关键词(关键词).Count(关键词项 => 特征.归一化文本.Contains(关键词项, StringComparison.OrdinalIgnoreCase));
            var 置信度 = 0.88 + Math.Min(0.08, 命中弱关键词 * 0.02);
            return (true, Math.Min(0.96, 置信度), 命中弱关键词 > 0 ? "检测到一组完整ABCD结构，并命中选择题弱特征。" : "检测到一组完整ABCD结构。");
        }

        private static IReadOnlyList<string> 获取全部关键词(IReadOnlyList<string> 外部关键词)
        {
            return 内置弱关键词.Concat(外部关键词 ?? Array.Empty<string>()).Distinct().ToList();
        }
    }
}
