using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aspose.Words;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 试卷元信息提取器
    {
        private static readonly Regex 难度Regex = new(@"【难度】(?<value>.*?)(?=【|$)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex 知识点Regex = new(@"【知识点】(?<value>.*?)(?=【|$)", RegexOptions.Singleline | RegexOptions.Compiled);

        public string 提取显示名称(Document 文档, string 默认文件名)
        {
            var 标题文本 = 文档.FirstSection?.Body?.Paragraphs
                .Cast<Paragraph>()
                .Select(段落 => 段落.ToString(SaveFormat.Text).Trim())
                .FirstOrDefault(文本 => !string.IsNullOrWhiteSpace(文本));

            if (!string.IsNullOrWhiteSpace(标题文本))
            {
                return 标题文本.Trim();
            }

            return Path.GetFileNameWithoutExtension(默认文件名);
        }

        public 答案元信息 提取答案元信息(string 难度段落纯文本, string 知识点段落纯文本)
        {
            var 难度文本 = 提取字段值(难度Regex, 难度段落纯文本);
            var 知识点文本 = 提取字段值(知识点Regex, 知识点段落纯文本);

            return new 答案元信息
            {
                原始难度文本 = 难度文本,
                原始知识点列表 = 拆分知识点文本(知识点文本),
            };
        }

        private static string 提取字段值(Regex regex, string 文本)
        {
            if (string.IsNullOrWhiteSpace(文本))
            {
                return string.Empty;
            }

            var 匹配结果 = regex.Match(文本);
            return 匹配结果.Success ? 匹配结果.Groups["value"].Value.Trim() : string.Empty;
        }

        private static IReadOnlyList<string> 拆分知识点文本(string 知识点文本)
        {
            if (string.IsNullOrWhiteSpace(知识点文本))
            {
                return Array.Empty<string>();
            }

            return 知识点文本
                .Split(new[] { '、', '，', ',', '；', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(文本 => 文本.Trim())
                .Where(文本 => 文本.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
