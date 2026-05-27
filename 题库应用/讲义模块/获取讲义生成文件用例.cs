using 题库核心.讲义模块.契约;

namespace 题库应用.讲义模块
{
    public class 获取讲义生成文件用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly I讲义文件存储 _讲义文件存储;

        public 获取讲义生成文件用例(I讲义仓储 讲义仓储, I讲义文件存储 讲义文件存储)
        {
            _讲义仓储 = 讲义仓储;
            _讲义文件存储 = 讲义文件存储;
        }

        public (byte[] 文件内容, string 文件名)? 执行(int 讲义ID, int 生成记录ID)
        {
            var 记录 = _讲义仓储.获取生成记录(生成记录ID);
            if (记录 == null || 记录.讲义ID != 讲义ID)
            {
                return null;
            }

            var 文件内容 = _讲义文件存储.读取生成文件(记录.文件路径);
            if (文件内容 == null)
            {
                return null;
            }

            return (文件内容, System.IO.Path.GetFileName(记录.文件路径));
        }
    }
}
