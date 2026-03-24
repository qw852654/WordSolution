using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace 题库核心.试卷导入模块.领域
{
    public class 试卷题目项
    {
        private static readonly JsonSerializerOptions Json序列化选项 = new(JsonSerializerDefaults.Web);

        private 试卷题目项()
        {
        }

        private 试卷题目项(
            int id,
            int 试卷记录ID,
            int 顺序号,
            string 题号文本,
            string 题目摘要,
            string 完整Ooxml内容,
            string 题目正文Ooxml内容,
            string 原始难度文本,
            string 原始知识点Json,
            int? 推荐题型ID,
            string? 推荐题型名称,
            string 识别说明,
            double 置信度,
            试卷题目项状态 状态,
            int? 正式题目ID)
        {
            Id = id;
            this.试卷记录ID = 试卷记录ID;
            this.顺序号 = 顺序号;
            this.题号文本 = 题号文本;
            this.题目摘要 = 题目摘要;
            this.完整Ooxml内容 = 完整Ooxml内容;
            this.题目正文Ooxml内容 = 题目正文Ooxml内容;
            this.原始难度文本 = 原始难度文本;
            this.原始知识点Json = 原始知识点Json;
            this.推荐题型ID = 推荐题型ID;
            this.推荐题型名称 = 推荐题型名称;
            this.识别说明 = 识别说明;
            this.置信度 = 置信度;
            this.状态 = 状态;
            this.正式题目ID = 正式题目ID;
        }

        public int Id { get; private set; }

        public int 试卷记录ID { get; private set; }

        public int 顺序号 { get; private set; }

        public string 题号文本 { get; private set; } = string.Empty;

        public string 题目摘要 { get; private set; } = string.Empty;

        public string 完整Ooxml内容 { get; private set; } = string.Empty;

        public string 题目正文Ooxml内容 { get; private set; } = string.Empty;

        public string 原始难度文本 { get; private set; } = string.Empty;

        public string 原始知识点Json { get; private set; } = "[]";

        public int? 推荐题型ID { get; private set; }

        public string? 推荐题型名称 { get; private set; }

        public string 识别说明 { get; private set; } = string.Empty;

        public double 置信度 { get; private set; }

        public 试卷题目项状态 状态 { get; private set; } = 试卷题目项状态.待处理;

        public int? 正式题目ID { get; private set; }

        public static 试卷题目项 创建(
            int 试卷记录ID,
            int 顺序号,
            string 题号文本,
            string 题目摘要,
            string 完整Ooxml内容,
            string 题目正文Ooxml内容,
            string 原始难度文本,
            IEnumerable<string> 原始知识点列表,
            int? 推荐题型ID,
            string? 推荐题型名称,
            string 识别说明,
            double 置信度)
        {
            return new 试卷题目项(
                0,
                试卷记录ID,
                顺序号,
                题号文本,
                题目摘要,
                完整Ooxml内容,
                题目正文Ooxml内容,
                原始难度文本,
                序列化原始知识点列表(原始知识点列表),
                推荐题型ID,
                推荐题型名称,
                识别说明,
                置信度,
                试卷题目项状态.待处理,
                null);
        }

        public static 试卷题目项 从持久化恢复(
            int id,
            int 试卷记录ID,
            int 顺序号,
            string 题号文本,
            string 题目摘要,
            string 完整Ooxml内容,
            string 题目正文Ooxml内容,
            string 原始难度文本,
            string 原始知识点Json,
            int? 推荐题型ID,
            string? 推荐题型名称,
            string 识别说明,
            double 置信度,
            试卷题目项状态 状态,
            int? 正式题目ID)
        {
            return new 试卷题目项(
                id,
                试卷记录ID,
                顺序号,
                题号文本,
                题目摘要,
                完整Ooxml内容,
                题目正文Ooxml内容,
                原始难度文本,
                原始知识点Json,
                推荐题型ID,
                推荐题型名称,
                识别说明,
                置信度,
                状态,
                正式题目ID);
        }

        public IReadOnlyList<string> 获取原始知识点列表()
        {
            if (string.IsNullOrWhiteSpace(原始知识点Json))
            {
                return Array.Empty<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(原始知识点Json, Json序列化选项)
                    ?.Where(文本 => !string.IsNullOrWhiteSpace(文本))
                    .Select(文本 => 文本.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    ?? new List<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public void 标记为已确认(int 正式题目ID)
        {
            if (状态 != 试卷题目项状态.待处理)
            {
                throw new InvalidOperationException("只有待处理题目项才能确认。");
            }

            if (正式题目ID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(正式题目ID));
            }

            this.正式题目ID = 正式题目ID;
            状态 = 试卷题目项状态.已确认;
        }

        public void 标记为已跳过()
        {
            if (状态 != 试卷题目项状态.待处理)
            {
                throw new InvalidOperationException("只有待处理题目项才能跳过。");
            }

            状态 = 试卷题目项状态.已跳过;
        }

        private static string 序列化原始知识点列表(IEnumerable<string> 原始知识点列表)
        {
            var 结果 = (原始知识点列表 ?? Array.Empty<string>())
                .Where(文本 => !string.IsNullOrWhiteSpace(文本))
                .Select(文本 => 文本.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return JsonSerializer.Serialize(结果, Json序列化选项);
        }
    }
}
