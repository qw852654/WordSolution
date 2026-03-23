using System.Collections.Generic;
using System.Linq;
using 题库应用.题目模块.题型识别;
using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 获取下一道待识别题型题目用例
    {
        private readonly I题目仓储 _题目仓储;
        private readonly I题型定义仓储 _题型定义仓储;
        private readonly I题目文件存储 _题目文件存储;
        private readonly I题目文档转换器 _题目文档转换器;
        private readonly 根据Ooxml识别题型用例 _根据Ooxml识别题型用例;

        public 获取下一道待识别题型题目用例(
            I题目仓储 题目仓储,
            I题型定义仓储 题型定义仓储,
            I题目文件存储 题目文件存储,
            I题目文档转换器 题目文档转换器,
            根据Ooxml识别题型用例 根据Ooxml识别题型用例)
        {
            _题目仓储 = 题目仓储;
            _题型定义仓储 = 题型定义仓储;
            _题目文件存储 = 题目文件存储;
            _题目文档转换器 = 题目文档转换器;
            _根据Ooxml识别题型用例 = 根据Ooxml识别题型用例;
        }

        public 获取下一道待识别题型题目结果? 执行()
        {
            var 当前题目 = _题目仓储.获取下一道未设置题型的题目();
            if (当前题目 == null)
            {
                return null;
            }

            var 题型定义列表 = _题型定义仓储.获取全部();
            var 题目文件路径 = _题目文件存储.获取题目文件路径(当前题目.Id, ".docx");
            var ooxml内容 = _题目文档转换器.读取题目文件Ooxml(题目文件路径);
            var 识别结果 = _根据Ooxml识别题型用例.执行(ooxml内容, 题型定义列表);

            return new 获取下一道待识别题型题目结果
            {
                题目ID = 当前题目.Id,
                描述 = 当前题目.Description,
                预览Html = _题目文件存储.读取题目预览HTML(当前题目.Id) ?? string.Empty,
                题型ID = 当前题目.题型ID,
                推荐题型ID = 识别结果.推荐题型ID,
                推荐题型名称 = 识别结果.推荐题型名称,
                识别说明 = 识别结果.说明,
                置信度 = 识别结果.置信度,
                可选题型列表 = 题型定义列表
                    .OrderBy(题型 => 题型.排序值)
                    .ThenBy(题型 => 题型.Id)
                    .Select(题型 => new 题型选项项
                    {
                        Id = 题型.Id,
                        名称 = 题型.名称,
                        描述 = 题型.描述,
                        排序值 = 题型.排序值,
                    })
                    .ToList(),
                剩余未设置题型数量 = _题目仓储
                    .根据条件查找(new List<int>(), 题库核心.筛选模块.领域.组合方式.交集, null, true)
                    .Count,
            };
        }
    }
}
