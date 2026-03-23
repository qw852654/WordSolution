using System;
using System.IO;
using System.Text;
using Aspose.Words;
using Aspose.Words.Loading;
using 题库核心.题目模块.契约;

namespace 题库基础设施.Aspose
{
    public class Aspose题目文档转换器 : I题目文档转换器
    {
        public void 保存Ooxml为题目文件(string Ooxml内容, string 题目文件路径)
        {
            if (string.IsNullOrWhiteSpace(Ooxml内容))
            {
                throw new ArgumentException("Ooxml内容不能为空。", nameof(Ooxml内容));
            }

            if (string.IsNullOrWhiteSpace(题目文件路径))
            {
                throw new ArgumentException("题目文件路径不能为空。", nameof(题目文件路径));
            }

            var 目录路径 = Path.GetDirectoryName(题目文件路径);
            if (!string.IsNullOrWhiteSpace(目录路径))
            {
                Directory.CreateDirectory(目录路径);
            }

            var 文档字节 = Encoding.UTF8.GetBytes(Ooxml内容);
            using var 输入流 = new MemoryStream(文档字节);
            var 加载选项 = new LoadOptions
            {
                LoadFormat = LoadFormat.FlatOpc,
            };

            var 文档 = new Document(输入流, 加载选项);
            文档清理帮助类.清理页眉页脚(文档);
            文档.Save(题目文件路径);
        }

        public string 读取题目文件Ooxml(string 题目文件路径)
        {
            if (string.IsNullOrWhiteSpace(题目文件路径))
            {
                throw new ArgumentException("题目文件路径不能为空。", nameof(题目文件路径));
            }

            var 文档 = new Document(题目文件路径);
            using var 输出流 = new MemoryStream();
            文档.Save(输出流, SaveFormat.FlatOpc);
            return Encoding.UTF8.GetString(输出流.ToArray());
        }
    }
}
