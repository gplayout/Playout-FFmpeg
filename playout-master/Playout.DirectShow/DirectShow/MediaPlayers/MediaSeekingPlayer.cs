using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using Playout.Log;
using System.ComponentModel;
using System.Diagnostics;

namespace Playout.DirectShow.MediaPlayers
{
    /// <summary>
    /// The MediaSeekingPlayer adds media seeking functionality to
    /// to the MediaPlayerBase class
    /// </summary>
    public abstract class MediaSeekingPlayer : MediaPlayerBase, INotifyPropertyChanged
    {
        /// <summary>
        /// Local cache of the current position
        /// </summary>
        private long m_currentPosition;
        /// <summary>
        /// The DirectShow media seeking interface
        /// </summary>
        private IMediaSeeking m_mediaSeeking;

        private long _duration;
        /// <summary>
        /// Gets the duration in miliseconds, of the media that is opened
        /// </summary>
        public virtual long Duration 
        {
            get { return this._duration; }
            set
            {
                if(this._duration!=value)
                {
                    this._duration = value;
                    NotifyPropertyChanged("Duration");
                }
            }
        }

        private long _mediaPositionPlusOffset;
        public long MediaPositionPlusOffset
        {
            get
            {
                return this._mediaPositionPlusOffset;
            }
            set
            {
                if(this._mediaPositionPlusOffset!=value)
                {
                    this._mediaPositionPlusOffset = value;
                    NotifyPropertyChanged("MediaPositionPlusOffset");   
                }
            }
        }
        /// <summary>
        /// Gets or sets the position in miliseconds of the media
        /// </summary>
        public long MediaPosition
        {
            get
            {
                return m_currentPosition;
            }
            set
            {
                try
                {
                    //bool allowChange = System.Math.Abs(m_currentPosition - value) > 100000000;
                    
                    if (m_currentPosition != value)// && allowChange)
                    {
                        m_currentPosition = value;
                        this.MediaPositionPlusOffset = value + this.PlayerTimePositionOffset;
                    }
                }
                catch(Exception ex)
                {
                    ex.Log();
                }
            }
        }

        public long PlayerTimePositionOffset { get; set; }

        /// <summary>
        /// The current position format the media is using
        /// </summary>
        private MediaPositionFormat m_currentPositionFormat;

        /// <summary>
        /// The prefered position format to use with the media
        /// </summary>
        private MediaPositionFormat m_preferedPositionFormat;

        /// <summary>
        /// The current media positioning format
        /// </summary>
        public virtual MediaPositionFormat CurrentPositionFormat
        {
            get { return m_currentPositionFormat; }
            protected set { m_currentPositionFormat = value; }
        }

        /// <summary>
        /// The prefered media positioning format
        /// </summary>
        public virtual MediaPositionFormat PreferedPositionFormat
        {
            get { return m_preferedPositionFormat; }
            set
            {
                m_preferedPositionFormat = value;
                SetMediaSeekingInterface(m_mediaSeeking);
            }
        }
        
        protected override void SetupFilterGraph(IFilterGraph graph)
        {
            SetMediaSeekingInterface(graph as IMediaSeeking);
            base.SetupFilterGraph(graph);
        }
        
        protected override void FreeResources()
        {
            
            base.FreeResources();
            //
            MediaPosition = 0;
            //
            if (m_mediaSeeking != null)
                Marshal.ReleaseComObject(m_mediaSeeking);

            m_mediaSeeking = null;
        }
        
        protected override void OnGraphTimerTick(long differTicks)
        {
            //Logger.InfoLog("OnGraphTimerTick");
            if (m_mediaSeeking != null && this.IsPlaying)
            {
                long lCurrentPos;
                //Logger.InfoLog("OnGraphTimerTick-1");
                //int hr = m_mediaSeeking.GetCurrentPosition(out lCurrentPos);
                //Logger.InfoLog("OnGraphTimerTick-2");
                //if (hr == 0)
                //{
                lCurrentPos = this.MediaPosition + (long)(differTicks);
                if (lCurrentPos != m_currentPosition)
                {
                    MediaPosition = lCurrentPos;
                }
                //}
                //Logger.InfoLog("OnGraphTimerTick-3");
            }
           
            base.OnGraphTimerTick(differTicks);
        }
        
        protected static Guid ConvertPositionFormat(MediaPositionFormat positionFormat)
        {
            Guid timeFormat;

            switch (positionFormat)
            {
                case MediaPositionFormat.MediaTime:
                    timeFormat = TimeFormat.MediaTime;
                    break;
                case MediaPositionFormat.Frame:
                    timeFormat = TimeFormat.Frame;
                    break;
                case MediaPositionFormat.Byte:
                    timeFormat = TimeFormat.Byte;
                    break;
                case MediaPositionFormat.Field:
                    timeFormat = TimeFormat.Field;
                    break;
                case MediaPositionFormat.Sample:
                    timeFormat = TimeFormat.Sample;
                    break;
                default:
                    timeFormat = TimeFormat.None;
                    break;
            }

            return timeFormat;
        }
        
        protected static MediaPositionFormat ConvertPositionFormat(Guid positionFormat)
        {
            MediaPositionFormat format;

            if (positionFormat == TimeFormat.Byte)
                format = MediaPositionFormat.Byte;
            else if (positionFormat == TimeFormat.Field)
                format = MediaPositionFormat.Field;
            else if (positionFormat == TimeFormat.Frame)
                format = MediaPositionFormat.Frame;
            else if (positionFormat == TimeFormat.MediaTime)
                format = MediaPositionFormat.MediaTime;
            else if (positionFormat == TimeFormat.Sample)
                format = MediaPositionFormat.Sample;
            else
                format = MediaPositionFormat.None;

            return format;
        }

        public void SetMediaSeekingInterface(IMediaSeeking mediaSeeking)
        {
            m_mediaSeeking = mediaSeeking;

            if (mediaSeeking == null)
            {
                CurrentPositionFormat = MediaPositionFormat.None;
                Duration = 0;
                return;
            }

            /* Get our prefered DirectShow TimeFormat */
            Guid preferedFormat = ConvertPositionFormat(PreferedPositionFormat);

            /* Attempt to set the time format */
            mediaSeeking.SetTimeFormat(preferedFormat);

            Guid currentFormat;

            /* Gets the current time format
             * we may not have been successful
             * setting our prefered format */
            mediaSeeking.GetTimeFormat(out currentFormat);

            /* Set our property up with the right format */
            CurrentPositionFormat = ConvertPositionFormat(currentFormat);

            //SetDuration();
        }
        

        //INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }
}
