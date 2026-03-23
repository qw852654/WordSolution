using System.Collections.Generic;
using 题库核心.题目模块.领域;

namespace 题库核心.题目模块.契约
{
    public interface I题型定义仓储
    {
        IReadOnlyList<题型定义> 获取全部();

        题型定义? 根据ID获取(int id);

        题型定义? 根据名称获取(string 名称);
    }
}
