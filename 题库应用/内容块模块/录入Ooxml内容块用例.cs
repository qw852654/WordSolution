using System;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 录入Ooxml内容块用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly I内容块文件存储 _内容块文件存储;
        private readonly I内容块文档转换器 _内容块文档转换器;
        private readonly I内容块预览生成器 _内容块预览生成器;

        public 录入Ooxml内容块用例(
            I内容块仓储 内容块仓储,
            I内容块文件存储 内容块文件存储,
            I内容块文档转换器 内容块文档转换器,
            I内容块预览生成器 内容块预览生成器)
        {
            _内容块仓储 = 内容块仓储;
            _内容块文件存储 = 内容块文件存储;
            _内容块文档转换器 = 内容块文档转换器;
            _内容块预览生成器 = 内容块预览生成器;
        }

        public 内容块详情结果 执行(录入Ooxml内容块的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            if (string.IsNullOrWhiteSpace(请求.Ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(请求));
            }

            var 新内容块 = 内容块.创建(
                请求.标题,
                请求.摘要,
                请求.内容块类型 ?? 内容块类型.知识点,
                请求.内容块状态 ?? 内容块状态.草稿,
                请求.内容块结构类型,
                请求.是否允许子块);
            _内容块仓储.增加内容块(新内容块);

            var 版本 = 创建内容块版本(新内容块.Id, 请求.Ooxml内容);
            _内容块仓储.增加版本并设为当前(新内容块, 版本);

            return 内容块详情结果.从内容块(新内容块, 版本);
        }

        private 内容块版本 创建内容块版本(int 内容块ID, string Ooxml内容)
        {
            const int 版本号 = 1;
            var 内容块文件路径 = _内容块文件存储.获取内容块文件路径(内容块ID, 版本号, ".docx");
            _内容块文档转换器.保存Ooxml为内容块文件(Ooxml内容, 内容块文件路径);

            var 预览文件路径 = _内容块文件存储.获取内容块预览文件路径(内容块ID, 版本号);
            _内容块预览生成器.生成HTML预览(内容块文件路径, 预览文件路径);

            var 纯文本内容 = _内容块文档转换器.提取纯文本(内容块文件路径);
            return 内容块版本.创建(内容块ID, 版本号, 内容块文件路径, 预览文件路径, 纯文本内容, true);
        }
    }
}
