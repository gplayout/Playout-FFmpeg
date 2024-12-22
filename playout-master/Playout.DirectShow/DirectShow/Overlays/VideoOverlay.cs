using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playout.DirectShow.Overlays
{
    public class PIPController
    {
        IGraphBuilder m_graph = null;
        SampleGrabberPIP sgPip = null;
        string _filePath = "";
        int _width = 0;
        int _height = 0;
        Action<Bitmap> OnHasPicture = null;
        public void Start(string file,int width,int height, Action<Bitmap> onHasPicture)
        {
            this._filePath = file;
            this._width = width;
            this._height = height;
            this.OnHasPicture = onHasPicture;
            this.CreateGraph();
        }
        private void PIP_OnHasPicure(System.Drawing.Bitmap bitmap)
        {
            if (this._width != 0 && this._height != 0)
                bitmap = this.ResizeImage(bitmap, new Size(_width, _height));
            //
            if (this.OnHasPicture != null)
                this.OnHasPicture(bitmap);
        }
        private Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch { return imgToResize; }
        }
        public void Stop()
        {
            if (m_graph == null)
                return;
            //
            var filterGraph = m_graph as IFilterGraph2;
            var mc = (filterGraph as IMediaControl);
            FilterState filterState;
            (filterGraph as IMediaControl).Stop();
            mc.GetState(0, out filterState);
            while (filterState != FilterState.Stopped)
                mc.GetState(0, out filterState);
            Marshal.ReleaseComObject(m_graph);
            m_graph = null;
            sgPip = null;
            //
            GC.Collect();
        }
        public void Pause()
        {
            if (m_graph == null)
                return;
            //
            var filterGraph = m_graph as IFilterGraph2;
            var mc = (filterGraph as IMediaControl);
            (filterGraph as IMediaControl).Pause();
        }
        public void Resume()
        {
            if (m_graph == null)
                return;
            //
            var filterGraph = m_graph as IFilterGraph2;
            var mc = (filterGraph as IMediaControl);
            (filterGraph as IMediaControl).Run();
        }
        private void CreateGraph()
        {
            m_graph = new FilterGraphNoThread() as IGraphBuilder;
            //
            var filterGraph = m_graph as IFilterGraph2;
            IBaseFilter sourceFilter = AddFilterByName(filterGraph, FilterCategory.LegacyAmFilterCategory, "Hamed Filter");
            (sourceFilter as IFileSourceFilter).Load(_filePath, null);
            //if (this.width != 0 && this.height != 0)
            //{
            //    int output = 0;
            //    (sourceFilter as Playout.DirectShow.MediaPlayers.IInputArgs).SetSize(this.width, this.height, out output);
            //}
            //
            var samGrabber = (ISampleGrabber)new SampleGrabber();
            ConfigureSampleGrabber(samGrabber);
            filterGraph.AddFilter((IBaseFilter)samGrabber, "Sample Grabber");
            //
            IPin sgPinInput = DsFindPin.ByDirection((IBaseFilter)samGrabber, PinDirection.Input, 0);
            IPin sourcePinOut = DsFindPin.ByDirection((IBaseFilter)sourceFilter, PinDirection.Output, 0);
            filterGraph.Connect(sourcePinOut, sgPinInput);
            //
            IBaseFilter nullRendererFilter = AddFilterByName(filterGraph, FilterCategory.LegacyAmFilterCategory, "Null Renderer");
            IPin sgPinOut = DsFindPin.ByDirection((IBaseFilter)samGrabber, PinDirection.Output, 0);
            //
            filterGraph.Render(sgPinOut);
            SaveSizeInfo(samGrabber);
            //(filterGraph as IMediaControl).Run();
            //
            Marshal.ReleaseComObject(sourceFilter);
            Marshal.ReleaseComObject(samGrabber);
            Marshal.ReleaseComObject(sgPinInput);
            Marshal.ReleaseComObject(sourcePinOut);
            Marshal.ReleaseComObject(nullRendererFilter);
            Marshal.ReleaseComObject(sgPinOut);
        }

        private void ConfigureSampleGrabber(ISampleGrabber sampGrabber)
        {
            int hr;
            AMMediaType media = new AMMediaType();
            // Set the media type to Video/RBG24
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB24;
            media.formatType = FormatType.VideoInfo;
            hr = sampGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;
            this.sgPip = new SampleGrabberPIP();
            this.sgPip.OnHasPicture = this.PIP_OnHasPicure;
            // Configure the samplegrabber callback
            hr = sampGrabber.SetCallback(this.sgPip, 1);
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
                this.sgPip.m_videoWidth = videoInfoHeader.BmiHeader.Width;
                this.sgPip.m_videoHeight = videoInfoHeader.BmiHeader.Height;
                this.sgPip.m_stride = this.sgPip.m_videoWidth * (videoInfoHeader.BmiHeader.BitCount / 8);

                DsUtils.FreeAMMediaType(media);
                media = null;
            }
            catch (Exception ex)
            {

            }
        }

        protected static IBaseFilter AddFilterByName(IGraphBuilder graphBuilder, Guid deviceCategory, string friendlyName)
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
    }
    public class SampleGrabberPIP : IDisposable, ISampleGrabberCB
    {
        public int m_videoWidth;
        public int m_videoHeight;
        public int m_stride;
        public Action<Bitmap> OnHasPicture = null;
        public SampleGrabberPIP()
        {
        }
        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            try
            {
                lock (this)
                {
                    Bitmap output = new Bitmap(m_videoWidth, m_videoHeight, m_stride, PixelFormat.Format24bppRgb, pBuffer);
                    if (this.OnHasPicture != null)
                        this.OnHasPicture(output);
                }
                //
                return 0;
            }
            catch { return 0; }
        }

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        public void Dispose()
        {

        }
    }
}
