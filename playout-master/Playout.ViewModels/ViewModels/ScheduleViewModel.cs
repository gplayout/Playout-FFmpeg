using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playout.Base;
using System.Xml.Linq;
using System.IO;
using Playout.Models;
using Playout.ApplicationService;

namespace Playout.ViewModels.ViewModels
{
    public class ScheduleViewModel : Base.FormBaseViewModel
    {
        Days2 _days;
        TimeSpan? _startTime;
        DateTime? _startDate;
        bool _enabled;
        bool _everyDays;
        string _filePath;
        int? _intervalMinutes;
        long _lastRanTicks;
        MediaPlaylistViewModel _playlistVM;

        public Days2 Days
        {
            get { return this._days; }
            set
            {
                if (this._days != value)
                {
                    this._days = value;
                    this.NotifyOfPropertyChange(() => this.Days);
                }
            }
        }
        public TimeSpan? StartTime
        {
            get { return this._startTime; }
            set
            {
                if (this._startTime != value)
                {
                    this._startTime = value;
                    this.NotifyOfPropertyChange(() => this.StartTime);
                }
            }
        }
        
        public DateTime? StartDate
        {
            get { return this._startDate; }
            set
            {
                if (this._startDate != value)
                {
                    this._startDate = value;
                    this.NotifyOfPropertyChange(() => this.StartDate);
                }
            }
        }
        
        public bool Enabled
        {
            get { return this._enabled; }
            set
            {
                if (this._enabled != value)
                {
                    this._enabled = value;
                    this.NotifyOfPropertyChange(() => this.Enabled);
                }
            }
        }
        public bool EveryDay
        {
            get { return this._everyDays; }
            set
            {
                if (this._everyDays != value)
                {
                    this._everyDays = value;
                    this.NotifyOfPropertyChange(() => this.EveryDay);
                }
            }
        }
        public int? IntervalMinutes
        {
            get { return this._intervalMinutes; }
            set
            {
                if (this._intervalMinutes != value)
                {
                    this._intervalMinutes = value;
                    this.NotifyOfPropertyChange(() => this.IntervalMinutes);
                }
            }
        }
        public long LastRanTicks
        {
            get { return this._lastRanTicks; }
            set
            {
                if (this._lastRanTicks != value)
                {
                    this._lastRanTicks = value;
                    this.NotifyOfPropertyChange(() => this.LastRanTicks);
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
        
        public MediaPlaylistViewModel PlaylistVM
        {
            get { return this._playlistVM; }
            set
            {
                if (this._playlistVM != value)
                {
                    this._playlistVM = value;
                    this.NotifyOfPropertyChange(() => this.PlaylistVM);
                }
            }
        }

        public ScheduleViewModel()
        {
            this.EveryDay = false;
            this.Days = Days2.None;
            this.Enabled = false;
            this.StartTime = new TimeSpan(DateTime.Now.TimeOfDay.Hours,DateTime.Now.Minute,DateTime.Now.Second);
            this.StartDate = null;
            this.IntervalMinutes = null;
            this.PlaylistVM = new MediaPlaylistViewModel();
        }
        public ScheduleViewModel(ScheduleModel model)
        {
            this.Days = model.Days;
            this.Enabled = model.Enabled;
            this.EveryDay = model.EveryDay;
            this.StartTime = model.StartTime == null ? null : (TimeSpan?)new TimeSpan(model.StartTime.Value.Hours, model.StartTime.Value.Minutes, model.StartTime.Value.Seconds);
            this.FilePath = model.FilePath;
            this.StartDate = model.StartDate;
            this.IntervalMinutes = model.IntervalMinutes;
            this.PlaylistVM = new MediaPlaylistViewModel();
            //
            if (model.Playlist != null)
            {
                this.PlaylistVM = new MediaPlaylistViewModel(model.Playlist);
            }
        }

        public ScheduleModel GetModel()
        {
            ScheduleModel model = new ScheduleModel()
            {
                Days = this.Days,
                Enabled = this.Enabled,
                EveryDay = this.EveryDay,
                FilePath = this.FilePath,
                StartTime = this.StartTime==null?null:(TimeSpan?)new TimeSpan(this.StartTime.Value.Hours,this.StartTime.Value.Minutes,this.StartTime.Value.Seconds),
                StartDate = this.StartDate,
                IntervalMinutes = this.IntervalMinutes
            };
            //
            if (this.PlaylistVM != null)
            {
                model.Playlist = this.PlaylistVM.GetModel();
            }
            //
            return model;
        }

        public ScheduleViewModel CloneForScheduling()
        {
            ScheduleViewModel so = new ScheduleViewModel()
            {
                Days=this.Days,
                FilePath = this.FilePath,
                Enabled=this.Enabled,
                EveryDay=this.EveryDay,
                IntervalMinutes=this.IntervalMinutes,
                StartDate=this.StartDate,
                StartTime=this.StartTime,
            };
            return so;
        }
    }
}
