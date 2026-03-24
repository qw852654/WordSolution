namespace 题库核心.试卷导入模块.契约
{
    public interface I试卷源文件存储
    {
        (string 相对路径, string 绝对路径) 保存源文件(int 试卷记录ID, string 原始文件名, byte[] 文件内容);
    }
}
