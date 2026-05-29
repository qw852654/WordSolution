using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块元数据选项帮助类
    {
        private readonly I元数据选项仓储 _元数据选项仓储;

        public 内容块元数据选项帮助类(I元数据选项仓储 元数据选项仓储)
        {
            _元数据选项仓储 = 元数据选项仓储;
        }

        public IReadOnlyDictionary<int, 元数据选项> 获取内容块选项字典(IEnumerable<内容块> 内容块列表)
        {
            var id列表 = 内容块列表.SelectMany(获取内容块选项ID列表);
            return _元数据选项仓储.获取选项字典(id列表);
        }

        public IReadOnlyDictionary<int, 元数据选项> 获取内容块选项字典(内容块 内容块)
        {
            return _元数据选项仓储.获取选项字典(获取内容块选项ID列表(内容块));
        }

        public void 校验内容块选项(
            int? roleOptionId,
            int? difficultyOptionId,
            int? usageOptionId,
            int? questionTypeOptionId)
        {
            var id列表 = new[]
            {
                roleOptionId,
                difficultyOptionId,
                usageOptionId,
                questionTypeOptionId,
            }.Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var 字典 = _元数据选项仓储.获取选项字典(id列表);
            校验选项(roleOptionId, 元数据选项类别.Role, 字典);
            校验选项(difficultyOptionId, 元数据选项类别.Difficulty, 字典);
            校验选项(usageOptionId, 元数据选项类别.Usage, 字典);
            校验选项(questionTypeOptionId, 元数据选项类别.QuestionType, 字典);
        }

        private static IEnumerable<int> 获取内容块选项ID列表(内容块 内容块)
        {
            if (内容块.RoleOptionId.HasValue) yield return 内容块.RoleOptionId.Value;
            if (内容块.DifficultyOptionId.HasValue) yield return 内容块.DifficultyOptionId.Value;
            if (内容块.UsageOptionId.HasValue) yield return 内容块.UsageOptionId.Value;
            if (内容块.QuestionTypeOptionId.HasValue) yield return 内容块.QuestionTypeOptionId.Value;
        }

        private static void 校验选项(int? 选项ID, 元数据选项类别 category, IReadOnlyDictionary<int, 元数据选项> 字典)
        {
            if (!选项ID.HasValue)
            {
                return;
            }

            if (!字典.TryGetValue(选项ID.Value, out var 选项))
            {
                throw new ArgumentException($"元数据选项 {选项ID.Value} 不存在。");
            }

            if (选项.Category != category)
            {
                throw new ArgumentException($"元数据选项“{选项.Name}”不属于 {category}。");
            }
        }
    }
}
