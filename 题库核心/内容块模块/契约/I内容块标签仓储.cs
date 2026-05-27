using System.Collections.Generic;

namespace 题库核心.内容块模块.契约
{
    public interface I内容块标签仓储
    {
        IReadOnlyList<int> 获取内容块标签ID列表(int 内容块ID);

        void 保存内容块标签ID列表(int 内容块ID, IReadOnlyList<int> 标签ID列表);
    }
}
