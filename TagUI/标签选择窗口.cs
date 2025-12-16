using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagRunner;

namespace TagUI
{
    /// <summary>
    /// DialogResult 为 OK 时，通过 标签列表 获取用户选择的标签集合。
    /// </summary>
    public partial class 标签选择窗口 : Form
    {
        private List<标签> resultTags;
        public IReadOnlyList<标签> 标签列表 => resultTags;

        public 标签选择窗口(标签查询服务 查询器)
        {
            InitializeComponent();
            加载标签(查询器);
            resultTags = new List<标签>();

        }

        private void 加载标签(标签查询服务 查询器)
        {
            var 标签列表 = 查询器.标签树根;
            if(标签列表.Count ==0)
            {
                MessageBox.Show("没有可供选择的标签，请先创建标签！");
                this.Close();
                return;
            }
            if (标签列表.Count >0)
            {
                flowLayoutPanel1.Controls.Clear();
                foreach (var tag in 标签列表)
                {
                    var tagControl = new TagUI.用户控件.标签树控件(tag);
                    tagControl.Height=flowLayoutPanel1.Height - 20;
                    flowLayoutPanel1.Controls.Add(tagControl);
                }
            }
        }

        private void 退出按钮_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void 确定按钮_Click(object sender, EventArgs e)
        {
            foreach (var control in flowLayoutPanel1.Controls)
            {
                if (control is 用户控件.标签树控件 tagControl)
                {
                    var selectedTag = tagControl.获取选中的标签();
                    if (selectedTag.Count>0)
                    {
                        foreach(标签 tag in selectedTag)
                        {
                            resultTags.Add(tag);
                        }
                    }
                }
                
            }

            this.DialogResult = DialogResult.OK;
            this.Close();

        }
    }
}
