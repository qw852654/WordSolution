using System;

namespace 题库核心.内容块模块.领域
{
    public class 内容块
    {
        private 内容块()
        {
        }

        private 内容块(
            int id,
            string 标题,
            string? 摘要,
            内容块类型 类型,
            内容块状态 状态,
            int? 当前版本ID,
            内容块结构类型 结构类型,
            bool 是否允许子块,
            DateTime 创建时间,
            DateTime 更新时间)
        {
            Id = id;
            this.标题 = 标题;
            this.摘要 = 摘要;
            this.类型 = 类型;
            this.状态 = 状态;
            this.当前版本ID = 当前版本ID;
            this.结构类型 = 结构类型;
            this.是否允许子块 = 是否允许子块;
            this.创建时间 = 创建时间;
            this.更新时间 = 更新时间;
        }

        public int Id { get; private set; }

        public string 标题 { get; private set; } = string.Empty;

        public string? 摘要 { get; private set; }

        public 内容块类型 类型 { get; private set; }

        public 内容块状态 状态 { get; private set; }

        public int? 当前版本ID { get; private set; }

        public 内容块结构类型 结构类型 { get; private set; }

        public bool 是否允许子块 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public DateTime 更新时间 { get; private set; }

        public static 内容块 创建(
            string 标题,
            string? 摘要,
            内容块类型 类型,
            内容块状态 状态,
            内容块结构类型? 结构类型 = null,
            bool? 是否允许子块 = null)
        {
            校验标题(标题);
            校验枚举值(类型, nameof(类型));
            校验枚举值(状态, nameof(状态));
            var 实际结构类型 = 结构类型 ?? 获取默认结构类型(类型);
            校验结构规则(类型, 实际结构类型, 是否允许子块);

            var now = DateTime.Now;
            return new 内容块(
                0,
                标题.Trim(),
                摘要,
                类型,
                状态,
                null,
                实际结构类型,
                是否允许子块 ?? 实际结构类型 == 内容块结构类型.组合块,
                now,
                now);
        }

        public static 内容块 从持久化恢复(
            int id,
            string 标题,
            string? 摘要,
            内容块类型 类型,
            内容块状态 状态,
            int? 当前版本ID,
            内容块结构类型 结构类型,
            bool 是否允许子块,
            DateTime 创建时间,
            DateTime 更新时间)
        {
            return new 内容块(id, 标题, 摘要, 类型, 状态, 当前版本ID, 结构类型, 是否允许子块, 创建时间, 更新时间);
        }

        public void 修改元数据(
            string 标题,
            string? 摘要,
            内容块类型 类型,
            内容块状态 状态,
            内容块结构类型? 结构类型 = null,
            bool? 是否允许子块 = null)
        {
            校验标题(标题);
            校验枚举值(类型, nameof(类型));
            校验枚举值(状态, nameof(状态));
            var 实际结构类型 = 结构类型 ?? this.结构类型;
            var 实际是否允许子块 = 是否允许子块 ?? this.是否允许子块;
            校验结构规则(类型, 实际结构类型, 实际是否允许子块);

            this.标题 = 标题.Trim();
            this.摘要 = 摘要;
            this.类型 = 类型;
            this.状态 = 状态;
            this.结构类型 = 实际结构类型;
            this.是否允许子块 = 实际是否允许子块;
            更新时间 = DateTime.Now;
        }

        public void 设置当前版本(int 内容块版本ID)
        {
            if (内容块版本ID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(内容块版本ID));
            }

            当前版本ID = 内容块版本ID;
            更新时间 = DateTime.Now;
        }

        private static void 校验标题(string 标题)
        {
            if (string.IsNullOrWhiteSpace(标题))
            {
                throw new ArgumentException("标题不能为空。", nameof(标题));
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

        private static 内容块结构类型 获取默认结构类型(内容块类型 类型)
        {
            return 类型 == 内容块类型.题组
                || 类型 == 内容块类型.小节
                || 类型 == 内容块类型.练习组
                || 类型 == 内容块类型.专题片段
                    ? 内容块结构类型.组合块
                    : 内容块结构类型.原子块;
        }

        private static void 校验结构规则(内容块类型 类型, 内容块结构类型 结构类型, bool? 是否允许子块)
        {
            校验枚举值(结构类型, nameof(结构类型));

            if (类型 == 内容块类型.题目 && 结构类型 == 内容块结构类型.组合块)
            {
                throw new ArgumentException("题目内容块必须是原子块，不能作为组合块。", nameof(结构类型));
            }

            if (是否允许子块.HasValue)
            {
                if (结构类型 == 内容块结构类型.原子块 && 是否允许子块.Value)
                {
                    throw new ArgumentException("原子块不能允许子块。", nameof(是否允许子块));
                }

                if (结构类型 == 内容块结构类型.组合块 && !是否允许子块.Value)
                {
                    throw new ArgumentException("组合块必须允许子块。", nameof(是否允许子块));
                }
            }
        }
    }
}
