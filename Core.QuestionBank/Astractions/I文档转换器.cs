using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagRunner;

namespace TagRunner.基础
{
    /// <summary>
    /// 文档转换器接口（基础层）：负责将 DOCX 等源文件转换为 HTML/PDF 等格式。
    /// 业务层通过此接口调用转换器完成文件格式转换，返回转换状态。
    /// </summary>
    public interface I文档转换器
    {
        /// <summary>
        /// 将指定的源 DOCX 文件转换为 HTML，输出到目标路径。
        /// </summary>
        bool ConvertToHtml(string 源Docx路径, string 目标Html路径);

        /// <summary>
        /// 将指定的源 DOCX 文件转换为 PDF，输出到目标路径。
        /// 返回文档转换结果状态。
        /// </summary>
        bool ConvertToPdf(string 源Docx路径, string 目标Pdf路径);
    }
}
