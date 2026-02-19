using Microsoft.Office.Interop.Word;
using System;
using System.ComponentModel;
using System.IO;
using TagRunner.Models;
using TagRunner.业务;

namespace WordLibrary
{
    public class WordHandler : IWordHandler
    {
        题库应用服务集 _服务集;
        Application _curApp;

        // 保留原有初始化方法签名（若项目中另有初始化约定请保持一致）
        public WordHandler(题库应用服务集 服务集, Application curApp)
        {
            this._服务集 = 服务集;
            this._curApp = curApp;
        }

        public ContentControl 插入题目(题目 q, InsertPosition insP)
        {
            var selRng=_curApp.Selection.Range;
            Range insertRng = null;
            ContentControl ct;
            var docxPath=_服务集.题目服务.获取题目文档路径(q);
            if (!File.Exists(docxPath))
            {
                throw new FileNotFoundException("题目文档未找到", docxPath);
            }
            else
            {
                switch (insP)
                {
                    case InsertPosition.AtSelection:
                        selRng.InsertParagraphAfter();
                        insertRng = selRng.Paragraphs[1].Next().Range;
                        break;
                    case InsertPosition.Before:
                        var rng=findCurrentControlRange(selRng.Start);
                        if(rng==null)
                        {
                            throw new InvalidOperationException("光标位置不在题目控件内，无法在题目控件前插入新题目");
                        }
                        else
                        {
                            rng.Collapse(WdCollapseDirection.wdCollapseStart);
                            rng.Move(WdUnits.wdCharacter, -1);
                            rng.InsertParagraphBefore();
                            insertRng = rng;
                        }
                        break;
                    case InsertPosition.After:
                        rng = findCurrentControlRange(selRng.Start);
                        if (rng == null)
                        {
                            throw new InvalidOperationException("光标位置不在题目控件内，无法在题目控件前插入新题目");
                        }
                        else
                        {
                            rng.Collapse(WdCollapseDirection.wdCollapseEnd);
                            var r = rng.Duplicate;
                            rng.Move(WdUnits.wdCharacter, 1);
                            rng.InsertParagraphAfter();
                            insertRng = r.Paragraphs[1].Next().Range;
                        }
                        break;

                }
                if (insertRng==null)
                    throw new InvalidOperationException("无法确定插入位置");
                var Doc = _curApp.Documents.Open(docxPath,ReadOnly:true,Visible:false);
                ct = insertRng.ContentControls.Add(WdContentControlType.wdContentControlRichText);
                ct.Range.FormattedText = Doc.Content;
                ct.Range.Paragraphs.Last.Range.Delete();
                var ctRng=ct.Range.Paragraphs.Last.Range;
                ctRng.Collapse(WdCollapseDirection.wdCollapseEnd);
                ctRng.Select();
                Doc.Close();
                ct.Title= $"题目:{q.Description}";
                ct.Tag = $"ID:{q.Id}";
                return ct;
            }
        }



        private Range findCurrentControlRange(int pointPosition)
        {
            foreach (ContentControl cc in _curApp.ActiveDocument.ContentControls)
            {
                if (cc.Range.Start <= pointPosition && cc.Range.End >= pointPosition)
                {
                    return cc.Range;
                }
                
            }
            return null;
        }

        public void 更新题目()
        {
            throw new NotImplementedException();
        }
    }

    public enum InsertPosition
    {
        Before,
        After,
        AtSelection
    }
}
