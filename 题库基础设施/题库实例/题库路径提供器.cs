using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace 题库基础设施.题库实例
{
    public class 题库路径提供器
    {
        public const string 默认测试题库键 = "TEST";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _题库中心根目录;

        public 题库路径提供器(IHttpContextAccessor httpContextAccessor, string 题库中心根目录)
        {
            _httpContextAccessor = httpContextAccessor;
            _题库中心根目录 = 题库中心根目录;
        }

        public string 获取题库中心根目录()
        {
            return _题库中心根目录;
        }

        public string 获取当前请求题库键()
        {
            var 题库键 = _httpContextAccessor.HttpContext?.Request.RouteValues["题库键"]?.ToString();
            return string.IsNullOrWhiteSpace(题库键) ? 默认测试题库键 : 规范化题库键(题库键);
        }

        public string 规范化题库键(string 题库键)
        {
            if (string.IsNullOrWhiteSpace(题库键))
            {
                throw new ArgumentException("题库键不能为空。", nameof(题库键));
            }

            var 修整后的题库键 = 题库键.Trim();
            if (修整后的题库键.Contains(Path.DirectorySeparatorChar)
                || 修整后的题库键.Contains(Path.AltDirectorySeparatorChar)
                || 修整后的题库键.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("题库键包含非法字符。", nameof(题库键));
            }

            return 修整后的题库键;
        }

        public string 获取题库根目录(string 题库键)
        {
            return Path.Combine(_题库中心根目录, 规范化题库键(题库键));
        }

        public string 获取数据库文件路径(string 题库键)
        {
            return Path.Combine(获取题库根目录(题库键), "question-bank.db");
        }

        public string 获取Source目录(string 题库键)
        {
            return Path.Combine(获取题库根目录(题库键), "source");
        }

        public string 获取Html目录(string 题库键)
        {
            return Path.Combine(获取题库根目录(题库键), "html");
        }

        public string 获取Index目录(string 题库键)
        {
            return Path.Combine(获取题库根目录(题库键), "index");
        }

        public IEnumerable<string> 获取所有题库键()
        {
            if (!Directory.Exists(_题库中心根目录))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.GetDirectories(_题库中心根目录)
                .Select(Path.GetFileName)
                .Where(题库键 => !string.IsNullOrWhiteSpace(题库键))
                .Select(题库键 => 题库键!)
                .OrderBy(题库键 => 题库键, StringComparer.OrdinalIgnoreCase);
        }

        public bool 题库目录结构完整(string 题库键)
        {
            var 题库根目录 = 获取题库根目录(题库键);
            return File.Exists(Path.Combine(题库根目录, "question-bank.db"))
                && Directory.Exists(Path.Combine(题库根目录, "source"))
                && Directory.Exists(Path.Combine(题库根目录, "html"));
        }
    }
}
