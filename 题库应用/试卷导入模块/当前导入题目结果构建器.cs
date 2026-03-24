using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.题目模块.契约;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库应用.试卷导入模块
{
    public class 当前导入题目结果构建器
    {
        private readonly I题目文档转换器 _题目文档转换器;
        private readonly I题目预览生成器 _题目预览生成器;
        private readonly I题型定义仓储 _题型定义仓储;
        private readonly I知识点映射仓储 _知识点映射仓储;
        private readonly I标签仓储 _标签仓储;

        public 当前导入题目结果构建器(
            I题目文档转换器 题目文档转换器,
            I题目预览生成器 题目预览生成器,
            I题型定义仓储 题型定义仓储,
            I知识点映射仓储 知识点映射仓储,
            I标签仓储 标签仓储)
        {
            _题目文档转换器 = 题目文档转换器;
            _题目预览生成器 = 题目预览生成器;
            _题型定义仓储 = 题型定义仓储;
            _知识点映射仓储 = 知识点映射仓储;
            _标签仓储 = 标签仓储;
        }

        public 当前导入题目结果 构建(试卷记录 试卷记录, 试卷题目项 试卷题目项)
        {
            var 知识点列表 = 构建知识点列表(试卷题目项);
            var 预填标签ID列表 = new List<int> { 试卷记录.年份标签ID, 试卷记录.来源标签ID };
            预填标签ID列表.AddRange(
                知识点列表
                    .Where(项 => 项.是否已解决 && !项.是否抛弃 && 项.目标标签ID.HasValue)
                    .Select(项 => 项.目标标签ID!.Value));

            return new 当前导入题目结果
            {
                试卷记录ID = 试卷记录.Id,
                试卷题目项ID = 试卷题目项.Id,
                草稿题序号 = 试卷题目项.顺序号,
                题号文本 = 试卷题目项.题号文本,
                题目摘要 = 试卷题目项.题目摘要,
                题目预览Html = 获取或生成题目预览Html(试卷记录.Id, 试卷题目项),
                推荐题型ID = 试卷题目项.推荐题型ID,
                推荐题型名称 = 试卷题目项.推荐题型名称,
                识别说明 = 试卷题目项.识别说明,
                置信度 = 试卷题目项.置信度,
                可选题型列表 = _题型定义仓储.获取全部(),
                原始难度文本 = 试卷题目项.原始难度文本,
                知识点列表 = 知识点列表,
                预填标签ID列表 = 预填标签ID列表.Distinct().ToList(),
                剩余数量 = Math.Max(试卷记录.总题数 - 试卷记录.已确认数 - 试卷记录.已跳过数, 0),
            };
        }

        private IReadOnlyList<知识点映射展示项> 构建知识点列表(试卷题目项 试卷题目项)
        {
            return 试卷题目项.获取原始知识点列表()
                .Select(原始知识点文本 =>
                {
                    var 归一化文本 = 知识点文本规范化器.规范化(原始知识点文本);
                    var 已有映射 = _知识点映射仓储.根据归一化原始文本获取(归一化文本);
                    if (已有映射 == null)
                    {
                        return new 知识点映射展示项
                        {
                            原始知识点文本 = 原始知识点文本,
                            是否已解决 = false,
                        };
                    }

                    if (已有映射.是否抛弃)
                    {
                        return new 知识点映射展示项
                        {
                            原始知识点文本 = 原始知识点文本,
                            是否已解决 = true,
                            是否抛弃 = true,
                        };
                    }

                    var 目标标签 = 已有映射.目标标签ID.HasValue ? _标签仓储.GetById(已有映射.目标标签ID.Value) : null;
                    if (目标标签 == null)
                    {
                        return new 知识点映射展示项
                        {
                            原始知识点文本 = 原始知识点文本,
                            是否已解决 = false,
                        };
                    }

                    return new 知识点映射展示项
                    {
                        原始知识点文本 = 原始知识点文本,
                        是否已解决 = true,
                        目标标签ID = 目标标签.Id,
                        目标标签名称 = 目标标签.名称,
                    };
                })
                .ToList();
        }

        private string 获取或生成题目预览Html(int 试卷记录ID, 试卷题目项 试卷题目项)
        {
            var 会话目录 = Path.Combine(Path.GetTempPath(), "WordSolution", "import-paper-preview", $"paper-{试卷记录ID}");
            Directory.CreateDirectory(会话目录);
            var 题目文件路径 = Path.Combine(会话目录, $"preview-{试卷题目项.Id}.docx");
            var 预览文件路径 = Path.Combine(会话目录, $"preview-{试卷题目项.Id}.html");

            if (!File.Exists(预览文件路径))
            {
                _题目文档转换器.保存Ooxml为题目文件(试卷题目项.完整Ooxml内容, 题目文件路径);
                _题目预览生成器.生成HTML预览(题目文件路径, 预览文件路径);
            }

            return File.ReadAllText(预览文件路径);
        }
    }
}
