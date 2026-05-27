using System;
using System.IO;
using System.Security.Cryptography;
using 题库核心.内容块模块.契约;

namespace 题库基础设施.内容块模块
{
    public class SHA256文件哈希服务 : I文件哈希服务
    {
        public string 计算SHA256(string 文件路径)
        {
            if (string.IsNullOrWhiteSpace(文件路径))
            {
                throw new ArgumentException("文件路径不能为空。", nameof(文件路径));
            }

            using var 文件流 = File.Open(文件路径, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sha256 = SHA256.Create();
            return Convert.ToHexString(sha256.ComputeHash(文件流));
        }
    }
}
