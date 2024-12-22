using Playout.DirectShow.MediaPlayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playout.Log;
using DirectShowLib;
using System.Runtime.InteropServices;
using Playout.DirectShow.Overlays;

namespace Playout.DirectShow.DirectShow.MediaPlayers
{
    public class MediaPreviewPlayer
    {
        string _inputName = "";
        int _inputDevIndex = 0;
        enMediaType _mediaType = enMediaType.VideoFile;
        IGraphBuilder m_graph;
        
        PreviewSampleGrabber _sampleGrabber = null;
        int _outWidth = 0;
        int _outHeight = 0;
        public int InputWidth
        {
            get
            {
                if (this._sampleGrabber != null)
                    return this._sampleGrabber.m_inp_videoWidth;
                else
                    return 0;
            }
        }
        public int InputHeight
        {
            get
            {
                if (this._sampleGrabber != null)
                    return this._sampleGrabber.m_inp_videoHeight;
                else
                    return 0;
            }
        }
        //
        private IMediaControl m_mediaControl;
        //
        public Queue<IntPtr> frameQueue
        {
            get
            {
                try
                {
                    return this._sampleGrabber.frameQueue;
                }
                catch
                {
                    return null;
                }
            }
        }

        MediaState _previewState;
        public Action<MediaState> OnPreviewStateChanged;
        public MediaState PreviewState
        {
            get { return this._previewState; }
            private set
            {
                if (this._previewState != value)
                {
                    this._previewState = value;
                    if (this.OnPreviewStateChanged != null)
                        this.OnPreviewStateChanged(this._previewState);
                }
            }
        }
        //
        public bool Run(string fud,enMediaType mediaType,int outWidth,int outHeight,int inputDeviceIndex)
        {
            _inputName = fud;
            _mediaType = mediaType;
            _outWidth = outWidth;
            _outHeight = outHeight;
            _inputDevIndex = inputDeviceIndex;
            //
            bool result = BuildGraph();
            if(result)
                this.PreviewState = MediaState.Play;
            else
                this.PreviewState = MediaState.Stop;
            //
            return result;
        }
        public void Close()
        {
            try
            {
                this.FreeResources();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public void Pause()
        {
            try
            {
                if (this.m_mediaControl != null)
                {
                    this.m_mediaControl.Pause();
                    this.PreviewState = MediaState.Pause;
                }
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        //
        private bool BuildGraph()
        {
            try
            {
                FreeResources();
                //
                if (String.IsNullOrEmpty(_inputName))
                    return false;
                //
                m_graph = new FilterGraphNoThread() as IGraphBuilder;
                if (m_graph == null)
                    throw new Exception("Could not create a graph");
                //
                var filterGraph = m_graph as IFilterGraph2;
                if (filterGraph == null)
                    throw new Exception("Could not QueryInterface for the IFilterGraph2");
                //
                IBaseFilter sourceFilter = null;
                IPin sourceOutPin = null;
                //
                #region Add Source
                sourceFilter = MediaPlayerBase.AddFilterByName(filterGraph, FilterCategory.LegacyAmFilterCategory, "Hamed Filter");
                var inputArgs = (sourceFilter as IInputArgs);
                var iFileSourceFilter = (sourceFilter as IFileSourceFilter);
                //
                inputArgs.SetKey(231212);
                int output = 0;
                inputArgs.SetSize(this._outWidth, this._outHeight, 0, 0, out output);
                //
                iFileSourceFilter.Load(this._inputName, null);
                //
                sourceOutPin = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 0);
                //
                    #region Add Audio Renderer
                IPin audioSourcePinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 1);
                IBaseFilter faDev = MediaPlayerBase.AddFilterByName(m_graph, FilterCategory.LegacyAmFilterCategory, "Null Renderer");
                IPin audioDevPinIn = DsFindPin.ByDirection(faDev, PinDirection.Input, 0);
                filterGraph.Connect(audioSourcePinOut, audioDevPinIn);
                Marshal.ReleaseComObject(faDev);
                Marshal.ReleaseComObject(audioSourcePinOut);
                Marshal.ReleaseComObject(audioDevPinIn);
                #endregion Add Audio Renderer
                //
                Marshal.ReleaseComObject(inputArgs);
                Marshal.ReleaseComObject(iFileSourceFilter);
                #endregion Add Source
                //
                #region Add SampleGrabber
                ISampleGrabber samGrabber = (ISampleGrabber)new SampleGrabber();
                ConfigureSampleGrabber(samGrabber);
                filterGraph.AddFilter((IBaseFilter)samGrabber, "Sample Grabber");
                IPin sginpPin = DsFindPin.ByDirection((IBaseFilter)samGrabber, PinDirection.Input, 0);
                filterGraph.Connect(sourceOutPin, sginpPin);
                IPin sgoutPin = DsFindPin.ByDirection((IBaseFilter)samGrabber, PinDirection.Output, 0);
                #endregion Add SampleGrabber
                //
                #region Add Renderer
                IBaseFilter renderer = MediaPlayerBase.AddFilterByName(m_graph, FilterCategory.LegacyAmFilterCategory, "Null Renderer");
                switch (this._mediaType)
                {
                    case enMediaType.VideoFile:
                    case enMediaType.Url:
                    case enMediaType.ImageFile:
                        filterGraph.Render(sgoutPin);
                        break;
                    case enMediaType.Device:
                        IBaseFilter fcscVideo = MediaPlayerBase.AddFilterByName(filterGraph, FilterCategory.LegacyAmFilterCategory, "Color Space Converter");
                        IPin cscVideoPinIn = DsFindPin.ByDirection(fcscVideo, PinDirection.Input, 0);
                        IPin cscVideoPinOut = DsFindPin.ByDirection(fcscVideo, PinDirection.Output, 0);
                        //
                        filterGraph.Connect(sgoutPin, cscVideoPinIn);
                        filterGraph.Render(cscVideoPinOut);
                        //
                        Marshal.ReleaseComObject(fcscVideo);
                        Marshal.ReleaseComObject(cscVideoPinIn);
                        Marshal.ReleaseComObject(cscVideoPinOut);
                        break;
                    default:
                        return false;
                }
                #endregion Add Renderer

                //
                SaveSizeInfo(samGrabber);
                this.m_mediaControl = m_graph as IMediaControl;
                //
                Marshal.ReleaseComObject(sourceFilter);
                Marshal.ReleaseComObject(sourceOutPin);
                Marshal.ReleaseComObject(samGrabber);
                Marshal.ReleaseComObject(sginpPin);
                Marshal.ReleaseComObject(sgoutPin);
                Marshal.ReleaseComObject(renderer);
                //
                if (m_mediaControl != null)
                    m_mediaControl.Run();
                //
                return true;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        private void FreeResources()
        {
            try
            {
                if (m_mediaControl != null)
                {
                    m_mediaControl.Stop();
                    FilterState filterState;
                    m_mediaControl.GetState(0, out filterState);

                    while (filterState != FilterState.Stopped)
                        m_mediaControl.GetState(0, out filterState);
                }
                //

                if (m_graph != null)
                {
                    MediaPlayerBase.RemoveFilters(m_graph);
                    Marshal.ReleaseComObject(m_graph);
                    m_graph = null;
                }
                //
                if (this._sampleGrabber != null)
                {
                    this._sampleGrabber.Dispose();
                    this._sampleGrabber = null;
                }
                //
                if (m_mediaControl != null)
                    Marshal.ReleaseComObject(m_mediaControl);
                m_mediaControl = null;
                //
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
                GC.WaitForPendingFinalizers();
                //GC.Collect();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            finally
            {
                this.PreviewState = MediaState.Stop;
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
            this._sampleGrabber = new PreviewSampleGrabber(_outWidth, _outHeight);
            // Configure the samplegrabber callback
            hr = sampGrabber.SetCallback(this._sampleGrabber, 1);
            DsError.ThrowExceptionForHR(hr);
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
                this._sampleGrabber.m_inp_videoWidth = videoInfoHeader.BmiHeader.Width;
                this._sampleGrabber.m_inp_videoHeight = videoInfoHeader.BmiHeader.Height;

                DsUtils.FreeAMMediaType(media);
                media = null;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
