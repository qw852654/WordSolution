using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Words;
using Aspose.Words.Saving;
using QuestionBank.Core.Models;

namespace QuestionBank.Core.Services
{
    public class WordConverter
    {
        public string RootDirectory { get; }
        public string SourceDir => Path.Combine(RootDirectory, "source");
        public string HtmlDir => Path.Combine(RootDirectory, "html");
        public string PdfDir => Path.Combine(RootDirectory, "pdf");

        public WordConverter(string rootDirectory)
        {
            RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            Directory.CreateDirectory(SourceDir);
            Directory.CreateDirectory(HtmlDir);
            Directory.CreateDirectory(PdfDir);
        }

        public void ConvertToHtml(string sourcePath, string htmlPath, bool exportImagesAsBase64 = true)
        {
            var doc = new Document(sourcePath);
            var options = new HtmlSaveOptions(SaveFormat.Html)
            {
                ExportImagesAsBase64 = exportImagesAsBase64,
                PrettyFormat = true
            };
            doc.Save(htmlPath, options);
        }

        public void ConvertToPdf(string sourcePath, string pdfPath)
        {
            var doc = new Document(sourcePath);
            doc.Save(pdfPath, SaveFormat.Pdf);
        }

        public void BatchConvert(IEnumerable<Question> questions, Action<string, string> onProgress = null, Action<string, Exception> onError = null, bool toHtml = true, bool toPdf = false)
        {
            foreach (var q in questions)
            {
                try
                {
                    var src = Path.IsPathRooted(q.SourcePath) ? q.SourcePath : Path.Combine(RootDirectory, q.SourcePath);
                    if (!File.Exists(src))
                        throw new FileNotFoundException("源文件不存在", src);

                    if (toHtml)
                    {
                        var htmlPath = Path.IsPathRooted(q.HtmlPath) ? q.HtmlPath : Path.Combine(RootDirectory, q.HtmlPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(htmlPath));
                        ConvertToHtml(src, htmlPath, exportImagesAsBase64: true);
                        onProgress?.Invoke(q.Id, "HTML");
                    }

                    if (toPdf)
                    {
                        var pdfPath = Path.IsPathRooted(q.PdfPath) ? q.PdfPath : Path.Combine(RootDirectory, q.PdfPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(pdfPath));
                        ConvertToPdf(src, pdfPath);
                        onProgress?.Invoke(q.Id, "PDF");
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(q.Id, ex);
                }
            }
        }
    }
}