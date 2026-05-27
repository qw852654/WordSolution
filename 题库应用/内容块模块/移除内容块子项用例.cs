using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 移除内容块子项用例
    {
        private readonly I内容块仓储 _内容块仓储;

        public 移除内容块子项用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public bool 执行(int 父内容块ID, int 子项ID)
        {
            var 子项 = _内容块仓储.获取子项(子项ID);
            if (子项 == null || 子项.父内容块ID != 父内容块ID)
            {
                return false;
            }

            _内容块仓储.删除子项(子项);
            return true;
        }
    }
}
