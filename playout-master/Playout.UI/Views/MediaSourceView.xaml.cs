using Playout.Base;
using Playout.MediaFramework;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.UI.Views
{
    public partial class MediaSourceView
    {
        bool isInsert = false;
        public MediaSourceViewModel ViewModel { get; private set; }
        public MediaSourceView(MediaSourceViewModel viewmodel,bool insert)
        {
            this.isInsert = insert;
            this.ViewModel = viewmodel;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            this.DataContext = this.ViewModel;
            //
            if (!isInsert)
            {
                this.cmbMediaSource.IsEnabled = false;
            }
            //
            this.cmbMediaSource.ItemsSource = Enum.GetNames(typeof(enMediaType))
                .Where(m => m != "ImageFile" && m != "DVD");
            this.cmbDevice.ItemsSource = Program.Dg.GetInputVideoDeviceNames();
            this.cmbCategories.ItemsSource = CategoryWorker.Categories.Keys;
            //
            if (this.ViewModel.MediaType == enMediaType.ImageFile)
            {
                this.txtThumbImage.IsEnabled =
                    this.btnSelectThumbImage.IsEnabled = false;
            }
        }
        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //MediaFramework.DirectShowGrabber.ShowPropertiesPageForInputVideoCapture(this.cmbDevice.Text);
            //return;
            if(this.isInsert && !MainViewModel.AllowAddMediaSource(this.ViewModel.MediaType))
            {
                await Functions.ShowMessage("License Validation",
                    String.Format("A media item with {0} type is already exist. You can't add more than one instance for your registered license.", this.ViewModel.MediaType));
                return;
            }
            //
            switch(this.ViewModel.MediaType)
            {
                case enMediaType.Device:
                    if(String.IsNullOrEmpty(this.ViewModel.DeviceName))
                    {
                        await Functions.ShowMessage("Media Source", "Please select a capture device.");
                        return;
                    }
                    break;
                case enMediaType.ImageFile:
                case enMediaType.VideoFile:
                case enMediaType.Playlist:
                    if(String.IsNullOrEmpty(this.ViewModel.File_Url) || !File.Exists(this.ViewModel.File_Url))
                    {
                        await Functions.ShowMessage("Media Source", "Media file is emplty or not exist. Please select an media file");
                        return;
                    }
                    break;
                case enMediaType.Url:
                    if (String.IsNullOrEmpty(this.ViewModel.File_Url) || !Uri.IsWellFormedUriString(this.ViewModel.File_Url,UriKind.Absolute))
                    {
                        await Functions.ShowMessage("Media Source", "Media url is emplty or has not correct format. Please input correct media url.");
                        return;
                    }
                    break;
                case enMediaType.DVD:
                    break;
                default:
                    break;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            string filter="";
            if (this.ViewModel.MediaType == enMediaType.ImageFile)
                filter = Program.OPEN_PICTURE_FILES;
            else if (this.ViewModel.MediaType == enMediaType.VideoFile)
                filter = Program.OPEN_ONLY_MEDIA_FILES;
            else
                filter = Program.PLAYLISTAndSCHEDULE_FILES;
            //
            string fileName = Functions.ShowOpenFileDialog("", filter, Program.DefaultDir_MediaFiles);
            if (!String.IsNullOrEmpty(fileName))
            {
                this.ViewModel.File_Url = fileName;
            }
        }

        private void cmbMediaSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cmbMediaSource.SelectedItem == null)
                return;
            //
            enMediaType mediaType = (enMediaType)Enum.Parse(typeof(enMediaType), this.cmbMediaSource.SelectedItem.ToString());
            switch(mediaType)
            {
                case enMediaType.Device:
                    this.cmbDevice.IsEnabled = true;
                    this.cmbInputDevFormats.IsEnabled = true;
                    this.txtFile.IsEnabled = false;
                    this.btnSelectFile.Visibility = System.Windows.Visibility.Collapsed;
                    this.iudDuration.IsEnabled = true;
                    break;
                case enMediaType.DVD:
                    this.cmbDevice.IsEnabled = false;
                    this.cmbInputDevFormats.IsEnabled = false;
                    this.txtFile.IsEnabled = false;
                    this.btnSelectFile.Visibility = System.Windows.Visibility.Collapsed;
                    this.iudDuration.IsEnabled = true;
                    break;
                case enMediaType.ImageFile:
                    this.cmbDevice.IsEnabled = false;
                    this.cmbInputDevFormats.IsEnabled = false;
                    this.txtFile.IsEnabled = true;
                    this.btnSelectFile.Visibility = System.Windows.Visibility.Visible;
                    this.lblFile.Content = "Select File:";
                    this.iudDuration.IsEnabled = true;
                    break;
                case enMediaType.Playlist:
                    this.cmbDevice.IsEnabled = false;
                    this.cmbInputDevFormats.IsEnabled = false;
                    this.iudDuration.IsEnabled = false;
                    this.btnSelectFile.Visibility = System.Windows.Visibility.Visible;
                    this.lblFile.Content = "Select File:";
                    this.txtFile.IsEnabled =
                        this.btnSelectFile.IsEnabled = isInsert;
                    break;
                case enMediaType.Url:
                    this.cmbDevice.IsEnabled = false;
                    this.cmbInputDevFormats.IsEnabled = false;
                    this.txtFile.IsEnabled = true;
                    this.btnSelectFile.Visibility = System.Windows.Visibility.Collapsed;
                    this.lblFile.Content = "Media Url:";
                    this.iudDuration.IsEnabled = true;
                    break;
                case enMediaType.VideoFile:
                    this.cmbDevice.IsEnabled = false;
                    this.cmbInputDevFormats.IsEnabled = false;
                    this.txtFile.IsEnabled = true;
                    this.btnSelectFile.Visibility = System.Windows.Visibility.Visible;
                    this.lblFile.Content = "Select File:";
                    this.iudDuration.IsEnabled = false;
                    break;
                default:
                    return;
            }
        }

        private void btnSelectThumbImage_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Functions.ShowOpenFileDialog("Image", Program.OPEN_PICTURE_FILES, Program.DefaultDir_MediaFiles);
            this.txtThumbImage.Text = fileName;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.ViewModel.File_Url_DeviceName) || this.ViewModel.MediaType == null)
                return;
            //
            this.prevPlayer.MediaViewModel = this.ViewModel;
            this.prevPlayer.Run();
        }

        private void BaseWindowView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.prevPlayer.Close();
        }

    }
}
