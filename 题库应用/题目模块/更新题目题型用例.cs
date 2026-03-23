using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 更新题目题型用例
    {
        private readonly I题目仓储 _题目仓储;
        private readonly 题型规则校验器 _题型规则校验器;

        public 更新题目题型用例(I题目仓储 题目仓储, 题型规则校验器 题型规则校验器)
        {
            _题目仓储 = 题目仓储;
            _题型规则校验器 = 题型规则校验器;
        }

        public bool 执行(int 题目ID, int 题型ID)
        {
            var 题目 = _题目仓储.GetById(题目ID);
            if (题目 == null)
            {
                return false;
            }

            _题型规则校验器.校验存在(题型ID);
            _题目仓储.更新题型(题目ID, 题型ID);
            return true;
        }
    }
}
