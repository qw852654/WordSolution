using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库核心.内容块模块.契约;

namespace 题库基础设施.内容块模块
{
    public class 内容块标签仓储 : I内容块标签仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 内容块标签仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public IReadOnlyList<int> 获取内容块标签ID列表(int 内容块ID)
        {
            return _题库DbContext.内容块标签关系表
                .AsNoTracking()
                .Where(关系 => 关系.内容块ID == 内容块ID)
                .OrderBy(关系 => 关系.标签ID)
                .Select(关系 => 关系.标签ID)
                .ToList();
        }

        public void 保存内容块标签ID列表(int 内容块ID, IReadOnlyList<int> 标签ID列表)
        {
            var 旧关系列表 = _题库DbContext.内容块标签关系表
                .Where(关系 => 关系.内容块ID == 内容块ID)
                .ToList();

            if (旧关系列表.Count > 0)
            {
                _题库DbContext.内容块标签关系表.RemoveRange(旧关系列表);
            }

            foreach (var 标签ID in 标签ID列表.Where(标签ID => 标签ID > 0).Distinct())
            {
                _题库DbContext.内容块标签关系表.Add(new 内容块标签关系
                {
                    内容块ID = 内容块ID,
                    标签ID = 标签ID,
                });
            }

            _题库DbContext.SaveChanges();
        }
    }
}
