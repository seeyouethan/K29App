using System;
using System.Collections.Generic;
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
    /// AlertWin.xaml 的交互逻辑
    /// </summary>
    public partial class AlertWin : WindowBase
    {
        public bool YesBtnSelected = false;
        public AlertWin(string msg)
        {
            InitializeComponent();

            this.Loaded += (sl, el) =>
            {
                YesButton.Content = "是";
                NoButton.Content = "否";
                this.txtMsg.Text = msg;
                this.YesButton.Click += (sender, e) =>
                {
                    YesBtnSelected = true;
                    this.Close();
                };

                this.NoButton.Click += (sender, e) =>
                {
                    YesBtnSelected = false;
                    this.Close();
                };
            };


        }
    }
}
