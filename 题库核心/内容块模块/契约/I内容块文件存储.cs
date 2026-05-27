namespace 题库核心.内容块模块.契约
{
    public interface I内容块文件存储
    {
        string 获取内容块文件路径(int 内容块ID, int 版本号, string 文件扩展名 = ".docx");

        string 获取内容块文件路径(string 题库键, int 内容块ID, int 版本号, string 文件扩展名 = ".docx");

        string 获取内容块预览文件路径(int 内容块ID, int 版本号);

        string 获取内容块预览文件路径(string 题库键, int 内容块ID, int 版本号);

        byte[]? 读取内容块文件(string 文件路径);

        string? 读取内容块预览HTML(string HTML文件路径);
    }
}
