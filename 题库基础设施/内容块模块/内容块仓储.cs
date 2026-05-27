using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库基础设施.内容块模块
{
    public class 内容块仓储 : I内容块仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 内容块仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 内容块? GetById(int id)
        {
            var 内容块数据 = _题库DbContext.内容块表
                .AsNoTracking()
                .SingleOrDefault(内容块 => 内容块.Id == id);

            return 内容块数据 == null ? null : 恢复内容块(内容块数据);
        }

        public 内容块版本? 获取当前版本(int 内容块ID)
        {
            var 当前版本ID = _题库DbContext.内容块表
                .AsNoTracking()
                .Where(内容块 => 内容块.Id == 内容块ID)
                .Select(内容块 => 内容块.当前版本ID)
                .SingleOrDefault();
            if (!当前版本ID.HasValue)
            {
                return null;
            }

            return _题库DbContext.内容块版本表
                .AsNoTracking()
                .Where(版本 => 版本.Id == 当前版本ID.Value)
                .Select(版本 => 恢复内容块版本(版本))
                .SingleOrDefault();
        }

        public 内容块版本? 获取版本(int 内容块版本ID)
        {
            return _题库DbContext.内容块版本表
                .AsNoTracking()
                .Where(版本 => 版本.Id == 内容块版本ID)
                .Select(版本 => 恢复内容块版本(版本))
                .SingleOrDefault();
        }

        public IReadOnlyList<内容块版本> 获取版本列表(int 内容块ID)
        {
            return _题库DbContext.内容块版本表
                .AsNoTracking()
                .Where(版本 => 版本.内容块ID == 内容块ID)
                .OrderByDescending(版本 => 版本.版本号)
                .Select(版本 => 恢复内容块版本(版本))
                .ToList();
        }

        public IReadOnlyList<内容块子项> 获取子项列表(int 父内容块ID)
        {
            return _题库DbContext.内容块子项表
                .AsNoTracking()
                .Where(子项 => 子项.父内容块ID == 父内容块ID)
                .OrderBy(子项 => 子项.排序)
                .ThenBy(子项 => 子项.Id)
                .Select(子项 => 恢复内容块子项(子项))
                .ToList();
        }

        public IReadOnlyList<内容块子项> 获取父项列表(int 子内容块ID)
        {
            return _题库DbContext.内容块子项表
                .AsNoTracking()
                .Where(子项 => 子项.子内容块ID == 子内容块ID)
                .OrderBy(子项 => 子项.父内容块ID)
                .ThenBy(子项 => 子项.Id)
                .Select(子项 => 恢复内容块子项(子项))
                .ToList();
        }

        public 内容块子项? 获取子项(int 子项ID)
        {
            var 子项 = _题库DbContext.内容块子项表
                .AsNoTracking()
                .SingleOrDefault(内容块子项 => 内容块子项.Id == 子项ID);

            return 子项 == null ? null : 恢复内容块子项(子项);
        }

        public IReadOnlyList<内容块> 查询内容块(内容块类型? 类型, 内容块状态? 状态, string? 关键词, IReadOnlyList<int>? 标签ID列表 = null)
        {
            var 查询 = _题库DbContext.内容块表
                .AsNoTracking()
                .AsQueryable();

            if (类型.HasValue)
            {
                查询 = 查询.Where(内容块 => 内容块.类型 == 类型.Value);
            }

            if (状态.HasValue)
            {
                查询 = 查询.Where(内容块 => 内容块.状态 == 状态.Value);
            }

            if (!string.IsNullOrWhiteSpace(关键词))
            {
                var 修整关键词 = 关键词.Trim();
                查询 = 查询.Where(内容块 =>
                    内容块.标题.Contains(修整关键词)
                    || (内容块.摘要 != null && 内容块.摘要.Contains(修整关键词)));
            }

            var 有效标签ID列表 = (标签ID列表 ?? new List<int>())
                .Where(标签ID => 标签ID > 0)
                .Distinct()
                .ToList();
            if (有效标签ID列表.Count > 0)
            {
                var 内容块ID列表 = _题库DbContext.内容块标签关系表
                    .AsNoTracking()
                    .Where(关系 => 有效标签ID列表.Contains(关系.标签ID))
                    .GroupBy(关系 => 关系.内容块ID)
                    .Where(分组 => 分组.Select(关系 => 关系.标签ID).Distinct().Count() == 有效标签ID列表.Count)
                    .Select(分组 => 分组.Key)
                    .ToList();

                if (内容块ID列表.Count == 0)
                {
                    return new List<内容块>();
                }

                查询 = 查询.Where(内容块 => 内容块ID列表.Contains(内容块.Id));
            }

            return 查询
                .OrderByDescending(内容块 => 内容块.更新时间)
                .ThenByDescending(内容块 => 内容块.Id)
                .Select(内容块 => 恢复内容块(内容块))
                .ToList();
        }

        public void 增加内容块(内容块 内容块)
        {
            _题库DbContext.内容块表.Add(内容块);
            _题库DbContext.SaveChanges();
        }

        public void 保存内容块(内容块 内容块)
        {
            _题库DbContext.内容块表.Update(内容块);
            _题库DbContext.SaveChanges();
        }

        public int 获取下一个版本号(int 内容块ID)
        {
            return (_题库DbContext.内容块版本表
                .AsNoTracking()
                .Where(版本 => 版本.内容块ID == 内容块ID)
                .Select(版本 => (int?)版本.版本号)
                .Max() ?? 0) + 1;
        }

        public void 增加版本并设为当前(内容块 内容块, 内容块版本 内容块版本)
        {
            var 旧当前版本列表 = _题库DbContext.内容块版本表
                .Where(版本 => 版本.内容块ID == 内容块.Id && 版本.是否当前版本)
                .ToList();

            foreach (var 旧当前版本 in 旧当前版本列表)
            {
                旧当前版本.设置是否当前版本(false);
            }

            _题库DbContext.内容块版本表.Add(内容块版本);
            _题库DbContext.SaveChanges();

            内容块.设置当前版本(内容块版本.Id);
            _题库DbContext.内容块表.Update(内容块);
            _题库DbContext.SaveChanges();
        }

        public void 增加子项(内容块子项 内容块子项)
        {
            _题库DbContext.内容块子项表.Add(内容块子项);
            _题库DbContext.SaveChanges();
        }

        public void 保存子项(内容块子项 内容块子项)
        {
            _题库DbContext.内容块子项表.Update(内容块子项);
            _题库DbContext.SaveChanges();
        }

        public void 删除子项(内容块子项 内容块子项)
        {
            _题库DbContext.内容块子项表.Remove(内容块子项);
            _题库DbContext.SaveChanges();
        }

        private static 内容块 恢复内容块(内容块 内容块数据)
        {
            return 内容块.从持久化恢复(
                内容块数据.Id,
                内容块数据.标题,
                内容块数据.摘要,
                内容块数据.类型,
                内容块数据.状态,
                内容块数据.当前版本ID,
                内容块数据.结构类型,
                内容块数据.是否允许子块,
                内容块数据.创建时间,
                内容块数据.更新时间);
        }

        private static 内容块版本 恢复内容块版本(内容块版本 版本数据)
        {
            return 内容块版本.从持久化恢复(
                版本数据.Id,
                版本数据.内容块ID,
                版本数据.版本号,
                版本数据.Docx路径,
                版本数据.Html预览路径,
                版本数据.纯文本内容,
                版本数据.创建时间,
                版本数据.是否当前版本);
        }

        private static 内容块子项 恢复内容块子项(内容块子项 子项数据)
        {
            return 内容块子项.从持久化恢复(
                子项数据.Id,
                子项数据.父内容块ID,
                子项数据.子内容块ID,
                子项数据.子内容块版本ID,
                子项数据.引用版本模式,
                子项数据.角色,
                子项数据.排序,
                子项数据.创建时间);
        }
    }
}
