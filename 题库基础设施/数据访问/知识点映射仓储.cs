using System.Linq;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库基础设施.数据访问
{
    public class 知识点映射仓储 : I知识点映射仓储
    {
        private readonly 题库DbContext _题库DbContext;

        public 知识点映射仓储(题库DbContext 题库DbContext)
        {
            _题库DbContext = 题库DbContext;
        }

        public 知识点映射? 根据归一化原始文本获取(string 归一化原始文本)
        {
            return _题库DbContext.知识点映射表
                .SingleOrDefault(映射 => 映射.归一化原始文本 == 归一化原始文本);
        }

        public void 增加(知识点映射 知识点映射)
        {
            _题库DbContext.知识点映射表.Add(知识点映射);
            _题库DbContext.SaveChanges();
        }
    }
}
