using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TagRunner.基础
{
    internal static class Sqlite写重试
    {
        public static T 在写操作重试中执行<T>(Func<T> 操作, int 写重试次数, int 初始重试间隔毫秒)
        {
            if (操作 == null) throw new ArgumentNullException(nameof(操作));
            int attempt = 0;
            int delay = Math.Max(10, 初始重试间隔毫秒);
            int maxAttempts = Math.Max(1, 写重试次数);
            while (true)
            {
                attempt++;
                try
                {
                    return 操作();
                }
                catch (System.Data.SQLite.SQLiteException ex) when (ex.ResultCode == System.Data.SQLite.SQLiteErrorCode.Busy || ex.ResultCode == System.Data.SQLite.SQLiteErrorCode.Locked)
                {
                    if (attempt >= maxAttempts) throw;
                    Thread.Sleep(delay);
                    delay *= 2;
                }
            }
        }
    }
}
