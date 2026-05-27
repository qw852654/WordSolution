using System;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 新建内容块用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 新建内容块用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public 内容块详情结果 执行(新建内容块的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 新内容块 = 内容块.创建(
                请求.标题,
                请求.摘要,
                请求.内容块类型 ?? 内容块类型.知识点,
                请求.内容块状态 ?? 内容块状态.草稿,
                请求.内容块结构类型,
                请求.是否允许子块);

            _内容块仓储.增加内容块(新内容块);

            return 内容块详情结果.从内容块(新内容块, null);
        }
    }
}
