using System.Collections.Generic;
using 题库核心.讲义模块.领域;

namespace 题库核心.讲义模块.契约
{
    public interface I讲义仓储
    {
        讲义? GetById(int id);

        讲义项? 获取讲义项(int 讲义项ID);

        IReadOnlyList<讲义> 查询讲义(讲义状态? 状态, string? 关键词);

        IReadOnlyList<讲义项> 获取讲义项列表(int 讲义ID);

        IReadOnlyList<讲义生成记录> 获取生成记录列表(int 讲义ID);

        讲义生成记录? 获取生成记录(int 生成记录ID);

        void 增加讲义(讲义 讲义);

        void 保存讲义(讲义 讲义);

        void 增加讲义项(讲义 讲义, 讲义项 讲义项);

        void 保存讲义项排序(讲义 讲义, IReadOnlyList<讲义项> 讲义项列表);

        void 删除讲义项(讲义 讲义, 讲义项 讲义项);

        void 增加生成记录(讲义生成记录 生成记录);
    }
}
