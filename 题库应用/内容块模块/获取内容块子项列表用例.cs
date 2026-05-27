using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 获取内容块子项列表用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 获取内容块子项列表用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public IReadOnlyList<内容块子项结果>? 执行(int 父内容块ID)
        {
            var 父内容块 = _内容块仓储.GetById(父内容块ID);
            if (父内容块 == null)
            {
                return null;
            }

            return _内容块仓储
                .获取子项列表(父内容块ID)
                .Select(构建子项结果)
                .ToList();
        }

        private 内容块子项结果 构建子项结果(内容块子项 子项)
        {
            var 子内容块 = _内容块仓储.GetById(子项.子内容块ID)!;
            var 引用版本 = 获取引用版本(子项, 子内容块);
            return 内容块子项结果.从子项(子项, 子内容块, 引用版本);
        }

        private 内容块版本? 获取引用版本(内容块子项 子项, 内容块 子内容块)
        {
            if (子项.引用版本模式 == 内容块引用版本模式.锁定版本 && 子项.子内容块版本ID.HasValue)
            {
                return _内容块仓储.获取版本(子项.子内容块版本ID.Value);
            }

            return 子内容块.当前版本ID.HasValue ? _内容块仓储.获取当前版本(子内容块.Id) : null;
        }
    }
}
