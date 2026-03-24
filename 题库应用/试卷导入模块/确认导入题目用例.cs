using System;
using System.Collections.Generic;
using System.Linq;
using 题库应用.题目模块;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库应用.试卷导入模块
{
    public class 确认导入题目用例
    {
        private readonly I试卷记录仓储 _试卷记录仓储;
        private readonly I试卷题目项仓储 _试卷题目项仓储;
        private readonly I标签仓储 _标签仓储;
        private readonly I知识点映射仓储 _知识点映射仓储;
        private readonly 题型规则校验器 _题型规则校验器;
        private readonly 题目标签规则校验器 _题目标签规则校验器;
        private readonly 录入Ooxml题目用例 _录入Ooxml题目用例;
        private readonly 获取当前导入题目用例 _获取当前导入题目用例;

        public 确认导入题目用例(
            I试卷记录仓储 试卷记录仓储,
            I试卷题目项仓储 试卷题目项仓储,
            I标签仓储 标签仓储,
            I知识点映射仓储 知识点映射仓储,
            题型规则校验器 题型规则校验器,
            题目标签规则校验器 题目标签规则校验器,
            录入Ooxml题目用例 录入Ooxml题目用例,
            获取当前导入题目用例 获取当前导入题目用例)
        {
            _试卷记录仓储 = 试卷记录仓储;
            _试卷题目项仓储 = 试卷题目项仓储;
            _标签仓储 = 标签仓储;
            _知识点映射仓储 = 知识点映射仓储;
            _题型规则校验器 = 题型规则校验器;
            _题目标签规则校验器 = 题目标签规则校验器;
            _录入Ooxml题目用例 = 录入Ooxml题目用例;
            _获取当前导入题目用例 = 获取当前导入题目用例;
        }

        public 当前导入题目结果? 执行(
            int 试卷记录ID,
            int 试卷题目项ID,
            int 题型ID,
            int 难度标签ID,
            IReadOnlyList<int> 最终标签ID列表,
            IReadOnlyList<知识点映射决策> 新建知识点映射列表)
        {
            var 试卷记录 = _试卷记录仓储.根据ID获取(试卷记录ID) ?? throw new InvalidOperationException("试卷记录不存在。");
            var 当前题目项 = _试卷题目项仓储.根据ID获取(试卷题目项ID) ?? throw new InvalidOperationException("试卷题目项不存在。");

            if (当前题目项.试卷记录ID != 试卷记录ID)
            {
                throw new InvalidOperationException("试卷题目项与试卷记录不匹配。");
            }

            var 下一道待处理题 = _试卷题目项仓储.获取下一道待处理题(试卷记录ID) ?? throw new InvalidOperationException("当前没有可确认的题目。");
            if (下一道待处理题.Id != 试卷题目项ID)
            {
                throw new InvalidOperationException("当前题目已经发生变化，请刷新后重试。");
            }

            _题型规则校验器.校验存在(题型ID);
            校验难度标签(难度标签ID);

            var 需要映射的知识点列表 = 当前题目项.获取原始知识点列表()
                .Where(文本 => !string.IsNullOrWhiteSpace(文本))
                .Select(文本 => 文本.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var 新建映射字典 = (新建知识点映射列表 ?? Array.Empty<知识点映射决策>())
                .GroupBy(映射 => 映射.原始知识点文本.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(分组 => 分组.Key, 分组 => 分组.Last(), StringComparer.OrdinalIgnoreCase);

            foreach (var 原始知识点文本 in 需要映射的知识点列表)
            {
                var 归一化文本 = 知识点文本规范化器.规范化(原始知识点文本);
                var 已有映射 = _知识点映射仓储.根据归一化原始文本获取(归一化文本);
                if (已有映射 != null)
                {
                    continue;
                }

                if (!新建映射字典.TryGetValue(原始知识点文本, out var 决策))
                {
                    throw new InvalidOperationException($"知识点“{原始知识点文本}”尚未完成映射。");
                }

                校验知识点映射决策(决策);
                if (决策.目标标签ID.HasValue)
                {
                    校验标签存在(决策.目标标签ID.Value);
                }

                _知识点映射仓储.增加(知识点映射.创建(
                    原始知识点文本,
                    归一化文本,
                    决策.目标标签ID,
                    决策.是否抛弃));
            }

            if (最终标签ID列表 == null || 最终标签ID列表.Count == 0)
            {
                throw new InvalidOperationException("请先完成题目标签检查。");
            }

            校验标签存在(难度标签ID);
            foreach (var 标签ID in 最终标签ID列表.Distinct())
            {
                校验标签存在(标签ID);
            }

            var 去重后标签ID列表 = 最终标签ID列表.Distinct().ToList();
            _题目标签规则校验器.校验(去重后标签ID列表);

            var 新题目 = _录入Ooxml题目用例.执行(new 录入Ooxml题目的请求
            {
                Description = 当前题目项.题目摘要,
                题型ID = 题型ID,
                标签ID列表 = 去重后标签ID列表,
                Ooxml内容 = 当前题目项.完整Ooxml内容,
            });

            当前题目项.标记为已确认(新题目.Id);
            _试卷题目项仓储.保存(当前题目项);

            试卷记录.标记已确认一题();
            _试卷记录仓储.保存(试卷记录);

            return _获取当前导入题目用例.执行(试卷记录ID);
        }

        private void 校验难度标签(int 难度标签ID)
        {
            var 标签 = _标签仓储.GetById(难度标签ID);
            if (标签 == null || 标签.标签种类ID != 系统标签种类.难度)
            {
                throw new InvalidOperationException("难度标签无效。");
            }
        }

        private void 校验标签存在(int 标签ID)
        {
            if (_标签仓储.GetById(标签ID) == null)
            {
                throw new InvalidOperationException("知识点映射目标标签不存在。");
            }
        }

        private static void 校验知识点映射决策(知识点映射决策 决策)
        {
            if (决策.是否抛弃 && 决策.目标标签ID.HasValue)
            {
                throw new InvalidOperationException("知识点映射不能同时选择标签和抛弃。");
            }

            if (!决策.是否抛弃 && !决策.目标标签ID.HasValue)
            {
                throw new InvalidOperationException("知识点映射必须选择一个标签或抛弃。");
            }
        }
    }
}
