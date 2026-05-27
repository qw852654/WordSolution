using System;
using System.IO;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库基础设施.内容块模块
{
    public class 编辑文件稳定性检测器 : I编辑文件稳定性检测器
    {
        public 编辑文件检测结果 检测(string 编辑文件路径)
        {
            var 结果 = new 编辑文件检测结果();
            if (string.IsNullOrWhiteSpace(编辑文件路径) || !File.Exists(编辑文件路径))
            {
                return 结果;
            }

            var 文件信息 = new FileInfo(编辑文件路径);
            结果.文件存在 = true;
            结果.文件长度 = 文件信息.Length;
            结果.最后写入时间Utc = 文件信息.LastWriteTimeUtc;
            结果.锁文件存在 = File.Exists(获取Word锁文件路径(编辑文件路径));
            结果.可独占打开 = 可独占打开(编辑文件路径);
            return 结果;
        }

        private static string 获取Word锁文件路径(string 编辑文件路径)
        {
            var 目录路径 = Path.GetDirectoryName(编辑文件路径) ?? string.Empty;
            var 文件名 = Path.GetFileName(编辑文件路径);
            return Path.Combine(目录路径, "~$" + 文件名);
        }

        private static bool 可独占打开(string 文件路径)
        {
            try
            {
                using var _ = new FileStream(文件路径, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
