using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagRunner;
using Core.QuestionBank.Domain;

namespace TagRunner.数据
{
    public interface I标签仓储
    {
        int 新增标签(标签 标签对象);
        bool 更新标签(标签 标签对象);
        bool 删除标签(标签 待删除标签);
        标签 Id获取标签(int 标签Id);
        bool 存在同名标签(string 名称, int? 父Id);
        

    }
}
