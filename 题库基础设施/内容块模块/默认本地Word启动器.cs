using System;
using System.Diagnostics;
using System.IO;
using 题库核心.内容块模块.契约;

namespace 题库基础设施.内容块模块
{
    public class 默认本地Word启动器 : I本地Word启动器
    {
        public void 打开文档(string 文档路径)
        {
            if (string.IsNullOrWhiteSpace(文档路径))
            {
                throw new ArgumentException("文档路径不能为空。", nameof(文档路径));
            }

            if (!File.Exists(文档路径))
            {
                throw new FileNotFoundException("要打开的 Word 文档不存在。", 文档路径);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = 文档路径,
                UseShellExecute = true,
            });
        }
    }
}
