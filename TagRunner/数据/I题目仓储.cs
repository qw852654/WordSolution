using Core.QuestionBank.Domain;
using System.Collections.Generic;
using TagRunner;

namespace TagRunner.数据
{
    /// <summary>
    /// 题目仓储接口：题目元数据的 CRUD 与查询契约。
    /// </summary>
    public interface I题目仓储
    {
        题目 Id获取题目(int 题目Id);
        int 新增题目(题目 新题目);
        bool 更新题目(题目 修改题目);
        bool 删除题目(int 题目Id);
        List<题目> 按标签查询(IEnumerable<标签> 标签列表);

    }
}
