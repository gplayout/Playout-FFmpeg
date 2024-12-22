using Playout.ApplicationService;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Base;
using System.Timers;
using Playout.DataService;
using Playout.MediaFramework;
using Playout.Log;
using Playout.DirectShow.MediaPlayers;

namespace Playout.ViewModels.ViewModels
{
    public class MainViewModel : Base.FormBaseViewModel
    {
        PlayerControlViewModel _playerControlVM;
        MediaPlaylistsViewModel _playlistsVM;
        SchedulingsViewModel _schedulingsVM;
        PreviewItemsViewModel _prevVM;
        OutputSettingsViewModel _outputSettingVM;
        GlobalSettingsViewModel _globalSettingVM;
        TimerSettingsViewModel _timerSettingsVM;
        RecordViewModel _recordVM;
        VUMeterViewModel _vuMeterVM;
        StreamViewModel _streamVM;
        
        private static MainViewModel _instance = null;
        public MediaPlaylistsViewModel PlaylistsVM
        {
            get
            { return this._playlistsVM; }
            set
            {
                if (this._playlistsVM != value)
                {
                    this._playlistsVM = value;
                    this.NotifyOfPropertyChange(() => this.PlaylistsVM);
                }
            }
        }
        public SchedulingsViewModel SchedulingsVM
        {
            get
            { return this._schedulingsVM; }
            set
            {
                if (this.SchedulingsVM != value)
                {
                    this._schedulingsVM = value;
                    this.NotifyOfPropertyChange(() => this.SchedulingsVM);
                }
            }
        }
        public PreviewItemsViewModel PrevItemsVM
        {
            get
            { return this._prevVM; }
            set
            {
                if (this._prevVM != value)
                {
                    this._prevVM = value;
                    this.NotifyOfPropertyChange(() => this.PrevItemsVM);
                }
            }
        }
        public OutputSettingsViewModel OutputSettingVM
        {
            get
            { return this._outputSettingVM; }
            set
            {
                if (this._outputSettingVM != value)
                {
                    this._outputSettingVM = value;
                    this.NotifyOfPropertyChange(() => this.OutputSettingVM);
                }
            }
        }
        public GlobalSettingsViewModel GlobalSettingVM
        {
            get
            { return this._globalSettingVM; }
            set
            {
                if (this._globalSettingVM != value)
                {
                    this._globalSettingVM = value;
                    this.NotifyOfPropertyChange(() => this.GlobalSettingVM);
                }
            }
        }
        public PlayerControlViewModel PlayerControlVM
        {
            get
            { return this._playerControlVM; }
            set
            {
                if (this._playerControlVM != value)
                {
                    this._playerControlVM = value;
                    this.NotifyOfPropertyChange(() => this.PlayerControlVM);
                }
            }
        }
        public VUMeterViewModel VUMeterVM
        {
            get
            { return this._vuMeterVM; }
            set
            {
                if (this._vuMeterVM != value)
                {
                    this._vuMeterVM = value;
                    this.NotifyOfPropertyChange(() => this.VUMeterVM);
                }
            }
        }
        public TimerSettingsViewModel TimerSettingVM
        {
            get { return this._timerSettingsVM; }
            set
            {
                if (this._timerSettingsVM != value)
                {
                    this._timerSettingsVM = value;
                    this.NotifyOfPropertyChange(() => this.TimerSettingVM);
                }
            }
        }
        public RecordViewModel RecordVM
        {
            get
            { return this._recordVM; }
            set
            {
                if (this._recordVM != value)
                {
                    this._recordVM = value;
                    this.NotifyOfPropertyChange(() => this.RecordVM);
                }
            }
        }
        public StreamViewModel StreamVM
        {
            get { return this._streamVM; }
            set
            {
                if (this._streamVM != value)
                {
                    this._streamVM = value;
                    this.NotifyOfPropertyChange(() => this.StreamVM);
                }
            }
        }
        

        public bool IsRegistered
        {
            get
            {
                return Program.Lock.IsRegistered;
            }
        }
        IScheduleAppService schAppService;
        IMediaPlaylistAppService mpAppService;
        ISettingsAppService settAppService;
        public MainViewModel(IScheduleAppService _schAppService,
            IMediaPlaylistAppService _mpAppService, ISettingsAppService _settAppService)
        {
            _instance = this;
            //
            this.schAppService = _schAppService;
            this.mpAppService = _mpAppService;
            this.settAppService = _settAppService;
            //
            this.PlaylistsVM = new MediaPlaylistsViewModel(this.mpAppService)
            {
                Playlists = new ObservableCollection<MediaPlaylistViewModel>()
            };
            //
            this.SchedulingsVM = new SchedulingsViewModel(this.schAppService)
            {
                Schedules = new ObservableCollection<ScheduleViewModel>()
            };
            //
            this.PrevItemsVM = new PreviewItemsViewModel()
            {
                Items = new ObservableCollection<MediaSourceViewModel>()
            };
            //
            this.VUMeterVM = new VUMeterViewModel(Program.Dg);
            //
            this.OutputSettingVM = new OutputSettingsViewModel();
            this.GlobalSettingVM = new GlobalSettingsViewModel();
            this.RecordVM = new RecordViewModel();
            this.StreamVM = new StreamViewModel();
            //
            this.TimerSettingVM = new TimerSettingsViewModel();
            //
            this.LoadSettings();
            //
            this.PlayerControlVM = new PlayerControlViewModel(Program.Dg, this.PlaylistsVM);
            this.PlayerControlVM.CurrentMediaSource = this.PlaylistsVM.SelectedPlaylist == null ? null : this.PlaylistsVM.SelectedPlaylist.MediaSourcesVM.SelectedSource;
            this.PlayerControlVM.CurrentPlaylist = this.PlaylistsVM.SelectedPlaylist;
            //
            this.SchedulingsVM.SchedulingStartThread(this.OnScheduledReceived);
        }

        private void OnScheduledReceived()
        {
            if (this.SchedulingsVM.CurrentSchedule == null || this.SchedulingsVM.CurrentSchedule.PlaylistVM == this.PlayerControlVM.CurrentPlaylist)
                return;
            //
            this.PlayerControlVM.PlayScheduledPlaylist(this.SchedulingsVM.CurrentSchedule.PlaylistVM);
        }

        public void LoadSettings()
        {
            try
            {
                SettingsModel model = this.settAppService.LoadSettings();
                if (model == null)
                       return;
                //
                this.PlaylistsVM.LoadPlaylists(model.Playlists,
                    model.GlobalSetting.LoopOnPlaylists, model.GlobalSetting.PlaybackMode);
                this.SchedulingsVM.LoadSchedules(model.Schedules);
                this.OutputSettingVM.Load(model.OutputSetting);
                this.GlobalSettingVM.Load(model.GlobalSetting);
                this.PrevItemsVM.Load(model.PreviewItems);
                this.RecordVM.Load(model.Record);
                this.StreamVM.Load(model.Stream);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public void SaveSettings()
        {
            try
            {
                bool loopOnPlaylists = false;
                bool playbackMode = false;
                //
                SettingsModel model = new SettingsModel()
                {
                    Playlists = this.PlaylistsVM.GetPlaylistModels(out loopOnPlaylists, out playbackMode),
                    Schedules = this.SchedulingsVM.GetScheduleModels(),
                    OutputSetting = this.OutputSettingVM.GetModel(),
                    GlobalSetting = this.GlobalSettingVM.GetModel(),
                    PreviewItems = this.PrevItemsVM.GetModel(),
                    Record = this.RecordVM.GetModel(),
                    Stream = this.StreamVM.GetModel()
                };
                //
                model.GlobalSetting.LoopOnPlaylists = loopOnPlaylists;
                model.GlobalSetting.PlaybackMode = playbackMode;
                //
                this.settAppService.SaveSettings(model);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public static bool AllowAddMediaSource(enMediaType mediaType)
        {
            if (Program.Lock.LockStatus != RoyalLock.LockStatusEnum.Basic ||
                mediaType == enMediaType.VideoFile || mediaType == enMediaType.ImageFile)
            {
                return true;
            }
            //
            bool exist = _instance.PlaylistsVM.Playlists
                .Any(m => m.MediaSourcesVM.Sources.Any(item => item.MediaType == mediaType));
            return !exist;
        }
        public static MediaPlaylistViewModel AddNewMediaPlaylist()
        {
            var pl = _instance.PlaylistsVM.NewPlaylist();
            _instance.PlaylistsVM.SelectedPlaylist = pl;
            return pl;
        }
    }
}
