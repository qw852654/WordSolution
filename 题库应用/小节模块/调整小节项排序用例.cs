using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.小节模块.契约;

namespace 题库应用.小节模块
{
    public class 调整小节项排序用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly 小节结果构建器 _小节结果构建器;

        public 调整小节项排序用例(I小节仓储 小节仓储, 小节结果构建器 小节结果构建器)
        {
            _小节仓储 = 小节仓储;
            _小节结果构建器 = 小节结果构建器;
        }

        public IReadOnlyList<小节项结果>? 执行(int 小节ID, 调整小节项排序的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 小节 = _小节仓储.GetById(小节ID);
            if (小节 == null)
            {
                return null;
            }

            var 项目列表 = new List<题库核心.小节模块.领域.小节项>();
            foreach (var 排序项 in 请求.项目排序列表)
            {
                var 小节项 = _小节仓储.获取小节项(排序项.小节项ID);
                if (小节项 == null || 小节项.小节ID != 小节ID)
                {
                    throw new InvalidOperationException("排序列表包含不属于当前小节的项目。");
                }

                小节项.修改排序(排序项.排序);
                项目列表.Add(小节项);
            }

            _小节仓储.保存小节项排序(小节, 项目列表);

            return _小节仓储
                .获取小节项列表(小节ID)
                .Select(_小节结果构建器.构建小节项)
                .ToList();
        }
    }
}
