using System;
using System.Linq;
using 题库核心.标签模块.契约;

namespace 题库应用.标签模块
{
    public class 调整标签排序用例
    {
        private readonly I标签仓储 _标签仓储;

        public 调整标签排序用例(I标签仓储 标签仓储)
        {
            _标签仓储 = 标签仓储;
        }

        public void 执行(int 标签ID, 调整标签排序的请求 请求)
        {
            var 标签 = _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException("标签不存在。");
            var 同级标签列表 = _标签仓储.根据种类获取标签(标签.标签种类ID)
                .Where(当前标签 => 当前标签.ParentId == 标签.ParentId)
                .OrderBy(当前标签 => 当前标签.同级排序值)
                .ToList();

            var 当前索引 = 同级标签列表.FindIndex(当前标签 => 当前标签.Id == 标签ID);
            if (当前索引 < 0)
            {
                return;
            }

            var 目标索引 = 请求.方向 == "上移" ? 当前索引 - 1 : 当前索引 + 1;
            if (目标索引 < 0 || 目标索引 >= 同级标签列表.Count)
            {
                return;
            }

            var 目标标签 = 同级标签列表[目标索引];
            var 原排序值 = 标签.同级排序值;

            标签.更新标签(标签.标签种类ID, 标签.名称, 标签.Description, 标签.ParentId, 目标标签.同级排序值, 标签.NumericValue, 标签.IsEnabled);
            目标标签.更新标签(目标标签.标签种类ID, 目标标签.名称, 目标标签.Description, 目标标签.ParentId, 原排序值, 目标标签.NumericValue, 目标标签.IsEnabled);

            _标签仓储.保存标签(标签);
            _标签仓储.保存标签(目标标签);
        }
    }
}
