using System.Collections.Generic;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 答案元信息
    {
        public string 原始难度文本 { get; set; } = string.Empty;

        public IReadOnlyList<string> 原始知识点列表 { get; set; } = new List<string>();
    }
}
