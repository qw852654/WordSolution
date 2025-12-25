using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using TagRunner.标签管理;

namespace TagRunner
{
    

    //<summary>
    //主要根据标签json文件加载标签树、查询标签、索引标签等查询服务
    //提供：
    //- 加载标签树（从扁平化 JSON 文件，无层级，只有父子节点记录）
    //- 标签Id查找标签名字
    //- 标签Id查找子标签列表
    //</summary>
    public class 标签查询服务 : I标签查询服务
    {
        private static 标签查询服务 _instance;
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        public List<标签> 标签树根 { get; private set; } = new List<标签>();
        public string TagsJsonPath { get; }

        private Dictionary<int, 标签> 标签ID字典_即标签树 = new Dictionary<int, 标签>();

        // 私有构造函数，防止外部直接 new
        private 标签查询服务(string 题库目录)
        {
            var tagsJsonPath = Path.Combine(题库目录, "tags.json");
            TagsJsonPath = tagsJsonPath ?? throw new ArgumentNullException(nameof(tagsJsonPath));
            if (!File.Exists(tagsJsonPath))
                throw new InvalidOperationException($"题库标签文件不存在:{tagsJsonPath}");
            加载标签树();
        }

        /// <summary>
        /// 初始化单例实例，只能调用一次
        /// </summary>
        public static void Initialize(string 题库目录)
        {
            
            lock (_lock)
            {
                _instance = new 标签查询服务(题库目录);
                _initialized = true;
            }
        }

        /// <summary>
        /// 获取单例实例，未初始化时抛异常
        /// </summary>
        public static 标签查询服务 Instance
        {
            get
            {
                if (!_initialized)
                    throw new InvalidOperationException("标签查询服务未初始化，请先调用 Initialize(题库目录)。");
                return _instance;
            }
        }

        /// <summary>
        /// 从指定的 JSON 文件路径加载标签的扁平列表，构建内存中的标签树（父子关系）
        /// 并建立按 Id 的索引字典以便快速查找。
        /// 若文件不存在则初始化为空的树与索引。
        /// </summary>
        public void 加载标签树()
        {
            if (!File.Exists(TagsJsonPath))
            {
                //提示标签文件不存在
                MessageBox.Show($"标签文件不存在：{TagsJsonPath}，初始化失败。", "标签文件缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var json = File.ReadAllText(TagsJsonPath);
            var flat = JsonConvert.DeserializeObject<List<标签>>(json) ?? new List<标签>();

            // 构建标签ID字典
            标签ID字典_即标签树 = flat.ToDictionary(t => t.Id, t => new 标签
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

            标签树根 = new List<标签>();
            var 父节点丢失的标签 = new List<标签>();

            // 第一遍：仅根节点
            foreach (var tag in 标签ID字典_即标签树.Values)
            {
                if (!tag.ParentId.HasValue)
                {
                    标签树根.Add(tag);
                }
            }

            // 第二遍：连接子节点到父
            foreach (var tag in 标签ID字典_即标签树.Values)
            {
                if (tag.ParentId.HasValue)
                {
                    if (标签ID字典_即标签树.TryGetValue(tag.ParentId.Value, out var parent))
                    {
                        parent.Children.Add(tag);
                    }
                    else
                    {
                        // 父缺失：收集为孤儿，避免错误加入根
                        父节点丢失的标签.Add(tag);
                    }
                }
            }

            // 如需将孤儿提升为根，可在此处选择性处理（当前策略：不加入根，防止误判）
            // 如果业务需要，也可以：TagsTree.AddRange(orphans);
        }

        

        

        /// <summary>
        /// 深度优先遍历当前标签树，返回所有标签节点的序列（包含根与子孙节点）。
        /// </summary>
        public IEnumerable<标签> 遍历标签树获取标签()
        {
            foreach (var root in 标签树根)
            {
                foreach (var t in Traverse(root))
                    yield return t;
            }
        }

        /// <summary>
        /// 判断在同一父节点下是否存在同名（且同类别）标签。
        /// 若 parentId 为 null，则判断根集合；类别为 null 也参与匹配（严格比较）。
        /// </summary>
        public bool ExistsByName(string name, int? parentId, string category = null)
        {
            var targetName = (name ?? string.Empty).Trim();
            // 根集合
            if (!parentId.HasValue)
            {
                return 标签树根.Any(t =>
                    string.Equals(t.Name, targetName, StringComparison.Ordinal) &&
                    string.Equals(t.Category, category, StringComparison.Ordinal));
            }

            // 父节点存在性与其子集合
            var parent = GetById(parentId.Value);
            if (parent == null) return false;

            var children = parent.Children ?? new List<标签>();
            return children.Any(t =>
                string.Equals(t.Name, targetName, StringComparison.Ordinal) &&
                string.Equals(t.Category, category, StringComparison.Ordinal));
        }

        /// <summary>
        /// 查找同一父节点下的指定名称（且类别）标签，找不到返回 null。
        /// </summary>
        

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
        public 标签 GetById(int id) => 标签ID字典_即标签树.TryGetValue(id, out var t) ? t : null;

        /// <summary>
        /// 返回某父标签的所有子孙标签 Id（不包含父标签本身），
        /// 用于在选择父节点时获取其覆盖范围。
        /// </summary>
        public List<int> ID获取当前标签及其子孙标签列表(标签 parentTag)
        {
            if (parentTag == null)
                throw new ArgumentNullException(nameof(parentTag));
            var result = new List<int>();
            var parent = parentTag;

            result.Add(parentTag.Id); // 包含父标签本身

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

        public 标签 标签名获取标签(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return null;
            foreach (var tag in 遍历标签树获取标签())
            {
                if (string.Equals(tag.Name, tagName.Trim(), StringComparison.Ordinal))
                    return tag;
            }
            return null;
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
