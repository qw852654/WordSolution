namespace 题库核心.讲义模块.契约
{
    public interface I讲义文件存储
    {
        string 获取讲义生成文件路径(int 讲义ID, string 文件名);

        byte[]? 读取生成文件(string 文件路径);
    }
}
