using System;
using 题库核心.试卷导入模块.契约;

namespace 题库应用.试卷导入模块
{
    public class 获取当前导入题目用例
    {
        private readonly I试卷记录仓储 _试卷记录仓储;
        private readonly I试卷题目项仓储 _试卷题目项仓储;
        private readonly 当前导入题目结果构建器 _当前导入题目结果构建器;

        public 获取当前导入题目用例(
            I试卷记录仓储 试卷记录仓储,
            I试卷题目项仓储 试卷题目项仓储,
            当前导入题目结果构建器 当前导入题目结果构建器)
        {
            _试卷记录仓储 = 试卷记录仓储;
            _试卷题目项仓储 = 试卷题目项仓储;
            _当前导入题目结果构建器 = 当前导入题目结果构建器;
        }

        public 当前导入题目结果? 执行(int 试卷记录ID)
        {
            var 试卷记录 = _试卷记录仓储.根据ID获取(试卷记录ID);
            if (试卷记录 == null)
            {
                throw new InvalidOperationException("试卷记录不存在。");
            }

            var 当前题目项 = _试卷题目项仓储.获取下一道待处理题(试卷记录ID);
            if (当前题目项 == null)
            {
                return null;
            }

            return _当前导入题目结果构建器.构建(试卷记录, 当前题目项);
        }
    }
}
