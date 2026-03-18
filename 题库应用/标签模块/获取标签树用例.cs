using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 获取标签树用例
    {
        private readonly I标签仓储 _标签仓储;

        public 获取标签树用例(I标签仓储 标签仓储)
        {
            _标签仓储 = 标签仓储;
        }

        public IReadOnlyList<标签> 执行()
        {
            var 全部标签 = _标签仓储.获取全部标签().ToList();
            var 标签字典 = 全部标签.ToDictionary(标签 => 标签.Id);
            var 根标签列表 = new List<标签>();

            foreach (var 标签 in 全部标签)
            {
                标签.清空子标签();
            }

            foreach (var 标签 in 全部标签.OrderBy(标签 => 标签.同级排序值))
            {
                if (标签.ParentId.HasValue && 标签字典.TryGetValue(标签.ParentId.Value, out var 父标签))
                {
                    父标签.添加子标签(标签);
                }
                else
                {
                    根标签列表.Add(标签);
                }
            }

            return 根标签列表;
        }
    }
}
