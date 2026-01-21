using System;

namespace TagRunner.基础
{
    internal static class Sqlite写重试
    {
        public static T 在写操作中执行<T>(Func<T> 操作)
        {
            if (操作 == null) throw new ArgumentNullException(nameof(操作));
            return 操作();
        }
    }
}
