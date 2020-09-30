using AutoUpdater.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace AutoUpdater
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            this.Startup += new StartupEventHandler(Application_Startup);
        }


        protected override void OnStartup(StartupEventArgs e)
        {

            if (e.Args.Length == 0)
            {
                MessageBox.Show("无参数启动");
                return;
            }
            else
            {
                //MessageBox.Show("参数启动");
                //MessageBox.Show(e.Args[0]);
                //MessageBox.Show(e.Args[1]);
                //MessageBox.Show(e.Args[2]);
                //MessageBox.Show(e.Args[3]);
                //MessageBox.Show(e.Args[4]);
                //MessageBox.Show(e.Args[5]);

                try
                {
                    string callExeName = e.Args[1];
                    string updateFileDir = e.Args[2];
                    string appDir = e.Args[3];
                    string appName = e.Args[4];
                    string appVersion = e.Args[5];
                    string desc = "更新描述";


                    var downUI = new DownFileProcess(callExeName, updateFileDir, appDir, appName, appVersion, desc) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                    downUI.ShowDialog();


                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


            //var dlg = new MainWindow();
            //dlg.ShowDialog();

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName)
                    {
                        System.Windows.MessageBox.Show("程序已经运行！");
                        System.Environment.Exit(0);
                        return;
                    }
                }
            }
        }








        



    }
}
