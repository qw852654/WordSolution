using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using 题库基础设施.题库实例;

namespace 题库本地服务.试卷导入模块
{
    [ApiController]
    [Route("api/题库实例/{题库键}/试卷导入")]
    public class 试卷文件控制器 : ControllerBase
    {
        private readonly 题库路径提供器 _题库路径提供器;

        public 试卷文件控制器(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        [HttpGet("{paperId}/下载")]
        public IActionResult 下载试卷(int paperId)
        {
            var 题库键 = RouteData.Values["题库键"]?.ToString() ?? string.Empty;
            var 题库根目录 = _题库路径提供器.获取题库根目录(题库键);
            var 试卷源文件目录 = Path.Combine(题库根目录, "papers", paperId.ToString(), "sources");
            if (!Directory.Exists(试卷源文件目录))
            {
                return NotFound("未找到这套试卷的源文件。");
            }

            var 最新文件 = new DirectoryInfo(试卷源文件目录)
                .GetFiles("*.docx")
                .OrderByDescending(文件 => 文件.LastWriteTimeUtc)
                .FirstOrDefault();

            if (最新文件 == null)
            {
                return NotFound("当前试卷还没有可下载的源文件。");
            }

            var 下载文件名 = 获取下载文件名(最新文件.Name);
            return PhysicalFile(
                最新文件.FullName,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                下载文件名);
        }

        private static string 获取下载文件名(string 存储文件名)
        {
            var 下划线索引 = 存储文件名.IndexOf('_');
            if (下划线索引 >= 0 && 下划线索引 < 存储文件名.Length - 1)
            {
                return 存储文件名.Substring(下划线索引 + 1);
            }

            return 存储文件名;
        }
    }
}
