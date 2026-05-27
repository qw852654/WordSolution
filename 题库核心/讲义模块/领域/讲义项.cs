using System;
using 题库核心.内容块模块.领域;

namespace 题库核心.讲义模块.领域
{
    public class 讲义项
    {
        private 讲义项()
        {
        }

        private 讲义项(
            int id,
            int 讲义ID,
            讲义项目标类型 目标类型,
            int 目标ID,
            内容块引用版本模式 引用版本模式,
            int? 锁定内容块版本ID,
            string? 角色,
            int 排序,
            DateTime 创建时间)
        {
            Id = id;
            this.讲义ID = 讲义ID;
            this.目标类型 = 目标类型;
            this.目标ID = 目标ID;
            this.引用版本模式 = 引用版本模式;
            this.锁定内容块版本ID = 锁定内容块版本ID;
            this.角色 = 角色;
            this.排序 = 排序;
            this.创建时间 = 创建时间;
        }

        public int Id { get; private set; }

        public int 讲义ID { get; private set; }

        public 讲义项目标类型 目标类型 { get; private set; }

        public int 目标ID { get; private set; }

        public 内容块引用版本模式 引用版本模式 { get; private set; }

        public int? 锁定内容块版本ID { get; private set; }

        public string? 角色 { get; private set; }

        public int 排序 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public static 讲义项 创建(
            int 讲义ID,
            讲义项目标类型 目标类型,
            int 目标ID,
            内容块引用版本模式 引用版本模式,
            int? 锁定内容块版本ID,
            string? 角色,
            int 排序)
        {
            校验ID(讲义ID, nameof(讲义ID));
            校验ID(目标ID, nameof(目标ID));
            校验枚举值(目标类型, nameof(目标类型));
            校验枚举值(引用版本模式, nameof(引用版本模式));

            if (目标类型 == 讲义项目标类型.内容块
                && 引用版本模式 == 内容块引用版本模式.锁定版本
                && (!锁定内容块版本ID.HasValue || 锁定内容块版本ID.Value <= 0))
            {
                throw new ArgumentException("直接引用内容块并锁定版本时，必须提供有效的内容块版本ID。", nameof(锁定内容块版本ID));
            }

            if (排序 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(排序));
            }

            return new 讲义项(
                0,
                讲义ID,
                目标类型,
                目标ID,
                引用版本模式,
                目标类型 == 讲义项目标类型.内容块 && 引用版本模式 == 内容块引用版本模式.锁定版本 ? 锁定内容块版本ID : null,
                string.IsNullOrWhiteSpace(角色) ? null : 角色.Trim(),
                排序,
                DateTime.Now);
        }

        public static 讲义项 从持久化恢复(
            int id,
            int 讲义ID,
            讲义项目标类型 目标类型,
            int 目标ID,
            内容块引用版本模式 引用版本模式,
            int? 锁定内容块版本ID,
            string? 角色,
            int 排序,
            DateTime 创建时间)
        {
            return new 讲义项(id, 讲义ID, 目标类型, 目标ID, 引用版本模式, 锁定内容块版本ID, 角色, 排序, 创建时间);
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
