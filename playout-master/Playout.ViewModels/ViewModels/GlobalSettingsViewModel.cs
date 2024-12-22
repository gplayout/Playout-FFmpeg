using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using enOverlayIndex = Playout.DirectShow.Overlays.enOverlayIndex;

namespace Playout.ViewModels.ViewModels
{
    public class GlobalSettingsViewModel : Base.FormBaseViewModel
    {
        private StringOverlayMediaViewModel _stringOverlay;
        private ImageOverlayMediaViewModel _imageOverlay;
        private CrawlOverlayMediaViewModel _crawlOverlay;
        private bool _loopOnPlaylists;
        private bool _playbackMode;

        public StringOverlayMediaViewModel StringOverlay
        {
            get { return this._stringOverlay; }
            set
            {
                if (this._stringOverlay != value)
                {
                    this._stringOverlay = value;
                    this.NotifyOfPropertyChange(() => this.StringOverlay);
                }
            }
        }
        public CrawlOverlayMediaViewModel CrawlOverlay
        {
            get { return this._crawlOverlay; }
            set
            {
                if (this._crawlOverlay != value)
                {
                    this._crawlOverlay = value;
                    this.NotifyOfPropertyChange(() => this.CrawlOverlay);
                }
            }
        }
        public ImageOverlayMediaViewModel ImageOverlay
        {
            get { return this._imageOverlay; }
            set
            {
                if (this._imageOverlay != value)
                {
                    this._imageOverlay = value;
                    this.NotifyOfPropertyChange(() => this.ImageOverlay);
                }
            }
        }
        public bool LoopOnPlaylists
        {
            get { return this._loopOnPlaylists; }
            set
            {
                if (this._loopOnPlaylists != value)
                {
                    this._loopOnPlaylists = value;
                    this.NotifyOfPropertyChange(() => this.LoopOnPlaylists);
                }
            }
        }
        public bool PlaybackMode
        {
            get { return this._playbackMode; }
            set
            {
                if (this._playbackMode != value)
                {
                    this._playbackMode = value;
                    this.NotifyOfPropertyChange(() => this.PlaybackMode);
                }
            }
        }
        public GlobalSettingsViewModel()
        {
            this.StringOverlay = new StringOverlayMediaViewModel();
            this.ImageOverlay = new ImageOverlayMediaViewModel();
            this.CrawlOverlay = new CrawlOverlayMediaViewModel();
            this.LoopOnPlaylists = false;
            //
            Program.Dg.OnCheckOverlay_Global = this.CheckOverlayGlobal;
        }
        public void Load(GlobalSettingModel model)
        {
            this.StringOverlay = new StringOverlayMediaViewModel(model.StringOverlay);
            this.ImageOverlay = new ImageOverlayMediaViewModel(model.ImageOverlay);
            this.CrawlOverlay = new CrawlOverlayMediaViewModel(model.CrawlOverlay);
            this.LoopOnPlaylists = model.LoopOnPlaylists;
            this.PlaybackMode = model.PlaybackMode;
            //
            this.OperateSettings();
        }
        public GlobalSettingModel GetModel()
        {
            GlobalSettingModel model = new GlobalSettingModel()
            {
                ImageOverlay = this.ImageOverlay.GetModel(),
                StringOverlay = this.StringOverlay.GetModel(),
                CrawlOverlay = this.CrawlOverlay.GetModel(),
                LoopOnPlaylists = this.LoopOnPlaylists,
                PlaybackMode = this.PlaybackMode
            };
            return model;
        }

        public void OperateSettings()
        {
            this.CheckOverlayGlobal();
        }

        private void CheckOverlayGlobal()
        {
            var so = this.StringOverlay;
            Program.Dg.SetTextOverlay(enOverlayIndex.GlobalText, so.Enabled, so.FontName, so.FontSize,
                Functions.ConvertWColorToDColor(so.Color), so.PositionLeft, so.PositionTop, so.Text,
                so.Shadow, Functions.ConvertWColorToDColor(so.ShadowColor),
                so.Transparency, Functions.ConvertWColorToDColor(so.BackColor), so.TextAlign);
            //
            var si = this.ImageOverlay;
            Program.Dg.SetImageOverlay(enOverlayIndex.GlobalImage, si.Enabled, si.FilePath, 
                si.Width, si.Height, si.PositionLeft, si.PositionTop,
                si.AlphaBlending, false, System.Drawing.Color.Black, 0);
            //
            var co = this.CrawlOverlay;
            Program.Dg.SetCrawlOverlay(enOverlayIndex.GlobalCrawl, co.Enabled, co.FontName, co.FontSize,
                Functions.ConvertWColorToDColor(co.Color), co.PositionLeft, co.PositionTop, co.Text,
                co.Shadow, Functions.ConvertWColorToDColor(co.ShadowColor),
                co.Transparency, Functions.ConvertWColorToDColor(co.BackColor), co.TextAlign,
                co.Scrolling, co.ScrollingSpeed, co.ReadFromFile, co.FilePath, co.Direction.ToString(), true);
            //
            bool trialEnabled = Program.Lock.LockStatus == RoyalLock.LockStatusEnum.Trial;
            Program.Dg.SetTrialTextOverlay(trialEnabled, Functions.ConvertWColorToDColor(Colors.White));
        }
    }
}
