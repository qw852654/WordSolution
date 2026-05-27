using 题库核心.小节模块.契约;

namespace 题库应用.小节模块
{
    public class 移除小节项用例
    {
        private readonly I小节仓储 _小节仓储;

        public 移除小节项用例(I小节仓储 小节仓储)
        {
            _小节仓储 = 小节仓储;
        }

        public bool 执行(int 小节ID, int 小节项ID)
        {
            var 小节 = _小节仓储.GetById(小节ID);
            var 小节项 = _小节仓储.获取小节项(小节项ID);
            if (小节 == null || 小节项 == null || 小节项.小节ID != 小节ID)
            {
                return false;
            }

            _小节仓储.删除小节项(小节, 小节项);
            return true;
        }
    }
}
