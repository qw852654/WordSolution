using System;

namespace Word本地文件操作核心库.工具
{
    public static class 匹配组卷文档
    {
        public static bool 是否匹配(string 文件名)
        {
            if (string.IsNullOrWhiteSpace(文件名)) return false;
            if (文件名.StartsWith("~$")) return false;

            return 文件名.Contains("高中物理作业") || 文件名.Contains("初中物理作业");
        }
    }
}
