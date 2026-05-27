using System.Collections.Generic;
using System.Linq;
using 题库核心.小节模块.契约;
using 题库核心.小节模块.领域;

namespace 题库应用.小节模块
{
    public class 获取小节列表用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly 小节结果构建器 _小节结果构建器;

        public 获取小节列表用例(I小节仓储 小节仓储, 小节结果构建器 小节结果构建器)
        {
            _小节仓储 = 小节仓储;
            _小节结果构建器 = 小节结果构建器;
        }

        public IReadOnlyList<小节详情结果> 执行(小节状态? 状态, int? 章节标签ID, string? 关键词)
        {
            return _小节仓储
                .查询小节(状态, 章节标签ID, 关键词)
                .Select(_小节结果构建器.构建详情)
                .ToList();
        }
    }
}
