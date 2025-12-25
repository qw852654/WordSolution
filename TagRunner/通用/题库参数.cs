using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TagRunner
{
    public static class 题库参数
    {
        private static string _题库目录;
        private static Dictionary<string, string> _题库路径表 = new Dictionary<string, string>();

        public static string 题库目录
        {
            get
            {
                if (string.IsNullOrEmpty(_题库目录))
                {
                    // 1. 尝试从配置文件读取
                    _题库目录 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData_题目查询服务");
                    // 2. 如果没有配置，弹窗让用户选择
                    if (string.IsNullOrEmpty(_题库目录) || !Directory.Exists(_题库目录))
                    {
                        using (var dlg = new FolderBrowserDialog())
                        {
                            dlg.Description = "请选择题库目录";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                _题库目录 = dlg.SelectedPath;
                            }
                            else
                            {
                                throw new InvalidOperationException("未设置题库目录，无法继续。");
                            }
                        }
                    }
                }
                return _题库目录;
            }
            set
            {
                _题库目录 = value;
            }
        }

        public static string 获取题库路径(string 题库名称)
        {
            if (_题库路径表.ContainsKey(题库名称))
            {
                return _题库路径表[题库名称];
            }
            else
            {
                return null;
            }
        }

        public static void 添加题库路径(string 题库名称)
        {
            if (!_题库路径表.ContainsKey(题库名称))
            {
                throw new ArgumentException("题库名称不存在");
            }

            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = $"请选择题库文件：{题库名称}";
            if (folderDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            _题库路径表[题库名称] = folderDialog.SelectedPath;

            //写入本地配置持久化


        }

        private static void 初始化题库目录()
        {

        }
    }
}
