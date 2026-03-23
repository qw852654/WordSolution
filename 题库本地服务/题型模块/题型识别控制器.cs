using Microsoft.AspNetCore.Mvc;
using 题库应用.题目模块;

namespace 题库本地服务.题型模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/题型识别")]
    public class 题型识别控制器 : ControllerBase
    {
        private readonly 获取下一道待识别题型题目用例 _获取下一道待识别题型题目用例;
        private readonly 更新题目题型用例 _更新题目题型用例;

        public 题型识别控制器(
            获取下一道待识别题型题目用例 获取下一道待识别题型题目用例,
            更新题目题型用例 更新题目题型用例)
        {
            _获取下一道待识别题型题目用例 = 获取下一道待识别题型题目用例;
            _更新题目题型用例 = 更新题目题型用例;
        }

        [HttpGet("下一题")]
        public IActionResult 获取下一题()
        {
            var 结果 = _获取下一道待识别题型题目用例.执行();
            if (结果 == null)
            {
                return NoContent();
            }

            return Ok(结果);
        }

        [HttpPost("确认")]
        public IActionResult 确认并获取下一题([FromBody] 题型识别确认请求 请求)
        {
            var 已更新 = _更新题目题型用例.执行(请求.题目ID, 请求.题型ID);
            if (!已更新)
            {
                return NotFound();
            }

            var 下一题 = _获取下一道待识别题型题目用例.执行();
            if (下一题 == null)
            {
                return NoContent();
            }

            return Ok(下一题);
        }
    }
}
