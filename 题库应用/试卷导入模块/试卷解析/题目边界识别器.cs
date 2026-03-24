using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 题目边界识别器
    {
        private static readonly Regex 题号起点Regex = new(@"^\s*(?<number>\d+)\s*[\.．、]\s*", RegexOptions.Compiled);
        private static readonly Regex 大题标题Regex = new(@"^\s*[一二三四五六七八九十]+、", RegexOptions.Compiled);

        public bool 是题号起点文本(string 文本)
        {
            return !string.IsNullOrWhiteSpace(文本) && 题号起点Regex.IsMatch(文本);
        }

        public bool 是大题标题文本(string 文本)
        {
            return !string.IsNullOrWhiteSpace(文本) && 大题标题Regex.IsMatch(文本);
        }

        public string 提取题号文本(string 文本)
        {
            var 匹配结果 = 题号起点Regex.Match(文本 ?? string.Empty);
            return 匹配结果.Success ? 匹配结果.Groups["number"].Value : string.Empty;
        }

        public IReadOnlyList<题目边界范围> 识别(IReadOnlyList<试卷段落信息> 段落列表)
        {
            var 起点列表 = 段落列表
                .Where(段落 => !段落.是答案区 && 是题号起点文本(段落.文本))
                .Select(段落 => 段落.索引)
                .ToList();

            var 结果 = new List<题目边界范围>();
            for (var i = 0; i < 起点列表.Count; i++)
            {
                var 开始索引 = 起点列表[i];
                var 结束索引 = i == 起点列表.Count - 1 ? 段落列表[^1].索引 : 起点列表[i + 1] - 1;
                var 起点段落 = 段落列表.First(段落 => 段落.索引 == 开始索引);
                结果.Add(new 题目边界范围
                {
                    开始索引 = 开始索引,
                    结束索引 = 结束索引,
                    题号文本 = 提取题号文本(起点段落.文本),
                });
            }

            return 结果;
        }
    }
}
