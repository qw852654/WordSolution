using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.标签模块.契约;

namespace 题库应用.内容块模块
{
    public class 更新内容块标签列表用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly I内容块标签仓储 _内容块标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;
        private readonly 内容块标签规则校验器 _内容块标签规则校验器;

        public 更新内容块标签列表用例(
            I内容块仓储 内容块仓储,
            I内容块标签仓储 内容块标签仓储,
            I标签种类仓储 标签种类仓储,
            内容块标签规则校验器 内容块标签规则校验器)
        {
            _内容块仓储 = 内容块仓储;
            _内容块标签仓储 = 内容块标签仓储;
            _标签种类仓储 = 标签种类仓储;
            _内容块标签规则校验器 = 内容块标签规则校验器;
        }

        public IReadOnlyList<内容块标签结果>? 执行(int 内容块ID, 更新内容块标签的请求 请求)
        {
            if (_内容块仓储.GetById(内容块ID) == null)
            {
                return null;
            }

            var 标签列表 = _内容块标签规则校验器.校验并返回标签(请求.标签ID列表 ?? new List<int>());
            _内容块标签仓储.保存内容块标签ID列表(内容块ID, 标签列表.Select(标签 => 标签.Id).ToList());

            var 标签种类字典 = _标签种类仓储.获取全部标签种类().ToDictionary(标签种类 => 标签种类.Id);
            return 标签列表
                .Select(标签 => 内容块标签结果.从标签(标签, 标签种类字典.GetValueOrDefault(标签.标签种类ID)))
                .ToList();
        }
    }
}
