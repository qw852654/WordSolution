using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.讲义模块.契约;

namespace 题库应用.讲义模块
{
    public class 调整讲义项排序用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly 讲义结果构建器 _讲义结果构建器;

        public 调整讲义项排序用例(I讲义仓储 讲义仓储, 讲义结果构建器 讲义结果构建器)
        {
            _讲义仓储 = 讲义仓储;
            _讲义结果构建器 = 讲义结果构建器;
        }

        public IReadOnlyList<讲义项结果>? 执行(int 讲义ID, 调整讲义项排序的请求 请求)
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

            var 现有项目 = _讲义仓储.获取讲义项列表(讲义ID).ToList();
            var 项目字典 = 现有项目.ToDictionary(项目 => 项目.Id);
            var 排序ID列表 = 请求.讲义项ID列表.Distinct().ToList();
            if (排序ID列表.Count != 现有项目.Count || 排序ID列表.Any(id => !项目字典.ContainsKey(id)))
            {
                throw new InvalidOperationException("排序列表必须完整包含当前讲义的所有项目。");
            }

            for (var i = 0; i < 排序ID列表.Count; i++)
            {
                项目字典[排序ID列表[i]].修改排序(i);
            }

            _讲义仓储.保存讲义项排序(讲义, 项目字典.Values.ToList());
            return _讲义仓储.获取讲义项列表(讲义ID)
                .Select(项目 => _讲义结果构建器.构建讲义项(项目))
                .ToList();
        }
    }
}
