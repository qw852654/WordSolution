using System;
using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 获取题目文件Base64用例
    {
        private readonly I题目文件存储 _题目文件存储;

        public 获取题目文件Base64用例(I题目文件存储 题目文件存储)
        {
            _题目文件存储 = 题目文件存储;
        }

        public string? 执行(int 题目ID)
        {
            var 文件内容 = _题目文件存储.读取题目文件(题目ID);
            if (文件内容 == null || 文件内容.Length == 0)
            {
                return null;
            }

            return Convert.ToBase64String(文件内容);
        }
    }
}
