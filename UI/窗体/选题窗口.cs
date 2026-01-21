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
using TagRunner;
using UI.控件;
using TagRunner.业务;

namespace UI.窗体
{
    public partial class 选题窗口 : Form
    {
        private 标签树控件 _标签树控件实例;
        private 标签筛选框控件 _筛选框控件实例;
        private 题目列表控件 _题目列表控件实例;
        private 题库应用服务集 _服务集;

        public 选题窗口(题库应用服务集 服务集)
        {
            InitializeComponent();
            _服务集 = 服务集;

            var 标签树根= 服务集.标签服务.获取标签树(); // 获取根标签合集

            // 在“标签树”选项卡中加载 TagRunner 的标签树控件（UI 层封装）
            _标签树控件实例 = new 标签树控件(标签树根); // 创建标签树控件实例
            _标签树控件实例.Dock = DockStyle.Fill; // 停靠填充 TabPage
            this.标签树选题.Controls.Add(_标签树控件实例); // 把控件加入到对应的 TabPage 中

            // 在“筛选框”选项卡中加载 标签筛选框控件（用于多标签筛选/输入）
            _筛选框控件实例 = new 标签筛选框控件(服务集.标签服务); // 创建筛选框控件实例
            _筛选框控件实例.Dock = DockStyle.Fill; // 停靠填充 TabPage
            this.筛选框选题.Controls.Add(_筛选框控件实例); // 把控件加入到对应的 TabPage 中

            // 在右侧主区域（splitContainer1.Panel2）放入题目列表控件，用于显示查询结果或题目集合
            _题目列表控件实例 = new 题目列表控件(); // 创建题目列表控件实例
            _题目列表控件实例.Dock = DockStyle.Fill; // 停靠填充右侧面板
            this.splitContainer1.Panel2.Controls.Add(_题目列表控件实例); // 加入到右侧面板
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 退出按钮：关闭窗口
            this.Close();
        }

        private void 选好题目(object sender, EventArgs e)
        {

        }

        private void 找题(object sender, EventArgs e)
        {
            var 题目列表= new List<题目>();
            if(tabControl1.SelectedIndex==0)
            {
                // 从标签树选题

                if (_标签树控件实例 != null)
                {
                    List<标签> 选中标签 = _标签树控件实例.获取选中标签();
                    // 根据选中标签获取题目列表
                    if (选中标签.Count > 0)
                        {
                        题目列表 = _服务集.题目服务.标签找题(选中标签);
                    }
                }        
            }
            else
            {
                               // 从筛选框选题
                if (_筛选框控件实例 != null)
                {
                    List<标签> 选中标签 = _筛选框控件实例.获取选中标签();
                    // 根据选中标签获取题目列表
                    if (选中标签.Count > 0)
                    {
                        题目列表 = _服务集.题目服务.标签找题(选中标签);
                    }
                }
            }
            _题目列表控件实例.加载题目列表(题目列表,_服务集.题目服务,_服务集.标签服务);
        }

    }
}
