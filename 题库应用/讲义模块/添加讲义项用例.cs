using System;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;
using 题库核心.小节模块.契约;

namespace 题库应用.讲义模块
{
    public class 添加讲义项用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly I小节仓储 _小节仓储;
        private readonly I内容块仓储 _内容块仓储;
        private readonly 讲义结果构建器 _讲义结果构建器;

        public 添加讲义项用例(
            I讲义仓储 讲义仓储,
            I小节仓储 小节仓储,
            I内容块仓储 内容块仓储,
            讲义结果构建器 讲义结果构建器)
        {
            _讲义仓储 = 讲义仓储;
            _小节仓储 = 小节仓储;
            _内容块仓储 = 内容块仓储;
            _讲义结果构建器 = 讲义结果构建器;
        }

        public 讲义项结果? 执行(int 讲义ID, 添加讲义项的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 讲义 = _讲义仓储.GetById(讲义ID);
            if (讲义 == null)
            {
                return null;
            }

            var 现有项目 = _讲义仓储.获取讲义项列表(讲义ID);
            if (现有项目.Any(项目 => 项目.目标类型 == 请求.目标类型 && 项目.目标ID == 请求.目标ID))
            {
                throw new InvalidOperationException("当前讲义已经包含这个项目。");
            }

            var 引用版本模式 = 内容块引用版本模式.跟随最新;
            int? 锁定版本ID = null;
            var 默认角色 = "小节";

            if (请求.目标类型 == 讲义项目标类型.小节)
            {
                var 小节 = _小节仓储.GetById(请求.目标ID);
                if (小节 == null)
                {
                    return null;
                }
            }
            else if (请求.目标类型 == 讲义项目标类型.内容块)
            {
                var 内容块 = _内容块仓储.GetById(请求.目标ID);
                if (内容块 == null)
                {
                    return null;
                }

                默认角色 = 内容块.类型.ToString();
                引用版本模式 = 请求.引用版本模式 ?? 内容块引用版本模式.跟随最新;
                var 引用版本 = 校验并获取引用版本(内容块, 引用版本模式, 请求.锁定内容块版本ID);
                锁定版本ID = 引用版本?.Id;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(请求.目标类型));
            }

            var 排序 = 请求.排序 ?? (现有项目.Count == 0 ? 0 : 现有项目.Max(项目 => 项目.排序) + 1);
            var 新讲义项 = 讲义项.创建(
                讲义ID,
                请求.目标类型,
                请求.目标ID,
                引用版本模式,
                锁定版本ID,
                string.IsNullOrWhiteSpace(请求.角色) ? 默认角色 : 请求.角色,
                排序);

            _讲义仓储.增加讲义项(讲义, 新讲义项);
            return _讲义结果构建器.构建讲义项(新讲义项);
        }

        private 内容块版本? 校验并获取引用版本(内容块 内容块, 内容块引用版本模式 引用版本模式, int? 内容块版本ID)
        {
            if (引用版本模式 == 内容块引用版本模式.跟随最新)
            {
                return null;
            }

            if (!内容块版本ID.HasValue)
            {
                throw new ArgumentException("锁定版本模式必须提供内容块版本ID。", nameof(内容块版本ID));
            }

            var 引用版本 = _内容块仓储.获取版本(内容块版本ID.Value);
            if (引用版本 == null || 引用版本.内容块ID != 内容块.Id)
            {
                throw new InvalidOperationException("内容块版本不存在，或不属于指定内容块。");
            }

            return 引用版本;
        }
    }
}
