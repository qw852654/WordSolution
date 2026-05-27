using System;

namespace 题库核心.内容块模块.领域
{
    public class 内容块版本
    {
        private 内容块版本()
        {
        }

        private 内容块版本(
            int id,
            int 内容块ID,
            int 版本号,
            string Docx路径,
            string Html预览路径,
            string? 纯文本内容,
            DateTime 创建时间,
            bool 是否当前版本)
        {
            Id = id;
            this.内容块ID = 内容块ID;
            this.版本号 = 版本号;
            this.Docx路径 = Docx路径;
            this.Html预览路径 = Html预览路径;
            this.纯文本内容 = 纯文本内容;
            this.创建时间 = 创建时间;
            this.是否当前版本 = 是否当前版本;
        }

        public int Id { get; private set; }

        public int 内容块ID { get; private set; }

        public int 版本号 { get; private set; }

        public string Docx路径 { get; private set; } = string.Empty;

        public string Html预览路径 { get; private set; } = string.Empty;

        public string? 纯文本内容 { get; private set; }

        public DateTime 创建时间 { get; private set; }

        public bool 是否当前版本 { get; private set; }

        public static 内容块版本 创建(
            int 内容块ID,
            int 版本号,
            string Docx路径,
            string Html预览路径,
            string? 纯文本内容,
            bool 是否当前版本)
        {
            if (内容块ID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(内容块ID));
            }

            if (版本号 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(版本号));
            }

            if (string.IsNullOrWhiteSpace(Docx路径))
            {
                throw new ArgumentException("Docx路径不能为空。", nameof(Docx路径));
            }

            if (string.IsNullOrWhiteSpace(Html预览路径))
            {
                throw new ArgumentException("Html预览路径不能为空。", nameof(Html预览路径));
            }

            return new 内容块版本(
                0,
                内容块ID,
                版本号,
                Docx路径,
                Html预览路径,
                纯文本内容,
                DateTime.Now,
                是否当前版本);
        }

        public static 内容块版本 从持久化恢复(
            int id,
            int 内容块ID,
            int 版本号,
            string Docx路径,
            string Html预览路径,
            string? 纯文本内容,
            DateTime 创建时间,
            bool 是否当前版本)
        {
            return new 内容块版本(
                id,
                内容块ID,
                版本号,
                Docx路径,
                Html预览路径,
                纯文本内容,
                创建时间,
                是否当前版本);
        }

        public void 设置是否当前版本(bool 是否当前版本)
        {
            this.是否当前版本 = 是否当前版本;
        }
    }
}
