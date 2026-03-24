using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using 题库应用.试卷导入模块;

namespace 题库本地服务.试卷导入模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/试卷导入")]
    public class 试卷导入控制器 : ControllerBase
    {
        private readonly 列出试卷记录用例 _列出试卷记录用例;
        private readonly 开始导入试卷用例 _开始导入试卷用例;
        private readonly 获取当前导入题目用例 _获取当前导入题目用例;
        private readonly 确认导入题目用例 _确认导入题目用例;
        private readonly 跳过导入题目用例 _跳过导入题目用例;

        public 试卷导入控制器(
            列出试卷记录用例 列出试卷记录用例,
            开始导入试卷用例 开始导入试卷用例,
            获取当前导入题目用例 获取当前导入题目用例,
            确认导入题目用例 确认导入题目用例,
            跳过导入题目用例 跳过导入题目用例)
        {
            _列出试卷记录用例 = 列出试卷记录用例;
            _开始导入试卷用例 = 开始导入试卷用例;
            _获取当前导入题目用例 = 获取当前导入题目用例;
            _确认导入题目用例 = 确认导入题目用例;
            _跳过导入题目用例 = 跳过导入题目用例;
        }

        [HttpGet("试卷列表")]
        public ActionResult<IReadOnlyList<试卷记录列表项结果>> 获取试卷列表()
        {
            return Ok(_列出试卷记录用例.执行());
        }

        [HttpPost("开始")]
        [RequestSizeLimit(50_000_000)]
        public ActionResult<开始导入试卷结果> 开始([FromForm] 开始导入试卷的请求 请求)
        {
            if (请求.File == null || 请求.File.Length == 0)
            {
                return BadRequest("请选择要导入的 docx 文件。");
            }

            using var 内存流 = new MemoryStream();
            请求.File.CopyTo(内存流);

            var 结果 = _开始导入试卷用例.执行(
                RouteData.Values["题库键"]?.ToString() ?? string.Empty,
                请求.File.FileName,
                内存流.ToArray(),
                请求.年份标签ID,
                请求.来源标签ID);

            return Ok(结果);
        }

        [HttpGet("{paperId}/当前题")]
        public ActionResult<当前导入题目结果> 获取当前题(int paperId)
        {
            var 结果 = _获取当前导入题目用例.执行(paperId);
            if (结果 == null)
            {
                return NoContent();
            }

            return Ok(结果);
        }

        [HttpPost("{paperId}/确认")]
        public ActionResult<当前导入题目结果> 确认并下一题(int paperId, [FromBody] 确认导入题目的请求 请求)
        {
            var 结果 = _确认导入题目用例.执行(
                paperId,
                请求.试卷题目项ID,
                请求.题型ID,
                请求.难度标签ID,
                请求.最终标签ID列表,
                请求.新建知识点映射列表?.Select(映射 => new 知识点映射决策
                {
                    原始知识点文本 = 映射.原始知识点文本,
                    目标标签ID = 映射.目标标签ID,
                    是否抛弃 = 映射.是否抛弃,
                }).ToList() ?? new List<知识点映射决策>());

            if (结果 == null)
            {
                return NoContent();
            }

            return Ok(结果);
        }

        [HttpPost("{paperId}/跳过")]
        public ActionResult<当前导入题目结果> 跳过(int paperId, [FromBody] 跳过导入题目的请求 请求)
        {
            var 结果 = _跳过导入题目用例.执行(paperId, 请求.试卷题目项ID);
            if (结果 == null)
            {
                return NoContent();
            }

            return Ok(结果);
        }
    }
}
