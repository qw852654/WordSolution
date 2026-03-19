using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using 题库应用.筛选模块;
using 题库应用.题目模块;
using 题库核心.筛选模块.领域;
using 题库核心.题目模块.领域;

namespace 题库本地服务.题目模块
{
    [ApiController]
    [Route("api/题目")]
    public class 题目控制器 : ControllerBase
    {
        private readonly 录入题目用例 _录入题目用例;
        private readonly 录入Ooxml题目用例 _录入Ooxml题目用例;
        private readonly 根据ID获取题目详情用例 _根据ID获取题目详情用例;
        private readonly 获取题目预览HTML用例 _获取题目预览HTML用例;
        private readonly 根据标签筛选题目用例 _根据标签筛选题目用例;

        public 题目控制器(
            录入题目用例 录入题目用例,
            录入Ooxml题目用例 录入Ooxml题目用例,
            根据ID获取题目详情用例 根据ID获取题目详情用例,
            获取题目预览HTML用例 获取题目预览HTML用例,
            根据标签筛选题目用例 根据标签筛选题目用例)
        {
            _录入题目用例 = 录入题目用例;
            _录入Ooxml题目用例 = 录入Ooxml题目用例;
            _根据ID获取题目详情用例 = 根据ID获取题目详情用例;
            _获取题目预览HTML用例 = 获取题目预览HTML用例;
            _根据标签筛选题目用例 = 根据标签筛选题目用例;
        }

        [HttpPost]
        public ActionResult<题目> 录入题目([FromBody] 录入题目的请求 请求)
        {
            var 新题目 = _录入题目用例.执行(请求);
            return CreatedAtAction(nameof(根据ID获取题目详情), new { id = 新题目.Id }, 新题目);
        }

        [HttpPost("ooxml")]
        public ActionResult<题目> 录入Ooxml题目([FromBody] 录入Ooxml题目的请求 请求)
        {
            var 新题目 = _录入Ooxml题目用例.执行(请求);
            return CreatedAtAction(nameof(根据ID获取题目详情), new { id = 新题目.Id }, 新题目);
        }

        [HttpGet("{id:int}")]
        public ActionResult<题目> 根据ID获取题目详情(int id)
        {
            var 题目 = _根据ID获取题目详情用例.执行(id);
            if (题目 == null)
            {
                return NotFound();
            }

            return Ok(题目);
        }

        [HttpGet("{id:int}/预览html")]
        public ActionResult 获取题目预览HTML(int id)
        {
            var HTML内容 = _获取题目预览HTML用例.执行(id);
            if (string.IsNullOrWhiteSpace(HTML内容))
            {
                return NotFound();
            }

            return Content(HTML内容, "text/html; charset=utf-8");
        }

        [HttpPost("筛选")]
        public ActionResult<IReadOnlyList<题目>> 根据标签筛选题目([FromBody] List<筛选步骤请求>? 请求列表)
        {
            var 筛选步骤列表 = (请求列表 ?? new List<筛选步骤请求>())
                .Select(步骤 => new 筛选步骤(
                    步骤.标签ID列表,
                    步骤.本步标签组合方式,
                    步骤.与前一步结果组合方式))
                .ToList();

            var 结果 = _根据标签筛选题目用例.执行(筛选步骤列表);
            return Ok(结果);
        }
    }
}
