using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;

namespace 题库基础设施.讲义模块
{
    public class 讲义仓储 : I讲义仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 讲义仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 讲义? GetById(int id)
        {
            var 讲义数据 = _题库DbContext.讲义表
                .AsNoTracking()
                .SingleOrDefault(讲义 => 讲义.Id == id);

            return 讲义数据 == null ? null : 恢复讲义(讲义数据);
        }

        public 讲义项? 获取讲义项(int 讲义项ID)
        {
            var 讲义项数据 = _题库DbContext.讲义项表
                .AsNoTracking()
                .SingleOrDefault(讲义项 => 讲义项.Id == 讲义项ID);

            return 讲义项数据 == null ? null : 恢复讲义项(讲义项数据);
        }

        public IReadOnlyList<讲义> 查询讲义(讲义状态? 状态, string? 关键词)
        {
            var 查询 = _题库DbContext.讲义表
                .AsNoTracking()
                .AsQueryable();

            if (状态.HasValue)
            {
                查询 = 查询.Where(讲义 => 讲义.状态 == 状态.Value);
            }

            if (!string.IsNullOrWhiteSpace(关键词))
            {
                var 修整关键词 = 关键词.Trim();
                查询 = 查询.Where(讲义 =>
                    讲义.标题.Contains(修整关键词)
                    || (讲义.摘要 != null && 讲义.摘要.Contains(修整关键词)));
            }

            return 查询
                .OrderByDescending(讲义 => 讲义.更新时间)
                .ThenByDescending(讲义 => 讲义.Id)
                .Select(讲义 => 恢复讲义(讲义))
                .ToList();
        }

        public IReadOnlyList<讲义项> 获取讲义项列表(int 讲义ID)
        {
            return _题库DbContext.讲义项表
                .AsNoTracking()
                .Where(讲义项 => 讲义项.讲义ID == 讲义ID)
                .OrderBy(讲义项 => 讲义项.排序)
                .ThenBy(讲义项 => 讲义项.Id)
                .Select(讲义项 => 恢复讲义项(讲义项))
                .ToList();
        }

        public IReadOnlyList<讲义生成记录> 获取生成记录列表(int 讲义ID)
        {
            return _题库DbContext.讲义生成记录表
                .AsNoTracking()
                .Where(记录 => 记录.讲义ID == 讲义ID)
                .OrderByDescending(记录 => 记录.生成时间)
                .ThenByDescending(记录 => 记录.Id)
                .Select(记录 => 恢复生成记录(记录))
                .ToList();
        }

        public 讲义生成记录? 获取生成记录(int 生成记录ID)
        {
            var 记录 = _题库DbContext.讲义生成记录表
                .AsNoTracking()
                .SingleOrDefault(生成记录 => 生成记录.Id == 生成记录ID);

            return 记录 == null ? null : 恢复生成记录(记录);
        }

        public void 增加讲义(讲义 讲义)
        {
            _题库DbContext.讲义表.Add(讲义);
            _题库DbContext.SaveChanges();
        }

        public void 保存讲义(讲义 讲义)
        {
            _题库DbContext.讲义表.Update(讲义);
            _题库DbContext.SaveChanges();
        }

        public void 增加讲义项(讲义 讲义, 讲义项 讲义项)
        {
            讲义.标记内容已调整();
            _题库DbContext.讲义表.Update(讲义);
            _题库DbContext.讲义项表.Add(讲义项);
            _题库DbContext.SaveChanges();
        }

        public void 保存讲义项排序(讲义 讲义, IReadOnlyList<讲义项> 讲义项列表)
        {
            讲义.标记内容已调整();
            _题库DbContext.讲义表.Update(讲义);
            _题库DbContext.讲义项表.UpdateRange(讲义项列表);
            _题库DbContext.SaveChanges();
        }

        public void 删除讲义项(讲义 讲义, 讲义项 讲义项)
        {
            讲义.标记内容已调整();
            _题库DbContext.讲义表.Update(讲义);
            _题库DbContext.讲义项表.Remove(讲义项);
            _题库DbContext.SaveChanges();
        }

        public void 增加生成记录(讲义生成记录 生成记录)
        {
            _题库DbContext.讲义生成记录表.Add(生成记录);
            _题库DbContext.SaveChanges();
        }

        private static 讲义 恢复讲义(讲义 讲义数据)
        {
            return 讲义.从持久化恢复(
                讲义数据.Id,
                讲义数据.标题,
                讲义数据.摘要,
                讲义数据.状态,
                讲义数据.创建时间,
                讲义数据.更新时间);
        }

        private static 讲义项 恢复讲义项(讲义项 讲义项数据)
        {
            return 讲义项.从持久化恢复(
                讲义项数据.Id,
                讲义项数据.讲义ID,
                讲义项数据.目标类型,
                讲义项数据.目标ID,
                讲义项数据.引用版本模式,
                讲义项数据.锁定内容块版本ID,
                讲义项数据.角色,
                讲义项数据.排序,
                讲义项数据.创建时间);
        }

        private static 讲义生成记录 恢复生成记录(讲义生成记录 记录数据)
        {
            return 讲义生成记录.从持久化恢复(
                记录数据.Id,
                记录数据.讲义ID,
                记录数据.文件路径,
                记录数据.版本清单Json,
                记录数据.生成时间);
        }
    }
}
