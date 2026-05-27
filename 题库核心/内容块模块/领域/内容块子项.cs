using System;

namespace 题库核心.内容块模块.领域
{
    public class 内容块子项
    {
        private 内容块子项()
        {
        }

        private 内容块子项(
            int id,
            int 父内容块ID,
            int 子内容块ID,
            int? 子内容块版本ID,
            内容块引用版本模式 引用版本模式,
            string? 角色,
            int 排序,
            DateTime 创建时间)
        {
            Id = id;
            this.父内容块ID = 父内容块ID;
            this.子内容块ID = 子内容块ID;
            this.子内容块版本ID = 子内容块版本ID;
            this.引用版本模式 = 引用版本模式;
            this.角色 = 角色;
            this.排序 = 排序;
            this.创建时间 = 创建时间;
        }

        public int Id { get; private set; }

        public int 父内容块ID { get; private set; }

        public int 子内容块ID { get; private set; }

        public int? 子内容块版本ID { get; private set; }

        public 内容块引用版本模式 引用版本模式 { get; private set; }

        public string? 角色 { get; private set; }

        public int 排序 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public static 内容块子项 创建(
            int 父内容块ID,
            int 子内容块ID,
            int? 子内容块版本ID,
            内容块引用版本模式 引用版本模式,
            string? 角色,
            int 排序)
        {
            校验ID(父内容块ID, nameof(父内容块ID));
            校验ID(子内容块ID, nameof(子内容块ID));
            if (父内容块ID == 子内容块ID)
            {
                throw new ArgumentException("内容块不能把自己作为子块。", nameof(子内容块ID));
            }

            校验枚举值(引用版本模式, nameof(引用版本模式));
            if (引用版本模式 == 内容块引用版本模式.锁定版本 && (!子内容块版本ID.HasValue || 子内容块版本ID.Value <= 0))
            {
                throw new ArgumentException("锁定版本模式必须提供有效的子内容块版本ID。", nameof(子内容块版本ID));
            }

            if (排序 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(排序));
            }

            return new 内容块子项(
                0,
                父内容块ID,
                子内容块ID,
                引用版本模式 == 内容块引用版本模式.锁定版本 ? 子内容块版本ID : null,
                引用版本模式,
                string.IsNullOrWhiteSpace(角色) ? null : 角色.Trim(),
                排序,
                DateTime.Now);
        }

        public static 内容块子项 从持久化恢复(
            int id,
            int 父内容块ID,
            int 子内容块ID,
            int? 子内容块版本ID,
            内容块引用版本模式 引用版本模式,
            string? 角色,
            int 排序,
            DateTime 创建时间)
        {
            return new 内容块子项(id, 父内容块ID, 子内容块ID, 子内容块版本ID, 引用版本模式, 角色, 排序, 创建时间);
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
