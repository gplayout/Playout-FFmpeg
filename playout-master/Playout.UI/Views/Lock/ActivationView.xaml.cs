using Playout.Base;
using Playout.ViewModels.ViewModels;
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

namespace Playout.UI.Views.Lock
{
    public partial class ActivationView
    {
        public LockViewModel ViewModel { get; set; }
        public ActivationView()
        {
            InitializeComponent();
            this.ViewModel = new LockViewModel();
            this.DataContext = this.ViewModel;
            //
            if (this.ViewModel.IsRegistered)
            {
                this.btnDeactivation.Visibility = System.Windows.Visibility.Visible;
            }
            else
                this.btnDeactivation.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void ForTrialCheckExpired()
        {
            this.btnCancel.Content = "Close Playout";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnActivate_Click(object sender, RoutedEventArgs e)
        {
            if(!this.ViewModel.IsValid)
            {
                return;
            }
            //
            var result = this.ViewModel.DoActivation();
            if (result)
            {
                if (this.DialogResult != true)
                    this.DialogResult = true;
                this.Close();
            }
            else
            {
                this.ViewModel.SerialNo = "";
                this.txtSerialNo.Focus();
            }
        }

        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ViewModel.DoRetry();
            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnDeactivation_Click(object sender, RoutedEventArgs e)
        {
            Views.Lock.DeactivationView wnd = new Views.Lock.DeactivationView();
            if (wnd.ShowDialog() == true)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
