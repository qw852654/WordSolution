using System;
using 题库核心.讲义模块.领域;

namespace 题库应用.讲义模块
{
    public class 讲义详情结果
    {
        public int Id { get; set; }

        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public 讲义状态 状态 { get; set; }

        public int 项目数量 { get; set; }

        public int? 最新生成记录ID { get; set; }

        public DateTime? 最新生成时间 { get; set; }

        public DateTime 创建时间 { get; set; }

        public DateTime 更新时间 { get; set; }

        public static 讲义详情结果 从讲义(讲义 讲义, int 项目数量, 讲义生成记录? 最新生成记录)
        {
            return new 讲义详情结果
            {
                Id = 讲义.Id,
                标题 = 讲义.标题,
                摘要 = 讲义.摘要,
                状态 = 讲义.状态,
                项目数量 = 项目数量,
                最新生成记录ID = 最新生成记录?.Id,
                最新生成时间 = 最新生成记录?.生成时间,
                创建时间 = 讲义.创建时间,
                更新时间 = 讲义.更新时间
            };
        }
    }
}
