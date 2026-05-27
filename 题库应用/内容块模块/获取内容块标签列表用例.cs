using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.标签模块.契约;

namespace 题库应用.内容块模块
{
    public class 获取内容块标签列表用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly I内容块标签仓储 _内容块标签仓储;
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 获取内容块标签列表用例(
            I内容块仓储 内容块仓储,
            I内容块标签仓储 内容块标签仓储,
            I标签仓储 标签仓储,
            I标签种类仓储 标签种类仓储)
        {
            _内容块仓储 = 内容块仓储;
            _内容块标签仓储 = 内容块标签仓储;
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public IReadOnlyList<内容块标签结果>? 执行(int 内容块ID)
        {
            if (_内容块仓储.GetById(内容块ID) == null)
            {
                return null;
            }

            var 标签种类字典 = _标签种类仓储.获取全部标签种类().ToDictionary(标签种类 => 标签种类.Id);
            return _内容块标签仓储
                .获取内容块标签ID列表(内容块ID)
                .Select(标签ID => _标签仓储.GetById(标签ID))
                .Where(标签 => 标签 != null)
                .Select(标签 => 内容块标签结果.从标签(标签!, 标签种类字典.GetValueOrDefault(标签!.标签种类ID)))
                .ToList();
        }
    }
}
