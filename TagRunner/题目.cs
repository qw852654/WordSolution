using System.Collections.Generic;

namespace TagRunner
{
    public class 题目
    {
        public string Id { get; set; }                 // "00001"
        public string Title { get; set; }
        public List<int> TagIds { get; set; } = new List<int>(); // 叶子标签ID
        public string SourcePath { get; set; }         // source/00001.docx
        public string HtmlPath { get; set; }           // html/00001.html
        public string PdfPath { get; set; }            // pdf/00001.pdf
        public string Status { get; set; }             // "已标注"/"待审核"
    }
}
