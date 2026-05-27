using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 获取内容块版本列表用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 获取内容块版本列表用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public IReadOnlyList<内容块版本结果> 执行(int 内容块ID)
        {
            return _内容块仓储
                .获取版本列表(内容块ID)
                .Select(内容块版本结果.从版本)
                .ToList();
        }
    }
}
