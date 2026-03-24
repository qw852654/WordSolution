using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspose.Words;
using Aspose.Words.Drawing;
using 题库应用.题目模块.题型识别;
using 题库核心.题目模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 当前模板试卷解析器
    {
        private readonly 试卷模板识别器 _试卷模板识别器;
        private readonly 题目边界识别器 _题目边界识别器;
        private readonly 答案区识别器 _答案区识别器;
        private readonly 单题内容划分器 _单题内容划分器;
        private readonly 试卷元信息提取器 _试卷元信息提取器;
        private readonly 根据Ooxml识别题型用例 _根据Ooxml识别题型用例;
        private readonly I题型定义仓储 _题型定义仓储;

        public 当前模板试卷解析器(
            试卷模板识别器 试卷模板识别器,
            题目边界识别器 题目边界识别器,
            答案区识别器 答案区识别器,
            单题内容划分器 单题内容划分器,
            试卷元信息提取器 试卷元信息提取器,
            根据Ooxml识别题型用例 根据Ooxml识别题型用例,
            I题型定义仓储 题型定义仓储)
        {
            _试卷模板识别器 = 试卷模板识别器;
            _题目边界识别器 = 题目边界识别器;
            _答案区识别器 = 答案区识别器;
            _单题内容划分器 = 单题内容划分器;
            _试卷元信息提取器 = 试卷元信息提取器;
            _根据Ooxml识别题型用例 = 根据Ooxml识别题型用例;
            _题型定义仓储 = 题型定义仓储;
        }

        public 试卷解析结果 解析(string 试卷文件路径)
        {
            if (string.IsNullOrWhiteSpace(试卷文件路径))
            {
                throw new ArgumentException("试卷文件路径不能为空。", nameof(试卷文件路径));
            }

            var 文档 = new Document(试卷文件路径);
            if (!_试卷模板识别器.是当前模板(文档))
            {
                throw new InvalidOperationException("当前试卷暂不符合已支持的导入模板。");
            }

            var 题型定义列表 = _题型定义仓储.获取全部();
            var 段落列表 = 收集可处理段落(文档);
            var 题目边界列表 = _题目边界识别器.识别(段落列表);
            if (题目边界列表.Count == 0)
            {
                throw new InvalidOperationException("未能在试卷中识别出题目边界。");
            }

            var 草稿题列表 = new List<导入试卷草稿题>();
            foreach (var 边界 in 题目边界列表)
            {
                var 单题段落列表 = 段落列表
                    .Where(段落 => 段落.索引 >= 边界.开始索引 && 段落.索引 <= 边界.结束索引)
                    .ToList();
                var 划分结果 = _单题内容划分器.划分(文档, 单题段落列表);
                var 答案元信息 = _试卷元信息提取器.提取答案元信息(划分结果.难度段落纯文本, 划分结果.知识点段落纯文本);
                var 题型识别结果 = _根据Ooxml识别题型用例.执行(划分结果.题目正文Ooxml内容, 题型定义列表);

                草稿题列表.Add(new 导入试卷草稿题
                {
                    序号 = 草稿题列表.Count + 1,
                    题号文本 = 边界.题号文本,
                    题目摘要 = 划分结果.题目摘要,
                    完整Ooxml内容 = 划分结果.完整Ooxml内容,
                    题目正文Ooxml内容 = 划分结果.题目正文Ooxml内容,
                    原始难度文本 = 答案元信息.原始难度文本,
                    原始知识点列表 = 答案元信息.原始知识点列表.ToList(),
                    推荐题型ID = 题型识别结果.推荐题型ID,
                    推荐题型名称 = 题型识别结果.推荐题型名称,
                    识别说明 = 题型识别结果.说明,
                    置信度 = 题型识别结果.置信度,
                });
            }

            return new 试卷解析结果
            {
                显示名称 = _试卷元信息提取器.提取显示名称(文档, Path.GetFileNameWithoutExtension(试卷文件路径)),
                草稿题列表 = 草稿题列表,
            };
        }

        private IReadOnlyList<试卷段落信息> 收集可处理段落(Document 文档)
        {
            var 结果 = new List<试卷段落信息>();
            var 索引 = 0;
            foreach (Section 节 in 文档.Sections)
            {
                foreach (Paragraph 段落 in 节.Body.Paragraphs)
                {
                    var 文本 = 段落.ToString(SaveFormat.Text).Trim();
                    var 是答案区 = _答案区识别器.是答案区段落(段落);
                    var 包含图片 = 段落.GetChildNodes(NodeType.Shape, true)
                        .Cast<Shape>()
                        .Any(形状 => 形状.HasImage);

                    if (!是答案区 && _题目边界识别器.是大题标题文本(文本))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(文本) && !包含图片 && !是答案区)
                    {
                        continue;
                    }

                    结果.Add(new 试卷段落信息
                    {
                        索引 = 索引++,
                        段落 = 段落,
                        文本 = 文本,
                        是答案区 = 是答案区,
                    });
                }
            }

            return 结果;
        }
    }
}
