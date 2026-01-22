using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagRunner.Models;

namespace UI.基础控件
{
    public partial class 多选标签控件 : UserControl
    {
        private List<标签> _已选标签 = new List<标签>();

        // 当已选标签集合变化时通知外部
        public event EventHandler SelectedChanged;

        public 多选标签控件()
        {
            InitializeComponent();
        }

        // Add a tag to the selected area
        public void 增加选中标签(标签 待加入标签)
        {
            if (待加入标签 == null) return;

            if (_已选标签.Exists(t => t.Id == 待加入标签.Id)) return;

            var 控件 = new 标签控件_右键点击删除(待加入标签);
            控件.Dock = DockStyle.Top;
            控件.DeleteRequested += 响应内部标签删除请求;

            _已选标签.Add(待加入标签);
            this.flowLayoutPanel1.Controls.Add(控件);

            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void 响应内部标签删除请求(object sender, 标签 e)
        {
            if (sender is Control c)
            {
                if (this.flowLayoutPanel1.Controls.Contains(c))
                {
                    this.flowLayoutPanel1.Controls.Remove(c);
                    c.Dispose();
                }
            }

            if (e != null)
            {
                _已选标签.RemoveAll(t => t.Id == e.Id);
            }

            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        // Clear all selected tags
        public void 清空()
        {
            var toRemove = new List<Control>();
            foreach (Control c in this.flowLayoutPanel1.Controls)
            {
                if (c is 标签控件_右键点击删除)
                    toRemove.Add(c);
            }

            foreach (var c in toRemove)
            {
                this.flowLayoutPanel1.Controls.Remove(c);
                c.Dispose();
            }

            _已选标签.Clear();

            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        // wired from designer
        private void button1_Click(object sender, EventArgs e)
        {
            清空();
        }

        // Return a copy of selected tags
        public List<标签> 获取选中标签()
        {
            return new List<标签>(_已选标签);
        }

        
    }
}
