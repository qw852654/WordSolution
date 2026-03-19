using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.标签模块;
using 题库核心.标签模块.领域;

namespace 题库本地服务.标签模块
{
    [ApiController]
    [Route("api/标签")]
    public class 标签控制器 : ControllerBase
    {
        private readonly 获取标签树用例 _获取标签树用例;
        private readonly 新增标签用例 _新增标签用例;

        public 标签控制器(获取标签树用例 获取标签树用例, 新增标签用例 新增标签用例)
        {
            _获取标签树用例 = 获取标签树用例;
            _新增标签用例 = 新增标签用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<标签>> 获取标签树()
        {
            var 标签树 = _获取标签树用例.执行();
            return Ok(标签树);
        }

        [HttpPost]
        public IActionResult 新增标签([FromBody] 新增标签的请求 请求)
        {
            _新增标签用例.执行(请求);
            return Ok();
        }
    }
}
