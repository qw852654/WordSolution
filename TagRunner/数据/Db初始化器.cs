using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace TagRunner.数据
{
    /// <summary>
    /// 数据库初始化器（v0）：确保 SQLite 数据库文件存在，并创建所需表结构。
    /// 说明：
    /// - 使用 Microsoft.Data.Sqlite（完全托管）；
    /// - SQLite 的 db 文件会在第一次打开连接时自动创建，因此不需要 CreateFile。
    /// </summary>
    public static class Db初始化器
    {
        /// <summary>
        /// 初始化数据库文件与表结构。
        /// </summary>
        /// <param name="数据库文件路径">目标 SQLite 数据库文件路径。</param>
        /// <param name="覆盖">为 true 时删除旧 db 文件重新创建。</param>
        public static void 初始化数据库(string 数据库文件路径, bool 覆盖 = false)
        {
            if (string.IsNullOrWhiteSpace(数据库文件路径))
                throw new ArgumentException("数据库文件路径不能为空", nameof(数据库文件路径));

            var fullPath = Path.GetFullPath(数据库文件路径);
            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(fullPath) && 覆盖)
                File.Delete(fullPath);

            CreateSchemaIfNotExists(fullPath);
        }

        private static void CreateSchemaIfNotExists(string dbPath)
        {
            // Microsoft.Data.Sqlite 的连接字符串只需要 Data Source
            var connStr = $"Data Source={dbPath};";

            using (var conn = new SqliteConnection(connStr))
            {
                conn.Open();

                // 打开外键约束（SQLite 默认是 OFF）
                using (var cmdPragma = conn.CreateCommand())
                {
                    cmdPragma.CommandText = "PRAGMA foreign_keys = ON;";
                    cmdPragma.ExecuteNonQuery();
                }

                using (var tran = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tran;

                        // 1) Tags 表：存标签树结构。UNIQUE(ParentId, Name) 防止同父重名。
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Tags (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Name TEXT NOT NULL,
                                                ParentId INTEGER NULL,
                                                Description TEXT,
                                                PrevSiblingId INTEGER,
                                                NextSiblingId INTEGER,
                                                FOREIGN KEY(ParentId) REFERENCES Tags(Id) ON DELETE SET NULL,
                                                UNIQUE(ParentId, Name)
                                            );";
                        cmd.ExecuteNonQuery();

                        // 2) Questions 表：题目元数据
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Questions (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Description TEXT,
                                                CreatedTime TEXT NOT NULL,
                                                UpdateTime TEXT NOT NULL,
                                                LastIndexedAt TEXT,
                                                Content TEXT
                                            );";
                        cmd.ExecuteNonQuery();

                        // 3) QuestionTags 中间表：题目-标签 多对多
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS QuestionTags (
                                                QuestionId INTEGER NOT NULL,
                                                TagId INTEGER NOT NULL,
                                                PRIMARY KEY (QuestionId, TagId),
                                                FOREIGN KEY(QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE,
                                                FOREIGN KEY(TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                                            );";
                        cmd.ExecuteNonQuery();

                        // 4) 索引：按 TagId 查题目会更快（虽然 v0 不追求性能，但索引不影响理解）
                        cmd.CommandText = @"CREATE INDEX IF NOT EXISTS IX_QuestionTags_TagId ON QuestionTags(TagId);";
                        cmd.ExecuteNonQuery();

                        // 5) （保留你原来的“系统根标签 Id=0”逻辑）
                        cmd.CommandText = @"INSERT OR IGNORE INTO Tags (Id, Name, ParentId, Description, PrevSiblingId, NextSiblingId)
                                            VALUES (0, '根', NULL, '系统根标签记录', NULL, NULL);";
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
            }
        }
    }
}
