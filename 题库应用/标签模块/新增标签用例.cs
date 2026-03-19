using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 新增标签用例
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 新增标签用例(I标签仓储 标签仓储, I标签种类仓储 标签种类仓储)
        {
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public 标签 执行(新增标签的请求 请求)
        {
            var 标签种类 = _标签种类仓储.GetById(请求.标签种类ID);
            标签规则帮助类.校验标签种类存在(标签种类, 请求.标签种类ID);

            var 父标签 = 请求.ParentId.HasValue ? _标签仓储.GetById(请求.ParentId.Value) : null;
            标签规则帮助类.校验新增或更新时的父级合法性(标签种类!, 父标签, 请求.ParentId);

            if (_标签仓储.是否存在同父同种类同名标签(请求.标签种类ID, 请求.ParentId, 请求.名称))
            {
                throw new InvalidOperationException("同父级下已经存在同名标签。");
            }

            var 当前最大排序值 = _标签仓储.获取同父同种类最大排序值(请求.标签种类ID, 请求.ParentId);

            var 新标签 = 标签.创建标签(
                标签种类ID: 请求.标签种类ID,
                名称: 请求.名称,
                description: 请求.Description,
                parentId: 请求.ParentId,
                同级排序值: 当前最大排序值 + 1,
                numericValue: 请求.NumericValue,
                isEnabled: 请求.IsEnabled);

            _标签仓储.增加标签(新标签);
            return 新标签;
        }
    }
}
