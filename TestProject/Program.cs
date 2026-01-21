using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TagRunner.Models;
using UI.控件;

namespace TestProject
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 构造示例标签树
            var roots = new List<标签>
            {
                new 标签 { Id = 1, Name = "编程", Children = new List<标签> {
                    new 标签 { Id = 11, Name = "C#", Children = new List<标签>() },
                    new 标签 { Id = 12, Name = "Python", Children = new List<标签>() }
                }},
                new 标签 { Id = 2, Name = "数学", Children = new List<标签> {
                    new 标签 { Id = 21, Name = "代数", Children = new List<标签>() },
                    new 标签 { Id = 22, Name = "几何", Children = new List<标签>() }
                }}
            };

            var form = new Form { Text = "标签树控件 测试", Width = 400, Height = 500 };
            var treeCtrl = new 标签树控件(roots) { Dock = DockStyle.Fill };
            form.Controls.Add(treeCtrl);

            Application.Run(form);
        }
    }
}
