using System.Data;

namespace TagRunner.数据
{
    /// <summary>
    /// 抽象数据库连接工厂，负责创建并返回配置好的 IDbConnection 实例。
    /// 实现应封装连接字符串、SQLite PRAGMA 初始化等细节。
    /// </summary>
    public interface IDb连接工厂
    {
        /// <summary>
        /// 创建并返回一个新的 IDbConnection（实现可选择是否已打开）。
        /// 调用方负责关闭/处置连接或使用 using 块。
        /// </summary>
        IDbConnection 创建连接();
    }
}
