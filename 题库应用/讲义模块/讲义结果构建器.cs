using System.Linq;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;
using 题库核心.小节模块.契约;

namespace 题库应用.讲义模块
{
    public class 讲义结果构建器
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly I小节仓储 _小节仓储;
        private readonly I内容块仓储 _内容块仓储;

        public 讲义结果构建器(
            I讲义仓储 讲义仓储,
            I小节仓储 小节仓储,
            I内容块仓储 内容块仓储)
        {
            _讲义仓储 = 讲义仓储;
            _小节仓储 = 小节仓储;
            _内容块仓储 = 内容块仓储;
        }

        public 讲义详情结果 构建详情(讲义 讲义)
        {
            var 项目数量 = _讲义仓储.获取讲义项列表(讲义.Id).Count;
            var 最新生成记录 = _讲义仓储.获取生成记录列表(讲义.Id).FirstOrDefault();
            return 讲义详情结果.从讲义(讲义, 项目数量, 最新生成记录);
        }

        public 讲义项结果 构建讲义项(讲义项 讲义项)
        {
            if (讲义项.目标类型 == 讲义项目标类型.小节)
            {
                var 小节 = _小节仓储.GetById(讲义项.目标ID);
                return 讲义项结果.从讲义项(讲义项, 小节?.标题 ?? "已删除小节", 小节?.摘要, null);
            }

            var 内容块 = _内容块仓储.GetById(讲义项.目标ID);
            var 引用版本 = 内容块 == null ? null : 获取引用版本(讲义项, 内容块.Id);
            return 讲义项结果.从讲义项(讲义项, 内容块?.标题 ?? "已删除内容块", 内容块?.摘要, 引用版本);
        }

        private 内容块版本? 获取引用版本(讲义项 讲义项, int 内容块ID)
        {
            if (讲义项.引用版本模式 == 内容块引用版本模式.锁定版本 && 讲义项.锁定内容块版本ID.HasValue)
            {
                return _内容块仓储.获取版本(讲义项.锁定内容块版本ID.Value);
            }

            return _内容块仓储.获取当前版本(内容块ID);
        }
    }
}
