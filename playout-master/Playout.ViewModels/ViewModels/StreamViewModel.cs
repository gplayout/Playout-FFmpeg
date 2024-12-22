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
using Size=System.Drawing.Size;
using System.Collections.ObjectModel;

namespace Playout.ViewModels.ViewModels
{
    public class StreamViewModel:Base.BaseViewModel
    {
        ObservableCollection<StreamInstanceViewModel> _profiles;
        string _selectedProfileName;
        public ObservableCollection<StreamInstanceViewModel> Profiles
        {
            get
            { return this._profiles; }
            set
            {
                if (this._profiles != value)
                {
                    this._profiles = value;
                    this.NotifyOfPropertyChange(() => this.Profiles);
                }
            }
        }

        public string SelectedProfileName
        {
            get { return this._selectedProfileName; }
            set
            {
                if (this._selectedProfileName != value)
                {
                    this._selectedProfileName = value;
                    this.NotifyOfPropertyChange(() => this.SelectedProfileName);
                    this.NotifyOfPropertyChange(() => this.SelectedProfile);
                }
            }
        }

        public StreamInstanceViewModel SelectedProfile
        {
            get
            {
                if (String.IsNullOrEmpty(this.SelectedProfileName))
                    return null;
                else
                {
                    var item = this.Profiles.Where(m => m.ProfileName == this.SelectedProfileName).SingleOrDefault();
                    return item;
                }
            }
        }
        public StreamViewModel()
        {
            this.Profiles = new ObservableCollection<StreamInstanceViewModel>();
        }

        public string GetNewProfileName()
        {
            string name = "Profile" + (this.Profiles.Count + 1);
            while(this.Profiles.Where(m=>m.ProfileName==name).Count()>0)
            {
                name += "1";
            }
            return name;
        }
        public void Load(StreamModel model)
        {
            this.Profiles = new ObservableCollection<StreamInstanceViewModel>(
                model.Profiles.Select(m => new StreamInstanceViewModel(m)).ToList());
            this.SelectedProfileName = model.SelectedProfileName;
        }
        public StreamModel GetModel()
        {
            StreamModel model = new StreamModel()
                {
                    SelectedProfileName = this.SelectedProfileName,
                    Profiles = this.Profiles.Select(m => m.MyGetModel()).ToList()
                };
            //
            return model;
        }

        public StreamViewModel Clone()
        {
            StreamViewModel vm = new StreamViewModel()
                {
                    SelectedProfileName = this.SelectedProfileName,
                    Profiles = new ObservableCollection<StreamInstanceViewModel>(
                        this.Profiles.Select(m => m.MyClone()).ToList())
                };
            //
            return vm;
        }
    }
    public class StreamInstanceViewModel : RecordViewModel
    {
        string _url;
        string _profileName;

        [Required]
        public string Url
        {
            get { return this._url; }
            set
            {
                if (this._url != value)
                {
                    this._url = value;
                    this.NotifyOfPropertyChange(() => this.Url);
                }
            }
        }

        [Required]
        public string ProfileName
        {
            get { return this._profileName; }
            set
            {
                if (this._profileName != value)
                {
                    this._profileName = value;
                    this.NotifyOfPropertyChange(() => this.ProfileName);
                }
            }
        }

        public bool DoRecord
        {
            get { return !String.IsNullOrEmpty(this.FileName); }
            set
            {
                if (value)
                {
                    if (String.IsNullOrEmpty(this.FileName))
                        this.FileName = "output";
                }
                else
                    this.FileName = "";
                this.NotifyOfPropertyChange(() => this.DoRecord);
            }
        }
        public StreamInstanceViewModel()
            : base()
        {
        }

        public StreamInstanceViewModel(StreamInstanceModel model)
        {
            base.Load(model);
            this.Url = model.Url;
            this.ProfileName = model.ProfileName;
        }

        public StreamInstanceViewModel(string profileName)
            : base()
        {
            this.ProfileName = profileName;
            this.Url = "";
        }

        public StreamInstanceModel MyGetModel()
        {
            StreamInstanceModel model = new StreamInstanceModel()
            {
                FileFormat = this.FileFormat,
                FileName = this.FileName,
                FrameRate = this.FrameRate,
                SampleRate = this.SampleRate,
                VideoBitrate = this.VideoBitrate,
                AudioBitrate = this.AudioBitrate,
                VCamChannel = this.VCamChannel,
                SegmentSecond = this.SegmentSecond,
                FrameSize = this.FrameSize,
                Url = this.Url,
                ProfileName = this.ProfileName,
                EncoderProgram = this.EncoderProgram,
            };
            //
            return model;
        }
        
        public StreamInstanceViewModel MyClone()
        {
            StreamInstanceViewModel vm = new StreamInstanceViewModel()
            {
                FileFormat = this.FileFormat,
                FileName = this.FileName,
                FrameRate = this.FrameRate,
                SampleRate = this.SampleRate,
                VCamChannel = this.VCamChannel,
                SegmentSecond = this.SegmentSecond,
                FrameSize = this.FrameSize,
                ProfileName = this.ProfileName,
                Url = this.Url,
                VideoBitrate = this.VideoBitrate,
                AudioBitrate = this.AudioBitrate,
                EncoderProgram=this.EncoderProgram
            };
            //
            return vm;
        }
    }
}
