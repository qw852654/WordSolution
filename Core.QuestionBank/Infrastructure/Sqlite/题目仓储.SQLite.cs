using Core.QuestionBank.Domain;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TagRunner.数据
{
    /// <summary>
    /// SQLite 实现的题目仓储（方案A：QuestionTags 中间表）。
    /// v0：保持同步、线性、无自动重试。
    /// </summary>
    public class 题目仓储SQLite : I题目仓储
    {
        private readonly IDb连接工厂 _连接工厂;

        public 题目仓储SQLite(IDb连接工厂 连接工厂)
        {
            _连接工厂 = 连接工厂 ?? throw new ArgumentNullException(nameof(连接工厂));
        }

        public 题目 Id获取题目(int 题目Id)
        {
            if (题目Id <= 0) return null;

            using (var conn = _连接工厂.创建连接())
            {
                var sql = @"SELECT Id, Description, CreatedTime, UpdateTime FROM Questions WHERE Id = @Id LIMIT 1;";
                var q = conn.QuerySingleOrDefault<题目>(sql, new { Id = 题目Id });
                if (q == null) return null;

                var tagSql = "SELECT TagId FROM QuestionTags WHERE QuestionId = @Id;";
                var tagIds = conn.Query<int>(tagSql, new { Id = 题目Id });
                q.TagIDs = tagIds.ToList();
                return q;
            }
        }

        public int 新增题目(题目 新题目)
        {
            if (新题目 == null) throw new ArgumentNullException(nameof(新题目));

            using (var conn = _连接工厂.创建连接())
            using (var tran = conn.BeginTransaction())
            {
                var now = DateTime.UtcNow;
                var insertSql = @"INSERT INTO Questions (Description, CreatedTime, UpdateTime) VALUES (@Description, @CreatedTime, @UpdateTime);";
                conn.Execute(insertSql, new { Description = 新题目.Description, CreatedTime = now, UpdateTime = now }, tran);

                var newId = conn.ExecuteScalar<long>("SELECT last_insert_rowid();", transaction: tran);

                // 插入标签关系
                if (新题目.TagIDs != null)
                {
                    var tagInsertSql = "INSERT INTO QuestionTags (QuestionId, TagId) VALUES (@QuestionId, @TagId);";
                    foreach (var tagId in 新题目.TagIDs.Distinct())
                    {
                        conn.Execute(tagInsertSql, new { QuestionId = (int)newId, TagId = tagId }, tran);
                    }
                }

                tran.Commit();
                return (int)newId;
            }
        }

        public bool 更新题目(题目 修改题目)
        {
            if (修改题目 == null) throw new ArgumentNullException(nameof(修改题目));
            if (修改题目.Id <= 0) return false;

            using (var conn = _连接工厂.创建连接())
            {
                // 检查存在性
                var exists = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Questions WHERE Id = @Id;", new { Id = 修改题目.Id }) > 0;
                if (!exists) return false;

                using (var tran = conn.BeginTransaction())
                {
                    var now = DateTime.UtcNow;
                    var updateSql = "UPDATE Questions SET Description = @Description, UpdateTime = @UpdateTime WHERE Id = @Id;";
                    conn.Execute(updateSql, new { Description = 修改题目.Description, UpdateTime = now, Id = 修改题目.Id }, tran);

                    // 更新标签关系：删除旧的，插入新的
                    conn.Execute("DELETE FROM QuestionTags WHERE QuestionId = @Id;", new { Id = 修改题目.Id }, tran);
                    if (修改题目.TagIDs != null)
                    {
                        var tagInsertSql = "INSERT INTO QuestionTags (QuestionId, TagId) VALUES (@QuestionId, @TagId);";
                        foreach (var tagId in 修改题目.TagIDs.Distinct())
                        {
                            conn.Execute(tagInsertSql, new { QuestionId = 修改题目.Id, TagId = tagId }, tran);
                        }
                    }

                    tran.Commit();
                    return true;
                }
            }
        }

        public bool 删除题目(int 题目Id)
        {
            if (题目Id <= 0) return false;

            using (var conn = _连接工厂.创建连接())
            using (var tran = conn.BeginTransaction())
            {
                conn.Execute("DELETE FROM QuestionTags WHERE QuestionId = @Id;", new { Id = 题目Id }, tran);
                var affected = conn.Execute("DELETE FROM Questions WHERE Id = @Id;", new { Id = 题目Id }, tran);
                tran.Commit();
                return affected > 0;
            }
        }

        public List<题目> 按标签查询(IEnumerable<标签> 标签列表)
        {
            if (标签列表 == null) return new List<题目>();
            var tagIds = 标签列表.Select(t => t.Id).Distinct().ToList();
            if (tagIds.Count == 0) return new List<题目>();

            using (var conn = _连接工厂.创建连接())
            {
                if (tagIds.Count == 1)
                {
                    var sql = @"SELECT q.Id, q.Description, q.CreatedTime, q.UpdateTime
                                FROM Questions q
                                JOIN QuestionTags qt ON q.Id = qt.QuestionId
                                WHERE qt.TagId = @TagId;";
                    var rows = conn.Query<题目>(sql, new { TagId = tagIds[0] });
                    var list = rows.ToList();
                    // fill TagIDs for each question
                    foreach (var q in list)
                    {
                        var ids = conn.Query<int>("SELECT TagId FROM QuestionTags WHERE QuestionId = @Id;", new { Id = q.Id });
                        q.TagIDs = ids.ToList();
                    }
                    return list;
                }
                else
                {
                    // 找出同时包含所有 tagIds 的 QuestionId
                    var inClause = string.Join(",", tagIds);
                    var sql = $@"SELECT QuestionId FROM QuestionTags WHERE TagId IN ({inClause}) GROUP BY QuestionId HAVING COUNT(DISTINCT TagId) = {tagIds.Count};";
                    var qids = conn.Query<int>(sql).ToList();
                    var result = new List<题目>();
                    foreach (var id in qids)
                    {
                        var q = Id获取题目(id);
                        if (q != null) result.Add(q);
                    }
                    return result;
                }
            }
        }
    }
}
