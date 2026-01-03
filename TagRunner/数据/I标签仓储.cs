using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagRunner;

namespace TagRunner.数据
{
    public interface I标签仓储
    {
        IEnumerable<标签> 读取扁平标签集合();
        int 插入标签(标签 标签对象);
        bool 更新标签(标签 标签对象);
        bool 删除标签(int 标签Id);
        标签 按Id获取标签(int 标签Id);
        bool 是否存在同名(string 名称, int? 父Id, string 类别 = null);
    }
}
