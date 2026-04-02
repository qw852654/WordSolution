using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 移动标签用例
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 移动标签用例(I标签仓储 标签仓储, I标签种类仓储 标签种类仓储)
        {
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public void 执行(int 标签ID, 移动标签的请求 请求)
        {
            if (请求.目标标签ID <= 0)
            {
                throw new InvalidOperationException("目标标签不存在。");
            }

            var 源标签 = _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException("标签不存在。");
            var 目标标签 = _标签仓储.GetById(请求.目标标签ID) ?? throw new InvalidOperationException("目标标签不存在。");

            if (源标签.Id == 目标标签.Id)
            {
                throw new InvalidOperationException("不能把标签移动到自己身上。");
            }

            if (源标签.标签种类ID != 目标标签.标签种类ID)
            {
                throw new InvalidOperationException("只能在同一种类内移动标签。");
            }

            var 标签种类 = _标签种类仓储.GetById(源标签.标签种类ID);
            标签规则帮助类.校验标签种类存在(标签种类, 源标签.标签种类ID);

            if (!标签种类!.是否树形)
            {
                throw new InvalidOperationException($"{标签种类.名称} 不支持拖拽移动。");
            }

            var 全部同种类标签 = _标签仓储.根据种类获取标签(源标签.标签种类ID)
                .OrderBy(标签 => 标签.同级排序值)
                .ToList();

            if (是后代标签(全部同种类标签, 源标签.Id, 目标标签.Id))
            {
                throw new InvalidOperationException("不能把标签移动到自己的后代下面。");
            }

            var 放置方式 = 规范化放置方式(请求.放置方式);
            var 新父标签ID = 放置方式 == "inside" ? 目标标签.Id : 目标标签.ParentId;

            标签规则帮助类.校验不会成环(全部同种类标签, 源标签.Id, 新父标签ID);

            var 待保存标签列表 = new Dictionary<int, 标签>();

            var 原父标签ID = 源标签.ParentId;
            if (原父标签ID != 新父标签ID)
            {
                var 原同级标签列表 = 全部同种类标签
                    .Where(标签 => 标签.ParentId == 原父标签ID && 标签.Id != 源标签.Id)
                    .OrderBy(标签 => 标签.同级排序值)
                    .ToList();

                重新写入同级排序(原同级标签列表, 原父标签ID, 待保存标签列表);
            }

            var 新同级标签列表 = 全部同种类标签
                .Where(标签 => 标签.ParentId == 新父标签ID && 标签.Id != 源标签.Id)
                .OrderBy(标签 => 标签.同级排序值)
                .ToList();

            if (放置方式 == "inside")
            {
                新同级标签列表.Add(源标签);
            }
            else
            {
                var 目标索引 = 新同级标签列表.FindIndex(标签 => 标签.Id == 目标标签.Id);
                if (目标索引 < 0)
                {
                    throw new InvalidOperationException("目标标签不存在于当前层级。");
                }

                var 插入索引 = 放置方式 == "before" ? 目标索引 : 目标索引 + 1;
                新同级标签列表.Insert(插入索引, 源标签);
            }

            重新写入同级排序(新同级标签列表, 新父标签ID, 待保存标签列表);
            _标签仓储.批量保存标签(待保存标签列表.Values.ToList());
        }

        private static string 规范化放置方式(string? 放置方式)
        {
            var 规范值 = 放置方式?.Trim().ToLowerInvariant();
            return 规范值 switch
            {
                "before" => "before",
                "after" => "after",
                "inside" => "inside",
                _ => throw new InvalidOperationException("不支持的放置方式。"),
            };
        }

        private static bool 是后代标签(IReadOnlyList<标签> 全部标签, int 祖先标签ID, int 待检查标签ID)
        {
            var 标签字典 = 全部标签.ToDictionary(标签 => 标签.Id);
            var 当前父标签ID = 标签字典.TryGetValue(待检查标签ID, out var 当前标签)
                ? 当前标签.ParentId
                : null;

            while (当前父标签ID.HasValue)
            {
                if (当前父标签ID.Value == 祖先标签ID)
                {
                    return true;
                }

                if (!标签字典.TryGetValue(当前父标签ID.Value, out var 父标签))
                {
                    break;
                }

                当前父标签ID = 父标签.ParentId;
            }

            return false;
        }

        private static void 重新写入同级排序(
            IReadOnlyList<标签> 同级标签列表,
            int? 父标签ID,
            IDictionary<int, 标签> 待保存标签列表)
        {
            for (var 索引 = 0; 索引 < 同级标签列表.Count; 索引 += 1)
            {
                var 标签 = 同级标签列表[索引];
                标签.更新标签(
                    标签.标签种类ID,
                    标签.名称,
                    标签.Description,
                    父标签ID,
                    索引,
                    标签.NumericValue,
                    标签.IsEnabled);
                待保存标签列表[标签.Id] = 标签;
            }
        }
    }
}
