using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.标签模块;
using 题库核心.标签模块.领域;

namespace 题库本地服务.标签模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/标签种类")]
    public class 标签种类控制器 : ControllerBase
    {
        private readonly 获取标签种类列表用例 _获取标签种类列表用例;

        public 标签种类控制器(获取标签种类列表用例 获取标签种类列表用例)
        {
            _获取标签种类列表用例 = 获取标签种类列表用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<标签种类>> 获取标签种类列表()
        {
            return Ok(_获取标签种类列表用例.执行());
        }
    }
}
