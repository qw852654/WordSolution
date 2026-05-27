using System.Collections.Generic;
using System.Linq;
using 题库核心.讲义模块.契约;

namespace 题库应用.讲义模块
{
    public class 获取讲义项列表用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly 讲义结果构建器 _讲义结果构建器;

        public 获取讲义项列表用例(I讲义仓储 讲义仓储, 讲义结果构建器 讲义结果构建器)
        {
            _讲义仓储 = 讲义仓储;
            _讲义结果构建器 = 讲义结果构建器;
        }

        public IReadOnlyList<讲义项结果>? 执行(int 讲义ID)
        {
            var 讲义 = _讲义仓储.GetById(讲义ID);
            if (讲义 == null)
            {
                return null;
            }

            return _讲义仓储.获取讲义项列表(讲义ID)
                .Select(项目 => _讲义结果构建器.构建讲义项(项目))
                .ToList();
        }
    }
}
