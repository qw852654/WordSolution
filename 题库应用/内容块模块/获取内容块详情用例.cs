using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 获取内容块详情用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 获取内容块详情用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public 内容块详情结果? 执行(int 内容块ID)
        {
            var 内容块 = _内容块仓储.GetById(内容块ID);
            if (内容块 == null)
            {
                return null;
            }

            var 当前版本 = _内容块仓储.获取当前版本(内容块ID);
            return 内容块详情结果.从内容块(内容块, 当前版本);
        }
    }
}
