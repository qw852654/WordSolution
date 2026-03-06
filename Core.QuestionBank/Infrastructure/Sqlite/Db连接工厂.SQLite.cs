using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace TagRunner.数据
{
    /// <summary>
    /// 基于 Microsoft.Data.Sqlite 的数据库连接工厂（完全托管）。
    /// v0：保持同步、线性、无锁、无重试。
    /// </summary>
    public class Db连接工厂SQLite : IDb连接工厂
    {
        private readonly string _数据库文件路径;
        private readonly string _连接字符串;

        public Db连接工厂SQLite(string 数据库文件路径)
        {
            if (string.IsNullOrWhiteSpace(数据库文件路径))
                throw new ArgumentException("数据库文件路径不能为空", nameof(数据库文件路径));

            _数据库文件路径 = Path.GetFullPath(数据库文件路径);
            _连接字符串 = $"Data Source={_数据库文件路径};";
        }

        public IDbConnection 创建连接()
        {
            // 初始化流程负责创建数据库文件；这里不存在就直接抛错，方便定位问题。
            if (!File.Exists(_数据库文件路径))
                throw new FileNotFoundException($"数据库文件不存在: {_数据库文件路径}", _数据库文件路径);

            var conn = new SqliteConnection(_连接字符串);
            conn.Open();

            // 每次打开连接后应用 PRAGMA：代码直观，不需要锁、状态位。
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA synchronous=NORMAL;";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }
    }
}
