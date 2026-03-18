using 题库核心.题目模块.契约;
using 题库核心.题目模块.领域;

namespace 题库应用.题目模块
{
    public class 根据ID获取题目详情用例
    {
        private readonly I题目仓储 _题目仓储;

        public 根据ID获取题目详情用例(I题目仓储 题目仓储)
        {
            _题目仓储 = 题目仓储;
        }

        public 题目? 执行(int id)
        {
            return _题目仓储.GetById(id);
        }
    }
}
