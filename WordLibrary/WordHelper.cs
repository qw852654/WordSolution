using System;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace WordLibrary
{
    public class word程序
    {
        private Application _wordApp;

        public word程序(Application 传入的word程序对象)
        {
            _wordApp = 传入的word程序对象;
        }

        public word程序(string 打开的文件路径)
        {
            _wordApp = new Application();
            _wordApp.Visible = true;
        }

        public void CreateDocument()
        {
            _wordApp.Documents.Add();
        }

        public Document 获取当前文档()
        {
            Document doc = _wordApp.ActiveDocument;
            return doc;
        }

        string 保存路径;

        public void CreateDocument(string filePath)
        {
            Document doc = _wordApp.Documents.Add();
            保存路径 = filePath;
        }






        public void CloseWord()
        {
            _wordApp.Quit();
        }
    }

    public class 文档
    {
        Document _document;

        //创建构造函数,接受document对象
        public 文档(Document 传入的文档对象)
        {
            _document = 传入的文档对象;
        }


        public Document 获取文档()
        {
            return _document;
        }

        public void 设置例题()
        {
            var doc = _document;
            Regex regex = new Regex(@"^\d{1,2}[．.]", RegexOptions.Compiled);

            foreach (Word.Paragraph para in doc.Paragraphs)
            {
                // 跳过表格内的段落
                if (para.Range.get_Information(Word.WdInformation.wdWithInTable))
                    continue;

                // 检查段落样式
                if (para.get_Style() is Word.Style style && style.NameLocal != "正文")
                    continue;

                // 提取段落文本（去除末尾回车符）
                string text = para.Range.Text;
                if (text.EndsWith("\r") || text.EndsWith("\n"))
                    text = text.Substring(0, text.Length - 1);

                var match = regex.Match(text);
                if (match.Success)
                {
                    // 删除匹配内容
                    var rng = doc.Range(para.Range.Start, para.Range.Start + match.Length);
                    rng.Delete();

                    // 应用新样式
                    para.set_Style("例题");
                }
            }
        }
        
        //创建一个方法,删除当前活动文档中的所有样式为答案的段落
        public void 删除答案(string[] 待删除的样式列表)
        {


            Document doc = _document;
            foreach (Paragraph para in doc.Paragraphs)
            {
                foreach (string 样式 in 待删除的样式列表)
                {
                    if (para.get_Style().NameLocal == 样式)
                    {
                        para.Range.Delete();
                        break;
                    }
                }
            }
        }

        public void 导出pdf到文档目录(string 保存的文件路径)
        {
            Document doc = _document;
            string pdfPath = 保存的文件路径;

            doc.ExportAsFixedFormat(
                pdfPath,
                WdExportFormat.wdExportFormatPDF,
                OpenAfterExport: false,
                OptimizeFor: WdExportOptimizeFor.wdExportOptimizeForPrint,
                Range: WdExportRange.wdExportAllDocument,
                IncludeDocProps: true,
                KeepIRM: true,
                CreateBookmarks: WdExportCreateBookmarks.wdExportCreateHeadingBookmarks,
                DocStructureTags: true,
                BitmapMissingFonts: true,
                UseISO19005_1: true
            );
        }

        public void 导出原始文档()
        {
            Document doc = _document;

            // 获取当前文档路径
            string originalPath = doc.FullName;



            if (originalPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // 替换网络路径为本地路径（如果适用）  
                string oneDriveLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\OneDrive";
                // 先将原路径转为小写再替换
                originalPath = originalPath.ToLowerInvariant();
                originalPath = originalPath.Replace("https://d.docs.live.net/7772308a8c2039d3/", oneDriveLocalPath + @"\");

                // 规范路径格式，将所有斜杠替换为反斜杠  
                originalPath = originalPath.Replace("/", "\\");
            }
            // 生成临时目录路径
            string tempDirectory = System.IO.Path.GetTempPath();
            string tempFilePath = System.IO.Path.Combine(tempDirectory, System.IO.Path.GetFileName(originalPath));

            string 文件路径 = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(originalPath), System.IO.Path.GetFileNameWithoutExtension(originalPath) + ".pdf");

            //导出文档
            this.导出pdf到文档目录(文件路径);

            

            //导出之后弹出提示
            System.Windows.Forms.MessageBox.Show($"导出完成");

            // 删除临时文件
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }
        }
        public void 导出无答案pdf(string[] 待删除的样式列表)
        {
            Document doc = _document;

            // 获取当前文档路径
            string originalPath = doc.FullName;

            

            if (originalPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // 替换网络路径为本地路径（如果适用）  
                string oneDriveLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\OneDrive";
                // 先将原路径转为小写再替换
                originalPath = originalPath.ToLowerInvariant();
                originalPath = originalPath.Replace("https://d.docs.live.net/7772308a8c2039d3/", oneDriveLocalPath + @"\");

                // 规范路径格式，将所有斜杠替换为反斜杠  
                originalPath = originalPath.Replace("/", "\\");
            }
            // 生成临时目录路径
            string tempDirectory = System.IO.Path.GetTempPath();
            string tempFilePath = System.IO.Path.Combine(tempDirectory, System.IO.Path.GetFileName(originalPath));

            string 文件路径 = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(originalPath), System.IO.Path.GetFileNameWithoutExtension(originalPath) + "_无答案.pdf");

            // 将文档对应的文件复制到临时目录
            System.IO.File.Copy(originalPath, tempFilePath, true);

            // 在新文档中打开临时文件
            Document tempDoc = doc.Application.Documents.Open(tempFilePath);

            // 删除答案
            文档 temp文档 = new 文档(tempDoc);
            temp文档.删除答案(待删除的样式列表);


            // 导出为PDF
            temp文档.导出pdf到文档目录(文件路径);



            // 关闭临时文档
            tempDoc.Close(false);

            //导出之后弹出提示
            System.Windows.Forms.MessageBox.Show($"导出完成");

            // 删除临时文件
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }


        }

        public void 根据底纹设置答案()
        {
            //检查每个段落底纹的填充,如果填充不是无颜色,则将该段落的样式设置为答案
            Document doc = _document;
            foreach (Paragraph para in doc.Paragraphs)
            {
                if (para.Shading.BackgroundPatternColor != WdColor.wdColorAutomatic)
                {
                    //设置段落样式为答案
                    para.set_Style("答案");
                }
            }
        }

        public void 根据字体颜色设置答案()
        {
            //检查每个样式为正文的段落的字体颜色,如果字体颜色是红色,则将该段落的样式设置为答案
            Document doc = _document;
            foreach (Paragraph para in doc.Paragraphs)
            {
                //如果段落的样式为正文或纯文本，并且字体颜色为红色
                if (para.get_Style().NameLocal == "正文" || para.get_Style().NameLocal == "纯文本")
                {
                    if (para.Range.Font.Color == WdColor.wdColorRed)
                    {
                        //设置段落样式为答案
                        para.set_Style("答案");
                    }
                }


               
                
            }

            //用msgbox提示完成
            System.Windows.Forms.MessageBox.Show($"完成答案设置");
        }



        public void 导入模板样式并更新文档(bool 弹出提示)
        {
            try
            {
                string templatePath = @"C:\Users\BOX\AppData\Roaming\Microsoft\Templates\Normal.dotm";

                // 安全验证
                if (!File.Exists(templatePath))
                {
                    MessageBox.Show("错误：找不到模板文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }



                // 导入样式（不依赖打开模板）
                _document.CopyStylesFromTemplate(templatePath);

                // 全文档更新策略
                foreach (Field field in _document.Fields)
                {
                    field.Update();
                }

                foreach (TableOfContents toc in _document.TablesOfContents)
                {
                    toc.Update();
                }

                foreach (TableOfFigures tableOfFigures in _document.TablesOfFigures)
                {
                    tableOfFigures.Update();
                }

                foreach (Section section in _document.Sections)
                {
                    foreach (HeaderFooter header in section.Headers)
                    {
                        foreach (Field field in header.Range.Fields)
                        {
                            field.Update();
                        }
                    }

                    foreach (HeaderFooter footer in section.Footers)
                    {
                        foreach (Field field in footer.Range.Fields)
                        {
                            field.Update();
                        }
                    }
                }

                //将当前文档的页面边距设为上下左右均为1.27厘米
                _document.PageSetup.RightMargin = _document.Application.CentimetersToPoints(1.5f);
                _document.PageSetup.BottomMargin = _document.Application.CentimetersToPoints(1.5f);
                _document.PageSetup.TopMargin = _document.Application.CentimetersToPoints(1.5f);
                _document.PageSetup.LeftMargin = _document.Application.CentimetersToPoints(1.5f);

                if (弹出提示==true)
                {
                    MessageBox.Show("样式更新完成！推荐操作：\n1. 按Ctrl+A全选检查样式格式\n2. 更新目录（若存在）", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误代码：{ex.HResult}\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _document.Application.ScreenUpdating = true;
            }
        }

        //将选中内容复制到新文档中,然后删除答案,最后到处,导出的目录是桌面,格式是pdf,文件名弹出对话框要求用户输入




    }
}

