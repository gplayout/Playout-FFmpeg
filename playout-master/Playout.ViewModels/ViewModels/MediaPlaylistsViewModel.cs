using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playout.Base;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using Playout.ApplicationService;
using Playout.Models;
using System.Timers;
using Playout.MediaFramework;
using Playout.Log;
using Playout.DirectShow.MediaPlayers;

namespace Playout.ViewModels.ViewModels
{
    public class MediaPlaylistsViewModel : Base.FormBaseViewModel
    {
        ObservableCollection<MediaPlaylistViewModel> _playlists;
        MediaPlaylistViewModel _selectedPlaylist;
        bool _loopOnPlaylists;
        bool _playbackMode;
        System.Timers.Timer tim2 = null;
        bool inTimer = false;

        public Action OnPlaybackModeChanged { get; set; }
        public Action OnSelectedItemChangedInPlaybackMode { get; set; }

        IMediaPlaylistAppService mpAppService;

        public ObservableCollection<MediaPlaylistViewModel> Playlists
        {
            get
            { return this._playlists; }
            set
            {
                if (this._playlists != value)
                {
                    this._playlists = value;
                    this.NotifyOfPropertyChange(() => this.Playlists);
                }
            }
        }
        public ObservableCollection<MediaSourceViewModel> AllSources
        {
            get
            {
                if (this.Playlists == null)
                    return null;
                //
                ObservableCollection<MediaSourceViewModel> items = new ObservableCollection<MediaSourceViewModel>();
                foreach (var pl in this.Playlists)
                {
                    foreach (var src in pl.MediaSourcesVM.Sources)
                        items.Add(src);
                }
                return items;
            }
        }
        public MediaPlaylistViewModel SelectedPlaylist
        {
            get
            { return this._selectedPlaylist; }
            set
            {
                if (this._selectedPlaylist != value)
                {
                    this._selectedPlaylist = value;
                    this.NotifyOfPropertyChange(() => this.SelectedPlaylist);
                }
            }
        }
        public bool LoopOnPlaylists
        {
            get { return this._loopOnPlaylists; }
            set
            {
                if (this._loopOnPlaylists != value)
                {
                    this._loopOnPlaylists = value;
                    this.NotifyOfPropertyChange(() => this.LoopOnPlaylists);
                }
            }
        }
        public bool PlaybackMode
        {
            get { return this._playbackMode; }
            set
            {
                if (this._playbackMode != value)
                {
                    this._playbackMode = value;
                    this.NotifyOfPropertyChange(() => this.PlaybackMode);
                    if (this.OnPlaybackModeChanged != null)
                        this.OnPlaybackModeChanged();
                }
            }
        }
        public MediaPlaylistsViewModel(IMediaPlaylistAppService _mpAppService)
        {
            this.mpAppService = _mpAppService;
            //
            this.tim2 = new System.Timers.Timer(500);
            this.tim2.Elapsed += tim2_Elapsed;
            this.tim2.Start();
        }

        void tim2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (inTimer)
                    return;
                //
                inTimer = true;
                var sourceItems = this.AllSources.Where(m =>
                    (m.MediaType == enMediaType.VideoFile || m.MediaType == enMediaType.ImageFile || m.MediaType == enMediaType.DVD) &&
                    (!m.GetInfoStatus.HasValue || m.GetInfoStatus.Value == false) &&
                    String.IsNullOrEmpty(m.WidthHeight)).Take(2);
                //
                MediaPlaylistsViewModel.LoadMediaSourceInfo(sourceItems.ToArray());
            }
            catch
            {

            }
            finally
            {
                inTimer = false;
            }
        }
        public static void LoadMediaSourceInfo(MediaSourceViewModel[] items)
        {
            foreach (var item in items)
            {
                long duration = 0;
                int videoWidth = 0;
                int videoHeight = 0;
                int frameRate = 0;
                //
                bool result = false;
                if (item.MediaType == enMediaType.DVD)
                {
                    //result = Program.Vg.GetDvdInfo(out duration, out frameCount, out videoWidth, out videoHeight,
                    //    out videoFrameRateFps);
                }
                else
                {
                    if (!File.Exists(item.File_Url))
                    {
                        item.GetInfoStatus = true;
                        continue;
                    }
                    //
                    result = Program.Dg.GetFileInfo(item.File_Url, ref duration, ref videoWidth, ref videoHeight, ref frameRate);
                }
                //
                if (result)
                {
                    item.Duration = TimeSpan.FromSeconds(duration / 10e6);
                    item.WidthHeight = String.Format("{0}x{1}", videoWidth, videoHeight);
                    item.FrameRate = frameRate.ToString();
                }
                //
                item.GetInfoStatus = true;
            }
        }
        
      
        public bool HasAnyFile()
        {
            if (this.Playlists == null)
                return false;
            //
            bool result = this.Playlists.Any(m => m.MediaSourcesVM.Sources.Count() > 0);
            return result;
        }
        public string GetNewPlaylistName()
        {
            string name = "Playlist" + this.Playlists.Count;
            while (this.Playlists.Where(m => m.Name == name).Count() > 0)
            {
                name += "1";
            }
            return name;
        }
        public MediaPlaylistViewModel NewPlaylist()
        {
            MediaPlaylistViewModel pvm = MediaPlaylistViewModel.GetNew(this.GetNewPlaylistName());
            this.Playlists.Add(pvm);
            //
            return pvm;
        }

        public void SavePlaylist(MediaPlaylistViewModel pvm)
        {
            MediaPlaylistModel model = pvm.GetModel();
            this.mpAppService.SaveFile(model);
        }

        public MediaPlaylistViewModel OpenPlaylist()
        {
            MediaPlaylistModel model = mpAppService.OpenFile();
            if (model == null)
                return null;
            //
            var pvm = new MediaPlaylistViewModel(model);
            //
            if (pvm != null)
                this.Playlists.Add(pvm);
            //
            return pvm;
        }

        public MediaPlaylistViewModel LoadFile(string filePath)
        {
            MediaPlaylistModel model = mpAppService.LoadFile(filePath);
            if (model == null)
                return null;
            //
            var pvm = new MediaPlaylistViewModel(model);
            //
            if (pvm != null)
                this.Playlists.Add(pvm);
            //
            return pvm;
        }

        public void RemovePlaylist(MediaPlaylistViewModel pvm)
        {
            this.Playlists.Remove(pvm);
        }

        public void ChangeOrderMediaSource(MediaPlaylistViewModel[] items, bool toUp)
        {
            if (items == null || items.Length == 0 || this.Playlists.Count < 1)
                return;
            //
            if (toUp)
            {
                int indexTarget = this.Playlists.IndexOf(items.OrderBy(m => this.Playlists.IndexOf(m)).ToList()[0]) - 1;
                if (indexTarget < 0)
                    return;
                //
                foreach (var item in items.OrderBy(m => this.Playlists.IndexOf(m)))
                {
                    int index = this.Playlists.IndexOf(item);
                    this.Playlists.Move(index, indexTarget);
                }
            }
            else
            {
                int indexTarget = this.Playlists.IndexOf(items.OrderByDescending(m => this.Playlists.IndexOf(m)).ToList()[0]) + 1;
                if (indexTarget >= this.Playlists.Count)
                    return;
                //
                foreach (var item in items.OrderByDescending(m => this.Playlists.IndexOf(m)))
                {
                    int index = this.Playlists.IndexOf(item);
                    this.Playlists.Move(index, indexTarget);
                }
            }
        }

        public void LoadPlaylists(List<MediaPlaylistModel> plModels, bool loopOnPlaylists, bool playbackMode)
        {
            if (plModels == null)
                return;
            //
            this.LoopOnPlaylists = loopOnPlaylists;
            this.PlaybackMode = playbackMode;
            this.Playlists = new ObservableCollection<MediaPlaylistViewModel>(
                        plModels.Select(m => new MediaPlaylistViewModel(m)));
            if (this.Playlists.Count > 0)
            {
                this.SelectedPlaylist = this.Playlists[0];
                if(this.SelectedPlaylist.MediaSourcesVM.Sources.Count>0)
                {
                    this.SelectedPlaylist.MediaSourcesVM.SelectedSource = this.SelectedPlaylist.MediaSourcesVM.Sources[0];
                }
            }
        }
        public void AddPlaylist(MediaPlaylistViewModel item, int index)
        {
            if (index == -1)
                this.Playlists.Add(item);
            else
                this.Playlists.Insert(index, item);
        }

        public List<MediaPlaylistModel> GetPlaylistModels(out bool loopOnPlaylists,out bool playbackMode)
        {
            loopOnPlaylists = false;
            playbackMode = false;
            try
            {
                List<MediaPlaylistModel> models = new List<MediaPlaylistModel>(
                    this.Playlists.Select(m => m.GetModel()));
                loopOnPlaylists = this.LoopOnPlaylists;
                playbackMode = this.PlaybackMode;
                //
                return models;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public void AddPlaylists()
        {
            var dlgOpen = new System.Windows.Forms.OpenFileDialog();
            dlgOpen.Multiselect = true;
            dlgOpen.Filter = Program.PLAYLISTAndSCHEDULE_FILES;
            dlgOpen.InitialDirectory = Program.DefaultDir_Playlists;
            if (dlgOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in dlgOpen.FileNames)
                    this.LoadFile(file);
            }
        }
    }
}
