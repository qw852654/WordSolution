using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 获取元数据选项列表用例
    {
        private readonly I元数据选项仓储 _元数据选项仓储;

        public 获取元数据选项列表用例(I元数据选项仓储 元数据选项仓储)
        {
            _元数据选项仓储 = 元数据选项仓储;
        }

        public IReadOnlyList<元数据选项结果> 执行(元数据选项类别? category = null)
        {
            return _元数据选项仓储
                .获取选项列表(category)
                .Select(元数据选项结果.从选项)
                .ToList();
        }
    }
}
