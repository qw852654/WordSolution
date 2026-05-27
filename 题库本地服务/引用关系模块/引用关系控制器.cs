using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.引用关系模块;

namespace 题库本地服务.引用关系模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/引用关系")]
    public class 引用关系控制器 : ControllerBase
    {
        private readonly 获取旧版本引用列表用例 _获取旧版本引用列表用例;

        public 引用关系控制器(获取旧版本引用列表用例 获取旧版本引用列表用例)
        {
            _获取旧版本引用列表用例 = 获取旧版本引用列表用例;
        }

        [HttpGet("旧版本引用")]
        public ActionResult<IReadOnlyList<旧版本引用结果>> 获取旧版本引用列表()
        {
            return Ok(_获取旧版本引用列表用例.执行());
        }
    }
}
