namespace 题库应用.题目模块.题型识别.题型特征
{
    public class 题型特征集
    {
        public string 归一化文本 { get; set; } = string.Empty;

        public int 归一化后段落数 { get; set; }

        public int 完整ABCD结构组数 { get; set; }

        public int 显式下划线数量 { get; set; }

        public int 下划线格式空白数量 { get; set; }

        public bool 是否有图片或绘图对象 { get; set; }

        public bool 是否存在多小问结构 { get; set; }
    }
}
