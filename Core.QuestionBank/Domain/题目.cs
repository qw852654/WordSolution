using System;
using System.Collections.Generic;

namespace Core.QuestionBank.Domain
{
    public class ÌâÄ¿
    {
        public int Id { get; set; }
        public List<int> TagIDs { get; set; } = new List<int>();
        public DateTime CreatedTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Description { get; set; } 
        
    }
}
