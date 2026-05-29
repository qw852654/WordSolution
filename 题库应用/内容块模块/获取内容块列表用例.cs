using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 获取内容块列表用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly 内容块元数据选项帮助类 _内容块元数据选项帮助类;

        public 获取内容块列表用例(
            I内容块仓储 内容块仓储,
            内容块元数据选项帮助类 内容块元数据选项帮助类)
        {
            _内容块仓储 = 内容块仓储;
            _内容块元数据选项帮助类 = 内容块元数据选项帮助类;
        }

        public IReadOnlyList<内容块详情结果> 执行(
            内容块类型? 类型,
            内容块状态? 状态,
            string? 关键词,
            IReadOnlyList<int>? 标签ID列表 = null)
        {
            var 内容块列表 = _内容块仓储
                .查询内容块(类型, 状态, 关键词, 标签ID列表)
                .ToList();
            var 选项字典 = _内容块元数据选项帮助类.获取内容块选项字典(内容块列表);

            return 内容块列表
                .Select(内容块 => 内容块详情结果.从内容块(内容块, _内容块仓储.获取当前版本(内容块.Id), 选项字典))
                .ToList();
        }
    }
}
