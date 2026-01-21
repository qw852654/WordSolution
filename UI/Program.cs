using System;
using System.Windows.Forms;

namespace UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 启动新的主窗口（占位）
            Application.Run(new 窗体.主窗口());
        }
    }
}
