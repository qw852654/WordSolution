using System.Collections.Generic;
using 题库核心.内容块模块.领域;

namespace 题库核心.内容块模块.契约
{
    public interface I元数据选项仓储
    {
        元数据选项? GetById(int id);

        IReadOnlyList<元数据选项> 获取选项列表(元数据选项类别? category = null);

        IReadOnlyDictionary<int, 元数据选项> 获取选项字典(IEnumerable<int> id列表);

        void 增加选项(元数据选项 选项);

        void 保存选项(元数据选项 选项);
    }
}
