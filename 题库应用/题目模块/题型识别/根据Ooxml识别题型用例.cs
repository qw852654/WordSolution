using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库应用.题目模块.题型识别
{
    public class 根据Ooxml识别题型用例
    {
        private readonly I题型识别器 _题型识别器;

        public 根据Ooxml识别题型用例(I题型识别器 题型识别器)
        {
            _题型识别器 = 题型识别器;
        }

        public 根据Ooxml识别题型结果 执行(string ooxml内容, IReadOnlyList<题型定义> 当前题型定义列表)
        {
            if (string.IsNullOrWhiteSpace(ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(ooxml内容));
            }

            var 标准结果 = _题型识别器.识别(ooxml内容, 当前题型定义列表);
            var 推荐题型 = 当前题型定义列表.FirstOrDefault(题型 => string.Equals(题型.名称, 标准结果.推荐题型名称, StringComparison.OrdinalIgnoreCase));

            if (推荐题型 == null && string.Equals(标准结果.推荐题型名称, "填空题", StringComparison.OrdinalIgnoreCase))
            {
                推荐题型 = 当前题型定义列表.FirstOrDefault(题型 => string.Equals(题型.名称, "实验题", StringComparison.OrdinalIgnoreCase));
            }

            return new 根据Ooxml识别题型结果
            {
                推荐题型ID = 推荐题型?.Id,
                推荐题型名称 = 推荐题型?.名称 ?? 标准结果.推荐题型名称,
                置信度 = 标准结果.置信度,
                说明 = 标准结果.说明,
            };
        }
    }
}
