using System.Collections.Generic;
using System.Linq;

namespace 题库核心.筛选模块.领域
{
    public class 筛选步骤
    {
        private readonly List<int> _标签ID列表 = new();

        public 筛选步骤(
            IEnumerable<int> 标签ID列表,
            组合方式 本步标签组合方式 = 组合方式.交集,
            组合方式 与前一步结果组合方式 = 组合方式.交集)
        {
            更新步骤(标签ID列表, 本步标签组合方式, 与前一步结果组合方式);
        }

        public IReadOnlyList<int> 标签ID列表 => _标签ID列表;

        public 组合方式 本步标签组合方式 { get; private set; }

        public 组合方式 与前一步结果组合方式 { get; private set; }

        public void 更新步骤(
            IEnumerable<int> 标签ID列表,
            组合方式 本步标签组合方式,
            组合方式 与前一步结果组合方式)
        {
            _标签ID列表.Clear();

            if (标签ID列表 != null)
            {
                _标签ID列表.AddRange(标签ID列表.Where(id => id > 0).Distinct());
            }

            this.本步标签组合方式 = 本步标签组合方式;
            this.与前一步结果组合方式 = 与前一步结果组合方式;
        }
    }
}
