using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库基础设施.数据访问
{
    public class 标签种类仓储 : I标签种类仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 标签种类仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 标签种类? GetById(int id)
        {
            return _题库DbContext.标签种类表
                .AsNoTracking()
                .Where(标签种类 => 标签种类.Id == id)
                .Select(标签种类 => 标签种类.从持久化恢复(
                    标签种类.Id,
                    标签种类.名称,
                    标签种类.是否树形,
                    标签种类.是否允许多选,
                    标签种类.是否系统内置,
                    标签种类.是否在正式工作流中可见))
                .SingleOrDefault();
        }

        public IReadOnlyList<标签种类> 获取全部标签种类()
        {
            return _题库DbContext.标签种类表
                .AsNoTracking()
                .OrderBy(标签种类 => 标签种类.Id)
                .Select(标签种类 => 标签种类.从持久化恢复(
                    标签种类.Id,
                    标签种类.名称,
                    标签种类.是否树形,
                    标签种类.是否允许多选,
                    标签种类.是否系统内置,
                    标签种类.是否在正式工作流中可见))
                .ToList();
        }
    }
}
