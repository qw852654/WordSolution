using System.Collections.Generic;
using 题库核心.筛选模块.领域;

namespace 题库本地服务.题目模块
{
    public class 筛选步骤请求
    {
        public List<int> 标签ID列表 { get; set; } = new();

        public 组合方式 本步标签组合方式 { get; set; } = 组合方式.交集;

        public 组合方式 与前一步结果组合方式 { get; set; } = 组合方式.交集;
    }
}
