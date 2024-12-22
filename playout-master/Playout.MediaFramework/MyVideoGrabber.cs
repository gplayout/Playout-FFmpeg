//using System;
//using System.Collections.Generic;
//using System.Collections;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using VidGrabNoForm;

//using HWND = System.IntPtr;
//using System.Threading;
//using System.IO;

//namespace Playout.MediaFramework
//{
//    public enum enOpenType { Frame, Time, Image,Preview }
//    public class MyVideoGrabber
//    {
//        private long playerPausedTime = -1;
//        private VideoGrabberWPF Vg;
//        private VideoGrabberWPF VgDeck;
//        private VideoGrabberWPF VgPIP;
//        OnAudioPeakEventHandler audioPeakEvent;
//        OnPlayerStateChangedEventHandler playerStateChanged;
//        EventHandler playerEndStream;

//        public Action OnCheckOverlay_CurrentItem { get; set; }
//        public Action OnCheckOverlay_Global { get; set; }

//        //private VCamCOMLib.VCamSDK vcam = new VCamCOMLib.VCamSDK();
//        public MyVideoGrabber()
//        {
//            this.Vg = new VideoGrabberWPF();
//            this.VgDeck = new VideoGrabberWPF();
//            //Vg.VideoRendererExternal = TVideoRendererExternal.vre_BlackMagic_Decklink;
//            //Vg.AudioDeviceRendering = true;
//        }
//        public void test()
//        {

//            this.VgDeck.VideoSource = TVideoSource.vs_VideoCaptureDevice;
//            this.VgDeck.VideoDevice = this.VgDeck.VideoDeviceIndex("Datastead Virtual Camera 00");
//           //this.VgDeck.UseNearestVideoSize(320, 240, true);
//            this.VgDeck.VideoRendererExternal = TVideoRendererExternal.vre_BlackMagic_Decklink;
//            this.VgDeck.VideoRendererExternalIndex = this.VgDeck.VideoDeviceIndex(this.OutputVideoDeviceName);
//            this.VgDeck.StartPreview();

//            this.Vg.VideoSource = TVideoSource.vs_ScreenRecording;
//            this.Vg.VirtualVideoStreamControl = 0;
//            this.Vg.StartPreview();
//            //Thread.Sleep(5000);
//            //this.Vg.StopPreview();
//            //this.Vg.VirtualVideoStreamControl = 0;
//            //this.Vg.PlayerFileName = Program.AppPath+ @"\Resources\ResetCard.jpg";
//            //this.Vg.OpenPlayer();
//        }
//        public long PlayerFrameCount
//        {
//            get { return this.Vg.PlayerFrameCount; }
//        }
//        public string PlayerAudioCodec
//        {
//            get { return this.Vg.PlayerAudioCodec; }
//        }
//        public string PlayerVideoCodec
//        {
//            get { return this.Vg.PlayerVideoCodec; }
//        }
//        public long PlayerDuration
//        {
//            get { return this.Vg.PlayerDuration; }
//        }
//        public double PlayerFrameRate
//        {
//            get { return this.Vg.PlayerFrameRate; }
//        }
//        public long PlayerTimePosition
//        {
//            get { return this.Vg.PlayerTimePosition; }
//        }
//        public TCurrentState CurrentState
//        {
//            get { return this.Vg.CurrentState; }
//        }
//        public TPlayerState PlayerState
//        {
//            get { return this.Vg.PlayerState; }
//        }
//        public int VideoWidth
//        {
//            get { return this.Vg.VideoWidth; }
//        }
//        public int VideoHeight
//        {
//            get { return this.Vg.VideoHeight; }
//        }
//        public long PlayerFramePosition
//        {
//            get { return this.Vg.PlayerFramePosition; }
//            set
//            {
//                if (!this.Vg.InFrameProgressEvent)
//                    this.Vg.PlayerFramePosition = value;
//            }
//        }
//        public int OutputVideoDeviceIndex
//        {
//            get { return this.Vg.VideoRendererExternalIndex; }
//        }
//        public int OutputAudioDeviceIndex
//        {
//            get { return this.Vg.AudioRenderer; }
//        }
//        public bool IsPaused
//        {
//            get { return this.playerPausedTime != -1; }
//        }
//        public bool CheckOutputFlag { get; set; }
//        public bool ForPreview { get; set; }      
//        public string OutputVideoSize { get; set; }
//        public string OutputVideoDeviceName { get; set; }
//        public string OutputAudioDeviceName { get; set; }
//        public TFrameGrabber FrameGrabber
//        {
//            get { return this.Vg.FrameGrabber; }
//            set { this.Vg.FrameGrabber = value; }
//        }
//        public void SetOnPlayerOpenedEvent(EventHandler ev)
//        {
//            this.Vg.OnPlayerOpened += ev;
//        }
//        public void SetOnPlayerEndOfStreamEvent(EventHandler ev)
//        {
//            this.playerEndStream = ev;
//            this.Vg.OnPlayerEndOfStream += ev;
//        }
//        public void SetOnPlayerUpdateTrackbarPositionEvent(OnPlayerUpdateTrackbarPositionEventHandler ev)
//        {
//            this.Vg.OnPlayerUpdateTrackbarPosition += ev;
//        }
//        public void SetOnPlayerStateChanged(OnPlayerStateChangedEventHandler ev)
//        {
//            this.playerStateChanged = ev;
//            this.Vg.OnPlayerStateChanged += ev;
//        }
//        public void SetOnPreviewStarted(EventHandler ev)
//        {
//            this.Vg.OnPreviewStarted += ev;
//        }
//        public void SetOnFrameRefreshEvent(VideoGrabberWPF.On_WPF_FrameRefreshHandler ev)
//        {
//            this.Vg.OnFrameRefresh += ev;
//        }
//        public void SetOnAudioPeak(OnAudioPeakEventHandler ev)
//        {
//            this.audioPeakEvent = ev;
//            this.Vg.OnAudioPeak += ev;
//        }
//        public string[] GetVideoDeviceNames()
//        {
//            string[] names = Vg.VideoDevices.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
//            return names;
//        }
//        public string[] GetVideoSizes()
//        {
//            string[] sizes = new string[] {"Auto",
//                    "88x60",
//                    "88x72",
//                    "180x120",
//                    "180x144",
//                    "320x240",
//                    "352x28",
//                    "640x480",
//                    "720x480",
//                    "720x576",
//                    "768x576",
//                    "1024x768",
//                    "1920x1080"};
//            return sizes;
//        }
//        public string[] GetAudioDeviceNames()
//        {
//            string[] names = Vg.AudioDevices.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
//            return names;
//        }
//        private int GetAudioDeviceIndex(string deviceName)
//        {
//            if (String.IsNullOrEmpty(deviceName))
//                return -1;
//            //
//            string[] ds = this.GetAudioDeviceNames();
//            for (int i = 0; i < ds.Length; i++)
//            {
//                if (ds[i] == deviceName)
//                    return i;
//            }
//            return -1;
//        }
//        public void MuteAudio()
//        {
//            this.Vg.MuteAudioRendering = true;
//        }
//        public void PlayerBackward()
//        {
//            try
//            {
//                this.Vg.RunPlayerBackwards();
//            }
//            catch { }
//        }
//        public void PlayerFF()
//        {
//            try
//            {
//                this.Vg.FastForwardPlayer();
//            }
//            catch { }
//        }
//        public void PlayerFR()
//        {
//            try
//            {
//                this.Vg.RewindPlayer();
//            }
//            catch { }
//        }
//        private void CheckOutputSettings()
//        {
//            if(this.ForPreview)
//            {
//                this.Vg.VideoRendererExternal = TVideoRendererExternal.vre_None;
//                this.Vg.AudioDeviceRendering = false;
//                return;
//            }
//            this.Vg.AudioPeakEvent = true;
//            //Set Video Size
//            int w = 0, h = 0;
//            if (!String.IsNullOrEmpty(this.OutputVideoSize))
//            {
//                string[] arr = this.OutputVideoSize.Split(new char[] { 'x' });
//                //
//                if (!this.OutputVideoSize.Equals("Auto", StringComparison.InvariantCultureIgnoreCase) && arr.Length == 2)
//                {
//                    int.TryParse(arr[0], out w);
//                    int.TryParse(arr[1], out h);
//                }
//            }
//            //
//            if (String.IsNullOrEmpty(this.OutputVideoDeviceName))
//            {
//                this.Vg.VideoRendererExternalIndex = -1;
//                this.VgDeck.VideoRendererExternalIndex = -1;
//            }
//            else if (OutputVideoDeviceName.Equals("Datastead Virtual Camera 00"))
//            {
//                this.VgDeck.VideoRendererExternalIndex = -1;
//                this.Vg.VirtualVideoStreamControl = 0;
//                this.Vg.UseNearestVideoSize(w, h, true);
//            }
//            else
//            {
//                this.Vg.VideoRendererExternalIndex = -1;
//                this.Vg.VirtualVideoStreamControl = 0;
//                this.VgDeck.VideoSource = TVideoSource.vs_VideoCaptureDevice;
//                this.VgDeck.VideoDevice = this.VgDeck.VideoDeviceIndex("Datastead Virtual Camera 00");
//                if (w != 0 && h != 0)
//                    this.VgDeck.UseNearestVideoSize(w, h, true);
//                this.VgDeck.FrameRate = 30;
//                this.VgDeck.VideoRendererExternal = TVideoRendererExternal.vre_BlackMagic_Decklink;
//                this.VgDeck.VideoRendererExternalIndex = this.VgDeck.VideoDeviceIndex(this.OutputVideoDeviceName);
//                this.VgDeck.StopPreview();
//                this.VgDeck.StartPreview();
//            }
//            //
//            if (!String.IsNullOrEmpty(this.OutputAudioDeviceName))
//            {
//                this.Vg.AudioRenderer = this.GetAudioDeviceIndex(this.OutputAudioDeviceName);
//                this.Vg.AudioDeviceRendering = true;
//            }
//            else
//            {
//                this.Vg.AudioRenderer = -1;
//                this.Vg.AudioDeviceRendering = false;
//            }
//            //
//            this.CheckOutputFlag = false;
//        }
//        public void OpenPlayer(enMediaType mediaType, string file_url, enOpenType openType,string devName, 
//            long startFrame = 0, long startSecond = 0, long stopSecond = 0)
//        {
//            try
//            {
//                if (this.timForStopPreview != null)
//                    this.timForStopPreview.Stop();
//                //
//                if (this.CheckOutputFlag)
//                    this.CheckOutputSettings();
//                //
//                //this.ClosePlayer();
//                //Check Pause
//                stopSecond = (long)(stopSecond * 10e6);
//                startSecond = (long)(startSecond * 10e6);
//                if (playerPausedTime >= 0)
//                {
//                    startSecond = this.playerPausedTime;
//                    this.playerPausedTime = -1;
//                }
//                if (this.Vg.PlayerFileName == file_url && (this.PlayerState == TPlayerState.ps_PlayingBackward ||
//                    this.PlayerState == TPlayerState.ps_FastRewinding || this.PlayerState == TPlayerState.ps_FastForwarding))
//                {
//                    if (mediaType == enMediaType.Device || mediaType == enMediaType.Url)
//                        this.Vg.ResumePreview();
//                    else
//                        this.Vg.RunPlayer();
//                    return;
//                }
//                //
//                switch (mediaType)
//                {
//                    case enMediaType.ImageFile:
//                    case enMediaType.VideoFile:
//                        this.Vg.PlayerFileName = file_url;
//                        //
//                        switch (openType)
//                        {
//                            case enOpenType.Frame:
//                                this.Vg.OpenPlayerAtFramePositions(startFrame, 0, false, true);
//                                break;
//                            case enOpenType.Time:
//                                this.Vg.OpenPlayerAtTimePositions(startSecond, stopSecond, false, true);
//                                break;
//                            case enOpenType.Image:
//                            case enOpenType.Preview:
//                                this.Vg.OpenPlayer();
//                                break;
//                        }
//                        break;
//                    case enMediaType.Device:
//                        this.Vg.PlayerFileName = "";
//                        this.Vg.VideoDevice = this.Vg.VideoDeviceIndex(devName);
//                        this.Vg.VideoSource = TVideoSource.vs_VideoCaptureDevice;
//                        break;
//                    case enMediaType.DVD:
//                        bool result = this.Vg.OpenDVD();
//                        this.Vg.OpenPlayer();
//                        break;
//                    case enMediaType.Url:
//                        this.Vg.PlayerFileName = "";
//                        this.Vg.IPCameraURL = file_url;
//                        this.Vg.VideoSource = TVideoSource.vs_IPCamera;                      
//                        break;
//                    default:
//                        return;
//                }
//                //
//                if (mediaType == enMediaType.Device || mediaType == enMediaType.Url)
//                {
//                    this.Vg.StartPreview();
//                    if (stopSecond != 0)
//                        this.SetTimerForStop(stopSecond);
//                }
//                //
//                if (this.OnCheckOverlay_CurrentItem != null)
//                    this.OnCheckOverlay_CurrentItem();
//                //
//                if (this.OnCheckOverlay_Global != null)
//                    this.OnCheckOverlay_Global();
//            }
//            catch(Exception ex)
//            {
                
//            }
//        }
//        public void ClosePlayer()
//        {
//            //if (this.Vg.AudioPeakEvent)
//            //{
//            //    this.Vg.AudioPeakEvent = false;
//            //    Thread.Sleep(500);
//            //    if (this.audioPeakEvent != null)
//            //        this.audioPeakEvent(null, new TOnAudioPeakEventArgs() { left_DB = 0, right_DB = 0 });
//            //}
//            this.Vg.Stop();
//            Thread.Sleep(1);
//            if (this.VgPIP != null)
//            {
//                this.VgPIP.ClosePlayer();
//            }
//            return;
//            //
//            if (this.CurrentState == TCurrentState.cs_Preview)
//            {
//                this.StopPreview();
//            }
//            else if (this.CurrentState == TCurrentState.cs_Playback)
//            {
//                //this.vcam.StopPlay();
//                if (this.Vg.PlayerState == TPlayerState.ps_Playing || this.Vg.PlayerState == TPlayerState.ps_Paused)
//                {
//                    this.Vg.StopPlayer();
//                }
//                if (this.Vg.PlayerState != TPlayerState.ps_Closed)
//                    Vg.ClosePlayer();
                
//            }
//        }
//        public void StopPreview()
//        {
//            this.Vg.StopPreview();
//            if (this.playerStateChanged != null)
//                this.playerStateChanged(null, new TOnPlayerStateChangedEventArgs());
//        }
//        public void PausePlayer(enMediaType mediaType)
//        {
//            playerPausedTime = this.Vg.GetPlayerTimePosition();
//            switch (mediaType)
//            {
//                case enMediaType.ImageFile:
//                case enMediaType.VideoFile:
//                case enMediaType.DVD:
//                    this.Vg.PausePlayer();
//                    break;
//                case enMediaType.Device:
//                case enMediaType.Url:
//                    this.Vg.PausePreview();
//                    break;
//                default:
//                    return;
//            }
//            if (this.VgPIP != null)
//                this.VgPIP.PausePlayer();
//        }
//        public void NotifyPlayerTrackbarAction(TTrackbarAction action)
//        {
//            this.Vg.NotifyPlayerTrackbarAction(action);
//        }
//        public bool GetFileInfo(string AVIFile, out long Duration, out long FrameCount, out int _VideoWidth, out int _VideoHeight,
//            out double VideoFrameRateFps, out int AvgBitRate, out int AudioChannels, out int AudioSamplesPerSec,
//            out int AudioBitsPerSample, out string VideoCodec, out string AudioCodec)
//        {
//            bool result = this.Vg.AVIInfo(AVIFile, out Duration, out FrameCount, out _VideoWidth, out _VideoHeight,
//                out VideoFrameRateFps, out AvgBitRate, out AudioChannels, out AudioSamplesPerSec, out AudioBitsPerSample,
//                out VideoCodec, out AudioCodec);
//            return result;
//        }
//        public bool GetDvdInfo(out long Duration, out long FrameCount, out int _VideoWidth, out int _VideoHeight,out double FrameRate)
//        {
//            Duration = 0;
//            FrameCount = 0; 
//            _VideoWidth = 0;
//            _VideoHeight = 0;
//            FrameRate = 0;
//            //
//            string rootdir = "";
//            System.IO.DriveInfo[] allDrives = System.IO.DriveInfo.GetDrives();
//            foreach (System.IO.DriveInfo d in allDrives)
//            {
//                if (d.IsReady && d.DriveType == System.IO.DriveType.CDRom)
//                {
//                    rootdir = d.VolumeLabel;
//                    break;
//                }
//            }
//            if (String.IsNullOrEmpty(rootdir))
//                return false;
//            //
//            Duration = (long)this.Vg.DVDInfo(rootdir,TDVDInfoType.dvd_TotalDuration, 0);
//            int titles = (int)this.Vg.DVDInfo(rootdir, TDVDInfoType.dvd_NumberOfTitles, 0);
//            if (titles > 1)
//            {
//                FrameRate = this.Vg.DVDInfo(rootdir, TDVDInfoType.dvd_TitleFrameRate, 1);
//                _VideoWidth = (int)this.Vg.DVDInfo(rootdir, TDVDInfoType.dvd_SourceResolutionX, 1);
//                _VideoHeight = (int)this.Vg.DVDInfo(rootdir, TDVDInfoType.dvd_SourceResolutionY, 1);
//                //
//                for (int i = 1; i <= titles; i++)
//                {
//                    FrameCount += (long)this.Vg.DVDInfo(rootdir, TDVDInfoType.dvd_TitleFrameCount, i);
//                }
//            }
//            return true;
//        }
//        public bool GetFileHeightWidth(string AVIFile, out int _VideoWidth, out int _VideoHeight)
//        {
//            long Duration, FrameCount;
//            double VideoFrameRateFps;
//            int AvgBitRate, AudioChannels, AudioSamplesPerSec, AudioBitsPerSample;
//            string VideoCodec, AudioCodec;
//            bool result = this.Vg.AVIInfo(AVIFile, out Duration, out FrameCount, out _VideoWidth, out _VideoHeight,
//                out VideoFrameRateFps, out AvgBitRate, out AudioChannels, out AudioSamplesPerSec, out AudioBitsPerSample,
//                out VideoCodec, out AudioCodec);
//            return result;
//        }
//        public bool GetFileDuration(string AVIFile, out long Duration)
//        {
//            long FrameCount=0;
//            bool result = this.Vg.AVIDuration(AVIFile, out Duration, out FrameCount);
//            return result;
//        }
//        public void SetPlayerTimePosition(long pos)
//        {
//            this.Vg.SetPlayerTimePosition(pos);
//        }
//        public void SetTextOverlay(int index, bool enabled, string fontName, float fontSize, int color,
//            int posLeft, int posTop, string text, bool shadow,int shadowColor, bool transparency,int backcolor,string textAlign)
//        {
//            try
//            {
//                //if (!String.IsNullOrEmpty(this.OutputVideoSize) && this.OutputVideoSize != "Auto" && !String.IsNullOrEmpty(this.Vg.PlayerFileName))
//                //{
//                //    var arr = this.OutputVideoSize.Split(new char[] { 'x' });
//                //    float ratio = int.Parse(arr[0]) / int.Parse(arr[1]);
//                //    int _w, _h;
//                //    this.GetFileHeightWidth(this.Vg.PlayerFileName, out _w, out _h);
//                //    float _ratio = _w / _h;
//                //    fontSize = fontSize * (_ratio / ratio);
//                //}
//                this.Vg.SetTextOverlay_Enabled(index, enabled);

//                var fm = new System.Drawing.FontFamily(fontName);
//                System.Drawing.Font fd = new System.Drawing.Font(fm, (float)fontSize);
//                this.Vg.SetTextOverlay_Font(index, fd.ToHfont());

//                this.Vg.SetTextOverlay_FontColor(index, color);
//                this.Vg.SetTextOverlay_Left(index, posLeft);
//                this.Vg.SetTextOverlay_String(index, text);
//                this.Vg.SetTextOverlay_Top(index, posTop);
//                this.Vg.SetTextOverlay_HighResFont(index, true);
//                this.Vg.AdjustOverlayAspectRatio = true;
//                this.Vg.SetTextOverlay_Transparent(index, transparency);
//                this.Vg.SetTextOverlay_Shadow(index, shadow);
//                this.Vg.SetTextOverlay_ShadowColor(index, shadowColor);
//                this.Vg.SetTextOverlay_BkColor(index, backcolor);
//                //
//                TTextOverlayAlign ta = TTextOverlayAlign.tf_Left;
//                if (textAlign == "Center")
//                    ta = TTextOverlayAlign.tf_Center;
//                else if (textAlign == "Right")
//                    ta = TTextOverlayAlign.tf_Right;
//                //
//                this.Vg.SetTextOverlay_Align(index,ta);
//            }
//            catch (Exception ex)
//            {
//            }
//        }
//        public void SetCrawlOverlay(int index, bool enabled, string fontName, float fontSize, int color,
//            int posLeft, int posTop, string text, bool shadow, int shadowColor, bool transparency, int backcolor, string textAlign,
//            bool scrolling, int scrollingSpeed,bool readFromFile,string filePath)
//        {
//            try
//            {
//                //if (!String.IsNullOrEmpty(this.OutputVideoSize) && this.OutputVideoSize != "Auto" && !String.IsNullOrEmpty(this.Vg.PlayerFileName))
//                //{
//                //    var arr = this.OutputVideoSize.Split(new char[] { 'x' });
//                //    float ratio = int.Parse(arr[0]) / int.Parse(arr[1]);
//                //    int _w, _h;
//                //    this.GetFileHeightWidth(this.Vg.PlayerFileName, out _w, out _h);
//                //    float _ratio = _w / _h;
//                //    fontSize = fontSize * (_ratio / ratio);
//                //}
//                this.Vg.SetTextOverlay_Enabled(index, enabled);

//                var fm = new System.Drawing.FontFamily(fontName);
//                System.Drawing.Font fd = new System.Drawing.Font(fm, (float)fontSize);
//                this.Vg.SetTextOverlay_Font(index, fd.ToHfont());

//                this.Vg.SetTextOverlay_FontColor(index, color);
//                this.Vg.SetTextOverlay_Left(index, posLeft);
//                this.Vg.SetTextOverlay_Top(index, posTop);
//                this.Vg.SetTextOverlay_HighResFont(index, true);
//                this.Vg.AdjustOverlayAspectRatio = true;
//                this.Vg.SetTextOverlay_Transparent(index, transparency);
//                this.Vg.SetTextOverlay_Shadow(index, shadow);
//                this.Vg.SetTextOverlay_ShadowColor(index, shadowColor);
//                this.Vg.SetTextOverlay_BkColor(index, backcolor);

//                this.Vg.SetTextOverlay_Scrolling(index, scrolling);
//                this.Vg.SetTextOverlay_ScrollingSpeed(index, scrollingSpeed);
//                if (readFromFile && File.Exists(filePath))
//                    text = File.ReadAllText(filePath);
//                //
//                this.Vg.SetTextOverlay_String(index, text);
//                //
//                TTextOverlayAlign ta = TTextOverlayAlign.tf_Left;
//                if (textAlign == "Center")
//                    ta = TTextOverlayAlign.tf_Center;
//                else if (textAlign == "Right")
//                    ta = TTextOverlayAlign.tf_Right;
//                //
//                this.Vg.SetTextOverlay_Align(index, ta);
//            }
//            catch (Exception ex)
//            {
//            }
//        }
//        public void SetTrialTextOverlay(bool enable,int color)
//        {
//            try
//            {
//                int index = 10;
//                this.Vg.SetTextOverlay_Enabled(index, enable);

//                var fm = new System.Drawing.FontFamily("Arial");
//                System.Drawing.Font fd = new System.Drawing.Font(fm, 15);
//                this.Vg.SetTextOverlay_Font(index, fd.ToHfont());

//                this.Vg.SetTextOverlay_FontColor(index, color);
//                //this.Vg.SetTextOverlay_Left(index, posLeft);
//                this.Vg.SetTextOverlay_Align(index, TTextOverlayAlign.tf_Center);
//                this.Vg.SetTextOverlay_VideoAlignment(index, TVideoAlignment.oa_Center);
//                this.Vg.SetTextOverlay_String(index, "Playout Watermark");
//                //this.Vg.SetTextOverlay_Top(index, posTop);
//                this.Vg.SetTextOverlay_HighResFont(index, true);
//                this.Vg.AdjustOverlayAspectRatio = false;
//                this.Vg.SetTextOverlay_Transparent(index, true);
//                this.Vg.SetTextOverlay_Shadow(index, false);
//            }
//            catch (Exception ex)
//            {
//            }
//        }
//        public void SetImageOverlay(int index, bool enabled, string filePath, int width, int height,
//            int posLeft, int posTop, int alphBlend,bool chromaKey,int chromaColor,int chromaLeeway)
//        {
//            try
//            {
//                if (String.IsNullOrEmpty(filePath))
//                    enabled = false;
//                //
//                //if (!String.IsNullOrEmpty(this.OutputVideoSize) && this.OutputVideoSize != "Auto" && !String.IsNullOrEmpty(this.Vg.PlayerFileName))
//                //{
//                //    var arr = this.OutputVideoSize.Split(new char[] { 'x' });
//                //    float ratio = int.Parse(arr[0]) / int.Parse(arr[1]);
//                //    int _w, _h;
//                //    this.GetFileHeightWidth(this.Vg.PlayerFileName, out _w, out _h);
//                //    float _ratio = _w / _h;
//                //    width = (int)(width * (ratio / ratio));
//                //    height = (int)(height * (ratio / ratio));
//                //}

//                //
//                this.Vg.SetImageOverlay_Enabled(index, enabled);

//                this.Vg.SetImageOverlayFromImageFile2(index, filePath);
//                this.Vg.SetImageOverlay_LeftLocation(index, posLeft);
//                this.Vg.SetImageOverlay_TopLocation(index, posTop);
//                this.Vg.SetImageOverlay_Width(index, width);
//                this.Vg.SetImageOverlay_Height(index, height);
//                this.Vg.SetImageOverlay_Transparent(index, true);
//                this.Vg.SetImageOverlay_AlphaBlend(index, true);
//                this.Vg.SetImageOverlay_AlphaBlendValue(index, alphBlend);
//                //
//                this.Vg.SetImageOverlay_ChromaKey(index, chromaKey);
//                this.Vg.SetImageOverlay_ChromaKeyRGBColor(index, chromaColor);
//                this.Vg.SetImageOverlay_ChromaKeyLeewayPercent(index, chromaLeeway);
//            }
//            catch (Exception ex) { }
//        }
//        public void SetVideoOverlay( bool enabled, string filePath, int width, int height,int posLeft, int posTop)
//        {
//            try
//            {
//                if (String.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
//                    enabled = false;
//                //
//                if (this.VgPIP == null)
//                {
//                    this.VgPIP = new VideoGrabberWPF();
//                    this.VgPIP.VideoRenderer = TVideoRenderer.vr_None;
//                }
//                if (enabled)
//                {
//                    if (this.VgPIP.PlayerState == TPlayerState.ps_Paused && filePath == this.VgPIP.PlayerFileName)
//                    {
//                        this.VgPIP.RunPlayer();
//                    }
//                    else
//                    {
//                        this.Vg.Mixer_SetupPIPFromSource(this.VgPIP.UniqueID, 0, 0, 0, 0, enabled, posLeft, posTop, width, height, false);
//                        this.VgPIP.PlayerFileName = filePath;
//                        this.VgPIP.OpenPlayer();
//                    }
//                }
//                else
//                {
//                    this.VgPIP.ClosePlayer();
//                }
//            }
//            catch { }
//        }
//        public System.Windows.Media.Imaging.BitmapSource CaptureLastFrame()
//        {
//            var bs = this.Vg.GetLastFrameAsBitmapSource(false);
//            return bs;
//        }
//        System.Timers.Timer timForStopPreview = null;
//        private void SetTimerForStop(long intervalSec)
//        {
//            this.timForStopPreview = new System.Timers.Timer(intervalSec * 1000);
//            this.timForStopPreview.Elapsed += (e, s) => {
//                this.timForStopPreview.Stop();
//                this.StopPreview();
//                if (this.playerEndStream != null)
//                    this.playerEndStream(null, null);
//            };
//            this.timForStopPreview.Start();
//        }
//    }
//}
