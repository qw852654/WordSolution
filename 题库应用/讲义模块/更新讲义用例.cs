using System;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 更新讲义用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly 讲义结果构建器 _讲义结果构建器;

        public 更新讲义用例(I讲义仓储 讲义仓储, 讲义结果构建器 讲义结果构建器)
        {
            _讲义仓储 = 讲义仓储;
            _讲义结果构建器 = 讲义结果构建器;
        }

        public 讲义详情结果? 执行(int 讲义ID, 更新讲义的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 讲义 = _讲义仓储.GetById(讲义ID);
            if (讲义 == null)
            {
                return null;
            }

            讲义.修改元数据(请求.标题, 请求.摘要, 请求.状态 ?? 讲义.状态);
            _讲义仓储.保存讲义(讲义);
            return _讲义结果构建器.构建详情(讲义);
        }
    }
}
