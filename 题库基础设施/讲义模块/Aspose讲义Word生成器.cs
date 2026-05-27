using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Words;
using 题库基础设施.Aspose;
using 题库核心.讲义模块.契约;

namespace 题库基础设施.讲义模块
{
    public class Aspose讲义Word生成器 : I讲义Word生成器
    {
        public void 生成(string 标题, IReadOnlyList<讲义生成源文件> 源文件列表, string 输出文件路径)
        {
            if (string.IsNullOrWhiteSpace(输出文件路径))
            {
                throw new ArgumentException("输出文件路径不能为空。", nameof(输出文件路径));
            }

            var 目录 = Path.GetDirectoryName(输出文件路径);
            if (!string.IsNullOrWhiteSpace(目录))
            {
                Directory.CreateDirectory(目录);
            }

            var 文档 = new Document();
            var builder = new DocumentBuilder(文档);
            builder.Font.Name = "Microsoft YaHei";
            builder.Font.Size = 16;
            builder.Font.Bold = true;
            builder.Writeln(标题);
            builder.Font.Bold = false;
            builder.Font.Size = 10;
            builder.Font.Color = System.Drawing.Color.Gray;
            builder.Writeln($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm}");
            builder.Font.Color = System.Drawing.Color.Black;

            if (源文件列表.Count == 0)
            {
                builder.Font.Size = 11;
                builder.Writeln("暂无内容。");
                文档.Save(输出文件路径);
                return;
            }

            foreach (var 源文件 in 源文件列表)
            {
                if (string.IsNullOrWhiteSpace(源文件.文件路径) || !File.Exists(源文件.文件路径))
                {
                    throw new FileNotFoundException("讲义源内容块文件不存在。", 源文件.文件路径);
                }

                builder.MoveToDocumentEnd();
                builder.InsertBreak(BreakType.ParagraphBreak);
                builder.Font.Name = "Microsoft YaHei";
                builder.Font.Size = 11;
                builder.Font.Bold = true;
                builder.Writeln($"{源文件.角色}：{源文件.标题}");
                builder.Font.Bold = false;

                var 源文档 = new Document(源文件.文件路径);
                文档清理帮助类.清理页眉页脚(源文档);
                文档.AppendDocument(源文档, ImportFormatMode.KeepSourceFormatting);
            }

            文档.Save(输出文件路径);
        }
    }
}
