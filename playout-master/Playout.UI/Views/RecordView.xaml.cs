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
    public partial class RecordView
    {
        public ViewModels.ViewModels.RecordViewModel ViewModel { get; set; }
        
        public RecordView(RecordViewModel vm)
        {
            this.ViewModel = vm;
            this.DataContext = this.ViewModel;
            FFMpegView.OnFFMpegChangedStatus += this.CheckButtonStatus;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            //
            this.CheckButtonStatus(null, null);
        }

        private void CheckButtonStatus(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.btnStart.IsEnabled = !FFMpegView.ffmpegProcStarted;
                this.btnStop.IsEnabled = FFMpegView.ffmpegProcStarted;
            });
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

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ViewModel.IsValid)
            {
                await Functions.ShowMessage("Record", "Please input all data fields.");
                return;
            }
            //
            FFMpegView.CloseAllFFMPEGS();
            //
            int vi = this.ViewModel.VCamChannel.GetHashCode();
            bool isSeg = this.ViewModel.SegmentSecond != 0;
            string fm = this.ViewModel.FileFormat;
            //
            string path = FFMpegView.streamDirPath + @"\Record\" + DateTime.Now.ToString("yyyyMMdd_HHmmss"); ;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //
            string args = " -y -loglevel warning -re -rtbufsize 1500M -f dshow -i video=\"Playout VCam" +
                vi + "\" -f dshow -i audio=\"Playout VSound" +
                vi + "\" -r " +
                this.ViewModel.FrameRate + " -threads 4 -vcodec libx264 -pix_fmt yuv420p "+
                (String.IsNullOrEmpty(this.ViewModel.VideoBitrate) ? "" : "-b:v " + this.ViewModel.VideoBitrate) +
                " -vf scale=" + this.ViewModel.FrameSizeString +
                " -c:a aac -strict -2 -q:a 1.1 -ar " + this.ViewModel.SampleRate + " -ac 2 " +
                (String.IsNullOrEmpty(this.ViewModel.AudioBitrate) ? "" : "-b:a " + this.ViewModel.AudioBitrate) +
                (isSeg ?
                    " -f segment -segment_time " + this.ViewModel.SegmentSecond +
                    " -segment_format " + fm + " \"" + path + "\\" + this.ViewModel.FileName + "_%03d." + fm + "\""
                    :
                    " \"" + path + "\\" + this.ViewModel.FileName + "." + fm + "\"");
            //
            FFMpegView.RunEncoder(EncoderProgram.FFMPEG, args);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            FFMpegView.CloseAllFFMPEGS();
        }
    }
}
