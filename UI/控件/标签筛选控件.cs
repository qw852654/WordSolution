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

namespace UI.控件
{
    
    public partial class 标签筛选控件 : UserControl
    {
        private 标签树控件 标签树;
        private 标签联想控件 联想器;
        public 标签筛选控件(TagRunner.业务.题库应用服务集 服务集)
        {
            InitializeComponent();
            标签树 = new 标签树控件();
            标签树.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(标签树);
            加载标签树(服务集.标签服务.获取标签树());

            联想器 = new 标签联想控件(服务集.标签服务);
            联想器.Dock = DockStyle.Fill;
            tabPage2.Controls.Add(联想器);

            // 订阅多选标签控件的已选标签变化事件
            多选标签控件1.SelectedChanged += (s, e) => 更新联想器已选排除标签();

            // 订阅联想器的标签选中事件
            联想器.TagSelected += (s, e) => 新增标签(e);

            //订阅标签树的标签选中事件
            标签树.TagSelected += (s, e) => 新增标签(e);
        }

        public List<标签> 获取选中标签()
        {
            var 结果 = new List<标签>();
            结果=多选标签控件1.获取选中标签();
            return 结果;
        }

        private void 新增标签(标签 待加入标签)
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
