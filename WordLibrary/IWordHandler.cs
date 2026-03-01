using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.QuestionBank.Domain;
using TagRunner.业务;

namespace WordLibrary
{
    
    /// <summary>
    /// 提供对活动文档的操作接口
    /// </summary>
    public interface IWordHandler
    {

        /// <summary>
        /// 向活动文档插入题目，三种插入模式：在光标处插入、在当前控件前插入、在当前控件后插入
        /// </summary>
        /// <param name="q"></param>
        /// <param name="insP"></param>
        /// <returns></returns>
        ContentControl 插入题目(题目 q,InsertPosition insP);
        void 更新题目();
    }
}
