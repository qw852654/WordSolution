using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库核心.小节模块.契约;
using 题库核心.小节模块.领域;

namespace 题库基础设施.小节模块
{
    public class 小节仓储 : I小节仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 小节仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 小节? GetById(int id)
        {
            var 小节数据 = _题库DbContext.小节表
                .AsNoTracking()
                .SingleOrDefault(小节 => 小节.Id == id);

            return 小节数据 == null ? null : 恢复小节(小节数据);
        }

        public 小节项? 获取小节项(int 小节项ID)
        {
            var 小节项数据 = _题库DbContext.小节项表
                .AsNoTracking()
                .SingleOrDefault(小节项 => 小节项.Id == 小节项ID);

            return 小节项数据 == null ? null : 恢复小节项(小节项数据);
        }

        public IReadOnlyList<小节> 查询小节(小节状态? 状态, int? 章节标签ID, string? 关键词)
        {
            var 查询 = _题库DbContext.小节表
                .AsNoTracking()
                .AsQueryable();

            if (状态.HasValue)
            {
                查询 = 查询.Where(小节 => 小节.状态 == 状态.Value);
            }

            if (章节标签ID.HasValue && 章节标签ID.Value > 0)
            {
                查询 = 查询.Where(小节 => 小节.章节标签ID == 章节标签ID.Value);
            }

            if (!string.IsNullOrWhiteSpace(关键词))
            {
                var 修整关键词 = 关键词.Trim();
                查询 = 查询.Where(小节 =>
                    小节.标题.Contains(修整关键词)
                    || (小节.摘要 != null && 小节.摘要.Contains(修整关键词)));
            }

            return 查询
                .OrderByDescending(小节 => 小节.更新时间)
                .ThenByDescending(小节 => 小节.Id)
                .Select(小节 => 恢复小节(小节))
                .ToList();
        }

        public IReadOnlyList<小节项> 获取小节项列表(int 小节ID)
        {
            return _题库DbContext.小节项表
                .AsNoTracking()
                .Where(小节项 => 小节项.小节ID == 小节ID)
                .OrderBy(小节项 => 小节项.排序)
                .ThenBy(小节项 => 小节项.Id)
                .Select(小节项 => 恢复小节项(小节项))
                .ToList();
        }

        public void 增加小节(小节 小节)
        {
            _题库DbContext.小节表.Add(小节);
            _题库DbContext.SaveChanges();
        }

        public void 保存小节(小节 小节)
        {
            _题库DbContext.小节表.Update(小节);
            _题库DbContext.SaveChanges();
        }

        public void 增加小节项(小节 小节, 小节项 小节项)
        {
            小节.标记内容已调整();
            _题库DbContext.小节表.Update(小节);
            _题库DbContext.小节项表.Add(小节项);
            _题库DbContext.SaveChanges();
        }

        public void 保存小节项排序(小节 小节, IReadOnlyList<小节项> 小节项列表)
        {
            小节.标记内容已调整();
            _题库DbContext.小节表.Update(小节);
            _题库DbContext.小节项表.UpdateRange(小节项列表);
            _题库DbContext.SaveChanges();
        }

        public void 删除小节项(小节 小节, 小节项 小节项)
        {
            小节.标记内容已调整();
            _题库DbContext.小节表.Update(小节);
            _题库DbContext.小节项表.Remove(小节项);
            _题库DbContext.SaveChanges();
        }

        private static 小节 恢复小节(小节 小节数据)
        {
            return 小节.从持久化恢复(
                小节数据.Id,
                小节数据.标题,
                小节数据.摘要,
                小节数据.章节标签ID,
                小节数据.状态,
                小节数据.创建时间,
                小节数据.更新时间);
        }

        private static 小节项 恢复小节项(小节项 小节项数据)
        {
            return 小节项.从持久化恢复(
                小节项数据.Id,
                小节项数据.小节ID,
                小节项数据.内容块ID,
                小节项数据.内容块版本ID,
                小节项数据.引用版本模式,
                小节项数据.角色,
                小节项数据.排序,
                小节项数据.创建时间);
        }
    }
}
