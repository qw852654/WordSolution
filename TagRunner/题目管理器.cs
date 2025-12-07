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

        public List<题目> QueryQuestions(
            标签管理器 tagManager,
            IEnumerable<int> selectedParentTagIds = null,
            int? minDifficulty = null,
            int? maxDifficulty = null,
            IEnumerable<string> types = null,
            string status = null)
        {
            var typeSet = types != null ? new HashSet<string>(types) : null;
            HashSet<int> targetLeafTagIds = null;

            if (selectedParentTagIds != null)
            {
                targetLeafTagIds = tagManager.ExpandToLeafIds(selectedParentTagIds);
            }

            var result = Questions.Values.Where(q =>
            {
                if (minDifficulty.HasValue && q.Difficulty < minDifficulty.Value) return false;
                if (maxDifficulty.HasValue && q.Difficulty > maxDifficulty.Value) return false;
                if (typeSet != null && !typeSet.Contains(q.Type)) return false;
                if (!string.IsNullOrEmpty(status) && !string.Equals(q.Status, status, StringComparison.OrdinalIgnoreCase)) return false;

                if (targetLeafTagIds != null && targetLeafTagIds.Count > 0)
                {
                    if (!q.TagIds.Any(t => targetLeafTagIds.Contains(t))) return false;
                }

                return true;
            }).ToList();

            return result;
        }
    }
}
