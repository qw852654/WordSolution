using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 调整内容块子块排序用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 调整内容块子块排序用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public IReadOnlyList<内容块子项结果>? 执行(int 父内容块ID, 调整内容块子块排序的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            if (_内容块仓储.GetById(父内容块ID) == null)
            {
                return null;
            }

            foreach (var 排序项 in 请求.子项排序列表)
            {
                var 子项 = _内容块仓储.获取子项(排序项.子项ID);
                if (子项 == null || 子项.父内容块ID != 父内容块ID)
                {
                    throw new InvalidOperationException("排序列表包含不属于当前内容块的子项。");
                }

                子项.修改排序(排序项.排序);
                _内容块仓储.保存子项(子项);
            }

            return _内容块仓储
                .获取子项列表(父内容块ID)
                .Select(构建子项结果)
                .ToList();
        }

        private 内容块子项结果 构建子项结果(内容块子项 子项)
        {
            var 子内容块 = _内容块仓储.GetById(子项.子内容块ID)!;
            var 引用版本 = 子项.引用版本模式 == 内容块引用版本模式.锁定版本 && 子项.子内容块版本ID.HasValue
                ? _内容块仓储.获取版本(子项.子内容块版本ID.Value)
                : _内容块仓储.获取当前版本(子项.子内容块ID);
            return 内容块子项结果.从子项(子项, 子内容块, 引用版本);
        }
    }
}
