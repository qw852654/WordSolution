using System;
using System.Collections.Generic;

namespace 题库应用.讲义模块
{
    public class 讲义结构树结果
    {
        public string 节点ID { get; set; } = string.Empty;

        public string 节点类型 { get; set; } = string.Empty;

        public int? 目标ID { get; set; }

        public string 标题 { get; set; } = string.Empty;

        public string? 摘要 { get; set; }

        public string? 状态 { get; set; }

        public int? 章节标签ID { get; set; }

        public string? 章节名称 { get; set; }

        public string? 内容类型 { get; set; }

        public string? 结构类型 { get; set; }

        public bool 是否允许子块 { get; set; }

        public int? 当前版本ID { get; set; }

        public int? 当前版本号 { get; set; }

        public string? 来源类型 { get; set; }

        public int? 来源ID { get; set; }

        public int? 父目标ID { get; set; }

        public string? 角色 { get; set; }

        public string? 引用版本模式 { get; set; }

        public int? 引用版本ID { get; set; }

        public int? 引用版本号 { get; set; }

        public int 排序 { get; set; }

        public int 深度 { get; set; }

        public int 子节点数量 { get; set; }

        public bool 是否错误 { get; set; }

        public string? 错误信息 { get; set; }

        public DateTime? 更新时间 { get; set; }

        public IList<string> 可执行操作 { get; set; } = new List<string>();

        public IList<讲义结构树结果> 子节点列表 { get; set; } = new List<讲义结构树结果>();
    }
}
