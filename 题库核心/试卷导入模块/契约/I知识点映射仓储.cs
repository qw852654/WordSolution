using 题库核心.试卷导入模块.领域;

namespace 题库核心.试卷导入模块.契约
{
    public interface I知识点映射仓储
    {
        知识点映射? 根据归一化原始文本获取(string 归一化原始文本);

        void 增加(知识点映射 知识点映射);
    }
}
