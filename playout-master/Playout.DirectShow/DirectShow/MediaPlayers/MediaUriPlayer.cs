#region Usings
using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.Collections.Generic;
using forms = System.Windows.Forms;
using Playout.DirectShow.Overlays;
#endregion
using Playout.Log;

namespace Playout.DirectShow.MediaPlayers
{
    public class MediaUriPlayer : MediaSeekingPlayer
    {
        DsROTEntry m_dsRotEntry = null;
        const string NoDevice = "No Device";
        private const string DEFAULT_AUDIO_RENDERER_NAME = "Default DirectSound Device";

        private string m_audioRenderer = DEFAULT_AUDIO_RENDERER_NAME;
        private int _videoFrameRate = (int)(DSHOW_ONE_SECOND_UNIT / 30);
        private int _videoFile_width = 0;
        private int _videoFile_height = 0;
        private int _audioSampleRate = 48000;

        public long SeekMiliSecond { get; set; }
        /// <summary>
        /// The DirectShow graph interface.  In this example
        /// We keep reference to this so we can dispose 
        /// of it later.
        /// </summary>
        private IGraphBuilder m_graph;

        /// <summary>
        /// The media Uri
        /// </summary>
        private string m_sourceUri;

        public Action OnSourceChanged;
        /// <summary>
        /// Gets or sets the Uri source of the media
        /// </summary>
        public string Source
        {
            get
            {
                return m_sourceUri;
            }
            set
            {
                if (value != m_sourceUri)
                {
                    if (this.OnSourceChanged != null)
                        this.OnSourceChanged();
                }

                m_sourceUri = value;

                OpenSource();
            }
        }
        
        /// <summary>
        /// The name of the audio renderer device
        /// </summary>
        public string AudioRenderer
        {
            get
            {
                return m_audioRenderer;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = DEFAULT_AUDIO_RENDERER_NAME;
                }

                m_audioRenderer = value;
            }
        }

        private void ConfigureSampleGrabber(ISampleGrabber sampGrabber)
        {
            int hr;
            AMMediaType media = new AMMediaType();

            // Set the media type to Video/RBG24
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB32;
            media.formatType = FormatType.VideoInfo;
            hr = sampGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;
            this.sgOverlays = new Overlays.SampleGrabberOverlays(MediaPlayerBase.OutputToVCam, MediaPlayerBase.VCamChannelIndex);
            this.sgOverlays.Overlays = this.Overlays;
            // Configure the samplegrabber callback
            hr = sampGrabber.SetCallback(this.sgOverlays, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        private void PIP_OnHasPicure(System.Drawing.Bitmap bitmap)
        {
            if (this.sgOverlays.PIPOverlay == null)
            {
                this.sgOverlays.PIPOverlay = bitmap;
            }
        }
        private void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            try
            {
                int hr;
                // Get the media type from the SampleGrabber
                AMMediaType media = new AMMediaType();
                hr = sampGrabber.GetConnectedMediaType(media);

                if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
                {
                    throw new NotSupportedException("Unknown Grabber Media Format");
                }

                // Grab the size info
                VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
                this.sgOverlays.m_videoWidth = videoInfoHeader.BmiHeader.Width;
                this.sgOverlays.m_videoHeight = videoInfoHeader.BmiHeader.Height;
                this.sgOverlays.m_videofile_width = this._videoFile_width == 0 ? videoInfoHeader.BmiHeader.Width : this._videoFile_width;
                this.sgOverlays.m_videofile_height = this._videoFile_height == 0 ? videoInfoHeader.BmiHeader.Height : this._videoFile_height;
                this.sgOverlays.m_stride = this.sgOverlays.m_videoWidth * (videoInfoHeader.BmiHeader.BitCount / 8);
                this.sgOverlays.m_VideoFrameRate = _videoFrameRate;
                this.sgOverlays.m_ShowupEffect = this.showupEffect;
                DsUtils.FreeAMMediaType(media);
                media = null;
            }
            catch (Exception ex)
            {

            }
        }

        public virtual void OpenSource()
        {
            if (this.m_graph != null)
            {
                this.MediaPosition = 0;
                //bool shouldStop = false;
                //XMediaHelper.MediaHelper mh = new XMediaHelper.MediaHelper();
                //shouldStop = mh.ShouldBeStop(m_sourceUri, m_hasAudio, m_hasVideo);
                //if (!shouldStop)
                //{
                    //this.inputArgs.SetNextFile(m_sourceUri, this.SeekMiliSecond);
                    ////
                    //return;
                //}
            }
            //
            /* Make sure we clean up any remaining mess */
            FreeResources();

            if (m_sourceUri == null)
                return;

            string fileSource = m_sourceUri;

            if (string.IsNullOrEmpty(fileSource))
                return;

            try
            {
                /* Creates the GraphBuilder COM object */
                m_graph = new FilterGraphNoThread() as IGraphBuilder;

                if (m_graph == null)
                    throw new Exception("Could not create a graph");

                //#if DEBUG
                /* Adds the GB to the ROT so we can view
                * it in graphedit */
                m_dsRotEntry = new DsROTEntry(m_graph);
                //#endif

                var filterGraph = m_graph as IFilterGraph2;

                if (filterGraph == null)
                    throw new Exception("Could not QueryInterface for the IFilterGraph2");
                //Add Source
                IPin sourceOutPin = null;
                this.AddSourceToGraph(ref filterGraph, fileSource, out sourceOutPin);
                if (sourceFilter == null)
                    return;
                if (inputArgs != null)
                {
                    inputArgs.GetMediaInformation(out m_hasAudio, out m_hasVideo);
                }
                
                //Add Overlay
                IPin ovOutPin = null;
                ISampleGrabber samGrabber = null;
                this.AddOverlayToGraph(ref filterGraph, sourceOutPin, out ovOutPin, out samGrabber);
                //Add PIP
                if (!String.IsNullOrEmpty(this.PIP_FilePath))
                {
                    this.AddPIPToGraph();
                }
                //Add Output
                this.AddOutputToGraph(ref filterGraph, ovOutPin);
                //
                //Get Media Parameters
                if (inputArgs != null)
                {
                    inputArgs.GetFileSizeInfo(out this._videoFile_width, out this._videoFile_height);
                    inputArgs.GetVideoFrameRate(out this._videoFrameRate);
                }
                //
                SaveSizeInfo(samGrabber);
                //
                Marshal.ReleaseComObject(ovOutPin);
                Marshal.ReleaseComObject(samGrabber);
                //
                //Add Audio Renderer
                InsertAudioRenderer(ref filterGraph, fileSource, AudioRenderer);
                //
                if (inputArgs != null)
                {
                    inputArgs.GetAudioSampleRate(out this._audioSampleRate);
                }
                if (this.sgAudio != null)
                {
                    this.sgAudio.m_AudioSampleRate = _audioSampleRate;
                }
                //
                ////
                Marshal.ReleaseComObject(sourceOutPin);
                //

                /* Configure the graph in the base class */
                SetupFilterGraph(m_graph);
                //
                //this.CreateGrapthForImage(fileSource, ref filterGraph, ref renderer);
                /* Sets the NaturalVideoWidth/Height */
                //SetNativePixelSizes(renderer);
            }
            catch (Exception ex)
            {
                FreeResources();

                /* Fire our failed event */
                InvokeMediaFailed(new MediaFailedEventArgs(ex.Message, ex));
                ex.Log();

            }

            InvokeMediaOpened();
        }
        private void AddOutputToGraph(ref IFilterGraph2 filterGraph, IPin sourcePin)
        {
            IBaseFilter renderer = CreateVideoRenderer(m_graph);
            //
            if (!String.IsNullOrEmpty(this.OutputVideoDeviceName) && this.OutputVideoDeviceName != MediaUriPlayer.NoDevice)
            {
                #region Device Output
                IBaseFilter infTee = (IBaseFilter)new InfTee();
                filterGraph.AddFilter(infTee, "Inf Tee");
                IPin teePinIn = DsFindPin.ByDirection(infTee, PinDirection.Input, 0);
                //
                filterGraph.Connect(sourcePin, teePinIn);
                //
                IPin teePinOut1 = DsFindPin.ByDirection(infTee, PinDirection.Output, 0);
                filterGraph.Render(teePinOut1);
                IPin teePinOut2 = DsFindPin.ByDirection(infTee, PinDirection.Output, 1);
                //
                //Add Device Filter
                IBaseFilter outFilter = AddFilterByName(m_graph, FilterCategory.TransmitCategory, this.OutputVideoDeviceName);
                //Connect Tee to Decklink
                IPin outPinInVideo = DsFindPin.ByDirection((IBaseFilter)outFilter, PinDirection.Input, 0);
                filterGraph.Connect(teePinOut2, outPinInVideo);
                ///////////////////////////////////////////////////////////////////////////////////////////////
                Marshal.ReleaseComObject(outFilter);
                Marshal.ReleaseComObject(outPinInVideo);
                Marshal.ReleaseComObject(teePinIn);
                Marshal.ReleaseComObject(teePinOut1);
                Marshal.ReleaseComObject(teePinOut2);
                Marshal.ReleaseComObject(infTee);
                #endregion Device Output
            }
            else
            {
                filterGraph.Render(sourcePin);
            }
            //
            Marshal.ReleaseComObject(renderer);
            //
        }
        private void AddSourceToGraph(ref IFilterGraph2 filterGraph, string file, out IPin outputPin)
        {
            outputPin = null;
            //
            sourceFilter = AddFilterByName(filterGraph, FilterCategory.LegacyAmFilterCategory, "Hamed Filter");
            inputArgs = (sourceFilter as IInputArgs);
            iFileSourceFilter = (sourceFilter as IFileSourceFilter);
            //
            inputArgs.SetKey(231212);
            if (!String.IsNullOrEmpty(MediaPlayerBase.OutputVideoFormat) && MediaPlayerBase.OutputVideoFormat != "Auto")
            {
                int output = 0;
                inputArgs.SetSize(MediaPlayerBase.OutputVideoFormat_Width,
                    MediaPlayerBase.OutputVideoFormat_Height,
                    MediaPlayerBase.OutputVideoFormat_FrameRateScale,
                    MediaPlayerBase.OutputVideoFormat_FrameRateDuration, out output);
            }
            if (this.SeekMiliSecond > 0)
            {
                inputArgs.SetSeek(this.SeekMiliSecond);
            }
            //
            iFileSourceFilter.Load(file, null);
            //
            outputPin = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 0);
        }
        private void AddOverlayToGraph(ref IFilterGraph2 filterGraph, IPin sourcePin, out IPin outputPin, out ISampleGrabber samGrabber)
        {
            samGrabber = (ISampleGrabber)new SampleGrabber();
            ConfigureSampleGrabber(samGrabber);
            filterGraph.AddFilter((IBaseFilter)samGrabber, "Sample Grabber");
            //Connect Overlay
            IPin ovPinInput = DsFindPin.ByDirection((IBaseFilter)samGrabber, PinDirection.Input, 0);
            filterGraph.Connect(sourcePin, ovPinInput);
            //
            outputPin = DsFindPin.ByDirection((IBaseFilter)samGrabber, PinDirection.Output, 0);
            //
            Marshal.ReleaseComObject(ovPinInput);
        }
        private void AddPIPToGraph()
        {
            if (this.PIP_PosX < 0)
                this.PIP_PosX = 0;
            this.sgOverlays.PIP_PosLeft = this.PIP_PosX;
            if (this.PIP_PosY < 0)
                this.PIP_PosY = 0;
            this.sgOverlays.PIP_PosTop = this.PIP_PosY;
            //
            this.pipController = new PIPController();
            this.pipController.Start(this.PIP_FilePath, this.PIP_Width, this.PIP_Height, this.PIP_OnHasPicure);
        }
        private void ConfigureVideoWindow(IVideoWindow videoWindow, System.Windows.Forms.Control hWin)
        {
            int hr;

            // Set the output window
            hr = videoWindow.put_Owner(hWin.Handle);
            DsError.ThrowExceptionForHR(hr);

            // Set the window style
            hr = videoWindow.put_WindowStyle((WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings));
            DsError.ThrowExceptionForHR(hr);

            // Make the window visible
            hr = videoWindow.put_Visible(OABool.True);
            DsError.ThrowExceptionForHR(hr);

            // Position the playing location
            System.Drawing.Rectangle rc = hWin.ClientRectangle;
            hr = videoWindow.SetWindowPosition(0, 0, rc.Right, rc.Bottom);
            DsError.ThrowExceptionForHR(hr);
        }
        protected virtual void InsertAudioRenderer(ref IFilterGraph2 filterGraph,string fileOrDevName, string audioRenderer)
        {
            IPin audioSourcePinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 1);
            //
            if (audioSourcePinOut == null)
                return;
            //Add Sample Grabber
            var audioGrabber = SampleGrabberAudio.GetAudioGrabber(ref filterGraph, out this.sgAudio,
                MediaPlayerBase.OutputToVCam, MediaPlayerBase.VCamChannelIndex);
            //Connect Decoder To Grabber
            IPin audioGrabberPinIn = DsFindPin.ByDirection(audioGrabber, PinDirection.Input, 0);
            filterGraph.Connect(audioSourcePinOut, audioGrabberPinIn);

            //Add Audio Device

            IBaseFilter faDev = null;
            if (!String.IsNullOrEmpty(this.OutputAudioDeviceName) && this.OutputAudioDeviceName != MediaUriPlayer.NoDevice)
                faDev = AddFilterByName(m_graph, FilterCategory.AudioRendererCategory, audioRenderer);
            else
                faDev = AddFilterByName(m_graph, FilterCategory.LegacyAmFilterCategory, "Null Renderer");
            //Connect Grabber to Audio Device
            IPin audioGrabberPinOut = DsFindPin.ByDirection(audioGrabber, PinDirection.Output, 0);
            IPin audioDevPinIn = DsFindPin.ByDirection(faDev, PinDirection.Input, 0);
            filterGraph.Connect(audioGrabberPinOut, audioDevPinIn);
            //
            Marshal.ReleaseComObject(faDev);
            Marshal.ReleaseComObject(audioGrabberPinOut);
            Marshal.ReleaseComObject(audioDevPinIn);

            //
            SampleGrabberAudio.SetMediaSampleGrabber(ref audioGrabber);
            //
            Marshal.ReleaseComObject(audioSourcePinOut);
            Marshal.ReleaseComObject(audioGrabber);
            Marshal.ReleaseComObject(audioGrabberPinIn);
        }

        /// <summary>
        /// Frees all unmanaged memory and resets the object back
        /// to its initial state
        /// </summary>
        protected override void FreeResources()
        {
            try
            {
                //#if DEBUG
                /* Remove us from the ROT */
                if (m_dsRotEntry != null)
                {
                    m_dsRotEntry.Dispose();
                    m_dsRotEntry = null;
                }
                //#endif

                /* We run the StopInternal() to avoid any 
                 * Dispatcher VeryifyAccess() issues because
                 * this may be called from the GC */
                StopInternal();
                this.IsPlaying = false;
                if (m_graph != null)
                {
                    RemoveFilters(m_graph);
                    Marshal.ReleaseComObject(m_graph);
                    m_graph = null;
                    //
                    if (this.sgOverlays != null)
                    {
                        this.sgOverlays.Dispose();
                        this.sgOverlays = null;
                    }
                    if (this.sgAudio != null)
                    {
                        this.sgAudio.Dispose();
                        this.sgAudio = null;
                    }
                    /* Only run the media closed if we have an
                     * initialized filter graph */
                    InvokeMediaClosed(new EventArgs());
                }
                //
                ///* Let's clean up the base 
                // * class's stuff first */
                base.FreeResources();
                //
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
                GC.WaitForPendingFinalizers();
                //GC.Collect();
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }

        private static void ShowPropertiesPageForInputVideoCapture(ref IBaseFilter filter)
        {
            System.Windows.Forms.Form frm = new System.Windows.Forms.Form();
            frm.StartPosition = forms.FormStartPosition.CenterScreen;
            Playout.DirectShow.Utils.FilterGraphTools.ShowFilterPropertyPage(filter, frm.Handle);
            //
        }
    }


    [Guid("B5AF8317-CD62-404D-B91E-6EEAF70D93B6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [System.Security.SuppressUnmanagedCodeSecurity]
    public interface IInputArgs
    {
        void SetSize(int width, int height, int frameRateScale, int frameRateDuration, out int output);
        void SetReplayMode(int mode);
        void SetSeek(long value);
        void GetVideoFrameRate(out int output);
	    void GetAudioSampleRate(out int output);
        void SetKey(int key);
        void GetFileSizeInfo(out int width, out int height);
        void SetNextFile(string file, long seek);
        void GetMediaInformation(out bool hasAudio, out bool hasVideo);
        void ShouldBeStop(string file, bool hasAudio, bool hasVideo, out bool should);
    }

    [Guid("388EEF20-40CC-4752-A0FF-66AA5C4AF8FA")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [System.Security.SuppressUnmanagedCodeSecurity]
    public interface ISettingsInterface
    {
        int GetParameter(IntPtr type, int buffersize, [MarshalAs(UnmanagedType.LPStr)] string value, ref int length);
        int SetParameter([MarshalAs(UnmanagedType.LPStr)]string type, [MarshalAs(UnmanagedType.LPStr)] string value);
        int GetParameterSettings([MarshalAs(UnmanagedType.LPStr)] string szResult, int nSize);
    }
}