using System;

namespace 题库核心.内容块模块.领域
{
    public class 编辑文件检测结果
    {
        public bool 文件存在 { get; set; }

        public bool 锁文件存在 { get; set; }

        public bool 可独占打开 { get; set; }

        public long? 文件长度 { get; set; }

        public DateTime? 最后写入时间Utc { get; set; }
    }
}
