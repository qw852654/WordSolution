using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TagRunner
{
    public class 题目管理器
    {
        public Dictionary<string, 题目> Questions { get; private set; } = new Dictionary<string, 题目>(StringComparer.OrdinalIgnoreCase);
        public string IndexJsonPath { get; }

        public 题目管理器(string indexJsonPath)
        {
            IndexJsonPath = indexJsonPath ?? throw new ArgumentNullException(nameof(indexJsonPath));
        }

        public void Load()
        {
            if (!File.Exists(IndexJsonPath))
            {
                Questions = new Dictionary<string, 题目>(StringComparer.OrdinalIgnoreCase);
                return;
            }
            var json = File.ReadAllText(IndexJsonPath);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, 题目>>(json)
                       ?? new Dictionary<string, 题目>(StringComparer.OrdinalIgnoreCase);
            Questions = new Dictionary<string, 题目>(dict, StringComparer.OrdinalIgnoreCase);
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(Questions, Formatting.Indented);
            File.WriteAllText(IndexJsonPath, json);
        }

        public IEnumerable<题目> All() => Questions.Values;

        public void Upsert(题目 q)
        {
            if (q == null || string.IsNullOrWhiteSpace(q.Id))
                throw new ArgumentException("题目 Id 不能为空");
            Questions[q.Id] = q;
        }

        public int BulkUpdateStatus(IEnumerable<string> ids, string status)
        {
            int count = 0;
            foreach (var id in ids ?? Enumerable.Empty<string>())
            {
                if (Questions.TryGetValue(id, out var q))
                {
                    q.Status = status;
                    count++;
                }
            }
            return count;
        }

        public int BulkAddTags(IEnumerable<string> ids, IEnumerable<int> tagIds)
        {
            var set = new HashSet<int>(tagIds ?? Enumerable.Empty<int>());
            int count = 0;
            foreach (var id in ids ?? Enumerable.Empty<string>())
            {
                if (Questions.TryGetValue(id, out var q))
                {
                    foreach (var t in set) if (!q.TagIds.Contains(t)) q.TagIds.Add(t);
                    count++;
                }
            }
            return count;
        }

        public int BulkRemoveTags(IEnumerable<string> ids, IEnumerable<int> tagIds)
        {
            var set = new HashSet<int>(tagIds ?? Enumerable.Empty<int>());
            int count = 0;
            foreach (var id in ids ?? Enumerable.Empty<string>())
            {
                if (Questions.TryGetValue(id, out var q))
                {
                    q.TagIds.RemoveAll(t => set.Contains(t));
                    count++;
                }
            }
            return count;
        }

        // 基于标签类别的查询：父标签展开 + 题型/难度/状态筛选
        public List<题目> QueryQuestions(
            标签查询服务 tagManager,
            IEnumerable<int> selectedParentTagIds = null,
            IEnumerable<string> typeNames = null,      // 目标题型名称集合（如 "选择题","计算题"），匹配 Category="题型" 的标签
            int? minDifficulty = null,                 // 难度下限（匹配 Category="难度" 的 NumericValue）
            int? maxDifficulty = null,                 // 难度上限
            string status = null)
        {
            HashSet<int> targetLeafTagIds = null;
            if (selectedParentTagIds != null)
                targetLeafTagIds = tagManager.ExpandToLeafIds(selectedParentTagIds);

            var typeSet = typeNames != null ? new HashSet<string>(typeNames) : null;

            var result = Questions.Values.Where(q =>
            {
                // 父标签展开匹配（题目只保存叶子标签）
                if (targetLeafTagIds != null && targetLeafTagIds.Count > 0)
                {
                    if (!q.TagIds.Any(t => targetLeafTagIds.Contains(t))) return false;
                }

                // 状态筛选
                if (!string.IsNullOrEmpty(status) && !string.Equals(q.Status, status, StringComparison.OrdinalIgnoreCase))
                    return false;

                // 题型筛选：题目的叶子标签中存在 Category="题型" 且 Name 命中
                if (typeSet != null)
                {
                    var typeTags = tagManager.GetLeafTagsByCategory(q.TagIds, "题型").ToList();
                    if (typeTags.Count == 0) return false; // 没有题型标签则不通过
                    if (!typeTags.Any(t => typeSet.Contains(t.Name))) return false;
                }

                // 难度筛选：取 Category="难度" 的 NumericValue
                if (minDifficulty.HasValue || maxDifficulty.HasValue)
                {
                    var diffTags = tagManager.GetLeafTagsByCategory(q.TagIds, "难度").ToList();
                    if (diffTags.Count == 0) return false; // 没有难度标签则不通过（可按业务改为跳过过滤）
                    // 如果存在多个难度标签，定义一个聚合规则。这里取“最大值”作为代表。
                    int? diff = diffTags.Where(t => t.NumericValue.HasValue).Select(t => t.NumericValue.Value).DefaultIfEmpty().Max();
                    if (!diff.HasValue) return false;
                    if (minDifficulty.HasValue && diff.Value < minDifficulty.Value) return false;
                    if (maxDifficulty.HasValue && diff.Value > maxDifficulty.Value) return false;
                }

                return true;
            }).ToList();

            return result;
        }
    }
}
