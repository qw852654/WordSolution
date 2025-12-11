using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TagRunner
{
    /// <summary>
    /// 用标签查询服务初始化，负责标签的写入与持久化
    /// 提供：
    /// - 新增标签
    /// - 删除标签
    /// - 修改标签
    /// 
    /// 写入本地文件和持久化是私有方法，不对外暴露。
    /// </summary>
    public class 标签维护器
    {
        private readonly 标签查询服务 _query;
        private string TagsJsonPath { get; }

        public 标签维护器(标签查询服务 queryService)
        {
            _query = queryService ?? throw new ArgumentNullException(nameof(queryService));
            TagsJsonPath = _query.TagsJsonPath;
        }

        /// <summary>
        /// 新增标签（自动生成唯一 Id）。必须提供非空名称和类别。
        /// </summary>
        public int 新增标签(string name, string category, int? parentId = null,  int? numericValue = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("标签名称不能为空", nameof(name));

            if (parentId == null)
                category = name;

            // 先检查重复（同一父节点 + 同类别 + 同名）
            if (_query.ExistsByName(name.Trim(), parentId, category))
                throw new InvalidOperationException($"已存在同名标签：'{name.Trim()}'（父Id={parentId?.ToString() ?? "null"}，类别={category ?? "null"}）");


            // 父存在性检查（当提供 parentId）
            标签 parent = null;
            if (!parentId.HasValue)
            { var result = MessageBox.Show(
                    $"指定的父标签 Id 不存在，是否作为根标签创建？\n\n标签名称：{name.Trim()}\n类别：{category ?? "null"}",
                    "确认创建根标签",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    throw new InvalidOperationException("操作已取消，未创建标签。");
                parentId = null; // 作为根标签
            }
            else
            {
                parent = _query.GetById(parentId.Value);
                if (parent == null)
                    throw new InvalidOperationException($"指定的父标签 Id 不存在，无法创建子标签（Id={parentId.Value}）。");
            }



            // 生成新 Id（最大 Id + 1）
            var newId = GenerateNewId();

            var newTag = new 标签
            {
                Id = newId,
                Name = name.Trim(),
                ParentId = parentId,
                Category = category,
                NumericValue = numericValue,
                Children = new List<标签>()
            };

            // 更新树结构
            if (parent == null)
            {
                _query.标签树根.Add(newTag);
            }
            else
            {
                parent.Children.Add(newTag);
            }

            // 持久化并重载，保证索引与树一致
            写入标签到Json();
            _query.加载标签树();

            return newId;
        }

        
        public bool ID删除标签(int tagId)
        {
            throw new NotImplementedException();
        }

        public bool ID修改标签(int tagId, string newName, string newCategory, int? newNumericValue = null)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// 通过遍历当前标签树（含所有子孙），生成新的唯一 Id：取现有最大 Id + 1。
        /// 若当前无任何标签，返回 1。
        /// </summary>
        private int GenerateNewId()
        {
            int maxId = 0;
            foreach (var t in _query.遍历标签树获取标签())
            {
                if (t.Id > maxId) maxId = t.Id;
            }
            // 若无标签则从 1 开始
            return checked(maxId + 1);
        }

        private void 写入标签到Json()
        {
            var flat = _query.遍历标签树获取标签().Select(t => new
            {
                Id = t.Id,
                Name = t.Name,
                ParentId = t.ParentId,
                Category = t.Category,
                NumericValue = t.NumericValue
            }).ToList();

            // 保留 null 值
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include
            };
            var json = JsonConvert.SerializeObject(flat, Formatting.Indented, settings);

            var dir = Path.GetDirectoryName(TagsJsonPath);
            Directory.CreateDirectory(dir ?? ".");

            var tmpPath = Path.Combine(dir ?? ".", Path.GetFileName(TagsJsonPath) + ".tmp");
            var bakPath = Path.Combine(dir ?? ".", Path.GetFileNameWithoutExtension(TagsJsonPath) +
                                                 "_bak_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");

            try
            {
                using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                    sw.Flush();
                    fs.Flush(true);
                }

                if (File.Exists(TagsJsonPath))
                {
                    File.Copy(TagsJsonPath, bakPath, overwrite: true);
                }

                if (File.Exists(TagsJsonPath))
                {
                    File.Delete(TagsJsonPath);
                }
                File.Move(tmpPath, TagsJsonPath);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (File.Exists(tmpPath))
                {
                    try { File.Delete(tmpPath); } catch { /* ignore */ }
                }
            }
        }
    }


}