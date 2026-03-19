using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库基础设施.数据访问
{
    public class 标签仓储 : I标签仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 标签仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 标签? GetById(int id)
        {
            var 标签数据 = _题库DbContext.标签表
                .AsNoTracking()
                .SingleOrDefault(标签 => 标签.Id == id);

            if (标签数据 == null)
            {
                return null;
            }

            return 标签.从持久化恢复标签(
                标签数据.Id,
                标签数据.大类ID,
                标签数据.名称,
                标签数据.Description,
                标签数据.ParentId,
                标签数据.同级排序值,
                标签数据.NumericValue,
                标签数据.IsEnabled);
        }

        public IReadOnlyList<标签> 获取全部标签()
        {
            return _题库DbContext.标签表
                .AsNoTracking()
                .OrderBy(标签 => 标签.同级排序值)
                .Select(标签 => 标签.从持久化恢复标签(
                    标签.Id,
                    标签.大类ID,
                    标签.名称,
                    标签.Description,
                    标签.ParentId,
                    标签.同级排序值,
                    标签.NumericValue,
                    标签.IsEnabled))
                .ToList();
        }

        public void 增加标签(标签 标签)
        {
            _题库DbContext.标签表.Add(标签);
            _题库DbContext.SaveChanges();
        }
    }
}
