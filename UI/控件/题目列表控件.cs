using System.Collections.Generic;
using System.Windows.Forms;
using TagRunner.Models;
using TagRunner.业务;

namespace UI.控件
{
    // 题目列表控件：顺序展示多个题目预览控件（垂直堆叠）
    public class 题目列表控件 : UserControl
    {
        private FlowLayoutPanel flowLayoutPanel1; // 用于按顺序排列题目预览控件

        public 题目列表控件()
        {
            InitializeComponent();

            // FlowLayoutPanel 设置为垂直排列并自动换行关闭，确保控件按添加顺序垂直堆叠
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = true;
        }

        // 公共方法：按照传入题目列表顺序展示每个题目
        // 参数：questions - 要展示的题目列表
        //      题目服务、标签服务 - 传入到每个题目预览控件中用于后续操作或显示
        public void 加载题目列表(List<题目> questions, I题目服务 题目服务实例, I标签服务 标签服务实例)
        {
            // 清空已有的显示项
            flowLayoutPanel1.Controls.Clear();

            if (questions == null) return; // 参数为空则不做任何事

            // 按顺序创建并添加每个题目预览控件
            foreach (var q in questions)
            {
                // 为每个题目创建一个预览控件，传入服务实例
                var preview = new 题目预览控件(q, 题目服务实例, 标签服务实例);

                // 使预览控件水平拉伸以匹配列表宽度
                preview.Width = flowLayoutPanel1.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                preview.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                // 在 FlowLayoutPanel 中按添加顺序展示（TopDown）
                flowLayoutPanel1.Controls.Add(preview);
            }
        }

        // 公共方法：向当前列表追加一个题目并显示
        public void 添加题目(题目 q, 题目服务 题目服务实例, 标签服务 标签服务实例)
        {
            if (q == null) return;
            var preview = new 题目预览控件(q, 题目服务实例, 标签服务实例);
            preview.Width = flowLayoutPanel1.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            preview.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            flowLayoutPanel1.Controls.Add(preview);
        }

        // 公共方法：清空列表
        public void 清空()
        {
            flowLayoutPanel1.Controls.Clear();
        }

        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(489, 433);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // 题目列表控件
            // 
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "题目列表控件";
            this.Size = new System.Drawing.Size(489, 433);
            this.ResumeLayout(false);

        }
    }
}
