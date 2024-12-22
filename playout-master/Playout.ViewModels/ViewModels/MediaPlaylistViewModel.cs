using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Base;
using Playout.ApplicationService;
using Playout.Models;
using System.Windows.Media;
using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.ViewModels.ViewModels
{
    public class MediaPlaylistViewModel : Base.FormBaseViewModel
    {
        public static object _copiedItem;
        
        #region Declarations
        string _name;
        MediaSourcesViewModel _mediaSourcesVM;
        string _filePath;
        int _color;
        bool _isCurrent;
        bool _loop;
        #endregion Declarations
        public object CopeidItem
        {
            get { return _copiedItem; }
            set
            {
                if (_copiedItem != value)
                {
                    _copiedItem = value;
                    this.NotifyOfPropertyChange(() => this.CopeidItem);
                }
            }
        }
        public string Name
        {
            get { return this._name; }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    this.NotifyOfPropertyChange(() => this.Name);
                    this.NotifyOfPropertyChange(() => this.InputNameAbbrivated);
                }
            }
        }
        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                if (this._filePath != value)
                {
                    this._filePath = value;
                    this.NotifyOfPropertyChange(() => this.FilePath);
                }
            }
        }
        public string InputNameAbbrivated
        {
            get
            {
                if (String.IsNullOrEmpty(this.Name))
                    return "";
                else
                {
                    string fileName = this.Name;
                    //
                    if (fileName.Length > 50)
                        fileName = fileName.Substring(0, 50) + "...";
                    //
                    return "Current Playlist: " + fileName;
                }
            }
        }
        public int Color
        {
            get { return this._color; }
            set
            {
                if (this._color != value)
                {
                    this._color = value;
                    this.NotifyOfPropertyChange(() => this.Color);
                    this.NotifyOfPropertyChange(() => this.ColorBrush);
                    //

                    if (this._color != 0 && this.MediaSourcesVM != null && this.MediaSourcesVM.Sources != null)
                    {
                        this.MediaSourcesVM.ColorPlaylist = this._color;
                        this.MediaSourcesVM.Sources.ToList().ForEach((item) =>
                        {
                            item.ColorPlaylist = this._color;
                        });
                    }
                }
            }
        }
        public bool Loop
        {
            get { return this._loop; }
            set
            {
                if (this._loop != value)
                {
                    this._loop = value;
                    this.NotifyOfPropertyChange(() => this.Loop);
                }
            }
        }
        public SolidColorBrush ColorBrush
        {
            get 
            {
                return new SolidColorBrush(Functions.ConvertIntToColor(this.Color));
            }
        }
        public MediaSourcesViewModel MediaSourcesVM
        {
            get { return this._mediaSourcesVM; }
            set
            {
                if (this._mediaSourcesVM != value)
                {
                    this._mediaSourcesVM = value;
                    this.NotifyOfPropertyChange(() => this.MediaSourcesVM);
                }
            }
        }
        public bool IsCurrent
        {
            get
            {
                return this._isCurrent;
            }
            set
            {
                if (this._isCurrent != value)
                {
                    this._isCurrent = value;
                    this.NotifyOfPropertyChange(() => this.IsCurrent);
                }
            }
        }

        public MediaPlaylistViewModel()
        {
            this.Name = "Unnamed Playlist";
            this.MediaSourcesVM = new MediaSourcesViewModel()
            {
                Sources = new ObservableCollection<MediaSourceViewModel>()
            };
            this.Color = Functions.GetRandomColorCode();
            this.Loop = false;
        }

        public static MediaPlaylistViewModel GetNew(string name)
        {
            MediaPlaylistViewModel vm = new MediaPlaylistViewModel();
            //
            vm.Name = String.IsNullOrEmpty(name) ? "Unnamed Playlist" : name;
            vm.MediaSourcesVM = new MediaSourcesViewModel()
            {
                Sources = new ObservableCollection<MediaSourceViewModel>()
            };
            vm.Color = Functions.GetRandomColorCode();
            vm.Loop = false;
            return vm;
        }

        public MediaPlaylistViewModel(MediaPlaylistModel model)
        {
            if (model == null)
                return;
            //
            this.FilePath = model.FilePath;
            this.Name = model.Name;
            this.Loop = model.Loop;
            //
            if (model.MediaSources != null)
            {
                this.MediaSourcesVM = new MediaSourcesViewModel()
                {
                    Sources = new ObservableCollection<MediaSourceViewModel>()
                };
                foreach (var item in model.MediaSources)
                {
                    if (item.MediaType == enMediaType.Playlist && item.NestedPlaylist == null)
                        continue;
                    var vm = new MediaSourceViewModel(this.MediaSourcesVM.Sources)
                    {
                        NestedPlaylist = item.NestedPlaylist,
                        Category = item.Category,
                        AliasName = item.AliasName,
                        MediaType = item.MediaType,
                        DeviceName = item.DeviceName,
                        File_Url = item.File_Url,
                        InputDeviceFormat=item.InputDeviceFormat,
                        ShowupEffect = item.ShowupEffect,
                        TrimStartSecond = item.TrimStartSecond,
                        ThumbnailId = item.ThumbnailId,
                        Loop = item.Loop,
                        StringOverlay = new StringOverlayMediaViewModel(item.StringOverlay),
                        CrawlOverlay = new CrawlOverlayMediaViewModel(item.CrawlOverlay),
                        ImageOverlay = new ImageOverlayMediaViewModel(item.ImageOverlay),
                        VideoOverlay = new VideoOverlayMediaViewModel(item.VideoOverlay),
                    };
                    vm.TrimStopSecond = item.TrimStopSecond;
                    this.MediaSourcesVM.AddFileSource(vm);
                }
            }
            //
            this.Color = model.Color;
        }

        public MediaPlaylistModel GetModel()
        {
            MediaPlaylistModel model = new MediaPlaylistModel()
            {
                FilePath = this.FilePath,
                Name = this.Name,
                Color = this.Color,
                Loop = this.Loop,
            };
            //
            if (this.MediaSourcesVM != null)
            {
                model.MediaSources = new List<MediaSourceModel>(
                    this.MediaSourcesVM.Sources.Select(m => new MediaSourceModel()
                        {
                            Category = m.Category,
                            AliasName = m.AliasName,
                            MediaType = m.MediaType,
                            ShowupEffect = m.ShowupEffect,
                            DeviceName = m.DeviceName,
                            File_Url = m.File_Url,
                            InputDeviceFormat=m.InputDeviceFormat,
                            TrimStartSecond = m.TrimStartSecond,
                            TrimStopSecond = m.TrimStopSecond,
                            ThumbnailId = m.ThumbnailId,
                            Loop = m.Loop,
                            StringOverlay = m.StringOverlay.GetModel(),
                            CrawlOverlay = m.CrawlOverlay.GetModel(),
                            ImageOverlay = m.ImageOverlay.GetModel(),
                            VideoOverlay = m.VideoOverlay.GetModel(),
                            NestedPlaylist = m.NestedPlaylist
                        }));
            }
            //
            return model;
        }

        public MediaPlaylistViewModel Clone()
        {
            MediaPlaylistViewModel so = new MediaPlaylistViewModel()
            {
                FilePath = this.FilePath,
                Name = this.Name,
                Color = this.Color,
                MediaSourcesVM = this.MediaSourcesVM.Clone(),
                Loop = false
            };
            return so;
        }

    }
}
