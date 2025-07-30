using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using 基本单元UI;

namespace YourNamespace
{
    public partial class 文档展示单元 : UserControl
    {
        public 文档展示单元()
        {
            InitializeComponent();
        }

        public void LoadXpsDocument(string filePath)
        {
            if (File.Exists(filePath))
            {
                // 创建新的题目
                var 新题目 = new 题目容器(filePath);

                // 将题目添加到 StackPanel 的底部
                if (this.Content is ScrollViewer scrollViewer && scrollViewer.Content is StackPanel stackPanel)
                {
                    stackPanel.Children.Add(新题目);
                }
            }
            else
            {
                MessageBox.Show("文件未找到: " + filePath, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
