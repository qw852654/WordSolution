using System;
using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 获取内容块文件Base64用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly I内容块文件存储 _内容块文件存储;

        public 获取内容块文件Base64用例(
            I内容块仓储 内容块仓储,
            I内容块文件存储 内容块文件存储)
        {
            _内容块仓储 = 内容块仓储;
            _内容块文件存储 = 内容块文件存储;
        }

        public string? 执行(int 内容块ID)
        {
            var 当前版本 = _内容块仓储.获取当前版本(内容块ID);
            if (当前版本 == null)
            {
                return null;
            }

            var 文件内容 = _内容块文件存储.读取内容块文件(当前版本.Docx路径);
            return 文件内容 == null ? null : Convert.ToBase64String(文件内容);
        }
    }
}
