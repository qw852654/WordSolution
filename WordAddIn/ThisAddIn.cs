using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using WordLibrary;
using System.IO;

namespace WordAddIn
{
    public partial class ThisAddIn
    {
        private FileSystemWatcher watcher;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // 订阅文档打开事件
            //处理下载文件夹重命名文件的操作
            watcher = new FileSystemWatcher();
            watcher.Path = @"E:\下载";
            watcher.Filter = "*.docx";
            watcher.Renamed+= 重命名文件时的操作;
            watcher.EnableRaisingEvents = true;

            // 自动隐藏页间空白
            this.Application.DocumentOpen += 隐藏页间空白;
            this.Application.DocumentOpen += 开启导航窗格;
            ((Word.ApplicationEvents4_Event)this.Application).NewDocument += 隐藏页间空白;

        }

        private void Application_DocumentOpen(Word.Document Doc)
        {
            throw new NotImplementedException();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // 取消订阅事件

        }

        private void 重命名文件时的操作(object sender, FileSystemEventArgs e)
        {
            // 检查文件名是否包含指定关键字  
            string fileName = e.Name;
            if (fileName.StartsWith("~$")) return; // 忽略临时文件

            if (fileName.Contains("高中物理作业") || fileName.Contains("初中物理作业"))
            {
                //记录当前文档的光标range
                Word.Document activeDoc=this.Application.ActiveDocument;
                Word.Range insertRange=this.Application.Selection.Range;

                // Word操作需要在主线程执行  
                Word.Document sourceDoc= 处理刚下载的组卷文档(e.FullPath);
                if (sourceDoc != null)
                {
                    insertRange.FormattedText=sourceDoc.Content.FormattedText; // 将处理后的内容插入到当前光标位置
                    //等待1.5秒
                    sourceDoc.Close(false);
                    insertRange.Collapse(Word.WdCollapseDirection.wdCollapseStart);
                }
                

            }
        }

        //隐藏页间空白
        private void 隐藏页间空白(Word.Document doc)
        {
            try
            {
                if (doc.ActiveWindow != null)
                {
                    // 修复错误：将 ShowWhiteSpace 替换为 DisplayPageBoundaries  
                    doc.ActiveWindow.View.DisplayPageBoundaries = false;
                }
            }
            catch { }
        }

        // Fix for CS1061: The 'View' object does not contain a definition for 'OutlineLevel'.
        // The error occurs because 'OutlineLevel' is not a property of the 'View' object.
        // Instead, you can use the 'OutlineView' property of the 'View' object to achieve the desired functionality.

        private void 开启导航窗格(Word.Document doc)
        {
            try
            {
                var navigationPane = doc.Application.CommandBars["Navigation"];
                if (navigationPane != null)
                {
                    navigationPane.Visible = true;
                }
            }
            catch { }
        }

        // 处理文档的具体逻辑
        private Word.Document 处理刚下载的组卷文档(string filePath)
        {
            Word.Document doc = null;
            try
            {
                doc = this.Application.Documents.Open(filePath);

                // 关闭导航窗格
                this.Application.CommandBars["Navigation"].Visible = false;

                // 删除前三个段落
                doc.Range(doc.Paragraphs[1].Range.Start, doc.Paragraphs[3].Range.End).Delete();

                // 使用Find方法将"^p^p"替换成"^p"，只替换一次
                Word.Range range = doc.Content;
                Word.Find find = range.Find;
                find.ClearFormatting();
                find.Text = "^p^p";
                find.Replacement.ClearFormatting();
                find.Replacement.Text = "^p";
                find.Forward = true;
                find.Wrap = Word.WdFindWrap.wdFindStop;
                find.Format = false;
                find.MatchCase = false;
                find.MatchWholeWord = false;
                find.MatchWildcards = false;
                find.MatchSoundsLike = false;
                find.MatchAllWordForms = false;

                object replaceOne = Word.WdReplace.wdReplaceAll;
                find.Execute(Replace: ref replaceOne);

                

                //删除题型
                // 删除所有加粗且匹配 [一二三四五]*^13 的内容
                Word.Range searchRange = doc.Content;
                Word.Find boldFind = searchRange.Find;
                boldFind.ClearFormatting();
                boldFind.Font.Bold = 1; // 只查找加粗内容
                boldFind.Text = "[一二三四五]*^13";
                boldFind.Replacement.ClearFormatting();
                boldFind.Replacement.Text = "";
                boldFind.Forward = true;
                boldFind.Wrap = Word.WdFindWrap.wdFindStop;
                boldFind.Format = true;
                boldFind.MatchWildcards = true;

                object replaceAll = Word.WdReplace.wdReplaceAll;
                boldFind.Execute(Replace: ref replaceAll);

                文档 文档对象 = new 文档(doc);
                文档对象.导入模板样式并更新文档(false); // 更新文档样式
                文档对象.根据底纹设置答案(); // 根据底纹设置答案
                文档对象.设置例题(); // 设置例题


                return 文档对象.获取文档();

                //doc.Save();
                // doc.Close(); // 如需自动关闭文档可取消注释
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("处理文档时出错: " + ex.Message);
                return null;
            }
        }



        #region VSTO 生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion

    }
}
