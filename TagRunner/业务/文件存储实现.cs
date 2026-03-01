using Core.QuestionBank.Domain;
using System;
using System.IO;

namespace TagRunner.业务
{
    /// <summary>
    /// 本地文件存储实现：基于题库配置管理文件路径与文件操作。
    /// 构造时注入 题库配置 实例。
    /// </summary>
    public class 文件存储实现 : I文件存储
    {
        private readonly 题库配置 _配置;

        public 文件存储实现(题库配置 配置)
        {
            _配置 = 配置 ?? throw new ArgumentNullException(nameof(配置));
            // 确保目录存在
            _配置.初始化目录(如果不存在就创建: true);
        }

        /// <summary>
        /// 复制源文件（如 DOCX）到题库 source 目录，按题目 Id 命名并返回目标路径。
        /// 若目标已存在则覆盖。
        /// </summary>
        public string 复制到题库(string 源文件路径, int 题目Id)
        {
            if (string.IsNullOrWhiteSpace(源文件路径)) throw new ArgumentException("源文件路径不能为空", nameof(源文件路径));
            if (题目Id <= 0) throw new ArgumentException("题目Id必须为正整数", nameof(题目Id));

            if (!File.Exists(源文件路径))
                throw new FileNotFoundException("源文件不存在", 源文件路径);

            var 目标路径 = _配置.获取Docx目录(题目Id);
            var 目标目录 = Path.GetDirectoryName(目标路径);
            if (!Directory.Exists(目标目录)) Directory.CreateDirectory(目标目录);

            // 教学版：直接覆盖（更直观），避免因为目标文件已存在导致异常
            File.Copy(源文件路径, 目标路径, overwrite: true);
            return 目标路径;
        }

        /// <summary>
        /// 获取题目的 Docx 路径（绝对路径）。
        /// </summary>
        public string 获取Docx路径(int 题目Id) => _配置.获取Docx目录(题目Id);

        /// <summary>
        /// 获取题目的 Html 路径（绝对路径）。
        /// </summary>
        public string 获取Html路径(int 题目Id) => _配置.获取Html目录(题目Id);

        /// <summary>
        /// 删除题目相关的文件（Docx + Html），返回是否删除成功（目标不存在也算成功）。
        /// </summary>
        public bool 删除题目文件(int 题目Id)
        {
            if (题目Id <= 0) return false;

            var docx = _配置.获取Docx目录(题目Id);
            var html = _配置.获取Html目录(题目Id);
            var ok = true;

            try
            {
                if (File.Exists(docx)) File.Delete(docx);
            }
            catch
            {
                ok = false;
            }

            try
            {
                if (File.Exists(html)) File.Delete(html);
            }
            catch
            {
                ok = false;
            }

            return ok;
        }
    }
}
