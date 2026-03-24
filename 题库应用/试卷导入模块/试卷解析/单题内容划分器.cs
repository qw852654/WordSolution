using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aspose.Words;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 单题内容划分器
    {
        private static readonly Regex 题号前缀Regex = new(@"^\s*\d+\s*[\.．、]\s*", RegexOptions.Compiled);
        private static readonly Regex 难度段落Regex = new(@"^\s*【难度】", RegexOptions.Compiled);
        private static readonly Regex 知识点段落Regex = new(@"^\s*【知识点】", RegexOptions.Compiled);

        public 单题内容划分结果 划分(Document 源文档, IReadOnlyList<试卷段落信息> 单题段落列表)
        {
            var 正文段落列表 = 单题段落列表.Where(段落 => !段落.是答案区).ToList();
            var 答案正文段落列表 = new List<试卷段落信息>();
            var 难度段落列表 = new List<试卷段落信息>();
            var 知识点段落列表 = new List<试卷段落信息>();

            foreach (var 段落 in 单题段落列表.Where(段落 => 段落.是答案区))
            {
                if (是难度段落(段落.文本))
                {
                    难度段落列表.Add(段落);
                    continue;
                }

                if (是知识点段落(段落.文本))
                {
                    知识点段落列表.Add(段落);
                    continue;
                }

                答案正文段落列表.Add(段落);
            }

            var 完整文档 = 构建文档(源文档, 正文段落列表.Concat(答案正文段落列表).ToList());
            var 正文文档 = 构建文档(源文档, 正文段落列表);

            去除首段题号前缀(完整文档);
            去除首段题号前缀(正文文档);

            return new 单题内容划分结果
            {
                完整Ooxml内容 = 转为FlatOpc(完整文档),
                题目正文Ooxml内容 = 转为FlatOpc(正文文档),
                题目摘要 = 提取题目摘要(正文文档),
                难度段落纯文本 = string.Join("\n", 难度段落列表.Select(段落 => 段落.文本).Where(文本 => !string.IsNullOrWhiteSpace(文本))),
                知识点段落纯文本 = string.Join("\n", 知识点段落列表.Select(段落 => 段落.文本).Where(文本 => !string.IsNullOrWhiteSpace(文本))),
            };
        }

        private static bool 是难度段落(string 文本)
        {
            return !string.IsNullOrWhiteSpace(文本) && 难度段落Regex.IsMatch(文本);
        }

        private static bool 是知识点段落(string 文本)
        {
            return !string.IsNullOrWhiteSpace(文本) && 知识点段落Regex.IsMatch(文本);
        }

        private static Document 构建文档(Document 源文档, IReadOnlyList<试卷段落信息> 段落列表)
        {
            var 新文档 = new Document();
            新文档.RemoveAllChildren();
            var 节 = new Section(新文档);
            新文档.AppendChild(节);
            var 正文 = new Body(新文档);
            节.AppendChild(正文);

            if (段落列表.Count == 0)
            {
                正文.AppendChild(new Paragraph(新文档));
                return 新文档;
            }

            var 导入器 = new NodeImporter(源文档, 新文档, ImportFormatMode.KeepSourceFormatting);
            foreach (var 段落 in 段落列表)
            {
                正文.AppendChild(导入器.ImportNode(段落.段落, true));
            }

            return 新文档;
        }

        private static void 去除首段题号前缀(Document 文档)
        {
            var 首段 = 文档.FirstSection?.Body?.Paragraphs
                .Cast<Paragraph>()
                .FirstOrDefault(段落 => !string.IsNullOrWhiteSpace(获取段落文本(段落)));
            if (首段 == null)
            {
                return;
            }

            var 匹配结果 = 题号前缀Regex.Match(获取段落文本(首段));
            if (!匹配结果.Success)
            {
                return;
            }

            var 剩余待移除字符数 = 匹配结果.Value.Length;
            foreach (Run 运行 in 首段.GetChildNodes(NodeType.Run, true))
            {
                if (剩余待移除字符数 <= 0)
                {
                    break;
                }

                var 当前文本 = 运行.Text ?? string.Empty;
                if (当前文本.Length == 0)
                {
                    continue;
                }

                if (当前文本.Length <= 剩余待移除字符数)
                {
                    运行.Text = string.Empty;
                    剩余待移除字符数 -= 当前文本.Length;
                    continue;
                }

                运行.Text = 当前文本[剩余待移除字符数..];
                break;
            }
        }

        private static string 提取题目摘要(Document 文档)
        {
            var 首段文本 = 文档.FirstSection?.Body?.Paragraphs
                .Cast<Paragraph>()
                .Select(获取段落文本)
                .FirstOrDefault(文本 => !string.IsNullOrWhiteSpace(文本))
                ?.Trim() ?? string.Empty;

            if (首段文本.Length <= 60)
            {
                return 首段文本;
            }

            return 首段文本[..60];
        }

        private static string 获取段落文本(Paragraph 段落)
        {
            return 段落.ToString(SaveFormat.Text).Trim();
        }

        private static string 转为FlatOpc(Document 文档)
        {
            using var 输出流 = new MemoryStream();
            文档.Save(输出流, SaveFormat.FlatOpc);
            return System.Text.Encoding.UTF8.GetString(输出流.ToArray());
        }
    }
}
