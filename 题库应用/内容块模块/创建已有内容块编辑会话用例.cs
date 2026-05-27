using System;
using 题库核心.内容块模块.契约;

namespace 题库应用.内容块模块
{
    public class 创建已有内容块编辑会话用例
    {
        private readonly I内容块仓储 _内容块仓储;
        private readonly 编辑会话创建帮助类 _编辑会话创建帮助类;

        public 创建已有内容块编辑会话用例(
            I内容块仓储 内容块仓储,
            编辑会话创建帮助类 编辑会话创建帮助类)
        {
            _内容块仓储 = 内容块仓储;
            _编辑会话创建帮助类 = 编辑会话创建帮助类;
        }

        public 内容块编辑会话结果? 执行(string 题库键, int 内容块ID, 创建已有内容块编辑会话的请求 请求)
        {
            if (请求 == null)
            {
                throw new ArgumentNullException(nameof(请求));
            }

            var 内容块 = _内容块仓储.GetById(内容块ID);
            if (内容块 == null)
            {
                return null;
            }

            var 当前版本 = _内容块仓储.获取当前版本(内容块ID);
            var 会话 = _编辑会话创建帮助类.创建会话(题库键, 内容块, 当前版本, 请求.是否打开Word ?? true);
            return 内容块编辑会话结果.从会话(会话);
        }
    }
}
