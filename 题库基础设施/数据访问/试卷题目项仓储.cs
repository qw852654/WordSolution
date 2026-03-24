using System.Collections.Generic;
using System.Linq;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库基础设施.数据访问
{
    public class 试卷题目项仓储 : I试卷题目项仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 试卷题目项仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public void 批量新增(IEnumerable<试卷题目项> 试卷题目项列表)
        {
            _题库DbContext.试卷题目项表.AddRange(试卷题目项列表);
            _题库DbContext.SaveChanges();
        }

        public 试卷题目项? 根据ID获取(int 试卷题目项ID)
        {
            return _题库DbContext.试卷题目项表.SingleOrDefault(题目项 => 题目项.Id == 试卷题目项ID);
        }

        public 试卷题目项? 获取下一道待处理题(int 试卷记录ID)
        {
            return _题库DbContext.试卷题目项表
                .Where(题目项 => 题目项.试卷记录ID == 试卷记录ID && 题目项.状态 == 试卷题目项状态.待处理)
                .OrderBy(题目项 => 题目项.顺序号)
                .FirstOrDefault();
        }

        public bool 存在题目项(int 试卷记录ID)
        {
            return _题库DbContext.试卷题目项表.Any(题目项 => 题目项.试卷记录ID == 试卷记录ID);
        }

        public void 保存(试卷题目项 试卷题目项)
        {
            _题库DbContext.试卷题目项表.Update(试卷题目项);
            _题库DbContext.SaveChanges();
        }
    }
}
