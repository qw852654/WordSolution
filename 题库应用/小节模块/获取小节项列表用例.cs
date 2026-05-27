using System.Collections.Generic;
using System.Linq;
using 题库核心.小节模块.契约;

namespace 题库应用.小节模块
{
    public class 获取小节项列表用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly 小节结果构建器 _小节结果构建器;

        public 获取小节项列表用例(I小节仓储 小节仓储, 小节结果构建器 小节结果构建器)
        {
            _小节仓储 = 小节仓储;
            _小节结果构建器 = 小节结果构建器;
        }

        public IReadOnlyList<小节项结果>? 执行(int 小节ID)
        {
            if (_小节仓储.GetById(小节ID) == null)
            {
                return null;
            }

            return _小节仓储
                .获取小节项列表(小节ID)
                .Select(_小节结果构建器.构建小节项)
                .ToList();
        }
    }
}
