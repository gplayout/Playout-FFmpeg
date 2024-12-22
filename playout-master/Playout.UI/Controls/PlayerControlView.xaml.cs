using Playout.Base;
using Playout.DirectShow.MediaPlayers;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Playout.Log;
using ffmpegView = Playout.UI.Views.FFMpegView;
//
namespace Playout.UI.Controls
{
    public partial class PlayerControlView
    {
        public Action<bool> PlayButtonEnableChanged;
        public Action OnRecordClick;
        PlayerControlViewModel _viewModel;
        public Action BeforePlay;
        public PlayerControlViewModel ViewModel 
        {
            get { return this._viewModel; }
            set
            {
                this._viewModel = value;
                this._viewModel.OnPlaylistEnd = this.OnPlaylistEnd;
                this.ViewModel.Dg.CurrentPlayer.UIDispatcher = this.Dispatcher;
                this.ViewModel.Dg.CurrentPlayer.OnPlayStatusChanged = this.Dg_OnPlayerStateChanged;
                this._viewModel.SetCurrentMediaSource += this.SetIsCurrentField;
                this.Dg_OnPlayerStateChanged(enPlayStatus.Stopped);

                this.DataContext = this._viewModel;
                //
                if(this._viewModel.Dg.ForPreview)
                {
                    this.btnNext.Visibility =
                        this.btnRecord.Visibility =
                        this.btnPrevious.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    this.btnNext.Visibility =
                        this.btnRecord.Visibility =
                        this.btnPrevious.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        public PlayerControlView()
        {
            InitializeComponent();
        }
        void Vg_OnPreviewStarted(object sender, EventArgs e)
        {
            this.btnPause.IsEnabled =
                    this.btnPlay.IsEnabled =false;
            //
            this.btnStop.IsEnabled =
                this.btnPrevious.IsEnabled =
                this.btnNext.IsEnabled = true;
            //
            this.SetIsCurrentField(null, null);
        }
        void Dg_OnPlayerStateChanged(enPlayStatus playStatus)
        {
            this.SetIsCurrentField(null, null);
            switch(playStatus)
            {
                case enPlayStatus.Playing:
                    this.btnPause.IsEnabled =
                        this.btnStop.IsEnabled =
                        this.btnPrevious.IsEnabled =
                        this.btnRecord.IsEnabled =
                        this.btnNext.IsEnabled = true;
                    //
                    this.btnPlay.IsEnabled = false;                    
                    break;
                case enPlayStatus.Paused:
                    this.btnPause.IsEnabled =
                        this.btnRecord.IsEnabled = false;
                    this.btnPrevious.IsEnabled =
                        this.btnNext.IsEnabled = 
                        this.btnStop.IsEnabled =
                        this.btnPlay.IsEnabled = true;
                    break;
                case enPlayStatus.Stopped:
                    this.btnPause.IsEnabled =
                        this.btnStop.IsEnabled =
                        this.btnPrevious.IsEnabled =
                        this.btnRecord.IsEnabled =
                        this.btnNext.IsEnabled = false;
                    this.btnPlay.IsEnabled = true;
                    break;
                default:
                    return;
            }
        }

        private void OnPlaylistEnd(MediaPlaylistViewModel pvm)
        {
            try
            {
                int index = this.ViewModel.PlaylistsVM.Playlists.IndexOf(pvm);
                index++;
                //
                if (index >= this.ViewModel.PlaylistsVM.Playlists.Count)
                {
                    if (this.ViewModel.PlaylistsVM.LoopOnPlaylists)
                    {
                        index = 0;
                    }
                    else
                    {
                        this.PerformStop();
                        return;
                    }
                }
                //
                var nextPVM = this.ViewModel.PlaylistsVM.Playlists[index];
                //
                this.ViewModel.PlayMediaSource(nextPVM, null);
            }
            catch (Exception)
            { }
        }
        
        public async void PerformPlay()
        {
            try
            {
                if (!this.ViewModel.PlaylistsVM.HasAnyFile())
                {
                    await Functions.ShowMessage("No Media File", "Please input a media file in the list");
                    return;
                }
                if (this.BeforePlay != null)
                    this.BeforePlay();
                //
                var selPItem = this.ViewModel.PlaylistsVM.SelectedPlaylist;
                if (selPItem == null)
                    selPItem = this.ViewModel.PlaylistsVM.Playlists[0];
                //
                if (selPItem.MediaSourcesVM.SelectedSource == null && selPItem.MediaSourcesVM.Sources.Count > 0)
                    selPItem.MediaSourcesVM.SelectedSource = selPItem.MediaSourcesVM.Sources[0];
                //
                var selFileItem = selPItem.MediaSourcesVM.SelectedSource;
                //
                if (this.ViewModel.OnBeforeSetMediaSource_ForPreview != null)
                {
                    this.ViewModel.OnBeforeSetMediaSource_ForPreview(selFileItem.File_Url_DeviceName);
                }
                //
                this.ViewModel.PlayMediaSource(selPItem, selFileItem);
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        public void PerformPause()
        {
            try
            {
                if (this.ViewModel.CurrentMediaSource == null)
                    return;
                //
                this.ViewModel.Dg.PausePlayer();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void PerformStop()
        {
            try
            {
                this.ViewModel.Dg.StopPlayer();
                //
                ffmpegView.CloseAllFFMPEGS();
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        public void PerformNext(bool pervious)
        {
            if ((!pervious && !this.btnNext.IsEnabled) || (pervious && !this.btnPrevious.IsEnabled))
                return;
            //
            this.ViewModel.LogCurrentMediaItem();
            this.ViewModel.PlayNext(this.ViewModel.CurrentPlaylist.Loop, pervious, true);
        }
      
        private void btnPlay_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.PerformPlay();
        }

        private void btnPause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.PerformPause();
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.PerformStop();
        }

      
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.PerformNext(false);
            this.SetIsCurrentField(null, null);
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            this.PerformNext(true);
            this.SetIsCurrentField(null, null);
        }

        private void SetIsCurrentField(object sender,EventArgs e)
        {
            if (this.ViewModel.PlaylistsVM == null)
                return;
            //
            Parallel.ForEach(this.ViewModel.PlaylistsVM.Playlists, (item) =>
            {
                item.IsCurrent = false;
                Parallel.ForEach(item.MediaSourcesVM.Sources, (source) =>
                {
                    source.IsCurrent = false;
                });
            });
            //
            if (this.ViewModel.CurrentMediaSource != null && this.ViewModel.CurrentPlaylist != null)
            {
                this.ViewModel.CurrentMediaSource.IsCurrent = true;
                this.ViewModel.CurrentPlaylist.IsCurrent = true;
                //
                if (this.ViewModel.PlaylistsVM.PlaybackMode)
                {
                    this.ViewModel.CurrentPlaylist.MediaSourcesVM.SelectedSource = this.ViewModel.CurrentMediaSource;
                    this.ViewModel.PlaylistsVM.SelectedPlaylist = this.ViewModel.CurrentPlaylist;
                    if (this.ViewModel.PlaylistsVM.OnSelectedItemChangedInPlaybackMode != null)
                        this.ViewModel.PlaylistsVM.OnSelectedItemChangedInPlaybackMode();
                }
            }
            //
        }

        private void btnPlay_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.PlayButtonEnableChanged != null)
            {
                this.PlayButtonEnableChanged(this.btnPlay.IsEnabled);
            }
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnRecordClick != null)
            {
                this.OnRecordClick();
            }
        }
        
    }
}
