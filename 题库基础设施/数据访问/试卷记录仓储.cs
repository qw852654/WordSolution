using System.Collections.Generic;
using System.Linq;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库基础设施.数据访问
{
    public class 试卷记录仓储 : I试卷记录仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 试卷记录仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 试卷记录? 根据年份与来源获取(int 年份标签ID, int 来源标签ID)
        {
            return _题库DbContext.试卷记录表
                .SingleOrDefault(试卷 => 试卷.年份标签ID == 年份标签ID && 试卷.来源标签ID == 来源标签ID);
        }

        public 试卷记录? 根据ID获取(int 试卷记录ID)
        {
            return _题库DbContext.试卷记录表.SingleOrDefault(试卷 => 试卷.Id == 试卷记录ID);
        }

        public IReadOnlyList<试卷记录> 获取全部()
        {
            return _题库DbContext.试卷记录表
                .OrderByDescending(试卷 => 试卷.Id)
                .ToList();
        }

        public 试卷记录 获取或创建(int 年份标签ID, int 来源标签ID, string 显示名称)
        {
            var 已有试卷 = 根据年份与来源获取(年份标签ID, 来源标签ID);
            if (已有试卷 != null)
            {
                if (!string.IsNullOrWhiteSpace(显示名称))
                {
                    已有试卷.更新显示名称(显示名称);
                    _题库DbContext.试卷记录表.Update(已有试卷);
                    _题库DbContext.SaveChanges();
                }

                return 已有试卷;
            }

            var 新试卷 = 试卷记录.创建(年份标签ID, 来源标签ID, 显示名称);
            _题库DbContext.试卷记录表.Add(新试卷);
            _题库DbContext.SaveChanges();
            return 新试卷;
        }

        public void 保存(试卷记录 试卷记录)
        {
            _题库DbContext.试卷记录表.Update(试卷记录);
            _题库DbContext.SaveChanges();
        }
    }
}
