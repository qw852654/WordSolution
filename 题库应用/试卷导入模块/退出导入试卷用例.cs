using 题库核心.试卷导入模块.契约;

namespace 题库应用.试卷导入模块
{
    public class 退出导入试卷用例
    {
        private readonly I导入会话存储 _导入会话存储;

        public 退出导入试卷用例(I导入会话存储 导入会话存储)
        {
            _导入会话存储 = 导入会话存储;
        }

        public void 执行(string sessionId)
        {
            _导入会话存储.删除(sessionId);
        }
    }
}
