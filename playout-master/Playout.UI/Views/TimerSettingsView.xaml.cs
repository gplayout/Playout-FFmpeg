using Playout.Base;
using Playout.ViewModels.ViewModels;
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

namespace Playout.UI.Views
{
    public partial class TimerSettingsView
    {
        public ViewModels.ViewModels.TimerSettingsViewModel ViewModel { get; set; }
        
        public TimerSettingsView(TimerSettingsViewModel vm)
        {
            this.ViewModel = vm;
            this.DataContext = this.ViewModel;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            //
            this.timeSpecific.Minimum = DateTime.Now;
            //
            if (this.ViewModel.Time == null)
                this.ViewModel.Time = DateTime.Now.TimeOfDay;

            if (vm.TimerMode == TimersMode.CountDown)
            {
                this.rbtCountdown.IsChecked = true;
                this.timeCountdown.Value = this.ViewModel.Time == null ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds((int)this.ViewModel.Time.Value.TotalSeconds);
            }
            else
            {
                this.rbtSpecTime.IsChecked = true;
                this.timeSpecific.Value = this.ViewModel.Time == null ? DateTime.Now :
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                            this.ViewModel.Time.Value.Hours, this.ViewModel.Time.Value.Minutes, this.ViewModel.Time.Value.Seconds);
            }
        }
        private void OperateView()
        {
            switch (this.ViewModel.TimerMode)
            {
                case TimersMode.None:
                    this.ViewModel.Time = null;
                    break;
                case TimersMode.CountDown:
                    this.ViewModel.Time = this.timeCountdown.Value;
                    break;
                case TimersMode.Specific:
                    this.ViewModel.Time = (this.timeSpecific.Value.HasValue ? (TimeSpan?)this.timeSpecific.Value.Value.TimeOfDay : null);
                    break;
                default:
                    break;
            }
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.OperateView();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void TimerMode_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.rbtSpecTime) && this.rbtSpecTime.IsChecked == true)
                this.ViewModel.TimerMode = TimersMode.Specific;
            else if (sender.Equals(this.rbtCountdown) && this.rbtCountdown.IsChecked == true)
                this.ViewModel.TimerMode = TimersMode.CountDown;
            //
            this.timeCountdown.IsEnabled = this.rbtCountdown.IsChecked.Value;
            this.timeSpecific.IsEnabled = this.rbtSpecTime.IsChecked.Value;
        }

        private void btnClearTimers_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TimerMode = TimersMode.None;
            this.DialogResult = true;
            this.OperateView();
            this.Close();
        }

    }
}
