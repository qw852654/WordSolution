using Core.QuestionBank.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TagRunner.业务
{
    /// <summary>
    /// 简单的题目业务服务（v0，教学优先）：
    /// - 实现新增题目流程：先写元数据以获取 Id，再复制源文件并转换为 HTML。
    /// - 同步、线性实现，出现错误会以异常方式返回，便于观察问题。
    /// </summary>
    public class 题目服务 
    {
        private readonly 数据.I题目仓储 _题目仓储;
        private readonly I文件存储 _文件存储;
        private readonly 基础.I文档转换器 _文档转换器;

        public 题目服务(数据.I题目仓储 题目仓储, I文件存储 文件存储, 基础.I文档转换器 文档转换器)
        {
            _题目仓储 = 题目仓储 ?? throw new ArgumentNullException(nameof(题目仓储));
            _文件存储 = 文件存储 ?? throw new ArgumentNullException(nameof(文件存储));
            _文档转换器 = 文档转换器 ?? throw new ArgumentNullException(nameof(文档转换器));
        }

        /// <summary>
        /// 新增题目（教学版本）：
        /// 1. 校验输入（必须有至少一个标签与存在的源文件路径）
        /// 2. 构造题目元数据并调用仓储新增，获取题目 Id
        /// 3. 复制源文件到题库（以 Id 命名）并调用转换器生成 HTML
        /// 4. 若转换成功返回新 Id，否则抛出异常（简单处理，便于观察错误）
        /// </summary>
        public int 新增题目(List<标签> 标签集合, string 源文件路径)
        {
            if (标签集合 == null || 标签集合.Count == 0)
                throw new ArgumentException("新增题目需要至少一个标签", nameof(标签集合));
            if (string.IsNullOrWhiteSpace(源文件路径))
                throw new ArgumentException("源文件路径不能为空", nameof(源文件路径));

            // 简单存在性检查：源文件必须存在
            if (!System.IO.File.Exists(源文件路径))
                throw new System.IO.FileNotFoundException("源文件不存在", 源文件路径);

            // 构造题目元数据（仓储会在插入时设置创建/更新时间）
            var model = new 题目
            {
                TagIDs = 标签集合.Select(t => t.Id).ToList(),
                Description = null
            };

            // 先把元数据写入仓储，获取新 Id
            int newId = _题目仓储.新增题目(model);

            // 复制源文件到题库（以 newId 命名）
            var destDocx = _文件存储.复制到题库(源文件路径, newId);

            // 生成 HTML（目标路径由文件存储计算）
            var targetHtml = _文件存储.获取Html路径(newId);
            bool ok = _文档转换器.ConvertToHtml(destDocx, targetHtml);
            if (!ok)
            {
                // 简单处理：抛出异常以便发现问题。生产中应做补偿（删除元数据/文件或记录待重试任务）。
                throw new InvalidOperationException("文档转换为 HTML 失败");
            }

            return newId;
        }

        // 实现标签查题：校验输入并委托给仓储实现，返回非空列表（空表示无结果）
        public List<题目> 标签找题(List<标签> 标签列表)
        {
            // 防御式校验：输入为 null 或空列表时返回空集合，避免上层处理 null
            if (标签列表 == null || 标签列表.Count == 0)
                return new List<题目>();

            // 只保留有效的标签 Id（Id > 0），仓储以 Id 为依据查询
            var 有效标签 = 标签列表.Where(t => t != null && t.Id > 0).ToList();
            if (有效标签.Count == 0)
                return new List<题目>();

            // 直接委托给底层仓储进行查询（仓储负责具体的 SQL 实现，包括 AND/OR 逻辑）
            var results = _题目仓储.按标签查询(有效标签);

            // 保证返回不为 null（习惯性防守），调用者只需处理结果集合
            return results ?? new List<题目>();
        }

        // 其余方法暂不实现（留待后续逐步完成）
        public bool 删除题目(int 题目Id) => throw new NotImplementedException();
        public bool 更新题目(题目 修改题目) => throw new NotImplementedException();
        public void 设置题目标签(int 题目Id, System.Collections.Generic.IEnumerable<int> 标签Ids) => throw new NotImplementedException();

        public string 获取题目文档路径(题目 q)
        {
            var path = _文件存储.获取Docx路径(q.Id);
            return path;
        }
    }
}
