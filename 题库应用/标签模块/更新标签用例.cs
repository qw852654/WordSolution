using System;
using System.Linq;
using 题库核心.标签模块.契约;
using 题库核心.标签模块.领域;

namespace 题库应用.标签模块
{
    public class 更新标签用例
    {
        private readonly I标签仓储 _标签仓储;
        private readonly I标签种类仓储 _标签种类仓储;

        public 更新标签用例(I标签仓储 标签仓储, I标签种类仓储 标签种类仓储)
        {
            _标签仓储 = 标签仓储;
            _标签种类仓储 = 标签种类仓储;
        }

        public 标签 执行(int 标签ID, 更新标签的请求 请求)
        {
            var 标签 = _标签仓储.GetById(标签ID) ?? throw new InvalidOperationException("标签不存在。");
            var 目标标签种类 = _标签种类仓储.GetById(请求.标签种类ID);
            标签规则帮助类.校验标签种类存在(目标标签种类, 请求.标签种类ID);

            var 全部标签 = _标签仓储.获取全部标签();
            var 现有子标签列表 = 全部标签.Where(当前标签 => 当前标签.ParentId == 标签ID).ToList();
            var 原标签种类ID = 标签.标签种类ID;
            var 新ParentId = 标签.ParentId;
            var 新排序值 = 标签.同级排序值;

            if (请求.标签种类ID != 原标签种类ID)
            {
                if (现有子标签列表.Count > 0)
                {
                    throw new InvalidOperationException("当前标签存在子标签，不能直接改种类。请先处理子标签后再修改。");
                }

                if (请求.标签种类ID == 系统标签种类.难度)
                {
                    if (标签.ParentId.HasValue)
                    {
                        throw new InvalidOperationException("改到难度时必须无父级。");
                    }

                    新ParentId = null;
                }
                else if (请求.标签种类ID == 系统标签种类.附加标签)
                {
                    新ParentId = null;
                }
                else
                {
                    新ParentId = null;
                }

                新排序值 = _标签仓储.获取同父同种类最大排序值(请求.标签种类ID, 新ParentId) + 1;
            }

            if (_标签仓储.是否存在同父同种类同名标签(请求.标签种类ID, 新ParentId, 请求.名称, 标签ID))
            {
                throw new InvalidOperationException("同父级下已经存在同名标签。");
            }

            标签.更新标签(
                请求.标签种类ID,
                请求.名称,
                请求.Description,
                新ParentId,
                新排序值,
                请求.NumericValue,
                请求.IsEnabled);

            _标签仓储.保存标签(标签);
            return 标签;
        }
    }
}
