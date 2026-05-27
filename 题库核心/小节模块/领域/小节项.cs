using System;
using 题库核心.内容块模块.领域;

namespace 题库核心.小节模块.领域
{
    public class 小节项
    {
        private 小节项()
        {
        }

        private 小节项(
            int id,
            int 小节ID,
            int 内容块ID,
            int? 内容块版本ID,
            内容块引用版本模式 引用版本模式,
            string? 角色,
            int 排序,
            DateTime 创建时间)
        {
            Id = id;
            this.小节ID = 小节ID;
            this.内容块ID = 内容块ID;
            this.内容块版本ID = 内容块版本ID;
            this.引用版本模式 = 引用版本模式;
            this.角色 = 角色;
            this.排序 = 排序;
            this.创建时间 = 创建时间;
        }

        public int Id { get; private set; }

        public int 小节ID { get; private set; }

        public int 内容块ID { get; private set; }

        public int? 内容块版本ID { get; private set; }

        public 内容块引用版本模式 引用版本模式 { get; private set; }

        public string? 角色 { get; private set; }

        public int 排序 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public static 小节项 创建(
            int 小节ID,
            int 内容块ID,
            int? 内容块版本ID,
            内容块引用版本模式 引用版本模式,
            string? 角色,
            int 排序)
        {
            校验ID(小节ID, nameof(小节ID));
            校验ID(内容块ID, nameof(内容块ID));
            校验枚举值(引用版本模式, nameof(引用版本模式));

            if (引用版本模式 == 内容块引用版本模式.锁定版本 && (!内容块版本ID.HasValue || 内容块版本ID.Value <= 0))
            {
                throw new ArgumentException("锁定版本模式必须提供有效的内容块版本ID。", nameof(内容块版本ID));
            }

            if (排序 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(排序));
            }

            return new 小节项(
                0,
                小节ID,
                内容块ID,
                引用版本模式 == 内容块引用版本模式.锁定版本 ? 内容块版本ID : null,
                引用版本模式,
                string.IsNullOrWhiteSpace(角色) ? null : 角色.Trim(),
                排序,
                DateTime.Now);
        }

        public static 小节项 从持久化恢复(
            int id,
            int 小节ID,
            int 内容块ID,
            int? 内容块版本ID,
            内容块引用版本模式 引用版本模式,
            string? 角色,
            int 排序,
            DateTime 创建时间)
        {
            return new 小节项(id, 小节ID, 内容块ID, 内容块版本ID, 引用版本模式, 角色, 排序, 创建时间);
        }

        public void 修改排序(int 排序)
        {
            if (排序 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(排序));
            }

            this.排序 = 排序;
        }

        private static void 校验ID(int id, string 参数名)
        {
            if (id <= 0)
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
