using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.QuestionBank.Domain;

namespace UI.基础控件
{
    public partial class 标签控件_仅展示 : UserControl
    {
        public 标签控件_仅展示(标签 待展示标签)
        {
            InitializeComponent();
            if (待展示标签 != null)
            {
                label1.Text = 待展示标签.Name;
            }

        }


    }
}
