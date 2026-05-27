using 题库核心.标签模块.领域;

namespace 题库应用.内容块模块
{
    public class 内容块标签结果
    {
        public int Id { get; set; }

        public int 标签种类ID { get; set; }

        public string 标签种类名称 { get; set; } = string.Empty;

        public string 名称 { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ParentId { get; set; }

        public bool IsEnabled { get; set; }

        public static 内容块标签结果 从标签(标签 标签, 标签种类? 标签种类)
        {
            return new 内容块标签结果
            {
                Id = 标签.Id,
                标签种类ID = 标签.标签种类ID,
                标签种类名称 = 标签种类?.名称 ?? string.Empty,
                名称 = 标签.名称,
                Description = 标签.Description,
                ParentId = 标签.ParentId,
                IsEnabled = 标签.IsEnabled,
            };
        }
    }
}
