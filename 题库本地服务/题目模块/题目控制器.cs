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
    [Route("api/题库实例/{题库键}/题目")]
    public class 题目控制器 : ControllerBase
    {
        private readonly 录入题目用例 _录入题目用例;
        private readonly 录入Ooxml题目用例 _录入Ooxml题目用例;
        private readonly 根据ID获取题目详情用例 _根据ID获取题目详情用例;
        private readonly 获取题目文件Base64用例 _获取题目文件Base64用例;
        private readonly 获取题目预览HTML用例 _获取题目预览HTML用例;
        private readonly 根据标签筛选题目用例 _根据标签筛选题目用例;
        private readonly 更新Ooxml题目用例 _更新Ooxml题目用例;
        private readonly 删除题目用例 _删除题目用例;
        private readonly 更新题目题型用例 _更新题目题型用例;

        public 题目控制器(
            录入题目用例 录入题目用例,
            录入Ooxml题目用例 录入Ooxml题目用例,
            根据ID获取题目详情用例 根据ID获取题目详情用例,
            获取题目文件Base64用例 获取题目文件Base64用例,
            获取题目预览HTML用例 获取题目预览HTML用例,
            根据标签筛选题目用例 根据标签筛选题目用例,
            更新Ooxml题目用例 更新Ooxml题目用例,
            删除题目用例 删除题目用例,
            更新题目题型用例 更新题目题型用例)
        {
            _录入题目用例 = 录入题目用例;
            _录入Ooxml题目用例 = 录入Ooxml题目用例;
            _根据ID获取题目详情用例 = 根据ID获取题目详情用例;
            _获取题目文件Base64用例 = 获取题目文件Base64用例;
            _获取题目预览HTML用例 = 获取题目预览HTML用例;
            _根据标签筛选题目用例 = 根据标签筛选题目用例;
            _更新Ooxml题目用例 = 更新Ooxml题目用例;
            _删除题目用例 = 删除题目用例;
            _更新题目题型用例 = 更新题目题型用例;
        }

        [HttpPost]
        public ActionResult<题目> 录入题目([FromBody] 录入题目的请求 请求)
        {
            var 新题目 = _录入题目用例.执行(请求);
            return CreatedAtAction(nameof(根据ID获取题目详情), new { 题库键 = RouteData.Values["题库键"], id = 新题目.Id }, 新题目);
        }

        [HttpPost("ooxml")]
        public ActionResult<题目> 录入Ooxml题目([FromBody] 录入Ooxml题目的请求 请求)
        {
            var 新题目 = _录入Ooxml题目用例.执行(请求);
            return CreatedAtAction(nameof(根据ID获取题目详情), new { 题库键 = RouteData.Values["题库键"], id = 新题目.Id }, 新题目);
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

        [HttpGet("{id:int}/文件base64")]
        public ActionResult 获取题目文件Base64(int id)
        {
            var 文件Base64 = _获取题目文件Base64用例.执行(id);
            if (string.IsNullOrWhiteSpace(文件Base64))
            {
                return NotFound();
            }

            return Content(文件Base64, "text/plain; charset=utf-8");
        }

        [HttpPut("{id:int}/ooxml")]
        public IActionResult 更新Ooxml题目(int id, [FromBody] 更新Ooxml题目的请求 请求)
        {
            var 已更新 = _更新Ooxml题目用例.执行(id, 请求);
            if (!已更新)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("{id:int}/题型")]
        public IActionResult 更新题目题型(int id, [FromBody] 更新题目题型的请求 请求)
        {
            var 已更新 = _更新题目题型用例.执行(id, 请求.题型ID);
            if (!已更新)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("筛选")]
        public ActionResult<IReadOnlyList<题目>> 根据标签筛选题目([FromBody] List<筛选步骤请求>? 请求列表)
        {
            var 筛选步骤列表 = (请求列表 ?? new List<筛选步骤请求>())
                .Select(步骤 => new 筛选步骤(
                    步骤.标签ID列表,
                    步骤.题型ID,
                    步骤.仅筛选题型未设置,
                    步骤.本步标签组合方式,
                    步骤.与前一步结果组合方式))
                .ToList();

            var 结果 = _根据标签筛选题目用例.执行(筛选步骤列表);
            return Ok(结果);
        }

        [HttpDelete("{id:int}")]
        public IActionResult 删除题目(int id)
        {
            var 已删除 = _删除题目用例.执行(id);
            if (!已删除)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
