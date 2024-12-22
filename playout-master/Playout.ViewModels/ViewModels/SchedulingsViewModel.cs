using Playout.ApplicationService;
using Playout.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Models;
using Playout.Log;
using System.Timers;
using Playout.MediaFramework;
using Playout.DirectShow.MediaPlayers;

namespace Playout.ViewModels.ViewModels
{
    public class SchedulingsViewModel : Base.FormBaseViewModel
    {
        ObservableCollection<ScheduleViewModel> _schedules;
        IScheduleAppService schAppService;
        System.Timers.Timer tim2 = null;
        bool inTimer = false;

        public ScheduleViewModel CurrentSchedule { get; set; }
        public ObservableCollection<MediaSourceViewModel> AllSources
        {
            get
            {
                if (this.Schedules == null)
                    return null;
                //
                ObservableCollection<MediaSourceViewModel> items = new ObservableCollection<MediaSourceViewModel>();
                foreach (var pl in this.Schedules)
                {
                    foreach (var src in pl.PlaylistVM.MediaSourcesVM.Sources)
                        items.Add(src);
                }
                return items;
            }
        }
        public ObservableCollection<ScheduleViewModel> Schedules
        {
            get
            { return this._schedules; }
            set
            {
                if (this._schedules != value)
                {
                    this._schedules = value;
                    this.NotifyOfPropertyChange(() => this.Schedules);
                }
            }
        }

        public SchedulingsViewModel(IScheduleAppService _schAppService)
        {
            this.schAppService = _schAppService;
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
        public ScheduleViewModel NewSchedule()
        {
            ScheduleViewModel svm = new ScheduleViewModel();
            this.Schedules.Add(svm);
            //
            return svm;
        }

        public void SaveSchedule(ScheduleViewModel scvm)
        {
            ScheduleModel model = scvm.GetModel();
            this.schAppService.SaveFile(model);
        }

        public ScheduleViewModel OpenSchedule()
        {
            ScheduleModel model = this.schAppService.OpenFile();
            ScheduleViewModel vm = new ScheduleViewModel(model);
            //
            if (vm != null)
                this.Schedules.Add(vm);
            //
            return vm;
        }

        public ScheduleViewModel LoadFile(string filePath)
        {
            ScheduleModel model = this.schAppService.LoadFile(filePath);
            if (model == null)
                return null;
            //
            var sch = new ScheduleViewModel(model);
            //
            if (sch != null)
                this.Schedules.Add(sch);
            //
            return sch;
        }

        public void RemoveSchedule(ScheduleViewModel scvm)
        {
            this.Schedules.Remove(scvm);
        }

        public void ChangeOrderSchedule(ScheduleViewModel item, bool toUp)
        {
            if (item == null || this.Schedules.Count < 1)
                return;
            //
            if (toUp)
            {
                int index = this.Schedules.IndexOf(item);
                if (index > 0 && this.Schedules[index - 1] != null)
                {
                    this.Schedules.RemoveAt(index);
                    this.Schedules.Insert(index - 1, item);
                }
            }
            else
            {
                int index = this.Schedules.IndexOf(item);
                if (index < this.Schedules.Count - 1 && this.Schedules[index + 1] != null)
                {
                    this.Schedules.RemoveAt(index);
                    this.Schedules.Insert(index + 1, item);
                }
            }
        }

        public void LoadSchedules(List<ScheduleModel> scModels)
        {
            this.CurrentSchedule = null;
            //
            if (scModels == null)
                return;
            //
            this.Schedules = new ObservableCollection<ScheduleViewModel>(
                    scModels.Select(m => new ScheduleViewModel(m))
                );
        }

        public List<ScheduleModel> GetScheduleModels()
        {
            try
            {
                List<ScheduleModel> models = new List<ScheduleModel>(
                    this.Schedules.Select(vm => vm.GetModel()));
                //
                return models;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public void SchedulingKillThread()
        {
            this.schAppService.KillThread();
        }

        public void SchedulingStartThread(Action fn)
        {
            this.CurrentSchedule = null;
            //
            this.schAppService.SchedulingStartThread(() => {
                this.SchedulingThreadDriver(fn);
            });
        }

        private void SchedulingThreadDriver(Action fn)
        {
            long nowTimeTicks = DateTime.Now.TimeOfDay.Ticks;
            var sch = this.Schedules
                .Where(m => m.PlaylistVM != null && m.PlaylistVM.MediaSourcesVM != null && m.Enabled &&
                    m.PlaylistVM.MediaSourcesVM.Sources != null && m.PlaylistVM.MediaSourcesVM.Sources.Count > 0 &&
                    m.StartTime.HasValue && m.StartTime.Value.Ticks <= nowTimeTicks &&
                    //Begin Check Start Date
                        (
                            (m.StartDate.HasValue && DateTime.Now.Year == m.StartDate.Value.Year &&
                            DateTime.Now.Month == m.StartDate.Value.Month && DateTime.Now.Day == m.StartDate.Value.Day)
                        ||
                            (!m.StartDate.HasValue &&
                             (m.EveryDay || (m.Days & this.GetTodayOfWeek()) == this.GetTodayOfWeek()))
                        ) &&
                    //End Check Start Date
                    //Begin Check Time
                        (
                            (m.IntervalMinutes.HasValue &&
                             m.LastRanTicks + m.IntervalMinutes * TimeSpan.TicksPerMinute < nowTimeTicks
                            )
                        ||
                            (!m.IntervalMinutes.HasValue &&
                             m.StartTime.Value.Ticks < nowTimeTicks + 3 * TimeSpan.TicksPerSecond &&
                             m.StartTime.Value.Ticks > nowTimeTicks - 3 * TimeSpan.TicksPerSecond
                            )
                        )
                    //End Check Time
                    ).FirstOrDefault();
            if (sch != null)
            {
                this.CurrentSchedule = sch;
                this.CurrentSchedule.LastRanTicks=nowTimeTicks;
                fn();
            }
        }

        private Days2 GetTodayOfWeek()
        {
            return (Days2)Enum.Parse(typeof(Days2), DateTime.Now.DayOfWeek.ToString());
        }
    }
}
