using System;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.小节模块
{
    public class 小节章节规则校验器
    {
        private readonly I标签仓储 _标签仓储;

        public 小节章节规则校验器(I标签仓储 标签仓储)
        {
            _标签仓储 = 标签仓储;
        }

        public 标签? 校验并返回章节标签(int? 章节标签ID)
        {
            if (!章节标签ID.HasValue)
            {
                return null;
            }

            var 标签 = _标签仓储.GetById(章节标签ID.Value);
            if (标签 == null)
            {
                throw new InvalidOperationException("章节标签不存在。");
            }

            if (标签.标签种类ID != 系统标签种类.章节)
            {
                throw new InvalidOperationException("小节只能挂到章节标签下。");
            }

            if (!标签.IsEnabled)
            {
                throw new InvalidOperationException("不能选择已停用的章节标签。");
            }

            return 标签;
        }
    }
}
