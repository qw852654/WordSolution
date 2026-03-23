using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库基础设施.数据访问
{
    public class 题型定义仓储 : I题型定义仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 题型定义仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public IReadOnlyList<题型定义> 获取全部()
        {
            return _题库DbContext.题型定义表
                .AsNoTracking()
                .OrderBy(题型 => 题型.排序值)
                .ThenBy(题型 => 题型.Id)
                .ToList();
        }

        public 题型定义? 根据ID获取(int id)
        {
            return _题库DbContext.题型定义表
                .AsNoTracking()
                .SingleOrDefault(题型 => 题型.Id == id);
        }

        public 题型定义? 根据名称获取(string 名称)
        {
            if (string.IsNullOrWhiteSpace(名称))
            {
                return null;
            }

            var 修整名称 = 名称.Trim();
            return _题库DbContext.题型定义表
                .AsNoTracking()
                .SingleOrDefault(题型 => 题型.名称 == 修整名称);
        }
    }
}
