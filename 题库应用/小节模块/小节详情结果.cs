using System;
using 题库核心.小节模块.领域;
using 题库核心.标签模块.领域;

namespace 题库应用.小节模块
{
    public class 小节详情结果
    {
        public int Id { get; set; }

        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public int? 章节标签ID { get; set; }

        public string? 章节名称 { get; set; }

        public 小节状态 状态 { get; set; }

        public int 项目数量 { get; set; }

        public int 知识点数量 { get; set; }

        public int 例题数量 { get; set; }

        public int 练习数量 { get; set; }

        public DateTime 创建时间 { get; set; }

        public DateTime 更新时间 { get; set; }

        public static 小节详情结果 从小节(小节 小节, 标签? 章节标签, 小节项目统计 统计)
        {
            return new 小节详情结果
            {
                Id = 小节.Id,
                标题 = 小节.标题,
                摘要 = 小节.摘要,
                章节标签ID = 小节.章节标签ID,
                章节名称 = 章节标签?.名称,
                状态 = 小节.状态,
                项目数量 = 统计.项目数量,
                知识点数量 = 统计.知识点数量,
                例题数量 = 统计.例题数量,
                练习数量 = 统计.练习数量,
                创建时间 = 小节.创建时间,
                更新时间 = 小节.更新时间,
            };
        }
    }

    public class 小节项目统计
    {
        public int 项目数量 { get; set; }

        public int 知识点数量 { get; set; }

        public int 例题数量 { get; set; }

        public int 练习数量 { get; set; }
    }
}
