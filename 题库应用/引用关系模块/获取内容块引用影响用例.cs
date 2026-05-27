namespace 题库应用.引用关系模块
{
    public class 获取内容块引用影响用例
    {
        private readonly 引用关系分析器 _引用关系分析器;

        public 获取内容块引用影响用例(引用关系分析器 引用关系分析器)
        {
            _引用关系分析器 = 引用关系分析器;
        }

        public 内容块引用影响结果? 执行(int 内容块ID)
        {
            return _引用关系分析器.分析内容块影响(内容块ID);
        }
    }
}
