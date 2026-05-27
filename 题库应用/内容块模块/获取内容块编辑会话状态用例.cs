using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 获取内容块编辑会话状态用例
    {
        private readonly I内容块编辑会话存储 _内容块编辑会话存储;

        public 获取内容块编辑会话状态用例(I内容块编辑会话存储 内容块编辑会话存储)
        {
            _内容块编辑会话存储 = 内容块编辑会话存储;
        }

        public 内容块编辑会话结果? 执行(string 会话ID)
        {
            var 会话 = _内容块编辑会话存储.获取(会话ID);
            return 会话 == null ? null : 内容块编辑会话结果.从会话(会话);
        }
    }
}
