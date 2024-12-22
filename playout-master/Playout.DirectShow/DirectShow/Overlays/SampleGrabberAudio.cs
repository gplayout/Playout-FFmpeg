using DirectShowLib;
using System;
using System.Collections.Generic;
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
    public class SampleGrabberAudio : IDisposable, ISampleGrabberCB
    {
        const int MAXSAMPLES = 250; // Number of samples to be checked
        const int MAX_AUDIO_FRAME_SIZE = 192000;
        //
        private short[] savedArray;
        public bool volumeInfoBusy = false;
        public int volumePeakR = 0;
        public int volumePeakL = 0;
        public int volumeAvgR = 0;
        public int volumeAvgL = 0;
        public int m_AudioSampleRate = 0;
        long sampleIndex = 0;

        bool mutexCreatedVsc = false;
        bool vscEnabled = false;
        int vscChannel = 1;
        Mutex mutexVsc = null;
        MemoryMappedFile mmfVCsc = null;

        public SampleGrabberAudio(bool _vscEnabled,int _vscChannel)
        {
            this.vscEnabled = _vscEnabled;
            this.vscChannel = _vscChannel;
        }
        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            this.savedArray = new short[BufferLen / 2];
            Marshal.Copy(pBuffer, this.savedArray, 0, BufferLen / 2);
            //
            this.AudioCapture(savedArray, this.savedArray.Length);
            //
            if (this.vscEnabled)
            {
                #region Virtual Sound
                try
                {
                    byte[] buffer = new byte[BufferLen];
                    Marshal.Copy(pBuffer, buffer, 0, BufferLen);
                    //
                    if (mmfVCsc == null)
                    {
                        mmfVCsc = MemoryMappedFile.CreateOrOpen("PlayoutXVsc"+this.vscChannel, MAX_AUDIO_FRAME_SIZE + 4 + 4+8);
                    }
                    //
                    if (mutexVsc == null)
                        mutexVsc = new Mutex(true, "PlayoutXVscmutex"+this.vscChannel, out mutexCreatedVsc);
                    else
                        mutexVsc.WaitOne();
                    //
                    using (MemoryMappedViewStream stream = mmfVCsc.CreateViewStream())
                    {
                        BinaryWriter writer = new BinaryWriter(stream);
                        //1-sample Index
                        var sampleIndexBuffer = BitConverter.GetBytes(this.sampleIndex++);
                        writer.Write(sampleIndexBuffer, 0, sampleIndexBuffer.Length);
                        //2-Sample Rate
                        var sampleRateBuffer = BitConverter.GetBytes(44100);//this.m_AudioSampleRate);
                        writer.Write(sampleRateBuffer, 0, sampleRateBuffer.Length);
                        //3-Sample Size
                        var bufferLenBuffer = BitConverter.GetBytes(BufferLen);
                        writer.Write(bufferLenBuffer, 0, bufferLenBuffer.Length);
                        //4-Audio Buffer
                        //Log.Logger.InfoLog(BitConverter.ToString(buffer));
                        writer.Write(buffer, 0, buffer.Length);
                    }
                    mutexVsc.ReleaseMutex();
                }
                catch (Exception ex)
                {

                }
                #endregion Virtual Sound
            }
            //
            return 0;   
        }

        private void AudioCapture(short[] e, int BufferLen)
        {
            try
            {
                if ((BufferLen <= 0) || (volumeInfoBusy))
                {
                    return;
                }
                volumeInfoBusy = true;

                int leftS = 0;
                int rightS = 0;
                int avgR = 0;
                int avgL = 0;
                int peakR = 0;
                int peakL = 0;
                int size = e.GetLength(0) / 2; // Assume this is 2 channel audio
                if (size > (BufferLen / 2))
                {
                    size = BufferLen / 2;
                }

                if (size > MAXSAMPLES)
                {
                    size = MAXSAMPLES;
                }

                if (size < 2)
                {
                    volumeInfoBusy = false;
                    return;
                }

                // Check array contents
                for (int i = 0; i < size; i += 2)
                {
                    leftS = Math.Abs(e[i]);
                    avgL += leftS;
                    if (leftS > peakL)
                    {
                        peakL = leftS;
                    }
                    rightS = Math.Abs(e[i + 1]);
                    avgR += rightS;
                    if (rightS > peakR)
                    {
                        peakR = rightS;
                    }
                } // for

                volumeAvgR = avgR / size;
                volumeAvgL = avgL / size;
                volumePeakR = peakR;
                volumePeakL = peakL;
            }
            catch(Exception ex)
            {
                //ex.Log();
            }
        }

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        public void Dispose()
        {

        }
        public static IBaseFilter GetAudioGrabber(ref IFilterGraph2 m_Graph, out SampleGrabberAudio sga, bool vscEnabled, int vscChannel)
        {
            ISampleGrabber audioGrabber = new SampleGrabber() as ISampleGrabber;
            sga = new SampleGrabberAudio(vscEnabled, vscChannel);
            IBaseFilter audioGrabberFilter = (IBaseFilter)audioGrabber;
            //
            int hr = 0;

            AMMediaType media = new AMMediaType();
            media.majorType = MediaType.Audio;
            media.subType = MediaSubType.PCM;
            media.formatPtr = IntPtr.Zero;
            hr = audioGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            hr = m_Graph.AddFilter(audioGrabberFilter, "AudioGrabber");
            DsError.ThrowExceptionForHR(hr);
            //
            hr = audioGrabber.SetBufferSamples(true);
            if (hr == 0)
            {
                hr = audioGrabber.SetOneShot(false);
            }
            if (hr == 0)
            {
                hr = audioGrabber.SetCallback(sga, 1);
            }
            DsError.ThrowExceptionForHR(hr);

            return audioGrabberFilter;
        }

        public static void SetMediaSampleGrabber(ref IBaseFilter audioGrabberFilter)
        {
            ISampleGrabber audioGrabber = (ISampleGrabber)audioGrabberFilter;
            //
            AMMediaType media = new AMMediaType();
            media.formatType = FormatType.WaveEx;
            int hr;

            hr = audioGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.WaveEx) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            WaveFormatEx wav = new WaveFormatEx();
            wav = (WaveFormatEx)Marshal.PtrToStructure
                    (media.formatPtr, typeof(WaveFormatEx));
            //this.avgBytesPerSec = wav.nAvgBytesPerSec;
            //this.audioBlockAlign = wav.nBlockAlign;
            //this.audioChannels = wav.nChannels;
            //this.audioSamplesPerSec = wav.nSamplesPerSec;
            //this.audioBitsPerSample = wav.wBitsPerSample;
            Marshal.FreeCoTaskMem(media.formatPtr);
            media.formatPtr = IntPtr.Zero;
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] uint Length);
    }


}
