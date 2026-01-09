using System.Collections.Generic;

namespace TagRunner.业务
{
    /// <summary>
    /// 文件存储抽象：负责题目文件的复制/删除与路径计算。
    /// </summary>
    public interface I文件存储
    {
        /// <summary>
        /// 复制源文件（如 DOCX）到题库 source 目录，按题目 Id 命名并返回目标路径；若目标已存在则覆盖。
        /// </summary>
        string 复制到题库(string 源文件路径, int 题目Id);

        /// <summary>
        /// 获取题目的 Docx 路径（绝对路径）。
        /// </summary>
        string 获取Docx路径(int 题目Id);

        /// <summary>
        /// 获取题目的 Html 路径（绝对路径）。
        /// </summary>
        string 获取Html路径(int 题目Id);

        /// <summary>
        /// 删除题目相关的文件（Docx + Html），返回是否全部删除成功（目标不存在也算成功）。
        /// </summary>
        bool 删除题目文件(int 题目Id);
    }
}
