using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 删除题目用例
    {
        private readonly I题目仓储 _题目仓储;
        private readonly I题目文件存储 _题目文件存储;

        public 删除题目用例(I题目仓储 题目仓储, I题目文件存储 题目文件存储)
        {
            _题目仓储 = 题目仓储;
            _题目文件存储 = 题目文件存储;
        }

        public bool 执行(int 题目ID)
        {
            var 题目 = _题目仓储.GetById(题目ID);
            if (题目 == null)
            {
                return false;
            }

            _题目仓储.删除题目(题目ID);
            _题目文件存储.删除题目文件(题目ID);
            _题目文件存储.删除题目预览文件(题目ID);
            return true;
        }
    }
}
