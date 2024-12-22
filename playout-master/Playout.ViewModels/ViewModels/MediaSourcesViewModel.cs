using Playout.Base;
using Playout.MediaFramework;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.ViewModels.ViewModels
{
    public class MediaSourcesViewModel : Base.FormBaseViewModel
    {
        Timer tim = null;
        ObservableCollection<MediaSourceViewModel> _sources;
        MediaSourceViewModel _selectedSource;
        TimeSpan _filesDuration;
        TimeSpan _filesTrimDuration;
        int _sourcesCount;
        int? _colorPlaylist;

        public ObservableCollection<MediaSourceViewModel> Sources
        {
            get
            { return this._sources; }
            set
            {
                if (this._sources != value)
                {
                    this._sources = value;
                    this.NotifyOfPropertyChange(() => this.Sources);
                }
            }
        }
        public MediaSourceViewModel SelectedSource
        {
            get
            { return this._selectedSource; }
            set
            {
                if (this._selectedSource != value)
                {
                    this._selectedSource = value;
                    this.NotifyOfPropertyChange(() => this.SelectedSource);
                }
            }
        }
        public TimeSpan FilesDuration
        {
            get
            { return this._filesDuration; }
            private set
            {
                if (this._filesDuration != value)
                {
                    this._filesDuration = value;
                    this.NotifyOfPropertyChange(() => this.FilesDuration);
                }
            }
        }
        public TimeSpan FilesTrimDuration
        {
            get
            { return this._filesTrimDuration; }
            private set
            {
                if (this._filesTrimDuration != value)
                {
                    this._filesTrimDuration = value;
                    this.NotifyOfPropertyChange(() => this.FilesTrimDuration);
                }
            }
        }
        public int? ColorPlaylist
        {
            get { return this._colorPlaylist; }
            set
            {
                if (this._colorPlaylist != value)
                {
                    this._colorPlaylist = value;
                    this.NotifyOfPropertyChange(() => this.ColorPlaylist);
                }
            }
        }
        public int SourcesCount
        {
            get
            { return this._sourcesCount; }
            private set
            {
                if (this._sourcesCount != value)
                {
                    this._sourcesCount = value;
                    this.NotifyOfPropertyChange(() => this.SourcesCount);
                }
            }
        }
                
        public MediaSourcesViewModel()
        {
            this.tim = new Timer(1000);
            this.tim.Elapsed += tim_Elapsed;
            this.tim.Start();
        }
        
        void tim_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                TimeSpan duration = new TimeSpan(0, 0, 0);
                TimeSpan trimDuration = new TimeSpan(0, 0, 0);
                //
                if (this.Sources == null)
                {
                    this.FilesDuration = duration;
                    this.FilesTrimDuration = duration;
                    this.SourcesCount = 0;
                    return;
                }
                //
                DateTime now = DateTime.Now;
                //
                foreach (var mf in this.Sources)
                {
                    mf.StartTime = now;
                    if (mf.Duration.HasValue)
                    {
                        now = now.Add(mf.Duration.Value);
                        duration = duration.Add(mf.Duration.Value);
                    }
                    trimDuration = trimDuration.Add(mf.TrimDuration);
                }
                //
                this.FilesDuration = duration;
                this.FilesTrimDuration = trimDuration;
                this.SourcesCount = this.Sources.Count;
            }
            catch { }
        }
     
        public void RemoveSource(MediaSourceViewModel item)
        {
            if (item == null)
                return;
            //
            this.Sources.Remove(item);
        }
        public void ChangeOrderMediaSource(MediaSourceViewModel[] items, int indexTarget)
        {
            if (items == null || items.Length == 0 || this.Sources.Count < 1)
                return;
            //
            foreach (var item in items.OrderBy(m => m.Index))
            {
                int index = this.Sources.IndexOf(item);
                this.Sources.Move(index, indexTarget);
            }
            //
            foreach (var item in this.Sources)
                item.IndexNotifyChanged();
        }
        public void ChangeOrderMediaSource(MediaSourceViewModel[] items, bool toUp)
        {
            if (items == null || items.Length == 0 || this.Sources.Count < 1)
                return;
            //
            if (toUp)
            {
                int indexTarget = this.Sources.IndexOf(items.OrderBy(m => m.Index).ToList()[0]) - 1;
                if (indexTarget < 0)
                    return;
                //
                this.ChangeOrderMediaSource(items.OrderBy(m => m.Index).ToArray(), indexTarget);
            }
            else
            {
                int indexTarget = this.Sources.IndexOf(items.OrderByDescending(m => m.Index).ToList()[0]) + 1;
                if (indexTarget >= this.Sources.Count)
                    return;
                //
                this.ChangeOrderMediaSource(items.OrderByDescending(m => m.Index).ToArray(), indexTarget);
            }
        }
        public bool AddFileSource(MediaSourceViewModel vm, int index = -1)
        {
            if (MainViewModel.AllowAddMediaSource(vm.MediaType))
            {
                vm.ColorPlaylist = this.ColorPlaylist;
                if (index == -1)
                    this.Sources.Add(vm);
                else
                    this.Sources.Insert(index, vm);
                //
                foreach (var item in this.Sources)
                    item.IndexNotifyChanged();
                //
                return true;
            }
            else
                return false;
        }
        public void AddFileSource(String file)
        {
            MediaSourceViewModel fm = MediaSourceViewModel.AddFileSource(file, this.Sources);
            if (fm != null)
            {
                this.AddFileSource(fm);
            }
        }
        public void AddObject(object items, int index)
        {
            List<MediaSourceViewModel> itemsToAdd = new List<MediaSourceViewModel>();
            if (items is List<MediaSourceViewModel>)
            {
                foreach (var item in (List<MediaSourceViewModel>)items)
                    itemsToAdd.Add((MediaSourceViewModel)item);
            }
            else if (items is List<MediaPlaylistViewModel>)
            {
                foreach (var item in (List<MediaPlaylistViewModel>)items)
                {
                    var pl = (MediaPlaylistViewModel)item;
                    MediaSourceViewModel fm = new MediaSourceViewModel(this.Sources)
                    {
                        DeviceName = "",
                        InputDeviceFormat="",
                        Category = pl.Name,
                        AliasName = "",
                        MediaType = enMediaType.Playlist,
                        Duration = TimeSpan.FromSeconds(pl.MediaSourcesVM.Sources.Sum(m => m.Duration == null ? 0 : m.Duration.Value.TotalSeconds)),
                        File_Url = pl.FilePath,
                        ShowupEffect = TransitionEffectType.None,
                        Loop = pl.Loop,
                        StringOverlay = new StringOverlayMediaViewModel(),
                        CrawlOverlay = new CrawlOverlayMediaViewModel(),
                        ImageOverlay = new ImageOverlayMediaViewModel(),
                        VideoOverlay = new VideoOverlayMediaViewModel(),
                        NestedPlaylist = pl.GetModel()
                    };
                    itemsToAdd.Add(fm);
                }
            }
            else if(items is MediaSourceViewModel[])
            {
                index = -1;
                foreach (var ni in (MediaSourceViewModel[])items)
                    itemsToAdd.Add(ni);
            }
            //
            foreach (var ms in itemsToAdd)
            {
                this.AddFileSource(ms, index);
            }
        }
        public void AddMediaFiles()
        {
            var dlgOpen = new System.Windows.Forms.OpenFileDialog();
            dlgOpen.Multiselect = true;
            dlgOpen.Filter = Program.OPEN_MEDIA_FILES;
            dlgOpen.InitialDirectory = Program.DefaultDir_MediaFiles;
            if (dlgOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in dlgOpen.FileNames)
                    AddFileSource(file);
            }
        }
        public MediaSourcesViewModel Clone()
        {
            MediaSourcesViewModel so = new MediaSourcesViewModel()
            {
                FilesDuration = this.FilesDuration,
                SourcesCount = this.SourcesCount,
                Sources = new ObservableCollection<MediaSourceViewModel>()
            };
            //
            if (this.Sources != null)
            {
                foreach (var item in this.Sources)
                    so.Sources.Add(item.Clone());
            }
            //
            return so;
        }
        
    }
}
