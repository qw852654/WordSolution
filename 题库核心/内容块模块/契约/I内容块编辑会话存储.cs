using System.Collections.Generic;
using 题库核心.内容块模块.领域;

namespace 题库核心.内容块模块.契约
{
    public interface I内容块编辑会话存储
    {
        void 保存(内容块编辑会话 会话);

        内容块编辑会话? 获取(string 会话ID);

        IReadOnlyList<内容块编辑会话> 获取活动会话();
    }
}
