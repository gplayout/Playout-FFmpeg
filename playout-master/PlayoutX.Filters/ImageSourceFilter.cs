using DirectShow;
using DirectShow.BaseClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayoutX.Filters
{
    #region Image Source

    [ComVisible(true)]
    public class ImageSourceStream : SourceStream
    {
        #region Constructor

        public ImageSourceStream(string _name, BaseSourceFilter _filter)
            : base(_name, _filter)
        {
        }

        #endregion

        #region Overridden Methods

        public override int GetMediaType(ref AMMediaType pMediaType)
        {
            return (m_Filter as ImageSourceFilter).GetMediaType(ref pMediaType);
        }

        public override int DecideBufferSize(ref IMemAllocatorImpl pAlloc, ref AllocatorProperties prop)
        {
            if (!IsConnected) return VFW_E_NOT_CONNECTED;
            return (m_Filter as ImageSourceFilter).DecideBufferSize(ref pAlloc, ref prop);
        }

        public override int FillBuffer(ref IMediaSampleImpl pSample)
        {
            return (m_Filter as ImageSourceFilter).FillBuffer(ref pSample);
        }

        #endregion
    }

    [ComVisible(true)]
    [Guid("170BB172-4FD1-4eb5-B6F6-A834B344268F")]
    [AMovieSetup(true)]
    [PropPageSetup(typeof(AboutForm))]
    public class ImageSourceFilter : BaseSourceFilter, IFileSourceFilter
    {
        #region Variables

        protected string m_sFileName = "";
        protected Bitmap m_pBitmap = null;
        protected long m_nAvgTimePerFrame = UNITS / 20;
        protected long m_lLastSampleTime = 0;

        #endregion

        #region Constructor

        public ImageSourceFilter()
            : base("PlayoutX Image Source Filter")
        {

        }

        ~ImageSourceFilter()
        {
            if (m_pBitmap != null)
            {
                m_pBitmap.Dispose();
                m_pBitmap = null;
            }
        }

        #endregion

        #region Overridden Methods

        protected override int OnInitializePins()
        {
            AddPin(new ImageSourceStream("Output", this));
            return NOERROR;
        }

        public override int Pause()
        {
            if (m_State == FilterState.Stopped)
            {
                m_lLastSampleTime = 0;
            }
            return base.Pause();
        }

        #endregion

        #region Methods

        public int GetMediaType(ref AMMediaType pMediaType)
        {
            if (m_pBitmap == null) return E_UNEXPECTED;
            pMediaType.majorType = DirectShow.MediaType.Video;
            pMediaType.subType = DirectShow.MediaSubType.RGB32;
            pMediaType.formatType = DirectShow.FormatType.VideoInfo;

            VideoInfoHeader vih = new VideoInfoHeader();
            vih.AvgTimePerFrame = m_nAvgTimePerFrame;
            vih.BmiHeader = new BitmapInfoHeader();
            vih.BmiHeader.Size = Marshal.SizeOf(typeof(BitmapInfoHeader));
            vih.BmiHeader.Compression = 0;
            vih.BmiHeader.BitCount = 32;
            vih.BmiHeader.Width = m_pBitmap.Width;
            vih.BmiHeader.Height = m_pBitmap.Height;
            vih.BmiHeader.Planes = 1;
            vih.BmiHeader.ImageSize = vih.BmiHeader.Width * vih.BmiHeader.Height * vih.BmiHeader.BitCount / 8;
            vih.SrcRect = new DsRect();
            vih.TargetRect = new DsRect();
            //
            AMMediaType.SetFormat(ref pMediaType, ref vih);
            pMediaType.fixedSizeSamples = true;
            pMediaType.sampleSize = vih.BmiHeader.ImageSize;

            return NOERROR;
        }

        public int DecideBufferSize(ref IMemAllocatorImpl pAlloc, ref AllocatorProperties prop)
        {
            AllocatorProperties _actual = new AllocatorProperties();

            BitmapInfoHeader _bmi = (BitmapInfoHeader)Pins[0].CurrentMediaType;
            prop.cbBuffer = _bmi.GetBitmapSize();
            if (prop.cbBuffer < _bmi.ImageSize)
            {
                prop.cbBuffer = _bmi.ImageSize;
            }
            prop.cBuffers = 1;

            int hr = pAlloc.SetProperties(prop, _actual);
            return hr;
        }

        public int FillBuffer(ref IMediaSampleImpl _sample)
        {
            BitmapInfoHeader _bmi = (BitmapInfoHeader)Pins[0].CurrentMediaType;

            IntPtr _ptr;
            _sample.GetPointer(out _ptr);
            Bitmap _bmp = new Bitmap(_bmi.Width, _bmi.Height, _bmi.Width * 4, PixelFormat.Format32bppRgb, _ptr);
            Graphics _graphics = Graphics.FromImage(_bmp);

            _graphics.DrawImage(m_pBitmap, new Rectangle(0, 0, _bmp.Width, _bmp.Height), 0, 0, m_pBitmap.Width, m_pBitmap.Height, GraphicsUnit.Pixel);
            _graphics.Dispose();
            _bmp.Dispose();
            _sample.SetActualDataLength(_bmi.ImageSize);
            _sample.SetSyncPoint(true);
            long _stop = m_lLastSampleTime + m_nAvgTimePerFrame;
            _sample.SetTime((DsLong)m_lLastSampleTime, (DsLong)_stop);
            m_lLastSampleTime = _stop;
            return NOERROR;
        }

        #endregion

        #region IFileSourceFilter Members

        public int Load(string pszFileName, AMMediaType pmt)
        {
            if (string.IsNullOrEmpty(pszFileName)) return E_POINTER;
            if (IsActive) return VFW_E_WRONG_STATE;
            m_sFileName = pszFileName;
            if (m_pBitmap != null)
            {
                m_pBitmap.Dispose();
            }
            m_pBitmap = new Bitmap(m_sFileName);
            m_pBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if (Pins[0].IsConnected)
            {
                ((BaseOutputPin)Pins[0]).ReconnectPin();
            }
            return NOERROR;
        }

        public int GetCurFile(out string pszFileName, AMMediaType pmt)
        {
            pszFileName = m_sFileName;
            if (pmt != null)
            {
                pmt.Set(Pins[0].CurrentMediaType);
            }
            return NOERROR;
        }

        #endregion
    }

    #endregion
}
