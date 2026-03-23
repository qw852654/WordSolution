using System;
using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库应用.题目模块
{
    public class 录入Ooxml题目用例
    {
        private readonly I题目仓储 _题目仓储;
        private readonly I题目文件存储 _题目文件存储;
        private readonly I题目文档转换器 _题目文档转换器;
        private readonly I题目预览生成器 _题目预览生成器;
        private readonly 题目标签规则校验器 _题目标签规则校验器;
        private readonly 题型规则校验器 _题型规则校验器;

        public 录入Ooxml题目用例(
            I题目仓储 题目仓储,
            I题目文件存储 题目文件存储,
            I题目文档转换器 题目文档转换器,
            I题目预览生成器 题目预览生成器,
            题目标签规则校验器 题目标签规则校验器,
            题型规则校验器 题型规则校验器)
        {
            _题目仓储 = 题目仓储;
            _题目文件存储 = 题目文件存储;
            _题目文档转换器 = 题目文档转换器;
            _题目预览生成器 = 题目预览生成器;
            _题目标签规则校验器 = 题目标签规则校验器;
            _题型规则校验器 = 题型规则校验器;
        }

        public 题目 执行(录入Ooxml题目的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            if (string.IsNullOrWhiteSpace(请求.Ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(请求));
            }

            _题目标签规则校验器.校验(请求.标签ID列表);
            _题型规则校验器.校验必填(请求.题型ID);

            var 新题目 = 题目.创建题目(请求.Description, 请求.题型ID, 请求.标签ID列表);
            _题目仓储.增加题目(新题目);

            var 题目文件路径 = _题目文件存储.获取题目文件路径(新题目.Id, ".docx");
            _题目文档转换器.保存Ooxml为题目文件(请求.Ooxml内容, 题目文件路径);

            var 预览文件路径 = _题目文件存储.获取题目预览文件路径(新题目.Id);
            _题目预览生成器.生成HTML预览(题目文件路径, 预览文件路径);

            return 新题目;
        }
    }
}
