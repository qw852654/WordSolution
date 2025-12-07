using System.Collections.Generic;

namespace QuestionBank.Core.Models
{
    public class Question
    {
        public string Id { get; set; }                 // "00001"
        public string Title { get; set; }
        public int Difficulty { get; set; }            // 1..5
        public string Type { get; set; }               // "选择题"/"计算题"
        public List<int> TagIds { get; set; } = new List<int>(); // 叶子标签ID
        public string SourcePath { get; set; }         // source/00001.docx
        public string HtmlPath { get; set; }           // html/00001.html
        public string PdfPath { get; set; }            // pdf/00001.pdf
        public string Status { get; set; }             // "已标注"/"待审核"
    }
}