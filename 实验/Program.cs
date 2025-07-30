using Microsoft.Office.Interop.Word;
using System;
using System.Windows;
using UI;

namespace Launcher
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new Application();
            var window = new System.Windows.Window
            {
                Content = new UserControl1()
            }; 

            var userControl = (UserControl1)window.Content;
            userControl.LoadWordDocument(@"E:\Desktop\测电动势和内阻.docx");

            app.Run(window);
        }
    }
}
