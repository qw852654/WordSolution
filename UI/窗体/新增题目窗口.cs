using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UI.控件;
using Core.QuestionBank.Domain;
using TagRunner.业务;

namespace UI.窗体
{
    public partial class 新增题目窗口 : Form
    {
        private readonly 题库应用服务集 _服务集;
        private 标签筛选控件 _标签筛选器;

        public 新增题目窗口(题库应用服务集 服务集)
        {
            if (服务集 == null) throw new ArgumentNullException(nameof(服务集));

            InitializeComponent();

            _服务集 = 服务集;

            splitContainer2.SplitterDistance = 28;
            splitContainer3.SplitterDistance = splitContainer3.Height - 20;

            // 左侧 splitContainer2.Panel2：装入筛选器
            _标签筛选器 = new 标签筛选控件(_服务集);
            _标签筛选器.Dock = DockStyle.Fill;
            splitContainer3.Panel1.Controls.Add(_标签筛选器);

            // 右侧 Panel2：按需求留空（不加载预览器）

            // 事件绑定（避免改 Designer）
            选择题目.Click += 选择题目_Click;
            button1.Click += 确定_Click;
            button2.Click += 退出_Click;
        }

        public void 设置源文件路径(string 源文件路径)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(设置源文件路径), 源文件路径);
                return;
            }

            题目路径.Text = 源文件路径 ?? string.Empty;
        }

        private void 选择题目_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "选择题目文件";
                ofd.Filter = "Word 文件 (*.doc;*.docx)|*.doc;*.docx|所有文件 (*.*)|*.*";
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    题目路径.Text = ofd.FileName;
                }
            }
        }

        private void 确定_Click(object sender, EventArgs e)
        {
            var 源文件路径 = 题目路径.Text;
            if (string.IsNullOrWhiteSpace(源文件路径))
            {
                MessageBox.Show(this, "请先选择题目文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(源文件路径))
            {
                MessageBox.Show(this, "文件不存在，请重新选择。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var 选中标签 = _标签筛选器 != null ? _标签筛选器.获取选中标签() : new List<标签>();
            if (选中标签 == null || 选中标签.Count == 0)
            {
                MessageBox.Show(this, "请至少选择一个标签。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var 新题目Id = _服务集.题目服务.新增题目(选中标签, 源文件路径);
                MessageBox.Show(this, $"新增成功，题目ID：{新题目Id}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 关闭窗口并返回 OK
                this.DialogResult = DialogResult.OK;
                // _服务集.标签服务.保存上次被选择的标签(_标签筛选器.获取选中标签());
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "新增失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 退出_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
