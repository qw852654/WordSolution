using System.Collections.Generic;
using 题库核心.小节模块.领域;

namespace 题库核心.小节模块.契约
{
    public interface I小节仓储
    {
        小节? GetById(int id);

        小节项? 获取小节项(int 小节项ID);

        IReadOnlyList<小节> 查询小节(小节状态? 状态, int? 章节标签ID, string? 关键词);

        IReadOnlyList<小节项> 获取小节项列表(int 小节ID);

        void 增加小节(小节 小节);

        void 保存小节(小节 小节);

        void 增加小节项(小节 小节, 小节项 小节项);

        void 保存小节项排序(小节 小节, IReadOnlyList<小节项> 小节项列表);

        void 删除小节项(小节 小节, 小节项 小节项);
    }
}
