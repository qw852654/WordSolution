using System.Linq;
using Microsoft.AspNetCore.Mvc;
using 题库核心.题目模块.契约;

namespace 题库本地服务.题型模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/题型")]
    public class 题型控制器 : ControllerBase
    {
        private readonly I题型定义仓储 _题型定义仓储;

        public 题型控制器(I题型定义仓储 题型定义仓储)
        {
            _题型定义仓储 = 题型定义仓储;
        }

        [HttpGet]
        public IActionResult 获取题型列表()
        {
            var 列表 = _题型定义仓储.获取全部()
                .Select(题型 => new
                {
                    id = 题型.Id,
                    名称 = 题型.名称,
                    描述 = 题型.描述,
                    排序值 = 题型.排序值,
                })
                .ToList();
            return Ok(列表);
        }
    }
}
