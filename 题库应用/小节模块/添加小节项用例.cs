using System;
using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.小节模块.契约;
using 题库核心.小节模块.领域;

namespace 题库应用.小节模块
{
    public class 添加小节项用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly I内容块仓储 _内容块仓储;
        private readonly 小节结果构建器 _小节结果构建器;

        public 添加小节项用例(
            I小节仓储 小节仓储,
            I内容块仓储 内容块仓储,
            小节结果构建器 小节结果构建器)
        {
            _小节仓储 = 小节仓储;
            _内容块仓储 = 内容块仓储;
            _小节结果构建器 = 小节结果构建器;
        }

        public 小节项结果? 执行(int 小节ID, 添加小节项的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 小节 = _小节仓储.GetById(小节ID);
            var 内容块 = _内容块仓储.GetById(请求.内容块ID);
            if (小节 == null || 内容块 == null)
            {
                return null;
            }

            var 现有项目 = _小节仓储.获取小节项列表(小节ID);
            if (现有项目.Any(项目 => 项目.内容块ID == 请求.内容块ID))
            {
                throw new InvalidOperationException("当前小节已经包含这个内容块。");
            }

            var 引用版本模式 = 请求.引用版本模式 ?? 内容块引用版本模式.跟随最新;
            var 引用版本 = 校验并获取引用版本(内容块, 引用版本模式, 请求.内容块版本ID);
            var 排序 = 请求.排序 ?? (现有项目.Count == 0 ? 0 : 现有项目.Max(项目 => 项目.排序) + 1);
            var 新小节项 = 小节项.创建(
                小节ID,
                内容块.Id,
                引用版本?.Id,
                引用版本模式,
                string.IsNullOrWhiteSpace(请求.角色) ? 内容块.类型.ToString() : 请求.角色,
                排序);

            _小节仓储.增加小节项(小节, 新小节项);
            return _小节结果构建器.构建小节项(新小节项);
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
