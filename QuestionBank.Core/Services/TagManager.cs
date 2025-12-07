using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using QuestionBank.Core.Models;

namespace QuestionBank.Core.Services
{
    public class TagManager
    {
        public List<Tag> Tags { get; private set; } = new List<Tag>();
        public string TagsJsonPath { get; }

        private Dictionary<int, Tag> _byId = new Dictionary<int, Tag>();

        public TagManager(string tagsJsonPath)
        {
            TagsJsonPath = tagsJsonPath ?? throw new ArgumentNullException(nameof(tagsJsonPath));
        }

        public void Load()
        {
            if (!File.Exists(TagsJsonPath))
            {
                Tags = new List<Tag>();
                _byId = new Dictionary<int, Tag>();
                return;
            }

            var json = File.ReadAllText(TagsJsonPath);
            var flat = JsonConvert.DeserializeObject<List<Tag>>(json) ?? new List<Tag>();
            // 扁平列表构建树与索引
            _byId = flat.ToDictionary(t => t.Id, t => new Tag
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId,
                Children = new List<Tag>()
            });

            Tags = new List<Tag>();
            foreach (var tag in _byId.Values)
            {
                if (tag.ParentId.HasValue && _byId.TryGetValue(tag.ParentId.Value, out var parent))
                {
                    parent.Children.Add(tag);
                }
                else
                {
                    Tags.Add(tag);
                }
            }
        }

        public void SaveFlat()
        {
            // 扁平化保存，便于编辑与版本管理
            var flat = Flatten().Select(t => new Tag
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId
            }).ToList();

            var json = JsonConvert.SerializeObject(flat, Formatting.Indented);
            File.WriteAllText(TagsJsonPath, json);
        }

        public IEnumerable<Tag> Flatten()
        {
            foreach (var root in Tags)
            {
                foreach (var t in Traverse(root))
                    yield return t;
            }
        }

        private IEnumerable<Tag> Traverse(Tag t)
        {
            yield return t;
            foreach (var c in t.Children)
            {
                foreach (var x in Traverse(c))
                    yield return x;
            }
        }

        public Tag GetById(int id) => _byId.TryGetValue(id, out var t) ? t : null;

        public List<int> GetAllChildTagIds(int parentId)
        {
            var result = new List<int>();
            var parent = GetById(parentId);
            if (parent == null) return result;

            void Walk(Tag node)
            {
                // 子孙节点ID（不包含父本身）
                foreach (var child in node.Children)
                {
                    result.Add(child.Id);
                    Walk(child);
                }
            }

            Walk(parent);

            return result;
        }

        public HashSet<int> ExpandToLeafIds(IEnumerable<int> selectedParentTagIds)
        {
            // 将选中的父标签展开为其子孙叶子标签ID集合；若选择的是叶子，也包含它本身
            var set = new HashSet<int>();
            foreach (var id in selectedParentTagIds ?? Enumerable.Empty<int>())
            {
                var tag = GetById(id);
                if (tag == null) continue;

                if (tag.Children.Count == 0)
                {
                    set.Add(tag.Id);
                }
                else
                {
                    // 收集所有子孙节点（仅叶子）
                    foreach (var leaf in GetDescendantLeafTags(tag))
                        set.Add(leaf.Id);
                }
            }
            return set;
        }

        private IEnumerable<Tag> GetDescendantLeafTags(Tag node)
        {
            if (node.Children.Count == 0)
            {
                yield return node;
                yield break;
            }

            foreach (var child in node.Children)
            {
                foreach (var leaf in GetDescendantLeafTags(child))
                    yield return leaf;
            }
        }
    }
}