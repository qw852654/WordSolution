using System;
using System.IO;
using Microsoft.Office.Interop.Word;
using Word本地文件操作核心库.工具;

namespace Word本地文件操作核心库.用例
{
    public class 导出源目录pdf参数
    {
        public Document 文档 { get; set; }
        public string 文件名不带扩展名 { get; set; }
    }

    public class 导出源目录pdf结果
    {
        public string Pdf路径 { get; set; }
    }

    public class 导出源目录pdf
    {
        public 导出源目录pdf结果 执行(导出源目录pdf参数 参数)
        {
            if (参数 == null) throw new ArgumentNullException(nameof(参数));
            if (参数.文档 == null) throw new ArgumentNullException(nameof(参数.文档));

            string 输出目录 = 文档路径工具.获取文档所在目录(参数.文档);

            string 文件名不带扩展名 = 参数.文件名不带扩展名;
            if (string.IsNullOrWhiteSpace(文件名不带扩展名))
            {
                文件名不带扩展名 = Path.GetFileNameWithoutExtension(参数.文档.Name);
            }

            string pdf路径 = 文档路径工具.生成输出文件路径(输出目录, 文件名不带扩展名, ".pdf");
            Pdf导出工具.导出(参数.文档, pdf路径);

            return new 导出源目录pdf结果
            {
                Pdf路径 = pdf路径
            };
        }
    }
}
