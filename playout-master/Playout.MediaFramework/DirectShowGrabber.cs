using DirectShowLib;
using Playout.DirectShow.Controls;
using Playout.DirectShow.MediaPlayers;
using Playout.DirectShow.Overlays;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Playout.Log;

namespace Playout.MediaFramework
{
    public enum DirectShowGrabberType { Uri = 1, Dvd = 2, Card = 3 }
    public class DirectShowGrabber
    {
        const string NoDevice = "No Device";
        RoutedEventHandler playerEndStream;
        System.Timers.Timer timForStopPreview = null;
        long playerPausedTime = 0;

        MediaPlayerBase _player;
        public MediaUriPlayer CurrentPlayerAsUri
        {
            get { return this.CurrentPlayer as MediaUriPlayer; }
        }
        
        public MediaPlayerBase CurrentPlayer
        {
            get {return this._player; }
            private set { this._player = value; }
        }
        public bool IsPlaying
        {
            get { return this.CurrentPlayer.IsPlaying; }
        }
        public DirectShowGrabberType CurrentType { get; private set; }
        public Action OnCheckOverlay_CurrentItem { get; set; }
        public Action OnCheckOverlay_Global { get; set; }
        public Action SetPIPOverlay { get; set; }
        public bool ForPreview { get; set; }
        public bool CheckOutputFlag { get; set; }
        public string OutputVideoFormat { get; set; }
        
        public string OutputVideoDeviceName { get; set; }
        public string OutputAudioDeviceName { get; set; }
        //
        public long PlayerTimePosition
        {
            get 
            {
                if (this.CurrentPlayerAsUri == null)
                    return 0;
                else
                    return this.CurrentPlayerAsUri.MediaPositionPlusOffset;
            }
            set
            {
                if (this.CurrentPlayerAsUri == null)
                    return;
                this.CurrentPlayerAsUri.MediaPosition = value; 
            }
        }
        public long PlayerTimePositionSecond
        {
            get
            {
                var result = (long)(Math.Round(this.PlayerTimePosition / 10e6));
                int a = 0;
                int b = 0;
                if(a== b)
                {
                    return result;
                }
                return result;
            }
        }

        public DirectShowGrabber()
        {
            this.ChangeElement(DirectShowGrabberType.Uri);
            this.CurrentPlayer.Overlays = new SortedList<int, BaseOverlay>();
        }

        public void ChangeElement(DirectShowGrabberType type)
        {
            if (this.CurrentType == type && this.CurrentPlayer != null)
                return;
            //
            switch (type)
            {
                case DirectShowGrabberType.Uri:
                    this.CurrentPlayer = new MediaUriPlayer();
                    break;
                default:
                    break;
            }
            this.CurrentType = type;
        }
        public void SetOnPlayerEndOfStreamEvent(RoutedEventHandler ev,Action endv)
        {
            this.playerEndStream = ev;
            this.CurrentPlayer.MediaEnded += endv;
        }
        public void SetOnPlayerStateChanged(RoutedEventHandler ev)
        {
            
        }
        public void StopPlayer()
        {
            try
            {
                this.UnsetTimerForStop();
                //
                if (this.CurrentPlayer != null)
                {
                    //this.CurrentPlayer.Pause();
                    //Thread.Sleep(100);
                    this.CurrentPlayer.Stop();
                }

                this.PlayerTimePosition = 0;
                this.playerPausedTime = 0;
                Playout.DirectShow.Overlays.CrawlOverlay.ScrollGlobalReset = true;
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        public void PausePlayer()
        {
            playerPausedTime = this.PlayerTimePosition;
            this.UnsetTimerForStop();

            this.CurrentPlayer.Pause();
        }

        public void OpenPlayer(enMediaType mediaType,string file_url, string devName,string devInputFormat, string showupEffect,
            long startSecond = 0,long stopSecond = 0)
        {
            try
            {
                this.CurrentPlayer.SourceMediaType = mediaType;
                this.CurrentPlayer.showupEffect = showupEffect;
                //
                this.UnsetTimerForStop();
                //
                this.CheckOutputSettings();
                //
                switch(mediaType)
                {
                    case enMediaType.VideoFile:
                    case enMediaType.ImageFile:
                        //Check Pause
                        stopSecond = (long)(stopSecond * 10e6);
                        startSecond = (long)(startSecond * 10e6);
                        this.CurrentPlayerAsUri.SeekMiliSecond = (int)(startSecond / 10000);
                        if (playerPausedTime > 0)
                        {
                            Playout.DirectShow.Overlays.CrawlOverlay.ScrollMediaReset = false;
                        }
                        else
                        {
                            this.CurrentPlayerAsUri.Source = file_url;
                            this.CurrentPlayerAsUri.PlayerTimePositionOffset = startSecond;
                        }
                        //
                        long dur = 0;
                        bool resultDur = this.GetFileDuration(file_url, ref dur);
                        this.CurrentPlayerAsUri.Duration = dur;
                        //
                        
                        break;
                    case enMediaType.Device:
                        string devVideoName2 = DecklinkWrapper.GetInputDirectShowName(devName, true);
                        int formatIndex = DecklinkWrapper.GetInputVideoCapabilityIndex(devName, devInputFormat);
                        this.CurrentPlayerAsUri.Source = devVideoName2 + (formatIndex == -1 ? "" : "@" + formatIndex);
                        break;
                    case enMediaType.Url:
                        this.CurrentPlayerAsUri.Source = file_url;
                        break;
                }
                //
                //this.CurrentPlayerAsUri.OnSourceChanged(new DependencyPropertyChangedEventArgs());
                //Set PIP
                if (this.SetPIPOverlay != null)
                    this.SetPIPOverlay();
                //
                this.CurrentPlayer.Play();
                //
                if (stopSecond != 0)
                    this.SetTimerForStop(stopSecond - startSecond, this.playerPausedTime - startSecond);
                //
                this.playerPausedTime = 0;
                //
                if (this.OnCheckOverlay_CurrentItem != null)
                    this.OnCheckOverlay_CurrentItem();
                //
                if (this.OnCheckOverlay_Global != null)
                    this.OnCheckOverlay_Global();
                //
                this.CheckOutputFlag = false;
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        private void SetTimerForStop(long intervalMicroSec, long pausedTime = 0)
        {
            this.UnsetTimerForStop();
            var interval = intervalMicroSec - (pausedTime > 0 ? pausedTime : 0);
            this.timForStopPreview = new System.Timers.Timer(interval / 10e3);
            this.timForStopPreview.Elapsed += this.TimerForStop_Elapsed;
            this.timForStopPreview.AutoReset = false;
            this.timForStopPreview.Start();
        }
        private void UnsetTimerForStop()
        {
            if (this.timForStopPreview != null)
            {
                this.timForStopPreview.Stop();
                this.timForStopPreview.Elapsed -= this.TimerForStop_Elapsed;
                this.timForStopPreview.Dispose();
                this.timForStopPreview = null;
            }
        }

        private void TimerForStop_Elapsed(object sender,EventArgs e)
        {
            this.CurrentPlayerAsUri.UIDispatcher.Invoke(() =>
            {
                try
                {
                    this.StopPlayer();
                    if (this.playerEndStream != null)
                        this.playerEndStream(null, null);
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            });
        }
        private void CheckOutputSettings()
        {
            if (this.CheckOutputFlag)
            {
                this.CurrentPlayer.OutputVideoDeviceName = DecklinkWrapper.GetOutputDirectShowName(this.OutputVideoDeviceName, true);
                this.CurrentPlayer.OutputAudioDeviceName = DecklinkWrapper.GetOutputDirectShowName(this.OutputAudioDeviceName, false);
                if (this.CurrentPlayerAsUri != null)
                {
                    //
                    MediaPlayerBase.OutputVideoFormat = this.OutputVideoFormat;
                    var pars = this.OutputVideoFormatPars;
                    MediaPlayerBase.OutputVideoFormat_FrameRateScale = pars.FrameRateScale;
                    MediaPlayerBase.OutputVideoFormat_FrameRateDuration = pars.FrameRateDuration;
                    MediaPlayerBase.OutputVideoFormat_Width = pars.Width;
                    MediaPlayerBase.OutputVideoFormat_Height = pars.Height;
                    this.CurrentPlayerAsUri.AudioRenderer = this.CurrentPlayer.OutputAudioDeviceName;
                }
                
            }
        }

        public string[] GetOutputVideoDeviceNames()
        {
            List<string> names = new List<string>();
            var devs = DecklinkWrapper.DecklinkDeviceNames;
            if (devs != null)
                names.AddRange(devs);

            names.AddRange(MultimediaUtil.VideoOutputNames.Where(m => !m.Contains("Decklink")));
            
            names.Add(DirectShowGrabber.NoDevice);
            return names.ToArray();
        }
        public string[] GetInputVideoDeviceNames()
        {
            List<string> names = new List<string>();
            var devs = DecklinkWrapper.DecklinkDeviceNames;
            if (devs != null)
                names.AddRange(devs);
            //
            names.AddRange(MultimediaUtil.VideoInputNames.Where(m => !m.Contains("Decklink") && !m.Contains("Blackmagic")));
            return names.ToArray();
        }
        public OutputVideoFormatPars OutputVideoFormatPars
        {
            get
            {
                var pars = new OutputVideoFormatPars();
                try
                {
                    if (String.IsNullOrEmpty(this.OutputVideoFormat) || this.OutputVideoFormat == "Auto")
                        return pars;
                    //
                    string[] arr = this.OutputVideoFormat.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //
                    string[] wha = arr[1].Split(new char[] { '*' });
                    pars.Width = int.Parse(wha[0]);
                    pars.Height = int.Parse(wha[1]);
                    //
                    wha = arr[2].Split(new char[] { '/' });
                    pars.FrameRateScale = int.Parse(wha[0]);
                    pars.FrameRateDuration = int.Parse(wha[1]);
                    //
                    return pars;
                }
                catch
                {
                    return pars;
                }
            }
        }
        public string[] GetOutputVideoFormats(string devName)
        {
            //stream << (char*)modeName << " " << modeWidth << "*" << modeHeight << " " << frameRate << ",";
            string[] deckFormats = DecklinkWrapper.GetVideoFormats(devName, false);
            //
            List<string> formats = new List<string>();
            formats.Add("Auto");
            //
            if (deckFormats == null || deckFormats.Length == 0)
            {
                formats.AddRange(new string[] {
                    "PAL 720*576 25000/1000",
                    "NTSC 720*480 30000/1001",
                    "NTSC 720*480 30000/1000",
                    "HD720 1280*720 25000/1000",
                    "HD720 1280*720 30000/1001",
                    "HD720 1280*720 30000/1000",
                    "HD1080 1920*1080 24000/1001",
                    "HD1080 1920*1080 24000/1000",
                    "HD1080 1920*1080 25000/1000",
                    "HD1080 1920*1080 30000/1001",
                    "HD1080 1920*1080 30000/1000",
                    //"HD2160 4096*2160 24000/1000",
                    //"HD2160 4096*2160 25000/1000",
                    //"HD2160 4096*2160 30000/1001",
                    //"HD2160 4096*2160 30000/1000",
                });
            }
            else
            {
                formats.AddRange(deckFormats);
            }
            //
            return formats.ToArray();
        }
        public string[] GetInputVideoFormats(string devName)
        {
            return DecklinkWrapper.GetVideoFormats(devName, true);
        }
        public string[] GetAudioDeviceNames()
        {
            List<string> names = new List<string>();
            var devs = DecklinkWrapper.DecklinkDeviceNames;
            if (devs != null)
                names.AddRange(devs);
            //
            names.AddRange(MultimediaUtil.AudioRendererNames.Where(m => !m.Contains("Decklink")));
            names.Add(DirectShowGrabber.NoDevice);
            return names.ToArray();
        }
        public void SetTextOverlay(enOverlayIndex index, bool enabled, string fontName, float fontSize, Color color,
            int posLeft, int posTop, string text, bool shadow, Color shadowColor, 
            bool transparency, Color backcolor, string textAlign)
        {
            try
            {
                if (String.IsNullOrEmpty(text))
                    enabled = false;
                //
                if (this.CurrentPlayer.Overlays.ContainsKey(index.GetHashCode()))
                {
                    var item = this.CurrentPlayer.Overlays[index.GetHashCode()];
                    item.Dispose();
                    this.CurrentPlayer.Overlays.Remove(index.GetHashCode());
                }
                //
                if(enabled)
                {
                    var ov = new TextOverlay(index, fontName, fontSize, color, posLeft, posTop, text, shadow,
                        shadowColor, new System.Drawing.Size(10, 10), transparency, backcolor, textAlign, false);
                    this.CurrentPlayer.Overlays.Add(index.GetHashCode(), ov);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void SetImageOverlay(enOverlayIndex index, bool enabled, string filePath, int width, int height,
            int posLeft, int posTop, int alphBlend, bool chromaKey, Color chromaColor, int chromaLeeway)
        {
            try
            {
                if (String.IsNullOrEmpty(filePath))
                    enabled = false;
                //
                if (this.CurrentPlayer.Overlays.ContainsKey(index.GetHashCode()))
                {
                    var item = this.CurrentPlayer.Overlays[index.GetHashCode()];
                    item.Dispose();
                    this.CurrentPlayer.Overlays.Remove(index.GetHashCode());
                }
                //
                if (enabled)
                {
                    var ov = new ImageOverlay(index, filePath, width, height, posLeft, posTop, alphBlend,
                        chromaKey, chromaColor, chromaLeeway);
                    this.CurrentPlayer.Overlays.Add(index.GetHashCode(), ov);
                }
                
            }
            catch (Exception ex) { ex.Log(); }
        }
        public void SetCrawlOverlay(enOverlayIndex index, bool enabled, string fontName, float fontSize,
            Color color, int posLeft, int posTop, string text, bool shadow, Color shadowColor, 
            bool transparency, Color backcolor, string textAlign,
            bool scrolling, int scrollingSpeed, bool readFromFile, string filePath,string direction,
            bool isGlobal)
        {
            try
            {
                if (String.IsNullOrEmpty(text))
                    enabled = false;
                //
                if (this.CurrentPlayer.Overlays.ContainsKey(index.GetHashCode()))
                {
                    var item = this.CurrentPlayer.Overlays[index.GetHashCode()];
                    item.Dispose();
                    this.CurrentPlayer.Overlays.Remove(index.GetHashCode());
                }
                //
                if (enabled)
                {
                    var ov = new CrawlOverlay(index, fontName, fontSize, color, posLeft, posTop, text, shadow,
                        shadowColor, new System.Drawing.Size(10, 10), transparency, backcolor, textAlign,
                        filePath, readFromFile, scrolling, scrollingSpeed, direction, isGlobal);

                    this.CurrentPlayer.Overlays.Add(index.GetHashCode(), ov);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void SetTrialTextOverlay(bool enable, Color color)
        {
            try
            {
                if (!enable)
                    return;

                if (this.CurrentPlayer.Overlays.ContainsKey(enOverlayIndex.Trial.GetHashCode()))
                {
                    var item = this.CurrentPlayer.Overlays[enOverlayIndex.Trial.GetHashCode()];
                    item.Dispose();
                    this.CurrentPlayer.Overlays.Remove(enOverlayIndex.Trial.GetHashCode());
                }
                //
                string text = "Playout - CopyRight - Watermark";
                var ov = new TextOverlay(enOverlayIndex.Trial, "Arial", 20, color, 10, 10, text, false,
                    Color.Black, new System.Drawing.Size(), true, Color.Black, "Center", true);
                this.CurrentPlayer.Overlays.Add(enOverlayIndex.Trial.GetHashCode(), ov);

            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void SetVideoOverlay(bool enabled, string filePath, int width, int height, int posLeft, int posTop)
        {
            try
            {
                if (enabled)
                {
                    this.CurrentPlayer.PIP_FilePath = filePath;
                    this.CurrentPlayer.PIP_Height = height;
                    this.CurrentPlayer.PIP_PosX = posLeft;
                    this.CurrentPlayer.PIP_PosY = posTop;
                    this.CurrentPlayer.PIP_Width = width;
                }
                else
                    this.CurrentPlayer.PIP_FilePath = null;
            }
            catch(Exception ex) { ex.Log(); }
        }

        public void PlayerBackward()
        {
            try
            {
                
                
            }
            catch(Exception ex) { ex.Log(); }
        }
        public bool PlayerFF(int ratio)
        {
            try
            {
                if (this.CurrentPlayerAsUri == null)
                    return false;
                
                return true;
            }
            catch(Exception ex) { ex.Log(); return false; }
        }
        public void PlayerFR()
        {
            try
            {
               // this.Vg.RewindPlayer();
            }
            catch (Exception ex) { ex.Log(); }
        }

        public static void ShowPropertiesPageForInputVideoCapture(string filterName)
        {
            var m_graph = new FilterGraphNoThread() as IGraphBuilder;
            IBaseFilter filter = DirectShow.Utils.FilterGraphTools.AddFilterByName(m_graph, FilterCategory.VideoInputDevice, filterName);
            System.Windows.Forms.Form frm = new System.Windows.Forms.Form();
            DirectShow.Utils.FilterGraphTools.LoadGraphFile(m_graph, "d:\\1.txt");
            //
            //DirectShow.Utils.FilterGraphTools.LoadFilterProperties(filter, "d:\\1.txt");
            DirectShow.Utils.FilterGraphTools.ShowFilterPropertyPage(filter, frm.Handle);
            //
            //DirectShow.Utils.FilterGraphTools.SaveFilterProperties(filter,"d:\\1.txt");
            DirectShow.Utils.FilterGraphTools.SaveGraphFile(m_graph, "d:\\1.txt");
            //
            Marshal.ReleaseComObject(filter);
            Marshal.ReleaseComObject(m_graph);
        }

        public bool GetFileInfo(string file, ref long Duration, ref int _VideoWidth, ref int _VideoHeight,ref int frameRate)
        {
            try
            {
                long duration = 0;
                int width = 0, height = 0, fr = 0;

                XMediaHelper.MediaHelper mh = new XMediaHelper.MediaHelper();
                int i = mh.GetFileInfo(file, ref width, ref height, ref fr, ref duration);
                //
                Duration = Math.Abs(duration) * 10;
                _VideoHeight = height;
                _VideoWidth = width;
                frameRate = fr;
                //
                return true;
            }
            catch(Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        public bool GetFileDuration(string file, ref long Duration)
        {
            try
            {
                XMediaHelper.MediaHelper mh = new XMediaHelper.MediaHelper();
                //
                Duration = mh.GetDuration(file);
                Duration = Math.Abs(Duration)* 10;
                //
                return true;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        public static string[] GetInstalledVCamChannels()
        {
            try
            {
                var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

                var deviceList = (from d in devices
                                  where d.Name.Contains("Playout VCam")
                                  select "Channel" + d.Name.Substring(d.Name.Length - 1, 1));
                return deviceList.ToArray();
            }
            catch (Exception ex)
            {
                ex.Log();
                return new string[] { "Channel1" };
            }
        }
    }
    public class OutputVideoFormatPars
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int FrameRateScale { get; set; }
        public int FrameRateDuration { get; set; }

        public OutputVideoFormatPars()
        {
            this.Width = 0;
            this.Height = 0;
            this.FrameRateDuration = 0;
            this.FrameRateScale = 0;
        }
    }
}
