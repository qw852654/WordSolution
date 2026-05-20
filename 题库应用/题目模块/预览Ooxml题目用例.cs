using System;
using System.IO;
using 题库应用.题目模块.题型识别;
using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 预览Ooxml题目用例
    {
        private readonly I题目文档转换器 _题目文档转换器;
        private readonly I题目预览生成器 _题目预览生成器;
        private readonly I题型定义仓储 _题型定义仓储;
        private readonly 根据Ooxml识别题型用例 _根据Ooxml识别题型用例;

        public 预览Ooxml题目用例(
            I题目文档转换器 题目文档转换器,
            I题目预览生成器 题目预览生成器,
            I题型定义仓储 题型定义仓储,
            根据Ooxml识别题型用例 根据Ooxml识别题型用例)
        {
            _题目文档转换器 = 题目文档转换器;
            _题目预览生成器 = 题目预览生成器;
            _题型定义仓储 = 题型定义仓储;
            _根据Ooxml识别题型用例 = 根据Ooxml识别题型用例;
        }

        public 预览Ooxml题目的结果 执行(预览Ooxml题目的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            if (string.IsNullOrWhiteSpace(请求.Ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(请求));
            }

            var 临时目录 = Path.Combine(Path.GetTempPath(), "WordSolution", "record-preview", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(临时目录);

            var 文档路径 = Path.Combine(临时目录, "preview.docx");
            var 预览路径 = Path.Combine(临时目录, "preview.html");

            try
            {
                _题目文档转换器.保存Ooxml为题目文件(请求.Ooxml内容, 文档路径);
                _题目预览生成器.生成HTML预览(文档路径, 预览路径);

                var 题型识别结果 = _根据Ooxml识别题型用例.执行(请求.Ooxml内容, _题型定义仓储.获取全部());

                return new 预览Ooxml题目的结果
                {
                    预览Html = File.ReadAllText(预览路径),
                    推荐题型ID = 题型识别结果.推荐题型ID,
                    推荐题型名称 = 题型识别结果.推荐题型名称,
                    识别说明 = 题型识别结果.说明,
                    置信度 = 题型识别结果.置信度,
                };
            }
            finally
            {
                try
                {
                    if (Directory.Exists(临时目录))
                    {
                        Directory.Delete(临时目录, true);
                    }
                }
                catch
                {
                    // 预览临时文件清理失败不影响主流程。
                }
            }
        }
    }
}
