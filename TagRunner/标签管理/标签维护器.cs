using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TagRunner
{
    /// <summary>
    /// 标签维护器：基于标签查询服务执行写操作（新增标签），自动分配唯一 Id。
    /// 使用前请确保查询服务已 Load。
    /// </summary>
    public class 标签维护器
    {
        private readonly 标签查询服务 _query;

        public 标签维护器(标签查询服务 queryService)
        {
            _query = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        /// <summary>
        /// 新增标签（自动生成唯一 Id）。必须提供非空名称。
        /// 可选：父标签Id、类别（如“题型”“难度”“知识点”）、数值（用于“难度”等）。
        /// 增加前会检查在同一父节点下是否已存在同名（且同类别）标签。
        /// 增加后会更新树结构与索引，并持久化到 tags.json。
        /// 当提供了不存在的父标签 Id 时，会弹出确认提示，询问是否作为根标签创建。
        /// </summary>
        public int 新增标签(string name, int? parentId = null, string category = null, int? numericValue = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("标签名称不能为空", nameof(name));

            // 先检查重复（同一父节点 + 同类别 + 同名）
            if (ExistsByName(name.Trim(), parentId, category))
                throw new InvalidOperationException($"已存在同名标签：'{name.Trim()}'（父Id={parentId?.ToString() ?? "null"}，类别={category ?? "null"}）");


            // 父存在性检查（当提供 parentId）
            标签 parent = null;
            if (!parentId.HasValue)
            { var result = MessageBox.Show(
                    $"指定的父标签 Id 不存在，是否作为根标签创建？\n\n标签名称：{name.Trim()}\n类别：{category ?? "null"}",
                    "确认创建根标签",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    throw new InvalidOperationException("操作已取消，未创建标签。");
                parentId = null; // 作为根标签
            }
            else
            {
                parent = _query.GetById(parentId.Value);
                if (parent == null)
                    throw new InvalidOperationException($"指定的父标签 Id 不存在，无法创建子标签（Id={parentId.Value}）。");
            }



            // 生成新 Id（最大 Id + 1）
            var newId = GenerateNewId();

            var newTag = new 标签
            {
                Id = newId,
                Name = name.Trim(),
                ParentId = parentId,
                Category = category,
                NumericValue = numericValue,
                Children = new List<标签>()
            };

            // 更新树结构
            if (parent == null)
            {
                _query.TagsTree.Add(newTag);
            }
            else
            {
                parent.Children.Add(newTag);
            }

            // 持久化并重载，保证索引与树一致
            _query.SaveFlat();
            _query.LoadTagsTree();

            return newId;
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
                return _query.TagsTree.Any(t =>
                    string.Equals(t.Name, targetName, StringComparison.Ordinal) &&
                    string.Equals(t.Category, category, StringComparison.Ordinal));
            }

            // 父节点存在性与其子集合
            var parent = _query.GetById(parentId.Value);
            if (parent == null) return false;

            var children = parent.Children ?? new List<标签>();
            return children.Any(t =>
                string.Equals(t.Name, targetName, StringComparison.Ordinal) &&
                string.Equals(t.Category, category, StringComparison.Ordinal));
        }

        /// <summary>
        /// 查找同一父节点下的指定名称（且类别）标签，找不到返回 null。
        /// </summary>
        public 标签 FindTagByName(string name, int? parentId, string category = null)
        {
            var targetName = (name ?? string.Empty).Trim();
            if (!parentId.HasValue)
            {
                return _query.TagsTree.FirstOrDefault(t =>
                    string.Equals(t.Name, targetName, StringComparison.Ordinal) &&
                    string.Equals(t.Category, category, StringComparison.Ordinal));
            }

            var parent = _query.GetById(parentId.Value);
            if (parent == null) return null;

            var children = parent.Children ?? new List<标签>();
            return children.FirstOrDefault(t =>
                string.Equals(t.Name, targetName, StringComparison.Ordinal) &&
                string.Equals(t.Category, category, StringComparison.Ordinal));
        }

        /// <summary>
        /// 通过遍历当前标签树（含所有子孙），生成新的唯一 Id：取现有最大 Id + 1。
        /// 若当前无任何标签，返回 1。
        /// </summary>
        private int GenerateNewId()
        {
            int maxId = 0;
            foreach (var t in _query.Flatten())
            {
                if (t.Id > maxId) maxId = t.Id;
            }
            // 若无标签则从 1 开始
            return checked(maxId + 1);
        }
    }
}