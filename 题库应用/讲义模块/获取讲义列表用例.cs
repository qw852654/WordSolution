using System.Collections.Generic;
using System.Linq;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 获取讲义列表用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly 讲义结果构建器 _讲义结果构建器;

        public 获取讲义列表用例(I讲义仓储 讲义仓储, 讲义结果构建器 讲义结果构建器)
        {
            _讲义仓储 = 讲义仓储;
            _讲义结果构建器 = 讲义结果构建器;
        }

        public IReadOnlyList<讲义详情结果> 执行(讲义状态? 状态, string? 关键词)
        {
            return _讲义仓储.查询讲义(状态, 关键词)
                .Select(讲义 => _讲义结果构建器.构建详情(讲义))
                .ToList();
        }
    }
}
