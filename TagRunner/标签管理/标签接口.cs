using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagRunner.标签管理
{
    public interface I标签查询服务
    {
        IEnumerable<标签> 遍历标签树获取标签();
    }
}
