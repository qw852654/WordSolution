using System.Collections.Generic;

namespace QuestionBank.Core.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }       // null ±íÊ¾¸ù
        public List<Tag> Children { get; set; } = new List<Tag>();
    }
}