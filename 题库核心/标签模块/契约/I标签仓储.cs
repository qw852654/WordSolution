using System.Collections.Generic;
using 题库核心.标签模块.领域;

namespace 题库核心.标签模块.契约
{
    public interface I标签仓储
    {
        标签? GetById(int id);

        IReadOnlyList<标签> 获取全部标签();

        IReadOnlyList<标签> 根据种类获取标签(int 标签种类ID);

        void 增加标签(标签 标签);

        void 保存标签(标签 标签);

        void 删除标签(标签 标签);

        bool 是否存在同父同种类同名标签(int 标签种类ID, int? parentId, string 名称, int? 排除标签ID = null);

        bool 是否被题目引用(int 标签ID);

        int 获取同父同种类最大排序值(int 标签种类ID, int? parentId);
    }
}
