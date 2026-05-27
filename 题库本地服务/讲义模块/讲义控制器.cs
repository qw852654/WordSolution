using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.讲义模块;
using 题库核心.讲义模块.领域;

namespace 题库本地服务.讲义模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/讲义")]
    public class 讲义控制器 : ControllerBase
    {
        private readonly 获取讲义列表用例 _获取讲义列表用例;
        private readonly 获取讲义详情用例 _获取讲义详情用例;
        private readonly 新建讲义用例 _新建讲义用例;
        private readonly 更新讲义用例 _更新讲义用例;
        private readonly 获取讲义项列表用例 _获取讲义项列表用例;
        private readonly 添加讲义项用例 _添加讲义项用例;
        private readonly 调整讲义项排序用例 _调整讲义项排序用例;
        private readonly 移除讲义项用例 _移除讲义项用例;
        private readonly 获取讲义结构树用例 _获取讲义结构树用例;
        private readonly 生成讲义用例 _生成讲义用例;
        private readonly 获取讲义生成记录列表用例 _获取讲义生成记录列表用例;
        private readonly 获取讲义生成文件用例 _获取讲义生成文件用例;

        public 讲义控制器(
            获取讲义列表用例 获取讲义列表用例,
            获取讲义详情用例 获取讲义详情用例,
            新建讲义用例 新建讲义用例,
            更新讲义用例 更新讲义用例,
            获取讲义项列表用例 获取讲义项列表用例,
            添加讲义项用例 添加讲义项用例,
            调整讲义项排序用例 调整讲义项排序用例,
            移除讲义项用例 移除讲义项用例,
            获取讲义结构树用例 获取讲义结构树用例,
            生成讲义用例 生成讲义用例,
            获取讲义生成记录列表用例 获取讲义生成记录列表用例,
            获取讲义生成文件用例 获取讲义生成文件用例)
        {
            _获取讲义列表用例 = 获取讲义列表用例;
            _获取讲义详情用例 = 获取讲义详情用例;
            _新建讲义用例 = 新建讲义用例;
            _更新讲义用例 = 更新讲义用例;
            _获取讲义项列表用例 = 获取讲义项列表用例;
            _添加讲义项用例 = 添加讲义项用例;
            _调整讲义项排序用例 = 调整讲义项排序用例;
            _移除讲义项用例 = 移除讲义项用例;
            _获取讲义结构树用例 = 获取讲义结构树用例;
            _生成讲义用例 = 生成讲义用例;
            _获取讲义生成记录列表用例 = 获取讲义生成记录列表用例;
            _获取讲义生成文件用例 = 获取讲义生成文件用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<讲义详情结果>> 获取讲义列表(
            [FromQuery] 讲义状态? 状态,
            [FromQuery] string? 关键词)
        {
            return Ok(_获取讲义列表用例.执行(状态, 关键词));
        }

        [HttpPost]
        public ActionResult<讲义详情结果> 新建讲义([FromBody] 新建讲义的请求 请求)
        {
            try
            {
                var 讲义 = _新建讲义用例.执行(请求);
                return CreatedAtAction(nameof(获取讲义详情), new { 题库键 = RouteData.Values["题库键"], id = 讲义.Id }, 讲义);
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
        public ActionResult<讲义详情结果> 获取讲义详情(int id)
        {
            var 讲义 = _获取讲义详情用例.执行(id);
            return 讲义 == null ? NotFound() : Ok(讲义);
        }

        [HttpPut("{id:int}")]
        public ActionResult<讲义详情结果> 更新讲义(int id, [FromBody] 更新讲义的请求 请求)
        {
            try
            {
                var 讲义 = _更新讲义用例.执行(id, 请求);
                return 讲义 == null ? NotFound() : Ok(讲义);
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
        public ActionResult<IReadOnlyList<讲义项结果>> 获取讲义项列表(int id)
        {
            var 项目列表 = _获取讲义项列表用例.执行(id);
            return 项目列表 == null ? NotFound() : Ok(项目列表);
        }

        [HttpPost("{id:int}/项目")]
        public ActionResult<讲义项结果> 添加讲义项(int id, [FromBody] 添加讲义项的请求 请求)
        {
            try
            {
                var 讲义项 = _添加讲义项用例.执行(id, 请求);
                return 讲义项 == null ? NotFound() : Ok(讲义项);
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
        public ActionResult<IReadOnlyList<讲义项结果>> 调整讲义项排序(int id, [FromBody] 调整讲义项排序的请求 请求)
        {
            try
            {
                var 项目列表 = _调整讲义项排序用例.执行(id, 请求);
                return 项目列表 == null ? NotFound() : Ok(项目列表);
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
        public ActionResult 移除讲义项(int id, int 项目ID)
        {
            return _移除讲义项用例.执行(id, 项目ID) ? NoContent() : NotFound();
        }

        [HttpGet("{id:int}/结构树")]
        public ActionResult<讲义结构树结果> 获取讲义结构树(int id)
        {
            var 结构树 = _获取讲义结构树用例.执行(id);
            return 结构树 == null ? NotFound() : Ok(结构树);
        }

        [HttpPost("{id:int}/生成")]
        public ActionResult<讲义生成记录结果> 生成讲义(int id)
        {
            try
            {
                var 生成记录 = _生成讲义用例.执行(id);
                return 生成记录 == null ? NotFound() : Ok(生成记录);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}/生成记录")]
        public ActionResult<IReadOnlyList<讲义生成记录结果>> 获取生成记录列表(int id)
        {
            var 生成记录列表 = _获取讲义生成记录列表用例.执行(id);
            return 生成记录列表 == null ? NotFound() : Ok(生成记录列表);
        }

        [HttpGet("{id:int}/生成记录/{生成记录ID:int}/文件")]
        public ActionResult 获取生成文件(int id, int 生成记录ID)
        {
            var 文件 = _获取讲义生成文件用例.执行(id, 生成记录ID);
            if (文件 == null)
            {
                return NotFound();
            }

            return File(
                文件.Value.文件内容,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                文件.Value.文件名);
        }
    }
}
