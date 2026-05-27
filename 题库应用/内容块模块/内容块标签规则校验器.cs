using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块标签规则校验器
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 内容块标签规则校验器(I标签仓储 标签仓储, I标签种类仓储 标签种类仓储)
        {
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public IReadOnlyList<标签> 校验并返回标签(IReadOnlyList<int> 标签ID列表)
        {
            var 已选标签列表 = 标签ID列表
                .Where(标签ID => 标签ID > 0)
                .Distinct()
                .Select(标签ID => _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException($"标签 {标签ID} 不存在。"))
                .ToList();

            if (已选标签列表.Any(标签 => !标签.IsEnabled))
            {
                throw new InvalidOperationException("不能选择已停用的标签。");
            }

            if (已选标签列表.Any(标签 => 标签.标签种类ID == 系统标签种类.待整理))
            {
                throw new InvalidOperationException("待整理标签不能挂到正式内容块。");
            }

            var 标签种类字典 = _标签种类仓储.获取全部标签种类().ToDictionary(标签种类 => 标签种类.Id);
            foreach (var 同种类标签组 in 已选标签列表.GroupBy(标签 => 标签.标签种类ID))
            {
                if (!标签种类字典.TryGetValue(同种类标签组.Key, out var 标签种类))
                {
                    throw new InvalidOperationException($"标签种类 {同种类标签组.Key} 不存在。");
                }

                if (!标签种类.是否允许多选 && 同种类标签组.Count() > 1)
                {
                    throw new InvalidOperationException($"{标签种类.名称} 最多只能选择一个标签。");
                }
            }

            return 已选标签列表;
        }
    }
}
