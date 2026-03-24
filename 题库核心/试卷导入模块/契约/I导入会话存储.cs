using 题库核心.试卷导入模块.领域;

namespace 题库核心.试卷导入模块.契约
{
    public interface I导入会话存储
    {
        bool 存在(string sessionId);

        void 保存(导入试卷会话 会话);

        导入试卷会话? 获取(string sessionId);

        void 删除(string sessionId);

        string 获取会话工作目录(string sessionId);
    }
}
