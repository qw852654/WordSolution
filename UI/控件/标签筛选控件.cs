using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.基础控件;
using TagRunner.Models;
using TagRunner;
using TagRunner.业务;

namespace UI.控件
{
    
    public partial class 标签筛选控件 : UserControl
    {
        private 标签树控件 标签树;
        private 标签联想控件 联想器;
        private 题库应用服务集 _服务集;

        public 标签筛选控件(TagRunner.业务.题库应用服务集 服务集)
        {
            InitializeComponent();

            _服务集 =  服务集;
            

            标签树 = new 标签树控件();
            标签树.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(标签树);
            加载标签树(服务集.标签服务.获取标签树());
            

            联想器 = new 标签联想控件(服务集.标签服务);
            联想器.Dock = DockStyle.Fill;
            tabPage2.Controls.Add(联想器);

            // 订阅多选标签控件的已选标签变化事件 
            多选标签控件1.SelectedChanged += (s, e) => 更新联想器已选排除标签();
            
            //初始化展示标签列表
            foreach(var tag in 服务集.标签服务.上次被选择的标签)
               新增选择的标签(tag);

            // 订阅联想器的标签选中事件
            联想器.TagSelected += (s, e) => 新增选择的标签(e);

            //订阅标签树的标签选中事件
            标签树.TagSelected += (s, e) => 新增选择的标签(e);
        }

        private void hostFormClosed(object sender,FormClosedEventArgs e)
        {
            _服务集.标签服务.保存上次被选择的标签(获取选中标签());
        }

        //控件放入窗体时执行
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            var hostform = this.FindForm();
            hostform.FormClosed += hostFormClosed;
        }

        //释放控件时保存当前选择的标签
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                _服务集.标签服务.保存上次被选择的标签(this.获取选中标签());
                components.Dispose();
            }
            base.Dispose(disposing);
        }




        public List<标签> 获取选中标签()
        {
            var 结果 = new List<标签>();
            结果=多选标签控件1.获取选中标签();
            return 结果;
        }

        private void 新增选择的标签(标签 待加入标签)
        {
            多选标签控件1.增加选中标签(待加入标签);
        }

        public void 加载标签树(List<标签> 根标签列表)
        {
            标签树.加载标签树(根标签列表);
        }
        
        private void 更新联想器已选排除标签()
        {
            var 已选标签 = 多选标签控件1.获取选中标签();
            联想器.设置已选排除(已选标签);
        }

    }
}
