using System.Text.RegularExpressions;

namespace 题库应用.试卷导入模块
{
    public static class 知识点文本规范化器
    {
        public static string 规范化(string 原始文本)
        {
            if (string.IsNullOrWhiteSpace(原始文本))
            {
                return string.Empty;
            }

            var 修整后文本 = 原始文本.Trim().Replace('　', ' ');
            修整后文本 = Regex.Replace(修整后文本, @"\s+", " ");
            return 修整后文本.ToLowerInvariant();
        }
    }
}
