using System;
using System.Collections.Generic;
using System.IO;
using TagRunner.Models;
using TagRunner.业务;

namespace TagRunner.测试
{
    /// <summary>
    /// 测试用示例数据生成器：在指定题库根目录下初始化数据库并插入若干标签与题目，
    /// 同时生成占位 docx 文件并为每题写入简单的 HTML 文件以便 UI 演示。
    /// 说明：此类仅用于本地测试与演示，不作为生产代码。
    /// </summary>
    public static class DemoDataInitializer
    {
        /// <summary>
        /// 在指定根目录初始化题库并插入示例数据。
        /// 返回已创建的题目 Id 列表。
        /// </summary>
        public static List<int> 生成示例题库(string 题库根目录, int 标签数量 = 3, int 题目数量 = 5)
        {
            if (string.IsNullOrWhiteSpace(题库根目录)) throw new ArgumentException("题库根目录不能为空", nameof(题库根目录));

            // 1) 初始化配置与目录（会创建 source/html/index）
            var cfg = new 题库配置(题库根目录);
            cfg.初始化目录(创建如果不存在: true);

            // 2) 初始化组合根并覆盖数据库以便每次 demo 从干净状态开始
            Bootstrapper.Initialize(cfg, 覆盖数据库: true);

            var createdQuestionIds = new List<int>();

            // 3) 创建一些示例标签（父 Id 使用 0 表示系统根）
            var tagIds = new List<int>();
            for (int i = 1; i <= 标签数量; i++)
            {
                var tag = new 标签
                {
                    Name = $"示例标签 {i}",
                    ParentId = 0,
                    Description = $"自动生成示例标签 {i}"
                };

                int newTagId = Bootstrapper.标签仓储.新增标签(tag);
                tagIds.Add(newTagId);
            }

            // 4) 为每个题目生成元数据、复制占位 docx、并写入 html 文件（模拟转换成功）
            for (int q = 1; q <= 题目数量; q++)
            {
                // 根据题号分配标签（循环分配）
                var assignedTag = new List<int> { tagIds[(q - 1) % tagIds.Count] };

                var model = new 题目
                {
                    Description = $"示例题目 {q}",
                    TagIDs = assignedTag
                };

                // 插入题目元数据并获取 Id
                int newId = Bootstrapper.题目仓储.新增题目(model);

                // 生成一个占位 docx 文件（实际上只是文本文件，但用于演示复制）
                var tmpDocx = Path.Combine(Path.GetTempPath(), $"sample_demo_{newId}.docx");
                File.WriteAllText(tmpDocx, $"这是题目 {newId} 的占位 DOCX 内容（仅文本）。");

                // 把占位文件复制到题库（source/{id}.docx）
                var dest = Bootstrapper.文件存储.复制到题库(tmpDocx, newId);

                // 为了让 UI 演示不依赖实际的 DOCX 转换器，我们在 html 目录下写入一个示例 HTML 文件
                var htmlPath = Bootstrapper.文件存储.获取Html路径(newId);
                var htmlContent = $@"<html>
<head><meta charset='utf-8'/><title>题目 {newId}</title></head>
<body>
<h1>题目 {newId}</h1>
<p>描述：{System.Security.SecurityElement.Escape(model.Description ?? "(无)")}</p>
<p>标签：{string.Join(",", assignedTag)}</p>
<p>示例内容：这是为演示而生成的 HTML 内容。</p>
</body>
</html>";

                try
                {
                    var htmlDir = Path.GetDirectoryName(htmlPath);
                    if (!Directory.Exists(htmlDir)) Directory.CreateDirectory(htmlDir);
                    File.WriteAllText(htmlPath, htmlContent);
                }
                catch
                {
                    // 忽略写入失败（演示环境），但保留题目 Id
                }

                createdQuestionIds.Add(newId);
            }

            return createdQuestionIds;
        }
    }
}
