using System.Collections.Generic;
using TagRunner.Models;

namespace TagRunner.业务
{
    /// <summary>
    /// 业务层标签服务：封装标签的业务规则与写流程（校验 + 仓储调用 + 触发索引/题目检查等）。
    /// </summary>
    public interface I标签服务
    {
        int 新增标签(标签 新标签);
        bool 更新标签(标签 修改标签);
        bool 删除标签(int 标签Id);

        List<标签> 上次被选择的标签{ get; }
        List<标签> 获取标签树();

        方法执行结果 保存上次被选择的标签(List<标签> Tags);

        // 联想标签：根据输入文本返回若干匹配标签（教学/UI 使用）
        List<标签> 联想标签(string 输入文本);
        标签 ID找标签(int id);
    }
}
