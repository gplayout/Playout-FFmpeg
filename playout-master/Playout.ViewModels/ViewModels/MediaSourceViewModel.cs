using Framework.UI.Input;
using Playout.ApplicationService;
using Playout.Base;
using Playout.MediaFramework;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.ViewModels.ViewModels
{
    public class MediaSourceViewModel : Base.FormBaseViewModel
    {
        
        private readonly DelegateCommand<MediaSourceViewModel> moveAboveCommand;
        private readonly DelegateCommand<MediaSourceViewModel> moveBelowCommand;
        
        public ObservableCollection<MediaSourceViewModel> Items { get; private set; }

        #region Fields
        enMediaType _mediaType;
        TransitionEffectType _showupEffect;
        string _file_url;
        string _category;
        string _aliasName;
        string _deviceName;
        string _widthHeight;
        TimeSpan? _duration;
        string _frameRate;
        DateTime? _startTime;
        Guid _thumbnailId;
        bool _loadThumb;
        bool _isCurrent;
        long _trimStartSecond;
        long _trimStopSecond;
        int? _colorPlaylist;
        bool _loop;
        string _inputDeviceFormat;
        BitmapSource _thumbnailImageForVideoFile;
        TimeSpan _previewOfVideoFilePosition;

        StringOverlayMediaViewModel _stringOverlay;
        CrawlOverlayMediaViewModel _crawlOverlay;
        ImageOverlayMediaViewModel _imageOverlay;
        VideoOverlayMediaViewModel _videoOverlay;
        #endregion Fields

        public object CopeidItem
        {
            get { return MediaPlaylistViewModel._copiedItem; }
            set
            {
                if (MediaPlaylistViewModel._copiedItem != value)
                {
                    MediaPlaylistViewModel._copiedItem = value;
                    this.NotifyOfPropertyChange(() => this.CopeidItem);
                }
            }
        }
        public MediaPlaylistModel NestedPlaylist { get; set; }
        public bool? GetInfoStatus { get; set; }
        #region Properties
        public bool IsCurrent 
        { 
            get
            {
                return this._isCurrent;
            }
            set
            {
                if(this._isCurrent!=value)
                {
                    this._isCurrent = value;
                    this.NotifyOfPropertyChange(() => this.IsCurrent);
                }
            }
        }
        public enMediaType MediaType
        {
            get { return this._mediaType; }
            set
            {
                if (this._mediaType != value)
                {
                    this._mediaType = value;
                    this.NotifyOfPropertyChange(() => this.MediaType);
                    this.NotifyOfPropertyChange(() => this.File_Url_DeviceName);
                    this.NotifyOfPropertyChange(() => this.IsVideoFile);
                }
            }
        }
        public string[] TransitionEffectTypes
        {
            get { return Enum.GetNames(typeof(TransitionEffectType)); }
        }
        public TransitionEffectType ShowupEffect
        {
            get { return this._showupEffect; }
            set
            {
                if (this._showupEffect != value)
                {
                    this._showupEffect = value;
                    this.NotifyOfPropertyChange(() => this.ShowupEffect);
                }
            }
        }
        public bool IsVideoFile
        {
            get
            {
                return this.MediaType == enMediaType.VideoFile;
            }
        }
        public string Category
        {
            get
            {
                return this._category;
            }
            set
            {
                if (this._category != value)
                {
                    this._category = value;
                    this.NotifyOfPropertyChange(() => this.Category);
                    this.NotifyOfPropertyChange(() => this.CategoryIcon);
                }
            }
        }
        public ImageSource CategoryIcon
        {
            get
            {
                if (String.IsNullOrEmpty(this.Category) || !CategoryWorker.Categories.ContainsKey(this.Category))
                    return null;
                //
                return CategoryWorker.Categories[this.Category];
            }
        }
        public string AliasName
        {
            get
            {
                return this._aliasName;
            }
            set
            {
                if(this._aliasName!=value)
                {
                    this._aliasName = value;
                    this.NotifyOfPropertyChange(() => this.AliasName);
                }
            }
        }
        public string FileName
        {
            get
            {
                if (!String.IsNullOrEmpty(this.File_Url))
                    return System.IO.Path.GetFileName(this.File_Url);
                else
                    return "";
            }
        }
        public string DeviceName
        {
            get { return this._deviceName; }
            set
            {
                if (this._deviceName != value)
                {
                    this._deviceName = value;
                    this.NotifyOfPropertyChange(() => this.DeviceName);
                    this.NotifyOfPropertyChange(() => this.File_Url_DeviceName);
                    this.NotifyOfPropertyChange(() => this.InputNameAbbrivated);
                    this.NotifyOfPropertyChange(() => this.InputDeviceFormats);
                }
            }
        }
        public string File_Url
        {
            get { return this._file_url; }
            set
            {
                if (this._file_url != value)
                {
                    this._file_url = value;
                    this.NotifyOfPropertyChange(() => this.File_Url);
                    this.NotifyOfPropertyChange(() => this.FileName);
                    this.NotifyOfPropertyChange(() => this.Extention);
                    this.NotifyOfPropertyChange(() => this.File_Url_DeviceName);
                    this.NotifyOfPropertyChange(() => this.InputNameAbbrivated);
                    this.NotifyOfPropertyChange(() => this.ThumbnailImage);
                    this.NotifyOfPropertyChange(() => this.FileExist);
                }
            }
        }
        public string[] InputDeviceFormats
        {
            get
            {
                return Program.Dg.GetInputVideoFormats(this._deviceName);
            }
        }
        public string InputDeviceFormat
        {
            get { return this._inputDeviceFormat; }
            set
            {
                if (this._inputDeviceFormat != value)
                {
                    this._inputDeviceFormat = value;
                    this.NotifyOfPropertyChange(() => this.InputDeviceFormat);
                }
            }
        }
        public string File_Url_DeviceName
        {
            get 
            {
                if (this._mediaType == enMediaType.Device)
                    return this.DeviceName;
                else
                    return this.File_Url;
            }
        }
        public string InputNameAbbrivated
        {
            get
            {
                if (String.IsNullOrEmpty(this.File_Url_DeviceName))
                    return "";
                else
                {
                    string fileName = this.File_Url_DeviceName;
                    if (this.File_Url_DeviceName.IndexOf("\\") != -1)
                        fileName = this.File_Url_DeviceName.Substring(this.File_Url_DeviceName.LastIndexOf("\\") + 1);
                    //
                    if (fileName.Length > 50)
                        fileName= fileName.Substring(0, 50) + "...";
                    //
                    return "Current Item: " + fileName;
                }
            }
        }
        public bool FileExist
        {
            get
            {
                if (String.IsNullOrEmpty(this.File_Url) || this.MediaType == enMediaType.Url || this.MediaType == enMediaType.Device || this.MediaType == enMediaType.DVD)
                    return true;
                //
                return System.IO.File.Exists(this.File_Url);
            }
        }
        public string Extention
        {
            get 
            {
                if (!String.IsNullOrEmpty(this.File_Url))
                    return System.IO.Path.GetExtension(this.File_Url);
                else
                    return "";
            }
        }
        public string WidthHeight
        {
            get { return this._widthHeight; }
            set
            {
                if (this._widthHeight != value)
                {
                    this._widthHeight = value;
                    this.NotifyOfPropertyChange(() => this.WidthHeight);
                }
            }
        }
        public TimeSpan? Duration
        {
            get { return this._duration; }
            set
            {
                if (this._duration != value)
                {
                    this._duration = value;
                    this.NotifyOfPropertyChange(() => this.Duration);
                    this.NotifyOfPropertyChange(() => this.TrimDuration);
                }
            }
        }
        public DateTime? StartTime
        {
            get { return this._startTime; }
            set
            {
                if (this._startTime != value)
                {
                    this._startTime = value;
                    this.NotifyOfPropertyChange(() => this.StartTime);
                    this.NotifyOfPropertyChange(() => this.EndTime);
                }
            }
        }
        public DateTime? EndTime
        {
            get 
            {
                if (this.StartTime != null && this.Duration != null)
                    return this.StartTime.Value.Add(this.Duration.Value);
                else
                    return null;
            }
            
        }
        public long TrimStartSecond
        {
            get { return this._trimStartSecond; }
            set
            {
                if (this._trimStartSecond != value)
                {
                    this._trimStartSecond = value;
                    this.NotifyOfPropertyChange(() => this.TrimStartSecond);
                    this.NotifyOfPropertyChange(() => this.TrimDuration);
                    this.NotifyOfPropertyChange(() => this.Trim);
                }
            }
        }
        public long TrimStopSecond
        {
            get { return this._trimStopSecond; }
            set
            {
                if (this._trimStopSecond != value)
                {
                    this._trimStopSecond = value;
                    this.NotifyOfPropertyChange(() => this.TrimStopSecond);
                    this.NotifyOfPropertyChange(() => this.TrimDuration);
                    this.NotifyOfPropertyChange(() => this.Trim);
                }
                //
                if (this.MediaType == enMediaType.Playlist && this.NestedPlaylist != null)
                {
                    long totalDuration = 0;
                    foreach (var item in this.NestedPlaylist.MediaSources)
                    {
                        long duration = 0;
                        if (item.MediaType == enMediaType.VideoFile)
                        {
                            bool result = Program.Dg.GetFileDuration(item.File_Url, ref duration);
                            if (result)
                                totalDuration += (long)(duration / 10e6);
                        }
                        else
                            totalDuration += this.TrimStopSecond;
                    }
                    this.Duration = TimeSpan.FromSeconds(totalDuration);
                }
                else if (this.MediaType == enMediaType.ImageFile)
                {
                    this.Duration = TimeSpan.FromSeconds(this.TrimStopSecond == 0 ? 1 : this.TrimStopSecond);
                }
                else if (this.MediaType != enMediaType.VideoFile)
                {
                    this.Duration = TimeSpan.FromSeconds(this.TrimStopSecond == 0 ? 0 : this.TrimStopSecond);
                }
            }
        }
        public TimeSpan TrimDuration
        {
            get
            {
                TimeSpan timspan;
                if (this.MediaType == enMediaType.VideoFile)
                {
                    if (this.TrimStopSecond == 0 && this.TrimStartSecond == 0)
                        timspan = this.Duration ?? TimeSpan.FromSeconds(0);
                    else if (this.TrimStopSecond == 0 && this.TrimStartSecond != 0)
                        timspan = TimeSpan.FromSeconds(this.Duration == null ? 0 : this.Duration.Value.TotalSeconds - this.TrimStartSecond);
                    else if (this.TrimStopSecond != 0 && this.TrimStartSecond == 0)
                        timspan = TimeSpan.FromSeconds(this.TrimStopSecond);
                    else
                        timspan = TimeSpan.FromSeconds(this.TrimStopSecond - this.TrimStartSecond);
                }
                else
                {
                    timspan = this.Duration ?? TimeSpan.FromSeconds(0);
                }
                return timspan;
            }
        }
        public string Trim
        {
            get
            {
                return this.TrimStopSecond > 0 || this.TrimStartSecond>0 ? "[ ]" : "";
            }
        }
        public Guid ThumbnailId
        {
            get { return this._thumbnailId; }
            set
            {
                if (this._thumbnailId != value)
                {
                    this._thumbnailId = value;
                    this.NotifyOfPropertyChange(() => this.ThumbnailId);
                    this.NotifyOfPropertyChange(() => this.ThumbnailImage);
                }
            }
        }
        public bool LoadThumb
        { get { return this._loadThumb; }
            set 
            {
                if(this._loadThumb!=value)
                {
                    this._loadThumb = value;
                    this.NotifyOfPropertyChange(() => this.LoadThumb);
                    this.NotifyOfPropertyChange(() => this.ThumbnailImage);
                }
            }
        }
        public BitmapSource ThumbnailImage
        {
            get
            {
                if (!this.LoadThumb)
                    return null;
                if (this.MediaType == enMediaType.ImageFile && !String.IsNullOrEmpty(this.File_Url))
                    return ThumbnailWorker.GetThumbnail(this.File_Url);
                else if (this.ThumbnailId == Guid.Empty)
                    return null;
                else
                {
                    return ThumbnailWorker.GetThumbnail(this.ThumbnailId);
                }
            }
        }
        public TimeSpan PreviewOfVideoFilePosition 
        { 
            get
            {
                if (this._previewOfVideoFilePosition == null)
                    return TimeSpan.FromSeconds(0);
                else
                    return this._previewOfVideoFilePosition;
            }
            set
            {
                if(this._previewOfVideoFilePosition!=value)
                {
                    this._previewOfVideoFilePosition = value;
                    this.NotifyOfPropertyChange(() => this.PreviewOfVideoFilePosition);
                }
            }
        }
        public BitmapSource ThumbnailImageForVideoFile
        {
            get { return this._thumbnailImageForVideoFile; }
            set
            {
                if(this._thumbnailImageForVideoFile!=value)
                {
                    this._thumbnailImageForVideoFile = value;
                    this.NotifyOfPropertyChange(() => this.ThumbnailImageForVideoFile);
                }
            }
        }
        public string FrameRate
        {
            get { return this._frameRate; }
            set
            {
                if (this._frameRate != value)
                {
                    this._frameRate = value;
                    this.NotifyOfPropertyChange(() => this.FrameRate);
                }
            }
        }
       
        public int? ColorPlaylist
        {
            get { return this._colorPlaylist; }
            set
            {
                if(this._colorPlaylist!=value)
                {
                    this._colorPlaylist = value;
                    this.NotifyOfPropertyChange(() => this.ColorPlaylist);
                    this.NotifyOfPropertyChange(() => this.ColorPlaylistBrush);
                }
            }
        }
        public int Index
        {
            get 
            {
                if (this.Items == null)
                    return 0;
                //
                return this.Items.IndexOf(this) + 1;
            }
        }
        public SolidColorBrush ColorPlaylistBrush
        {
            get
            {
                return new SolidColorBrush(Functions.ConvertIntToColor(this.ColorPlaylist ?? 0));
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
        public StringOverlayMediaViewModel StringOverlay
        {
            get { return this._stringOverlay; }
            set
            {
                if (this._stringOverlay != value)
                {
                    this._stringOverlay = value;
                    this.NotifyOfPropertyChange(() => this.StringOverlay);
                }
            }
        }
        public CrawlOverlayMediaViewModel CrawlOverlay
        {
            get { return this._crawlOverlay; }
            set
            {
                if (this._crawlOverlay != value)
                {
                    this._crawlOverlay = value;
                    this.NotifyOfPropertyChange(() => this.CrawlOverlay);
                }
            }
        }
        public ImageOverlayMediaViewModel ImageOverlay
        {
            get { return this._imageOverlay; }
            set
            {
                if (this._imageOverlay != value)
                {
                    this._imageOverlay = value;
                    this.NotifyOfPropertyChange(() => this.ImageOverlay);
                }
            }
        }
        public VideoOverlayMediaViewModel VideoOverlay
        {
            get { return this._videoOverlay; }
            set
            {
                if (this._videoOverlay != value)
                {
                    this._videoOverlay = value;
                    this.NotifyOfPropertyChange(() => this.VideoOverlay);
                }
            }
        }
        #endregion Properties

        public DelegateCommand<MediaSourceViewModel> MoveAboveCommand
        {
            get { return this.moveAboveCommand; }
        }
        public DelegateCommand<MediaSourceViewModel> MoveBelowCommand
        {
            get { return this.moveBelowCommand; }
        }
        public MediaSourceViewModel(ObservableCollection<MediaSourceViewModel> _items)
        {
            this.moveAboveCommand = new DelegateCommand<MediaSourceViewModel>(this.MoveAbove, this.CanMoveAbove);
            this.moveBelowCommand = new DelegateCommand<MediaSourceViewModel>(this.MoveBelow, this.CanMoveBelow);
            this.Items = _items;
            //
            this.InitialMediaInfo();
        }
        private void InitialMediaInfo()
        {
            this.StringOverlay = new StringOverlayMediaViewModel();
            this.CrawlOverlay = new CrawlOverlayMediaViewModel();
            this.ImageOverlay = new ImageOverlayMediaViewModel();
            this.VideoOverlay = new VideoOverlayMediaViewModel();
            this.DeviceName = "";
            this.Duration = TimeSpan.FromSeconds(0);
        }      
        public static TimeSpan GetDurationFiles(string[] files)
        {
            long totalDuration = 0;
            foreach (var file in files)
            {
                long duration = 0;
                bool result = Program.Dg.GetFileDuration(file, ref duration);
                if (result)
                    totalDuration += duration;
            }
            return TimeSpan.FromSeconds(totalDuration / 10e6);
        }
        public static MediaSourceViewModel AddFileSource(String file, ObservableCollection<MediaSourceViewModel> items)
        {
            string extention = System.IO.Path.GetExtension(file).ToLower();
            if (Program.OPEN_MEDIA_FILES.Contains(extention))
            {
                MediaSourceViewModel fm = new MediaSourceViewModel(items)
                {
                    DeviceName = "",
                    InputDeviceFormat="",
                    MediaType = Program.IMAGE_FILES.Contains(extention) ? enMediaType.ImageFile : enMediaType.VideoFile,
                    File_Url = file,
                    AliasName = System.IO.Path.GetFileNameWithoutExtension(file),
                    Category = "",
                };
                return fm;
            }
            else if (Program.PLAYLIST_Ext == extention || Program.SCHEDULE_Ext == extention)
            {
                IMediaPlaylistAppService mpAppService = (IMediaPlaylistAppService)DI.NinjectConfig.kernel.GetService(typeof(IMediaPlaylistAppService));
                MediaPlaylistModel model = mpAppService.LoadFile(file);
                if (model == null)
                    return null;
                MediaPlaylistViewModel plvm = new MediaPlaylistViewModel(model);
                //
                MediaSourceViewModel fm = new MediaSourceViewModel(items)
                {
                    DeviceName = "",
                    InputDeviceFormat="",
                    Category = model.Name,
                    AliasName = "",
                    MediaType = enMediaType.Playlist,
                    File_Url = file,
                    Loop = model.Loop,
                    Duration = TimeSpan.FromSeconds(plvm.MediaSourcesVM.Sources.Sum(m => m.Duration == null ? 0 : m.Duration.Value.TotalSeconds)),
                    StringOverlay = new StringOverlayMediaViewModel(),
                    CrawlOverlay = new CrawlOverlayMediaViewModel(),
                    ImageOverlay = new ImageOverlayMediaViewModel(),
                    VideoOverlay = new VideoOverlayMediaViewModel(),
                    NestedPlaylist = model,
                };
                return fm;
            }
            //
            return null;
        }
        public MediaSourceViewModel Clone()
        {
            MediaSourceViewModel vm = new MediaSourceViewModel(this.Items)
            {
                Category = this.Category,
                AliasName = this.AliasName,
                DeviceName = this.DeviceName,
                InputDeviceFormat=this.InputDeviceFormat,
                Duration = this.Duration,
                File_Url = this.File_Url,
                FrameRate = this.FrameRate,
                MediaType = this.MediaType,
                StartTime = this.StartTime,
                Loop = this.Loop,
                TrimStartSecond = this.TrimStartSecond,
                TrimStopSecond = this.TrimStopSecond,// == 0 ? (this.Duration == null ? 0 : (long)this.Duration.Value.TotalSeconds) : this.TrimStopSecond,
                ThumbnailId = this.ThumbnailId,
                WidthHeight = this.WidthHeight,
                LoadThumb = this.LoadThumb,
                ShowupEffect=this.ShowupEffect,
                StringOverlay = this.StringOverlay.Clone(),
                CrawlOverlay = this.CrawlOverlay.Clone(),
                ImageOverlay = this.ImageOverlay.Clone(),
                VideoOverlay = this.VideoOverlay.Clone(),
            };
            return vm;
        }

        public void CopyValues(MediaSourceViewModel vm,string thumbText)
        {
            this.MediaType = vm.MediaType;
            this.File_Url = vm.File_Url;
            this.InputDeviceFormat = vm.InputDeviceFormat;
            this.ShowupEffect = vm.ShowupEffect;
            this.AliasName = vm.AliasName;
            this.Category = vm.Category;
            this.Loop = vm.Loop;
            if (!String.IsNullOrEmpty(thumbText))
            {
                if (this.ThumbnailId != Guid.Empty)
                {
                    //Delete Last Picture
                    ThumbnailWorker.DeleteThumbnail(this.ThumbnailId);
                }
                var guid = Guid.NewGuid();
                ThumbnailWorker.CreateThumbnail(guid, thumbText);
                this.LoadThumb = true;
                this.ThumbnailId = guid;
            }
            switch (vm.MediaType)
            {
                case enMediaType.ImageFile:
                    this.TrimStopSecond = vm.TrimStopSecond;
                    break;
                case enMediaType.Device:
                    this.DeviceName = vm.DeviceName;
                    this.File_Url = "";
                    this.TrimStopSecond = vm.TrimStopSecond;
                    break;
                case enMediaType.Url:
                    this.TrimStopSecond = vm.TrimStopSecond;
                    break;
                case enMediaType.DVD:
                    this.DeviceName = "DVD";
                    this.File_Url = "";
                    this.TrimStopSecond = vm.TrimStopSecond;
                    break;
                default:
                    break;
            }
        }
        public void IndexNotifyChanged()
        {
            this.NotifyOfPropertyChange(() => this.Index);
        }

        #region Private Methods

        private bool CanMoveAbove(MediaSourceViewModel fund)
        {
            return true;
        }

        private void MoveAbove(MediaSourceViewModel fund)
        {
            this.Move(fund);
        }
        private bool CanMoveBelow(MediaSourceViewModel synonymItem)
        {
            return true;
        }

        private void MoveBelow(MediaSourceViewModel synonymItem)
        {
            this.Move(synonymItem);
        }

        private void Move(MediaSourceViewModel fund)
        {
            int index = this.Items.IndexOf(this);
            this.Items.Move(this.Items.IndexOf(fund), index);
            //
            foreach (var item in this.Items)
            {
                if (item != this)
                {
                    item.MoveAboveCommand.RaiseCanExecuteChanged();
                    item.MoveBelowCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion
    }
}
