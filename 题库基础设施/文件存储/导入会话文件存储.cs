using System.IO;
using System.Text.Json;
using 题库基础设施.题库实例;
using 题库核心.试卷导入模块.契约;
using 题库核心.试卷导入模块.领域;

namespace 题库基础设施.文件存储
{
    public class 导入会话文件存储 : I导入会话存储
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
        };

        private readonly 题库路径提供器 _题库路径提供器;

        public 导入会话文件存储(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        public void 保存(导入试卷会话 会话)
        {
            var 文件路径 = 获取会话文件路径(会话.SessionId);
            var 目录路径 = Path.GetDirectoryName(文件路径);
            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            var Json内容 = JsonSerializer.Serialize(会话, JsonOptions);
            File.WriteAllText(文件路径, Json内容);
        }

        public bool 存在(string sessionId)
        {
            return File.Exists(获取会话文件路径(sessionId));
        }

        public 导入试卷会话? 获取(string sessionId)
        {
            var 文件路径 = 获取会话文件路径(sessionId);
            if (!File.Exists(文件路径))
            {
                return null;
            }

            var Json内容 = File.ReadAllText(文件路径);
            return JsonSerializer.Deserialize<导入试卷会话>(Json内容, JsonOptions);
        }

        public void 删除(string sessionId)
        {
            var 会话目录 = 获取会话工作目录(sessionId);
            if (Directory.Exists(会话目录))
            {
                Directory.Delete(会话目录, true);
            }
        }

        public string 获取会话工作目录(string sessionId)
        {
            var 当前题库键 = _题库路径提供器.获取当前请求题库键();
            return Path.Combine(_题库路径提供器.获取题库根目录(当前题库键), "temp", "import-sessions", sessionId);
        }

        private string 获取会话文件路径(string sessionId)
        {
            return Path.Combine(获取会话工作目录(sessionId), "session.json");
        }
    }
}
