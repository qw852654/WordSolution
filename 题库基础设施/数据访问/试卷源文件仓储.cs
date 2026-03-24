using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库基础设施.数据访问
{
    public class 试卷源文件仓储 : I试卷源文件仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 试卷源文件仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public void 增加(试卷源文件记录 试卷源文件记录)
        {
            _题库DbContext.试卷源文件记录表.Add(试卷源文件记录);
            _题库DbContext.SaveChanges();
        }
    }
}
