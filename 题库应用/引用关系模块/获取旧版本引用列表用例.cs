using System.Collections.Generic;

namespace 题库应用.引用关系模块
{
    public class 获取旧版本引用列表用例
    {
        private readonly 引用关系分析器 _引用关系分析器;

        public 获取旧版本引用列表用例(引用关系分析器 引用关系分析器)
        {
            _引用关系分析器 = 引用关系分析器;
        }

        public IReadOnlyList<旧版本引用结果> 执行()
        {
            return _引用关系分析器.获取全部旧版本引用();
        }
    }
}
