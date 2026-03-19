using System;
using System.Linq;
using 题库核心.标签模块.契约;

namespace 题库应用.标签模块
{
    public class 删除标签用例
    {
        private readonly I标签仓储 _标签仓储;

        public 删除标签用例(I标签仓储 标签仓储)
        {
            _标签仓储 = 标签仓储;
        }

        public void 执行(int 标签ID)
        {
            var 标签 = _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException("标签不存在。");

            if (_标签仓储.获取全部标签().Any(当前标签 => 当前标签.ParentId == 标签ID))
            {
                throw new InvalidOperationException("当前标签存在子标签，不能删除。");
            }

            if (_标签仓储.是否被题目引用(标签ID))
            {
                throw new InvalidOperationException("当前标签已被题目引用，不能删除。");
            }

            _标签仓储.删除标签(标签);
        }
    }
}
