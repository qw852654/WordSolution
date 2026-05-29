using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.内容块模块;
using 题库核心.内容块模块.领域;

namespace 题库本地服务.元数据选项模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/元数据选项")]
    public class 元数据选项控制器 : ControllerBase
    {
        private readonly 获取元数据选项列表用例 _获取元数据选项列表用例;
        private readonly 新增元数据选项用例 _新增元数据选项用例;
        private readonly 更新元数据选项用例 _更新元数据选项用例;
        private readonly 设置元数据选项启用状态用例 _设置元数据选项启用状态用例;

        public 元数据选项控制器(
            获取元数据选项列表用例 获取元数据选项列表用例,
            新增元数据选项用例 新增元数据选项用例,
            更新元数据选项用例 更新元数据选项用例,
            设置元数据选项启用状态用例 设置元数据选项启用状态用例)
        {
            _获取元数据选项列表用例 = 获取元数据选项列表用例;
            _新增元数据选项用例 = 新增元数据选项用例;
            _更新元数据选项用例 = 更新元数据选项用例;
            _设置元数据选项启用状态用例 = 设置元数据选项启用状态用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<元数据选项结果>> 获取列表([FromQuery] 元数据选项类别? category)
        {
            return Ok(_获取元数据选项列表用例.执行(category));
        }

        [HttpPost]
        public ActionResult<元数据选项结果> 新增([FromBody] 新增元数据选项的请求 请求)
        {
            try
            {
                var 选项 = _新增元数据选项用例.执行(请求);
                return CreatedAtAction(nameof(获取列表), new { 题库键 = RouteData.Values["题库键"], category = 选项.Category }, 选项);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public ActionResult<元数据选项结果> 更新(int id, [FromBody] 更新元数据选项的请求 请求)
        {
            try
            {
                var 选项 = _更新元数据选项用例.执行(id, 请求);
                return 选项 == null ? NotFound() : Ok(选项);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id:int}/停用")]
        public ActionResult<元数据选项结果> 停用(int id)
        {
            var 选项 = _设置元数据选项启用状态用例.执行(id, false);
            return 选项 == null ? NotFound() : Ok(选项);
        }

        [HttpPost("{id:int}/启用")]
        public ActionResult<元数据选项结果> 启用(int id)
        {
            var 选项 = _设置元数据选项启用状态用例.执行(id, true);
            return 选项 == null ? NotFound() : Ok(选项);
        }
    }
}
