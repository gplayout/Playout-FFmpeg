using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using Size = System.Windows.Size;
using Playout.DirectShow.Overlays;
using Playout.Log;
using System.Collections;
using System.Diagnostics;

namespace Playout.DirectShow.MediaPlayers
{
    public abstract class MediaPlayerBase
    {
        protected IBaseFilter sourceFilter { get; set; }
        protected IFileSourceFilter iFileSourceFilter { get; set; }
        protected IInputArgs inputArgs { get; set; }

        private enPlayStatus _lastPlaystatus;
        public Action<enPlayStatus> OnPlayStatusChanged;
        public enMediaType SourceMediaType { get; set; }
        public static bool OutputToVCam { get; set; }
        public static int VCamChannelIndex { get; set; }
        protected Overlays.SampleGrabberOverlays sgOverlays = null;
        public Overlays.SampleGrabberAudio sgAudio = null;
        public string showupEffect = null;

        public System.Windows.Threading.Dispatcher UIDispatcher { get; set; }

        public static string OutputVideoFormat { get; set; }
        public static int OutputVideoFormat_FrameRateScale { get; set; }
        public static int OutputVideoFormat_FrameRateDuration { get; set; }
        public static int OutputVideoFormat_Width { get; set; }
        public static int OutputVideoFormat_Height { get; set; }
        public static int OutputPreview_Width { get; set; }
        public static int OutputPreview_Height { get; set; }
        public static bool OutputPreview_IsPaused { get; set; }

        protected Overlays.PIPController pipController = null;
        public string PIP_FilePath { get; set; }
        public int PIP_PosX { get; set; }
        public int PIP_PosY { get; set; }
        public int PIP_Width { get; set; }
        public int PIP_Height { get; set; }

        private bool _isPlaying = false;
        public bool IsPlaying 
        {
            get { return this._isPlaying; }
            set
            {
                this._isPlaying = value;
            }
        }
        public SortedList<int, BaseOverlay> Overlays { get; set; }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// A static value to hold a count for all graphs.  Each graph
        /// has it's own value that it uses and is updated by the
        /// GraphInstanceCookie property in the get method
        /// </summary>
        private static int m_graphInstances;

        /// <summary>
        /// The custom windows message constant for graph events
        /// </summary>
        private const int WM_GRAPH_NOTIFY = 0x0400 + 13;

        /// <summary>
        /// One second in 100ns units
        /// </summary>
        protected const long DSHOW_ONE_SECOND_UNIT = 10000000;

        /// <summary>
        /// The IBasicAudio volume value for silence
        /// </summary>
        private const int DSHOW_VOLUME_SILENCE = -10000;

        /// <summary>
        /// The IBasicAudio volume value for full volume
        /// </summary>
        private const int DSHOW_VOLUME_MAX = 0;

        /// <summary>
        /// The IBasicAudio balance max absolute value
        /// </summary>
        private const int DSHOW_BALACE_MAX_ABS = 10000;

        /// <summary>
        /// Rate which our DispatcherTimer polls the graph
        /// </summary>
        private const int DSHOW_TIMER_POLL_MS = 33;

        /// <summary>
        /// UserId value for the VMR9 Allocator - Not entirely useful
        /// for this application of the VMR
        /// </summary>
        private readonly IntPtr m_userId = new IntPtr(unchecked((int)0xDEADBEEF));

        /// <summary>
        /// Static lock.  Seems multiple EVR controls instantiated at the same time crash
        /// </summary>
        private static readonly object m_videoRendererInitLock = new object();

        /// <summary>
        /// DirectShow interface for controlling audio
        /// functions such as volume and balance
        /// </summary>
        private IBasicAudio m_basicAudio;

        /// <summary>
        /// Flag for the Dispose pattern
        /// </summary>
        private bool m_disposed;

        /// <summary>
        /// The DirectShow filter graph reference
        /// </summary>
        private IFilterGraph m_graph;
        
        private IMediaControl m_mediaControl;

        
        private IMediaEventEx m_mediaEvent;

        protected bool m_hasVideo;
        protected bool m_hasAudio;

        /// <summary>
        /// The natural video pixel height, if applicable
        /// </summary>
        private int m_naturalVideoHeight;

        /// <summary>
        /// The natural video pixel width, if applicable
        /// </summary>
        private int m_naturalVideoWidth;

        /// <summary>
        /// Our Win32 timer to poll the DirectShow graph
        /// </summary>
        private System.Timers.Timer m_timer;
        private Stopwatch m_timerStopwatch = null;
        private long m_timerStopWatch_Ticks = 0;


        /// <summary>
        /// This objects last stand
        /// </summary>
        ~MediaPlayerBase()
        {
            Dispose();
        }

        /// <summary>
        /// The global instance Id of the graph.  We use this
        /// for the WndProc callback method.
        /// </summary>
        private int? m_graphInstanceId;

        /// <summary>
        /// The globally unqiue identifier of the graph
        /// </summary>
        protected int GraphInstanceId
        {
            get
            {
                if (m_graphInstanceId != null)
                    return m_graphInstanceId.Value;

                /* Increment our static value and store the current
                 * instance id of our player graph */
                m_graphInstanceId = Interlocked.Increment(ref m_graphInstances);

                return m_graphInstanceId.Value;
            }
        }
        
        /// <summary>
        /// Gets the natural pixel width of the current media.
        /// The value will be 0 if there is no video in the media.
        /// </summary>
        public virtual int NaturalVideoWidth
        {
            get
            {
                return m_naturalVideoWidth;
            }
            protected set
            {
                m_naturalVideoWidth = value;
            }
        }

        /// <summary>
        /// Gets the natural pixel height of the current media.  
        /// The value will be 0 if there is no video in the media.
        /// </summary>
        public virtual int NaturalVideoHeight
        {
            get
            {
                return m_naturalVideoHeight;
            }
            protected set
            {
                m_naturalVideoHeight = value;
            }
        }
        /// <summary>
        /// Gets or sets the balance on the audio.
        /// The value can range from -1 to 1. The value -1 means the right channel is attenuated by 100 dB 
        /// and is effectively silent. The value 1 means the left channel is silent. The neutral value is 0, 
        /// which means that both channels are at full volume. When one channel is attenuated, the other 
        /// remains at full volume.
        /// </summary>
        public virtual double Balance
        {
            get
            {
                /* Check if we even have an 
                 * audio interface */
                if (m_basicAudio == null)
                    return 0;

                int balance;

                /* Get the interface supplied balance value */
                m_basicAudio.get_Balance(out balance);

                /* Calc and return the balance based on 0 == silence */
                return (double)balance / DSHOW_BALACE_MAX_ABS;
            }
            set
            {
                /* Check if we even have an 
                 * audio interface */
                if (m_basicAudio == null)
                    return;

                /* Calc the dshow balance value */
                int balance = (int)value * DSHOW_BALACE_MAX_ABS;

                m_basicAudio.put_Balance(balance);
            }
        }

        public string OutputVideoDeviceName
        {
            get;
            set;
        }
        public string OutputAudioDeviceName
        {
            get;
            set;
        }
        
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            //if (m_disposed)
            //    return;

            if (!disposing)
                return;
            
            if (m_timer != null)
                m_timer.Dispose();
            //
            if (this.m_timerStopwatch != null)
            {
                this.m_timerStopwatch.Stop();
                m_timerStopWatch_Ticks = 0;
            }
            //
            m_timer = null;

            FreeResources();
            
            m_disposed = true;
            this.ChangedPlayStatus(enPlayStatus.Stopped);
        }

        
        private void ChangedPlayStatus(enPlayStatus status)
        {
            try
            {
                if (status == _lastPlaystatus)
                    return;
                this._lastPlaystatus = status;
                //
                if (this.UIDispatcher != null)
                {
                    this.UIDispatcher.Invoke(() =>
                    {
                        try
                        {
                            if (this.OnPlayStatusChanged != null)
                                this.OnPlayStatusChanged(status);
                        }
                        catch (Exception ex)
                        {
                            ex.Log();
                        }
                    });
                }
            }
            catch(Exception ex)
            {

            }
        }
        /// <summary>
        /// Polls the graph for various data about the media that is playing
        /// </summary>
        protected virtual void OnGraphTimerTick(long differTick)
        {
        }

        /// <summary>
        /// Starts the graph polling timer to update possibly needed
        /// things like the media position
        /// </summary>
        protected void StartGraphPollTimer()
        {
            if (m_timer == null)
            {
                m_timer = new System.Timers.Timer();
                m_timer.Interval = DSHOW_TIMER_POLL_MS;
                m_timer.Elapsed += TimerElapsed;
                //
                if (this.m_timerStopwatch == null)
                    this.m_timerStopwatch = new Stopwatch();
                m_timerStopWatch_Ticks = 0;
                this.m_timerStopwatch.Restart();
            }
            m_timer.Enabled = true;
        }

        private void ProcessGraphEvents()
        {
            try
            {
                //Dispatcher.BeginInvoke((Action)delegate
                //{
                if (m_mediaEvent != null)
                {
                    IntPtr param1;
                    IntPtr param2;
                    EventCode code;

                    /* Get all the queued events from the interface */
                    while (m_mediaEvent.GetEvent(out code, out param1, out param2, 0) == 0)
                    {
                        bool isCompleted = false;
                        //
                        /* Handle anything for this event code */
                        switch (code)
                        {
                            case EventCode.Complete:
                            case EventCode.Built:
                                this.UIDispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        InvokeMediaEnded(null);
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Log();
                                    }
                                });
                                isCompleted = true;
                                break;
                            case EventCode.Paused:
                                break;
                            default:
                                break;
                        }
                        /* Free everything..we only need the code */
                        m_mediaEvent.FreeEventParams(code, param1, param2);
                        //
                        if (isCompleted)
                            return;
                    }
                }
                //});
            }
            catch(Exception ex)
            {

            }
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_timer == null)
                return;
            //
            //UIDispatcher.Invoke((Action)delegate
            //{
            ProcessGraphEvents();
            //
            long differTicks = 0;
            if (this.m_timerStopwatch !=null && this.m_timerStopwatch.IsRunning && this.IsPlaying)
            {
                long ticks = this.m_timerStopwatch.Elapsed.Ticks;
                differTicks = ticks - this.m_timerStopWatch_Ticks;
                this.m_timerStopWatch_Ticks = ticks;
                OnGraphTimerTick(differTicks);
            }
            //});
        }

        protected void StopGraphPollTimer()
        {
            if (m_timer != null)
            {
                if (this.m_timerStopwatch != null)
                {
                    this.m_timerStopwatch.Stop();
                    this.m_timerStopWatch_Ticks = 0;
                    this.m_timerStopwatch = null;
                }
                m_timer.Stop();
                m_timer.Enabled = false;
                m_timer.Elapsed -= TimerElapsed;
                m_timer.Dispose();
                m_timer = null;
            }
        }

        private void SetMediaEventExInterface(IMediaEventEx mediaEventEx)
        {
            m_mediaEvent = mediaEventEx;
            //int hr = m_mediaEvent.SetNotifyWindow(HwndHelper.Handle, WM_GRAPH_NOTIFY, (IntPtr)GraphInstanceId);
        }

        /// <summary>
        /// Configures all general DirectShow interfaces that the
        /// FilterGraph supplies.
        /// </summary>
        /// <param name="graph">The FilterGraph to setup</param>
        protected virtual void SetupFilterGraph(IFilterGraph graph)
        {
            m_graph = graph;

            /* Setup the interfaces and query basic information
             * on the graph that is passed */
            SetBasicAudioInterface(m_graph as IBasicAudio);
            SetMediaControlInterface(m_graph as IMediaControl);
            SetMediaEventExInterface(m_graph as IMediaEventEx);
        }

        /// <summary>
        /// Sets the MediaControl interface
        /// </summary>
        private void SetMediaControlInterface(IMediaControl mediaControl)
        {
            m_mediaControl = mediaControl;
        }

        /// <summary>
        /// Sets the basic audio interface for controlling
        /// volume and balance
        /// </summary>
        protected void SetBasicAudioInterface(IBasicAudio basicAudio)
        {
            m_basicAudio = basicAudio;
        }

        /// <summary>
        /// Notifies when the media has successfully been opened
        /// </summary>
        public event Action MediaOpened;

        /// <summary>
        /// Notifies when the media has been closed
        /// </summary>
        public event Action MediaClosed;

        /// <summary>
        /// Notifies when the media has failed and produced an exception
        /// </summary>
        public event EventHandler<MediaFailedEventArgs> MediaFailed;

        /// <summary>
        /// Notifies when the media has completed
        /// </summary>
        public event Action MediaEnded;

        /// <summary>
        /// Resets the local graph resources to their
        /// default settings
        /// </summary>
        private void ResetLocalGraphResources()
        {
            m_graph = null;

            if (m_basicAudio != null)
                Marshal.ReleaseComObject(m_basicAudio);
            m_basicAudio = null;

            if (m_mediaControl != null)
                Marshal.ReleaseComObject(m_mediaControl);
            m_mediaControl = null;

            if (m_mediaEvent != null)
                Marshal.ReleaseComObject(m_mediaEvent);
            m_mediaEvent = null;
        }

        /// <summary>
        /// Frees any allocated or unmanaged resources
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void FreeResources()
        {
            try
            {
                if (inputArgs != null)
                {
                    Marshal.ReleaseComObject(inputArgs);
                }
                //
                if (iFileSourceFilter != null)
                    Marshal.ReleaseComObject(iFileSourceFilter);
                //
                if (sourceFilter != null)
                {
                    Marshal.ReleaseComObject(sourceFilter);
                }
                //
                StopGraphPollTimer();
                ResetLocalGraphResources();
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }

        /// <summary>
        /// Creates a new renderer and configures it with a custom allocator
        /// </summary>
        /// <param name="rendererType">The type of renderer we wish to choose</param>
        /// <param name="graph">The DirectShow graph to add the renderer to</param>
        /// <param name="streamCount">Number of input pins for the renderer</param>
        /// <returns>An initialized DirectShow renderer</returns>
        protected IBaseFilter CreateVideoRenderer(IGraphBuilder graph)
        {
            IBaseFilter renderer;
            renderer = AddFilterByName(graph, FilterCategory.LegacyAmFilterCategory, "Null Renderer");
            //switch (rendererType)
            //{
            //    case VideoRendererType.VideoMixingRenderer9:
            //        renderer = CreateVideoMixingRenderer9(graph, streamCount);
            //        break;
            //    case VideoRendererType.EnhancedVideoRenderer:
            //        renderer = CreateEnhancedVideoRenderer(graph, streamCount);
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException("rendererType");
            //}

            return renderer;
        }

        /// <summary>
        /// Plays the media
        /// </summary>
        public virtual void Play()
        {
            if (m_basicAudio != null)
            {
                //Balance = Balance;
                //Volume = Volume;
            }

            if (m_mediaControl != null)
                m_mediaControl.Run();
            //
            if (this.pipController != null)
                this.pipController.Resume();
            //
            if (this.m_timerStopwatch != null && !this.m_timerStopwatch.IsRunning)
                this.m_timerStopwatch.Start();
            //
            this.IsPlaying = true;
            //
            this.ChangedPlayStatus(enPlayStatus.Playing);
        }

        /// <summary>
        /// Stops the media
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                StopInternal();
                //
                FreeResources();
                this.IsPlaying = false;
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        
        public void StopInternal()
        {
            if (m_mediaControl != null)
            {
                this.ChangedPlayStatus(enPlayStatus.Stopped);
                //
                if (this.pipController != null)
                    this.pipController.Stop();
                //
                m_mediaControl.Stop();
                FilterState filterState;
                m_mediaControl.GetState(0, out filterState);

                while (filterState != FilterState.Stopped)
                    m_mediaControl.GetState(0, out filterState);
            }
        }

        
        public virtual void Close()
        {
            StopInternal();
            FreeResources();
            this.IsPlaying = false;
        }
        
        public virtual void Pause()
        {
            if (this.m_timerStopwatch != null && this.m_timerStopwatch.IsRunning)
                this.m_timerStopwatch.Stop();
            //
            if (m_mediaControl != null)
            {
                m_mediaControl.Pause();
                if (this.pipController != null)
                    this.pipController.Pause();
            }
            this.IsPlaying = false;
            this.ChangedPlayStatus(enPlayStatus.Paused);
        }

        #region Event Invokes
        
        protected void InvokeMediaEnded(EventArgs e)
        {
            var mediaEndedHandler = MediaEnded;
            if (mediaEndedHandler != null)
                mediaEndedHandler();
            //this.IsPlaying = false;
        }

        /// <summary>
        /// Invokes the MediaOpened event, notifying any subscriber that
        /// media has successfully been opened
        /// </summary>
        protected void InvokeMediaOpened()
        {
            /* This is generally a good place to start
             * our polling timer */
            StartGraphPollTimer();

            var mediaOpenedHandler = MediaOpened;
            if (mediaOpenedHandler != null)
                mediaOpenedHandler();
            this.IsPlaying = true;
        }

        /// <summary>
        /// Invokes the MediaClosed event, notifying any subscriber that
        /// the opened media has been closed
        /// </summary>
        protected void InvokeMediaClosed(EventArgs e)
        {
            var mediaClosedHandler = MediaClosed;
            if (mediaClosedHandler != null)
                mediaClosedHandler();
            //
            this.IsPlaying = false;
        }

        /// <summary>
        /// Invokes the MediaFailed event, notifying any subscriber that there was
        /// a media exception.
        /// </summary>
        /// <param name="e">The MediaFailedEventArgs contains the exception that caused this event to fire</param>
        protected void InvokeMediaFailed(MediaFailedEventArgs e)
        {
            var mediaFailedHandler = MediaFailed;
            if (mediaFailedHandler != null)
                mediaFailedHandler(this, e);
            //
            this.IsPlaying = false;
        }
        
        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Removes all filters from a DirectShow graph
        /// </summary>
        /// <param name="graphBuilder">The DirectShow graph to remove all the filters from</param>
        internal static void RemoveAllFilters(IGraphBuilder graphBuilder)
        {
            if (graphBuilder == null)
                return;

            IEnumFilters enumFilters;

            /* The list of filters from the DirectShow graph */
            var filtersArray = new List<IBaseFilter>();

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            /* Gets the filter enumerator from the graph */
            int hr = graphBuilder.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                /* This array is filled with reference to a filter */
                var filters = new IBaseFilter[1];
                IntPtr fetched = IntPtr.Zero;

                /* Get reference to all the filters */
                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    /* Add the filter to our array */
                    filtersArray.Add(filters[0]);
                }
            }
            finally
            {
                /* Enum filters is a COM, so release that */
                Marshal.ReleaseComObject(enumFilters);
            }

            /* Loop over and release each COM */
            for (int i = 0; i < filtersArray.Count; i++)
            {
                graphBuilder.RemoveFilter(filtersArray[i]);
                while (Marshal.ReleaseComObject(filtersArray[i]) > 0)
                { }
            }
        }

        /// <summary>
        /// Adds a filter to a DirectShow graph based on it's name and filter category
        /// </summary>
        /// <param name="graphBuilder">The graph builder to add the filter to</param>
        /// <param name="deviceCategory">The category the filter belongs to</param>
        /// <param name="friendlyName">The friendly name of the filter</param>
        /// <returns>Reference to the IBaseFilter that was added to the graph or returns null if unsuccessful</returns>
        internal static IBaseFilter AddFilterByName(IGraphBuilder graphBuilder, Guid deviceCategory, string friendlyName)
        {
            var devices = DsDevice.GetDevicesOfCat(deviceCategory);

            var deviceList = (from d in devices
                              where d.Name == friendlyName
                              select d);
            DsDevice device = null;
            if (deviceList.Count() > 0)
                device = deviceList.Take(1).Single();

            foreach (var item in deviceList)
            {
                if (item != device)
                    item.Dispose();
            }

            return AddFilterByDevice(graphBuilder, device);
        }

        internal static IBaseFilter AddFilterByDevicePath(IGraphBuilder graphBuilder, Guid deviceCategory, string devicePath)
        {
            var devices = DsDevice.GetDevicesOfCat(deviceCategory);

            var deviceList = (from d in devices
                              where d.DevicePath == devicePath
                              select d);
            DsDevice device = null;
            if (deviceList.Count() > 0)
                device = deviceList.Take(1).Single();

            return AddFilterByDevice(graphBuilder, device);
        }

        private static IBaseFilter AddFilterByDevice(IGraphBuilder graphBuilder, DsDevice device)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            var filterGraph = graphBuilder as IFilterGraph2;

            if (filterGraph == null)
                return null;

            IBaseFilter filter = null;
            if (device != null)
            {
                int hr = filterGraph.AddSourceFilterForMoniker(device.Mon, null, device.Name, out filter);
                DsError.ThrowExceptionForHR(hr);
            }
            return filter;
        }

        /// <summary>
        /// Finds a pin that exists in a graph.
        /// </summary>
        /// <param name="majorOrMinorMediaType">The GUID of the major or minor type of the media</param>
        /// <param name="pinDirection">The direction of the pin - in/out</param>
        /// <param name="graph">The graph to search in</param>
        /// <returns>Returns null if the pin was not found, or if a pin is found, returns the first instance of it</returns>
        internal static IPin FindPinInGraphByMediaType(Guid majorOrMinorMediaType, PinDirection pinDirection, IGraphBuilder graph)
        {
            IEnumFilters enumFilters;

            /* Get the filter enum */
            graph.EnumFilters(out enumFilters);

            /* Init our vars */
            var filters = new IBaseFilter[1];
            var fetched = IntPtr.Zero;
            IPin pin = null;
            IEnumMediaTypes mediaTypesEnum = null;

            /* Loop over each filter in the graph */
            while (enumFilters.Next(1, filters, fetched) == 0)
            {
                var filter = filters[0];

                int i = 0;

                /* Loop over each pin in the filter */
                while ((pin = DsFindPin.ByDirection(filter, pinDirection, i)) != null)
                {
                    /* Get the pin enumerator */
                    pin.EnumMediaTypes(out mediaTypesEnum);
                    var mediaTypesFetched = IntPtr.Zero;
                    var mediaTypes = new AMMediaType[1];

                    /* Enumerate the media types on the pin */
                    while (mediaTypesEnum.Next(1, mediaTypes, mediaTypesFetched) == 0)
                    {
                        /* See if the major or subtype meets our requirements */
                        if (mediaTypes[0].majorType.Equals(majorOrMinorMediaType) || mediaTypes[0].subType.Equals(majorOrMinorMediaType))
                        {
                            /* We found a match */
                            goto done;
                        }
                    }
                    i++;
                }
            }

        done:
            if (mediaTypesEnum != null)
            {
                mediaTypesEnum.Reset();
                Marshal.ReleaseComObject(mediaTypesEnum);
            }

            enumFilters.Reset();
            Marshal.ReleaseComObject(enumFilters);

            return pin;
        }

        internal static void RemoveFilters(IGraphBuilder graphBuilder)
        {
            RemoveFilters(graphBuilder, string.Empty);
        }
        internal static void RemoveFilters(IGraphBuilder graphBuilder, string filterName)
        {
            if (graphBuilder == null)
            {
                return;
            }

            int hr = 0;
            IEnumFilters enumFilters = null;
            ArrayList filtersArray = new ArrayList();

            try
            {
                hr = graphBuilder.EnumFilters(out enumFilters);
                DsError.ThrowExceptionForHR(hr);

                IBaseFilter[] filters = new IBaseFilter[1];
                IntPtr fetched = IntPtr.Zero;

                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    filtersArray.Add(filters[0]);
                }

                foreach (IBaseFilter filter in filtersArray)
                {
                    FilterInfo info;
                    filter.QueryFilterInfo(out info);
                    Marshal.ReleaseComObject(info.pGraph);

                    try
                    {
                        if (!String.IsNullOrEmpty(filterName))
                        {
                            if (String.Equals(info.achName, filterName))
                            {
                                DisconnectAllPins(graphBuilder, filter);
                                hr = graphBuilder.RemoveFilter(filter);
                                DsError.ThrowExceptionForHR(hr);
                                Marshal.ReleaseComObject(filter);
                            }
                        }
                        else
                        {
                            DisconnectAllPins(graphBuilder, filter);
                            hr = graphBuilder.RemoveFilter(filter);
                            DsError.ThrowExceptionForHR(hr);
                            int i = Marshal.ReleaseComObject(filter);
                        }
                    }
                    catch (Exception error)
                    {
                        error.Log();
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                if (enumFilters != null)
                {
                    Marshal.ReleaseComObject(enumFilters);
                }
            }
        }
        internal static bool DisconnectAllPins(IGraphBuilder graphBuilder, IBaseFilter filter)
        {
            IEnumPins pinEnum;
            int hr = filter.EnumPins(out pinEnum);
            if (hr != 0 || pinEnum == null)
            {
                return false;
            }
            FilterInfo info;
            filter.QueryFilterInfo(out info);

            Marshal.ReleaseComObject(info.pGraph);
            bool allDisconnected = true;
            for (;;)
            {
                IPin[] pins = new IPin[1];
                IntPtr fetched = IntPtr.Zero;
                hr = pinEnum.Next(1, pins, fetched);
                if (hr != 0 || fetched == IntPtr.Zero)
                {
                    break;
                }
                PinInfo pinInfo;
                pins[0].QueryPinInfo(out pinInfo);
                DsUtils.FreePinInfo(pinInfo);
                if (pinInfo.dir == PinDirection.Output)
                {
                    if (!DisconnectPin(graphBuilder, pins[0]))
                    {
                        allDisconnected = false;
                    }
                }
                Marshal.ReleaseComObject(pins[0]);
            }
            Marshal.ReleaseComObject(pinEnum);
            return allDisconnected;
        }

        internal static bool DisconnectPin(IGraphBuilder graphBuilder, IPin pin)
        {
            IPin other;
            int hr = pin.ConnectedTo(out other);
            bool allDisconnected = true;
            PinInfo info;
            pin.QueryPinInfo(out info);
            DsUtils.FreePinInfo(info);

            if (hr == 0 && other != null)
            {
                other.QueryPinInfo(out info);
                if (!DisconnectAllPins(graphBuilder, info.filter))
                {
                    allDisconnected = false;
                }
                hr = pin.Disconnect();
                if (hr != 0)
                {
                    allDisconnected = false;

                }
                hr = other.Disconnect();
                if (hr != 0)
                {
                    allDisconnected = false;

                }
                DsUtils.FreePinInfo(info);
                Marshal.ReleaseComObject(other);
            }
            else
            {

            }
            return allDisconnected;
        }

        internal static Bitmap ResizeBitmap(Image source, System.Drawing.Drawing2D.InterpolationMode quality
            ,int width, int height, 
            System.Drawing.Imaging.PixelFormat pf=System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        {
            if (source == null)
                return null;
            // Create the new bitmap.
            var bmp = new Bitmap(width, height, pf);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = quality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(source, new Rectangle(0, 0, width, height));
                g.Save();
            }

            return bmp;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        #endregion
    }
}
