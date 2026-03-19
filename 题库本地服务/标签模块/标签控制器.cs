using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.标签模块;
using 题库核心.标签模块.领域;

namespace 题库本地服务.标签模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/标签")]
    public class 标签控制器 : ControllerBase
    {
        private readonly 获取标签列表用例 _获取标签列表用例;
        private readonly 新增标签用例 _新增标签用例;
        private readonly 更新标签用例 _更新标签用例;
        private readonly 调整标签父级用例 _调整标签父级用例;
        private readonly 调整标签排序用例 _调整标签排序用例;
        private readonly 删除标签用例 _删除标签用例;

        public 标签控制器(
            获取标签列表用例 获取标签列表用例,
            新增标签用例 新增标签用例,
            更新标签用例 更新标签用例,
            调整标签父级用例 调整标签父级用例,
            调整标签排序用例 调整标签排序用例,
            删除标签用例 删除标签用例)
        {
            _获取标签列表用例 = 获取标签列表用例;
            _新增标签用例 = 新增标签用例;
            _更新标签用例 = 更新标签用例;
            _调整标签父级用例 = 调整标签父级用例;
            _调整标签排序用例 = 调整标签排序用例;
            _删除标签用例 = 删除标签用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<标签>> 获取标签列表([FromQuery] int 标签种类ID)
        {
            var 标签列表 = _获取标签列表用例.执行(标签种类ID);
            return Ok(标签列表);
        }

        [HttpPost]
        public ActionResult<标签> 新增标签([FromBody] 新增标签的请求 请求)
        {
            var 新标签 = _新增标签用例.执行(请求);
            return Ok(新标签);
        }

        [HttpPut("{id:int}")]
        public ActionResult<标签> 更新标签(int id, [FromBody] 更新标签的请求 请求)
        {
            var 标签 = _更新标签用例.执行(id, 请求);
            return Ok(标签);
        }

        [HttpPost("{id:int}/调整父级")]
        public ActionResult<标签> 调整父级(int id, [FromBody] 调整标签父级的请求 请求)
        {
            var 标签 = _调整标签父级用例.执行(id, 请求);
            return Ok(标签);
        }

        [HttpPost("{id:int}/同级排序")]
        public IActionResult 调整同级排序(int id, [FromBody] 调整标签排序的请求 请求)
        {
            _调整标签排序用例.执行(id, 请求);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public IActionResult 删除标签(int id)
        {
            _删除标签用例.执行(id);
            return NoContent();
        }
    }
}
