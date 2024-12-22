using Playout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playout.ViewModels.ViewModels
{
    public class VideoOverlayMediaViewModel : MediaOverlayViewModel
    {
        string _filePath;
        int _width;
        int _height;

        [Required]
        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                if (this._filePath != value)
                {
                    this._filePath = value;
                    try
                    {
                        this.OverlayCaption = (!string.IsNullOrEmpty(this._filePath) ? System.IO.Path.GetFileName(this._filePath) : "");
                    }
                    catch { }
                    this.NotifyOfPropertyChange(() => this.FilePath);
                    this.NotifyOfPropertyChange(() => this.MediaOverlayIcon);
                    this.NotifyOfPropertyChange(() => this.MediaOverlayBackground);
                }
            }
        }

        public int Width
        {
            get { return this._width; }
            set
            {
                if (this._width != value)
                {
                    this._width = value;
                    this.NotifyOfPropertyChange(() => this.Width);
                }
            }
        }
        public int Height
        {
            get { return this._height; }
            set
            {
                if (this._height != value)
                {
                    this._height = value;
                    this.NotifyOfPropertyChange(() => this.Height);
                }
            }
        }
        public Color Color
        {
            get { return Colors.White; }
        }
        public override string MediaOverlayIcon
        {
            get
            {
                return this.Enabled && !String.IsNullOrEmpty(this.FilePath) ? "V" : "";
            }
        }
        public string MediaOverlayBackground
        {
            get
            {
                if (String.IsNullOrEmpty(this.FilePath))
                    return @"Resources\VideoOverlay.mp4";
                else
                    return this.FilePath;
            }
        }
        public override SolidColorBrush OverlayCaptionForground
        {
            get
            {
                return new SolidColorBrush(Colors.White);
            }
        }
        public VideoOverlayMediaViewModel()
        {
            this.Width = 120;
            this.Height = 80;
            this.Enabled = false;
        }
       
        
        public VideoOverlayMediaViewModel(VideoOverlayMediaModel model)
        {
            this.Width = model.Width;
            this.Height = model.Height;
            this.Enabled = model.Enabled;
            this.PositionTop = model.PositionTop;
            this.PositionLeft = model.PositionLeft;
            this.FilePath = model.FilePath;
        }
        public VideoOverlayMediaViewModel Clone()
        {
            var si = new VideoOverlayMediaViewModel()
            {
                Enabled = this.Enabled,
                FilePath = this.FilePath,
                Height = this.Height,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                Width = this.Width
            };
            return si;
        }

        public VideoOverlayMediaModel GetModel()
        {
            var model = new VideoOverlayMediaModel()
            {
                Enabled = this.Enabled,
                FilePath = this.FilePath,
                Height = this.Height,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                Width = this.Width
            };
            return model;
        }
    }
}
