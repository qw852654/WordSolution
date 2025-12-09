using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TagRunner
{
    public class 标签查询服务
    {
        public List<标签> TagsTree { get; private set; } = new List<标签>();
        public string TagsJsonPath { get; }

        private Dictionary<int, 标签> _byId = new Dictionary<int, 标签>();

        public 标签查询服务(string tagsJsonPath)
        {
            TagsJsonPath = tagsJsonPath ?? throw new ArgumentNullException(nameof(tagsJsonPath));
        }

        /// <summary>
        /// 从指定的 JSON 文件路径加载标签的扁平列表，构建内存中的标签树（父子关系）
        /// 并建立按 Id 的索引字典以便快速查找。
        /// 若文件不存在则初始化为空的树与索引。
        /// </summary>
        public void LoadTagsTree()
        {
            if (!File.Exists(TagsJsonPath))
            {
                TagsTree = new List<标签>();
                _byId = new Dictionary<int, 标签>();
                return;
            }

            var json = File.ReadAllText(TagsJsonPath);
            var flat = JsonConvert.DeserializeObject<List<标签>>(json) ?? new List<标签>();

            // 构建索引与节点（包含 Category、NumericValue）
            _byId = flat.ToDictionary(t => t.Id, t => new 标签
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId,
                Category = t.Category,
                NumericValue = t.NumericValue,
                Children = new List<标签>()
            });

            // 计划（伪代码）：
            // 1) 初始化 TagsTree 为空，准备一个 orphans 列表收集“父缺失”的节点。
            // 2) 第一遍：仅将 ParentId == null 的节点加入根列表（避免误把有父的节点当根）。
            // 3) 第二遍：为每个有父的节点尝试连接到父的 Children；
            //    - 若能找到父，则加入父.Children；
            //    - 若找不到父（数据缺失），则将其加入 orphans（不作为根，以免误分类）。
            // 4) 可选：orphans 留存（这里不加入到树），避免错误地成为根；后续可由调用方处理或告警。

            TagsTree = new List<标签>();
            var orphans = new List<标签>();

            // 第一遍：仅根节点
            foreach (var tag in _byId.Values)
            {
                if (!tag.ParentId.HasValue)
                {
                    TagsTree.Add(tag);
                }
            }

            // 第二遍：连接子节点到父
            foreach (var tag in _byId.Values)
            {
                if (tag.ParentId.HasValue)
                {
                    if (_byId.TryGetValue(tag.ParentId.Value, out var parent))
                    {
                        parent.Children.Add(tag);
                    }
                    else
                    {
                        // 父缺失：收集为孤儿，避免错误加入根
                        orphans.Add(tag);
                    }
                }
            }

            // 如需将孤儿提升为根，可在此处选择性处理（当前策略：不加入根，防止误判）
            // 如果业务需要，也可以：TagsTree.AddRange(orphans);
        }

        // 计划（伪代码）:
        // - 目标：在保存扁平化 JSON 时保留 null 值（例如 ParentId、Category、NumericValue 的 null）。
        // - 修改 SaveFlat：
        //   1) 保留当前扁平化逻辑（Id/Name/ParentId/Category/NumericValue）。
        //   2) 将 JsonSerializerSettings 的 NullValueHandling 改为 Include，DefaultValueHandling 改为 Include，或移除设置使其默认包含 null。
        //   3) 继续采用临时文件写入与备份策略，确保写入安全。
        // - 验证：生成的 JSON 中应出现属性值为 null 的键。

        public void SaveFlat()
        {
            var flat = Flatten().Select(t => new
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId,
                Category = t.Category,
                NumericValue = t.NumericValue
            }).ToList();

            // 保留 null 值
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include
            };
            var json = JsonConvert.SerializeObject(flat, Formatting.Indented, settings);

            var dir = Path.GetDirectoryName(TagsJsonPath);
            Directory.CreateDirectory(dir ?? ".");

            var tmpPath = Path.Combine(dir ?? ".", Path.GetFileName(TagsJsonPath) + ".tmp");
            var bakPath = Path.Combine(dir ?? ".", Path.GetFileNameWithoutExtension(TagsJsonPath) +
                                                 "_bak_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");

            try
            {
                using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                    sw.Flush();
                    fs.Flush(true);
                }

                if (File.Exists(TagsJsonPath))
                {
                    File.Copy(TagsJsonPath, bakPath, overwrite: true);
                }

                if (File.Exists(TagsJsonPath))
                {
                    File.Delete(TagsJsonPath);
                }
                File.Move(tmpPath, TagsJsonPath);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (File.Exists(tmpPath))
                {
                    try { File.Delete(tmpPath); } catch { /* ignore */ }
                }
            }
        }

        /// <summary>
        /// 深度优先遍历当前标签树，返回所有标签节点的序列（包含根与子孙节点）。
        /// </summary>
        public IEnumerable<标签> Flatten()
        {
            foreach (var root in TagsTree)
            {
                foreach (var t in Traverse(root))
                    yield return t;
            }
        }

        /// <summary>
        /// 递归遍历指定标签节点，先返回当前节点，再递归返回其所有子孙节点。
        /// </summary>
        private IEnumerable<标签> Traverse(标签 t)
        {
            yield return t;
            var children = t.Children ?? new List<标签>();
            foreach (var c in children)
            {
                foreach (var x in Traverse(c))
                    yield return x;
            }
        }

        /// <summary>
        /// 根据标签 Id 从内部字典中快速查找并返回标签对象。
        /// 若不存在则返回 null。
        /// </summary>
        public 标签 GetById(int id) => _byId.TryGetValue(id, out var t) ? t : null;

        /// <summary>
        /// 返回某父标签的所有子孙标签 Id（不包含父标签本身），
        /// 用于在选择父节点时获取其覆盖范围。
        /// </summary>
        public List<int> GetAllChildTagIds(int parentId)
        {
            var result = new List<int>();
            var parent = GetById(parentId);
            if (parent == null) return result;

            void Walk(标签 node)
            {
                var children = node.Children ?? new List<标签>();
                foreach (var child in children)
                {
                    result.Add(child.Id);
                    Walk(child);
                }
            }

            Walk(parent);
            return result;
        }

        /// <summary>
        /// 将选中的标签 Id 集合统一展开为“叶子标签 Id 集合”：
        /// - 若输入为叶子标签，则直接包含其自身；
        /// - 若输入为非叶（父）标签，则递归收集其所有叶子标签。
        /// 返回去重后的叶子 Id 集合。
        /// </summary>
        public HashSet<int> ExpandToLeafIds(IEnumerable<int> selectedParentTagIds)
        {
            var set = new HashSet<int>();
            foreach (var id in selectedParentTagIds ?? Enumerable.Empty<int>())
            {
                var tag = GetById(id);
                if (tag == null) continue;

                var children = tag.Children ?? new List<标签>();
                if (children.Count == 0)
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

        /// <summary>
        /// 获取指定标签节点下的所有“叶子标签”（没有子节点的标签）。
        /// 若节点本身为叶子，则直接返回该节点。
        /// </summary>
        private IEnumerable<标签> GetDescendantLeafTags(标签 node)
        {
            var children = node.Children ?? new List<标签>();
            if (children.Count == 0)
            {
                yield return node;
                yield break;
            }

            foreach (var child in children)
            {
                foreach (var leaf in GetDescendantLeafTags(child))
                    yield return leaf;
            }
        }

        /// <summary>
        /// 根据类别（Category）从给定的题目标签 Id 集合中筛选并返回叶子标签对象，
        /// 常用于按“题型”“难度”等类别维度进行筛选与统计。
        /// </summary>
        public IEnumerable<标签> GetLeafTagsByCategory(IEnumerable<int> tagIds, string category)
        {
            if (tagIds == null || string.IsNullOrEmpty(category)) yield break;
            foreach (var id in tagIds)
            {
                var t = GetById(id);
                if (t != null && string.Equals(t.Category, category, StringComparison.Ordinal))
                    yield return t;
            }
        }
    }
}
