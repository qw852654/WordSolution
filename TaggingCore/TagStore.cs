using System;
using System.Collections.Generic;

namespace TaggingCore
{
    // 模型全部设为 public，避免可访问性冲突
    public class Dimension
    {
        public string Id { get; set; }
        public string Name { get; set; }   // 后面保证同名不重复（忽略大小写）
    }

    public class TagNode
    {
        public string Id { get; set; }
        public string DimensionId { get; set; }
        public string ParentId { get; set; } // null 表示根
        public string Name { get; set; }
    }

    public class Assignment
    {
        public string DocumentId { get; set; } // 先用文档路径
        public string TagId { get; set; }
    }

    public class TagStore
    {
        // 内部存储
        private readonly List<Dimension> _dimensions = new List<Dimension>();
        private readonly List<TagNode> _tags = new List<TagNode>();
        private readonly List<Assignment> _assignments = new List<Assignment>();

        // 暂时直接暴露 List（简单）；以后需要再换 IReadOnlyList
        public List<Dimension> Dimensions => _dimensions;
        public List<TagNode> Tags => _tags;
        public List<Assignment> Assignments => _assignments;

        // 第2步：确保维度存在，不存在就创建并返回其 Id
        private string EnsureDimension(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("维度名称不能为空", nameof(name));

            // 忽略大小写查找
            foreach (var d in _dimensions)
            {
                if (string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase))
                    return d.Id;
            }

            // 没找到就新建
            var dim = new Dimension
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name.Trim()
            };
            _dimensions.Add(dim);
            return dim.Id;
        }

        // 下面的方法后面再一步步加：
        // EnsureTagPath(...)
        // AssignTag(...)
        // GetTagsForDocument(...)
        // GetDocumentsByTag(...)
        // Save / Load (JSON)
    }
}
