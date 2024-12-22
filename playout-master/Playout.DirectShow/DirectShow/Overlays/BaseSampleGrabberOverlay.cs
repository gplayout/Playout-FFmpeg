using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.IO;
using Playout.Log;

namespace Playout.DirectShow.Overlays
{
    public enum VDBufferState { Init = 0, Empty = 1, Full = 2 }
    public abstract class BaseSampleGrabberOverlay: IDisposable
    {
        public byte[] VD_Buffer { get; set; }
        public VDBufferState VD_BufferIsReady { get; set; }
        public object VD_BufferLock = new object();
        public bool VD_IsEnable { get; private set; }

        int vd_ChannelNo = 1;
        string vd_mutexName;
        string vd_mmfName;
        Thread vd_thread;
        bool vd_runThread;
        bool vd_mutexCreated = false;
        Mutex vd_mutex = null;
        MemoryMappedFile vd_mmf = null;
        public BaseSampleGrabberOverlay(bool _vd_isEnable,int _vd_channelNo,string _mutexName,
            string _mmfName)
        {
            this.vd_mutexName = _mutexName;
            this.vd_mmfName = _mmfName;
            this.VD_IsEnable = _vd_isEnable;
            this.vd_ChannelNo = _vd_channelNo;
            this.VD_Buffer = new byte[0];
            this.VD_BufferIsReady = VDBufferState.Init;
            //
            vd_thread = new Thread(this.ThreadDriver);
            vd_runThread = true;
            vd_thread.Start();
        }

        private void ThreadDriver()
        {
            while (vd_runThread)
            {
                try
                {
                    if (this.VD_BufferIsReady != VDBufferState.Full)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    //
                    if (vd_mmf == null)
                    {
                        vd_mmf = MemoryMappedFile.CreateOrOpen(vd_mmfName, this.VD_Buffer.LongLength);
                    }
                    //
                    bool mutexCreated = false;
                    if (vd_mutex == null)
                        vd_mutex = new Mutex(true, vd_mutexName, out mutexCreated);
                    else
                    {
                        vd_mutex.WaitOne();
                    }
                    //
                    lock (this.VD_BufferLock)
                    {
                        this.VD_BufferIsReady = VDBufferState.Empty;
                        using (MemoryMappedViewStream stream = vd_mmf.CreateViewStream())
                        {
                            BinaryWriter writer = new BinaryWriter(stream);
                            writer.Write(this.VD_Buffer);
                        }
                    }
                    //
                    vd_mutex.ReleaseMutex();
                }
                catch (Exception ex)
                {
                    try { vd_mutex.ReleaseMutex(); }
                    catch { }
                }
                //
                Thread.Sleep(10);
            }
        }
        public virtual void Dispose()
        {
            try
            {
                vd_runThread = false;
                //
                if (vd_thread != null)
                {
                    vd_thread.Join();
                }
                //
                if (this.vd_mutex != null)
                    this.vd_mutex.Dispose();
                if (this.vd_mmf != null)
                    this.vd_mmf.Dispose();
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
    }
}
