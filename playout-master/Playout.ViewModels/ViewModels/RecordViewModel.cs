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

namespace Playout.ViewModels.ViewModels
{
    public class RecordViewModel : Base.FormBaseViewModel
    {
        float _frameRate;
        int _sampleRate;
        string _fileName;
        string _fileFormat;
        int _segmentSecond;
        Size _frameSize;
        string _videoBitrate;
        string _audioBitrate;
        VCamChannelType _vcamChannel;
        EncoderProgram _encoderProgram;


        public float[] FrameRates { get { return Functions.Stream_GetFrameRates(); } }
        public int[] SampleRates { get { return Functions.Stream_GetSampleRates(); } }
        public string[] VideoBitrates { get { return Functions.Stream_GetVideoBitrates(); } }
        public string[] AudioBitrates { get { return Functions.Stream_GetAudioBitrates(); } }
        public string[] VCamChannels
        {
            get { return DirectShowGrabber.GetInstalledVCamChannels(); }
        }
        public string[] EncoderPrograms
        {
            get { return new string[] { EncoderProgram.FFMPEG.ToString(), EncoderProgram.FMLE.ToString() }; }
        }
        public string[] FrameSizes { get { return Functions.Stream_GetFrameSizes(); } }

        [Required]
        public float FrameRate
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
        
        public string VideoBitrate
        {
            get { return this._videoBitrate; }
            set
            {
                if (this._videoBitrate != value)
                {
                    this._videoBitrate = value;
                    this.NotifyOfPropertyChange(() => this.VideoBitrate);
                }
            }
        }
        public string AudioBitrate
        {
            get { return this._audioBitrate; }
            set
            {
                if (this._audioBitrate != value)
                {
                    this._audioBitrate = value;
                    this.NotifyOfPropertyChange(() => this.AudioBitrate);
                }
            }
        }

        [Required]
        public int SampleRate
        {
            get { return this._sampleRate; }
            set
            {
                if (this._sampleRate != value)
                {
                    this._sampleRate = value;
                    this.NotifyOfPropertyChange(() => this.SampleRate);
                }
            }
        }

        [Required]
        public virtual string FileName
        {
            get { return this._fileName; }
            set
            {
                if (this._fileName != value)
                {
                    this._fileName = value;
                    this.NotifyOfPropertyChange(() => this.FileName);
                }
            }
        }

        [Required]
        public string FileFormat
        {
            get { return this._fileFormat; }
            set
            {
                if (this._fileFormat != value)
                {
                    this._fileFormat = value;
                    this.NotifyOfPropertyChange(() => this.FileFormat);
                }
            }
        }

        [Required]
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

        [Required]
        public int SegmentSecond
        {
            get { return this._segmentSecond; }
            set
            {
                if (this._segmentSecond != value)
                {
                    this._segmentSecond = value;
                    this.NotifyOfPropertyChange(() => this.SegmentSecond);
                }
            }
        }

        public Size FrameSize
        {
            get { return this._frameSize; }
            set
            {
                if (this._frameSize != value)
                {
                    this._frameSize = value;
                    this.NotifyOfPropertyChange(() => this.FrameSize);
                }
            }
        }

        public string FrameSizeString
        {
            get { return this.FrameSize.Width.ToString() + "x" + this.FrameSize.Height.ToString(); }
            set
            {
                try
                {
                    int[] arr = value.Split(new char[] { 'x' }).Select(m => int.Parse(m)).ToArray();
                    this.FrameSize = new Size(arr[0], arr[1]);
                    this.NotifyOfPropertyChange(() => this.FrameSizeString);
                }
                catch
                {

                }
            }
        }

        public int FrameHeight
        {
            get { return this.FrameSize.Height; }
            set
            {
                if (this._frameSize.Height != value)
                {
                    this._frameSize.Height = value;
                    this.NotifyOfPropertyChange(() => this.FrameHeight);
                }
            }
        }

        [Required]
        public EncoderProgram EncoderProgram
        {
            get { return this._encoderProgram; }
            set
            {
                if (this._encoderProgram != value)
                {
                    this._encoderProgram = value;
                    this.NotifyOfPropertyChange(() => this.EncoderProgram);
                }
            }
        }

        public RecordViewModel()
        {
            this._frameSize=new Size();
        }

        public void Load(RecordModel model)
        {
            this.FrameRate = model.FrameRate;
            this.SampleRate = model.SampleRate;
            this.AudioBitrate = model.AudioBitrate;
            this.VideoBitrate = model.VideoBitrate;
            this.FileName = model.FileName;
            this.FileFormat = model.FileFormat;
            this.VCamChannel = model.VCamChannel;
            this.SegmentSecond = model.SegmentSecond;
            this.FrameSize = model.FrameSize;
            this.EncoderProgram = model.EncoderProgram;
        }
        public RecordModel GetModel()
        {
            RecordModel model = new RecordModel()
            {
                FileFormat = this.FileFormat,
                FileName = this.FileName,
                VideoBitrate = this.VideoBitrate,
                AudioBitrate = this.AudioBitrate,
                FrameRate = this.FrameRate,
                SampleRate = this.SampleRate,
                VCamChannel = this.VCamChannel,
                SegmentSecond = this.SegmentSecond,
                FrameSize = this.FrameSize
            };
            return model;
        }
        
        public RecordViewModel Clone()
        {
            RecordViewModel sett = new RecordViewModel()
            {
                FileFormat = this.FileFormat,
                FileName = this.FileName,
                FrameRate = this.FrameRate,
                SampleRate = this.SampleRate,
                VCamChannel = this.VCamChannel,
                SegmentSecond = this.SegmentSecond,
                FrameSize = this.FrameSize,
                VideoBitrate = this.VideoBitrate,
                AudioBitrate = this.AudioBitrate,
            };
            return sett;
        }
    }
}
