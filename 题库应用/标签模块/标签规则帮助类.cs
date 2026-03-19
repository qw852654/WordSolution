using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    internal static class 标签规则帮助类
    {
        public static void 校验标签种类存在(标签种类? 标签种类, int 标签种类ID)
        {
            if (标签种类 == null)
            {
                throw new InvalidOperationException($"标签种类 {标签种类ID} 不存在。");
            }
        }

        public static void 校验新增或更新时的父级合法性(
            标签种类 标签种类,
            标签? 父标签,
            int? parentId)
        {
            if (!标签种类.是否树形)
            {
                if (parentId.HasValue)
                {
                    throw new InvalidOperationException($"{标签种类.名称} 不允许设置父级。");
                }

                return;
            }

            if (parentId.HasValue)
            {
                if (父标签 == null)
                {
                    throw new InvalidOperationException("父标签不存在。");
                }

                if (父标签.标签种类ID != 标签种类.Id)
                {
                    throw new InvalidOperationException("父标签和当前标签必须属于同一种类。");
                }
            }
        }

        public static void 校验不会成环(IReadOnlyList<标签> 全部标签, int 当前标签ID, int? 新父标签ID)
        {
            if (!新父标签ID.HasValue)
            {
                return;
            }

            var 标签字典 = 全部标签.ToDictionary(标签 => 标签.Id);
            var 当前父标签ID = 新父标签ID;

            while (当前父标签ID.HasValue)
            {
                if (当前父标签ID.Value == 当前标签ID)
                {
                    throw new InvalidOperationException("调整父级后会形成环。");
                }

                if (!标签字典.TryGetValue(当前父标签ID.Value, out var 父标签))
                {
                    break;
                }

                当前父标签ID = 父标签.ParentId;
            }
        }
    }
}
