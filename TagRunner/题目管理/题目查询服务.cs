using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TagRunner
{
    public class 题目查询服务
    {
        private readonly 文档转换器 _文档转换器;
        private readonly string _题目Json;
        private List<题目> _题目列表 = new List<题目>();
        private Dictionary<int, List<题目>> 标签题目索引 = new Dictionary<int, List<题目>>();

        public 题目查询服务(string rootDirectory)
        {
            _文档转换器 = new 文档转换器(rootDirectory);
            _题目Json = Path.Combine(rootDirectory, "Questions.json");
            Load题目列表();
            Build标签题目索引();
        }

        // 加载题目列表
        public void Load题目列表()
        {
            if (File.Exists(_题目Json))
            {
                var json = File.ReadAllText(_题目Json);
                _题目列表 = JsonConvert.DeserializeObject<List<题目>>(json) ?? new List<题目>();
            }
            else
            {
                _题目列表 = new List<题目>();
            }
        }

        // 保存题目列表到 JSON 文件
        public void Save题目列表()
        {
            var json = JsonConvert.SerializeObject(_题目列表, Formatting.Indented);
            File.WriteAllText(_题目Json, json);
        }

        // 构建标签索引
        public void Build标签题目索引()
        {
            标签题目索引.Clear();
            foreach (var q in _题目列表)
            {
                if (q.TagIds == null) continue;
                foreach (var tagId in q.TagIds)
                {
                    if (!标签题目索引.ContainsKey(tagId))
                        标签题目索引[tagId] = new List<题目>();
                    标签题目索引[tagId].Add(q);
                }
            }
        }

        // 按标签查找题目
        public List<题目> 按标签查找(int tagId)
        {
            if (标签题目索引.ContainsKey(tagId))
                return 标签题目索引[tagId];
            return new List<题目>();
        }

        // 新增题目
        public bool 新增题目(题目 newQ, string 文档路径 = null)
        {
            if (newQ == null) throw new ArgumentNullException(nameof(newQ));
            if (string.IsNullOrWhiteSpace(文档路径) || !File.Exists(文档路径))
                return false;

            // 1. 先上传文档（失败则不改动题目列表/索引/JSON）
            var uploadOk = 上传题目文档(newQ.Id, 文档路径);
            if (!uploadOk) return false;

            // 2. 上传成功后，自动转换为 HTML（不影响数据写入；失败可记录或返回 false 由业务决定）
            var convertStatus = _文档转换器.ConvertToHtml(newQ.Id);
            if (convertStatus != 文档转换结果状态.成功)
            {
                // 可选：如果转换失败要回滚文档/新增，则在此返回 false 并删除已复制的 DOCX
                // File.Delete(Path.Combine(_文档转换器.SourceDir, $"{newQ.Id}.docx"));
                // return false;
                // 这里选择不中断新增流程，仅继续写入数据
            }

            // 3. 更新内存与索引
            _题目列表.Add(newQ);
            if (newQ.TagIds != null)
            {
                foreach (var tagId in newQ.TagIds)
                {
                    if (!标签题目索引.ContainsKey(tagId))
                        标签题目索引[tagId] = new List<题目>();
                    标签题目索引[tagId].Add(newQ);
                }
            }

            // 4. 安全保存到 JSON（临时文件 + 覆盖替换）
            try
            {
                var json = JsonConvert.SerializeObject(_题目列表, Formatting.Indented);
                var tmpPath = _题目Json + ".tmp";
                File.WriteAllText(tmpPath, json);
                var bakPath = _题目Json + ".bak";
                if (File.Exists(_题目Json))
                    File.Copy(_题目Json, bakPath, overwrite: true);
                File.Copy(tmpPath, _题目Json, overwrite: true);
                File.Delete(tmpPath);
                return true;
            }
            catch
            {
                // 写 JSON 失败回滚内存与索引（可选也回滚已生成的 HTML）
                _题目列表.Remove(newQ);
                if (newQ.TagIds != null)
                {
                    foreach (var tagId in newQ.TagIds)
                    {
                        if (标签题目索引.ContainsKey(tagId))
                            标签题目索引[tagId].Remove(newQ);
                    }
                }
                return false;
            }
        }

        // 删除题目：同时处理关联文件（DOCX/HTML/PDF），并保证列表与索引一致
        public bool 删除题目(int id)
        {
            var q = _题目列表.FirstOrDefault(x => x.Id == id);
            if (q == null) return false;

            // 1. 先记录将要删除的文件路径
            var docxPath = Path.Combine(_文档转换器.SourceDir, $"{id}.docx");
            var htmlPath = Path.Combine(_文档转换器.HtmlDir, $"{id}.html");
            var pdfPath  = Path.Combine(_文档转换器.PdfDir,  $"{id}.pdf");

            // 2. 尝试删除文件（分别删除，彼此不影响；失败不阻塞，但记录结果）
            bool docxDeleted = TryDeleteFile(docxPath);
            bool htmlDeleted = TryDeleteFile(htmlPath);
            bool pdfDeleted  = TryDeleteFile(pdfPath);

            // 3. 更新内存列表与标签索引（只移除该题目的索引，避免全量重建）
            _题目列表.Remove(q);
            if (q.TagIds != null)
            {
                foreach (var tagId in q.TagIds)
                {
                    if (标签题目索引.ContainsKey(tagId))
                        标签题目索引[tagId].Remove(q);
                }
            }

            // 4. 安全保存到 JSON（临时文件 + 覆盖替换）
            try
            {
                var json = JsonConvert.SerializeObject(_题目列表, Formatting.Indented);
                var tmpPath = _题目Json + ".tmp";
                File.WriteAllText(tmpPath, json);
                var bakPath = _题目Json + ".bak";
                if (File.Exists(_题目Json))
                    File.Copy(_题目Json, bakPath, overwrite: true);
                File.Copy(tmpPath, _题目Json, overwrite: true);
                File.Delete(tmpPath);
                return true;
            }
            catch
            {
                // 写 JSON 失败则回滚内存与索引（文件删除不做强回滚，避免复杂度与风险）
                _题目列表.Add(q);
                if (q.TagIds != null)
                {
                    foreach (var tagId in q.TagIds)
                    {
                        if (!标签题目索引.ContainsKey(tagId))
                            标签题目索引[tagId] = new List<题目>();
                        if (!标签题目索引[tagId].Contains(q))
                            标签题目索引[tagId].Add(q);
                    }
                }
                return false;
            }
        }

        // 上传题目文档
        public bool 上传题目文档(int 题目id, string 源文件路径)
        {
            string destPath = Path.Combine(_文档转换器.SourceDir, $"{题目id}.docx");
            try
            {
                File.Copy(源文件路径, destPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 获取题目文档路径
        public string 获取题目文档路径(int 题目id)
        {
            string path = Path.Combine(_文档转换器.SourceDir, $"{题目id}.docx");
            return File.Exists(path) ? path : null;
        }

        // 转换为Html
        public 文档转换结果状态 转换为Html(int 题目id)
        {
            return _文档转换器.ConvertToHtml(题目id);
        }

        // 小工具：安全删除文件（存在才删，异常吞掉并返回 false）
        private static bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    // 可选：先改名为 .del 临时文件再删除，规避占用问题
                    // var tmp = path + ".del";
                    // File.Move(path, tmp);
                    // File.Delete(tmp);
                    File.Delete(path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //生产新的题目ID
        public int GenerataeNewQuestionId()
        {
            if (_题目列表.Count == 0)
                return 1;
            return _题目列表.Max(q => q.Id) + 1;
        }
    }
}
