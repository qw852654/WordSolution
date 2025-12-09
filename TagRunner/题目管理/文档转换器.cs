using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Words;
using Aspose.Words.Saving;

namespace TagRunner
{
    //<summary>
    //由项目根目录初始化，主要功能：转换文档格式
    //</summary>
    public class 文档转换器
    {
        public string RootDirectory { get; }
        public string SourceDir => Path.Combine(RootDirectory, "source");
        public string HtmlDir => Path.Combine(RootDirectory, "html");
        public string PdfDir => Path.Combine(RootDirectory, "pdf");

        public 文档转换器(string rootDirectory)
        {
            RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            Directory.CreateDirectory(SourceDir);
            Directory.CreateDirectory(HtmlDir);
            Directory.CreateDirectory(PdfDir);
        }

        
        public 文档转换结果状态 ConvertToHtml(int QuestionId)
        {
            string sourcePath = Path.Combine(SourceDir, $"{QuestionId}.docx");
            string htmlPath = Path.Combine(HtmlDir, $"{QuestionId}.html");

            if (!File.Exists(sourcePath))
                return 文档转换结果状态.源文件不存在;

            try
            {
                var doc = new Document(sourcePath);
                var saveOptions = new HtmlSaveOptions
                {
                    ExportImagesAsBase64 = true,
                    PrettyFormat = true
                };
                doc.Save(htmlPath, saveOptions);
                return 文档转换结果状态.成功;
            }
            catch
            {
                return 文档转换结果状态.转换失败;
            }
        }
    }
}
