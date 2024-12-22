using Playout.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Playout.ViewModels.ViewModels
{
    public class TimerSettingsViewModel : Base.FormBaseViewModel
    {
        public Action OnTimeReceived { get; set; }
        System.Windows.Forms.Timer tim = null;
        //
        TimersMode _timerMode;
        TimeSpan _playTime;
        TimeSpan? _time;

        public TimersMode TimerMode
        {
            get { return this._timerMode; }
            set
            {
                if (this._timerMode != value)
                {
                    this._timerMode = value;
                    this.NotifyOfPropertyChange(() => this.TimerMode);
                }
            }
        }
        public TimeSpan? Time
        {
            get { return this._time; }
            set
            {
                this._time = value;
                if (this._time == null)
                    this.PlayTime = TimeSpan.FromSeconds(0);
                else
                {
                    switch (this.TimerMode)
                    {
                        case TimersMode.CountDown:
                            this.PlayTime = DateTime.Now.TimeOfDay.Add(this._time.Value);
                            break;
                        case TimersMode.Specific:
                            this.PlayTime = this._time.Value;
                            break;
                        default:
                            break;
                    }
                }
                //
                this.NotifyOfPropertyChange(() => this.Time);
                this.NotifyOfPropertyChange(() => this.PlayTime);
                this.NotifyOfPropertyChange(() => this.CountdownTime);
            }
        }
        public TimeSpan PlayTime
        {
            get 
            {
                return this._playTime;
            }
            private set
            {
                if (this._playTime != value)
                {
                    this._playTime = value;
                    this.NotifyOfPropertyChange(() => this.PlayTime);
                    this.NotifyOfPropertyChange(() => this.CountdownTime);
                }
            }
        }
        public TimeSpan CountdownTime
        {
            get
            {
                if (this.PlayTime == null)
                    return TimeSpan.FromSeconds(0);
                //
                var secs = this.PlayTime.TotalSeconds - DateTime.Now.TimeOfDay.TotalSeconds;
                if (secs <= 0)
                    return TimeSpan.FromSeconds(0);
                else
                    return TimeSpan.FromSeconds(secs);
            }
        }

        public TimerSettingsViewModel()
        {
            this.tim = new System.Windows.Forms.Timer();
            this.tim.Interval = 1000;
            this.tim.Tick+=tim_Tick;
        }

        
        public void Operate()
        {
            if (this.TimerMode != TimersMode.None)
            {
                this.tim.Start();
            }
            else
            {
                this.tim.Stop();
            }
        }

        public TimerSettingsViewModel Clone()
        {
            TimerSettingsViewModel sett = new TimerSettingsViewModel()
            {
                TimerMode = this.TimerMode,
                PlayTime = this.PlayTime,
                Time = this.Time
            };
            return sett;
        }
        void tim_Tick(object sender, EventArgs e)
        {
            try
            {
                this.NotifyOfPropertyChange(() => this.CountdownTime);
                //
                if (this.PlayTime <= DateTime.Now.TimeOfDay)
                {
                    this.tim.Stop();
                    if (this.OnTimeReceived != null)
                        this.OnTimeReceived();
                    //
                    this.TimerMode = TimersMode.None;
                }
            }
            catch { }
        }
    }
}
