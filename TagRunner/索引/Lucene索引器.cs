using System;
using System.Collections.Generic;

namespace TagRunner.索引
{
    /// <summary>
    /// Lucene 索引器骨架：负责管理 IndexWriter/IndexReader，并提供基本的索引与查询方法。
    /// 目前为骨架实现，具体分词和写入策略将在后续提交中实现。
    /// </summary>
    public class Lucene索引器 : I索引服务
    {
        private readonly string _索引目录;

        public Lucene索引器(string 索引目录)
        {
            _索引目录 = 索引目录 ?? throw new ArgumentNullException(nameof(索引目录));
        }

        public void 启动()
        {
            // TODO: 初始化 IndexWriter/Analyzer 等资源
        }

        public void 停止()
        {
            // TODO: 释放 IndexWriter/Reader 资源
        }

        public void 索引文档(int 题目Id, string html路径)
        {
            // TODO: 读取 HTML，提取文本并写入 Lucene 索引
            throw new NotImplementedException();
        }

        public void 删除文档(int 题目Id)
        {
            // TODO: 从索引中删除文档
            throw new NotImplementedException();
        }

        public List<int> 搜索(string 查询, int topN = 50)
        {
            // TODO: 执行查询并返回匹配的题目 Id 列表
            throw new NotImplementedException();
        }

        public void 全量重建(string html根目录)
        {
            // TODO: 遍历 html 根目录，逐个索引文档
            throw new NotImplementedException();
        }
    }
}
