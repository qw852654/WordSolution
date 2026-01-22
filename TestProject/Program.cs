using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TagRunner.Models;
using TagRunner.测试;
using TagRunner.业务;
using UI.控件;

namespace TestProject
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // 1) 初始化题库到桌面的 test 文件夹（覆盖旧数据库）
                var demoRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test");
                Console.WriteLine($"Initializing demo library at: {demoRoot}");

                // 生成示例题库（会调用 Bootstrapper.Initialize 并覆盖数据库）
                var created = DemoDataInitializer.生成示例题库(demoRoot, 标签数量: 3, 题目数量: 10);
                Console.WriteLine($"Created {created.Count} demo questions.");

                // 2) 获取应用服务集并传入选题窗口进行 UI 测试
                var services = Bootstrapper.获取应用服务集();

                // 启动 WinForms 选题窗口，传入服务集（UI 将使用服务集中的标签/题目服务）
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 手工测试：新增题目窗口
                Application.Run(new UI.窗体.新增题目窗口(services));
            }
            catch (Exception ex)
            {
                Console.WriteLine("初始化或运行失败：" + ex.Message);
                Console.WriteLine(ex.ToString());
                Console.WriteLine("按回车退出。");
                Console.ReadLine();
            }
        }
    }
}
