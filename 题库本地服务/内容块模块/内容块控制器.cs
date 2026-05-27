using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using 题库应用.内容块模块;
using 题库应用.引用关系模块;
using 题库核心.内容块模块.领域;

namespace 题库本地服务.内容块模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/内容块")]
    public class 内容块控制器 : ControllerBase
    {
        private readonly 获取内容块列表用例 _获取内容块列表用例;
        private readonly 新建内容块用例 _新建内容块用例;
        private readonly 更新内容块元数据用例 _更新内容块元数据用例;
        private readonly 录入Ooxml内容块用例 _录入Ooxml内容块用例;
        private readonly 获取内容块详情用例 _获取内容块详情用例;
        private readonly 更新Ooxml内容块用例 _更新Ooxml内容块用例;
        private readonly 获取内容块预览HTML用例 _获取内容块预览HTML用例;
        private readonly 获取内容块文件Base64用例 _获取内容块文件Base64用例;
        private readonly 获取内容块版本列表用例 _获取内容块版本列表用例;
        private readonly 获取内容块标签列表用例 _获取内容块标签列表用例;
        private readonly 更新内容块标签列表用例 _更新内容块标签列表用例;
        private readonly 获取内容块子项列表用例 _获取内容块子项列表用例;
        private readonly 添加内容块子项用例 _添加内容块子项用例;
        private readonly 调整内容块子块排序用例 _调整内容块子块排序用例;
        private readonly 移除内容块子项用例 _移除内容块子项用例;
        private readonly 获取内容块结构树用例 _获取内容块结构树用例;
        private readonly 获取内容块引用影响用例 _获取内容块引用影响用例;
        private readonly 创建新内容块编辑会话用例 _创建新内容块编辑会话用例;
        private readonly 创建已有内容块编辑会话用例 _创建已有内容块编辑会话用例;
        private readonly 获取内容块编辑会话状态用例 _获取内容块编辑会话状态用例;
        private readonly 同步内容块编辑会话用例 _同步内容块编辑会话用例;
        private readonly 取消内容块编辑会话用例 _取消内容块编辑会话用例;

        public 内容块控制器(
            获取内容块列表用例 获取内容块列表用例,
            新建内容块用例 新建内容块用例,
            更新内容块元数据用例 更新内容块元数据用例,
            录入Ooxml内容块用例 录入Ooxml内容块用例,
            获取内容块详情用例 获取内容块详情用例,
            更新Ooxml内容块用例 更新Ooxml内容块用例,
            获取内容块预览HTML用例 获取内容块预览HTML用例,
            获取内容块文件Base64用例 获取内容块文件Base64用例,
            获取内容块版本列表用例 获取内容块版本列表用例,
            获取内容块标签列表用例 获取内容块标签列表用例,
            更新内容块标签列表用例 更新内容块标签列表用例,
            获取内容块子项列表用例 获取内容块子项列表用例,
            添加内容块子项用例 添加内容块子项用例,
            调整内容块子块排序用例 调整内容块子块排序用例,
            移除内容块子项用例 移除内容块子项用例,
            获取内容块结构树用例 获取内容块结构树用例,
            获取内容块引用影响用例 获取内容块引用影响用例,
            创建新内容块编辑会话用例 创建新内容块编辑会话用例,
            创建已有内容块编辑会话用例 创建已有内容块编辑会话用例,
            获取内容块编辑会话状态用例 获取内容块编辑会话状态用例,
            同步内容块编辑会话用例 同步内容块编辑会话用例,
            取消内容块编辑会话用例 取消内容块编辑会话用例)
        {
            _获取内容块列表用例 = 获取内容块列表用例;
            _新建内容块用例 = 新建内容块用例;
            _更新内容块元数据用例 = 更新内容块元数据用例;
            _录入Ooxml内容块用例 = 录入Ooxml内容块用例;
            _获取内容块详情用例 = 获取内容块详情用例;
            _更新Ooxml内容块用例 = 更新Ooxml内容块用例;
            _获取内容块预览HTML用例 = 获取内容块预览HTML用例;
            _获取内容块文件Base64用例 = 获取内容块文件Base64用例;
            _获取内容块版本列表用例 = 获取内容块版本列表用例;
            _获取内容块标签列表用例 = 获取内容块标签列表用例;
            _更新内容块标签列表用例 = 更新内容块标签列表用例;
            _获取内容块子项列表用例 = 获取内容块子项列表用例;
            _添加内容块子项用例 = 添加内容块子项用例;
            _调整内容块子块排序用例 = 调整内容块子块排序用例;
            _移除内容块子项用例 = 移除内容块子项用例;
            _获取内容块结构树用例 = 获取内容块结构树用例;
            _获取内容块引用影响用例 = 获取内容块引用影响用例;
            _创建新内容块编辑会话用例 = 创建新内容块编辑会话用例;
            _创建已有内容块编辑会话用例 = 创建已有内容块编辑会话用例;
            _获取内容块编辑会话状态用例 = 获取内容块编辑会话状态用例;
            _同步内容块编辑会话用例 = 同步内容块编辑会话用例;
            _取消内容块编辑会话用例 = 取消内容块编辑会话用例;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<内容块详情结果>> 获取内容块列表(
            [FromQuery] 内容块类型? 类型,
            [FromQuery] 内容块状态? 状态,
            [FromQuery] string? 关键词,
            [FromQuery] List<int>? 标签ID列表)
        {
            return Ok(_获取内容块列表用例.执行(类型, 状态, 关键词, 标签ID列表));
        }

        [HttpPost]
        public ActionResult<内容块详情结果> 新建内容块([FromBody] 新建内容块的请求 请求)
        {
            var 内容块 = _新建内容块用例.执行(请求);
            return CreatedAtAction(nameof(获取内容块详情), new { 题库键 = RouteData.Values["题库键"], id = 内容块.Id }, 内容块);
        }

        [HttpPost("ooxml")]
        public ActionResult<内容块详情结果> 录入Ooxml内容块([FromBody] 录入Ooxml内容块的请求 请求)
        {
            var 内容块 = _录入Ooxml内容块用例.执行(请求);
            return CreatedAtAction(nameof(获取内容块详情), new { 题库键 = RouteData.Values["题库键"], id = 内容块.Id }, 内容块);
        }

        [HttpGet("{id:int}")]
        public ActionResult<内容块详情结果> 获取内容块详情(int id)
        {
            var 内容块 = _获取内容块详情用例.执行(id);
            if (内容块 == null)
            {
                return NotFound();
            }

            return Ok(内容块);
        }

        [HttpPut("{id:int}")]
        public ActionResult<内容块详情结果> 更新内容块元数据(int id, [FromBody] 更新内容块元数据的请求 请求)
        {
            try
            {
                var 内容块 = _更新内容块元数据用例.执行(id, 请求);
                if (内容块 == null)
                {
                    return NotFound();
                }

                return Ok(内容块);
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

        [HttpPut("{id:int}/ooxml")]
        public ActionResult<内容块详情结果> 更新Ooxml内容块(int id, [FromBody] 更新Ooxml内容块的请求 请求)
        {
            var 内容块 = _更新Ooxml内容块用例.执行(id, 请求);
            if (内容块 == null)
            {
                return NotFound();
            }

            return Ok(内容块);
        }

        [HttpGet("{id:int}/预览html")]
        public ActionResult 获取内容块预览HTML(int id)
        {
            var HTML内容 = _获取内容块预览HTML用例.执行(id);
            if (string.IsNullOrWhiteSpace(HTML内容))
            {
                return NotFound();
            }

            return Content(HTML内容, "text/html; charset=utf-8");
        }

        [HttpGet("{id:int}/文件base64")]
        public ActionResult 获取内容块文件Base64(int id)
        {
            var 文件Base64 = _获取内容块文件Base64用例.执行(id);
            if (string.IsNullOrWhiteSpace(文件Base64))
            {
                return NotFound();
            }

            return Content(文件Base64, "text/plain; charset=utf-8");
        }

        [HttpGet("{id:int}/版本")]
        public ActionResult<IReadOnlyList<内容块版本结果>> 获取内容块版本列表(int id)
        {
            return Ok(_获取内容块版本列表用例.执行(id));
        }

        [HttpGet("{id:int}/标签")]
        public ActionResult<IReadOnlyList<内容块标签结果>> 获取内容块标签列表(int id)
        {
            var 标签列表 = _获取内容块标签列表用例.执行(id);
            if (标签列表 == null)
            {
                return NotFound();
            }

            return Ok(标签列表);
        }

        [HttpPut("{id:int}/标签")]
        public ActionResult<IReadOnlyList<内容块标签结果>> 更新内容块标签列表(int id, [FromBody] 更新内容块标签的请求 请求)
        {
            try
            {
                var 标签列表 = _更新内容块标签列表用例.执行(id, 请求);
                if (标签列表 == null)
                {
                    return NotFound();
                }

                return Ok(标签列表);
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

        [HttpGet("{id:int}/子块")]
        public ActionResult<IReadOnlyList<内容块子项结果>> 获取内容块子项列表(int id)
        {
            var 子项列表 = _获取内容块子项列表用例.执行(id);
            if (子项列表 == null)
            {
                return NotFound();
            }

            return Ok(子项列表);
        }

        [HttpPost("{id:int}/子块")]
        public ActionResult<内容块子项结果> 添加内容块子项(int id, [FromBody] 添加内容块子项的请求 请求)
        {
            try
            {
                var 子项 = _添加内容块子项用例.执行(id, 请求);
                if (子项 == null)
                {
                    return NotFound();
                }

                return Ok(子项);
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

        [HttpPut("{id:int}/子块排序")]
        public ActionResult<IReadOnlyList<内容块子项结果>> 调整内容块子块排序(int id, [FromBody] 调整内容块子块排序的请求 请求)
        {
            try
            {
                var 子项列表 = _调整内容块子块排序用例.执行(id, 请求);
                if (子项列表 == null)
                {
                    return NotFound();
                }

                return Ok(子项列表);
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

        [HttpDelete("{id:int}/子块/{子项ID:int}")]
        public ActionResult 移除内容块子项(int id, int 子项ID)
        {
            return _移除内容块子项用例.执行(id, 子项ID) ? NoContent() : NotFound();
        }

        [HttpGet("{id:int}/结构树")]
        public ActionResult<内容块结构树结果> 获取内容块结构树(int id)
        {
            var 结构树 = _获取内容块结构树用例.执行(id);
            if (结构树 == null)
            {
                return NotFound();
            }

            return Ok(结构树);
        }

        [HttpGet("{id:int}/引用")]
        public ActionResult<内容块引用影响结果> 获取内容块引用影响(int id)
        {
            var 引用影响 = _获取内容块引用影响用例.执行(id);
            if (引用影响 == null)
            {
                return NotFound();
            }

            return Ok(引用影响);
        }

        [HttpPost("编辑会话")]
        public ActionResult<内容块编辑会话结果> 创建新内容块编辑会话(string 题库键, [FromBody] 创建新内容块编辑会话的请求 请求)
        {
            try
            {
                return Ok(_创建新内容块编辑会话用例.执行(题库键, 请求));
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

        [HttpPost("{id:int}/编辑会话")]
        public ActionResult<内容块编辑会话结果> 创建已有内容块编辑会话(string 题库键, int id, [FromBody] 创建已有内容块编辑会话的请求 请求)
        {
            try
            {
                var 会话 = _创建已有内容块编辑会话用例.执行(题库键, id, 请求);
                if (会话 == null)
                {
                    return NotFound();
                }

                return Ok(会话);
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

        [HttpGet("编辑会话/{会话ID}")]
        public ActionResult<内容块编辑会话结果> 获取内容块编辑会话状态(string 会话ID)
        {
            var 会话 = _获取内容块编辑会话状态用例.执行(会话ID);
            if (会话 == null)
            {
                return NotFound();
            }

            return Ok(会话);
        }

        [HttpPost("编辑会话/{会话ID}/同步")]
        public ActionResult<内容块编辑会话结果> 同步内容块编辑会话(string 会话ID)
        {
            var 会话 = _同步内容块编辑会话用例.执行(会话ID, true);
            if (会话 == null)
            {
                return NotFound();
            }

            return Ok(会话);
        }

        [HttpPost("编辑会话/{会话ID}/取消")]
        public ActionResult<内容块编辑会话结果> 取消内容块编辑会话(string 会话ID)
        {
            var 会话 = _取消内容块编辑会话用例.执行(会话ID);
            if (会话 == null)
            {
                return NotFound();
            }

            return Ok(会话);
        }
    }
}
