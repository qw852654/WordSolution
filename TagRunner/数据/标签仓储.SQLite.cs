using System;
using Dapper;
using TagRunner.Models;

namespace TagRunner.数据
{
    /// <summary>
    /// SQLite 实现的标签仓储（v0）：同步、线性、无自动重试。
    /// </summary>
    public class 标签仓储SQLite : I标签仓储
    {
        private readonly IDb连接工厂 _数据库连接工厂;

        public 标签仓储SQLite(IDb连接工厂 连接工厂)
        {
            _数据库连接工厂 = 连接工厂 ?? throw new ArgumentNullException(nameof(连接工厂));
        }

        public int 新增标签(标签 标签对象)
        {
            if (标签对象 == null) throw new ArgumentNullException(nameof(标签对象));

            var 名称 = 标签对象.Name?.Trim();
            if (string.IsNullOrWhiteSpace(名称)) throw new ArgumentException("标签名称不能为空", nameof(标签对象));

            using (var conn = _数据库连接工厂.创建连接())
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

                long newId = conn.ExecuteScalar<long>("SELECT last_insert_rowid();", transaction: tran);
                标签对象.Id = (int)newId;

                tran.Commit();
                return (int)newId;
            }
        }

        public bool 更新标签(标签 标签对象)
        {
            if (标签对象 == null) throw new ArgumentNullException(nameof(标签对象));
            if (标签对象.Id <= 0) return false;

            using (var conn = _数据库连接工厂.创建连接())
            {
                var exists = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Tags WHERE Id = @Id;", new { Id = 标签对象.Id }) > 0;
                if (!exists) return false;

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

                    tran.Rollback();
                    return false;
                }
            }
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
                return conn.QuerySingleOrDefault<标签>(sql, new { Id = 标签Id });
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
                            AND (
                                (@ParentId IS NULL AND ParentId IS NULL)
                                OR (@ParentId IS NOT NULL AND ParentId = @ParentId)
                            );";
                var count = conn.ExecuteScalar<long>(sql, new { Name = trimmed, ParentId = 父标签Id });
                return count > 0;
            }
        }
    }
}
