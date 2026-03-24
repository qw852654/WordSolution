using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.试卷导入模块.契约;

namespace 题库应用.试卷导入模块
{
    public class 列出试卷记录用例
    {
        private readonly I试卷记录仓储 _试卷记录仓储;
        private readonly I标签仓储 _标签仓储;

        public 列出试卷记录用例(I试卷记录仓储 试卷记录仓储, I标签仓储 标签仓储)
        {
            _试卷记录仓储 = 试卷记录仓储;
            _标签仓储 = 标签仓储;
        }

        public IReadOnlyList<试卷记录列表项结果> 执行()
        {
            return _试卷记录仓储.获取全部()
                .Select(试卷 =>
                {
                    var 年份标签 = _标签仓储.GetById(试卷.年份标签ID);
                    var 来源标签 = _标签仓储.GetById(试卷.来源标签ID);
                    return new 试卷记录列表项结果
                    {
                        试卷记录ID = 试卷.Id,
                        显示名称 = 试卷.显示名称,
                        年份标签ID = 试卷.年份标签ID,
                        年份标签名称 = 年份标签?.名称 ?? string.Empty,
                        来源标签ID = 试卷.来源标签ID,
                        来源标签名称 = 来源标签?.名称 ?? string.Empty,
                        总题数 = 试卷.总题数,
                        已确认数 = 试卷.已确认数,
                        已跳过数 = 试卷.已跳过数,
                        状态 = 试卷.状态.ToString(),
                    };
                })
                .ToList();
        }
    }
}
