using System;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 调整标签父级用例
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 调整标签父级用例(I标签仓储 标签仓储, I标签种类仓储 标签种类仓储)
        {
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public 标签 执行(int 标签ID, 调整标签父级的请求 请求)
        {
            var 标签 = _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException("标签不存在。");
            var 标签种类 = _标签种类仓储.GetById(标签.标签种类ID) ?? throw new InvalidOperationException("标签种类不存在。");

            if (!标签种类.是否树形)
            {
                throw new InvalidOperationException($"{标签种类.名称} 不允许调整父级。");
            }

            var 新父标签 = 请求.新父标签ID.HasValue ? _标签仓储.GetById(请求.新父标签ID.Value) : null;
            标签规则帮助类.校验新增或更新时的父级合法性(标签种类, 新父标签, 请求.新父标签ID);

            var 全部标签 = _标签仓储.获取全部标签().ToList();
            标签规则帮助类.校验不会成环(全部标签, 标签ID, 请求.新父标签ID);

            if (新父标签 != null && 新父标签.标签种类ID != 标签.标签种类ID)
            {
                throw new InvalidOperationException("父标签和当前标签必须属于同一种类。");
            }

            标签.更新标签(
                标签.标签种类ID,
                标签.名称,
                标签.Description,
                请求.新父标签ID,
                _标签仓储.获取同父同种类最大排序值(标签.标签种类ID, 请求.新父标签ID) + 1,
                标签.NumericValue,
                标签.IsEnabled);

            _标签仓储.保存标签(标签);
            return 标签;
        }
    }
}
