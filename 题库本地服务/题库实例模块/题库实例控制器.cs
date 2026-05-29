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

        [HttpPost]
        public ActionResult<题库实例信息> 创建题库实例([FromBody] 创建题库实例请求 请求)
        {
            try
            {
                var 题库实例 = _题库实例服务.创建题库实例(请求.题库键, 请求.显示名称);
                return CreatedAtAction(nameof(获取题库实例列表), 题库实例);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }

    public class 创建题库实例请求
    {
        public string 题库键 { get; set; } = string.Empty;

        public string? 显示名称 { get; set; }
    }
}
