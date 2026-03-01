using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Core.QuestionBank.Domain;
using UI.控件;
using TagRunner.业务;

namespace UI.窗体
{
    public partial class 选题窗口 : Form
    {
        private 标签筛选控件 _标签筛选控件;
        private 题目列表控件 _题目列表控件;
        private 题库应用服务集 _服务集;

        private List<题目> _当前题目列表 = new List<题目>();

        public List<题目> 选中的题目 { get; private set; } = new List<题目>();

        public 选题窗口(题库应用服务集 服务集)
        {
            InitializeComponent();
            _服务集 = 服务集 ?? throw new ArgumentNullException(nameof(服务集));

            splitContainer2.SplitterDistance = splitContainer2.Height - 30;

            // 左侧：标签筛选控件
            _标签筛选控件 = new 标签筛选控件(_服务集);
            _标签筛选控件.Dock = DockStyle.Fill;
            this.splitContainer2.Panel1.Controls.Add(_标签筛选控件);

            // 右侧：题目列表控件
            _题目列表控件 = new 题目列表控件();
            _题目列表控件.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(_题目列表控件);

            this.splitContainer2.SplitterDistance = this.splitContainer2.Height - 30;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 选好题目(object sender, EventArgs e)
        {
            选中的题目= _题目列表控件.获取选中题目();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void 找题(object sender, EventArgs e)
        {
            if (_标签筛选控件 == null || _题目列表控件 == null)
                return;

            var 选中标签 = _标签筛选控件.获取选中标签();
            if (选中标签 == null || 选中标签.Count == 0)
            {
                _当前题目列表 = new List<题目>();
                _题目列表控件.加载题目列表(_当前题目列表, _服务集.题目服务, _服务集.标签服务);
                return;
            }

            _当前题目列表 = _服务集.题目服务.标签找题(选中标签);
            if (_当前题目列表 == null)
                _当前题目列表 = new List<题目>();

            _题目列表控件.加载题目列表(_当前题目列表, _服务集.题目服务, _服务集.标签服务);
        }
    }
}
