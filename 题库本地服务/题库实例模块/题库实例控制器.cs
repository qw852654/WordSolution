using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库基础设施.题库实例;

namespace 题库本地服务.题库实例模块
{
    [ApiController]
    [Route("api/题库实例")]
    public class 题库实例控制器 : ControllerBase
    {
        private readonly 题库实例服务 _题库实例服务;

        public 题库实例控制器(题库实例服务 题库实例服务)
        {
            _题库实例服务 = 题库实例服务;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<题库实例信息>> 获取题库实例列表()
        {
            return Ok(_题库实例服务.获取题库实例列表());
        }
    }
}
