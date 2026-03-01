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
using TagRunner.业务;

namespace UI.基础控件
{
    public partial class 标签联想控件 : UserControl
    {
        private I标签服务 _标签服务;
        private List<标签> _已选排除 = new List<标签>();

       

        public event EventHandler<标签> TagSelected;

        public 标签联想控件(I标签服务 标签服务)
        {
            InitializeComponent();
            this.listBox1.Visible = false;
       _标签服务 = 标签服务; }

        

        // 允许父控件设置当前已选标签，以便在联想时排除
        public void 设置已选排除(List<标签> 已选)
        {
            _已选排除 = 已选 ?? new List<标签>();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var 输入文本 = this.textBox1.Text;
            if (_标签服务 == null)
            {
                更新联想列表(null);
                return;
            }

            List<标签> 联想结果 = _标签服务.联想标签(输入文本);
            var 过滤后 = (联想结果 ?? new List<标签>()).FindAll(t => !_已选排除.Exists(s => s.Id == t.Id));
            更新联想列表(过滤后);
        }

        private void 更新联想列表(List<标签> 联想结果)
        {
            if (联想结果 == null || 联想结果.Count == 0)
            {
                this.listBox1.Visible = false;
                this.listBox1.Items.Clear();
                return;
            }

            this.listBox1.Visible = true;
            this.listBox1.Items.Clear();
            foreach (var tag in 联想结果)
            {
                this.listBox1.Items.Add(tag);
            }
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.listBox1.SelectedItem is 标签 selected)
            {
                TagSelected?.Invoke(this, selected);
                // 清空输入并隐藏联想列表
                this.textBox1.Clear();
                更新联想列表(null);
                this.textBox1.Focus();
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && this.listBox1.SelectedItem is 标签 selected)
            {
                e.Handled = true;
                TagSelected?.Invoke(this, selected);
                this.textBox1.Clear();
                更新联想列表(null);
                this.textBox1.Focus();
            }
        }
    }
}
