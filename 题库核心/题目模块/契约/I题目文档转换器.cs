namespace 题库核心.题目模块.契约
{
    public interface I题目文档转换器
    {
        void 保存Ooxml为题目文件(string Ooxml内容, string 题目文件路径);

        string 读取题目文件Ooxml(string 题目文件路径);
    }
}
