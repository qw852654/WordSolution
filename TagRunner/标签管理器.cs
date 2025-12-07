using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TagRunner
{
    public class 标签管理器
    {
        public List<标签> Tags { get; private set; } = new List<标签>();
        public string TagsJsonPath { get; }

        private Dictionary<int, 标签> _byId = new Dictionary<int, 标签>();

        public 标签管理器(string tagsJsonPath)
        {
            TagsJsonPath = tagsJsonPath ?? throw new ArgumentNullException(nameof(tagsJsonPath));
        }

        public void Load()
        {
            if (!File.Exists(TagsJsonPath))
            {
                Tags = new List<标签>();
                _byId = new Dictionary<int, 标签>();
                return;
            }

            var json = File.ReadAllText(TagsJsonPath);
            var flat = JsonConvert.DeserializeObject<List<标签>>(json) ?? new List<标签>();

            _byId = flat.ToDictionary(t => t.Id, t => new 标签
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId,
                Children = new List<标签>()
            });

            Tags = new List<标签>();
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
            var flat = Flatten().Select(t => new 标签
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId
            }).ToList();

            var json = JsonConvert.SerializeObject(flat, Formatting.Indented);
            File.WriteAllText(TagsJsonPath, json);
        }

        public IEnumerable<标签> Flatten()
        {
            foreach (var root in Tags)
            {
                foreach (var t in Traverse(root))
                    yield return t;
            }
        }

        private IEnumerable<标签> Traverse(标签 t)
        {
            yield return t;
            foreach (var c in t.Children)
            {
                foreach (var x in Traverse(c))
                    yield return x;
            }
        }

        public 标签 GetById(int id) => _byId.TryGetValue(id, out var t) ? t : null;

        public List<int> GetAllChildTagIds(int parentId)
        {
            var result = new List<int>();
            var parent = GetById(parentId);
            if (parent == null) return result;

            void Walk(标签 node)
            {
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
                    foreach (var leaf in GetDescendantLeafTags(tag))
                        set.Add(leaf.Id);
                }
            }
            return set;
        }

        private IEnumerable<标签> GetDescendantLeafTags(标签 node)
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
