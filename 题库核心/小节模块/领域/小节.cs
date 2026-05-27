using System;

namespace 题库核心.小节模块.领域
{
    public class 小节
    {
        private 小节()
        {
        }

        private 小节(
            int id,
            string 标题,
            string? 摘要,
            int? 章节标签ID,
            小节状态 状态,
            DateTime 创建时间,
            DateTime 更新时间)
        {
            Id = id;
            this.标题 = 标题;
            this.摘要 = 摘要;
            this.章节标签ID = 章节标签ID;
            this.状态 = 状态;
            this.创建时间 = 创建时间;
            this.更新时间 = 更新时间;
        }

        public int Id { get; private set; }

        public string 标题 { get; private set; } = string.Empty;

        public string? 摘要 { get; private set; }

        public int? 章节标签ID { get; private set; }

        public 小节状态 状态 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public DateTime 更新时间 { get; private set; }

        public static 小节 创建(string 标题, string? 摘要, int? 章节标签ID, 小节状态 状态)
        {
            校验标题(标题);
            校验枚举值(状态, nameof(状态));
            校验可选ID(章节标签ID, nameof(章节标签ID));

            var now = DateTime.Now;
            return new 小节(0, 标题.Trim(), 修整可空文本(摘要), 章节标签ID, 状态, now, now);
        }

        public static 小节 从持久化恢复(
            int id,
            string 标题,
            string? 摘要,
            int? 章节标签ID,
            小节状态 状态,
            DateTime 创建时间,
            DateTime 更新时间)
        {
            return new 小节(id, 标题, 摘要, 章节标签ID, 状态, 创建时间, 更新时间);
        }

        public void 修改元数据(string 标题, string? 摘要, int? 章节标签ID, 小节状态 状态)
        {
            校验标题(标题);
            校验枚举值(状态, nameof(状态));
            校验可选ID(章节标签ID, nameof(章节标签ID));

            this.标题 = 标题.Trim();
            this.摘要 = 修整可空文本(摘要);
            this.章节标签ID = 章节标签ID;
            this.状态 = 状态;
            更新时间 = DateTime.Now;
        }

        public void 标记内容已调整()
        {
            更新时间 = DateTime.Now;
        }

        private static string? 修整可空文本(string? 文本)
        {
            return string.IsNullOrWhiteSpace(文本) ? null : 文本.Trim();
        }

        private static void 校验标题(string 标题)
        {
            if (string.IsNullOrWhiteSpace(标题))
            {
                throw new ArgumentException("标题不能为空。", nameof(标题));
            }
        }

        private static void 校验可选ID(int? id, string 参数名)
        {
            if (id.HasValue && id.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(参数名);
            }
        }

        private static void 校验枚举值<TEnum>(TEnum 值, string 参数名)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), 值))
            {
                throw new ArgumentOutOfRangeException(参数名);
            }
        }
    }
}
