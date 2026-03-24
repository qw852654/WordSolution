using System.Collections.Generic;
using 题库核心.试卷导入模块.领域;

namespace 题库核心.试卷导入模块.契约
{
    public interface I试卷题目项仓储
    {
        void 批量新增(IEnumerable<试卷题目项> 试卷题目项列表);

        试卷题目项? 根据ID获取(int 试卷题目项ID);

        试卷题目项? 获取下一道待处理题(int 试卷记录ID);

        bool 存在题目项(int 试卷记录ID);

        void 保存(试卷题目项 试卷题目项);
    }
}
