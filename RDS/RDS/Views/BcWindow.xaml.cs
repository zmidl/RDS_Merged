using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Sias.ReSaTrax;

namespace RDS.Views
{
    /// <summary>
    /// BcWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BcWindow : Window, IDialogHelper
    {
        public BcWindow()
        {
            InitializeComponent();
            IsAborted = false;
            IsRetryRequested = false;
            BtnRetry.IsEnabled = false;
            BtnAbort.IsEnabled = true;
        }

        //终止
        public virtual bool IsAborted { get; set; }

        //重试
        public virtual bool IsRetryRequested { get; set; }

        public virtual void Done()
        {
            this.Close();
        }

        public virtual void Failed()
        {
            Dispatcher.Invoke(() => {

                BtnRetry.IsEnabled = true;
            });
            while (!IsRetryRequested && !IsAborted)
            {
                WpfApplication.DoEvents();
            }
        }

        public void ShowDialog(int slotIndex)
        {
            TxtSlotIndex.Text = (slotIndex-1).ToString();
            this.Show();
        }

        private void BtnRetry_Click(object sender, RoutedEventArgs e)
        {
            IsAborted = false;
            IsRetryRequested = true;
            BtnRetry.IsEnabled = false;
        }

        private void BtnAbort_Click(object sender, RoutedEventArgs e)
        {
            IsAborted = true;
            IsRetryRequested = false;
            this.Close();
        }
    }
}
