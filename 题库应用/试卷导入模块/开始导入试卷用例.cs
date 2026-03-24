using System;
using System.IO;
using System.Linq;
using 题库应用.试卷导入模块.试卷解析;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库应用.试卷导入模块
{
    public class 开始导入试卷用例
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I试卷记录仓储 _试卷记录仓储;
        private readonly I试卷源文件仓储 _试卷源文件仓储;
        private readonly I试卷题目项仓储 _试卷题目项仓储;
        private readonly I试卷源文件存储 _试卷源文件存储;
        private readonly 当前模板试卷解析器 _当前模板试卷解析器;
        private readonly 获取当前导入题目用例 _获取当前导入题目用例;

        public 开始导入试卷用例(
            I标签仓储 标签仓储,
            I试卷记录仓储 试卷记录仓储,
            I试卷源文件仓储 试卷源文件仓储,
            I试卷题目项仓储 试卷题目项仓储,
            I试卷源文件存储 试卷源文件存储,
            当前模板试卷解析器 当前模板试卷解析器,
            获取当前导入题目用例 获取当前导入题目用例)
        {
            _标签仓储 = 标签仓储;
            _试卷记录仓储 = 试卷记录仓储;
            _试卷源文件仓储 = 试卷源文件仓储;
            _试卷题目项仓储 = 试卷题目项仓储;
            _试卷源文件存储 = 试卷源文件存储;
            _当前模板试卷解析器 = 当前模板试卷解析器;
            _获取当前导入题目用例 = 获取当前导入题目用例;
        }

        public 开始导入试卷结果 执行(string 题库键, string 原始文件名, byte[] 文件内容, int 年份标签ID, int 来源标签ID)
        {
            if (string.IsNullOrWhiteSpace(原始文件名))
            {
                throw new InvalidOperationException("请选择试卷文件。");
            }

            if (!string.Equals(Path.GetExtension(原始文件名), ".docx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("当前只支持导入 docx 文件。");
            }

            校验标签种类(年份标签ID, 系统标签种类.年份, "年份");
            校验标签种类(来源标签ID, 系统标签种类.来源, "来源");

            var 已有试卷 = _试卷记录仓储.根据年份与来源获取(年份标签ID, 来源标签ID);
            if (已有试卷 != null && _试卷题目项仓储.存在题目项(已有试卷.Id))
            {
                if (已有试卷.状态 == 试卷导入状态.已完成)
                {
                    throw new InvalidOperationException("当前年份和来源对应的试卷已经导入完成，请不要重复导入。");
                }

                return new 开始导入试卷结果
                {
                    试卷记录ID = 已有试卷.Id,
                    当前题目 = _获取当前导入题目用例.执行(已有试卷.Id),
                    已完成 = false,
                };
            }

            var 试卷记录 = 已有试卷 ?? _试卷记录仓储.获取或创建(年份标签ID, 来源标签ID, Path.GetFileNameWithoutExtension(原始文件名));
            var 保存结果 = _试卷源文件存储.保存源文件(试卷记录.Id, 原始文件名, 文件内容);
            _试卷源文件仓储.增加(试卷源文件记录.创建(试卷记录.Id, 原始文件名, 保存结果.相对路径, DateTime.Now));

            var 解析结果 = _当前模板试卷解析器.解析(保存结果.绝对路径);
            if (解析结果.草稿题列表.Count == 0)
            {
                throw new InvalidOperationException("未能从当前试卷中拆分出题目。");
            }

            试卷记录.更新显示名称(string.IsNullOrWhiteSpace(解析结果.显示名称) ? 试卷记录.显示名称 : 解析结果.显示名称);
            试卷记录.设置总题数(解析结果.草稿题列表.Count);
            _试卷记录仓储.保存(试卷记录);

            var 试卷题目项列表 = 解析结果.草稿题列表.Select(草稿题 => 试卷题目项.创建(
                试卷记录.Id,
                草稿题.序号,
                草稿题.题号文本,
                草稿题.题目摘要,
                草稿题.完整Ooxml内容,
                草稿题.题目正文Ooxml内容,
                草稿题.原始难度文本,
                草稿题.原始知识点列表,
                草稿题.推荐题型ID,
                草稿题.推荐题型名称,
                草稿题.识别说明,
                草稿题.置信度)).ToList();
            _试卷题目项仓储.批量新增(试卷题目项列表);

            return new 开始导入试卷结果
            {
                试卷记录ID = 试卷记录.Id,
                当前题目 = _获取当前导入题目用例.执行(试卷记录.Id),
                已完成 = false,
            };
        }

        private void 校验标签种类(int 标签ID, int 预期标签种类ID, string 字段名称)
        {
            var 标签 = _标签仓储.GetById(标签ID);
            if (标签 == null || 标签.标签种类ID != 预期标签种类ID)
            {
                throw new InvalidOperationException($"{字段名称}标签无效。");
            }
        }
    }
}
