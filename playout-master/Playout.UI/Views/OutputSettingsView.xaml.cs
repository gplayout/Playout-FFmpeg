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
    public partial class OutputSettingsView
    {
        public ViewModels.ViewModels.OutputSettingsViewModel ViewModel { get; set; }
        
        public OutputSettingsView(OutputSettingsViewModel vm)
        {
            this.ViewModel = vm;
            this.DataContext = this.ViewModel;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            //
            cmbVideoDevice_SelectionChanged(null, null);
        }

        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists(this.ViewModel.DefaultDir_MediaFiles))
            {
                await Functions.ShowMessage("Settings", "Please select a correct path for Media Files Default Directory.");
                this.btnSelectDefDir_MediaFiles.Focus();
                return;
            }
            if (!Directory.Exists(this.ViewModel.DefaultDir_Playlists))
            {
                await Functions.ShowMessage("Settings", "Please select a correct path for Playlists Files Default Directory.");
                this.btnSelectDefDir_Playlists.Focus();
                return;
            }
            this.DialogResult = true;
            this.ViewModel.OperateSettings();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnResetOutput_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnSelectDefDir_Playlists_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Functions.ShowOpenFolderDialog(this.ViewModel.DefaultDir_Playlists);
            this.ViewModel.DefaultDir_Playlists = fileName;
        }

        private void btnSelectDefDir_MediaFiles_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Functions.ShowOpenFolderDialog(this.ViewModel.DefaultDir_MediaFiles);
            this.ViewModel.DefaultDir_MediaFiles = fileName;
        }

        private void cmbVideoDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cmbVideoFormats == null)
                return;
            //
            var formats = Program.Dg.GetOutputVideoFormats(this.cmbVideoDevice.SelectedValue == null ? "" : this.cmbVideoDevice.SelectedValue.ToString());
            this.cmbVideoFormats.ItemsSource = formats;
        }

    }
}
