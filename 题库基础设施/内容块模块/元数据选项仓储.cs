using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库基础设施.数据访问;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库基础设施.内容块模块
{
    public class 元数据选项仓储 : I元数据选项仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 元数据选项仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 元数据选项? GetById(int id)
        {
            var 选项 = _题库DbContext.元数据选项表
                .AsNoTracking()
                .SingleOrDefault(元数据选项 => 元数据选项.Id == id);

            return 选项 == null ? null : 恢复元数据选项(选项);
        }

        public IReadOnlyList<元数据选项> 获取选项列表(元数据选项类别? category = null)
        {
            var 查询 = _题库DbContext.元数据选项表
                .AsNoTracking()
                .AsQueryable();

            if (category.HasValue)
            {
                查询 = 查询.Where(选项 => 选项.Category == category.Value);
            }

            return 查询
                .OrderBy(选项 => 选项.Category)
                .ThenBy(选项 => 选项.SortOrder)
                .ThenBy(选项 => 选项.Id)
                .Select(选项 => 恢复元数据选项(选项))
                .ToList();
        }

        public IReadOnlyDictionary<int, 元数据选项> 获取选项字典(IEnumerable<int> id列表)
        {
            var 有效ID列表 = id列表
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            if (有效ID列表.Count == 0)
            {
                return new Dictionary<int, 元数据选项>();
            }

            return _题库DbContext.元数据选项表
                .AsNoTracking()
                .Where(选项 => 有效ID列表.Contains(选项.Id))
                .Select(选项 => 恢复元数据选项(选项))
                .ToDictionary(选项 => 选项.Id);
        }

        public void 增加选项(元数据选项 选项)
        {
            _题库DbContext.元数据选项表.Add(选项);
            _题库DbContext.SaveChanges();
        }

        public void 保存选项(元数据选项 选项)
        {
            _题库DbContext.元数据选项表.Update(选项);
            _题库DbContext.SaveChanges();
        }

        private static 元数据选项 恢复元数据选项(元数据选项 选项)
        {
            return 元数据选项.从持久化恢复(
                选项.Id,
                选项.Category,
                选项.Name,
                选项.SortOrder,
                选项.IsActive,
                选项.CreatedTime,
                选项.UpdatedTime);
        }
    }
}
