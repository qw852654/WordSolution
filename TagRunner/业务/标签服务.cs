using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TagRunner.Models;

namespace TagRunner.业务
{
    /// <summary>
    /// 简化版标签服务（v0，教学优先）：
    /// - 单线程、线性实现（没有锁、没有缓存、没有并发保护）
    /// - 依赖注入仓储和可选的 DB 连接工厂用于简单查询
    /// - 每个方法实现都尽量直观，并在必要时使用父链遍历检测环
    /// </summary>
    public class 标签服务 : I标签服务
    {
        private readonly 数据.I标签仓储 _标签仓储;
        private readonly 数据.I题目仓储 _题目仓储;
        private readonly 数据.IDb连接工厂 _db连接工厂; // 可为 null

        // 提供一个受保护的无参构造，用于测试时派生不依赖仓储的轻量实现
        protected 标签服务()
        {
        }

        public 标签服务(数据.I标签仓储 标签仓储, 数据.I题目仓储 题目仓储, 数据.IDb连接工厂 db连接工厂 = null)
        {
            _标签仓储 = 标签仓储 ?? throw new ArgumentNullException(nameof(标签仓储));
            _题目仓储 = 题目仓储 ?? throw new ArgumentNullException(nameof(题目仓储));
            _db连接工厂 = db连接工厂; // 如果为 null，有些操作将回退到仓储或抛出更明确的错误
        }

        /// <summary>
        /// 读取数据库中所有标签并构建父子树。这个方法每次调用都会从数据库读取最新数据（没有缓存），实现直观。
        /// 要求：需要提供 _db连接工厂；如果没有则抛出异常（教学版本保持简单）。
        /// </summary>
        public List<标签> 获取标签树()
        {
            if (_db连接工厂 == null)
                throw new InvalidOperationException("标签服务需要数据库连接工厂来读取全部标签，请在构造时传入 db 连接工厂。");

            using (var conn = _db连接工厂.创建连接())
            {
                // 直接从 Tags 表读取所有行，然后在内存中按照 ParentId 组织成树
                var sql = "SELECT Id, Name, ParentId, Description, PrevSiblingId, NextSiblingId FROM Tags;";
                var rows = conn.Query<标签>(sql).ToList();

                var dict = rows.ToDictionary(t => t.Id);

                // 清空 children
                foreach (var t in dict.Values)
                    t.Children = new List<标签>();

                var roots = new List<标签>();

                foreach (var t in dict.Values)
                {
                    if (t.ParentId == null)
                    {
                        roots.Add(t);
                    }
                    else
                    {
                        if (dict.TryGetValue(t.ParentId.Value, out var parent))
                        {
                            parent.Children.Add(t);
                        }
                        else
                        {
                            // 父记录未找到，简单地当作根节点处理
                            roots.Add(t);
                        }
                    }
                }

                return roots;
            }
        }

        /// <summary>
        /// 新增标签：校验名字和父节点存在，检查同名（同父），然后调用仓储新增。
        /// 直观、同步、无缓存。
        /// </summary>
        public int 新增标签(标签 新标签)
        {
            if (新标签 == null) throw new ArgumentNullException(nameof(新标签));
            var 名称 = 新标签.Name?.Trim();
            if (string.IsNullOrWhiteSpace(名称)) throw new ArgumentException("标签名称不能为空", nameof(新标签));

            // 父存在性检查（如果提供）
            if (新标签.ParentId.HasValue)
            {
                var parent = _标签仓储.Id获取标签(新标签.ParentId.Value);
                if (parent == null) throw new InvalidOperationException($"父标签不存在(Id={新标签.ParentId.Value})");
            }

            // 同名检查（同一父下）
            if (_标签仓储.存在同名标签(名称, 新标签.ParentId))
                throw new InvalidOperationException("同一父节点下已存在相同名称的标签");

            新标签.Name = 名称;
            var newId = _标签仓储.新增标签(新标签);
            return newId;
        }

        /// <summary>
        /// 更新标签：检查存在性、父存在、禁止设置自身为父、检测父为自身子孙（防环），以及同名冲突（排除自身）。
        /// 实现采用简单的父链迭代来判断环：从新父向上遍历 parentId，若遇到当前节点 Id 则说明产生环。
        /// </summary>
        public bool 更新标签(标签 修改标签)
        {
            if (修改标签 == null) throw new ArgumentNullException(nameof(修改标签));
            if (修改标签.Id <= 0) throw new ArgumentException("标签 Id 无效", nameof(修改标签));

            var 名称 = 修改标签.Name?.Trim();
            if (string.IsNullOrWhiteSpace(名称)) throw new ArgumentException("标签名称不能为空", nameof(修改标签));

            // 检查目标存在
            var existing = _标签仓储.Id获取标签(修改标签.Id);
            if (existing == null) return false;

            // 父存在性检查
            if (修改标签.ParentId.HasValue)
            {
                var parent = _标签仓储.Id获取标签(修改标签.ParentId.Value);
                if (parent == null) throw new InvalidOperationException($"父标签不存在(Id={修改标签.ParentId.Value})");
            }

            // 禁止将自身设置为父
            if (修改标签.ParentId.HasValue && 修改标签.ParentId.Value == 修改标签.Id)
                throw new InvalidOperationException("不能将标签设置为自身的父节点");

            // 检查环：如果将父设置为自己的子孙，会形成环。用简单的父链遍历检测。
            if (修改标签.ParentId.HasValue)
            {
                int cur = 修改标签.ParentId.Value;
                while (cur != 0)
                {
                    var p = _标签仓储.Id获取标签(cur);
                    if (p == null) break; // 到了顶层或数据不完整，认为安全
                    if (p.Id == 修改标签.Id)
                        throw new InvalidOperationException("不能将标签的父设置为其子孙（会产生环）");
                    if (!p.ParentId.HasValue) break;
                    cur = p.ParentId.Value;
                }
            }

            // 同名检查（排除自身）：如果名称或父发生变化，需要确保同一父下没有其他同名
            bool nameOrParentChanged = 名称 != existing.Name || 修改标签.ParentId != existing.ParentId;
            if (nameOrParentChanged)
            {
                // 如果有 db 连接工厂，用 SQL 排除自身检查更精确；否则用仓储存在检查作为简单方案
                if (_db连接工厂 != null)
                {
                    using (var conn = _db连接工厂.创建连接())
                    {
                        var sql = @"SELECT COUNT(1) FROM Tags WHERE Name = @Name AND ((@ParentId IS NULL AND ParentId IS NULL) OR ParentId = @ParentId) AND Id != @Id;";
                        var count = conn.ExecuteScalar<long>(sql, new { Name = 名称, ParentId = 修改标签.ParentId, Id = 修改标签.Id });
                        if (count > 0) throw new InvalidOperationException("在同一父节点下已存在相同名称的其它标签");
                    }
                }
                else
                {
                    if (_标签仓储.存在同名标签(名称, 修改标签.ParentId))
                        throw new InvalidOperationException("在同一父节点下已存在相同名称的标签");
                }
            }

            修改标签.Name = 名称;
            return _标签仓储.更新标签(修改标签);
        }

        /// <summary>
        /// 删除标签（保守策略）：禁止删除有子标签或被题目引用的标签。
        /// 直观实现：先检查子标签数量，再检查 QuestionTags 中是否有引用。
        /// </summary>
        public bool 删除标签(int 标签Id)
        {
            if (标签Id <= 0) throw new ArgumentException("标签Id无效", nameof(标签Id));

            var tag = _标签仓储.Id获取标签(标签Id);
            if (tag == null) return false;

            // 检查是否有子标签
            if (_db连接工厂 != null)
            {
                using (var conn = _db连接工厂.创建连接())
                {
                    var cnt = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Tags WHERE ParentId = @Id;", new { Id = 标签Id });
                    if (cnt > 0) throw new InvalidOperationException("该标签存在子标签，禁止删除（保守策略）。");
                }
            }
            else
            {
                // 如果没有 db 连接工厂，简单地通过读取全部标签树判断
                var all = 获取标签树();
                var found = FindTagInTree(all, 标签Id);
                if (found != null && found.Children != null && found.Children.Count > 0)
                    throw new InvalidOperationException("该标签存在子标签，禁止删除（保守策略）。");
            }

            // 检查是否被题目引用
            if (_db连接工厂 != null)
            {
                using (var conn = _db连接工厂.创建连接())
                {
                    var cnt = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM QuestionTags WHERE TagId = @Id;", new { Id = 标签Id });
                    if (cnt > 0) throw new InvalidOperationException("该标签被题目引用，禁止删除（保守策略）。");
                }
            }
            else
            {
                var qs = _题目仓储.按标签查询(new List<标签> { tag });
                if (qs != null && qs.Count > 0) throw new InvalidOperationException("该标签被题目引用，禁止删除（保守策略）。");
            }

            return _标签仓储.删除标签(tag);
        }

        // 简单递归查找（教学用途）
        private 标签 FindTagInTree(IEnumerable<标签> roots, int id)
        {
            if (roots == null) return null;
            foreach (var r in roots)
            {
                if (r.Id == id) return r;
                var found = FindTagInTree(r.Children, id);
                if (found != null) return found;
            }
            return null;
        }

        // 简单的联想示例实现（教学/测试用途）：
        // - 对输入进行非严格匹配，返回若干模拟标签。生产中应实现真实查询或调用仓储/DB。
        public virtual List<标签> 联想标签(string 输入文本)
        {
            var result = new List<标签>();
            if (string.IsNullOrWhiteSpace(输入文本)) return result;
            var s = 输入文本.Trim();

            // 返回 5 个示例项用于 UI 测试
            for (int i = 1; i <= 5; i++)
            {
                result.Add(new 标签
                {
                    Id = i,
                    Name = s + " " + i,
                    Description = null,
                    Children = new List<标签>(),
                    ParentId = null,
                    PrevSiblingId = null,
                    NextSiblingId = null
                });
            }

            return result;
        }
    }
}
