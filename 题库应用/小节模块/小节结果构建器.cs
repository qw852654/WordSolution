using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.小节模块.契约;
using 题库核心.小节模块.领域;
using 题库核心.标签模块.契约;

namespace 题库应用.小节模块
{
    public class 小节结果构建器
    {
        private readonly I小节仓储 _小节仓储;
        private readonly I内容块仓储 _内容块仓储;
        private readonly I标签仓储 _标签仓储;

        public 小节结果构建器(
            I小节仓储 小节仓储,
            I内容块仓储 内容块仓储,
            I标签仓储 标签仓储)
        {
            _小节仓储 = 小节仓储;
            _内容块仓储 = 内容块仓储;
            _标签仓储 = 标签仓储;
        }

        public 小节详情结果 构建详情(小节 小节)
        {
            var 章节标签 = 小节.章节标签ID.HasValue ? _标签仓储.GetById(小节.章节标签ID.Value) : null;
            return 小节详情结果.从小节(小节, 章节标签, 构建统计(小节.Id));
        }

        public 小节项结果 构建小节项(小节项 小节项)
        {
            var 内容块 = _内容块仓储.GetById(小节项.内容块ID)!;
            var 引用版本 = 获取引用版本(小节项);
            return 小节项结果.从小节项(小节项, 内容块, 引用版本);
        }

        private 小节项目统计 构建统计(int 小节ID)
        {
            var 项列表 = _小节仓储.获取小节项列表(小节ID);
            var 内容块列表 = 项列表
                .Select(项 => _内容块仓储.GetById(项.内容块ID))
                .Where(内容块 => 内容块 != null)
                .Cast<内容块>()
                .ToList();

            return new 小节项目统计
            {
                项目数量 = 项列表.Count,
                知识点数量 = 内容块列表.Count(内容块 => 内容块.类型 == 内容块类型.知识点),
                例题数量 = 内容块列表.Count(内容块 => 内容块.类型 == 内容块类型.例题 || 内容块.类型 == 内容块类型.题目),
                练习数量 = 内容块列表.Count(内容块 => 内容块.类型 == 内容块类型.练习 || 内容块.类型 == 内容块类型.练习组),
            };
        }

        private 内容块版本? 获取引用版本(小节项 小节项)
        {
            if (小节项.引用版本模式 == 内容块引用版本模式.锁定版本 && 小节项.内容块版本ID.HasValue)
            {
                return _内容块仓储.获取版本(小节项.内容块版本ID.Value);
            }

            return _内容块仓储.获取当前版本(小节项.内容块ID);
        }
    }
}
