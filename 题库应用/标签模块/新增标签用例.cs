using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 新增标签用例
    {
        private readonly I标签仓储 _标签仓储;

        public 新增标签用例(I标签仓储 标签仓储)
        {
            _标签仓储 = 标签仓储;
        }

        public void 执行(新增标签的请求 请求)
        {
            var 当前最大排序值 = _标签仓储.获取全部标签()
                .Select(标签 => 标签.同级排序值)
                .DefaultIfEmpty(0)
                .Max();

            var 新标签 = 标签.创建标签(
                大类ID: 1,
                名称: 请求.名称,
                description: 请求.Description,
                parentId: null,
                同级排序值: 当前最大排序值 + 1,
                numericValue: null,
                isEnabled: true);

            _标签仓储.增加标签(新标签);
        }
    }
}
