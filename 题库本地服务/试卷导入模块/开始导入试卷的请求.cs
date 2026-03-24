using Microsoft.AspNetCore.Http;

namespace 题库本地服务.试卷导入模块
{
    public class 开始导入试卷的请求
    {
        public IFormFile? File { get; set; }

        public int 年份标签ID { get; set; }

        public int 来源标签ID { get; set; }
    }
}
