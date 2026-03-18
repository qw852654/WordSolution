using System.Collections.Generic;
using 题库核心.筛选模块.领域;
using 题库核心.题目模块.领域;

namespace 题库核心.题目模块.契约
{
    public interface I题目仓储
    {
        题目? GetById(int id);

        void 增加题目(题目 题目);

        void 保存题目(题目 题目);

        IReadOnlyList<题目> 根据标签查找(IReadOnlyList<int> 标签ID列表, 组合方式 组合方式);
    }
}
