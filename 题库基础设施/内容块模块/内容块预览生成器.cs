using System;
using System.IO;
using Aspose.Words;
using Aspose.Words.Saving;
using 题库基础设施.Aspose;
using 题库核心.内容块模块.契约;

namespace 题库基础设施.内容块模块
{
    public class 内容块预览生成器 : I内容块预览生成器
    {
        public void 生成HTML预览(string 内容块文件路径, string HTML文件路径)
        {
            if (string.IsNullOrWhiteSpace(内容块文件路径))
            {
                throw new ArgumentException("内容块文件路径不能为空。", nameof(内容块文件路径));
            }

            if (string.IsNullOrWhiteSpace(HTML文件路径))
            {
                throw new ArgumentException("HTML文件路径不能为空。", nameof(HTML文件路径));
            }

            var 目录路径 = Path.GetDirectoryName(HTML文件路径);
            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            var 文档 = new Document(内容块文件路径);
            文档清理帮助类.清理页眉页脚(文档);
            var 保存选项 = new HtmlSaveOptions
            {
                ExportImagesAsBase64 = true,
                PrettyFormat = true
            };

            文档.Save(HTML文件路径, 保存选项);
        }
    }
}
