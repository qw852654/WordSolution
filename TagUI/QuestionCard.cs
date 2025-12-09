using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TagUI
{
    public partial class QuestionCard : UserControl
    {

        public QuestionCard(string htmlPath)
        {
            InitializeComponent();

            if(!string.IsNullOrEmpty(htmlPath)&& File.Exists(htmlPath))
            {
                QuestionContent.Navigate(htmlPath);
            }
            else
            {
                QuestionContent.DocumentText = "<html><body><h2>题目内容未找到</h2></body></html>";
             }

        }
    }
}
