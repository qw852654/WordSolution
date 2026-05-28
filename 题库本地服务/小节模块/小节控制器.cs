using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.小节模块;
using 题库核心.小节模块.领域;

namespace 题库本地服务.小节模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/小节")]
    public class 小节控制器 : ControllerBase
    {
        private readonly 获取小节列表用例 _获取小节列表用例;
        private readonly 获取小节详情用例 _获取小节详情用例;
        private readonly 新建小节用例 _新建小节用例;
        private readonly 更新小节用例 _更新小节用例;
        private readonly 获取小节项列表用例 _获取小节项列表用例;
        private readonly 添加小节项用例 _添加小节项用例;
        private readonly 调整小节项排序用例 _调整小节项排序用例;
        private readonly 移除小节项用例 _移除小节项用例;
        private readonly 获取小节预览HTML用例 _获取小节预览HTML用例;
        private readonly 导出小节Word用例 _导出小节Word用例;

        public 小节控制器(
            获取小节列表用例 获取小节列表用例,
            获取小节详情用例 获取小节详情用例,
            新建小节用例 新建小节用例,
            更新小节用例 更新小节用例,
            获取小节项列表用例 获取小节项列表用例,
            添加小节项用例 添加小节项用例,
            调整小节项排序用例 调整小节项排序用例,
            移除小节项用例 移除小节项用例,
            获取小节预览HTML用例 获取小节预览HTML用例,
            导出小节Word用例 导出小节Word用例)
        {
            _获取小节列表用例 = 获取小节列表用例;
            _获取小节详情用例 = 获取小节详情用例;
            _新建小节用例 = 新建小节用例;
            _更新小节用例 = 更新小节用例;
            _获取小节项列表用例 = 获取小节项列表用例;
            _添加小节项用例 = 添加小节项用例;
            _调整小节项排序用例 = 调整小节项排序用例;
            _移除小节项用例 = 移除小节项用例;
            _获取小节预览HTML用例 = 获取小节预览HTML用例;
            _导出小节Word用例 = 导出小节Word用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<小节详情结果>> 获取小节列表(
            [FromQuery] 小节状态? 状态,
            [FromQuery] int? 章节标签ID,
            [FromQuery] string? 关键词)
        {
            return Ok(_获取小节列表用例.执行(状态, 章节标签ID, 关键词));
        }

        [HttpPost]
        public ActionResult<小节详情结果> 新建小节([FromBody] 新建小节的请求 请求)
        {
            try
            {
                var 小节 = _新建小节用例.执行(请求);
                return CreatedAtAction(nameof(获取小节详情), new { 题库键 = RouteData.Values["题库键"], id = 小节.Id }, 小节);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public ActionResult<小节详情结果> 获取小节详情(int id)
        {
            var 小节 = _获取小节详情用例.执行(id);
            if (小节 == null)
            {
                return NotFound();
            }

            return Ok(小节);
        }

        [HttpPut("{id:int}")]
        public ActionResult<小节详情结果> 更新小节(int id, [FromBody] 更新小节的请求 请求)
        {
            try
            {
                var 小节 = _更新小节用例.执行(id, 请求);
                if (小节 == null)
                {
                    return NotFound();
                }

                return Ok(小节);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}/项目")]
        public ActionResult<IReadOnlyList<小节项结果>> 获取小节项列表(int id)
        {
            var 项目列表 = _获取小节项列表用例.执行(id);
            if (项目列表 == null)
            {
                return NotFound();
            }

            return Ok(项目列表);
        }

        [HttpPost("{id:int}/项目")]
        public ActionResult<小节项结果> 添加小节项(int id, [FromBody] 添加小节项的请求 请求)
        {
            try
            {
                var 小节项 = _添加小节项用例.执行(id, 请求);
                if (小节项 == null)
                {
                    return NotFound();
                }

                return Ok(小节项);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}/项目排序")]
        public ActionResult<IReadOnlyList<小节项结果>> 调整小节项排序(int id, [FromBody] 调整小节项排序的请求 请求)
        {
            try
            {
                var 项目列表 = _调整小节项排序用例.执行(id, 请求);
                if (项目列表 == null)
                {
                    return NotFound();
                }

                return Ok(项目列表);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}/项目/{项目ID:int}")]
        public ActionResult 移除小节项(int id, int 项目ID)
        {
            return _移除小节项用例.执行(id, 项目ID) ? NoContent() : NotFound();
        }

        [HttpGet("{id:int}/预览html")]
        public ActionResult 获取小节预览HTML(int id)
        {
            var HTML内容 = _获取小节预览HTML用例.执行(id);
            if (string.IsNullOrWhiteSpace(HTML内容))
            {
                return NotFound();
            }

            return Content(HTML内容, "text/html; charset=utf-8");
        }

        [HttpPost("{id:int}/导出Word")]
        public ActionResult 导出小节Word(int id)
        {
            try
            {
                var 导出结果 = _导出小节Word用例.执行(id);
                if (导出结果 == null)
                {
                    return NotFound();
                }

                return File(
                    导出结果.文件内容,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    导出结果.文件名);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
