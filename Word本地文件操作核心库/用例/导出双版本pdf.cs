using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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

            try
            {
                导出无答案版Pdf(参数, 无答案版Pdf路径);
            }
            catch
            {
                无答案版Pdf路径 = null;
            }

            return new 导出双版本pdf结果
            {
                原始版Pdf路径 = 原始版Pdf路径,
                无答案版Pdf路径 = 无答案版Pdf路径
            };
        }

        private static void 导出无答案版Pdf(导出双版本pdf参数 参数, string 无答案版Pdf路径)
        {
            string 源文档本地路径 = 文档路径工具.获取文档完整本地路径(参数.文档);
            string 临时文档路径 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".docx");
            File.Copy(源文档本地路径, 临时文档路径, true);

            Application 后台Word应用 = null;
            Document 临时文档 = null;

            try
            {
                后台Word应用 = new Application
                {
                    Visible = false,
                    ScreenUpdating = false,
                    DisplayAlerts = WdAlertLevel.wdAlertsNone
                };

                临时文档 = 后台Word应用.Documents.Open(
                    FileName: 临时文档路径,
                    ConfirmConversions: false,
                    ReadOnly: false,
                    AddToRecentFiles: false,
                    Visible: false,
                    OpenAndRepair: true,
                    NoEncodingDialog: true);

                答案处理工具.删除指定样式段落(临时文档, 参数.待删除样式);
                Pdf导出工具.导出(临时文档, 无答案版Pdf路径);
            }
            finally
            {
                if (临时文档 != null)
                {
                    try
                    {
                        临时文档.Close(WdSaveOptions.wdDoNotSaveChanges);
                    }
                    catch
                    {
                    }
                }

                if (后台Word应用 != null)
                {
                    try
                    {
                        后台Word应用.Quit(WdSaveOptions.wdDoNotSaveChanges);
                    }
                    catch
                    {
                    }
                }

                释放Com对象(临时文档);
                释放Com对象(后台Word应用);
                尝试删除临时文件(临时文档路径);
            }
        }

        private static void 释放Com对象(object com对象)
        {
            if (com对象 != null && Marshal.IsComObject(com对象))
            {
                Marshal.FinalReleaseComObject(com对象);
            }
        }

        private static void 尝试删除临时文件(string 临时文档路径)
        {
            if (string.IsNullOrWhiteSpace(临时文档路径))
            {
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (File.Exists(临时文档路径))
                    {
                        File.Delete(临时文档路径);
                    }

                    return;
                }
                catch
                {
                    Thread.Sleep(200);
                }
            }
        }
    }
}
