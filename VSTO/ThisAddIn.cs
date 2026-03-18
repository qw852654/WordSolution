using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using Word本地文件操作核心库.工具;
using Word本地文件操作核心库.用例;
using Word本地文件操作核心库.自动处理;

namespace VSTO
{
    public static class 获取用户下载目录
    {
        private static readonly Guid DownloadsFolderGuid = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr ppszPath);

        public static string GetDownloadsPath()
        {
            IntPtr outPath;
            int result = SHGetKnownFolderPath(DownloadsFolderGuid, 0, IntPtr.Zero, out outPath);
            if (result == 0)
            {
                string path = Marshal.PtrToStringUni(outPath);
                Marshal.FreeCoTaskMem(outPath);
                return path;
            }
            else
            {
                throw new ExternalException("无法获取下载文件夹路径，错误码: " + result);
            }
        }
    }

    public partial class ThisAddIn
    {
        private FileSystemWatcher _下载文档监控器;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            启动下载目录监控();

            this.Application.DocumentOpen += 隐藏页间空白;
            this.Application.DocumentOpen += 开启导航窗格;
            ((Word.ApplicationEvents4_Event)this.Application).NewDocument += 隐藏页间空白;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            关闭下载目录监控();

            this.Application.DocumentOpen -= 隐藏页间空白;
            this.Application.DocumentOpen -= 开启导航窗格;
            ((Word.ApplicationEvents4_Event)this.Application).NewDocument -= 隐藏页间空白;
        }
         
        private void 启动下载目录监控()
        {
            string 下载目录 = 获取用户下载目录.GetDownloadsPath();
            if (!Directory.Exists(下载目录)) return;

            _下载文档监控器 = new FileSystemWatcher();
            _下载文档监控器.Path = 下载目录;
            _下载文档监控器.Filter = "*.docx";
            _下载文档监控器.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _下载文档监控器.Renamed += 下载文档被重命名;
            _下载文档监控器.EnableRaisingEvents = true;
        }

        private void 关闭下载目录监控()
        {
            if (_下载文档监控器 == null) return;

            _下载文档监控器.EnableRaisingEvents = false;
            _下载文档监控器.Renamed -= 下载文档被重命名;
            _下载文档监控器.Dispose();
            _下载文档监控器 = null;
        }

        private void 下载文档被重命名(object sender, RenamedEventArgs e)
        {
            try
            {
                string 文件名 = e.Name ?? string.Empty;
                if (!匹配组卷文档.是否匹配(文件名)) return;

                var 用例 = new 将新下载的组卷试卷插入文档();
                用例.执行(this.Application, e.FullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("处理下载文档失败：" + ex.Message);
            }
        }

        private void 隐藏页间空白(Word.Document doc)
        {
            try
            {
                if (doc.ActiveWindow != null)
                {
                    doc.ActiveWindow.View.DisplayPageBoundaries = false;
                }
            }
            catch
            {
            }
        }

        private void 开启导航窗格(Word.Document doc)
        {
            try
            {
                var navigationPane = doc.Application.CommandBars["Navigation"];
                if (navigationPane != null)
                {
                    navigationPane.Visible = true;
                }
            }
            catch
            {
            }
        }

        #region VSTO 生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
