using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using 题库基础设施.题库实例;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;

namespace 题库基础设施.内容块模块
{
    public class 内容块编辑会话文件存储 : I内容块编辑会话文件存储
    {
        private readonly 题库路径提供器 _题库路径提供器;

        public 内容块编辑会话文件存储(题库路径提供器 题库路径提供器)
        {
            _题库路径提供器 = 题库路径提供器;
        }

        public string 获取编辑文件路径(string 题库键, string 会话ID)
        {
            return Path.Combine(获取会话目录(题库键, 会话ID), "content.docx");
        }

        public string 获取会话信息文件路径(string 题库键, string 会话ID)
        {
            return Path.Combine(获取会话目录(题库键, 会话ID), "session.json");
        }

        public void 复制文件(string 源文件路径, string 目标文件路径, bool 覆盖 = true)
        {
            var 目录路径 = Path.GetDirectoryName(目标文件路径);
            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            File.Copy(源文件路径, 目标文件路径, 覆盖);
        }

        public void 写入会话信息(内容块编辑会话 会话)
        {
            var 会话信息文件路径 = 获取会话信息文件路径(会话.题库键, 会话.会话ID);
            var 目录路径 = Path.GetDirectoryName(会话信息文件路径);
            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            var json = JsonSerializer.Serialize(会话, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            File.WriteAllText(会话信息文件路径, json);
        }

        private string 获取会话目录(string 题库键, string 会话ID)
        {
            return Path.Combine(
                _题库路径提供器.获取题库根目录(题库键),
                "temp",
                "edit-sessions",
                会话ID);
        }
    }
}
