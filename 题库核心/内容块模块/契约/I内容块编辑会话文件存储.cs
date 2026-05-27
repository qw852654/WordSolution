using 题库核心.内容块模块.领域;

namespace 题库核心.内容块模块.契约
{
    public interface I内容块编辑会话文件存储
    {
        string 获取编辑文件路径(string 题库键, string 会话ID);

        string 获取会话信息文件路径(string 题库键, string 会话ID);

        void 复制文件(string 源文件路径, string 目标文件路径, bool 覆盖 = true);

        void 写入会话信息(内容块编辑会话 会话);
    }
}
