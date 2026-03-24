using System.Collections.Generic;
using 题库核心.试卷导入模块.领域;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 试卷解析结果
    {
        public string 显示名称 { get; set; } = string.Empty;

        public IReadOnlyList<导入试卷草稿题> 草稿题列表 { get; set; } = new List<导入试卷草稿题>();
    }
}
