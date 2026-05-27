using System.Collections.Generic;
using System.Linq;
using 题库核心.讲义模块.契约;

namespace 题库应用.讲义模块
{
    public class 获取讲义生成记录列表用例
    {
        private readonly I讲义仓储 _讲义仓储;

        public 获取讲义生成记录列表用例(I讲义仓储 讲义仓储)
        {
            _讲义仓储 = 讲义仓储;
        }

        public IReadOnlyList<讲义生成记录结果>? 执行(int 讲义ID)
        {
            var 讲义 = _讲义仓储.GetById(讲义ID);
            if (讲义 == null)
            {
                return null;
            }

            return _讲义仓储.获取生成记录列表(讲义ID)
                .Select(记录 => 讲义生成记录结果.从生成记录(记录))
                .ToList();
        }
    }
}
