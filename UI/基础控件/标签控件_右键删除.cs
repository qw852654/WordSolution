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
using TagRunner.Models;

namespace UI.基础控件
{

    public partial class 标签控件_右键点击删除 : UserControl
    {
        private 标签 _标签; // 存储当前控件显示的标签对象

        // 当用户右键点击该控件时，触发此事件以通知上层服务删除该标签
        public event EventHandler<标签> DeleteRequested;

        public 标签控件_右键点击删除(标签 显示的标签)
        {
            InitializeComponent(); // 初始化设计器生成的子控件
            _标签 = 显示的标签 ?? throw new ArgumentNullException(nameof(显示的标签)); // 参数校验并赋值
            this.label1.Text = _标签.Name; // 设置标签名称显示

            // 订阅鼠标弹起事件，便于检测右键点击
            this.label1.MouseUp += 带叉的标签控件_MouseUp;
        }

        // 鼠标弹起事件处理器：若为右键，则触发 DeleteRequested 事件通知上层删除
        private void 带叉的标签控件_MouseUp(object sender, MouseEventArgs e)
        {
            // 只有右键点击才视为删除请求
            if (e.Button == MouseButtons.Right)
            {
                // 触发事件，传递当前标签给订阅者；如果没有订阅者，安全地不做任何事
                DeleteRequested?.Invoke(this, _标签);
            }
        }
    }
}
