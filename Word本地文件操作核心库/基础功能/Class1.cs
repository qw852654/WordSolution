using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using System.Text.RegularExpressions;

namespace Word本地文件操作核心库
{
    internal class Class1
    {
        public static string 获取文档路径(Document doc)
        {
            string docPath;

            if (doc == null) {throw new ArgumentNullException("待获取路径的文档不存在"); }

            docPath = System.IO.Path.GetDirectoryName(doc.FullName);

            docPath = 转换成本地路径(docPath);

            return docPath;

        }

        private string 转换成本地路径(string docPath)
        {
            if (docPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // 替换网络路径为本地路径（如果适用）  
                string oneDriveLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\OneDrive";
                // 先将原路径转为小写再替换
                docPath = docPath.ToLowerInvariant();
                

                // 规范路径格式，将所有斜杠替换为反斜杠  
                docPath = docPath.Replace("/", "\\");
            }
        }
    }



    public static class 常量
    {
        public static string onedrivePath = @"C:\Users\BOX\OneDrive\";
    }
}
