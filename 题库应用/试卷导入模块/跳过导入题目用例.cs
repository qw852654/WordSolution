using System;
using 题库核心.试卷导入模块.契约;

namespace 题库应用.试卷导入模块
{
    public class 跳过导入题目用例
    {
        private readonly I试卷记录仓储 _试卷记录仓储;
        private readonly I试卷题目项仓储 _试卷题目项仓储;
        private readonly 获取当前导入题目用例 _获取当前导入题目用例;

        public 跳过导入题目用例(
            I试卷记录仓储 试卷记录仓储,
            I试卷题目项仓储 试卷题目项仓储,
            获取当前导入题目用例 获取当前导入题目用例)
        {
            _试卷记录仓储 = 试卷记录仓储;
            _试卷题目项仓储 = 试卷题目项仓储;
            _获取当前导入题目用例 = 获取当前导入题目用例;
        }

        public 当前导入题目结果? 执行(int 试卷记录ID, int 试卷题目项ID)
        {
            var 试卷记录 = _试卷记录仓储.根据ID获取(试卷记录ID) ?? throw new InvalidOperationException("试卷记录不存在。");
            var 当前题目项 = _试卷题目项仓储.根据ID获取(试卷题目项ID) ?? throw new InvalidOperationException("试卷题目项不存在。");

            if (当前题目项.试卷记录ID != 试卷记录ID)
            {
                throw new InvalidOperationException("试卷题目项与试卷记录不匹配。");
            }

            var 下一道待处理题 = _试卷题目项仓储.获取下一道待处理题(试卷记录ID) ?? throw new InvalidOperationException("当前没有可跳过的题目。");
            if (下一道待处理题.Id != 试卷题目项ID)
            {
                throw new InvalidOperationException("当前题目已经发生变化，请刷新后重试。");
            }

            当前题目项.标记为已跳过();
            _试卷题目项仓储.保存(当前题目项);

            试卷记录.标记已跳过一题();
            _试卷记录仓储.保存(试卷记录);

            return _获取当前导入题目用例.执行(试卷记录ID);
        }
    }
}
