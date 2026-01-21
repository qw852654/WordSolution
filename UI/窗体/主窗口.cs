using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TagRunner.Models;
using UI.控件;

namespace UI.窗体
{
    // 主窗口（用于测试题目预览控件和题目列表）
    public class 主窗口 : Form
    {
        private SplitContainer splitContainer2;
        private MenuStrip menuStrip1;

        public 主窗口()
        {
            // 初始化 designer 风格的基础控件
            InitializeComponent();

            // 设置窗口标题与大小
            this.Text = "题目管理 - 主窗口";
            this.Width = 1000;
            this.Height = 700;

            // 在左下区域放置标签树控件（占用左侧整个面板）
            var tagTree = new 控件.标签树控件();
            tagTree.Dock = DockStyle.Fill;
            // 将标签树加入左侧（splitContainer2 的 Panel1）
            splitContainer2.Panel1.Controls.Add(tagTree);

            // 在右下区域放置题目列表控件（占用右侧整个面板）
            var listCtrl = new 题目列表控件();
            listCtrl.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(listCtrl);

        }

        private void 刷新窗口()
        {
            
        }

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 24);
            this.splitContainer2.Name = "splitContainer2";
            // 左侧为标签树区域，右侧为题目列表/预览区域
            this.splitContainer2.Size = new System.Drawing.Size(800, 426);
            this.splitContainer2.SplitterDistance = 250;
            this.splitContainer2.TabIndex = 0;
            // 
            // 主窗口
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Name = "主窗口";
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
