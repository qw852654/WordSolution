namespace 题库核心.内容块模块.契约
{
    public interface I内容块文档转换器
    {
        void 保存Ooxml为内容块文件(string Ooxml内容, string 内容块文件路径);

        void 创建空白内容块文件(string 内容块文件路径);

        string 读取内容块文件Ooxml(string 内容块文件路径);

        string 提取纯文本(string 内容块文件路径);
    }
}
