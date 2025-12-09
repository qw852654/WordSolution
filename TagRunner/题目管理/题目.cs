using System.Collections.Generic;

namespace TagRunner
{
    // 题目状态枚举
    public enum 题目状态
    {
        待审核 = 0,
        已标注 = 1
    }

    public class 题目
    {
        public int Id { get; set; }                         // 唯一标识，如 1
        public List<int> TagIds { get; set; } = new List<int>(); // 关联的叶子标签ID
        public 题目状态 Status { get; set; } = 题目状态.待审核;   // 默认待审核

        public override bool Equals(object obj)
        {
            return obj is 题目 other && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
