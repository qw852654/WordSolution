using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace TagRunner.数据
{
    /// <summary>
    /// 基于 SQLite 的数据库连接工厂。
    /// 责任：
    /// - 封装数据库文件路径与连接字符串；
    /// - 提供创建配置好（已打开）的 IDbConnection；
    /// - 在首次连接时应用必要的 PRAGMA（如 WAL / synchronous=NORMAL）。
    /// 注意：本工厂不负责创建数据库文件；若在创建连接时找不到数据库文件，会抛出 FileNotFoundException。
    /// 数据库的创建由外部的初始化逻辑负责（例如 初始化题库 方法）。
    /// </summary>
    public class Db连接工厂SQLite : IDb连接工厂
    {
        private readonly string _数据库文件路径;
        private readonly string _连接字符串;
        private readonly object _pragma锁 = new object();
        private bool _pragma已应用;
        private readonly int _最大重试次数;
        private readonly int _初始重试间隔毫秒;

        /// <summary>
        /// 构造函数，接受 SQLite 数据库文件路径。
        /// 构造时不创建数据库文件或目录；若需要创建请调用单独的初始化方法。
        /// </summary>
        /// <param name="数据库文件路径">SQLite 数据库文件完整路径。</param>
        /// <param name="最大重试次数">遇到 BUSY/LOCKED 时的最大重试次数，默认 3 次。</param>
        /// <param name="初始重试间隔毫秒">重试的初始间隔（毫秒），采用指数退避。</param>
        public Db连接工厂SQLite(string 数据库文件路径, int 最大重试次数 = 3, int 初始重试间隔毫秒 = 100)
        {
            if (string.IsNullOrWhiteSpace(数据库文件路径))
                throw new ArgumentException("数据库文件路径不能为空", nameof(数据库文件路径));

            _数据库文件路径 = Path.GetFullPath(数据库文件路径);

            // 不在此处创建目录或数据库文件；数据库的创建由外部初始化流程负责。
            _连接字符串 = $"Data Source={_数据库文件路径};Version=3;Pooling=True;";
            _pragma已应用 = false;
            _最大重试次数 = Math.Max(1, 最大重试次数);
            _初始重试间隔毫秒 = Math.Max(10, 初始重试间隔毫秒);
        }

        /// <summary>
        /// 创建并返回一个已打开的 IDbConnection。
        /// - 在首次成功打开连接后会应用必要的 PRAGMA。
        /// - 遇到 SQLite BUSY/LOCKED 错误会做短重试（指数退避）。
        /// - 如果数据库文件不存在，则抛出 FileNotFoundException（不自动创建）。
        /// 调用方负责关闭/处置返回的连接。
        /// </summary>
        public IDbConnection 创建连接()
        {
            // 在尝试打开连接前，明确检查数据库文件是否存在；如果不存在，抛出异常，避免隐式创建
            if (!File.Exists(_数据库文件路径))
                throw new FileNotFoundException($"数据库文件不存在: {_数据库文件路径}", _数据库文件路径);

            SQLiteConnection conn = null;
            int attempt = 0;
            int delay = _初始重试间隔毫秒;

            while (true)
            {
                attempt++;
                try
                {
                    conn = new SQLiteConnection(_连接字符串);
                    conn.Open();

                    // 在首次连接时应用 PRAGMA（线程安全）
                    ApplyPragmaIfNeeded(conn);

                    return conn; // 返回已打开的连接，由调用方负责 Dispose
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Busy || ex.ResultCode == SQLiteErrorCode.Locked)
                {
                    // 处理短重试
                    conn?.Dispose();
                    if (attempt >= _最大重试次数)
                        throw; // 达到上限，向上抛出异常
                    Thread.Sleep(delay);
                    delay *= 2;
                    continue;
                }
                catch
                {
                    conn?.Dispose();
                    throw;
                }
            }
        }

        /// <summary>
        /// 在第一次成功连接后应用必要的 PRAGMA 设置（WAL 与同步级别）。
        /// 该方法线程安全且只会执行一次（进程内）。
        /// </summary>
        private void ApplyPragmaIfNeeded(SQLiteConnection conn)
        {
            if (_pragma已应用) return;

            lock (_pragma锁)
            {
                if (_pragma已应用) return;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA journal_mode=WAL;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "PRAGMA synchronous=NORMAL;";
                    cmd.ExecuteNonQuery();
                }

                _pragma已应用 = true;
            }
        }
    }
}
