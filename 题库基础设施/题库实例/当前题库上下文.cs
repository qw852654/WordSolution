namespace 题库基础设施.题库实例
{
    public class 当前题库上下文
    {
        public string? 题库键 { get; private set; }

        public void 设置题库键(string 题库键)
        {
            this.题库键 = 题库键;
        }
    }
}
