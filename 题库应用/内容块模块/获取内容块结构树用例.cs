using System.Collections.Generic;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 获取内容块结构树用例
    {
        private const int 最大嵌套深度 = 10;

        private readonly I内容块仓储 _内容块仓储;
        private readonly 内容块元数据选项帮助类 _内容块元数据选项帮助类;

        public 获取内容块结构树用例(
            I内容块仓储 内容块仓储,
            内容块元数据选项帮助类 内容块元数据选项帮助类)
        {
            _内容块仓储 = 内容块仓储;
            _内容块元数据选项帮助类 = 内容块元数据选项帮助类;
        }

        public 内容块结构树结果? 执行(int 内容块ID)
        {
            var 内容块 = _内容块仓储.GetById(内容块ID);
            if (内容块 == null)
            {
                return null;
            }

            return 构建节点(内容块, null, 1, new HashSet<int>());
        }

        private 内容块结构树结果 构建节点(内容块 内容块, 内容块子项结果? 来源子项, int 深度, ISet<int> 已访问)
        {
            var 当前版本 = _内容块仓储.获取当前版本(内容块.Id);
            var 节点 = new 内容块结构树结果
            {
                内容块 = 内容块详情结果.从内容块(内容块, 当前版本, _内容块元数据选项帮助类.获取内容块选项字典(内容块)),
                来源子项 = 来源子项,
                深度 = 深度,
                已达到最大深度 = 深度 >= 最大嵌套深度,
            };

            if (!内容块.是否允许子块 || 深度 >= 最大嵌套深度 || !已访问.Add(内容块.Id))
            {
                return 节点;
            }

            foreach (var 子项 in _内容块仓储.获取子项列表(内容块.Id))
            {
                var 子内容块 = _内容块仓储.GetById(子项.子内容块ID);
                if (子内容块 == null)
                {
                    continue;
                }

                var 引用版本 = 子项.引用版本模式 == 内容块引用版本模式.锁定版本 && 子项.子内容块版本ID.HasValue
                    ? _内容块仓储.获取版本(子项.子内容块版本ID.Value)
                    : _内容块仓储.获取当前版本(子内容块.Id);
                var 来源 = 内容块子项结果.从子项(子项, 子内容块, 引用版本);
                节点.子块列表.Add(构建节点(子内容块, 来源, 深度 + 1, 已访问));
            }

            已访问.Remove(内容块.Id);
            return 节点;
        }
    }
}
