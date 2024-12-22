using Playout.Base;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ninject;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;
using Playout.Log;
using System.Reflection;

namespace Playout.UI.Views
{
    public partial class MainView
    {
        MainViewModel ViewModel = DI.NinjectConfig.kernel.Get<MainViewModel>();
        Codes.MainShortcuts shorts = null;
        public MainView()
        {
            InitializeComponent();
            App.CheckScale(new System.Windows.Window[] { this });
            //
            
            this.DataContext = this.ViewModel;
            //
            this.playerControl.ViewModel = this.ViewModel.PlayerControlVM;
            this.playerControl.PlayButtonEnableChanged = this.PlayButtonEnableChanged;
            this.playerControl.OnRecordClick = this.OnRecordClicked;
            //
            this.playlistsV.ViewModel = this.ViewModel.PlaylistsVM;
            this.vuMeter.ViewModel = this.ViewModel.VUMeterVM;
            //
            //
            this.ViewModel.TimerSettingVM.OnTimeReceived = this.Timer_OnTimeReceived;
            //
            this.prevPanel.PrevItemsVM = this.ViewModel.PrevItemsVM;
            this.prevPanel.PVViewModel = this.ViewModel.PlayerControlVM;
            //
            this.shorts = new Codes.MainShortcuts();
            //
            FFMpegView.OnFFMpegChangedStatus += FFMpegView_OnFFMpegChangedStatus;
            FFMpegView.OnLogReceived = this.FFMpegView_OnErrorReceived;
            //
            this.FFMpegView_OnFFMpegChangedStatus(null, null);
            //
            this.Title = "Smarter Broadcast - Professional Playout";

            this.Title += " - " + Program.Lock.LockStatus.ToString() + " Edition - V" + Functions.GetPlayoutVersion();
        }

        void FFMpegView_OnErrorReceived(string data)
        {
            this.Dispatcher.Invoke(() =>
                {
                    this.txtStreamLog.Text = data;// this.txtStreamLog.Text.Insert(0, data + Environment.NewLine);
                });
        }
        void FFMpegView_OnFFMpegChangedStatus(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                this.iconStreamStarted.Visibility =
                (FFMpegView.ffmpegProcStarted ? Visibility.Visible : Visibility.Collapsed);
                if (!FFMpegView.ffmpegProcStarted)
                    this.txtStreamLog.Text = "";
            });
            
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                FFMpegView.CloseAllFFMPEGS();
                //
                Codes.emDataGridOptions.SaveGridOptions(this.playlistsV.dgPlaylist);
                Codes.emDataGridOptions.SaveGridOptions(this.playlistsV.filesV.dgMediaItems);
                this.ViewModel.SaveSettings();
                //
                Process.GetCurrentProcess().Kill();
                //
                base.OnClosing(e);
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }

        //void Vg_OnAudioPeak(object sender, TOnAudioPeakEventArgs e)
        //{
        //    if (e.left_DB < 0)
        //        this.vuMeter.ViewModel.LeftVU = 200 - (e.left_DB * -6);
        //    else
        //        this.vuMeter.ViewModel.LeftVU = 0;
        //    //
        //    if (e.right_DB < 0)
        //        this.vuMeter.ViewModel.RightVU = 200 - (e.right_DB * -6);
        //    else
        //        this.vuMeter.ViewModel.RightVU = 0;
        //    this.vuMeter.ViewModel.CounterUpdate = 0;
        //}

        public void btnSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = this.ViewModel.OutputSettingVM.Clone();
            var wnd = new OutputSettingsView(vm);
            if (wnd.ShowDialog() == true)
            {
                this.ViewModel.OutputSettingVM.VideoDeviceName = wnd.ViewModel.VideoDeviceName;
                this.ViewModel.OutputSettingVM.AudioDeviceName = wnd.ViewModel.AudioDeviceName;
                this.ViewModel.OutputSettingVM.VideoSize = wnd.ViewModel.VideoSize;
                this.ViewModel.OutputSettingVM.DefaultDir_MediaFiles = wnd.ViewModel.DefaultDir_MediaFiles;
                this.ViewModel.OutputSettingVM.DefaultDir_Playlists = wnd.ViewModel.DefaultDir_Playlists;
                this.ViewModel.OutputSettingVM.PlayoutLog = wnd.ViewModel.PlayoutLog;
                this.ViewModel.OutputSettingVM.OutputToVCam = wnd.ViewModel.OutputToVCam;
                this.ViewModel.OutputSettingVM.VCamChannel = wnd.ViewModel.VCamChannel;
                this.ViewModel.OutputSettingVM.AlternateTimeZone = wnd.ViewModel.AlternateTimeZone;
                this.ViewModel.OutputSettingVM.ChannelName = wnd.ViewModel.ChannelName;
            }
        }
        
        public void btnSchedule_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Views.SchedulingView wnd = new SchedulingView(this.ViewModel.SchedulingsVM);
            wnd.Show();
        }
        public void btnGlobalSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = this.ViewModel.GlobalSettingVM;
            //var wnd = new GlobalSettingsView(vm.StringOverlay, vm.ImageOverlay, vm.CrawlOverlay);
            var ovm = new MediaSourceViewModel(null)
            {
                ImageOverlay = vm.ImageOverlay.Clone(),
                StringOverlay = vm.StringOverlay.Clone(),
                CrawlOverlay = vm.CrawlOverlay.Clone(),
                VideoOverlay = null
            };
            var wnd = new OverlayMediaView(ovm, true);
            if (wnd.ShowDialog() == true)
            {
                //Scale Positions
                //
                vm.ImageOverlay = wnd.ViewModel.ImageOverlay;
                vm.StringOverlay = wnd.ViewModel.StringOverlay;
                vm.CrawlOverlay = wnd.ViewModel.CrawlOverlay;
                vm.OperateSettings();
            }
        }
        private void btnActivation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Lock.ActivationView wnd = new Lock.ActivationView();
            wnd.ShowDialog();
            this.ViewModel.NotifyOfPropertyChange(() => this.ViewModel.IsRegistered);
        }
        public void btnTimerSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = this.ViewModel.TimerSettingVM.Clone();
            Views.TimerSettingsView wnd = new TimerSettingsView(vm);
            if (wnd.ShowDialog() == true)
            {
                this.ViewModel.TimerSettingVM.TimerMode = vm.TimerMode;
                this.ViewModel.TimerSettingVM.Time = vm.Time;
                this.ViewModel.TimerSettingVM.Operate();
                //
                this.playerControl.btnPlay.IsEnabled = (this.ViewModel.TimerSettingVM.TimerMode == TimersMode.None);
            }
        }
        private void PlayButtonEnableChanged(bool enable)
        {
            if (this.ViewModel.TimerSettingVM.TimerMode != TimersMode.None)
                return;
            //
            this.btnTimerSettings.IsEnabled = enable;
        }
        private void Timer_OnTimeReceived()
        {
            this.playerControl.PerformPlay();
            //this.playerControl.PerformStateChanged();
        }

        private void itemLoopIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ViewModel.PlayerControlVM.CurrentMediaSource == null)
                return;
            this.ViewModel.PlayerControlVM.CurrentMediaSource.Loop = !this.ViewModel.PlayerControlVM.CurrentMediaSource.Loop;
        }

        private void iconLoopPlaylist_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ViewModel.PlayerControlVM.CurrentPlaylist == null)
                return;
            this.ViewModel.PlayerControlVM.CurrentPlaylist.Loop = !this.ViewModel.PlayerControlVM.CurrentPlaylist.Loop;
        }

        public void btnStreamOut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = this.ViewModel.StreamVM.Clone();
            FFMpegView view = new FFMpegView(vm);
            if (view.ShowDialog() == true)
            {
                var svm = this.ViewModel.StreamVM;
                //
                svm.SelectedProfileName = view.ViewModel.SelectedProfileName;
                svm.Profiles = view.ViewModel.Profiles;
            }
        }

        private void OnRecordClicked()
        {
            var vm = this.ViewModel.RecordVM.Clone();
            var wnd = new RecordView(vm);
            if (wnd.ShowDialog() == true)
            {
                this.ViewModel.RecordVM.FileFormat = wnd.ViewModel.FileFormat;
                this.ViewModel.RecordVM.FileName = wnd.ViewModel.FileName;
                this.ViewModel.RecordVM.FrameRate = wnd.ViewModel.FrameRate;
                this.ViewModel.RecordVM.VideoBitrate = wnd.ViewModel.VideoBitrate;
                this.ViewModel.RecordVM.AudioBitrate = wnd.ViewModel.AudioBitrate;
                this.ViewModel.RecordVM.FrameSize = wnd.ViewModel.FrameSize;
                this.ViewModel.RecordVM.SampleRate = wnd.ViewModel.SampleRate;
                this.ViewModel.RecordVM.VCamChannel = wnd.ViewModel.VCamChannel;
                this.ViewModel.RecordVM.EncoderProgram = wnd.ViewModel.EncoderProgram;
                this.ViewModel.RecordVM.SegmentSecond = wnd.ViewModel.SegmentSecond;
            }
        }

        private async void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ViewModel.SaveSettings();
                await Functions.ShowMessage("Save Settings", "All settings was saved successfully.");
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

    }
}
