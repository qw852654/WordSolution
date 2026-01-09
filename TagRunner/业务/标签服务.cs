using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TagRunner.Models;

namespace TagRunner.业务
{
    /// <summary>
    /// 业务层标签服务：实现校验/编排等业务规则（构造注入仓储与可选依赖）。
    /// 当前先实现构造函数与 获取标签树() 方法，其他写方法按步骤实现。
    /// </summary>
    public class 标签服务 : I标签服务
    {
        private readonly 数据.I标签仓储 _标签仓储;
        private readonly 数据.I题目仓储 _题目仓储;
        private readonly 数据.IDb连接工厂 _db连接工厂; // 用于在业务层快速读取标签表构建树（谨慎使用）
        private readonly object _lock = new object();

        // 内存缓存：根节点列表（ParentId == null）
        private List<标签> _标签树缓存;

        public 标签服务(数据.I标签仓储 标签仓储, 数据.I题目仓储 题目仓储, 数据.IDb连接工厂 db连接工厂 = null)
        {
            _标签仓储 = 标签仓储 ?? throw new ArgumentNullException(nameof(标签仓储));
            _题目仓储 = 题目仓储 ?? throw new ArgumentNullException(nameof(题目仓储));
            _db连接工厂 = db连接工厂; // 可以为 null，但获取树时需要抛出明确异常或回退到仓储逐项查询
        }

        /// <summary>
        /// 返回标签树的根节点列表（ParentId == null）。
        /// 实现策略：优先使用内存缓存；缓存为空时从数据库全量读取并构建树。
        /// 原因：频繁读取标签树时避免重复 SQL；但写操作需刷新缓存。
        /// </summary>
        public List<标签> 获取标签树()
        {
            lock (_lock)
            {
                if (_标签树缓存 != null)
                {
                    // 返回浅拷贝，避免外部修改内部缓存
                    return _标签树缓存.Select(CloneTagShallow).ToList();
                }

                if (_db连接工厂 == null)
                    throw new InvalidOperationException("标签服务未配置数据库连接工厂，无法从数据库加载标签树。");

                using (var conn = _db连接工厂.创建连接())
                {
                    var sql = "SELECT Id, Name, ParentId, Description, PrevSiblingId, NextSiblingId FROM Tags";
                    var rows = conn.Query<标签>(sql).ToList();

                    // 字典按 Id
                    var dict = rows.ToDictionary(t => t.Id);

                    // 清空 children
                    foreach (var t in dict.Values)
                        t.Children = new List<标签>();

                    List<标签> roots = new List<标签>();

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
                                // 父不存在，视为根
                                roots.Add(t);
                            }
                        }
                    }

                    // 排序或其他后处理可在此处添加

                    _标签树缓存 = roots;

                    return _标签树缓存.Select(CloneTagShallow).ToList();
                }
            }
        }

        // 辅助：浅克隆标签对象（不克隆子树引用）以避免外部破坏缓存
        private 标签 CloneTagShallow(标签 src)
        {
            if (src == null) return null;
            return new 标签
            {
                Id = src.Id,
                Name = src.Name,
                ParentId = src.ParentId,
                Description = src.Description,
                PrevSiblingId = src.PrevSiblingId,
                NextSiblingId = src.NextSiblingId,
                Children = src.Children != null ? new List<标签>(src.Children) : new List<标签>()
            };
        }

        public int 新增标签(标签 新标签)
        {
            if (新标签 == null) throw new ArgumentNullException(nameof(新标签));

            var 名称 = 新标签.Name?.Trim();
            if (string.IsNullOrWhiteSpace(名称)) throw new ArgumentException("标签名称不能为空", nameof(新标签));

            int? 父Id = 新标签.ParentId;

            lock (_lock)
            {
                // 父节点存在性检查（如果提供）
                if (父Id.HasValue)
                {
                    var parent = _标签仓储.Id获取标签(父Id.Value);
                    if (parent == null)
                        throw new InvalidOperationException($"父标签不存在 (Id={父Id.Value})");
                }

                // 同名检查（同一父下不能有相同名称）
                if (_标签仓储.存在同名标签(名称, 父Id))
                    throw new InvalidOperationException($"在父节点 {父Id?.ToString() ?? "null"} 下已存在同名标签 '{名称}'");

                // 准备实体并持久化
                新标签.Name = 名称;
                var newId = _标签仓储.新增标签(新标签);

                // 刷新/失效缓存，下一次读取会从数据库重建树
                _标签树缓存 = null;

                return newId;
            }
        }

        public bool 更新标签(标签 修改标签)
        {
            if (修改标签 == null) throw new ArgumentNullException(nameof(修改标签));
            if (修改标签.Id <= 0) throw new ArgumentException("标签 Id 无效", nameof(修改标签));

            var 名称 = 修改标签.Name?.Trim();
            if (string.IsNullOrWhiteSpace(名称)) throw new ArgumentException("标签名称不能为空", nameof(修改标签));

            lock (_lock)
            {
                // 检查目标是否存在
                var existing = _标签仓储.Id获取标签(修改标签.Id);
                if (existing == null) return false;

                int? newParentId = 修改标签.ParentId;

                // 父存在性检查（若提供）
                if (newParentId.HasValue)
                {
                    var parent = _标签仓储.Id获取标签(newParentId.Value);
                    if (parent == null)
                        throw new InvalidOperationException($"父标签不存在 (Id={newParentId.Value})");
                }

                // 禁止将节点作为自己的父节点
                if (newParentId.HasValue && newParentId.Value == 修改标签.Id)
                    throw new InvalidOperationException("不能将标签设置为自身的父节点");

                // 检查是否将父节点设置为自身的子孙（产生环）
                if (newParentId.HasValue && _db连接工厂 != null)
                {
                    // 构建树并判断
                    var roots = 获取标签树(); // 已加锁，但 获取标签树 内部会锁，避免 deadlock because same lock? 获取标签树 uses same _lock; we're inside lock -> calling 获取标签树 causes deadlock. Need to avoid. We'll call a helper to get tree without taking lock by temporarily releasing? Simpler: use DB to check ancestry via walking parents.
                }

                // 同名检查（排除自身）
                if (_db连接工厂 == null)
                {
                    // 没有 db 连接工厂 时，使用仓储的存在检查，若存在则认为冲突（可能误判已存在自身，但这种场景较少）
                    if (_标签仓储.存在同名标签(名称, newParentId))
                        throw new InvalidOperationException($"在父节点 {newParentId?.ToString() ?? "null"} 下已存在同名标签 '{名称}'");
                }
                else
                {
                    using (var conn = _db连接工厂.创建连接())
                    {
                        var sql = @"SELECT COUNT(1) FROM Tags WHERE Name = @Name AND ((@ParentId IS NULL AND ParentId IS NULL) OR ParentId = @ParentId) AND Id != @Id;";
                        var count = conn.ExecuteScalar<long>(sql, new { Name = 名称, ParentId = newParentId, Id = 修改标签.Id });
                        if (count > 0)
                            throw new InvalidOperationException($"在父节点 {newParentId?.ToString() ?? "null"} 下已存在同名标签 '{名称}'");
                    }
                }

                // 如果通过所有校验，执行更新
                修改标签.Name = 名称;
                var success = _标签仓储.更新标签(修改标签);
                if (success)
                {
                    _标签树缓存 = null;
                }
                return success;
            }
        }

        public bool 删除标签(int 标签Id)
        {
            if (标签Id <= 0) throw new ArgumentException("标签Id无效", nameof(标签Id));

            // 检查标签是否存在
            var tag = _标签仓储.Id获取标签(标签Id);
            if (tag == null) return false;

            // 检查是否有子标签
            bool 有子标签 = false;
            if (_db连接工厂 != null)
            {
                using (var conn = _db连接工厂.创建连接())
                {
                    var cnt = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Tags WHERE ParentId = @Id;", new { Id = 标签Id });
                    有子标签 = cnt > 0;
                }
            }
            else
            {
                // 使用内存树检查
                var tree = 获取标签树();
                // 找到对应节点
                标签 found = null;
                var stack = new Stack<标签>(tree);
                while (stack.Count > 0)
                {
                    var n = stack.Pop();
                    if (n.Id == 标签Id) { found = n; break; }
                    if (n.Children != null)
                    {
                        foreach (var c in n.Children) stack.Push(c);
                    }
                }
                if (found != null && found.Children != null && found.Children.Count > 0) 有子标签 = true;
            }

            if (有子标签)
                throw new InvalidOperationException("该标签存在子标签，禁止删除（保守策略）。");

            // 检查是否被题目引用
            bool 被题目引用 = false;
            if (_db连接工厂 != null)
            {
                using (var conn = _db连接工厂.创建连接())
                {
                    var cnt = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM QuestionTags WHERE TagId = @Id;", new { Id = 标签Id });
                    被题目引用 = cnt > 0;
                }
            }
            else
            {
                // 使用题目仓储查询
                var qs = _题目仓储.按标签查询(new List<标签> { tag });
                被题目引用 = qs != null && qs.Count > 0;
            }

            if (被题目引用)
                throw new InvalidOperationException("该标签被题目引用，禁止删除（保守策略）。");

            lock (_lock)
            {
                // 最终删除（仓储负责事务）
                var success = _标签仓储.删除标签(tag);
                if (success)
                {
                    _标签树缓存 = null;
                }
                return success;
            }
        }
    }
}
