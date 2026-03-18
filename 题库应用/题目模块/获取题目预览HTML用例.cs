using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 获取题目预览HTML用例
    {
        private readonly I题目文件存储 _题目文件存储;

        public 获取题目预览HTML用例(I题目文件存储 题目文件存储)
        {
            _题目文件存储 = 题目文件存储;
        }

        public string? 执行(int 题目ID)
        {
            return _题目文件存储.读取题目预览HTML(题目ID);
        }
    }
}
