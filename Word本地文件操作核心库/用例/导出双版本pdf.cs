using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Office.Interop.Word;
using Word本地文件操作核心库.工具;

namespace Word本地文件操作核心库.用例
{
    public class 导出双版本pdf参数
    {
        public Document 文档 { get; set; }
        public string 输出目录 { get; set; }
        public string 文件名不带扩展名 { get; set; }
        public List<string> 待删除样式 { get; set; } = new List<string>();
    }

    public class 导出双版本pdf结果
    {
        public string 原始版Pdf路径 { get; set; }
        public string 无答案版Pdf路径 { get; set; }
    }

    public class 导出双版本pdf
    {
        public 导出双版本pdf结果 执行(导出双版本pdf参数 参数)
        {
            if (参数 == null) throw new ArgumentNullException(nameof(参数));
            if (参数.文档 == null) throw new ArgumentNullException(nameof(参数.文档));

            string 输出目录 = 参数.输出目录;
            if (string.IsNullOrWhiteSpace(输出目录))
            {
                输出目录 = 文档路径工具.获取文档所在目录(参数.文档);
            }
            Directory.CreateDirectory(输出目录);

            string 文件名不带扩展名 = 参数.文件名不带扩展名;
            if (string.IsNullOrWhiteSpace(文件名不带扩展名))
            {
                文件名不带扩展名 = Path.GetFileNameWithoutExtension(参数.文档.Name);
            }

            string 原始版Pdf路径 = 文档路径工具.生成输出文件路径(输出目录, 文件名不带扩展名, ".pdf");
            string 无答案版Pdf路径 = 文档路径工具.生成输出文件路径(输出目录, 文件名不带扩展名 + "_无答案", ".pdf");

            Pdf导出工具.导出(参数.文档, 原始版Pdf路径);

            string 源文档本地路径 = 文档路径工具.获取文档完整本地路径(参数.文档);
            string 临时文档路径 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".docx");
            File.Copy(源文档本地路径, 临时文档路径, true);

            Document 临时文档 = null;
            try
            {
                临时文档 = 参数.文档.Application.Documents.Open(临时文档路径, ReadOnly: false, Visible: false);
                答案处理工具.删除指定样式段落(临时文档, 参数.待删除样式);
                Pdf导出工具.导出(临时文档, 无答案版Pdf路径);
            }
            finally
            {
                if (临时文档 != null)
                {
                    临时文档.Close(false);
                }

                if (File.Exists(临时文档路径))
                {
                    File.Delete(临时文档路径);
                }
            }

            return new 导出双版本pdf结果
            {
                原始版Pdf路径 = 原始版Pdf路径,
                无答案版Pdf路径 = 无答案版Pdf路径
            };
        }
    }
}

