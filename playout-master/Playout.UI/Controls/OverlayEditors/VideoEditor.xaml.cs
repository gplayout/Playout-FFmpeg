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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playout.UI.Controls.OverlayEditors
{
    public partial class VideoEditor
    {
        public VideoOverlayMediaViewModel ViewModel { get; set; }
        public VideoEditor(VideoOverlayMediaViewModel data)
        {
            this.ViewModel = data;
            this.DataContext = this.ViewModel;
            InitializeComponent();
        }
        private void btnSelectVideo_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Functions.ShowOpenFileDialog("Video", Program.OPEN_ONLY_MEDIA_FILES, Program.DefaultDir_MediaFiles);
            this.ViewModel.FilePath = fileName;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }

}
