using System;
using System.Collections.Generic;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 添加内容块子项用例
    {
        private const int 最大嵌套深度 = 10;

        private readonly I内容块仓储 _内容块仓储;

        public 添加内容块子项用例(I内容块仓储 内容块仓储)
        {
            _内容块仓储 = 内容块仓储;
        }

        public 内容块子项结果? 执行(int 父内容块ID, 添加内容块子项的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 父内容块 = _内容块仓储.GetById(父内容块ID);
            var 子内容块 = _内容块仓储.GetById(请求.子内容块ID);
            if (父内容块 == null || 子内容块 == null)
            {
                return null;
            }

            if (!父内容块.是否允许子块)
            {
                throw new InvalidOperationException("当前内容块不允许添加子内容块。");
            }

            var 引用版本模式 = 请求.引用版本模式 ?? 内容块引用版本模式.跟随最新;
            var 引用版本 = 校验并获取引用版本(子内容块, 引用版本模式, 请求.子内容块版本ID);
            校验不会循环嵌套(父内容块ID, 子内容块.Id);
            校验嵌套深度(父内容块ID, 子内容块.Id);

            var 现有子项 = _内容块仓储.获取子项列表(父内容块ID);
            var 排序 = 请求.排序 ?? (现有子项.Count == 0 ? 0 : 现有子项.Max(子项 => 子项.排序) + 1);
            var 子项 = 内容块子项.创建(
                父内容块ID,
                子内容块.Id,
                引用版本?.Id,
                引用版本模式,
                请求.角色,
                排序);

            _内容块仓储.增加子项(子项);

            return 内容块子项结果.从子项(子项, 子内容块, 引用版本模式 == 内容块引用版本模式.跟随最新 ? _内容块仓储.获取当前版本(子内容块.Id) : 引用版本);
        }

        private 内容块版本? 校验并获取引用版本(内容块 子内容块, 内容块引用版本模式 引用版本模式, int? 子内容块版本ID)
        {
            if (引用版本模式 == 内容块引用版本模式.跟随最新)
            {
                return null;
            }

            if (!子内容块版本ID.HasValue)
            {
                throw new ArgumentException("锁定版本模式必须提供子内容块版本ID。", nameof(子内容块版本ID));
            }

            var 引用版本 = _内容块仓储.获取版本(子内容块版本ID.Value);
            if (引用版本 == null || 引用版本.内容块ID != 子内容块.Id)
            {
                throw new InvalidOperationException("子内容块版本不存在，或不属于指定的子内容块。");
            }

            return 引用版本;
        }

        private void 校验不会循环嵌套(int 父内容块ID, int 子内容块ID)
        {
            if (父内容块ID == 子内容块ID || 子树包含目标内容块(子内容块ID, 父内容块ID, new HashSet<int>()))
            {
                throw new InvalidOperationException("添加该子内容块会造成循环嵌套。");
            }
        }

        private bool 子树包含目标内容块(int 起点内容块ID, int 目标内容块ID, ISet<int> 已访问)
        {
            if (!已访问.Add(起点内容块ID))
            {
                return false;
            }

            foreach (var 子项 in _内容块仓储.获取子项列表(起点内容块ID))
            {
                if (子项.子内容块ID == 目标内容块ID)
                {
                    return true;
                }

                if (子树包含目标内容块(子项.子内容块ID, 目标内容块ID, 已访问))
                {
                    return true;
                }
            }

            return false;
        }

        private void 校验嵌套深度(int 父内容块ID, int 子内容块ID)
        {
            var 子树深度 = 计算子树深度(子内容块ID, new HashSet<int>());
            if (子树深度 + 1 > 最大嵌套深度)
            {
                throw new InvalidOperationException($"内容块嵌套深度不能超过 {最大嵌套深度} 层。");
            }

            foreach (var 祖先ID in 获取所有祖先ID(父内容块ID, new HashSet<int>()))
            {
                var 父到祖先距离 = 计算祖先到子孙距离(祖先ID, 父内容块ID, new HashSet<int>());
                if (父到祖先距离 > 0 && 父到祖先距离 + 子树深度 + 1 > 最大嵌套深度)
                {
                    throw new InvalidOperationException($"内容块嵌套深度不能超过 {最大嵌套深度} 层。");
                }
            }
        }

        private int 计算子树深度(int 内容块ID, ISet<int> 已访问)
        {
            if (!已访问.Add(内容块ID))
            {
                return 1;
            }

            var 子项列表 = _内容块仓储.获取子项列表(内容块ID);
            if (子项列表.Count == 0)
            {
                return 1;
            }

            return 1 + 子项列表.Max(子项 => 计算子树深度(子项.子内容块ID, 已访问));
        }

        private IReadOnlyList<int> 获取所有祖先ID(int 内容块ID, ISet<int> 已访问)
        {
            if (!已访问.Add(内容块ID))
            {
                return Array.Empty<int>();
            }

            var 祖先ID列表 = new List<int>();
            foreach (var 父项 in _内容块仓储.获取父项列表(内容块ID))
            {
                祖先ID列表.Add(父项.父内容块ID);
                祖先ID列表.AddRange(获取所有祖先ID(父项.父内容块ID, 已访问));
            }

            return 祖先ID列表;
        }

        private int 计算祖先到子孙距离(int 起点内容块ID, int 目标内容块ID, ISet<int> 已访问)
        {
            if (起点内容块ID == 目标内容块ID)
            {
                return 0;
            }

            if (!已访问.Add(起点内容块ID))
            {
                return -1;
            }

            foreach (var 子项 in _内容块仓储.获取子项列表(起点内容块ID))
            {
                var 子距离 = 计算祖先到子孙距离(子项.子内容块ID, 目标内容块ID, 已访问);
                if (子距离 >= 0)
                {
                    return 子距离 + 1;
                }
            }

            return -1;
        }
    }
}
