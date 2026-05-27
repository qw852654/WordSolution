using 题库核心.讲义模块.契约;

namespace 题库应用.讲义模块
{
    public class 移除讲义项用例
    {
        private readonly I讲义仓储 _讲义仓储;

        public 移除讲义项用例(I讲义仓储 讲义仓储)
        {
            _讲义仓储 = 讲义仓储;
        }

        public bool 执行(int 讲义ID, int 讲义项ID)
        {
            var 讲义 = _讲义仓储.GetById(讲义ID);
            var 讲义项 = _讲义仓储.获取讲义项(讲义项ID);
            if (讲义 == null || 讲义项 == null || 讲义项.讲义ID != 讲义ID)
            {
                return false;
            }

            _讲义仓储.删除讲义项(讲义, 讲义项);
            return true;
        }
    }
}
