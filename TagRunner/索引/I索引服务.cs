using System.Collections.Generic;

namespace TagRunner.索引
{
    /// <summary>
    /// 索引服务接口：定义索引层对外提供的最小契约。
    /// 业务层通过该接口提交索引任务或查询索引结果。
    /// </summary>
    public interface I索引服务
    {
        /// <summary>
        /// 索引指定题目的 HTML 文件或路径（幂等）。
        /// </summary>
        void 索引文档(int 题目Id, string html路径);

        /// <summary>
        /// 从索引中删除指定题目的文档。
        /// </summary>
        void 删除文档(int 题目Id);

        /// <summary>
        /// 简单搜索，返回匹配的题目 Id 列表（按得分降序）。
        /// </summary>
        List<int> 搜索(string 查询, int topN = 50);

        /// <summary>
        /// 全量重建索引，从指定的 HTML 根目录读取所有文档并重建索引。
        /// </summary>
        void 全量重建(string html根目录);

        /// <summary>
        /// 启动索引服务（例如启动内部资源或线程）。
        /// </summary>
        void 启动();

        /// <summary>
        /// 停止索引服务并释放资源。
        /// </summary>
        void 停止();
    }
}
