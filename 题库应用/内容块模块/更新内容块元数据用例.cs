using System;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 更新内容块元数据用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 更新内容块元数据用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public 内容块详情结果? 执行(int 内容块ID, 更新内容块元数据的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 内容块 = _内容块仓储.GetById(内容块ID);
            if (内容块 == null)
            {
                return null;
            }

            var 标题 = 请求.标题 ?? 内容块.标题;
            var 类型 = 请求.内容块类型 ?? 内容块.类型;
            var 状态 = 请求.内容块状态 ?? 内容块.状态;
            var 结构类型 = 请求.内容块结构类型 ?? 内容块.结构类型;
            var 是否允许子块 = 请求.是否允许子块 ?? 内容块.是否允许子块;

            if ((结构类型 == 内容块结构类型.原子块 || !是否允许子块)
                && _内容块仓储.获取子项列表(内容块ID).Count > 0)
            {
                throw new InvalidOperationException("当前内容块已有子块，不能改为原子块或关闭子块能力。请先移除子块引用。");
            }

            内容块.修改元数据(
                标题,
                请求.摘要,
                类型,
                状态,
                结构类型,
                是否允许子块);

            _内容块仓储.保存内容块(内容块);

            return 内容块详情结果.从内容块(内容块, _内容块仓储.获取当前版本(内容块ID));
        }
    }
}
