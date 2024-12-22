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
    #region Text Over Filter

    [ComVisible(true)]
    [Guid("8AF6F710-1AF5-4952-AAFF-ACCD0DB2C9BB")]
    [AMovieSetup(true)]
    [PropPageSetup(typeof(AboutForm))]
    public class TextOverFilter : TransformFilter
    {
        #region Variables

        public string OverlayText { get; set; }
        private Font m_Font = new Font("Arial", 20.0f, FontStyle.Regular, GraphicsUnit.Point);
        private Color m_Color = Color.Red;

        #endregion

        #region Constructor

        public TextOverFilter()
            : base("PlayoutX Text Overlay Filter")
        {
        }

        #endregion

        #region Overridden Methods

        public override int CheckInputType(AMMediaType pmt)
        {
            if (pmt.majorType != MediaType.Video)
            {
                return VFW_E_TYPE_NOT_ACCEPTED;
            }
            if (pmt.subType != MediaSubType.RGB32)
            {
                return VFW_E_TYPE_NOT_ACCEPTED;
            }
            if (pmt.formatType != FormatType.VideoInfo)
            {
                return VFW_E_TYPE_NOT_ACCEPTED;
            }
            if (pmt.formatPtr == IntPtr.Zero)
            {
                return VFW_E_TYPE_NOT_ACCEPTED;
            }
            return NOERROR;
        }

        public override int Transform(ref IMediaSampleImpl _input, ref IMediaSampleImpl _sample)
        {
            int lDataLength = _input.GetActualDataLength();
            _sample.SetActualDataLength(lDataLength);
            IntPtr _ptrIn;
            IntPtr _ptrOut;

            _input.GetPointer(out _ptrIn);
            _sample.GetPointer(out _ptrOut);

            BitmapInfoHeader _bmiIn = (BitmapInfoHeader)Input.CurrentMediaType;
            BitmapInfoHeader _bmiOut = (BitmapInfoHeader)Output.CurrentMediaType;
            Bitmap _bmpIn = new Bitmap(_bmiIn.Width, _bmiIn.Height, _bmiIn.Width * 4, PixelFormat.Format32bppRgb, _ptrIn);
            Bitmap _bmpOut = new Bitmap(_bmiOut.Width, _bmiOut.Height, _bmiOut.Width * 4, PixelFormat.Format32bppRgb, _ptrOut);

            {
                _bmpIn.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Graphics _graphics = Graphics.FromImage(_bmpIn);

                StringFormat _format = new StringFormat();
                _format.Alignment = StringAlignment.Center;
                _format.LineAlignment = StringAlignment.Center;

                Brush _brush = new SolidBrush(m_Color);
                RectangleF _rect = new RectangleF(0, 0, _bmpIn.Width, _bmpIn.Height);
                _graphics.DrawString(OverlayText, m_Font, _brush, _rect, _format);

                _graphics.Dispose();
                _bmpIn.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            {
                Graphics _graphics = Graphics.FromImage(_bmpOut);
                _graphics.DrawImage(_bmpIn, 0, 0);
                _graphics.Dispose();
            }
            _bmpOut.Dispose();
            _bmpIn.Dispose();
            return S_OK;
        }

        public override int DecideBufferSize(ref IMemAllocatorImpl pAlloc, ref AllocatorProperties prop)
        {
            if (!Output.IsConnected) return VFW_E_NOT_CONNECTED;
            if (Output.CurrentMediaType.majorType != MediaType.Video) return VFW_E_INVALIDMEDIATYPE;
            AllocatorProperties _actual = new AllocatorProperties();
            BitmapInfoHeader _bmi = (BitmapInfoHeader)Output.CurrentMediaType;
            if (_bmi == null) return VFW_E_INVALIDMEDIATYPE;
            prop.cbBuffer = _bmi.GetBitmapSize();
            if (prop.cbBuffer < _bmi.ImageSize)
            {
                prop.cbBuffer = _bmi.ImageSize;
            }
            prop.cBuffers = 1;
            int hr = pAlloc.SetProperties(prop, _actual);
            return hr;
        }

        public override int GetMediaType(int iPosition, ref AMMediaType pMediaType)
        {
            if (iPosition > 0) return VFW_S_NO_MORE_ITEMS;
            if (pMediaType == null) return E_INVALIDARG;
            if (!Input.IsConnected) return VFW_E_NOT_CONNECTED;

            AMMediaType.Copy(Input.CurrentMediaType, ref pMediaType);

            return NOERROR;
        }

        public override int CheckTransform(AMMediaType mtIn, AMMediaType mtOut)
        {
            return AMMediaType.AreEquals(mtIn, mtOut) ? NOERROR : VFW_E_INVALIDMEDIATYPE;
        }

        #endregion
    }

    #endregion
}
