using System.Collections.Generic;
using 题库核心.标签模块.领域;

namespace 题库核心.标签模块.契约
{
    public interface I标签种类仓储
    {
        标签种类? GetById(int id);

        IReadOnlyList<标签种类> 获取全部标签种类();
    }
}
