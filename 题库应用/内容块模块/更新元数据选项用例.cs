using System;
using System.Linq;
using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 更新元数据选项用例
    {
        private readonly I元数据选项仓储 _元数据选项仓储;

        public 更新元数据选项用例(I元数据选项仓储 元数据选项仓储)
        {
            _元数据选项仓储 = 元数据选项仓储;
        }

        public 元数据选项结果? 执行(int id, 更新元数据选项的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 选项 = _元数据选项仓储.GetById(id);
            if (选项 == null)
            {
                return null;
            }

            var 选项名称 = 请求.Name?.Trim() ?? string.Empty;
            if (_元数据选项仓储
                .获取选项列表(选项.Category)
                .Any(同类选项 => 同类选项.Id != id && string.Equals(同类选项.Name, 选项名称, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("同一字段类别下已存在同名选项。");
            }

            选项.修改(选项名称, 请求.SortOrder ?? 选项.SortOrder);
            _元数据选项仓储.保存选项(选项);
            return 元数据选项结果.从选项(选项);
        }
    }
}
