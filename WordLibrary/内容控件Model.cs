using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagRunner.Models;
using TagRunner.业务;

namespace WordLibrary
{
    public class 内容控件model
    {
        public ContentControl 内容控件 { get; set; }
        public 题目 题目 { get; set; }
        public List<string> tagNames { get; set; }
        public bool isCrossPage { get; set; } = true;
        

        

        public 内容控件model(题目 q, ContentControl Control,I标签服务 tagServer)
        {
            题目 = q;
            内容控件 = Control;
            var tags = q.TagIDs.Select(id => tagServer.ID找标签(id)).ToList();
            tagNames = new List<string>();
            foreach (var tag in tags)
            {
                tagNames.Add(tag.Name);
            }
        }
    }
}
