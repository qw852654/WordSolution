using Core.QuestionBank.Domain;
using System.Collections.Generic;

namespace TagRunner.业务
{
    /// <summary>
    /// 题目业务服务接口：负责题目的业务流程（文件复制/转换/元数据写入/触发索引等）。
    /// </summary>
    public interface I题目服务
    {
        int 新增题目(List<标签> 标签集合, string 源文件路径);
        bool 删除题目(int 题目Id);
        bool 更新题目(题目 修改题目);
        List<题目> 标签找题(List<标签> 标签列表);
        void 设置题目标签(int 题目Id, IEnumerable<int> 标签Ids);
        string 获取题目文档路径(题目 q);
    }
}
