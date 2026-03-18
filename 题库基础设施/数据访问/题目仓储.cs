using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库核心.筛选模块.领域;
using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库基础设施.数据访问
{
    public class 题目仓储 : I题目仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 题目仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 题目? GetById(int id)
        {
            var 题目数据 = _题库DbContext.题目表
                .AsNoTracking()
                .SingleOrDefault(题目 => 题目.Id == id);

            if (题目数据 == null)
            {
                return null;
            }

            var 标签ID列表 = _题库DbContext.题目标签关系表
                .AsNoTracking()
                .Where(关系 => 关系.题目ID == id)
                .Select(关系 => 关系.标签ID)
                .ToList();

            return 题目.从持久化恢复题目(
                题目数据.Id,
                题目数据.Description,
                题目数据.CreatedTime,
                题目数据.UpdateTime,
                标签ID列表);
        }

        public void 增加题目(题目 题目)
        {
            _题库DbContext.题目表.Add(题目);
            _题库DbContext.SaveChanges();

            保存题目标签关系(题目.Id, 题目.标签ID列表);
        }

        public void 保存题目(题目 题目)
        {
            _题库DbContext.题目表.Update(题目);
            _题库DbContext.SaveChanges();

            保存题目标签关系(题目.Id, 题目.标签ID列表);
        }

        public IReadOnlyList<题目> 根据标签查找(IReadOnlyList<int> 标签ID列表, 组合方式 组合方式)
        {
            if (标签ID列表 == null || 标签ID列表.Count == 0)
            {
                return new List<题目>();
            }

            var 有效标签ID列表 = 标签ID列表
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (有效标签ID列表.Count == 0)
            {
                return new List<题目>();
            }

            var 题目ID列表 = 查询题目ID列表(有效标签ID列表, 组合方式);
            var 题目列表 = new List<题目>();

            foreach (var 题目ID in 题目ID列表)
            {
                var 题目 = GetById(题目ID);
                if (题目 != null)
                {
                    题目列表.Add(题目);
                }
            }

            return 题目列表;
        }

        private List<int> 查询题目ID列表(List<int> 标签ID列表, 组合方式 组合方式)
        {
            var 关系查询 = _题库DbContext.题目标签关系表
                .AsNoTracking()
                .Where(关系 => 标签ID列表.Contains(关系.标签ID));

            if (组合方式 == 组合方式.并集)
            {
                return 关系查询
                    .Select(关系 => 关系.题目ID)
                    .Distinct()
                    .ToList();
            }

            return 关系查询
                .GroupBy(关系 => 关系.题目ID)
                .Where(分组 => 分组.Select(关系 => 关系.标签ID).Distinct().Count() == 标签ID列表.Count)
                .Select(分组 => 分组.Key)
                .ToList();
        }

        private void 保存题目标签关系(int 题目ID, IReadOnlyList<int> 标签ID列表)
        {
            var 旧关系列表 = _题库DbContext.题目标签关系表
                .Where(关系 => 关系.题目ID == 题目ID)
                .ToList();

            if (旧关系列表.Count > 0)
            {
                _题库DbContext.题目标签关系表.RemoveRange(旧关系列表);
            }

            foreach (var 标签ID in 标签ID列表.Distinct())
            {
                _题库DbContext.题目标签关系表.Add(new 题目标签关系
                {
                    题目ID = 题目ID,
                    标签ID = 标签ID
                });
            }

            _题库DbContext.SaveChanges();
        }
    }
}
