using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Dapper;
using TagRunner.Models;

namespace TagRunner.数据
{
    /// <summary>
    /// SQLite 实现的标签仓储（骨架）。
    /// 头部包含：私有字段、构造函数、接口方法签名以及写操作重试逻辑示例和创建连接的使用范例。
    /// 实际方法实现将在后续分步提交（按你的要求逐方法实现）。
    /// </summary>
    public class 标签仓储SQLite : I标签仓储
    {
        // 私有字段（存放依赖与配置）
        private readonly IDb连接工厂 _数据库连接工厂; // 用于创建 IDbConnection
        private readonly int _写重试次数; // 遇到 BUSY/LOCKED 时的最大重试次数
        private readonly int _初始重试间隔毫秒; // 重试的初始间隔，指数退避

        // 构造函数：注入连接工厂和（可选）重试配置
        public 标签仓储SQLite(IDb连接工厂 连接工厂, int 写重试次数 = 3, int 初始重试间隔毫秒 = 500)
        {
            _数据库连接工厂 = 连接工厂 ?? throw new ArgumentNullException(nameof(连接工厂));
            _写重试次数 = Math.Max(1, 写重试次数);
            _初始重试间隔毫秒 = Math.Max(10, 初始重试间隔毫秒);
        }

        // 接口方法签名（已实现 插入标签，下面实现 Id获取标签）


        public int 插入标签(标签 标签对象)
        {
            if (标签对象 == null) throw new ArgumentNullException(nameof(标签对象));

            var 名称 = 标签对象.Name?.Trim();
            if (string.IsNullOrWhiteSpace(名称)) throw new ArgumentException("标签名称不能为空", nameof(标签对象));

            return 在写操作重试中执行(() =>
            {
                using (var conn = _数据库连接工厂.创建连接())
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        var sql = @"INSERT INTO Tags (Name, ParentId, Description, PrevSiblingId, NextSiblingId)
                                    VALUES (@Name, @ParentId, @Description, @PrevSiblingId, @NextSiblingId);";

                        conn.Execute(sql, new
                        {
                            Name = 名称,
                            ParentId = 标签对象.ParentId,
                            Description = 标签对象.Description,
                            PrevSiblingId = 标签对象.PrevSiblingId,
                            NextSiblingId = 标签对象.NextSiblingId
                        }, tran);

                        // 获取刚插入行的 rowid
                        long newId = conn.ExecuteScalar<long>("SELECT last_insert_rowid();", transaction: tran);
                        标签对象.Id = (int)newId;

                        tran.Commit();
                        return (int)newId;
                    }
                }
            });
        }

        public bool 更新标签(标签 标签对象)
        {
            if (标签对象 == null) throw new ArgumentNullException(nameof(标签对象));
            if (标签对象.Id <= 0) return false;

            return 在写操作重试中执行(() =>
            {
                using (var conn = _数据库连接工厂.创建连接())
                {
                    // 先通过 ID 检查记录是否存在
                    var exists = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Tags WHERE Id = @Id;", new { Id = 标签对象.Id }) > 0;
                    if (!exists)
                        return false;

                    using (var tran = conn.BeginTransaction())
                    {
                        var sql = @"UPDATE Tags 
                                        SET Name = @Name, ParentId = @ParentId, Description = @Description, PrevSiblingId = @PrevSiblingId, NextSiblingId = @NextSiblingId 
                                        WHERE Id = @Id;";
                        var affected = conn.Execute(sql, new
                        {
                            Name = 标签对象.Name?.Trim(),
                            ParentId = 标签对象.ParentId,
                            Description = 标签对象.Description,
                            PrevSiblingId = 标签对象.PrevSiblingId,
                            NextSiblingId = 标签对象.NextSiblingId,
                            Id = 标签对象.Id
                        }, tran);

                        if (affected > 0)
                        {
                            tran.Commit();
                            return true;
                        }
                        else
                        {
                            tran.Rollback();
                            return false;
                        }
                    }
                }
            });
        }

        public bool 删除标签(标签 待删除标签)
        {
            throw new NotImplementedException();
        }

        public 标签 Id获取标签(int 标签Id)
        {
            if (标签Id <= 0) return null;

            using (var conn = _数据库连接工厂.创建连接())
            {
                var sql = @"SELECT Id, Name, ParentId, Description, PrevSiblingId, NextSiblingId FROM Tags WHERE Id = @Id LIMIT 1;";
                var result = conn.QuerySingleOrDefault<标签>(sql, new { Id = 标签Id });
                return result;
            }
        }

        public bool 存在同名标签(string 名称, int? 父标签Id)
        {
            if (string.IsNullOrWhiteSpace(名称)) return false;
            var trimmed = 名称.Trim();

            using (var conn = _数据库连接工厂.创建连接())
            {
                var sql = @"SELECT COUNT(1) FROM Tags 
                                WHERE Name = @Name 
                                AND ((@ParentId IS NULL AND ParentId IS NULL) OR ParentId = @ParentId);";
                var count = conn.ExecuteScalar<long>(sql, new { Name = trimmed, ParentId = 父标签Id });
                return count > 0;
            }
        }

        // 私有帮助方法：写操作重试模板（示例）
        // 使用示例（伪代码，实际 SQL 在实现时提供）：
        // 在 插入/更新/删除 等写方法中调用：
        // return 在写操作重试中执行(() => { using (var conn = _连接工厂.创建连接()) { // 执行 Dapper 的 Execute/Query } });
        private T 在写操作重试中执行<T>(Func<T> 操作)
        {
            int attempt = 0;
            int delay = _初始重试间隔毫秒;
            while (true)
            {
                attempt++;
                try
                {
                    return 操作();
                }
                catch (System.Data.SQLite.SQLiteException ex) when (ex.ResultCode == System.Data.SQLite.SQLiteErrorCode.Busy || ex.ResultCode == System.Data.SQLite.SQLiteErrorCode.Locked)
                {
                    if (attempt >= _写重试次数)
                        throw;
                    Thread.Sleep(delay);
                    delay *= 2;
                    continue;
                }
            }
        }


        // 创建连接的使用范例（示例代码片段：请在方法实现中使用）
        // using (var conn = _连接工厂.创建连接())
        // {
        //     // 查询示例（Dapper）
        //     var rows = conn.Query<TagRunner.标签>("SELECT Id AS Id, Name AS Name, ParentId AS ParentId, Category AS Category, NumericValue AS NumericValue FROM Tags");
        //     return rows;
        // }
    }
}
