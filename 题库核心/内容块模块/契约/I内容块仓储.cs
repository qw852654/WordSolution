using System.Collections.Generic;
using 题库核心.内容块模块.领域;

namespace 题库核心.内容块模块.契约
{
    public interface I内容块仓储
    {
        内容块? GetById(int id);

        内容块版本? 获取当前版本(int 内容块ID);

        内容块版本? 获取版本(int 内容块版本ID);

        IReadOnlyList<内容块版本> 获取版本列表(int 内容块ID);

        IReadOnlyList<内容块子项> 获取子项列表(int 父内容块ID);

        IReadOnlyList<内容块子项> 获取父项列表(int 子内容块ID);

        内容块子项? 获取子项(int 子项ID);

        IReadOnlyList<内容块> 查询内容块(内容块类型? 类型, 内容块状态? 状态, string? 关键词, IReadOnlyList<int>? 标签ID列表 = null);

        void 增加内容块(内容块 内容块);

        void 保存内容块(内容块 内容块);

        int 获取下一个版本号(int 内容块ID);

        void 增加版本并设为当前(内容块 内容块, 内容块版本 内容块版本);

        void 增加子项(内容块子项 内容块子项);

        void 保存子项(内容块子项 内容块子项);

        void 删除子项(内容块子项 内容块子项);
    }
}
