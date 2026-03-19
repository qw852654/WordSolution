using System.Collections.Generic;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 获取标签种类列表用例
    {
        private readonly I标签种类仓储 _标签种类仓储;

        public 获取标签种类列表用例(I标签种类仓储 标签种类仓储)
        {
            _标签种类仓储 = 标签种类仓储;
        }

        public IReadOnlyList<标签种类> 执行()
        {
            return _标签种类仓储.获取全部标签种类();
        }
    }
}
