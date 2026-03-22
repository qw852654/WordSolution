using System;
using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 更新Ooxml题目用例
    {
        private readonly I题目仓储 _题目仓储;
        private readonly I题目文件存储 _题目文件存储;
        private readonly I题目文档转换器 _题目文档转换器;
        private readonly I题目预览生成器 _题目预览生成器;

        public 更新Ooxml题目用例(
            I题目仓储 题目仓储,
            I题目文件存储 题目文件存储,
            I题目文档转换器 题目文档转换器,
            I题目预览生成器 题目预览生成器)
        {
            _题目仓储 = 题目仓储;
            _题目文件存储 = 题目文件存储;
            _题目文档转换器 = 题目文档转换器;
            _题目预览生成器 = 题目预览生成器;
        }

        public bool 执行(int 题目ID, 更新Ooxml题目的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            if (string.IsNullOrWhiteSpace(请求.Ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(请求));
            }

            var 题目 = _题目仓储.GetById(题目ID);
            if (题目 == null)
            {
                return false;
            }

            var 题目文件路径 = _题目文件存储.获取题目文件路径(题目ID, ".docx");
            _题目文档转换器.保存Ooxml为题目文件(请求.Ooxml内容, 题目文件路径);

            var 预览文件路径 = _题目文件存储.获取题目预览文件路径(题目ID);
            _题目预览生成器.生成HTML预览(题目文件路径, 预览文件路径);

            return true;
        }
    }
}
