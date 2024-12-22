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

namespace Playout.UI.Controls
{
    public partial class SchedulesListView
    {
        SchedulingsViewModel _viewModel;
        Controls.DataGridDropCap ddCap = null;
        public SchedulingsViewModel ViewModel
        {
            get { return this._viewModel; }
            set
            {
                this._viewModel = value;
                //
                this.DataContext = this._viewModel;
                if (this.dgPlaylist.SelectedItem == null && this.ViewModel.Schedules.Count > 0)
                    this.dgPlaylist.SelectedIndex = 0;
            }
        }

        public SchedulesListView()
        {
            InitializeComponent();
            //
            this.ddCap = new DataGridDropCap(this.dgPlaylist, this.dgPlaylist_OnDropFiles);
        }

        private void expMenuUp_Click(object sender, RoutedEventArgs e)
        {
            var selitem = (ScheduleViewModel)this.dgPlaylist.SelectedItem;
            if (selitem != null)
            {
                this.ViewModel.ChangeOrderSchedule(selitem, true);
                this.dgPlaylist.SelectedItem = selitem;
            }
        }

        private void expMenuDown_Click(object sender, RoutedEventArgs e)
        {
            var selitem = (ScheduleViewModel)this.dgPlaylist.SelectedItem;
            if (selitem != null)
            {
                this.ViewModel.ChangeOrderSchedule(selitem, false);
                this.dgPlaylist.SelectedItem = selitem;
            }
        }

        private void MenuItemNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var pvm = this.ViewModel.NewSchedule();
            if (pvm != null)
                this.dgPlaylist.SelectedItem = pvm;
        }

        private void MenuItemOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var pvm = this.ViewModel.OpenSchedule();
            if (pvm != null)
                this.dgPlaylist.SelectedItem = pvm;
        }

        private void MenuItemSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgPlaylist.SelectedItem == null)
                return;
            //
            var pvm = (ScheduleViewModel)this.dgPlaylist.SelectedItem;
            this.ViewModel.SaveSchedule(pvm);
        }

        private void MenuItemRemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgPlaylist.SelectedItem == null)
                return;
            //
            var pvm = (ScheduleViewModel)this.dgPlaylist.SelectedItem;
            this.ViewModel.RemoveSchedule(pvm);
        }

        private void MenuItemSchedulePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgPlaylist.SelectedItem == null)
                return;
            //
            var svm = (ScheduleViewModel)this.dgPlaylist.SelectedItem;
            var cloned = svm.CloneForScheduling();
            //
            Views.PlaylistScheduleView wnd = new Views.PlaylistScheduleView(cloned);
            if (wnd.ShowDialog() == true)
            {
                svm.Days = cloned.Days;
                svm.Enabled = cloned.Enabled;
                svm.EveryDay = cloned.EveryDay;
                svm.FilePath = cloned.FilePath;
                svm.IntervalMinutes = cloned.IntervalMinutes;
                svm.StartDate = cloned.StartDate;
                svm.StartTime = cloned.StartTime;
            }
        }

        private void dgPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MenuItemRemovePlaylist.Visibility =
                this.MenuItemSavePlaylist.Visibility =
                this.MenuItemSchedulePlaylist.Visibility = (this.dgPlaylist.SelectedItem == null ? Visibility.Collapsed : Visibility.Visible);
        }

        private void dgPlaylist_OnDropFiles(object sender, OnDropFilesEventArgs e)
        {
            if (e.Files == null)
                return;
            //
            foreach (string file in e.Files)
            {
                this.ViewModel.LoadFile(file);
            }
        }
    }
}
