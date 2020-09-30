﻿using AutoUpdater.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoUpdater.UI
{
    /// <summary>
    /// DownFileProcess.xaml 的交互逻辑
    /// </summary>
    public partial class DownFileProcess : WindowBase
    {
        private string updateFileDir;//更新文件存放的文件夹
        private string callExeName;
        private string appDir;
        private string appName;
        private string appVersion;
        private string desc;
        public DownFileProcess(string callExeName, string updateFileDir, string appDir, string appName, string appVersion, string desc)
        {
            InitializeComponent();
            this.Loaded += (sl, el) =>
            {
                YesButton.Content = "现在更新";
                NoButton.Content = "暂不更新";

                this.YesButton.Click += (sender, e) =>
                {
                    Process[] processes = Process.GetProcessesByName(this.callExeName);

                    if (processes.Length > 0)
                    {
                        foreach (var p in processes)
                        {
                            p.Kill();
                        }
                    }

                    DownloadUpdateFile();
                };

                this.NoButton.Click += (sender, e) =>
                {
                    this.Close();
                };

                this.txtProcess.Text = this.appName + "发现新的版本(" + this.appVersion + "),是否现在更新?";
                txtDes.Text = this.desc;
            };
            //this.callExeName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(callExeName));
            this.callExeName = callExeName;
            //this.updateFileDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(updateFileDir));
            this.updateFileDir = updateFileDir;
            //this.appDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appDir));
            this.appDir = appDir;
            //this.appName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appName));
            this.appName = appName;
            //this.appVersion = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appVersion));
            this.appVersion = appVersion;

            //string sDesc = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(desc));
            string sDesc = desc;
            if (sDesc.ToLower().Equals("null"))
            {
                this.desc = "";
            }
            else
            {
                this.desc = "更新内容如下:\r\n" + sDesc;
            }
        }

        public void DownloadUpdateFile()
        {
            var url = Constants.RemoteUrl + "/app.zip";
            
            var client = new System.Net.WebClient();
            client.DownloadProgressChanged += (sender, e) =>
            {
                UpdateProcess(e.BytesReceived, e.TotalBytesToReceive);
            };
            client.DownloadDataCompleted += (sender, e) =>
            {
                MessageBox.Show(updateFileDir);
                string zipFilePath = System.IO.Path.Combine(updateFileDir, "app.zip");
                byte[] data = e.Result;
                BinaryWriter writer = new BinaryWriter(new FileStream(zipFilePath, FileMode.OpenOrCreate));
                writer.Write(data);
                writer.Flush();
                writer.Close();

                System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    Action f = () =>
                    {
                        txtProcess.Text = "开始更新程序...";
                    };
                    this.Dispatcher.Invoke(f);

                    string tempDir = System.IO.Path.Combine(updateFileDir, "temp");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    UnZipFile(zipFilePath, tempDir);

                    //移动文件
                    //App
                    if (Directory.Exists(System.IO.Path.Combine(tempDir, "App")))
                    {
                        CopyDirectory(System.IO.Path.Combine(tempDir, "App"), appDir);
                    }

                    f = () =>
                    {
                        txtProcess.Text = "更新完成!";

                        try
                        {
                            //清空缓存文件夹
                            string rootUpdateDir = updateFileDir.Substring(0, updateFileDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                            foreach (string p in System.IO.Directory.EnumerateDirectories(rootUpdateDir))
                            {
                                if (!p.ToLower().Equals(updateFileDir.ToLower()))
                                {
                                    System.IO.Directory.Delete(p, true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message);
                        }

                    };
                    this.Dispatcher.Invoke(f);

                    try
                    {
                        f = () =>
                        {
                            AlertWin alert = new AlertWin("更新完成,是否现在启动软件?") { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
                            alert.Title = "更新完成";
                            alert.Loaded += (ss, ee) =>
                            {
                                alert.YesButton.Width = 40;
                                alert.NoButton.Width = 40;
                            };
                            alert.Width = 300;
                            alert.Height = 200;
                            alert.ShowDialog();
                            if (alert.YesBtnSelected)
                            {
                                //启动软件
                                string exePath = System.IO.Path.Combine(appDir, callExeName + ".exe");
                                var info = new System.Diagnostics.ProcessStartInfo(exePath);
                                info.UseShellExecute = true;
                                info.WorkingDirectory = appDir;// exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                                System.Diagnostics.Process.Start(info);
                            }
                            else
                            {

                            }
                            this.Close();
                        };
                        this.Dispatcher.Invoke(f);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                });

            };
            client.DownloadDataAsync(new Uri(url));
        }

        private static void UnZipFile(string zipFilePath, string targetDir)
        {
            ICCEmbedded.SharpZipLib.Zip.FastZipEvents evt = new ICCEmbedded.SharpZipLib.Zip.FastZipEvents();
            ICCEmbedded.SharpZipLib.Zip.FastZip fz = new ICCEmbedded.SharpZipLib.Zip.FastZip(evt);
            fz.ExtractZip(zipFilePath, targetDir, "");
        }

        public void UpdateProcess(long current, long total)
        {
            string status = (int)((float)current * 100 / (float)total) + "%";
            this.txtProcess.Text = status;
            rectProcess.Width = ((float)current / (float)total) * bProcess.ActualWidth;
        }

        public void CopyDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                MessageBox.Show(sourceDirName);
                MessageBox.Show(destDirName);
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                    File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
                }
                if (destDirName[destDirName.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                    destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;
                string[] files = Directory.GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    File.Copy(file, destDirName + System.IO.Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + System.IO.Path.GetFileName(file), FileAttributes.Normal);
                }
                string[] dirs = Directory.GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("复制文件错误");
            }
        }
    }
}
