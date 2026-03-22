using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.题目模块
{
    public class 题目标签规则校验器
    {
        private readonly I标签仓储 _标签仓储;

        public 题目标签规则校验器(I标签仓储 标签仓储)
        {
            _标签仓储 = 标签仓储;
        }

        public void 校验(IReadOnlyList<int> 标签ID列表)
        {
            var 已选标签列表 = 标签ID列表
                .Distinct()
                .Select(标签ID => _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException($"标签 {标签ID} 不存在。"))
                .ToList();

            if (已选标签列表.Any(标签 => 标签.标签种类ID == 系统标签种类.待整理))
            {
                throw new InvalidOperationException("待整理标签不能进入正式录题流程。");
            }

            if (已选标签列表.Count(标签 => 标签.标签种类ID == 系统标签种类.难度) > 1)
            {
                throw new InvalidOperationException("题目最多只能选择一个难度标签。");
            }

            if (已选标签列表.Count(标签 => 标签.标签种类ID == 系统标签种类.试卷题型) > 1)
            {
                throw new InvalidOperationException("题目最多只能选择一个试卷题型标签。");
            }
        }
    }
}
