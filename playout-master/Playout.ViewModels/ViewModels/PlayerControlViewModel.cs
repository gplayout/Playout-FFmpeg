using Playout.Base;
using Playout.MediaFramework;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using enOverlayIndex = Playout.DirectShow.Overlays.enOverlayIndex;
using Playout.Log;
using Playout.DirectShow.MediaPlayers;

namespace Playout.ViewModels.ViewModels
{
    public class PlayerControlViewModel : Base.FormBaseViewModel
    {
        System.Windows.Forms.Timer _tim = null;
        public event EventHandler SetCurrentMediaSource = null;
        public Action<string> OnBeforeSetMediaSource_ForPreview = null;
        //
        MediaSourceViewModel _currentMediaSource;
        MediaPlaylistViewModel _currentPlaylist;
        bool _scheduled = false;
        MediaPlaylistViewModel _beforeScheduledPlaylist = null;
        MediaSourceViewModel _beforeScheduledFile = null;
        long _beforeScheduledPosition = 0;
        InfoLogger _playLogger = null;
        TimeSpan _playerStartTime = new TimeSpan(0, 0, 0);
        TimeSpan _currentPlaylistStartTime = new TimeSpan(0, 0, 0);

        public MediaSourceViewModel CurrentMediaSource
        {
            get { return this._currentMediaSource; }
            set
            {
                if (this._currentMediaSource != value)
                {
                    this._currentMediaSource = value;
                    this.NotifyOfPropertyChange(() => this.CurrentMediaSource);
                }
            }
        }
        private MediaSourceModel CurrentNestedMediaSource { get; set; }
        public MediaPlaylistViewModel CurrentPlaylist
        {
            get { return this._currentPlaylist; }
            set
            {
                if (this._currentPlaylist != value)
                {
                    this._currentPlaylist = value;
                    this.NotifyOfPropertyChange(() => this.CurrentPlaylist);
                }
            }
        }
        public MediaFramework.DirectShowGrabber Dg { get; private set; }
        public MediaPlaylistsViewModel PlaylistsVM { get; private set; }
        public Action<MediaPlaylistViewModel> OnPlaylistEnd { get; set; }

        public TimeSpan PlayerTimeElapsed
        {
            get
            {
                var curpl = this.CurrentPlaylist;
                if (curpl == null || this.CurrentMediaSource == null)
                    return TimeSpan.FromSeconds(0);
                //
                int index = curpl.MediaSourcesVM.Sources.IndexOf(this.CurrentMediaSource);
                if (index == -1)
                    return TimeSpan.FromSeconds(0);
                //
                double secs = this.Dg.PlayerTimePositionSecond - this.CurrentMediaSource.TrimStartSecond;

                for (int i = 0; i < index; i++)
                {
                    secs += curpl.MediaSourcesVM.Sources[i].TrimDuration.TotalSeconds;
                }
                //
                index = this.PlaylistsVM.Playlists.IndexOf(curpl);
                if (index != -1)
                {
                    for (int i = 0; i < index; i++)
                    {
                        secs += this.PlaylistsVM.Playlists[i].MediaSourcesVM.FilesTrimDuration.TotalSeconds;
                    }
                }
                //   
                return TimeSpan.FromSeconds(secs);
            }
        }
        public TimeSpan PlayerStartTime
        {
            get { return this._playerStartTime; }
            set
            {
                if (this._playerStartTime != value)
                {
                    this._playerStartTime = value;
                    this.NotifyOfPropertyChange(() => this.PlayerStartTime);
                }
            }
        }
        public TimeSpan CurrentPlaylistStartTime
        {
            get { return this._currentPlaylistStartTime; }
            set
            {
                if (this._currentPlaylistStartTime != value)
                {
                    this._currentPlaylistStartTime = value;
                    this.NotifyOfPropertyChange(() => this.CurrentPlaylistStartTime);
                }
            }
        }
        public TimeSpan PlayerTimeRemained
        {
            get
            {
                return TimeSpan.FromSeconds(this.TotalDuration.TotalSeconds - this.PlayerTimeElapsed.TotalSeconds);
            }
        }
        public TimeSpan CurrentItemTimeElapsed
        {
            get
            {
                var curpl = this.CurrentPlaylist;
                if (curpl == null || this.CurrentMediaSource == null)
                    return TimeSpan.FromSeconds(0);
                //
                int index = curpl.MediaSourcesVM.Sources.IndexOf(this.CurrentMediaSource);
                if (index == -1)
                    return TimeSpan.FromSeconds(0);
                //
                double secs = this.Dg.PlayerTimePositionSecond;// - this.CurrentMediaSource.TrimStartSecond;
                if (secs < 0)
                    secs = 0;
                //   
                return TimeSpan.FromSeconds(secs);
            }
        }
        public TimeSpan CurrentItemTimeRemained
        {
            get
            {
                var curpl = this.CurrentPlaylist;
                if (curpl == null || this.CurrentMediaSource == null || this.CurrentMediaSource.Duration == null)
                    return TimeSpan.FromSeconds(0);
                //
                return TimeSpan.FromSeconds(this.CurrentMediaSource.TrimDuration.TotalSeconds 
                    - this.CurrentItemTimeElapsed.TotalSeconds
                    + this.CurrentMediaSource.TrimStartSecond);
            }
        }
        public TimeSpan CurrentPlaylistTimeRemained
        {
            get
            {
                var current = this.CurrentPlaylist;
                if (current == null && this.PlaylistsVM != null && this.PlaylistsVM.Playlists.Count > 0)
                    current = this.PlaylistsVM.Playlists[0];
                //
                if (current == null)
                    return TimeSpan.FromSeconds(0);
                //
                if (this.CurrentMediaSource == null)
                    return current.MediaSourcesVM.FilesTrimDuration;
                //
                double secs = this.CurrentMediaSource.TrimStartSecond - this.Dg.PlayerTimePositionSecond;
                //
                int index = current.MediaSourcesVM.Sources.IndexOf(this.CurrentMediaSource);
                if (index == -1)
                    return TimeSpan.FromSeconds(0);
                //
                for (int i = index; i < current.MediaSourcesVM.Sources.Count; i++)
                {
                    secs += current.MediaSourcesVM.Sources[i].TrimDuration.TotalSeconds;
                }
                return TimeSpan.FromSeconds(secs);
            }
        }
        public TimeSpan CurrentPlaylistTimeElapsed
        {
            get
            {
                var current = this.CurrentPlaylist;
                if (current == null && this.PlaylistsVM != null && this.PlaylistsVM.Playlists.Count > 0)
                    current = this.PlaylistsVM.Playlists[0];
                //
                if (current == null)
                    return TimeSpan.FromSeconds(0);
                //
                if (this.CurrentMediaSource == null)
                    return current.MediaSourcesVM.FilesTrimDuration;
                //
                long ticks = this.Dg.CurrentPlayerAsUri.MediaPositionPlusOffset;
                //
                int index = current.MediaSourcesVM.Sources.IndexOf(this.CurrentMediaSource);
                if (index == -1)
                    return TimeSpan.FromSeconds(0);
                //
                for (int i = 0; i < index; i++)
                {
                    ticks += (long)current.MediaSourcesVM.Sources[i].TrimDuration.TotalSeconds * TimeSpan.TicksPerSecond;
                }
                return TimeSpan.FromTicks(ticks);
            }
        }
        public TimeSpan CurrentPlaylistTimeTotal
        {
            get
            {
                var current = this.CurrentPlaylist;
                if (current == null && this.PlaylistsVM != null && this.PlaylistsVM.Playlists.Count > 0)
                    current = this.PlaylistsVM.Playlists[0];
                //
                if (current == null)
                    return TimeSpan.FromSeconds(0);
                //
                return current.MediaSourcesVM.FilesTrimDuration;
            }
        }
        public TimeSpan TotalDuration
        {
            get
            {
                if (this.PlaylistsVM == null || this.PlaylistsVM.Playlists == null)
                    return TimeSpan.FromSeconds(0);
                //
                TimeSpan totalDuration = TimeSpan.FromSeconds(0);
                //
                foreach (var pl in this.PlaylistsVM.Playlists)
                {
                    if (pl.MediaSourcesVM != null)
                    {
                        totalDuration = totalDuration.Add(pl.MediaSourcesVM.FilesTrimDuration);
                    }
                }
                //
                return totalDuration;
            }
        }
        public TimeSpan NowTime
        {
            get { return DateTime.Now.TimeOfDay; }
        }
        public string NowTimeZone
        {
            get
            {
                var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                //
                return TimeZone.CurrentTimeZone.StandardName + " ("
                        + ((utcOffset < TimeSpan.Zero) ? "-" : "+") + utcOffset.ToString("hh\\:mm") + " )";
            }
        }
        public enMediaType CurrentPlayMediaType
        {
            get
            {
                if (this.CurrentMediaSource == null)
                    return enMediaType.VideoFile;
                //
                if (this.CurrentMediaSource.MediaType != enMediaType.Playlist)
                    return this.CurrentMediaSource.MediaType;
                else
                    return this.CurrentNestedMediaSource.MediaType;
            }
        }

        public PlayerControlViewModel(MediaFramework.DirectShowGrabber dg, MediaPlaylistsViewModel playlistsVM)
        {
            this.Dg = dg;
            this.PlaylistsVM = playlistsVM;
            //
            this.Dg.SetOnPlayerEndOfStreamEvent(this.Vg_OnPlayerEndOfStream, CurrentPlayer_MediaEnded);
            this.Dg.OnCheckOverlay_CurrentItem = this.CheckOverlay;
            this.Dg.SetPIPOverlay = this.SetPIPOverlay;
            //
            this._tim = new System.Windows.Forms.Timer();
            this._tim.Interval = 300;
            this._tim.Tick += _tim_Tick;
            this._tim.Start();
            //
            if (!this.Dg.ForPreview)
            {
                this._playLogger = new InfoLogger();
                string message = "Date,Starting Play Time,Ending Play Time,Category,Alias Name,Source,Playlist Name,Type";
                this._playLogger.Initial("PlayoutLog.csv", message);
            }
        }

        void CurrentPlayer_MediaEnded()
        {
            this.LogCurrentMediaItem();
            this.PlayNext(this.CurrentPlaylist.Loop, false, false);
        }

        void _tim_Tick(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(() => this.NowTime);
            this.NotifyOfPropertyChange(() => this.NowTimeZone);
            this.NotifyOfPropertyChange(() => this.TotalDuration);
            this.NotifyOfPropertyChange(() => this.PlayerTimeElapsed);
            this.NotifyOfPropertyChange(() => this.PlayerTimeRemained);
            this.NotifyOfPropertyChange(() => this.CurrentPlaylistTimeRemained);
            this.NotifyOfPropertyChange(() => this.CurrentPlaylistTimeElapsed);
            this.NotifyOfPropertyChange(() => this.CurrentPlaylistTimeTotal);
            this.NotifyOfPropertyChange(() => this.CurrentItemTimeElapsed);
            this.NotifyOfPropertyChange(() => this.CurrentItemTimeRemained);
        }
        public void PlayScheduledPlaylist(MediaPlaylistViewModel pvm)
        {
            try
            {
                if (pvm == null || pvm.MediaSourcesVM == null || pvm.MediaSourcesVM.Sources.Count == 0)
                    return;
                //
                if (!this._scheduled)
                {
                    this._beforeScheduledPlaylist = this.CurrentPlaylist;
                    this._beforeScheduledFile = this.CurrentMediaSource;
                    this._beforeScheduledPosition = (long)(this.Dg.PlayerTimePosition / 10e6);
                    this._scheduled = true;
                }
                //
                //this.Dg.StopPlayer();
                //
                if (this.OnBeforeSetMediaSource_ForPreview != null)
                {
                    this.OnBeforeSetMediaSource_ForPreview(pvm.MediaSourcesVM.Sources[0].File_Url_DeviceName);
                }
                //
                this.PlayMediaSource(pvm, null);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void PlayMediaSource(MediaPlaylistViewModel pvm, MediaSourceViewModel fvm = null, long startPos = 0, int index = 0)
        {
            try
            {
                if (pvm == null || pvm.MediaSourcesVM == null || pvm.MediaSourcesVM.Sources.Count == 0)
                    return;
                //
                if (this.CurrentPlaylist != pvm || this.CurrentPlaylist == null || this.CurrentPlaylistStartTime.TotalSeconds == 0)
                {
                    this.CurrentPlaylistStartTime = DateTime.Now.TimeOfDay;
                }
                //
                this.CurrentPlaylist = pvm;
                //
                if (fvm != null)
                    this.CurrentMediaSource = fvm;
                else
                    this.CurrentMediaSource = pvm.MediaSourcesVM.Sources[index];
                //
                string fileurl = "";
                if (this.CurrentMediaSource.MediaType == enMediaType.Playlist)
                {
                    if (this.CurrentNestedMediaSource == null)
                        index = 0;
                    this.CurrentNestedMediaSource = this.CurrentMediaSource.NestedPlaylist.MediaSources[index];
                    fileurl = this.CurrentNestedMediaSource.File_Url;
                    //
                    this.Dg.OpenPlayer(this.CurrentPlayMediaType, fileurl, this.CurrentNestedMediaSource.DeviceName,
                        this.CurrentNestedMediaSource.InputDeviceFormat,
                        this.CurrentNestedMediaSource.ShowupEffect.ToString(),
                        startPos != 0 ? startPos : this.CurrentNestedMediaSource.TrimStartSecond, this.CurrentNestedMediaSource.TrimStopSecond);
                }
                else
                {
                    fileurl = this.CurrentMediaSource.File_Url;
                    this.Dg.OpenPlayer(this.CurrentPlayMediaType, fileurl, this.CurrentMediaSource.DeviceName,
                        this.CurrentMediaSource.InputDeviceFormat,
                        this.CurrentMediaSource.ShowupEffect.ToString(),
                        startPos != 0 ? startPos : this.CurrentMediaSource.TrimStartSecond, this.CurrentMediaSource.TrimStopSecond);
                }
                //
                this.PlayerStartTime = DateTime.Now.TimeOfDay;
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        private void SetPIPOverlay()
        {
            if (this.CurrentPlayMediaType == enMediaType.Playlist)
            {
                var vo = this.CurrentNestedMediaSource.VideoOverlay;
                this.Dg.SetVideoOverlay(vo.Enabled, vo.FilePath, vo.Width, vo.Height, vo.PositionLeft, vo.PositionTop);
            }
            else
            {
                var vo = this.CurrentMediaSource.VideoOverlay;
                this.Dg.SetVideoOverlay(vo.Enabled, vo.FilePath, vo.Width, vo.Height, vo.PositionLeft, vo.PositionTop);
            }
        }
        private void CheckOverlay()
        {
            if (this.CurrentPlayMediaType == enMediaType.Playlist)
            {
                var so = this.CurrentNestedMediaSource.StringOverlay;
                this.Dg.SetTextOverlay(enOverlayIndex.ItemText, so.Enabled, so.FontName, so.FontSize,
                    Functions.ConvertIntToDColor(so.Color),
                    so.PositionLeft, so.PositionTop, so.Text,
                    so.Shadow, Functions.ConvertIntToDColor(so.ShadowColor),
                    so.Transparency, Functions.ConvertIntToDColor(so.BackColor), so.TextAlign);
                //
                var io = this.CurrentNestedMediaSource.ImageOverlay;
                this.Dg.SetImageOverlay(enOverlayIndex.ItemImage, io.Enabled, io.FilePath,
                    io.Width, io.Height, io.PositionLeft, io.PositionTop,
                    io.AlphaBlending, io.ChromaKey, Functions.ConvertIntToDColor(io.ChromaColor), io.ChromaLeeway);
                //
                var co = this.CurrentNestedMediaSource.CrawlOverlay;
                this.Dg.SetCrawlOverlay(enOverlayIndex.ItemCrawl, co.Enabled, co.FontName, co.FontSize,
                    Functions.ConvertIntToDColor(co.Color), co.PositionLeft, co.PositionTop, co.Text,
                    co.Shadow, Functions.ConvertIntToDColor(co.ShadowColor), co.Transparency,
                    Functions.ConvertIntToDColor(co.BackColor), co.TextAlign, co.Scrolling, co.ScrollingSpeed,
                    co.ReadFromFile, co.FilePath, co.Direction.ToString(), false);
            }
            else
            {
                var so = this.CurrentMediaSource.StringOverlay;
                this.Dg.SetTextOverlay(enOverlayIndex.ItemText, so.Enabled, so.FontName, so.FontSize,
                    Functions.ConvertWColorToDColor(so.Color), so.PositionLeft, so.PositionTop, so.Text,
                    so.Shadow, Functions.ConvertWColorToDColor(so.ShadowColor),
                    so.Transparency, Functions.ConvertWColorToDColor(so.BackColor), so.TextAlign);
                //
                var io = this.CurrentMediaSource.ImageOverlay;
                this.Dg.SetImageOverlay(enOverlayIndex.ItemImage, io.Enabled, io.FilePath,
                    io.Width, io.Height, io.PositionLeft, io.PositionTop,
                    io.AlphaBlending, io.ChromaKey, Functions.ConvertWColorToDColor(io.ChromaColor),
                    io.ChromaLeeway);
                //
                var co = this.CurrentMediaSource.CrawlOverlay;
                this.Dg.SetCrawlOverlay(enOverlayIndex.ItemCrawl, co.Enabled, co.FontName, co.FontSize,
                    Functions.ConvertWColorToDColor(co.Color), co.PositionLeft, co.PositionTop, co.Text,
                    co.Shadow, Functions.ConvertWColorToDColor(co.ShadowColor),
                    co.Transparency, Functions.ConvertWColorToDColor(co.BackColor), co.TextAlign,
                    co.Scrolling, co.ScrollingSpeed, co.ReadFromFile, co.FilePath, co.Direction.ToString(), false);
            }

        }
        public void PlayNext(bool loopInPlaylist, bool previsous, bool force)
        {
            try
            {
                if (this.CurrentPlaylist == null || this.CurrentMediaSource == null)
                    return;
                //
                int index = 0;
                //Check Nested
                if (this.CurrentMediaSource.MediaType == enMediaType.Playlist)
                {
                    index = this.CurrentMediaSource.NestedPlaylist.MediaSources.IndexOf(this.CurrentNestedMediaSource);
                    if (this.CurrentNestedMediaSource.Loop)
                        index = index; //Loop On Nested Item
                    else if (previsous)
                        index--;
                    else
                        index++;
                    //
                    if (index < this.CurrentMediaSource.NestedPlaylist.MediaSources.Count && index >= 0)
                    {
                        this.PlayMediaSource(this.CurrentPlaylist, this.CurrentMediaSource, 0, index);
                        return;
                    }
                    else
                    {
                        this.CurrentNestedMediaSource = null;
                    }
                }
                //Next Item               
                index = this.CurrentPlaylist.MediaSourcesVM.Sources.IndexOf(this.CurrentMediaSource);
                //
                if (!force && this.CurrentMediaSource.Loop)
                    index = index;//Loop on Item
                else if (previsous)
                    index--;
                else
                    index++;
                //
                if (index >= this.CurrentPlaylist.MediaSourcesVM.Sources.Count || index < 0)
                {
                    if (loopInPlaylist)
                        index = 0;
                    else
                    {
                        //Check Is Scheduled Played
                        if (this._scheduled)
                        {
                            this._scheduled = false;
                            //
                            this.PlayMediaSource(this._beforeScheduledPlaylist, this._beforeScheduledFile, this._beforeScheduledPosition);
                        }
                        else
                        {
                            this.CurrentMediaSource = null;
                            if (this.OnPlaylistEnd != null)
                                this.OnPlaylistEnd(this.CurrentPlaylist);
                        }
                        return; //no item in playlist
                    }
                }
                //
                if (this.CurrentPlaylist != null && this.CurrentPlaylist.MediaSourcesVM.Sources.Count > index
                    && this.OnBeforeSetMediaSource_ForPreview != null)
                {
                    this.OnBeforeSetMediaSource_ForPreview(this.CurrentPlaylist.MediaSourcesVM.Sources[index].File_Url_DeviceName);
                }
                //
                this.PlayMediaSource(this.CurrentPlaylist, null, 0, index);
                //
                if (this.SetCurrentMediaSource != null)
                    this.SetCurrentMediaSource(null, null);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        private void Vg_OnPlayerEndOfStream(object sender, System.Windows.RoutedEventArgs e)
        {
            this.CurrentPlayer_MediaEnded();
        }

        public void LogCurrentMediaItem()
        {
            try
            {
                if (!Program.PlayoutLog || this.CurrentMediaSource == null || this._playLogger == null)
                    return;
                //
                string message =
                        DateTime.Now.ToString("d") + "," +
                        this.PlayerStartTime.ToString("T") + "," +
                        DateTime.Now.ToString("T") + "," +
                        (this.CurrentMediaSource.Category ?? " ") + ",";
                //
                if (this.CurrentMediaSource.MediaType == enMediaType.Playlist)
                {
                    message +=
                        (this.CurrentNestedMediaSource.AliasName ?? " ") + "," +
                        (!String.IsNullOrEmpty(this.CurrentNestedMediaSource.DeviceName) ? this.CurrentNestedMediaSource.DeviceName : this.CurrentNestedMediaSource.File_Url) + "," +
                        this.CurrentPlaylist.Name + "," +
                        this.CurrentMediaSource.MediaType.ToString();
                }
                else
                {
                    message +=
                        (this.CurrentMediaSource.AliasName ?? " ") + "," +
                        (!String.IsNullOrEmpty(this.CurrentMediaSource.DeviceName) ? this.CurrentMediaSource.DeviceName : this.CurrentMediaSource.File_Url) + "," +
                        this.CurrentPlaylist.Name + "," +
                        this.CurrentMediaSource.MediaType.ToString();
                }
                this._playLogger.InfoLog(message);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
    }
}
