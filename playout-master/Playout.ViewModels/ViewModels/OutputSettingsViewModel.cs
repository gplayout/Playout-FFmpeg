using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Playout.Log;
using Playout.DirectShow.MediaPlayers;
using Playout.MediaFramework;

namespace Playout.ViewModels.ViewModels
{
    public class OutputSettingsViewModel : Base.FormBaseViewModel
    {
        Timer _tim = null;

        string _videoDeviceName;
        string _audioDeviceName;
        string _videoSize;
        string _defaultDir_MediaFiles;
        string _defaultDir_Playlists;
        bool _playoutLog;
        bool _runOnVM;
        bool _outputToVM;
        string _channelName;
        string _alternateTimeZone;
        string _ffmpegValue;
        VCamChannelType _vcamChannel;
        
        
        public string[] VideoDevices
        {
            get 
            {
                return Program.Dg.GetOutputVideoDeviceNames();
            }
        }
        public string[] AudioDevices
        {
            get
            {
                return Program.Dg.GetAudioDeviceNames();
            }
        }
        public string[] TimeZones
        {
            get
            {
                return TimeZoneInfo.GetSystemTimeZones().Select(m => m.Id).ToArray();
            }
        }
        [Required]
        public string VideoDeviceName
        {
            get { return this._videoDeviceName; }
            set
            {
                if (this._videoDeviceName != value)
                {
                    this._videoDeviceName = value;
                    this.NotifyOfPropertyChange(() => this.VideoDeviceName);
                }
            }
        }
        [Required]
        public string AudioDeviceName
        {
            get { return this._audioDeviceName; }
            set
            {
                if (this._audioDeviceName != value)
                {
                    this._audioDeviceName = value;
                    this.NotifyOfPropertyChange(() => this.AudioDeviceName);
                }
            }
        }
        [Required]
        public string VideoSize
        {
            get { return this._videoSize; }
            set
            {
                if (this._videoSize != value)
                {
                    this._videoSize = value;
                    this.NotifyOfPropertyChange(() => this.VideoSize);
                }
            }
        }

        public string DefaultDir_MediaFiles
        {
            get { return this._defaultDir_MediaFiles; }
            set
            {
                if (this._defaultDir_MediaFiles != value)
                {
                    this._defaultDir_MediaFiles = value;
                    this.NotifyOfPropertyChange(() => this.DefaultDir_MediaFiles);
                }
            }
        }
        public string DefaultDir_Playlists
        {
            get { return this._defaultDir_Playlists; }
            set
            {
                if (this._defaultDir_Playlists != value)
                {
                    this._defaultDir_Playlists = value;
                    this.NotifyOfPropertyChange(() => this.DefaultDir_Playlists);
                }
            }
        }
        public bool PlayoutLog
        {
            get { return this._playoutLog; }
            set
            {
                if(this._playoutLog!=value)
                {
                    this._playoutLog = value;
                    this.NotifyOfPropertyChange(() => this.PlayoutLog);
                }
            }
        }
        public bool OutputToVCam
        {
            get { return this._outputToVM; }
            set
            {
                if (this._outputToVM != value)
                {
                    this._outputToVM = value;
                    this.NotifyOfPropertyChange(() => this.OutputToVCam);
                }
            }
        }
        public VCamChannelType VCamChannel
        {
            get { return this._vcamChannel; }
            set
            {
                if (this._vcamChannel != value)
                {
                    this._vcamChannel = value;
                    this.NotifyOfPropertyChange(() => this.VCamChannel);
                }
            }
        }
        public string[] VCamChannels
        {
            get { return DirectShowGrabber.GetInstalledVCamChannels(); }
        }
        public string ChannelName
        {
            get { return this._channelName; }
            set
            {
                if (this._channelName != value)
                {
                    this._channelName = value;
                    this.NotifyOfPropertyChange(() => this.ChannelName);
                    this.NotifyOfPropertyChange(() => this.ChannelNameForPresent);
                }
            }
        }
        public string ChannelNameForPresent
        {
            get 
            {
                var cn = this.ChannelName;
                if (String.IsNullOrEmpty(cn))
                    cn = "[Unnamed]";
                return "Channel Name: " + cn;
            }
        }
        public string AlternateTimeZone
        {
            get 
            {
                if (String.IsNullOrEmpty(this._alternateTimeZone))
                    return TimeZone.CurrentTimeZone.StandardName;
                //
                return this._alternateTimeZone; 
            }
            set
            {
                if (this._alternateTimeZone != value)
                {
                    this._alternateTimeZone = value;
                    this.NotifyOfPropertyChange(() => this.AlternateTimeZone);
                }
            }
        }

        public string AlternateDateTime
        {
            get
            {
                try
                {
                    if (String.IsNullOrEmpty(this.AlternateTimeZone))
                        return null;
                    //
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById(this.AlternateTimeZone))
                        .ToString("MM/dd/yyyy H\\:mm\\:ss");
                }
                catch (Exception ex)
                {
                    ex.Log();
                    return null;
                }
            }
        }
        public OutputSettingsViewModel()
        {
            this._tim = new Timer(1000);
            this._tim.Elapsed += tim_Elapsed;
            this._tim.Start();
        }

        public void Load(OutputSettingModel model)
        {
            this.VideoDeviceName = model.VideoDeviceName;
            this.AudioDeviceName = model.AudioDeviceName;
            this.DefaultDir_MediaFiles = model.DefaultDir_MediaFiles;
            this.DefaultDir_Playlists = model.DefaultDir_Playlists;
            this.VideoSize = model.VideoSize;
            this.PlayoutLog = model.PlayoutLog;
            this.OutputToVCam = model.OutputToVCam;
            this.VCamChannel = model.VCamChannel;
            this.AlternateTimeZone = model.AlternateTimeZone;
            this.ChannelName = model.ChannelName;
            //
            this.OperateSettings();
        }
        public OutputSettingModel GetModel()
        {
            OutputSettingModel model = new OutputSettingModel()
            {
                AudioDeviceName = this.AudioDeviceName,
                VideoDeviceName = this.VideoDeviceName,
                VideoSize = this.VideoSize,
                DefaultDir_MediaFiles = this.DefaultDir_MediaFiles,
                DefaultDir_Playlists = this.DefaultDir_Playlists,
                PlayoutLog = this.PlayoutLog,
                OutputToVCam = this.OutputToVCam,
                VCamChannel = this.VCamChannel,
                ChannelName = this.ChannelName,
                AlternateTimeZone = this.AlternateTimeZone
            };
            return model;
        }

        public void OperateSettings()
        {
            Program.Dg.OutputVideoDeviceName = this.VideoDeviceName;
            Program.Dg.OutputAudioDeviceName = this.AudioDeviceName;
            Program.Dg.OutputVideoFormat = this.VideoSize;
            Program.DefaultDir_MediaFiles = this.DefaultDir_MediaFiles;
            Program.DefaultDir_Playlists = this.DefaultDir_Playlists;
            Program.Dg.CheckOutputFlag = true;
            Program.PlayoutLog = this.PlayoutLog;
            Playout.DirectShow.MediaPlayers.MediaPlayerBase.OutputToVCam = this.OutputToVCam;
            Playout.DirectShow.MediaPlayers.MediaPlayerBase.VCamChannelIndex = this.VCamChannel.GetHashCode();
        }

        public OutputSettingsViewModel Clone()
        {
            OutputSettingsViewModel sett = new OutputSettingsViewModel()
            {
                AudioDeviceName = this.AudioDeviceName,
                DefaultDir_MediaFiles = this.DefaultDir_MediaFiles,
                DefaultDir_Playlists = this.DefaultDir_Playlists,
                PlayoutLog = this.PlayoutLog,
                OutputToVCam = this.OutputToVCam,
                VCamChannel = this.VCamChannel,
                VideoDeviceName = this.VideoDeviceName,
                VideoSize = this.VideoSize,
                AlternateTimeZone = this.AlternateTimeZone,
                ChannelName = this.ChannelName,
            };
            return sett;
        }

        void tim_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.NotifyOfPropertyChange(() => this.AlternateDateTime);
        }
    }
}
