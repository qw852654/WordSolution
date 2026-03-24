using 题库核心.试卷导入模块.领域;

namespace 题库核心.试卷导入模块.契约
{
    public interface I试卷记录仓储
    {
        试卷记录? 根据年份与来源获取(int 年份标签ID, int 来源标签ID);

        试卷记录? 根据ID获取(int 试卷记录ID);

        IReadOnlyList<试卷记录> 获取全部();

        试卷记录 获取或创建(int 年份标签ID, int 来源标签ID, string 显示名称);

        void 保存(试卷记录 试卷记录);
    }
}
