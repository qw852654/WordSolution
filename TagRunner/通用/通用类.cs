using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Words.Tables;
using Newtonsoft.Json;

namespace TagRunner
{
    public enum 文档转换结果状态
    {
        成功 = 0,
        源文件不存在 = 1,
        转换失败 = 2
    }

    public class 初始化题库
    {
        /// <summary>
        /// 初始化题库环境（幂等）：
        /// - 若目录/文件已存在则跳过，不覆盖
        /// - 创建 source/html/pdf 三个目录
        /// - 创建 tags.json 与 Questions.json（示例数据，若不存在才写入）
        /// - 为示例题目生成占位 DOCX 与示例 HTML（若不存在才写入）
        /// </summary>
        public static void 初始化题库目录(string rootDir)
        {
            if (string.IsNullOrWhiteSpace(rootDir))
                throw new ArgumentException("题库根目录不存在", nameof(rootDir));

            var sourceDir = Path.Combine(rootDir, "source");
            var htmlDir   = Path.Combine(rootDir, "html");
            var pdfDir    = Path.Combine(rootDir, "pdf");
            Directory.CreateDirectory(rootDir);
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(htmlDir);
            Directory.CreateDirectory(pdfDir);

            // 示例标签与题目数据（仅在文件不存在时写入）
            var tagsPath = Path.Combine(rootDir, "tags.json");
            var questionsPath = Path.Combine(rootDir, "Questions.json");

            // 写入扁平化的标签列表（仅在文件不存在时写入）
            if (!File.Exists(tagsPath))
            {
                File.Create(tagsPath).Dispose();
                标签查询服务.Initialize(rootDir);
                标签维护器.Initialize(标签查询服务.Instance);
                foreach (var tag in 标签类别.允许的根标签类别)
                    标签维护器.Instance.新增标签(tag,tag,parentId:null);

            }

            // 题目 JSON（若不存在则初始化；已存在则保留）
            List<题目> questions;
            if (!File.Exists(questionsPath))
            {
                questions = new List<题目>
                {
                    new 题目 { Id = 1001, TagIds = new List<int>{1,2}, Status = 题目状态.待审核 },
                    new 题目 { Id = 1002, TagIds = new List<int>{1,2}, Status = 题目状态.已标注 }
                };
                File.WriteAllText(questionsPath, JsonConvert.SerializeObject(questions, Formatting.Indented));

                // 为当前题目集合生成占位内容（仅在文件不存在时写入）
                foreach (var q in questions)
                {
                    var docxPath = Path.Combine(sourceDir, $"{q.Id}.docx");
                    var htmlPath = Path.Combine(htmlDir, $"{q.Id}.html");

                    // 占位 DOCX（纯文本占位，便于测试路径存在性）
                    if (!File.Exists(docxPath))
                    {
                        File.WriteAllText(docxPath,
                            $"这是题目 {q.Id} 的占位 DOCX 文件（仅占位，不是有效的 Word 文档）。\n" +
                            $"标签ID：{string.Join(",", q.TagIds ?? new List<int>())}");
                    }

                    // 示例 HTML（可直接在预览中显示）
                    if (!File.Exists(htmlPath))
                    {
                        var tagText = q.TagIds != null && q.TagIds.Count > 0 ? string.Join(", ", q.TagIds) : "(无标签)";
                        var htmlContent =
                            "<!DOCTYPE html><html><head><meta charset='utf-8'/>" +
                            "<style>body{font-family:Segoe UI, Arial;line-height:1.6;padding:16px;} .q{margin:16px 0;padding:12px;border:1px solid #ddd;border-radius:6px;} .head{font-weight:bold;margin-bottom:8px;}</style>" +
                            "</head><body>" +
                            $"<div class='q'><div class='head'>题目ID：{q.Id}</div>" +
                            $"<div>标签ID：{tagText}</div>" +
                            "<div style='margin-top:8px;'>示例题目内容：<br/>" +
                            "若 a=2, 求表达式 a^2 + 3a 的值。</div></div>" +
                            "</body></html>";
                        File.WriteAllText(htmlPath, htmlContent);
                    }
                }
            }
            else
            {
                // 已存在则读取现有题目，避免覆盖
                var json = File.ReadAllText(questionsPath);
                questions = JsonConvert.DeserializeObject<List<题目>>(json) ?? new List<题目>();
            }

            
        }
    }
}
