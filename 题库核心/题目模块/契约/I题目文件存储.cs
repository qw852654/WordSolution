namespace 题库核心.题目模块.契约
{
    public interface I题目文件存储
    {
        string 保存题目文件(int 题目ID, byte[] 文件内容, string 文件扩展名);

        string 获取题目文件路径(int 题目ID, string 文件扩展名 = ".docx");

        string 获取题目预览文件路径(int 题目ID);

        string? 读取题目预览HTML(int 题目ID);
    }
}
