using System;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 更新Ooxml内容块用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly I内容块文件存储 _内容块文件存储;
        private readonly I内容块文档转换器 _内容块文档转换器;
        private readonly I内容块预览生成器 _内容块预览生成器;
        private readonly 内容块元数据选项帮助类 _内容块元数据选项帮助类;

        public 更新Ooxml内容块用例(
            I内容块仓储 内容块仓储,
            I内容块文件存储 内容块文件存储,
            I内容块文档转换器 内容块文档转换器,
            I内容块预览生成器 内容块预览生成器,
            内容块元数据选项帮助类 内容块元数据选项帮助类)
        {
            _内容块仓储 = 内容块仓储;
            _内容块文件存储 = 内容块文件存储;
            _内容块文档转换器 = 内容块文档转换器;
            _内容块预览生成器 = 内容块预览生成器;
            _内容块元数据选项帮助类 = 内容块元数据选项帮助类;
        }

        public 内容块详情结果? 执行(int 内容块ID, 更新Ooxml内容块的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            if (string.IsNullOrWhiteSpace(请求.Ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(请求));
            }

            var 内容块 = _内容块仓储.GetById(内容块ID);
            if (内容块 == null)
            {
                return null;
            }

            if (请求.标题 != null
                || 请求.内容块类型.HasValue
                || 请求.内容块状态.HasValue
                || 请求.内容块结构类型.HasValue
                || 请求.是否允许子块.HasValue
                || 请求.RoleOptionId.HasValue
                || 请求.DifficultyOptionId.HasValue
                || 请求.UsageOptionId.HasValue
                || 请求.QuestionTypeOptionId.HasValue
                || 请求.DefaultIncluded.HasValue
                || 请求.Note != null)
            {
                _内容块元数据选项帮助类.校验内容块选项(
                    请求.RoleOptionId,
                    请求.DifficultyOptionId,
                    请求.UsageOptionId,
                    请求.QuestionTypeOptionId);

                内容块.修改元数据(
                    请求.标题 ?? 内容块.标题,
                    请求.摘要 ?? 内容块.摘要,
                    请求.内容块类型 ?? 内容块.类型,
                    请求.内容块状态 ?? 内容块.状态,
                    请求.内容块结构类型,
                    请求.是否允许子块,
                    请求.RoleOptionId ?? 内容块.RoleOptionId,
                    请求.DifficultyOptionId ?? 内容块.DifficultyOptionId,
                    请求.UsageOptionId ?? 内容块.UsageOptionId,
                    请求.QuestionTypeOptionId ?? 内容块.QuestionTypeOptionId,
                    请求.DefaultIncluded,
                    请求.Note ?? 内容块.Note);
            }

            var 版本号 = _内容块仓储.获取下一个版本号(内容块ID);
            var 内容块文件路径 = _内容块文件存储.获取内容块文件路径(内容块ID, 版本号, ".docx");
            _内容块文档转换器.保存Ooxml为内容块文件(请求.Ooxml内容, 内容块文件路径);

            var 预览文件路径 = _内容块文件存储.获取内容块预览文件路径(内容块ID, 版本号);
            _内容块预览生成器.生成HTML预览(内容块文件路径, 预览文件路径);

            var 纯文本内容 = _内容块文档转换器.提取纯文本(内容块文件路径);
            var 新版本 = 内容块版本.创建(内容块ID, 版本号, 内容块文件路径, 预览文件路径, 纯文本内容, true);

            _内容块仓储.增加版本并设为当前(内容块, 新版本);

            return 内容块详情结果.从内容块(内容块, 新版本, _内容块元数据选项帮助类.获取内容块选项字典(内容块));
        }
    }
}
