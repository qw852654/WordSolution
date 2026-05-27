using System.Collections.Concurrent;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库基础设施.内容块模块
{
    public class 内存内容块编辑会话存储 : I内容块编辑会话存储
    {
        private readonly ConcurrentDictionary<string, 内容块编辑会话> _会话字典 = new();

        public void 保存(内容块编辑会话 会话)
        {
            _会话字典[会话.会话ID] = 会话;
        }

        public 内容块编辑会话? 获取(string 会话ID)
        {
            if (string.IsNullOrWhiteSpace(会话ID))
            {
                return null;
            }

            return _会话字典.TryGetValue(会话ID, out var 会话) ? 会话 : null;
        }

        public IReadOnlyList<内容块编辑会话> 获取活动会话()
        {
            return _会话字典.Values
                .Where(会话 => !会话.是终态())
                .OrderBy(会话 => 会话.创建时间)
                .ToList();
        }
    }
}
