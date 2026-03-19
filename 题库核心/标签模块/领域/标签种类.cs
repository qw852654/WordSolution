using System;

namespace 题库核心.标签模块.领域
{
    public class 标签种类
    {
        private 标签种类()
        {
        }

        private 标签种类(
            int id,
            string 名称,
            bool 是否树形,
            bool 是否允许多选,
            bool 是否系统内置,
            bool 是否在正式工作流中可见)
        {
            if (string.IsNullOrWhiteSpace(名称))
            {
                throw new ArgumentException("名称不能为空。", nameof(名称));
            }

            Id = id;
            this.名称 = 名称.Trim();
            this.是否树形 = 是否树形;
            this.是否允许多选 = 是否允许多选;
            this.是否系统内置 = 是否系统内置;
            this.是否在正式工作流中可见 = 是否在正式工作流中可见;
        }

        public int Id { get; private set; }

        public string 名称 { get; private set; } = string.Empty;

        public bool 是否树形 { get; private set; }

        public bool 是否允许多选 { get; private set; }

        public bool 是否系统内置 { get; private set; }

        public bool 是否在正式工作流中可见 { get; private set; }

        public static 标签种类 创建(
            int id,
            string 名称,
            bool 是否树形,
            bool 是否允许多选,
            bool 是否系统内置,
            bool 是否在正式工作流中可见)
        {
            return new 标签种类(id, 名称, 是否树形, 是否允许多选, 是否系统内置, 是否在正式工作流中可见);
        }

        public static 标签种类 从持久化恢复(
            int id,
            string 名称,
            bool 是否树形,
            bool 是否允许多选,
            bool 是否系统内置,
            bool 是否在正式工作流中可见)
        {
            return new 标签种类(id, 名称, 是否树形, 是否允许多选, 是否系统内置, 是否在正式工作流中可见);
        }
    }
}
