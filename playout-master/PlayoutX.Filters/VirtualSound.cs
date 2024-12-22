using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using DirectShow;
using DirectShow.BaseClasses;
using Sonic;
using System.IO;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace PlayoutX.Filters
{
    #region Virtual Sound

    public class AudioData
    {
        public long SampleIndex { get; set; }
        public int SampleRate { get; set; }
        public int SampleSize { get; set; }
        public byte[] Sample { get; set; }
    }

    [ComVisible(false)]
    public class VirtualSoundStream : SourceStream
                        , IAMStreamControl
						, IKsPropertySet
						, IAMPushSource
                        , IAMLatency
						, IAMStreamConfig
						, IAMBufferNegotiation
    {

        #region Constants

        public static HRESULT E_PROP_SET_UNSUPPORTED { get { unchecked { return (HRESULT)0x80070492; } } }
        public static HRESULT E_PROP_ID_UNSUPPORTED { get { unchecked { return (HRESULT)0x80070490; } } }

        #endregion

        #region Variables

        protected object m_csPinLock = new object();
        protected object m_csTimeLock = new object();
	    protected long m_rtStart = 0;
        protected long m_rtStreamOffset = 0;
        protected long m_rtStreamOffsetMax = -1;
        protected long m_rtStartAt = -1;
        protected long m_rtStopAt = -1;
        protected int m_dwStopCookie = 0;
        protected int m_dwStartCookie = 0;
        protected bool m_bShouldFlush = false;
        protected bool m_bStartNotified = false;
        protected bool m_bStopNotified = false;
        protected AllocatorProperties m_pProperties = null;
        protected IReferenceClockImpl m_pClock = null;
	    // Clock Token
        protected int m_dwAdviseToken = 0;
	    // Clock Semaphore
        protected Semaphore m_hSemaphore = null;
	    // Clock Start time
        protected long m_rtClockStart = 0;
	    // Clock Stop time
        protected long m_rtClockStop = 0;

        #endregion

        #region Constructor

        public VirtualSoundStream(string _name, BaseSourceFilter _filter)
            : base(_name, _filter)
        {
            m_mt.majorType = Guid.Empty;
            GetMediaType(0, ref m_mt);
        }

        #endregion

        #region Overridden Methods

        public override int SetMediaType(AMMediaType mt)
        {
            if (mt == null) return E_POINTER;
            if (mt.formatPtr == IntPtr.Zero) return VFW_E_INVALIDMEDIATYPE;
            HRESULT hr = (HRESULT)CheckMediaType(mt);
            if (hr.Failed) return hr;
            hr = (HRESULT)base.SetMediaType(mt);
            if (hr.Failed) return hr;
            if (m_pProperties != null)
            {
                SuggestAllocatorProperties(m_pProperties);
            }
            return (m_Filter as VirtualSoundFilter).SetMediaType(mt);
        }

        public override int CheckMediaType(AMMediaType pmt)
        {
            return (m_Filter as VirtualSoundFilter).CheckMediaType(pmt);
        }

        public override int GetMediaType(int iPosition, ref AMMediaType pMediaType)
        {
            return (m_Filter as VirtualSoundFilter).GetMediaType(iPosition,ref pMediaType);
        }

        public override int DecideBufferSize(ref IMemAllocatorImpl pAlloc, ref AllocatorProperties pProperties)
        {
            if (!IsConnected) return VFW_E_NOT_CONNECTED;
            AllocatorProperties _actual = new AllocatorProperties();
            HRESULT hr = (HRESULT)GetAllocatorProperties(_actual);
            if (SUCCEEDED(hr) && _actual.cBuffers <= pProperties.cBuffers && _actual.cbBuffer <= pProperties.cbBuffer && _actual.cbAlign == pProperties.cbAlign)
            {
                AllocatorProperties Actual = new AllocatorProperties();
                hr = (HRESULT)pAlloc.SetProperties(pProperties, Actual);
                if (SUCCEEDED(hr))
                {
                    pProperties.cbAlign  = Actual.cbAlign;
                    pProperties.cbBuffer = Actual.cbBuffer;
                    pProperties.cbPrefix = Actual.cbPrefix;
                    pProperties.cBuffers = Actual.cBuffers;
                }
            }
            return (m_Filter as VirtualSoundFilter).DecideBufferSize(ref pAlloc, ref pProperties);
        }

        public override int Active()
        {
            m_rtStart = 0;
            m_bStartNotified = false;
            m_bStopNotified = false;
            {
                lock (m_Filter.FilterLock)
                {
                    m_pClock = m_Filter.Clock;
		            if (m_pClock.IsValid)
		            {
                        m_pClock._AddRef();
                        m_hSemaphore = new Semaphore(0, 0x7FFFFFFF);
		            }
                }
            }
            return base.Active();
        }

        public override int Inactive()
        {
            HRESULT hr = (HRESULT)base.Inactive();
            if (m_pClock != null)
            {
                if (m_dwAdviseToken != 0)
                {
                    m_pClock.Unadvise(m_dwAdviseToken);
                    m_dwAdviseToken = 0;
                }
                m_pClock._Release();
                m_pClock = null;
                if (m_hSemaphore != null)
                {
                    m_hSemaphore.Close();
                    m_hSemaphore = null;
                }
            }
            return hr;
        }

        public override int FillBuffer(ref IMediaSampleImpl _sample)
        {
            {
                AMMediaType pmt;
                if (S_OK == _sample.GetMediaType(out pmt))
                {
                    if (FAILED(SetMediaType(pmt)))
                    {
                        ASSERT(false);
                        _sample.SetMediaType(null);
                    }
                    pmt.Free();
                }
            }
            long _start, _stop;
            HRESULT hr = NOERROR;
            long rtLatency;
            /////////////////////////////////////////////////////
            rtLatency = 232199;
            _start = m_rtStart;
            hr = (HRESULT)(m_Filter as VirtualSoundFilter).FillBuffer(ref _sample);
            _stop = _start + rtLatency;
            _sample.SetTime(_start, _stop);
            m_rtStart = _stop;
            _sample.SetPreroll(false);
            _sample.SetDiscontinuity(false);
            _sample.SetSyncPoint(true);
            return NOERROR;
            /////////////////////////////////////////////////////
            if (FAILED(GetLatency(out rtLatency)))
            {
                rtLatency = UNITS / 30;
            }
            bool bShouldDeliver = false;
            do
            {
                if (m_dwAdviseToken == 0)
                {
                    m_pClock.GetTime(out m_rtClockStart);
                    hr = (HRESULT)m_pClock.AdvisePeriodic(m_rtClockStart + rtLatency, rtLatency, m_hSemaphore.Handle, out m_dwAdviseToken);
                    hr.Assert();
                }
                else
                {
                    if (!m_hSemaphore.WaitOne())
                    {
                        ASSERT(FALSE);
                    }
                }
                bShouldDeliver = TRUE;
                _start = m_rtStart;
                _stop = m_rtStart + 1;
                _sample.SetTime(_start, _stop);
                hr = (HRESULT)(m_Filter as VirtualSoundFilter).FillBuffer(ref _sample);
                if (FAILED(hr) || S_FALSE == hr) return hr;

                m_pClock.GetTime(out m_rtClockStop);
                _sample.GetTime(out _start, out _stop);
                
                if (rtLatency > 0 && rtLatency * 3 < m_rtClockStop - m_rtClockStart)
                {
                    m_rtClockStop = m_rtClockStart + rtLatency;
                }
                _stop = _start + (m_rtClockStop - m_rtClockStart);
                m_rtStart = _stop;
                lock (m_csPinLock)
                {
                    _start -= m_rtStreamOffset;
                    _stop -= m_rtStreamOffset;
                }
                _sample.SetTime(_start, _stop);
                m_rtClockStart = m_rtClockStop;

                bShouldDeliver = ((_start >= 0) && (_stop >= 0));

                if (bShouldDeliver)
                {
                    lock (m_csPinLock)
                        if (m_rtStartAt != -1)
                        {
                            if (m_rtStartAt > _start)
                            {
                                bShouldDeliver = FALSE;
                            }
                            else
                            {
                                if (m_dwStartCookie != 0 && !m_bStartNotified)
                                {
                                    m_bStartNotified = TRUE;
                                    hr = (HRESULT)m_Filter.NotifyEvent(EventCode.StreamControlStarted, Marshal.GetIUnknownForObject(this), (IntPtr)m_dwStartCookie);
                                    if (FAILED(hr)) return hr;
                                }
                            }
                        }
                    if (!bShouldDeliver) continue;
                    if (m_rtStopAt != -1)
                    {
                        if (m_rtStopAt < _start)
                        {
                            if (!m_bStopNotified)
                            {
                                m_bStopNotified = TRUE;
                                if (m_dwStopCookie != 0)
                                {
                                    hr = (HRESULT)m_Filter.NotifyEvent(EventCode.StreamControlStopped, Marshal.GetIUnknownForObject(this), (IntPtr)m_dwStopCookie);
                                    if (FAILED(hr)) return hr;
                                }
                                bShouldDeliver = m_bShouldFlush;
                            }
                            else
                            {
                                bShouldDeliver = FALSE;
                            }
                            // EOS
                            if (!bShouldDeliver) return S_FALSE;
                        }
                    }
                }
            }
            while (!bShouldDeliver);

            return NOERROR;
        }

        #endregion

        #region IAMBufferNegotiation Members

        public int SuggestAllocatorProperties(AllocatorProperties pprop)
        {
            return S_OK;
            //if (IsConnected) return VFW_E_ALREADY_CONNECTED;
            //HRESULT hr = (HRESULT)(m_Filter as VirtualSoundFilter).SuggestAllocatorProperties(pprop);
            //if (FAILED(hr))
            //{
            //    m_pProperties = null;
            //    return hr;
            //}
            //if (m_pProperties == null)
            //{
            //    m_pProperties = new AllocatorProperties();
            //    (m_Filter as VirtualSoundFilter).GetAllocatorProperties(m_pProperties);
            //}
            //if (pprop.cbBuffer != -1) m_pProperties.cbBuffer = pprop.cbBuffer;
            //if (pprop.cbAlign != -1) m_pProperties.cbAlign = pprop.cbAlign;
            //if (pprop.cbPrefix != -1) m_pProperties.cbPrefix = pprop.cbPrefix;
            //if (pprop.cBuffers != -1) m_pProperties.cBuffers = pprop.cBuffers;
            //return NOERROR;
        }

        public int GetAllocatorProperties(AllocatorProperties pprop)
        {
            this.Allocator.GetProperties(pprop);
            return S_OK;
            //if (pprop == null) return E_POINTER;
            //if (m_pProperties != null)
            //{
            //    pprop.cbAlign = m_pProperties.cbAlign;
            //    pprop.cbBuffer = m_pProperties.cbBuffer;
            //    pprop.cbPrefix = m_pProperties.cbPrefix;
            //    pprop.cBuffers = m_pProperties.cBuffers;
            //    return NOERROR;
            //}
            //if (IsConnected)
            //{
            //    HRESULT hr = (HRESULT)Allocator.GetProperties(pprop);
            //    if (SUCCEEDED(hr) && pprop.cBuffers > 0 && pprop.cbBuffer > 0) return hr;
            //}
            //return (m_Filter as VirtualSoundFilter).GetAllocatorProperties(pprop);
        }

        #endregion

        #region IAMStreamConfig Members

        public int SetFormat(AMMediaType pmt)
        {
            if (m_Filter.IsActive) return VFW_E_WRONG_STATE;
            HRESULT hr;
            AMMediaType _newType = new AMMediaType(pmt);
            AMMediaType _oldType = new AMMediaType(m_mt);
            hr = (HRESULT)CheckMediaType(_newType);
            if (FAILED(hr)) return hr;
            m_mt.Set(_newType);
            if (IsConnected)
            {
                hr = (HRESULT)Connected.QueryAccept(_newType);
                if (SUCCEEDED(hr))
                {
                    hr = (HRESULT)m_Filter.ReconnectPin(this, _newType);
                    if (SUCCEEDED(hr))
                    {
                        hr = (HRESULT)(m_Filter as VirtualSoundFilter).SetMediaType(_newType);
                    }
                    else
                    {
                        m_mt.Set(_oldType);
                        m_Filter.ReconnectPin(this, _oldType);
                    }
                }
            }
            else
            {
                hr = (HRESULT)(m_Filter as VirtualSoundFilter).SetMediaType(_newType);
            }
            return hr;
        }

        public int GetFormat(out AMMediaType pmt)
        {
            pmt = new AMMediaType(m_mt);
            return NOERROR;
        }

        public int GetNumberOfCapabilities(IntPtr piCount, IntPtr piSize)
        {
            int iCount;
            int iSize;
            HRESULT hr = (HRESULT)(m_Filter as VirtualSoundFilter).GetNumberOfCapabilities(out iCount, out iSize);
            if (hr.Failed) return hr;
            if (piCount != IntPtr.Zero)
            {
                Marshal.WriteInt32(piCount, iCount);
            }
            if (piSize != IntPtr.Zero)
            {
                Marshal.WriteInt32(piSize, iSize);
            }
            return hr;
        }

        public int GetStreamCaps(int iIndex, IntPtr ppmt, IntPtr pSCC)
        {
            AMMediaType pmt;
            AudioStreamConfigCaps _caps;
            HRESULT hr = (HRESULT)(m_Filter as VirtualSoundFilter).GetStreamCaps(iIndex, out pmt, out _caps);
            if (hr != S_OK) return hr;

            if (ppmt != IntPtr.Zero)
            {
                IntPtr _ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(pmt));
                Marshal.StructureToPtr(pmt, _ptr, true);
                Marshal.WriteIntPtr(ppmt, _ptr);
            }
            if (pSCC != IntPtr.Zero)
            {
                Marshal.StructureToPtr(_caps, pSCC, false);
            }
            return hr;
        }

        #endregion

        #region IAMPushSource Members

        public int GetPushSourceFlags(out AMPushSourceFlags pFlags)
        {
            pFlags = AMPushSourceFlags.None;
            return NOERROR;
        }

        public int SetPushSourceFlags(AMPushSourceFlags Flags)
        {
            return E_NOTIMPL;
        }

        public int SetStreamOffset(long rtOffset)
        {
            lock (m_csPinLock)
            {
                m_rtStreamOffset = rtOffset;
                if (m_rtStreamOffset > m_rtStreamOffsetMax) m_rtStreamOffsetMax = m_rtStreamOffset;                
            }
            return NOERROR;
        }

        public int GetStreamOffset(out long prtOffset)
        {
            prtOffset = m_rtStreamOffset;
            return NOERROR;
        }

        public int GetMaxStreamOffset(out long prtMaxOffset)
        {
            prtMaxOffset = 0;
            if (m_rtStreamOffsetMax == -1)
            {
                HRESULT hr = (HRESULT)GetLatency(out m_rtStreamOffsetMax);
                if (FAILED(hr)) return hr;
                if (m_rtStreamOffsetMax < m_rtStreamOffset) m_rtStreamOffsetMax = m_rtStreamOffset;
            }
            prtMaxOffset = m_rtStreamOffsetMax;
            return NOERROR;
        }

        public int SetMaxStreamOffset(long rtMaxOffset)
        {
            if (rtMaxOffset < m_rtStreamOffset) return E_INVALIDARG;
            m_rtStreamOffsetMax = rtMaxOffset;
            return NOERROR;
        }

        #endregion

        #region IKsPropertySet Members

        public int Set(Guid guidPropSet, int dwPropID, IntPtr pInstanceData, int cbInstanceData, IntPtr pPropData, int cbPropData)
        {
            return E_NOTIMPL;
        }

        public int Get(Guid guidPropSet, int dwPropID, IntPtr pInstanceData, int cbInstanceData, IntPtr pPropData, int cbPropData, out int pcbReturned)
        {
            pcbReturned = Marshal.SizeOf(typeof(Guid));
            if (guidPropSet != PropSetID.Pin)
            {
                return E_PROP_SET_UNSUPPORTED;
            }
            if (dwPropID != (int)AMPropertyPin.Category)
            {
                return E_PROP_ID_UNSUPPORTED;
            }
            if (pPropData == IntPtr.Zero)
            {
                return NOERROR;
            }
            if (cbPropData < Marshal.SizeOf(typeof(Guid)))
            {
                return E_UNEXPECTED;
            }
            Marshal.StructureToPtr(PinCategory.Capture, pPropData, false);
            return NOERROR;
        }

        public int QuerySupported(Guid guidPropSet, int dwPropID, out KSPropertySupport pTypeSupport)
        {
            pTypeSupport = KSPropertySupport.Get;
            if (guidPropSet != PropSetID.Pin)
            {
                return E_PROP_SET_UNSUPPORTED;
            }
            if (dwPropID != (int)AMPropertyPin.Category)
            {
                return E_PROP_ID_UNSUPPORTED;
            }
            return S_OK;
        }

        #endregion

        #region IAMStreamControl Members

        public int StartAt(DsLong ptStart, int dwCookie)
        {
            lock (m_csPinLock)
            {
                if (ptStart != null && ptStart != MAX_LONG)
                {
                    m_rtStartAt = ptStart;
                    m_dwStartCookie = dwCookie;
                }
                else
                {
                    m_rtStartAt = -1;
                    m_dwStartCookie = 0;
                }
            }
	        return NOERROR;
        }

        public int StopAt(DsLong ptStop, bool bSendExtra, int dwCookie)
        {
            lock (m_csPinLock)
            {
                if (ptStop != null && ptStop != MAX_LONG)
                {
                    m_rtStopAt = ptStop;
                    m_bShouldFlush = bSendExtra;
                    m_dwStopCookie = dwCookie;
                }
                else
                {
                    m_rtStopAt = -1;
                    m_bShouldFlush = false;
                    m_dwStopCookie = 0;
                }
            }
	        return NOERROR;
        }

        public int GetInfo(out AMStreamInfo pInfo)
        {
            lock(m_csPinLock)
            {
	            pInfo = new AMStreamInfo();
                pInfo.dwFlags = AMStreamInfoFlags.None;

	            if (m_rtStart < m_rtStartAt)
	            {
                    pInfo.dwFlags = pInfo.dwFlags | AMStreamInfoFlags.Discarding;
	            }
	            if (m_rtStartAt != -1)
	            {
                    pInfo.dwFlags       = pInfo.dwFlags | AMStreamInfoFlags.StartDefined;
		            pInfo.tStart		= m_rtStartAt;
		            pInfo.dwStartCookie	= m_dwStartCookie;
	            }
	            if (m_rtStopAt != -1)
	            {
                    pInfo.dwFlags       = pInfo.dwFlags | AMStreamInfoFlags.StopDefined;
		            pInfo.tStop			= m_rtStopAt;
		            pInfo.dwStopCookie	= m_dwStopCookie;
	            }
                if (m_bShouldFlush) pInfo.dwFlags = pInfo.dwFlags |  AMStreamInfoFlags.StopSendExtra;
            }
	        return NOERROR;
        }

        #endregion
    
        #region IAMLatency Members

        public int  GetLatency(out long prtLatency)
        {
            return (m_Filter as VirtualSoundFilter).GetLatency(out prtLatency);
        }

        #endregion

      
    }
    

    [ComVisible(true)]
    [Guid("394BD6DB-30DC-4E67-8839-41303F45543D")]
    [AMovieSetup(Merit.Normal,AMovieSetup.CLSID_AudioInputDeviceCategory)]
    [PropPageSetup(typeof(AboutForm))]
    public class VirtualSoundFilter : BaseSourceFilter, IAMFilterMiscFlags
    {
        #region Constants

        private const int c_iDefaultChannels = 2;
        private const int c_nDefaultBitCount = 16;
        private const int BITS_PER_BYTE = 8;
        private const int MAX_AUDIO_FRAME_SIZE = 192000;
        private const int BYTE_PER_SAMPLE = 2;
        #endregion

        #region Variables

        protected int m_SamplePerSecond = 48000;
        private byte[] lastBuffer = null;
        private int checkBufferCount = 0;
        protected IntPtr m_hAudioSample = IntPtr.Zero;
        private uint m_audioSampleSize = 0;
        protected WaveFormatEx m_wfe = new WaveFormatEx();
        protected long lastSampleIndex = -1;

        MemoryMappedFile mmf = null;
        bool readAudioInfo = false;
        #endregion

        #region Constructor

        public VirtualSoundFilter()
            : base("SmarterBroadcast VSound")
        {
            AddPin(new VirtualSoundStream("Capture", this));
            //AddPin(new VirtualCamInputPin("Volume", this));
        }

        #endregion

        #region Overridden Methods

        protected override int OnInitializePins()
        {
            return NOERROR;
        }

        public override int Pause()
        {
            if (m_State == FilterState.Stopped)
            {
            }
            return base.Pause();
        }

        public override int Stop()
        {
            int hr = base.Stop();
            if (m_hAudioSample != IntPtr.Zero)
            {
                DeleteObject(m_hAudioSample);
                m_hAudioSample = IntPtr.Zero;
            }
            if (mmf != null)
            {
                mmf.Dispose();
                mmf = null;
            }
            return hr;
        }

        #endregion

        #region Methods

        public int CheckMediaType(AMMediaType pmt)
        {
            if (pmt == null) return E_POINTER;
            if (pmt.formatPtr == IntPtr.Zero) return VFW_E_INVALIDMEDIATYPE;
            if (pmt.majorType != MediaType.Audio)
            {
                return VFW_E_INVALIDMEDIATYPE;
            }
            //
            WaveFormatEx pvi = pmt;

            if (pvi == null)
            {
                return E_UNEXPECTED;
            }
            //
            return NOERROR;
        }

        public int SetMediaType(AMMediaType pmt)
        {
            lock (m_Lock)
            {
                if (m_hAudioSample != IntPtr.Zero)
                {
                    DeleteObject(m_hAudioSample);
                    m_hAudioSample = IntPtr.Zero;
                }
                WaveFormatEx wfe = pmt;
                //
                this.m_wfe.cbSize = wfe.cbSize;
                this.m_wfe.nAvgBytesPerSec = wfe.nAvgBytesPerSec;
                this.m_wfe.nBlockAlign = wfe.nBlockAlign;
                this.m_wfe.nChannels = wfe.nChannels;
                this.m_wfe.wBitsPerSample = wfe.wBitsPerSample;
                this.m_wfe.nSamplesPerSec = wfe.nSamplesPerSec;
                this.m_wfe.wFormatTag = wfe.wFormatTag;
            }
            return NOERROR;
        }

        public int GetMediaType(int iPosition, ref AMMediaType pMediaType)
        {
            if (iPosition < 0) return E_INVALIDARG;
            //
            this.GetAudioSampleInfo();
            if (iPosition > 0)
                return VFW_S_NO_MORE_ITEMS;
            //

            //

            //if (iPosition == 0)
            //{
            //    if (Pins.Count > 0 && Pins[0].CurrentMediaType.majorType == MediaType.Audio)
            //    {
            //        pMediaType.Set(Pins[0].CurrentMediaType);
            //        return NOERROR;
            //    }
            //}
            //else
            //{
            //    iPosition--;
            //    if (iPosition > 1)
            //        return VFW_S_NO_MORE_ITEMS;
            //}

            pMediaType.majorType = DirectShow.MediaType.Audio;
            pMediaType.formatType = DirectShow.FormatType.WaveEx;
            //
            WaveFormatEx wfe = new WaveFormatEx();
            wfe.cbSize = 0;
            wfe.nAvgBytesPerSec = m_SamplePerSecond * c_iDefaultChannels * c_nDefaultBitCount / BITS_PER_BYTE;
            wfe.nBlockAlign = (c_nDefaultBitCount * c_iDefaultChannels) / BITS_PER_BYTE;
            wfe.nChannels = c_iDefaultChannels;
            wfe.nSamplesPerSec = m_SamplePerSecond;
            wfe.wBitsPerSample = c_nDefaultBitCount;
            wfe.wFormatTag = 1;

            AMMediaType.SetFormat(ref pMediaType, ref wfe);
            pMediaType.fixedSizeSamples = true;
            pMediaType.sampleSize = wfe.nAvgBytesPerSec / 2;
            return NOERROR;
        }

        public int DecideBufferSize(ref IMemAllocatorImpl pAlloc, ref AllocatorProperties prop)
        {
            try
            {
                AllocatorProperties _actual = new AllocatorProperties();

                prop.cbBuffer = MAX_AUDIO_FRAME_SIZE;
                prop.cBuffers = 1;
                prop.cbAlign = 1;
                prop.cbPrefix = 0;
                int hr = pAlloc.SetProperties(prop, _actual);
                return hr;
            }
            catch(Exception ex)
            {
                return E_FAIL;
            }
        }
        public static void Log(string msg)
        {
            try
            {
                return;
                File.AppendAllText("e:\\1.txt", DateTime.Now.ToString("HH:mm:ss:ff:FFF") + "{" + msg + "}" + Environment.NewLine);
            }
            catch
            {

            }
        }
        private AudioData ReadAudioSamplefromPipe()
        {
            try
            {
                if (mmf == null)
                    mmf = MemoryMappedFile.OpenExisting("PlayoutXVsc");
                //
                Mutex mutex = Mutex.OpenExisting("PlayoutXVscmutex");
                while (true)
                {
                    bool mr = mutex.WaitOne();
                    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        var buffer = reader.ReadBytes((int)stream.Length);
                        AudioData ad = new AudioData();
                        ad.SampleIndex = BitConverter.ToInt64(buffer, 0);
                        if (ad.SampleIndex == lastSampleIndex)
                        {
                            mutex.ReleaseMutex();
                            Thread.Sleep(5);
                            if (this.checkBufferCount++ > 1000)
                                return null;
                            //
                            continue;
                        }
                        //
                        checkBufferCount = 0;
                        lastSampleIndex = ad.SampleIndex;
                        lastBuffer = buffer.ToArray();
                        //
                        ad.SampleRate = BitConverter.ToInt32(buffer, 8);
                        ad.SampleSize = BitConverter.ToInt32(buffer, 12);
                        //
                        ad.Sample = buffer.Skip(16).Take(ad.SampleSize).ToArray();
                        mutex.ReleaseMutex();
                        return ad;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void GetAudioSampleInfo()
        {
            if (readAudioInfo)
                return;
            //
            try
            {
                var ad = this.ReadAudioSamplefromPipe();
                if (ad != null && ad.SampleRate != 0)
                {
                    this.readAudioInfo = true;
                    m_SamplePerSecond = ad.SampleRate;
                }
            }
            catch (Exception)
            {
            }
        }

        private IntPtr CreateAudioSample()
        {
            IntPtr result = IntPtr.Zero;
            try
            {
                var ad = this.ReadAudioSamplefromPipe();
                m_audioSampleSize = (uint)ad.SampleSize;
                m_SamplePerSecond = ad.SampleRate;
                //
                result = Marshal.AllocHGlobal(ad.SampleSize);
                Marshal.Copy(ad.Sample, 0, result, ad.SampleSize);
                //
                return result;
            }
            catch (FileNotFoundException)
            {
                return IntPtr.Zero;
            }
        }

        public int FillBuffer(ref IMediaSampleImpl _sample)
        {
            try
            {
                m_hAudioSample = this.CreateAudioSample();
                //
                IntPtr _ptr;
                _sample.GetPointer(out _ptr);

                memcpy(_ptr, m_hAudioSample, new UIntPtr(this.m_audioSampleSize));

                //_sample.SetActualDataLength(_sample.GetSize());
                _sample.SetActualDataLength((int)this.m_audioSampleSize);
                _sample.SetSyncPoint(true);
                //
                Marshal.FreeHGlobal(m_hAudioSample);
                return NOERROR;
            }
            catch(Exception ex)
            {
                return E_FAIL;
            }
        }

        public int GetLatency(out long prtLatency)
        {
            AMMediaType mt = Pins[0].CurrentMediaType;
            //
            //prtLatency = 232199;
            prtLatency = (long)(UNITS * ((double)this.m_audioSampleSize /
                    (c_iDefaultChannels * m_SamplePerSecond * BYTE_PER_SAMPLE))) /1000;
            Log("Latency:" + prtLatency);
            return NOERROR;
            //
            if (mt.majorType == MediaType.Audio)
            {
                {
                    WaveFormatEx _pvi = mt;
                    if (_pvi != null)
                    {
                        prtLatency = _pvi.nSamplesPerSec;
                    }
                }
            }
            return NOERROR;
        }

        public int GetNumberOfCapabilities(out int iCount, out int iSize)
        {
            iCount = 0;
            AMMediaType mt = new AMMediaType();
            while (GetMediaType(iCount, ref mt) == S_OK) { mt.Free(); iCount++; };
            iSize = Marshal.SizeOf(typeof(AudioStreamConfigCaps));
            return NOERROR;
        }

        public int GetStreamCaps(int iIndex,out AMMediaType ppmt, out AudioStreamConfigCaps _caps)
        {
            ppmt = null;
            _caps = null;
            if (iIndex < 0) return E_INVALIDARG;

            ppmt = new AMMediaType();
            HRESULT hr = (HRESULT)GetMediaType(iIndex, ref ppmt);
            if (FAILED(hr)) return hr;
            if (hr == VFW_S_NO_MORE_ITEMS) return S_FALSE;
            hr = (HRESULT)GetDefaultCaps(iIndex, out _caps);
            return hr;
        }

        public int SuggestAllocatorProperties(AllocatorProperties pprop)
        {
            AllocatorProperties _properties = new AllocatorProperties();
            HRESULT hr = (HRESULT)GetAllocatorProperties(_properties);
            if (FAILED(hr)) return hr;
            if (pprop.cbBuffer != -1)
            {
                if (pprop.cbBuffer < _properties.cbBuffer) return E_FAIL;
            }
            if (pprop.cbAlign != -1 && pprop.cbAlign != _properties.cbAlign) return E_FAIL;
            if (pprop.cbPrefix != -1 && pprop.cbPrefix != _properties.cbPrefix) return E_FAIL;
            if (pprop.cBuffers != -1 && pprop.cBuffers < 1) return E_FAIL;
            return NOERROR;
        }

        public int GetAllocatorProperties(AllocatorProperties pprop)
        {
            AMMediaType mt = Pins[0].CurrentMediaType;
            if (mt.majorType == MediaType.Audio)
            {
                int lSize = mt.sampleSize;
                pprop.cbBuffer = lSize;
                pprop.cBuffers = 1;
                pprop.cbAlign = 1;
                pprop.cbPrefix = 0;

            }
            return NOERROR;
        }

        public int GetDefaultCaps(int nIndex, out AudioStreamConfigCaps _caps)
        {
            _caps = new AudioStreamConfigCaps();

            this.GetAudioSampleInfo();
            //
            _caps.guid = FormatType.WaveEx;
            _caps.MaximumChannels = c_iDefaultChannels;
            _caps.MinimumChannels = c_iDefaultChannels;
            _caps.ChannelsGranularity = c_iDefaultChannels; // doesn't matter
            _caps.MaximumSampleFrequency = m_SamplePerSecond;
            _caps.MinimumSampleFrequency = m_SamplePerSecond;
            _caps.SampleFrequencyGranularity = m_SamplePerSecond; // doesn't matter
            _caps.MaximumBitsPerSample = c_nDefaultBitCount;
            _caps.MinimumBitsPerSample = c_nDefaultBitCount;
            _caps.BitsPerSampleGranularity = c_nDefaultBitCount; // doesn't matter

            return NOERROR;
        }

        #endregion

        #region IAMFilterMiscFlags Members

        public int GetMiscFlags()
        {
            return (int)AMFilterMiscFlags.IsSource;
        }

        #endregion

        #region API

        [DllImport("msvcrt.dll", EntryPoint = "memcpy",CallingConvention = CallingConvention.Cdecl,SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        private enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);


        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        #endregion
    }


    [ComVisible(false)]
    public class VirtualCamInputPin : BaseInputPin, IAMAudioInputMixer
    {
        #region Constructor

        public VirtualCamInputPin(string _name, BaseSourceFilter _filter)
            : base(_name, _filter)
        {
            m_mt.majorType = Guid.Empty;
            GetMediaType(0, ref m_mt);
        }
        #endregion

        #region Overridden Methods
        public override int SetMediaType(AMMediaType mt)
        {
            if (mt == null) return E_POINTER;
            //
            return (m_Filter as VirtualCamFilter).SetMediaType(mt);
        }
        public override int CheckMediaType(AMMediaType pmt)
        {
            return (m_Filter as VirtualCamFilter).CheckMediaType(pmt);
        }
        public override int GetMediaType(int iPosition, ref AMMediaType pMediaType)
        {
            return (m_Filter as VirtualCamFilter).GetMediaType(iPosition, ref pMediaType);
        }
        #endregion


        public int put_Enable(bool fEnable)
        {
            throw new NotImplementedException();
        }

        public int get_Enable(out bool pfEnable)
        {
            pfEnable = true;
            return NOERROR;
        }

        public int put_Mono(bool fMono)
        {
            throw new NotImplementedException();
        }

        public int get_Mono(out bool pfMono)
        {
            throw new NotImplementedException();
        }

        public int put_MixLevel(double Level)
        {
            throw new NotImplementedException();
        }

        public int get_MixLevel(out double pLevel)
        {
            throw new NotImplementedException();
        }

        public int put_Pan(double Pan)
        {
            throw new NotImplementedException();
        }

        public int get_Pan(out double pPan)
        {
            throw new NotImplementedException();
        }

        public int put_Loudness(bool fLoudness)
        {
            throw new NotImplementedException();
        }

        public int get_Loudness(out bool pfLoudness)
        {
            throw new NotImplementedException();
        }

        public int put_Treble(double Treble)
        {
            throw new NotImplementedException();
        }

        public int get_Treble(out double pTreble)
        {
            throw new NotImplementedException();
        }

        public int get_TrebleRange(out double pRange)
        {
            throw new NotImplementedException();
        }

        public int put_Bass(double Bass)
        {
            throw new NotImplementedException();
        }

        public int get_Bass(out double pBass)
        {
            throw new NotImplementedException();
        }

        public int get_BassRange(out double pRange)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

}
