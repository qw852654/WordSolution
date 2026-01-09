using System;
using TagRunner.Models;

namespace TagRunner.业务
{
    /// <summary>
    /// 组合根（Bootstrapper）：负责在应用启动时组装并初始化题库相关的基础组件与服务。
    /// 主要职责：
    /// - 读取或接收 `题库配置`，确保目录与数据库初始化；
    /// - 创建并持有共享的基础组件实例（Db连接工厂、仓储、文件存储、文档转换器、索引器等）；
    /// - 对外暴露这些共享实例，供 UI 或业务服务在运行时获取。
    /// </summary>
    public static class Bootstrapper
    {
        // 已配置的题库配置对象
        public static 题库配置 配置 { get; private set; }

        // 基础组件（对外可读）
        public static 数据.IDb连接工厂 Db连接工厂 { get; private set; }
        public static 数据.I标签仓储 标签仓储 { get; private set; }
        public static 数据.I题目仓储 题目仓储 { get; private set; }
        public static I文件存储 文件存储 { get; private set; }
        public static 基础.I文档转换器 文档转换器 { get; private set; }
        public static 索引.I索引服务 索引服务 { get; private set; }

        /// <summary>
        /// 初始化组合根并创建必须的组件实例。
        /// - config: 题库配置（包含目录与数据库路径）
        /// - 覆盖数据库: 若为 true 则删除并重新创建数据库文件（按你之前的策略）
        /// </summary>
        public static void Initialize(题库配置 config, bool 覆盖数据库 = false)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 记录配置并确保目录存在
            配置 = config;
            配置.初始化目录(创建如果不存在: true);

            // 初始化数据库（程序化建表，覆盖模式可删除旧 DB）
            数据.Db初始化器.初始化数据库(配置.数据库文件路径, 覆盖数据库);

            // 创建 Db 连接工厂
            Db连接工厂 = new 数据.Db连接工厂SQLite(配置.数据库文件路径);

            // 创建仓储实现（注入连接工厂）
            标签仓储 = new 数据.标签仓储SQLite(Db连接工厂);
            题目仓储 = new 数据.题目仓储SQLite(Db连接工厂);

            // 文件存储与文档转换器
            文件存储 = new 文件存储实现(配置);
            文档转换器 = new 基础.文档转换器();

            // 索引器（骨架），索引路径由配置提供
            索引服务 = new 索引.Lucene索引器(配置.索引目录路径);

            // 启动索引服务（若需要，当前为骨架无需自动启动）
            // 索引服务.启动();
        }
    }
}
