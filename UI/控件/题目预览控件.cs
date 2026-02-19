using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Windows.Forms;
using TagRunner.Models;
using TagRunner.业务;

namespace UI.控件
{
    // 题目预览控件：展示题目的标签列表（上方 Panel）和 HTML 预览（下方 WebBrowser）
    public class 题目预览控件 : UserControl
    {
        private I题目服务 _题目服务; // 注入的题目服务，用于后续操作（当前实现只是保存引用）
        private I标签服务 _标签服务; // 注入的标签服务，用于获取标签信息
        private bool chosen=false;
        private WebBrowser webBrowser1; // 用于显示题目 HTML 内容
        private Button 选中按钮; 
        private SplitContainer splitContainer1; // 分割面板：上部显示标签，下部显示 HTML
        private Panel panel1; // 新增：最底层容器面板，所有其它控件都放在此面板内
        private 题目 _题目; // 当前正在展示的题目对象

        public event EventHandler<题目状态> 题目被点击事件;

        // 构造：接受待展示的题目对象与服务实例


        public 题目预览控件(题目 待展示题目, I题目服务 注入题目服务, I标签服务 注入标签服务)
        {
            // 初始化窗体子控件（设计器风格方法）
            InitializeComponent(); 

            // 保存注入的服务引用，便于以后调用（比如删除或编辑）
            _题目服务 = 注入题目服务;
            _标签服务 = 注入标签服务;

            // 保存当前题目对象
            _题目 = 待展示题目 ?? throw new ArgumentNullException(nameof(待展示题目));

            // 加载标签到上方 Panel（Panel1）
            加载题目标签();

            // 加载题目的 HTML 到 WebBrowser（Panel2）
            加载题目Html();
        }

        // 将题目的标签加载到 splitContainer1.Panel1 中
        private void 加载题目标签()
        {
            // 如果题目没有标签，直接返回（不抛异常）
            if (_题目.TagIDs == null || _题目.TagIDs.Count == 0)
                return;

            // 从后向前添加，这样 Dock=Top 时显示顺序和 TagIDs 顺序一致
            for (int i = _题目.TagIDs.Count - 1; i >= 0; i--)
            {
                int tagId = _题目.TagIDs[i];

                // 获取标签对象：通过 Bootstrapper 的仓储读取（避免依赖标签服务未暴露的方法）
                var tag = TagRunner.业务.Bootstrapper.标签仓储.Id获取标签(tagId);
                if (tag == null) continue; // 找不到标签则跳过

                // 创建一个带叉的标签控件用于显示（UI 层的基础控件）
                var tagCtrl = new 基础控件.标签控件_仅展示(tag);

                // 把控件顶端停靠，以便多个标签垂直堆叠
                tagCtrl.Dock = DockStyle.Left;

                // 添加到 Panel1
                splitContainer1.Panel1.Controls.Add(tagCtrl);
            }
        }

        // 加载题目的 HTML 到 webBrowser1 控件
        private void 加载题目Html()
        {
            try
            {
                // 通过全局 Bootstrapper 的文件存储计算 HTML 路径
                var htmlPath = TagRunner.业务.Bootstrapper.文件存储.获取Html路径(_题目.Id);

                // 如果文件存在，导航到文件路径；否则显示占位文本
                if (!string.IsNullOrWhiteSpace(htmlPath) && File.Exists(htmlPath))
                {
                    // 使用绝对 URI 导航本地文件
                    webBrowser1.Navigate(new Uri(htmlPath).AbsoluteUri);
                }
                else
                {
                    // 未找到 HTML，显示简短提示（内联 HTML）
                    webBrowser1.DocumentText = "<html><body><h3>题目内容未找到</h3><p>尚未生成 HTML 或文件缺失。</p></body></html>";
                }
            }
            catch (Exception ex)
            {
                // 出现异常时，在控件内显示错误信息而不抛出（避免影响宿主窗体）
                webBrowser1.DocumentText = $"<html><body><h3>加载失败</h3><pre>{System.Security.SecurityElement.Escape(ex.Message)}</pre></body></html>";
            }
        }

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.选中按钮 = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(1);
            this.panel1.Size = new System.Drawing.Size(715, 283);
            this.panel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(1, 1);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.选中按钮);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(711, 279);
            this.splitContainer1.SplitterDistance = 36;
            this.splitContainer1.TabIndex = 0;
            // 
            // 选中按钮
            // 
            this.选中按钮.Dock = System.Windows.Forms.DockStyle.Right;
            this.选中按钮.Location = new System.Drawing.Point(666, 0);
            this.选中按钮.Name = "选中按钮";
            this.选中按钮.Size = new System.Drawing.Size(45, 36);
            this.选中按钮.TabIndex = 0;
            this.选中按钮.Text = "选中";
            this.选中按钮.UseVisualStyleBackColor = true;
            this.选中按钮.Click += new System.EventHandler(this.题目预览控件_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(711, 239);
            this.webBrowser1.TabIndex = 0;
            // 
            // 题目预览控件
            // 
            this.Controls.Add(this.panel1);
            this.Name = "题目预览控件";
            this.Size = new System.Drawing.Size(715, 283);
            this.Click += new System.EventHandler(this.题目预览控件_Click);
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void 题目预览控件_Click(object sender, EventArgs e)
        {
            if (chosen)
            {
                this.panel1.BackColor = System.Drawing.SystemColors.Control;
                chosen = false;
                题目被点击事件.Invoke(this, new 题目状态(_题目, false));
            }
            else
            {
                this.panel1.BackColor = System.Drawing.Color.Red;
                chosen = true;
                题目被点击事件.Invoke(this, new 题目状态(_题目, true));
            }
        }

        public class 题目状态:EventArgs
        {
           public 题目 题目对象 { get;  }
            public bool 选中状态 { get;  }
            public 题目状态(题目 题目对象, bool 选中状态)
            {
                this.题目对象 = 题目对象;
                this.选中状态 = 选中状态;
            }
        }
    }
}
