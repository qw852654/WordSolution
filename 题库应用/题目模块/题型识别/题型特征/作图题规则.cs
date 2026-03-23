namespace 题库应用.题目模块.题型识别.题型特征
{
    public class 作图题规则
    {
        public (bool 命中, double 置信度, string 说明) 判断(题型特征集 特征, int 关键词命中数量)
        {
            if (!特征.是否有图片或绘图对象)
            {
                return (false, 0, "未检测到图片或绘图对象。");
            }

            if (关键词命中数量 <= 0)
            {
                return (false, 0, "未命中作图题关键词。");
            }

            return (true, 0.92, "检测到图片/绘图对象，且命中作图关键词。");
        }
    }
}
