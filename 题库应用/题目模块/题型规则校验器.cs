using System;
using 题库核心.题目模块.契约;

namespace 题库应用.题目模块
{
    public class 题型规则校验器
    {
        private readonly I题型定义仓储 _题型定义仓储;

        public 题型规则校验器(I题型定义仓储 题型定义仓储)
        {
            _题型定义仓储 = 题型定义仓储;
        }

        public void 校验必填(int? 题型ID)
        {
            if (!题型ID.HasValue)
            {
                throw new InvalidOperationException("题型不能为空。");
            }

            校验存在(题型ID.Value);
        }

        public void 校验存在(int 题型ID)
        {
            if (_题型定义仓储.根据ID获取(题型ID) == null)
            {
                throw new InvalidOperationException("题型不存在。");
            }
        }
    }
}
