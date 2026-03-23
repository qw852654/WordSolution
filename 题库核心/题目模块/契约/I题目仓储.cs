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

        void 删除题目(int 题目ID);

        void 更新题型(int 题目ID, int 题型ID);

        题目? 获取下一道未设置题型的题目();

        IReadOnlyList<题目> 根据条件查找(
            IReadOnlyList<int> 标签ID列表,
            组合方式 本步标签组合方式,
            int? 题型ID,
            bool 仅筛选题型未设置);
    }
}
