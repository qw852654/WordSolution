using Microsoft.VisualBasic;
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
using TagRunner;

namespace TagUI
{
    public partial class QuestionCard : UserControl
    {
        private 题目 _ques;

        public QuestionCard(题目 ques)
        {
            InitializeComponent();

            if (ques == null)
            {
                throw new ArgumentNullException(nameof(ques));
            }

            _ques = ques;
            var htmlPath = Path.Combine(静态参数.题库目录, "html", ques.Id.ToString() + ".html");

            if (!string.IsNullOrEmpty(htmlPath) && File.Exists(htmlPath))
            {
                QuestionContent.Navigate(htmlPath);
            }
            else
            {
                QuestionContent.DocumentText = "<html><body><h2>题目内容未找到</h2></body></html>";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Interaction.MsgBox("是否删除题目",Buttons:MsgBoxStyle.OkCancel) == MsgBoxResult.Ok)
            {
                题目服务.Instance.删除题目(_ques);
                
                DeleteCard();

            }
        }

        private void DeleteCard()
        {
            this.Parent.Controls.Remove(this);
        }
    }
}
