using DirectShowLib;
using Playout.DirectShow.MediaPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Playout.Log;

namespace Playout.DirectShow.Overlays
{
    public class SampleGrabberOverlays : IDisposable, ISampleGrabberCB
    {
        public static Queue<Bitmap> frameQueue = new Queue<Bitmap>();
        public SortedList<int, BaseOverlay> Overlays { get; set; }
        public int m_videoWidth;
        public int m_videoHeight;
        public int m_videofile_width;
        public int m_videofile_height;
        public int m_stride;
        public int m_VideoFrameRate;
        public string m_ShowupEffect;

        int sf_level = 255;
        int sf_slide = 0;
        int sf_slideTo1 = 0;
        int sf_slideTo2 = 0;
        int sf_slideFactor = 30;
        long frameIndex = 0;

        bool mutexCreatedVCam = false;
        bool vcamEnabled = false;
        int vcamChannel = 1;
        Mutex mutexVCam = null;
        MemoryMappedFile mmfVCam = null;

        public int PIP_PosLeft { get; set; }
        public int PIP_PosTop { get; set; }
        Bitmap prelastPIPOverlay;
        public Bitmap PIPOverlay { get; set; }
        public SampleGrabberOverlays(bool _vcamEnabled,int _vcamChannel)
        {
            this.vcamEnabled = _vcamEnabled;
            this.vcamChannel = _vcamChannel;
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            try
            {
                if (this.Overlays == null && this.PIPOverlay == null)
                    return 0;
                //
                Bitmap output = new Bitmap(this.m_videoWidth, this.m_videoHeight,
                            this.m_stride, PixelFormat.Format32bppRgb, pBuffer);
                //
                Graphics g = Graphics.FromImage(output);
                //
                if (this.Overlays != null)
                {
                    lock (this)
                    {
                        foreach (var item in this.Overlays)
                        {
                            //item.Value.ProcessBufferCB(ref g, ref output, this.m_videofile_width, this.m_videofile_height);
                            item.Value.ProcessBufferCB(ref g, ref output, this.m_videoWidth, this.m_videoHeight);
                        }
                    }
                }
                //
                if (PIPOverlay != null)
                {
                    lock (PIPOverlay)
                    {
                        g.DrawImage(this.PIPOverlay, this.PIP_PosLeft, this.PIP_PosTop);
                        this.prelastPIPOverlay = this.PIPOverlay;
                        PIPOverlay = null;
                    }
                }
                else if (this.prelastPIPOverlay != null)
                {
                    g.DrawImage(this.prelastPIPOverlay, this.PIP_PosLeft, this.PIP_PosTop);
                }
                #region Switch Effects
                switch (this.m_ShowupEffect)
                {
                    case "Fade":
                        if (sf_level >= 0)
                        {
                            Pen pLight = new Pen(Color.FromArgb(sf_level, 0, 0, 0), this.m_videoWidth * 2);
                            g.DrawLine(pLight, -1, -1, this.m_videoWidth, this.m_videoHeight);
                        }
                        sf_level -= 5;
                        break;
                    case "SlideToDown":
                        sf_slideTo1 = this.m_videoHeight - sf_slide * (this.m_videoHeight / sf_slideFactor);
                        if (sf_slideTo1 >= 0)
                        {
                            g.FillRectangle(Brushes.Black, 0, 0, this.m_videoWidth, sf_slideTo1);
                        }
                        sf_slide++;
                        break;
                    case "SlideToUp":
                        sf_slideTo1 = this.m_videoHeight - (sf_slideFactor - sf_slide) * (this.m_videoHeight / sf_slideFactor);
                        if (sf_slideTo1 >= 0)
                        {
                            g.FillRectangle(Brushes.Black, 0, sf_slideTo1, this.m_videoWidth, this.m_videoHeight);
                        }
                        sf_slide++;
                        break;
                    case "SlideToLeft":
                        sf_slideTo1 = this.m_videoWidth - sf_slide * (this.m_videoWidth / sf_slideFactor);
                        if (sf_slideTo1 >= 0)
                        {
                            g.FillRectangle(Brushes.Black, 0, 0, sf_slideTo1, this.m_videoHeight);
                        }
                        sf_slide++;
                        break;
                    case "SlideToRight":
                        sf_slideTo1 = this.m_videoWidth - (sf_slideFactor - sf_slide) * (this.m_videoWidth / sf_slideFactor);
                        if (sf_slideTo1 >= 0)
                        {
                            g.FillRectangle(Brushes.Black, sf_slideTo1, 0, this.m_videoWidth, this.m_videoHeight);
                        }
                        sf_slide++;
                        break;
                    case "SlideToCenter":
                        sf_slideTo1 = sf_slide * (this.m_videoWidth / (sf_slideFactor * 2));
                        sf_slideTo2 = sf_slide * (this.m_videoHeight / (sf_slideFactor * 2));
                        if (sf_slide <= sf_slideFactor*2)
                        {
                            g.FillRectangle(Brushes.Black, sf_slideTo1, sf_slideTo2,
                                this.m_videoWidth - 2 * sf_slideTo1, this.m_videoHeight - 2 * sf_slideTo2);
                        }
                        sf_slide++;
                        break;
                    default:
                        break;
                }
                #endregion Switch Effects
                //////////////////////////
                int bmpWidth = output.Width;
                int bmpHeight = output.Height;
                g.Flush();
                g.Dispose();
                //Check Frame Is Ready
                if (frameQueue.Count < 20 && !MediaPlayerBase.OutputPreview_IsPaused)
                {
                    //Bitmap resized = new Bitmap(output, new Size(355, 200));
                    //byte[] buffer = new byte[BufferLen];
                    //var bp = Marshal.AllocHGlobal(BufferLen);
                    //CopyMemory(bp, pBuffer, (uint)BufferLen);
                    //System.Runtime.InteropServices.Marshal.Copy(pBuffer,0, bp,BufferLen);
                    //
                    var width = MediaPlayerBase.OutputPreview_Width == 0 ? 600 : MediaPlayerBase.OutputPreview_Width;
                    var height = MediaPlayerBase.OutputPreview_Height == 0 ? 337 : MediaPlayerBase.OutputPreview_Height;
                    //
                    var bitmap = MediaPlayerBase.ResizeBitmap((Image)output,
                        System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor, width, height);
                    frameQueue.Enqueue(bitmap);
                }
                //
                output.Dispose();
                //
                if (this.vcamEnabled)
                {
                    #region Virtual Camera
                    try
                    {
                        if (mmfVCam == null)
                        {
                            mmfVCam = MemoryMappedFile.CreateOrOpen("PlayoutXVCam"+vcamChannel,
                                sizeof(long) + 4 * sizeof(int) +
                                bmpWidth * bmpHeight * 4);
                        }
                        //
                        if (mutexVCam == null)
                            mutexVCam = new Mutex(true, "PlayoutXVCammutex"+vcamChannel, out mutexCreatedVCam);
                        else
                            mutexVCam.WaitOne(1);
                        //
                        byte[] rgbValues = new byte[(int)(BufferLen)];
                        Marshal.Copy(pBuffer, rgbValues, 0, rgbValues.Length);
                        //
                        var frameIndexBuffer = BitConverter.GetBytes(this.frameIndex++);
                        var frameRateBuffer = BitConverter.GetBytes(this.m_VideoFrameRate);
                        var widthBuffer = BitConverter.GetBytes(bmpWidth);
                        var heightBuffer = BitConverter.GetBytes(bmpHeight);
                        var lengthBuffer = BitConverter.GetBytes(rgbValues.Length);
                        using (MemoryMappedViewStream stream = mmfVCam.CreateViewStream())
                        {
                            BinaryWriter writer = new BinaryWriter(stream);
                            //1-Frame Index
                            writer.Write(frameIndexBuffer, 0, frameIndexBuffer.Length);
                            //2-Frame Rate Buffer
                            writer.Write(frameRateBuffer, 0, frameRateBuffer.Length);
                            //3-Width
                            writer.Write(widthBuffer, 0, widthBuffer.Length);
                            //4-Height
                            writer.Write(heightBuffer, 0, heightBuffer.Length);
                            //5-Buffer Length
                            writer.Write(lengthBuffer, 0, lengthBuffer.Length);
                            //
                            writer.Write(rgbValues, 0, rgbValues.Length);
                        }
                        mutexVCam.ReleaseMutex();
                    }
                    catch (Exception ex)
                    {
                        try { mutexVCam.ReleaseMutex(); }
                        catch { }
                    }
                    #endregion Virtual Camera
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
            try
            {
                if (frameQueue != null)
                {
                    while (frameQueue.Count > 0)
                    {
                        var item = frameQueue.Dequeue();
                        item.Dispose();
                    }
                }
                if (this.Overlays != null)
                {
                    while (this.Overlays.Count > 0)
                    {
                        var item = this.Overlays.ToList()[0];
                        if (item.Value != null)
                            item.Value.Dispose();
                        this.Overlays.Remove(item.Key);
                    }
                }
                if (this.mutexVCam != null)
                    this.mutexVCam.Dispose();
                if (this.mmfVCam != null)
                    this.mmfVCam.Dispose();
                if (this.prelastPIPOverlay != null)
                    this.prelastPIPOverlay.Dispose();
                if (this.PIPOverlay != null)
                    this.PIPOverlay.Dispose();
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
    }
}
