using System;
using System.Collections.Generic;
using System.Linq;

namespace 题库核心.试卷导入模块.领域
{
    public class 导入试卷会话
    {
        public string SessionId { get; set; } = string.Empty;

        public string 题库键 { get; set; } = string.Empty;

        public int 试卷记录ID { get; set; }

        public string 试卷显示名称 { get; set; } = string.Empty;

        public int 年份标签ID { get; set; }

        public int 来源标签ID { get; set; }

        public string 模板类型 { get; set; } = "当前模板";

        public int 当前索引 { get; set; }

        public List<导入试卷草稿题> 草稿题列表 { get; set; } = new();

        public static 导入试卷会话 创建(
            string sessionId,
            string 题库键,
            int 试卷记录ID,
            string 试卷显示名称,
            int 年份标签ID,
            int 来源标签ID,
            IEnumerable<导入试卷草稿题> 草稿题列表)
        {
            return new 导入试卷会话
            {
                SessionId = sessionId,
                题库键 = 题库键,
                试卷记录ID = 试卷记录ID,
                试卷显示名称 = 试卷显示名称,
                年份标签ID = 年份标签ID,
                来源标签ID = 来源标签ID,
                草稿题列表 = 草稿题列表.ToList(),
                当前索引 = 0,
            };
        }

        public 导入试卷草稿题? 获取当前草稿题()
        {
            推进到下一个未跳过草稿();
            if (当前索引 < 0 || 当前索引 >= 草稿题列表.Count)
            {
                return null;
            }

            return 草稿题列表[当前索引];
        }

        public void 确认当前题并前进()
        {
            当前索引++;
            推进到下一个未跳过草稿();
        }

        public void 跳过当前题并前进()
        {
            var 当前题 = 获取当前草稿题();
            if (当前题 == null)
            {
                return;
            }

            当前题.已跳过 = true;
            当前索引++;
            推进到下一个未跳过草稿();
        }

        public bool 是否已完成()
        {
            return 获取当前草稿题() == null;
        }

        public int 获取剩余未处理数量()
        {
            推进到下一个未跳过草稿();
            if (当前索引 >= 草稿题列表.Count)
            {
                return 0;
            }

            return 草稿题列表.Skip(当前索引).Count(草稿题 => !草稿题.已跳过);
        }

        private void 推进到下一个未跳过草稿()
        {
            while (当前索引 < 草稿题列表.Count && 草稿题列表[当前索引].已跳过)
            {
                当前索引++;
            }
        }
    }
}
