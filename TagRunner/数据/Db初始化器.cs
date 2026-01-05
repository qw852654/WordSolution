using System;
using System.Data.SQLite;
using System.IO;

namespace TagRunner.数据
{
    /// <summary>
    /// 程序化数据库初始化器（覆盖模式）。
    /// 用途：创建或覆盖指定路径的 SQLite 数据库文件，并创建基础表结构：Tags, Questions, QuestionTags。
    /// 注意：该初始化会在覆盖模式下删除已有文件，请谨慎使用。
    /// </summary>
    public static class Db初始化器
    {
        /// <summary>
        /// 初始化数据库文件并创建基础表（覆盖模式可删除已有文件）。
        /// </summary>
        /// <param name="数据库文件路径">目标 SQLite 数据库文件路径。</param>
        /// <param name="覆盖">如果为 true 且文件存在则先删除再创建新库。</param>
        public static void 初始化数据库(string 数据库文件路径, bool 覆盖 = false)
        {
            if (string.IsNullOrWhiteSpace(数据库文件路径))
                throw new ArgumentException("数据库文件路径不能为空", nameof(数据库文件路径));

            var fullPath = Path.GetFullPath(数据库文件路径);
            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(fullPath))
            {
                if (覆盖)
                {
                    File.Delete(fullPath);
                }
                else
                {
                    // 已存在且不覆盖，仍尝试确保表存在
                    CreateSchemaIfNotExists(fullPath);
                    return;
                }
            }

            // 创建空数据库文件
            SQLiteConnection.CreateFile(fullPath);

            CreateSchemaIfNotExists(fullPath);
        }

        private static void CreateSchemaIfNotExists(string dbPath)
        {
            var connStr = $"Data Source={dbPath};Version=3;";
            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();

                // 开启外键支持
                using (var cmdPragma = conn.CreateCommand())
                {
                    cmdPragma.CommandText = "PRAGMA foreign_keys = ON;";
                    cmdPragma.ExecuteNonQuery();
                }

                using (var tran = conn.BeginTransaction())
                using (var cmd = conn.CreateCommand())
                {
                    // Tags 表（自引用父子关系，ParentId+Name 唯一约束）
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Tags (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    ParentId INTEGER NULL,
    Description TEXT,
    PrevSiblingId INTEGER,
    NextSiblingId INTEGER,
    FOREIGN KEY(ParentId) REFERENCES Tags(Id) ON DELETE SET NULL,
    UNIQUE(ParentId, Name)
);
";
                    cmd.ExecuteNonQuery();

                    // Questions 表，包含 LastIndexedAt 便于后续索引增量
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Questions (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Description TEXT,
                                            CreatedTime TEXT NOT NULL,
                                            UpdateTime TEXT NOT NULL,
                                            LastIndexedAt TEXT);";
                    cmd.ExecuteNonQuery();

                    // QuestionTags 中间表
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS QuestionTags (
                                            QuestionId INTEGER NOT NULL,
                                            TagId INTEGER NOT NULL,
                                            PRIMARY KEY (QuestionId, TagId),
                                            FOREIGN KEY(QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE,
                                            FOREIGN KEY(TagId) REFERENCES Tags(Id) ON DELETE CASCADE);";
                    cmd.ExecuteNonQuery();

                    // 索引：按需创建
                    cmd.CommandText = @"CREATE INDEX IF NOT EXISTS IX_QuestionTags_TagId ON QuestionTags(TagId);";
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                }

                conn.Close();
            }
        }
    }
}
