using System;
using 题库核心.内容块模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块编辑会话结果
    {
        public string 会话ID { get; set; } = string.Empty;

        public string 题库键 { get; set; } = string.Empty;

        public int 内容块ID { get; set; }

        public string 编辑文件路径 { get; set; } = string.Empty;

        public int? 基准版本ID { get; set; }

        public int? 最新版本ID { get; set; }

        public int? 最新版本号 { get; set; }

        public 内容块编辑会话状态 状态 { get; set; }

        public string? 消息 { get; set; }

        public string? 错误信息 { get; set; }

        public DateTime 创建时间 { get; set; }

        public DateTime? 打开时间 { get; set; }

        public DateTime? 最近检测时间 { get; set; }

        public DateTime? 同步时间 { get; set; }

        public DateTime? 取消时间 { get; set; }

        public int 稳定检测次数 { get; set; }

        public bool 最近检测锁文件存在 { get; set; }

        public bool 最近检测可独占打开 { get; set; }

        public static 内容块编辑会话结果 从会话(内容块编辑会话 会话)
        {
            return new 内容块编辑会话结果
            {
                会话ID = 会话.会话ID,
                题库键 = 会话.题库键,
                内容块ID = 会话.内容块ID,
                编辑文件路径 = 会话.编辑文件路径,
                基准版本ID = 会话.基准版本ID,
                最新版本ID = 会话.最新版本ID,
                最新版本号 = 会话.最新版本号,
                状态 = 会话.状态,
                消息 = 会话.消息,
                错误信息 = 会话.错误信息,
                创建时间 = 会话.创建时间,
                打开时间 = 会话.打开时间,
                最近检测时间 = 会话.最近检测时间,
                同步时间 = 会话.同步时间,
                取消时间 = 会话.取消时间,
                稳定检测次数 = 会话.稳定检测次数,
                最近检测锁文件存在 = 会话.最近检测锁文件存在,
                最近检测可独占打开 = 会话.最近检测可独占打开,
            };
        }
    }
}
