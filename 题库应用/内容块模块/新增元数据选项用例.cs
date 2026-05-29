using System;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 新增元数据选项用例
    {
        private readonly I元数据选项仓储 _元数据选项仓储;

        public 新增元数据选项用例(I元数据选项仓储 元数据选项仓储)
        {
            _元数据选项仓储 = 元数据选项仓储;
        }

        public 元数据选项结果 执行(新增元数据选项的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 选项名称 = 请求.Name?.Trim() ?? string.Empty;
            if (_元数据选项仓储.获取选项列表(请求.Category).Any(选项 => string.Equals(选项.Name, 选项名称, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("同一字段类别下已存在同名选项。");
            }

            var sortOrder = 请求.SortOrder ?? 获取下一排序值(请求.Category);
            var 选项 = 元数据选项.创建(请求.Category, 选项名称, sortOrder, true);
            _元数据选项仓储.增加选项(选项);
            return 元数据选项结果.从选项(选项);
        }

        private int 获取下一排序值(元数据选项类别 category)
        {
            var 当前最大排序值 = _元数据选项仓储
                .获取选项列表(category)
                .Select(选项 => (int?)选项.SortOrder)
                .Max();

            return (当前最大排序值 ?? -1) + 1;
        }
    }
}
