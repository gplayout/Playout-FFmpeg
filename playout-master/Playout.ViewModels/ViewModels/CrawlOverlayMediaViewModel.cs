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
    public class BaseTextOverlayMediaViewModel:MediaOverlayViewModel
    {
        string _text;
        Color _color;
        string _fontName;
        int _fontSize;
        Color _backColor;
        bool _shadow;
        bool _transparency;
        string _textAlign;
        Color _shadowColor;

        [Required]
        public string Text
        {
            get { return this._text; }
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    this.OverlayCaption = this._text;
                    this.NotifyOfPropertyChange(() => this.Text);
                    this.NotifyOfPropertyChange(() => this.MediaOverlayIcon);
                }
            }
        }

        [Required]
        public Color Color
        {
            get { return this._color; }
            set
            {
                if (this._color != value)
                {
                    this._color = value;
                    this.NotifyOfPropertyChange(() => this.Color);
                }
            }
        }
        public Color BackColor
        {
            get { return this._backColor; }
            set
            {
                if (this._backColor != value)
                {
                    this._backColor = value;
                    this.NotifyOfPropertyChange(() => this.BackColor);
                    this.NotifyOfPropertyChange(() => this.TextOverlayBackground);
                }
            }
        }

        public bool Transparency
        {
            get { return this._transparency; }
            set
            {
                if (this._transparency != value)
                {
                    this._transparency = value;
                    this.NotifyOfPropertyChange(() => this.Transparency);
                }
            }
        }
        public bool Shadow
        {
            get { return this._shadow; }
            set
            {
                if (this._shadow != value)
                {
                    this._shadow = value;
                    this.NotifyOfPropertyChange(() => this.Shadow);
                }
            }
        }
        public Color ShadowColor
        {
            get { return this._shadowColor; }
            set
            {
                if (this._shadowColor != value)
                {
                    this._shadowColor = value;
                    this.NotifyOfPropertyChange(() => this.ShadowColor);
                }
            }
        }
        public string TextAlign
        {
            get { return this._textAlign; }
            set
            {
                if (this._textAlign != value)
                {
                    this._textAlign = value;
                    this.NotifyOfPropertyChange(() => this.TextAlign);
                    this.NotifyOfPropertyChange(() => this.FlowDirection);
                }
            }
        }
        public System.Windows.FlowDirection FlowDirection
        {
            get
            {
                if (!String.IsNullOrEmpty(this._textAlign) && this._textAlign == "Right")
                    return System.Windows.FlowDirection.RightToLeft;
                else
                    return System.Windows.FlowDirection.LeftToRight;
            }
        }
        public FontFamily[] Fonts
        {
            get { return System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(m => m.Source).ToArray(); }
        }
        public string[] TextAligns
        {
            get { return Enum.GetNames(typeof(TextOverlayAlign)); }
        }
        [Required]
        public string FontName
        {
            get { return this._fontName; }
            set
            {
                if (this._fontName != value)
                {
                    this._fontName = value;
                    this.NotifyOfPropertyChange(() => this.FontName);
                }
            }
        }
        public int FontSize
        {
            get { return this._fontSize; }
            set
            {
                if (this._fontSize != value)
                {
                    this._fontSize = value;
                    this.NotifyOfPropertyChange(() => this.FontSize);
                }
            }
        }
        public override string MediaOverlayIcon { get { return ""; } }
        public Brush TextOverlayBackground
        {
            get
            {
                var brush = new SolidColorBrush(this.BackColor);
                return brush;
            }
        }

        public BaseTextOverlayMediaViewModel()
        {
            this.Color = Colors.White;
            this.BackColor = Colors.Brown;
            this.FontName = "Arial";
            this.FontSize = 15;
            this.Enabled = false;
        }
        
    }
    public class CrawlOverlayMediaViewModel : BaseTextOverlayMediaViewModel
    {
        
        bool _scrolling;
        int _scrollingSpeed;
        string _filePath;
        bool _readFromFile;
        CrawlDirection _direction;

        
        public bool Scrolling
        {
            get { return this._scrolling; }
            set
            {
                if (this._scrolling != value)
                {
                    this._scrolling = value;
                    this.NotifyOfPropertyChange(() => this.Scrolling);
                }
            }
        }
        public int ScrollingSpeed
        {
            get { return this._scrollingSpeed; }
            set
            {
                if (this._scrollingSpeed != value)
                {
                    this._scrollingSpeed = value;
                    this.NotifyOfPropertyChange(() => this.ScrollingSpeed);
                }
            }
        }
        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                if (this._filePath != value)
                {
                    this._filePath = value;
                    this.NotifyOfPropertyChange(() => this.FilePath);
                }
            }
        }
        public bool ReadFromFile
        {
            get { return this._readFromFile; }
            set
            {
                if (this._readFromFile != value)
                {
                    this._readFromFile = value;
                    this.NotifyOfPropertyChange(() => this.ReadFromFile);
                    if (this._readFromFile && String.IsNullOrEmpty(this.Text))
                        this.Text = "-";
                }
            }
        }
        public string[] CrawlDirections
        {
            get { return Enum.GetNames(typeof(CrawlDirection)); }
        }
        public CrawlDirection Direction
        {
            get { return this._direction; }
            set
            {
                if (this._direction != value)
                {
                    this._direction = value;
                    this.NotifyOfPropertyChange(() => this.Direction);
                }
            }
        }
        public override string MediaOverlayIcon
        {
            get
            {
                if (this.Enabled &&
                    (
                        (this.ReadFromFile && !String.IsNullOrEmpty(this.FilePath))
                        ||
                        (!this.ReadFromFile && !String.IsNullOrEmpty(this.Text))
                    ))
                {
                    return "C";
                }
                else
                    return "";
            }
        }
        public CrawlOverlayMediaViewModel() : base() { }
        public CrawlOverlayMediaViewModel(CrawlOverlayMediaModel model)
        {
            this.Color = Functions.ConvertIntToColor(model.Color);
            this.FontName = model.FontName;
            this.FontSize = model.FontSize;
            this.Enabled = model.Enabled;
            this.PositionLeft = model.PositionLeft;
            this.PositionTop = model.PositionTop;
            this.Text = model.Text;
            this.Shadow = model.Shadow;
            this.Transparency = model.Transparency;
            this.TextAlign = model.TextAlign;
            this.BackColor = Functions.ConvertIntToColor(model.BackColor);
            this.ShadowColor = Functions.ConvertIntToColor(model.ShadowColor);
            this.Scrolling = model.Scrolling;
            this.ScrollingSpeed = model.ScrollingSpeed;
            this.FilePath = model.FilePath;
            this.ReadFromFile = model.ReadFromFile;
            this.Direction = model.Direction;
        }

        public CrawlOverlayMediaViewModel Clone()
        {
            CrawlOverlayMediaViewModel so = new CrawlOverlayMediaViewModel()
            {
                Color = this.Color,
                Enabled = this.Enabled,
                FontName = this.FontName,
                FontSize = this.FontSize,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                Text = this.Text,
                BackColor = this.BackColor,
                Shadow = this.Shadow,
                ShadowColor = this.ShadowColor,
                TextAlign = this.TextAlign,
                Transparency = this.Transparency,
                Scrolling = this.Scrolling,
                Direction = this.Direction,
                ScrollingSpeed = this.ScrollingSpeed,
                FilePath = this.FilePath,
                ReadFromFile = this.ReadFromFile
            };
            return so;
        }
        public CrawlOverlayMediaModel GetModel()
        {
            CrawlOverlayMediaModel model = new CrawlOverlayMediaModel()
            {
                Color = Functions.ConvertColorToInt(this.Color),
                Enabled = this.Enabled,
                FontName = this.FontName,
                FontSize = this.FontSize,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                Text = this.Text,
                BackColor = Functions.ConvertColorToInt(this.BackColor),
                Transparency = this.Transparency,
                Shadow = this.Shadow,
                ShadowColor = Functions.ConvertColorToInt(this.ShadowColor),
                TextAlign = this.TextAlign,
                Scrolling = this.Scrolling,
                ScrollingSpeed = this.ScrollingSpeed,
                FilePath = this.FilePath,
                ReadFromFile = this.ReadFromFile,
                Direction = this.Direction
            };
            return model;
        }
    }
}
