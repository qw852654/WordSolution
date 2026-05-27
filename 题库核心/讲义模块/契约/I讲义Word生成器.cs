using System.Collections.Generic;

namespace 题库核心.讲义模块.契约
{
    public interface I讲义Word生成器
    {
        void 生成(string 标题, IReadOnlyList<讲义生成源文件> 源文件列表, string 输出文件路径);
    }

    public class 讲义生成源文件
    {
        public string 标题 { get; set; } = string.Empty;

        public string 角色 { get; set; } = string.Empty;

        public string 文件路径 { get; set; } = string.Empty;
    }
}
