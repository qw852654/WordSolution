using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 设置元数据选项启用状态用例
    {
        private readonly I元数据选项仓储 _元数据选项仓储;

        public 设置元数据选项启用状态用例(I元数据选项仓储 元数据选项仓储)
        {
            _元数据选项仓储 = 元数据选项仓储;
        }

        public 元数据选项结果? 执行(int id, bool isActive)
        {
            var 选项 = _元数据选项仓储.GetById(id);
            if (选项 == null)
            {
                return null;
            }

            if (isActive)
            {
                选项.启用();
            }
            else
            {
                选项.停用();
            }

            _元数据选项仓储.保存选项(选项);
            return 元数据选项结果.从选项(选项);
        }
    }
}
