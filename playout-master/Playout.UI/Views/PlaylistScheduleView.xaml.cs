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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Elysium.Controls;
using Playout.Base;

namespace Playout.UI.Views
{
    public partial class PlaylistScheduleView
    {
        public ScheduleViewModel ViewModel { get; set; }
        public PlaylistScheduleView(ScheduleViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
            App.CheckScale(new System.Windows.Window[] { this });
            this.DataContext = this.ViewModel;
            //
            foreach(UIElement item in this.gridMain.Children)
            {
                if (item is ToggleSwitch)
                {
                    var tb = (ToggleSwitch)item;
                    if (tb.Tag != null && this.ViewModel.Days.ToString().Contains(tb.Tag.ToString()))
                        tb.IsChecked = true;
                }
            }
        }

        private void tbDay_CheckChanged(object sender, RoutedEventArgs e)
        {
            string dayName = (sender as ToggleSwitch).Tag.ToString();
            bool isChecked = (sender as ToggleSwitch).IsChecked;
            Days2 day=(Days2)Enum.Parse(typeof(Days2),dayName);
            //
            if (isChecked && (this.ViewModel.Days & day) != day)
                this.ViewModel.Days |= day;
            else if (!isChecked && (this.ViewModel.Days & day) == day)
                this.ViewModel.Days ^= day;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
