using 题库核心.小节模块.契约;

namespace 题库应用.小节模块
{
    public class 获取小节详情用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly 小节结果构建器 _小节结果构建器;

        public 获取小节详情用例(I小节仓储 小节仓储, 小节结果构建器 小节结果构建器)
        {
            _小节仓储 = 小节仓储;
            _小节结果构建器 = 小节结果构建器;
        }

        public 小节详情结果? 执行(int id)
        {
            var 小节 = _小节仓储.GetById(id);
            return 小节 == null ? null : _小节结果构建器.构建详情(小节);
        }
    }
}
