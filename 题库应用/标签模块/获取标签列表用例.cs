using System.Collections.Generic;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 获取标签列表用例
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 获取标签列表用例(I标签仓储 标签仓储, I标签种类仓储 标签种类仓储)
        {
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public IReadOnlyList<标签> 执行(int 标签种类ID)
        {
            var 标签种类 = _标签种类仓储.GetById(标签种类ID);
            if (标签种类 == null)
            {
                return new List<标签>();
            }

            var 标签列表 = _标签仓储.根据种类获取标签(标签种类ID).ToList();
            if (!标签种类.是否树形)
            {
                return 标签列表.OrderBy(标签 => 标签.同级排序值).ToList();
            }

            var 标签字典 = 标签列表.ToDictionary(标签 => 标签.Id);
            var 根标签列表 = new List<标签>();

            foreach (var 标签 in 标签列表)
            {
                标签.清空子标签();
            }

            foreach (var 标签 in 标签列表.OrderBy(标签 => 标签.同级排序值))
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
