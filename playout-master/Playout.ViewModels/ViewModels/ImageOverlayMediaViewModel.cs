using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playout.ViewModels.ViewModels
{
    public class ImageOverlayMediaViewModel : MediaOverlayViewModel
    {
        string _filePath;
        int _alphaBlending;
        int _width;
        int _height;
        bool _chromaKey;
        Color _chromaColor;
        int _chromaLeeway;

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
                        this.OverlayCaption = (!string.IsNullOrEmpty(this._filePath) ? Path.GetFileName(this._filePath) : "");
                    }
                    catch { }
                    this.NotifyOfPropertyChange(() => this.FilePath);
                    this.NotifyOfPropertyChange(() => this.MediaOverlayIcon);
                    this.NotifyOfPropertyChange(() => this.MediaOverlayBackground);
                }
            }
        }
        public int AlphaBlending
        {
            get { return this._alphaBlending; }
            set
            {
                if (this._alphaBlending != value)
                {
                    this._alphaBlending = value;
                    this.NotifyOfPropertyChange(() => this.AlphaBlending);
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
        
        public bool ChromaKey
        {
            get { return this._chromaKey; }
            set
            {
                if (this._chromaKey != value)
                {
                    this._chromaKey = value;
                    this.NotifyOfPropertyChange(() => this.ChromaKey);
                }
            }
        }
        public Color ChromaColor
        {
            get { return this._chromaColor; }
            set
            {
                if (this._chromaColor != value)
                {
                    this._chromaColor = value;
                    this.NotifyOfPropertyChange(() => this.ChromaColor);
                }
            }
        }
        public int ChromaLeeway
        {
            get { return this._chromaLeeway; }
            set
            {
                if (this._chromaLeeway != value)
                {
                    this._chromaLeeway = value;
                    this.NotifyOfPropertyChange(() => this.ChromaLeeway);
                }
            }
        }
        public override string MediaOverlayIcon
        {
            get
            {
                return this.Enabled && !String.IsNullOrEmpty(this.FilePath) ? "I" : "";
            }
        }
        public string MediaOverlayBackground
        {
            get
            {
                if (String.IsNullOrEmpty(this.FilePath))
                    return @"Resources\NoImage.jpg";
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
        public ImageOverlayMediaViewModel()
        {
            this.Width = 120;
            this.Height = 120;
            this.AlphaBlending = 255;
            this.Enabled = false;
            this.ChromaLeeway = 90;
        }
        public ImageOverlayMediaViewModel(ImageOverlayMediaModel model)
        {
            this.Width = model.Width;
            this.Height = model.Height;
            this.AlphaBlending = model.AlphaBlending;
            this.Enabled = model.Enabled;
            this.PositionTop = model.PositionTop;
            this.PositionLeft = model.PositionLeft;
            this.ChromaKey = model.ChromaKey;
            this.ChromaColor = Functions.ConvertIntToColor(model.ChromaColor);
            this.ChromaLeeway = model.ChromaLeeway;
            this.FilePath = model.FilePath;
        }

        public ImageOverlayMediaViewModel Clone()
        {
            ImageOverlayMediaViewModel si = new ImageOverlayMediaViewModel()
            {
                AlphaBlending = this.AlphaBlending,
                Enabled = this.Enabled,
                FilePath = this.FilePath,
                Height = this.Height,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                ChromaKey = this.ChromaKey,
                ChromaColor = this.ChromaColor,
                ChromaLeeway = this.ChromaLeeway,
                Width = this.Width
            };
            return si;
        }

        public ImageOverlayMediaModel GetModel()
        {
            ImageOverlayMediaModel model = new ImageOverlayMediaModel()
            {
                AlphaBlending = this.AlphaBlending,
                Enabled = this.Enabled,
                FilePath = this.FilePath,
                Height = this.Height,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                ChromaColor = Functions.ConvertColorToInt(this.ChromaColor),
                ChromaKey = this.ChromaKey,
                ChromaLeeway = this.ChromaLeeway,
                Width = this.Width
            };
            return model;
        }
    }
}
