using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace 题库应用.题目模块.题型识别.题型特征
{
    public class Ooxml题型特征提取器
    {
        private static readonly Regex 选项A正则 = new(@"(?:(?<=^)|(?<=[\s（(；;、，。,]))A(?:[\.．、]|\s)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex 选项B正则 = new(@"(?:(?<=^)|(?<=[\s（(；;、，。,]))B(?:[\.．、]|\s)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex 选项C正则 = new(@"(?:(?<=^)|(?<=[\s（(；;、，。,]))C(?:[\.．、]|\s)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex 选项D正则 = new(@"(?:(?<=^)|(?<=[\s（(；;、，。,]))D(?:[\.．、]|\s)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex 显式下划线正则 = new(@"_+", RegexOptions.Compiled);
        private static readonly Regex 多小问正则 = new(@"(\([0-9]+\)|（[0-9]+）|[①②③④⑤⑥⑦⑧⑨⑩])", RegexOptions.Compiled);

        public 题型特征集 提取(string ooxml内容)
        {
            if (string.IsNullOrWhiteSpace(ooxml内容))
            {
                return new 题型特征集();
            }

            var document = XDocument.Parse(ooxml内容, LoadOptions.PreserveWhitespace);
            var 段落列表 = document.Descendants().Where(元素 => 元素.Name.LocalName == "p").ToList();
            var 归一化段落文本列表 = new List<string>();

            foreach (var 段落 in 段落列表)
            {
                if (是答案样式段落(段落))
                {
                    continue;
                }

                var 文本 = 提取段落文本(段落);
                var 归一化文本 = 归一化文本内容(文本);
                if (!string.IsNullOrWhiteSpace(归一化文本))
                {
                    归一化段落文本列表.Add(归一化文本);
                }
            }

            var 拼接文本 = string.Join("\n", 归一化段落文本列表);
            var 显式下划线数量 = 显式下划线正则.Matches(拼接文本).Cast<Match>().Sum(匹配 => 匹配.Length);
            var 下划线格式空白数量 = 统计下划线格式空白数量(document);
            var 完整ABCD结构组数 = 计算完整ABCD结构组数(拼接文本);
            var 是否有图片 = document.Descendants().Any(元素 => 是图片或绘图元素(元素.Name.LocalName));

            return new 题型特征集
            {
                归一化文本 = 拼接文本,
                归一化后段落数 = 归一化段落文本列表.Count,
                完整ABCD结构组数 = 完整ABCD结构组数,
                显式下划线数量 = 显式下划线数量,
                下划线格式空白数量 = 下划线格式空白数量,
                是否有图片或绘图对象 = 是否有图片,
                是否存在多小问结构 = 多小问正则.Matches(拼接文本).Count >= 2,
            };
        }

        private static string 提取段落文本(XElement 段落)
        {
            var builder = new StringBuilder();
            foreach (var 元素 in 段落.Descendants())
            {
                if (元素.Name.LocalName == "t")
                {
                    builder.Append(元素.Value);
                }
                else if (元素.Name.LocalName == "tab")
                {
                    builder.Append('\t');
                }
                else if (元素.Name.LocalName == "br")
                {
                    builder.Append(' ');
                }
            }

            return builder.ToString();
        }

        private static string 归一化文本内容(string 文本)
        {
            if (string.IsNullOrWhiteSpace(文本))
            {
                return string.Empty;
            }

            var 结果 = 文本
                .Replace('\u3000', ' ')
                .Replace('\u00A0', ' ')
                .Replace('\t', ' ')
                .Replace("\r", string.Empty)
                .Replace("\n", " ");

            结果 = Regex.Replace(结果, @"\s+", " ").Trim();
            return 结果;
        }

        private static bool 是答案样式段落(XElement 段落)
        {
            var 段落属性 = 段落.Elements().FirstOrDefault(元素 => 元素.Name.LocalName == "pPr");
            if (段落属性 == null)
            {
                return false;
            }

            var 段落样式 = 段落属性.Elements().FirstOrDefault(元素 => 元素.Name.LocalName == "pStyle");
            if (段落样式 == null)
            {
                return false;
            }

            var 样式值 = 段落样式.Attributes().FirstOrDefault(属性 => 属性.Name.LocalName == "val")?.Value?.Trim();
            return string.Equals(样式值, "答案", StringComparison.Ordinal);
        }

        private static int 统计下划线格式空白数量(XDocument document)
        {
            var 数量 = 0;
            var run列表 = document.Descendants().Where(元素 => 元素.Name.LocalName == "r");
            foreach (var run in run列表)
            {
                var 有下划线格式 = run.Descendants().Any(元素 => 元素.Name.LocalName == "u");
                if (!有下划线格式)
                {
                    continue;
                }

                var run文本 = string.Concat(run.Descendants().Where(元素 => 元素.Name.LocalName == "t").Select(元素 => 元素.Value));
                if (string.IsNullOrEmpty(run文本))
                {
                    continue;
                }

                var 去除空白后 = run文本.Replace(" ", string.Empty).Replace("　", string.Empty).Replace("\u00A0", string.Empty);
                if (去除空白后.Length == 0)
                {
                    数量++;
                }
            }

            return 数量;
        }

        private static int 计算完整ABCD结构组数(string 文本)
        {
            var a数量 = 选项A正则.Matches(文本).Count;
            var b数量 = 选项B正则.Matches(文本).Count;
            var c数量 = 选项C正则.Matches(文本).Count;
            var d数量 = 选项D正则.Matches(文本).Count;
            return new[] { a数量, b数量, c数量, d数量 }.Min();
        }

        private static bool 是图片或绘图元素(string 元素名称)
        {
            return 元素名称 == "drawing"
                || 元素名称 == "pict"
                || 元素名称 == "object"
                || 元素名称 == "shape"
                || 元素名称 == "imagedata"
                || 元素名称 == "graphic";
        }
    }
}
