using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Playout.ViewModels.ViewModels
{
    public abstract class MediaOverlayViewModel:Base.FormBaseViewModel
    {
        int _positionLeft;
        int _positionTop;
        bool _enabled;
        string _overlayCaption;
        public int PositionLeft
        {
            get { return this._positionLeft; }
            set
            {
                if (this._positionLeft != value)
                {
                    this._positionLeft = value;
                    this.NotifyOfPropertyChange(() => this.PositionLeft);
                }
            }
        }
        public int PositionTop
        {
            get { return this._positionTop; }
            set
            {
                if (this._positionTop != value)
                {
                    this._positionTop = value;
                    this.NotifyOfPropertyChange(() => this.PositionTop);
                }
            }
        }
        public bool Enabled
        {
            get { return this._enabled; }
            set
            {
                if (this._enabled != value)
                {
                    this._enabled = value;
                    this.NotifyOfPropertyChange(() => this.Enabled);
                    this.NotifyOfPropertyChange(() => this.MediaOverlayIcon);
                }
            }
        }
        public abstract string MediaOverlayIcon { get; }

        public string OverlayCaption
        {
            get { return this._overlayCaption; }
            set
            {
                if (this._overlayCaption != value)
                {
                    if (!string.IsNullOrEmpty(value) && value.Length > 50)
                        this._overlayCaption = value.Substring(0, 50) + "...";
                    else
                        this._overlayCaption = value;
                    //
                    this.NotifyOfPropertyChange(() => this.OverlayCaption);
                    this.NotifyOfPropertyChange(() => this.OverlayCaption);
                }
            }
        }

        public virtual SolidColorBrush OverlayCaptionForground
        {
            get
            {
                return new SolidColorBrush(Colors.WhiteSmoke);
            }
        }
    }
    
}
