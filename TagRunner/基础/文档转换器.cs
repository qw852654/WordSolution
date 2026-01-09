using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.Words.Saving;

namespace TagRunner.基础
{
    /// <summary>
    /// 基于 Aspose.Words 的文档转换器实现，按文件路径执行转换。
    /// 实现 I文档转换器 接口，返回 bool 表示是否成功。
    /// </summary>
    internal class 文档转换器 : I文档转换器
    {
        public 文档转换器()
        {
        }

        public bool ConvertToHtml(string 源Docx路径, string 目标Html路径)
        {
            if (string.IsNullOrWhiteSpace(源Docx路径) || string.IsNullOrWhiteSpace(目标Html路径))
                throw new ArgumentException("源或目标路径不能为空");

            if (!File.Exists(源Docx路径))
                return false;

            try
            {
                var doc = new Document(源Docx路径);
                var saveOptions = new HtmlSaveOptions
                {
                    ExportImagesAsBase64 = true,
                    PrettyFormat = true
                };

                var dir = Path.GetDirectoryName(目标Html路径);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                doc.Save(目标Html路径, saveOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ConvertToPdf(string 源Docx路径, string 目标Pdf路径)
        {
            if (string.IsNullOrWhiteSpace(源Docx路径) || string.IsNullOrWhiteSpace(目标Pdf路径))
                throw new ArgumentException("源或目标路径不能为空");

            if (!File.Exists(源Docx路径))
                return false;

            try
            {
                var doc = new Document(源Docx路径);
                var saveOptions = SaveOptions.CreateSaveOptions(SaveFormat.Pdf);

                var dir = Path.GetDirectoryName(目标Pdf路径);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                doc.Save(目标Pdf路径, saveOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
