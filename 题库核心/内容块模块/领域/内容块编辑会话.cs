using System;

namespace 题库核心.内容块模块.领域
{
    public class 内容块编辑会话
    {
        private 内容块编辑会话()
        {
        }

        private 内容块编辑会话(
            string 会话ID,
            string 题库键,
            int 内容块ID,
            string 编辑文件路径,
            int? 基准版本ID,
            string 基准Hash,
            DateTime 创建时间)
        {
            this.会话ID = 会话ID;
            this.题库键 = 题库键;
            this.内容块ID = 内容块ID;
            this.编辑文件路径 = 编辑文件路径;
            this.基准版本ID = 基准版本ID;
            this.基准Hash = 基准Hash;
            this.创建时间 = 创建时间;
            状态 = 内容块编辑会话状态.已创建;
        }

        public string 会话ID { get; private set; } = string.Empty;

        public string 题库键 { get; private set; } = string.Empty;

        public int 内容块ID { get; private set; }

        public string 编辑文件路径 { get; private set; } = string.Empty;

        public int? 基准版本ID { get; private set; }

        public string 基准Hash { get; private set; } = string.Empty;

        public int? 最新版本ID { get; private set; }

        public int? 最新版本号 { get; private set; }

        public 内容块编辑会话状态 状态 { get; private set; }

        public string? 消息 { get; private set; }

        public string? 错误信息 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public DateTime? 打开时间 { get; private set; }

        public DateTime? 最近检测时间 { get; private set; }

        public DateTime? 同步时间 { get; private set; }

        public DateTime? 取消时间 { get; private set; }

        public long? 最近文件长度 { get; private set; }

        public DateTime? 最近写入时间Utc { get; private set; }

        public int 稳定检测次数 { get; private set; }

        public bool 最近检测锁文件存在 { get; private set; }

        public bool 最近检测可独占打开 { get; private set; }

        public static 内容块编辑会话 创建(
            string 会话ID,
            string 题库键,
            int 内容块ID,
            string 编辑文件路径,
            int? 基准版本ID,
            string 基准Hash)
        {
            if (string.IsNullOrWhiteSpace(会话ID))
            {
                throw new ArgumentException("会话ID不能为空。", nameof(会话ID));
            }

            if (string.IsNullOrWhiteSpace(题库键))
            {
                throw new ArgumentException("题库键不能为空。", nameof(题库键));
            }

            if (内容块ID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(内容块ID));
            }

            if (string.IsNullOrWhiteSpace(编辑文件路径))
            {
                throw new ArgumentException("编辑文件路径不能为空。", nameof(编辑文件路径));
            }

            if (string.IsNullOrWhiteSpace(基准Hash))
            {
                throw new ArgumentException("基准Hash不能为空。", nameof(基准Hash));
            }

            return new 内容块编辑会话(
                会话ID,
                题库键,
                内容块ID,
                编辑文件路径,
                基准版本ID,
                基准Hash,
                DateTime.Now);
        }

        public void 标记已打开()
        {
            状态 = 内容块编辑会话状态.编辑中;
            打开时间 = DateTime.Now;
            消息 = "Word 已打开，等待文档关闭后同步。";
            错误信息 = null;
        }

        public void 标记编辑中未打开Word()
        {
            状态 = 内容块编辑会话状态.编辑中;
            消息 = "编辑会话已创建，未自动打开 Word。";
            错误信息 = null;
        }

        public void 记录检测(编辑文件检测结果 检测结果)
        {
            最近检测时间 = DateTime.Now;
            最近检测锁文件存在 = 检测结果.锁文件存在;
            最近检测可独占打开 = 检测结果.可独占打开;

            if (检测结果.文件存在
                && !检测结果.锁文件存在
                && 检测结果.可独占打开
                && 最近文件长度 == 检测结果.文件长度
                && 最近写入时间Utc == 检测结果.最后写入时间Utc)
            {
                稳定检测次数++;
            }
            else if (检测结果.文件存在 && !检测结果.锁文件存在 && 检测结果.可独占打开)
            {
                稳定检测次数 = 1;
            }
            else
            {
                稳定检测次数 = 0;
            }

            最近文件长度 = 检测结果.文件长度;
            最近写入时间Utc = 检测结果.最后写入时间Utc;
        }

        public void 标记同步中()
        {
            状态 = 内容块编辑会话状态.同步中;
            消息 = "正在同步编辑文件。";
            错误信息 = null;
        }

        public void 标记已同步(int 最新版本ID, int 最新版本号)
        {
            this.最新版本ID = 最新版本ID;
            this.最新版本号 = 最新版本号;
            状态 = 内容块编辑会话状态.已同步;
            同步时间 = DateTime.Now;
            消息 = $"已同步为 v{最新版本号}。";
            错误信息 = null;
        }

        public void 标记无变化()
        {
            状态 = 内容块编辑会话状态.无变化;
            同步时间 = DateTime.Now;
            消息 = "编辑文件没有变化，未生成新版本。";
            错误信息 = null;
        }

        public void 标记失败(string 错误信息)
        {
            状态 = 内容块编辑会话状态.失败;
            this.错误信息 = 错误信息;
            消息 = "编辑会话同步失败。";
        }

        public void 标记已取消()
        {
            状态 = 内容块编辑会话状态.已取消;
            取消时间 = DateTime.Now;
            消息 = "编辑会话已取消。";
            错误信息 = null;
        }

        public bool 是终态()
        {
            return 状态 == 内容块编辑会话状态.已同步
                || 状态 == 内容块编辑会话状态.无变化
                || 状态 == 内容块编辑会话状态.失败
                || 状态 == 内容块编辑会话状态.已取消;
        }
    }
}
