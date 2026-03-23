namespace 题库应用.题目模块.题型识别.题型特征
{
    public class 解答题规则
    {
        public (bool 命中, double 置信度, string 说明) 判断()
        {
            return (true, 0.6, "未命中其他题型强规则，按剩余类归为解答题。");
        }
    }
}
