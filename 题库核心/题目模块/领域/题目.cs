using System;
using System.Collections.Generic;
using System.Linq;

namespace 题库核心.题目模块.领域
{
    public class 题目
    {
        private readonly List<int> _标签ID列表 = new();

        private 题目()
        {
        }

        private 题目(
            int id,
            string? description,
            DateTime createdTime,
            DateTime updateTime,
            IEnumerable<int>? 标签ID列表)
        {
            Id = id;
            Description = description;
            CreatedTime = createdTime;
            UpdateTime = updateTime;

            if (标签ID列表 != null)
            {
                _标签ID列表.AddRange(标签ID列表.Distinct());
            }
        }

        public int Id { get; private set; }

        public string? Description { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public DateTime UpdateTime { get; private set; }

        public IReadOnlyList<int> 标签ID列表 => _标签ID列表;

        public static 题目 创建题目(string? description, IEnumerable<int>? 标签ID列表)
        {
            var now = DateTime.Now;
            return new 题目(0, description, now, now, 标签ID列表);
        }

        public static 题目 从持久化恢复题目(
            int id,
            string? description,
            DateTime createdTime,
            DateTime updateTime,
            IEnumerable<int>? 标签ID列表)
        {
            return new 题目(id, description, createdTime, updateTime, 标签ID列表);
        }

        public void 修改题目(string? description, IEnumerable<int>? 标签ID列表)
        {
            Description = description;
            UpdateTime = DateTime.Now;

            _标签ID列表.Clear();
            if (标签ID列表 != null)
            {
                _标签ID列表.AddRange(标签ID列表.Distinct());
            }
        }
    }
}
