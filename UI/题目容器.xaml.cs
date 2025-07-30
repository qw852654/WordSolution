using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;

namespace 基本单元UI
{
    /// <summary>
    /// 题目容器.xaml 的交互逻辑
    /// </summary>
    public partial class 题目容器 : UserControl
    {
        public 题目容器(string filePath)
        {
            InitializeComponent();

            //在documentviewer中显示xps文件
            // 加载 XPS 文档
            XpsDocument xpsDocument = new XpsDocument(filePath, FileAccess.Read);
            documentViewer.Document = xpsDocument.GetFixedDocumentSequence();
            documentViewer.FitToWidth();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
