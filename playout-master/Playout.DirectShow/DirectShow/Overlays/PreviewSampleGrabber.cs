using DirectShowLib;
using Playout.DirectShow.MediaPlayers;
using System;
using System.Collections;
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

namespace Playout.DirectShow.Overlays
{
    public class PreviewSampleGrabber : IDisposable, ISampleGrabberCB
    {
        public Queue<IntPtr> frameQueue = new Queue<IntPtr>();
        public int m_inp_videoWidth = 0;
        public int m_inp_videoHeight = 0;

        int m_out_videoWidth = 0;
        int m_out_videoHeight = 0;
        public PreviewSampleGrabber(int ovwidth,int ovheight)
        {
            this.m_out_videoWidth = ovwidth;
            this.m_out_videoHeight = ovheight;
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            try
            {
                if (frameQueue.Count < 20)
                {
                    var bp = Marshal.AllocHGlobal(BufferLen);
                    MediaPlayerBase.CopyMemory(bp, pBuffer, (uint)BufferLen);
                    frameQueue.Enqueue(bp);
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
            if (this.frameQueue != null)
            {
                foreach (var item in this.frameQueue)
                {
                    Marshal.FreeHGlobal(item);
                }
            }
        }
    }



}
